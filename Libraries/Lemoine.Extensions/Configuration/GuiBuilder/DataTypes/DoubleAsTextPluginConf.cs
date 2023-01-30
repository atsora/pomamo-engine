// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a TextBox is considered to set a double
  /// </summary>
  public class DoubleAsTextPluginConf
    : TextPluginConf
    , IPluginConfDataType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DoubleAsTextPluginConf).FullName);

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetDoublePropertyToString (property, c);
    }

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Action<TConfiguration, object> GetConfigurationSetter<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return delegate (TConfiguration c, object v)
      { SetDoublePropertyFromString (property, c, v); };
    }

    object GetDoublePropertyToString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      if (this.Optional) {
        var o = GetProperty (propertyInfo, configuration);
        var d = (double?)o;
        if (d.HasValue) {
          return d.Value.ToString (CultureInfo.InvariantCulture);
        }
        else {
          return "";
        }
      }
      else {
        var o = GetProperty (propertyInfo, configuration);
        var d = (double)o;
        return d.ToString (CultureInfo.InvariantCulture);
      }
    }

    void SetDoublePropertyFromString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      var s = (string)v;

      if (this.Optional) {
        double? d;
        if (string.IsNullOrEmpty (s)) {
          d = null;
        }
        else {
          try {
            d = double.Parse (s, CultureInfo.InvariantCulture);
          }
          catch (Exception) {
            log.ErrorFormat ("SetDoublePropertyFromString: could not parse {0}", s);
            throw;
          }
        }
        SetProperty (propertyInfo, configuration, d);
      }
      else {
        double d;
        try {
          d = double.Parse (s, CultureInfo.InvariantCulture);
        }
        catch (Exception) {
          log.ErrorFormat ("SetDoublePropertyFromString: could not parse {0}", s);
          throw;
        }
        SetProperty (propertyInfo, configuration, d);
      }
    }
  }
}
