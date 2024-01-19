// Copyright (C) 2023-2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Pulse.Graphql.InputType;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Response to the creation or update of a <see cref="ICncAcquisition"/>
  /// </summary>
  public class CncAcquisitionResponse
  {
    readonly ILog log = LogManager.GetLogger<CncAcquisitionResponse> ();

    /// <summary>
    /// Abort in case of load error
    /// </summary>
    static readonly string LOAD_ERROR_ABORT_KEY = "Graphql.CncAcquisition.LoadErrorAbort";
    static readonly bool LOAD_ERROR_ABORT_DEFAULT = true;

    /// <summary>
    /// Is it ok to consider the default value if the value is not set and not optional?
    /// </summary>
    static readonly string USE_DEFAULT_VALUE_IF_NOT_OPTIONAL_KEY = "Graphql.CncAcquisition.UseDefaultValueIfNotOptional";
    static readonly bool USE_DEFAULT_VALUE_IF_NOT_OPTIONAL_DEFAULT = true;

    /// <summary>
    /// Abort in case of missing parameter
    /// </summary>
    static readonly string MISSING_PARAMETER_ABORT_KEY = "Graphql.CncAcquisition.MissingParameterAbort";
    static readonly bool MISSING_PARAMETER_ABORT_DEFAULT = true;

    /// <summary>
    /// Abort in case of invalid parameter
    /// </summary>
    static readonly string INVALID_PARAMETER_ABORT_KEY = "Graphql.CncAcquisition.InvalidParameterAbort";
    static readonly bool INVALID_PARAMETER_ABORT_DEFAULT = true;

    /// <summary>
    /// Abort in case of additional parameter
    /// </summary>
    static readonly string ADDITIONAL_PARAMETER_ABORT_KEY = "Graphql.CncAcquisition.AdditionalParameterAbort";
    static readonly bool ADDITIONAL_PARAMETER_ABORT_DEFAULT = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncAcquisition">not null</param>
    public CncAcquisitionResponse (ICncAcquisition cncAcquisition)
    {
      this.CncAcquisition = cncAcquisition;
    }

    /// <summary>
    /// Associated cnc acquisition
    /// </summary>
    public ICncAcquisition CncAcquisition { get; }

    /// <summary>
    /// Cnc acquisition id
    /// </summary>
    public int? Id => this.CncAcquisition.Id;

    /// <summary>
    /// Cnc configuration file
    /// </summary>
    public string ConfigFile => this.CncAcquisition.ConfigFile;

    /// <summary>
    /// Return false if the update / creation was aborted
    /// </summary>
    public bool UpdateAborted { get; set; } = false;

    /// <summary>
    /// Return false if the configuration file could not be loaded
    /// </summary>
    public bool LoadError { get; private set; } = false;

    /// <summary>
    /// Missing parameters
    /// </summary>
    public IList<string>? MissingParameters { get; private set; } = null;

    /// <summary>
    /// Add a missing parameter
    /// </summary>
    /// <param name="parameterName"></param>
    public void AddMissingParameter (string parameterName)
    {
      if (this.MissingParameters is null) {
        this.MissingParameters = new List<string> ();
      }
      this.MissingParameters.Add (parameterName);
    }

    /// <summary>
    /// Invalid parameters
    /// </summary>
    public IList<string>? InvalidParameters { get; private set; } = null;

    /// <summary>
    /// Add an invalid parameter
    /// </summary>
    /// <param name="parameterName"></param>
    public void AddInvalidParameter (string parameterName)
    {
      if (this.InvalidParameters is null) {
        this.InvalidParameters = new List<string> ();
      }
      this.InvalidParameters.Add (parameterName);
    }

    /// <summary>
    /// Additional parameters
    /// </summary>
    public IList<string>? AdditionalParameters { get; private set; } = null;

    /// <summary>
    /// Add an additional parameter
    /// </summary>
    /// <param name="parameterName"></param>
    public void AddAdditionalParameter (string parameterName)
    {
      if (this.AdditionalParameters is null) {
        this.AdditionalParameters = new List<string> ();
      }
      this.AdditionalParameters.Add (parameterName);
    }

    /// <summary>
    /// Check the parameters
    /// </summary>
    /// <returns></returns>
    public bool CheckParameters (IDictionary<string, string> parameters)
    {
      try {
        var configFileName = this.ConfigFile;
        if (string.IsNullOrEmpty (configFileName)) {
          log.Error ($"CheckParameters: no configuration file was set");
          this.LoadError = true;
          return false;
        }
        var cncConfig = new CncConfig (configFileName);
        // Check the parameters: are there mandatory parameters that were not set?
        IList<CncConfigParam> cncConfigParams;
        try {
          cncConfigParams = cncConfig.Parameters;
        }
        catch (Exception ex) {
          log.Error ($"CheckParameters: exception while loading the parameters", ex);
          this.LoadError = true;
          return !Lemoine.Info.ConfigSet.LoadAndGet (LOAD_ERROR_ABORT_KEY, LOAD_ERROR_ABORT_DEFAULT);
        }
        var mandatoryParameters = cncConfigParams.Where (x => !x.Optional);
        if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEFAULT_VALUE_IF_NOT_OPTIONAL_KEY, USE_DEFAULT_VALUE_IF_NOT_OPTIONAL_DEFAULT)) {
          mandatoryParameters.Where (x => string.IsNullOrEmpty (x.Default));
        }
        foreach (var mandatoryParameter in mandatoryParameters) {
          if (!parameters.Select (x => x.Key).Contains (mandatoryParameter.Name)) {
            log.Error ($"CheckParameters: parameter {mandatoryParameter.Name} is not set");
            this.AddMissingParameter (mandatoryParameter.Name);
          }
        }
        foreach (var p in parameters) {
          var cncConfigParam = cncConfig.Parameters.FirstOrDefault (x => x.Name.Equals (p.Key, StringComparison.InvariantCultureIgnoreCase));
          if (cncConfigParam is null) {
            log.Info ($"CheckParameters: additional parameter {p.Key}");
            this.AddAdditionalParameter (p.Key);
          }
          else if (!cncConfigParam.IsValidValue (p.Value)) {
            log.Error ($"CheckParameters: invalid value {p.Value} for parameter {p.Key}");
            this.AddInvalidParameter (p.Key);
          }
        }
        if (this.MissingParameters?.Any () ?? false
          && Lemoine.Info.ConfigSet.LoadAndGet (MISSING_PARAMETER_ABORT_KEY, MISSING_PARAMETER_ABORT_DEFAULT)) {
          log.Error ($"CheckParameters: abort because of missing parameters");
          return false;
        }
        if (this.InvalidParameters?.Any () ?? false
          && Lemoine.Info.ConfigSet.LoadAndGet (INVALID_PARAMETER_ABORT_KEY, INVALID_PARAMETER_ABORT_DEFAULT)) {
          log.Error ($"CheckParameters: abort because of invalid parameters");
          return false;
        }
        if (this.AdditionalParameters?.Any () ?? false
          && Lemoine.Info.ConfigSet.LoadAndGet (ADDITIONAL_PARAMETER_ABORT_KEY, ADDITIONAL_PARAMETER_ABORT_DEFAULT)) {
          log.Error ($"CheckParameters: abort because of additional parameters");
          return false;
        }
        return true;
      }
      catch (Exception ex) {
        log.Error ("CheckParameters: exception", ex);
        return false;
      }
    }
  }

  /// <summary>
  /// Graphql type for <see cref="ICncAcquisition"/>
  /// </summary>
  public class CncAcquisitionGraphType : ObjectGraphType<CncAcquisitionResponse>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncAcquisitionGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAcquisitionGraphType ()
    {
      Name = "CncAcquisition";
      Field<IdGraphType> ("id");
      Field<NonNullGraphType<CncConfigGraphType>> ("cncConfig")
        .Resolve (ctx => new CncConfig (ctx.Source.ConfigFile));
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueGraphType>>>> ("parameters")
        .Resolve (ctx => ConvertConfigParameters (ctx.Source.CncAcquisition.ConfigParameters)
          .Union (ctx.Source.CncAcquisition.ConfigKeyParams.Select (x => new CncConfigParamValue (x.Key, x.Value.ToString ())))
          .ToList ());
      Field<BooleanGraphType> ("updateAborted")
        .Resolve (ctx => ctx.Source.UpdateAborted ? true : null);
      Field<BooleanGraphType> ("loadError")
        .Resolve (ctx => ctx.Source.LoadError ? true : null);
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("missingParameters");
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("invalidParameters");
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("additionalParameters");
    }

    IEnumerable<CncConfigParamValue> ConvertConfigParameters (string parameters)
    {
      var parray = EnumerableString.ParseListString (parameters);
      for (int i = 0; i < parray.Length; ++i) {
        yield return new CncConfigParamValue ($"Param{i + 1}", parray[i]);
      }
    }
  }
}
