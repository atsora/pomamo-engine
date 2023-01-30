// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Lemoine.Cnc;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Method to execute the method in a thread safe way
  /// </summary>
  public sealed class CncModuleExecutor : IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncModuleExecutor).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (CncModuleExecutor).FullName);

    readonly ICncModule m_cncModule;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1);

    #region Getters / Setters
    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    public int CncAcquisitionId
    {
      get => m_cncModule.CncAcquisitionId;
    }

    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    public string CncAcquisitionName
    {
      get => m_cncModule.CncAcquisitionName;
    }

    /// <summary>
    /// Associated semaphore
    /// </summary>
    public SemaphoreSlim Semaphore => m_semaphore;

    /// <summary>
    /// Associated Cnc Module
    /// </summary>
    public ICncModule CncModule => m_cncModule;

    /// <summary>
    /// In case this is true, skip the instructions after the start (get/set/finish)
    /// </summary>
    public bool SkipInstructions { get; set; } = false;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncModule">not null</param>
    public CncModuleExecutor (ICncModule cncModule)
    {
      Debug.Assert (null != cncModule);

      m_cncModule = cncModule;
    }

    #endregion // Constructors

    /// <summary>
    /// <see cref="IDisposable"/>
    /// 
    /// Warning: this class also disposes the associated cnc module
    /// </summary>
    public void Dispose ()
    {
      if (null != m_cncModule) {
        m_cncModule.Dispose ();
      }
      m_semaphore.Dispose ();
    }

    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    public void PauseCheck ()
    {
      m_cncModule.PauseCheck ();
    }

    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    public void ResumeCheck ()
    {
      m_cncModule.ResumeCheck ();
    }

    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    public void SetActive ()
    {
      m_cncModule.SetActive ();
    }

    /// <summary>
    /// <see cref="ICncModule"/>
    /// </summary>
    /// <param name="dataHandler"></param>
    public void SetDataHandler (IChecked dataHandler)
    {
      m_cncModule.SetDataHandler (dataHandler);
    }

    /// <summary>
    /// Start the execution of the module
    /// </summary>
    /// <param name="moduleElement">the whole XML element</param>
    /// <param name="cncData">Cnc data</param>
    /// <returns>Success</returns>
    public bool Start (XmlElement moduleElement, IDictionary<string, object> cncData)
    {
      try {
        var moduleType = m_cncModule.GetType ();
        MethodInfo methodInfo = moduleType
          .GetMethod ("Start",
                      BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
        if (null == methodInfo) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Start: method Start does not exist => return true (this is optional)");
          }
          return true;
        }
        ParameterInfo[] methodParameters =
          methodInfo.GetParameters ();
        var numberParameters = methodParameters.Length;
        if (0 == numberParameters) {
          return Start ();
        }
        else if (2 == numberParameters) {
          var parameter1Type = methodParameters[0].ParameterType;
          var parameter2Type = methodParameters[1].ParameterType;
          if (typeof (XmlElement).Equals (parameter1Type)) {
            object result = moduleType
              .InvokeMember ("Start",
                             BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                             null,
                             m_cncModule,
                             new object[] { moduleElement, cncData });
            if (log.IsDebugEnabled) {
              log.Debug ($"Start: Start method of {m_cncModule} is completed");
            }
            if (result != null && result.Equals (false)) {
              // Only a warning because there is a case where a start method returns false in normal conditions:
              // when GDBcurrent is called every 2s while it's configured to do something every 4s
              // if there is an error, this is the job to the start method to log a more accurate message
              log.Warn ($"Start: Start method of {m_cncModule} failed");
              return false;
            }
            else {
              // Let's consider everything was managed in the Start method
              this.SkipInstructions = true;
              return true;
            }
          }
          else {
            log.Error ($"Start: invalid first parameter type {parameter1Type} in method Start");
            throw new Exception ("Invalid first parameter type in method Start");
          }
        }
        else { // numberParameters not 0 or 2
          log.Error ($"Start: invalid number of parameters {numberParameters} in method Start");
          throw new Exception ("Invalid number of parameters in method Start");
        }
      }
      catch (MissingMethodException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Start: Start method does not exist in {m_cncModule}", ex);
        }
        return true;
      }
      catch (Exception ex) {
        log.Error ($"Start: Start method of {m_cncModule} failed", ex);
        throw;
      }
    }

    /// <summary>
    /// Start the execution of the module
    /// </summary>
    /// <returns></returns>
    public bool Start ()
    {
      try {
        Type type = m_cncModule.GetType ();
        object result = type.InvokeMember ("Start",
                                           BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                           null,
                                           m_cncModule,
                                           new object[] { });
        if (log.IsDebugEnabled) {
          log.Debug ($"Start: Start method of {m_cncModule} is completed");
        }
        if (result != null && result.Equals (false)) {
          // Only a warning because there is a case where a start method returns false in normal conditions:
          // when GDBcurrent is called every 2s while it's configured to do something every 4s
          // if there is an error, this is the job to the start method to log a more accurate message
          log.Warn ($"Start: Start method of {m_cncModule} failed");
          return false;
        }
        else {
          return true;
        }
      }
      catch (MissingMethodException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Start: Start method does not exist in {m_cncModule}", ex);
        }
        return true;
      }
      catch (Exception ex) {
        log.Error ($"Start: Start method of {m_cncModule} failed", ex);
        throw;
      }
    }

    /// <summary>
    /// Finish the execution of a module
    /// </summary>
    public void Finish ()
    {
      try { // Run the Finish method if it exists
        var type = m_cncModule.GetType ();
        type.InvokeMember ("Finish",
                           BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                           null,
                           m_cncModule,
                           new object[] { });
        if (log.IsDebugEnabled) {
          log.Debug ($"Finish: Finish method of {m_cncModule} is completed");
        }
      }
      catch (MissingMethodException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Finish: Finish method does not exist in {m_cncModule}", ex);
        }
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Finish: Finish method of {m_cncModule} failed", ex);
        }
        throw;
      }
    }

    bool TryMethod (out object result, string method, params object[] parameters)
    {
      Debug.Assert (!string.IsNullOrEmpty (method));

      try {
        var type = m_cncModule.GetType ();
        result = type.InvokeMember (method,
                                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                    null,
                                    m_cncModule,
                                    parameters);
        if (log.IsDebugEnabled) {
          log.Debug ($"TryMethod: method {method} of {m_cncModule} returned {result}");
        }
        return true;
      }
      catch (MissingMethodException) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryMethod: no method {method} in module {m_cncModule}");
        }
        result = null;
        return false;
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ($"TryMethod: Method {method} of {m_cncModule} failed with param {parameters}", ex);
        }
        result = null;
        throw;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="method"></param>
    /// <param name="param"></param>
    /// <returns>false if the method does not exist</returns>
    /// <exception cref="Exception">Exception of the method</exception>
    public bool TryGet (out object result, string method, string param)
    {
      return TryMethod (out result, method, param);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="property"></param>
    /// <returns>false if the property does not exist</returns>
    /// <exception cref="Exception">Exception when trying to read the property</exception>
    public bool TryGetProperty (out object result, string property)
    {
      Debug.Assert (!string.IsNullOrEmpty (property));

      try {
        var type = m_cncModule.GetType ();
        var propertyInfo = type.GetProperty (property);
        if (null == propertyInfo) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryGetProperty: property {property} does not exist in module {m_cncModule}");
          }
          result = null;
          return false;
        }
        result = propertyInfo.GetValue (m_cncModule, null);
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetProperty: the property {property} of {m_cncModule} is {result}");
        }
        return true;
      }
      catch (Exception ex) {
        log.Warn ($"TryGetProperty: using the property {property} of {m_cncModule} failed", ex);
        result = null;
        throw;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="datavalue"></param>
    /// <param name="GetParam"></param>
    /// <returns>false if the method does not exist</returns>
    /// <exception cref="Exception">Exception when trying to read the property</exception>
    public bool TrySet (string method, object datavalue, Func<string> GetParam)
    {
      Debug.Assert (!string.IsNullOrEmpty ("method"));

      try {
        var moduleType = m_cncModule.GetType ();
        MethodInfo methodInfo = moduleType
          .GetMethod (method,
                      BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
        if (null == methodInfo) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TrySet: method {method} does not exist");
          }
          return false;
        }
        ParameterInfo[] methodParameters =
          methodInfo.GetParameters ();
        if (methodParameters.Length < 1) {
          log.Error ($"TrySet: set method {method} does not have any parameter");
          throw new Exception ("No parameter for set method");
        }
        object convertedValue = datavalue;
        Type type = methodParameters[methodParameters.Length - 1].ParameterType;
        try {
          convertedValue = ConvertData (datavalue, type);
        }
        catch (Exception ex) {
          if (log.IsWarnEnabled) {
            log.Warn ($"TrySet: conversion of {datavalue} to {type} failed", ex);
          }
        }
        if (methodParameters.Length <= 1) { // Only one parameter
          moduleType.InvokeMember (method,
                                   BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                   null,
                                   m_cncModule,
                                   new object[] { convertedValue });
        }
        else { // More than one parameter
          string key = GetParam ();
          moduleType.InvokeMember (method,
                                   BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                   null,
                                   m_cncModule,
                                   new object[] { key, convertedValue });
        }
      }
      catch (Exception ex) {
        log.Error ($"TrySet: using the method {method} of {m_cncModule} failed", ex);
        throw;
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="datavalue"></param>
    /// <returns>false if the property does not exist</returns>
    /// <exception cref="Exception">Exception when trying to read the property</exception>
    public bool TrySetProperty (string property, object datavalue)
    {
      try {
        var moduleType = m_cncModule.GetType ();
        var propertyInfo = moduleType.GetProperty (property);
        if (null == propertyInfo) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TrySetProperty: no property {property} in module {m_cncModule}");
          }
          return false;
        }

        var propertyType = propertyInfo.PropertyType;
        object convertedValue = datavalue;
        try {
          convertedValue = ConvertData (datavalue, propertyType);
        }
        catch (Exception ex) {
          if (log.IsWarnEnabled) {
            log.Warn ($"TrySetProperty: conversion of {datavalue} to {propertyType} failed", ex);
          }
        }
        propertyInfo.SetValue (m_cncModule,
                               convertedValue,
                               null);
      }
      catch (Exception ex) {
        log.Error ($"TrySetProperty: using the property {property} of {m_cncModule} failed", ex);
        throw;
      }

      return true;
    }

    /// <summary>
    /// Convert a data to another type
    /// </summary>
    /// <param name="datavalue"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    static public object ConvertData (object datavalue, Type type)
    {
      var converter = new Lemoine.Conversion.DefaultAutoConverter ();
      return converter.ConvertAuto (datavalue, type);
    }
  }
}
