// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to consider a TextBox is considered to set one or different strings
  /// 
  /// If Multiple is true, the separator is the first character of the text (StringList)
  /// </summary>
  public class StringAsTextPluginConf
    : TextPluginConf
    , IPluginConfDataType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (StringAsTextPluginConf).FullName);

    /// <summary>
    /// <see cref="IPluginConfDataType"/>
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public override Func<TConfiguration, object> GetConfigurationReader<TConfiguration> (System.Reflection.PropertyInfo property)
    {
      return c => GetStringPropertyToString (property, c);
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
      { SetStringPropertyFromString (property, c, v); };
    }

    object GetStringPropertyToString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration)
    {
      if (this.Multiple) {
        var o = GetProperty (propertyInfo, configuration);
        var configurationKeys = (IEnumerable<string>)o;
        return configurationKeys.ToListString<string> ();
      }
      else { // !this.Multiple
        var o = GetProperty (propertyInfo, configuration);
        return o.ToString ();
      }
    }

    void SetStringPropertyFromString<TConfiguration> (System.Reflection.PropertyInfo propertyInfo, TConfiguration configuration, object v)
    {
      var s = (string)v;

      if (this.Multiple) {
        var items = EnumerableString.ParseListString<string> (s);
        SetProperty (propertyInfo, configuration, items);
      }
      else { // !this.Multiple
        SetProperty (propertyInfo, configuration, s);
      }
    }
  }
}
