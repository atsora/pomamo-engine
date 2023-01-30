// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// PulseVersions
  /// </summary>
  public static class PulseVersions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PulseVersions).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get the pulse versions of the different software
    /// </summary>
    public static IDictionary<string, string> Versions
    {
      get
      {
        var assemblyName = "Lemoine.Core.TargetSpecific";
        var typeFullName = "Lemoine.Info.TargetSpecific.PulseVersions";
        var propertyName = "Versions";
        try {
          var assembly = Assembly.Load (assemblyName);
          if (null == assembly) {
            log.Error ($"Versions.get: assembly {assemblyName} could not be loaded, check it exists");
            return new Dictionary<string, string> ();
          }

          var type = assembly.GetType (typeFullName);
          if (null == type) {
            log.Fatal ($"Versions.get: type {typeFullName} does not exist in {assemblyName}");
            Debug.Assert (false);
            return new Dictionary<string, string> ();
          }

          var propertyInfo = type.GetProperty (propertyName, BindingFlags.Public | BindingFlags.Static);
          if (null == propertyInfo) {
            log.Fatal ($"Versions.get: property {propertyName} of type {typeFullName} does not exist in {assemblyName}");
            Debug.Assert (false);
            return new Dictionary<string, string> ();
          }

          return (IDictionary<string, string>)propertyInfo.GetValue (null, null);
        }
        catch (Exception ex) {
          log.Error ($"Versions.get: load error", ex);
          return new Dictionary<string, string> ();
        }
      }
    }
    #endregion // Getters / Setters
  }
}
