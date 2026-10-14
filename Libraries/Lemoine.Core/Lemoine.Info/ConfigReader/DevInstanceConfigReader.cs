// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Config reader that is only active in a dev instance (see <see cref="InstanceInfo"/>),
  /// for example to read first the registry key with the -dev suffix
  ///
  /// It never returns the config key dev itself, so that it can be used to determine if the instance is a dev one
  ///
  /// Thread safe if the inner config reader is thread safe
  /// </summary>
  public sealed class DevInstanceConfigReader : IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DevInstanceConfigReader).FullName);

    readonly IGenericConfigReader m_configReader;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configReader">Config reader to use in a dev instance, not null</param>
    public DevInstanceConfigReader (IGenericConfigReader configReader)
    {
      if (configReader is null) {
        log.Fatal ($"DevInstanceConfigReader: null config reader");
        throw new ArgumentNullException (nameof (configReader));
      }
      m_configReader = configReader;
    }

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (string.Equals (key, InstanceInfo.DEV_KEY, StringComparison.InvariantCultureIgnoreCase)) {
        throw new ConfigKeyNotFoundException (key);
      }
      if (!InstanceInfo.IsDev) {
        throw new ConfigKeyNotFoundException (key);
      }
      return m_configReader.Get<T> (key);
    }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"DevInstanceConfigReader({m_configReader})";
  }
}
