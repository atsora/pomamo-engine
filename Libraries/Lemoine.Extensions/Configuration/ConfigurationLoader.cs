// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Newtonsoft.Json;
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
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public virtual TConfiguration LoadConfiguration (string parameters)
    {
      var s = parameters;
      if (string.IsNullOrEmpty (s)) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("LoadConfiguration: null or empty parameters => replace it by {}");
        }
        s = "{}";
      }

      var result = JsonConvert.DeserializeObject<TConfiguration> (parameters, m_jsonSettings);
      if (null == result) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("LoadConfiguration: a null object was returned after deserializing {0}", s);
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
