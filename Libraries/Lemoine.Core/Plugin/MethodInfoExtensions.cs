// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Conversion;
using Lemoine.Core.Log;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// Extensions to <see cref="MethodInfo"/>
  /// </summary>
  public static class MethodInfoExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MethodInfoExtensions).FullName);

    /// <summary>
    /// Check if a methodInfo matches specified parameters
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="autoConverter"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static bool IsParameterMatch (this MethodInfo methodInfo, IAutoConverter autoConverter, params object[] parameters)
    {
      var parameterInfos = methodInfo.GetParameters ();
      var maxNumberParameters = parameterInfos.Length;
      var minNumberParameters = parameterInfos
        .Where (p => p.DefaultValue == DBNull.Value)
        .Count ();
      if (log.IsDebugEnabled) {
        log.Debug ($"IsParameterMatch: {methodInfo} has between {minNumberParameters} and {maxNumberParameters} parameters");
      }
      if (parameters.Length < minNumberParameters) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsParameterMatch: minNumber of parameters in {methodInfo} is {minNumberParameters} while the number of parameters is {parameters.Length}");
        }
        return false;
      }
      if (maxNumberParameters < parameters.Length) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsParameterMatch: maxNumber of parameters in {methodInfo} is {maxNumberParameters} while the number of parameters is {parameters.Length}");
        }
        return false;
      }

      for (int i = 0; i < parameters.Length; ++i) {
        var parameterType = parameterInfos[i].ParameterType;
        if (log.IsDebugEnabled) {
          log.Debug ($"IsParameterMatch: check parameter #{i} {parameters[i]} with type {parameterType}");
        }
        if (!autoConverter.IsCompatible (parameters[i], parameterType)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsParameterMatch: parameter #{i} of type {parameterType} is not compatible with {parameters[i]} => return false");
          }
          return false;
        } 
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"IsParameterMatch: {methodInfo} is ok");
      }
      return true;
    }

    /// <summary>
    /// Invoke extension with an auto-conversion of the parameters
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="autoConverter"></param>
    /// <param name="instance"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeAutoConvert (this MethodInfo methodInfo, IAutoConverter autoConverter, object instance, params object[] parameters)
    {
      try {
        var parameterInfos = methodInfo.GetParameters ();
        if (parameters.Length != parameterInfos.Length) {
          log.Error ($"InvokeAutoConvert: invalid number of input parameters {parameters.Length} VS {parameterInfos.Length} in method {methodInfo.Name}");
          throw new ArgumentException ("Invalid length", "parameters");
        }
        var convertedParameters = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; ++i) {
          var parameterType = parameterInfos[i].ParameterType;
          convertedParameters[i] = autoConverter.ConvertAuto (parameters[i], parameterType);
        }
        return methodInfo.Invoke (instance, convertedParameters);
      }
      catch (Exception ex) {
        log.Error ("InvokeAutoConvert: exception", ex);
        throw;
      }
    }
  }
}
