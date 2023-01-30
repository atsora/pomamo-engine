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
  /// Plugin conf to consider a TextBox is considered to set a int
  /// 
  /// If Multiple is true, the integers are separated by ','
  /// </summary>
  public class IntAsTextPluginConf
    : TextPluginConf
    , IPluginConfDataType
  {
    static readonly char[] SEPARATORS = new char[] { ',', ' ', ';' };
    static readonly string DEFAULT_SEPARATOR = ",";

    static readonly ILog log = LogManager.GetLogger (typeof (IntAsTextPluginConf).FullName);

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetIntPropertyToString (property, c);
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
      { SetIntPropertyFromString (property, c, v); };
    }

    object GetIntPropertyToString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      if (this.Multiple) {
        var o = GetProperty (propertyInfo, configuration);
        var configurationKeys = (IEnumerable<int>)o;
        return string.Join (DEFAULT_SEPARATOR, configurationKeys
          .Select (x => x.ToString ())
          .ToArray ());
      }
      else { // !this.Multiple
        if (this.Optional) {
          var o = GetProperty (propertyInfo, configuration);
          var d = (int?)o;
          if (d.HasValue) {
            return d.Value.ToString (CultureInfo.InvariantCulture);
          }
          else {
            return "";
          }
        }
        else {
          var o = GetProperty (propertyInfo, configuration);
          var d = (int)o;
          return d.ToString (CultureInfo.InvariantCulture);
        }
      }
    }

    void SetIntPropertyFromString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      var s = (string)v;

      if (this.Multiple) {
        var items = s.Split (SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
        var integers = items
          .Select (x => ParseErrorIsNull (x))
          .Where (x => x.HasValue)
          .Select (x => x.Value);
        SetProperty (propertyInfo, configuration, integers);
      }
      else { // !this.Multiple
        if (this.Optional) {
          int? d;
          if (string.IsNullOrEmpty (s)) {
            d = null;
          }
          else {
            try {
              d = int.Parse (s, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) {
              log.Error ($"SetIntPropertyFromString: could not parse {s}", ex);
              throw;
            }
          }
          SetProperty (propertyInfo, configuration, d);
        }
        else {
          int d;
          try {
            d = int.Parse (s, CultureInfo.InvariantCulture);
          }
          catch (Exception ex) {
            log.Error ($"SetIntPropertyFromString: could not parse {s}", ex);
            throw;
          }
          SetProperty (propertyInfo, configuration, d);
        }
      }
    }

    int? ParseErrorIsNull (string s)
    {
      int i;
      if (int.TryParse (s, out i)) {
        return i;
      }
      else {
        return null;
      }
    }
  }
}
