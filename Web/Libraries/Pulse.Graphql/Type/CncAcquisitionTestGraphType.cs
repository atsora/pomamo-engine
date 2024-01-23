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
  /// Response to the test of a cnc acquisition
  /// </summary>
  public class CncAcquisitionTestResponse : CncAcquisitionChangeResponse
  {
    readonly ILog log = LogManager.GetLogger<CncAcquisitionTestResponse> ();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncAcquisition">not null</param>
    public CncAcquisitionTestResponse (ICncAcquisition cncAcquisition)
      : base (cncAcquisition)
    {
    }

    /// <summary>
    /// Cnc data
    /// </summary>
    public string CncData { get; set; } = "";

    /// <summary>
    /// Exit code of the test console application
    /// </summary>
    public int? ExitCode { get; set; } = null;
  }

  /// <summary>
  /// Graphql response type to TestCncAcquisition
  /// </summary>
  public class CncAcquisitionTestGraphType : ObjectGraphType<CncAcquisitionTestResponse>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncAcquisitionTestGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAcquisitionTestGraphType ()
    {
      Name = "CncAcquisitionTest";
      Field<NonNullGraphType<CncConfigGraphType>> ("cncConfig");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueGraphType>>>> ("parameters")
        .Resolve (ctx => ConvertConfigParameters (ctx.Source.CncAcquisition.ConfigParameters)
          .Union (ctx.Source.CncAcquisition.ConfigKeyParams.Select (x => new CncConfigParamValue (x.Key, x.Value.ToString ())))
          .ToList ());
      Field<BooleanGraphType> ("loadError")
        .Resolve (ctx => ctx.Source.LoadError ? true : null);
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("missingParameters");
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("invalidParameters");
      Field<ListGraphType<NonNullGraphType<StringGraphType>>> ("additionalParameters");
      Field<string> ("cncData", nullable: false);
      Field<int?> ("exitCode", nullable: true);
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
