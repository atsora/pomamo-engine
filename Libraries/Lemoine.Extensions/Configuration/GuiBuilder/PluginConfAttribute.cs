// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder.DataTypes;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Collections;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Custom attribute for AutoConfigGuiBuilder
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public class PluginConfAttribute: Attribute
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginConfAttribute).FullName);

    static readonly string ADDITIONAL_EXTENSIONS_ASSEMBLIES_KEY = "Extensions.AdditionalAssemblies"; // ListString

    IPluginConfDataType ConvertDataType (string dataType)
    {
      try {
        var type = GetType (dataType);
        if (!typeof (IPluginConfDataType).IsAssignableFrom (type)) {
          log.Error ($"ConvertDataType: dataType {dataType}, full {type?.FullName} is not a IPluginConfDataType");
          throw new ArgumentException ("'" + dataType + "' is not a valid IPluginConfDataType type", "dataType");
        }
        return (IPluginConfDataType)Activator.CreateInstance (type);
      }
      catch (Exception ex) {
        log.Error ("ConvertDataType: exception", ex);
        throw;
      }
    }

    Type GetType (string dataType)
    {
      if (dataType.Contains (".")) {
        return Type.GetType (dataType);
      }
      else {
        // Try first in Lemoine.Extensions
        var type = Type.GetType ("Lemoine.Extensions.Configuration.GuiBuilder.DataTypes." + dataType + "PluginConf, Lemoine.Extensions");
        if (null != type) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetType: {type.FullName} is a valid type");
          }
          return type;
        }
        else {
          var additionalExtensionsAssembliesString = Lemoine.Info.ConfigSet
            .LoadAndGet (ADDITIONAL_EXTENSIONS_ASSEMBLIES_KEY, "");
          if (!string.IsNullOrEmpty (additionalExtensionsAssembliesString)) {
            var additionalExtensionsAssemblies = EnumerableString.ParseListString (additionalExtensionsAssembliesString);
            foreach (var additionalExtensionsAssembly in additionalExtensionsAssemblies) {
              var typeFullName = "Lemoine.Extensions.Configuration.GuiBuilder.DataTypes." + dataType + "PluginConf, " + additionalExtensionsAssembly;
              if (log.IsDebugEnabled) {
                log.Debug ($"GetType: try {typeFullName}");
              }
              type = Type.GetType (typeFullName);
              if (null != type) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetType: {type.FullName} is a valid type");
                }
                return type;
              }
            }
          }
        }
      }
      log.Error ($"GetType: no valid type was found for {dataType} => return null");
      return null;
    }

    /// <summary>
    /// Default constructor (no label is set)
    /// </summary>
    /// <param name="dataType"></param>
    public PluginConfAttribute (string dataType)
      : this (dataType, "")
    {
    }

    /// <summary>
    /// Constructor with a label
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="label"></param>
    public PluginConfAttribute (string dataType, string label)
    {
      this.DataType = ConvertDataType (dataType);
      this.Label = label;
      this.Parameters = "";
      this.Optional = false;
      this.Multiple = false;
    }

    /// <summary>
    /// Label text of the control
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Parameters of the control (not available on all components)
    /// </summary>
    public string Parameters {
      get { return this.DataType.Parameters; }
      set { this.DataType.Parameters = value; }
    }

    /// <summary>
    /// Optional field (not available on all components)
    /// </summary>
    public bool Optional {
      get { return this.DataType.Optional; }
      set { this.DataType.Optional = value; }
    }

    /// <summary>
    /// Multiple values are allowed (not available on all components)
    /// </summary>
    public bool Multiple {
      get { return this.DataType.Multiple; }
      set { this.DataType.Multiple = value; }
    }

    /// <summary>
    /// Data type
    /// </summary>
    public IPluginConfDataType DataType { get; set; }
  }
}
