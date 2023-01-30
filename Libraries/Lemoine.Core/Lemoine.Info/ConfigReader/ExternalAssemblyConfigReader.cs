// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;
using Microsoft.Win32;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// GenericConfigReader using a IGenericConfigReader implementation in an external assembly
  /// </summary>
  public class ExternalAssemblyConfigReader : IGenericConfigReader
  {
    #region Members
    readonly string m_assemblyName;
    readonly string m_typeFullName;
    readonly IGenericConfigReader m_configReader;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ExternalAssemblyConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ExternalAssemblyConfigReader (string assemblyName, string typeFullName, object[] parameters = null)
    {
      m_assemblyName = assemblyName;
      m_typeFullName = typeFullName;

      try {
        var assembly = Assembly.Load (assemblyName);
        if (null == assembly) {
          log.Error ($"ExternalAssemblyConfigReader: assembly {assemblyName} could not be loaded, check it exists");
          m_configReader = null;
        }
        else {
          var type = assembly.GetType (typeFullName);
          if (null == type) {
            log.Fatal ($"ExternalAssemblyConfigReader: type {typeFullName} does not exist in {assemblyName}");
            m_configReader = null;
          }
          if (null == parameters) {
            m_configReader = (IGenericConfigReader)Activator.CreateInstance (type);
          }
          else {
            m_configReader = (IGenericConfigReader)Activator.CreateInstance (type, parameters);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ExternalAssemblyConfigReader: load error", ex);
        m_configReader = null;
      }
    }
    #endregion // Constructors

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (null == m_configReader) {
        log.Error ($"Get: {m_typeFullName} in {m_assemblyName} was not valid => return ConfigKeyNotFoundException");
        throw new ConfigKeyNotFoundException (key);
      }
      return m_configReader.Get<T> (key);
    }
    #endregion // IGenericConfigReader implementation
  }
}
