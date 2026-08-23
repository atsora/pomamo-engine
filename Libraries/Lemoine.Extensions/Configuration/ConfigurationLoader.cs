// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Configuration
{
  /// <summary>
  /// Configuration loader
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public class ConfigurationLoader<TConfiguration>
    : IConfigurationLoader<TConfiguration>
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigurationLoader<TConfiguration>).FullName);

    readonly JsonSerializerSettings m_jsonSettings = new JsonSerializerSettings
    {
      DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
      NullValueHandling = NullValueHandling.Ignore,
      Converters = new List<JsonConverter>
      {
        new Lemoine.Conversion.JavaScript.TimeSpanConverter ()
      }
    };

    /// <summary>
    /// Load the parameters to build a Configuration object
    ///
    /// Only the Json format is supported. The legacy XML format was removed:
    /// an <see cref="ArgumentException"/> is raised if it is detected.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">the parameters use the removed XML format</exception>
    public virtual TConfiguration LoadConfiguration (string parameters)
    {
      var s = parameters;
      if (string.IsNullOrEmpty (s)) {
        if (log.IsWarnEnabled) {
          log.Warn ("LoadConfiguration: null or empty parameters => replace it by {}");
        }
        s = "{}";
      }

      if (s.TrimStart ().StartsWith ("<", StringComparison.InvariantCulture)) {
        log.Fatal ($"LoadConfiguration: the plugin configuration uses the XML format, which is not supported any more. Rewrite this configuration in Json format, else this plugin instance is skipped. Configuration={s}");
        throw new ArgumentException ("The XML plugin configuration format is not supported any more: rewrite the configuration in Json format", nameof (parameters));
      }

      // Note: s is used here and not parameters, so that empty parameters are processed like {}
      var result = JsonConvert.DeserializeObject<TConfiguration> (s, m_jsonSettings);
      if (null == result) {
        if (log.IsWarnEnabled) {
          log.Warn ($"LoadConfiguration: a null object was returned after deserializing {s}");
        }
      }
      return result;
    }

    /// <summary>
    /// Get the properties to save from the configuration
    /// </summary>
    /// <returns></returns>
    public virtual string GetProperties (TConfiguration configuration)
    {
      string s = JsonConvert.SerializeObject (configuration, m_jsonSettings);
      log.DebugFormat ("GetConfiguration: " +
                       "configuration is {0}",
                       s);
      return s;
    }
  }
}
