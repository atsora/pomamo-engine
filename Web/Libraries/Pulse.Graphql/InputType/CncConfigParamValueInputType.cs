// Copyright (c) 2023 Atsora Solutions

using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Cnc config parameter value
  /// </summary>
  public class CncConfigParamValueInput
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncConfigParamValueInput));

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public CncConfigParamValueInput (string name, string value)
    {
      this.Name = name;
      this.Value = value;
    }

    public static string GetParametersString (IList<CncConfigParamValueInput> parameters)
    {
      var a = new string[10];
      int maxParamNumber = 0;
      foreach (var paramValue in parameters) {
        if (paramValue.Name.StartsWith ("Param", StringComparison.CurrentCultureIgnoreCase)) {
          if (int.TryParse (paramValue.Name.Substring ("Param".Length), out var paramNumber)) {
            if (a.Length < paramNumber) {
              log.Error ($"GetParametersString: param number {paramNumber} for name {paramValue.Name} is not supported yet");
            }
            else {
              a[paramNumber - 1] = paramValue.Value;
              if (maxParamNumber < paramNumber) {
                maxParamNumber = paramNumber;
              }
            }
          }
          else {
            log.Error ($"GetParametersString: parameter name {paramValue.Name} does not contain the param number");
          }
        }
        else {
          log.Error ($"GetParametersString: skip the parameter of name {paramValue.Name}");
        }
      }
      if (0 < maxParamNumber) {
        return a.Take (maxParamNumber).ToListString ();
      }
      else {
        log.Warn ($"GetParametersString: no parameter found => return an empty string");
        return "";
      }
    }
  }

  /// <summary>
  /// Graphql type for <see cref=""/>
  /// </summary>
  public class CncConfigParamValueInputType : InputObjectGraphType<CncConfigParamValueInput>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigParamValueInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigParamValueInputType ()
    {
      Name = "CncConfigParamValueInput";
      Field<NonNullGraphType<StringGraphType>> ("name");
      Field<string> ("value", nullable: true);
    }
  }
}
