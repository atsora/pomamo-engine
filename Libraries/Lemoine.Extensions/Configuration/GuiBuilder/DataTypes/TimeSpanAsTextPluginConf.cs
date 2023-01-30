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
  /// Plugin conf to consider a TextBox is considered to set a time span
  /// </summary>
  public class TimeSpanAsTextPluginConf
    : TextPluginConf
    , IPluginConfDataType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TimeSpanAsTextPluginConf).FullName);

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetTimeSpanPropertyToString (property, c);
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
      { SetTimeSpanPropertyFromString (property, c, v); };
    }

    object GetTimeSpanPropertyToString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      if (this.Optional) {
        var o = GetProperty (propertyInfo, configuration);
        var d = (TimeSpan?)o;
        if (d.HasValue) {
          return "";
        }
        else {
          return d.Value.ToString ();
        }
      }
      else {
        var o = GetProperty (propertyInfo, configuration);
        var d = (TimeSpan)o;
        return d.ToString ();
      }
    }

    void SetTimeSpanPropertyFromString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      var s = (string)v;
      if (this.Optional) {
        if (string.IsNullOrEmpty (s)) {
          SetProperty (propertyInfo, configuration, (TimeSpan?)null);
        }
        else {
          TimeSpan t;
          try {
            t = TimeSpan.Parse (s);
          }
          catch (Exception) {
            log.ErrorFormat ("SetTimeSpanPropertyFromString: could not parse {0}", s);
            throw;
          }
          SetProperty (propertyInfo, configuration, (TimeSpan?)t);
        }
      }
      else {
        TimeSpan t;
        try {
          t = TimeSpan.Parse (s);
        }
        catch (Exception) {
          log.ErrorFormat ("SetTimeSpanPropertyFromString: could not parse {0}", s);
          throw;
        }
        SetProperty (propertyInfo, configuration, t);
      }
    }
  }
}
