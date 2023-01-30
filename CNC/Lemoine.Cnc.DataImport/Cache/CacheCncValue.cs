// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Cnc.DataImport.Cache
{
  /// <summary>
  /// Description of CacheCncValue.
  /// </summary>
  internal sealed class CacheCncValue
  {
    static readonly string MAX_CNC_VALUE_GAP_KEY = "Cnc.DataImport.MaxCncValueGap";
    static readonly TimeSpan MAX_CNC_VALUE_GAP_DEFAULT = TimeSpan.FromMinutes(1);
    
    #region Members
    // FieldId > CncValue
    readonly IDictionary<int, ICncValue> m_cncValues = new Dictionary<int, ICncValue>();
    readonly IDictionary<int, ICncValue> m_previousValues = new Dictionary<int, ICncValue>();
    readonly IDictionary<int, ICncValue> m_currentValues = new Dictionary<int, ICncValue>();
    readonly IMachineModule m_machineModule;
    readonly IEnumerable<Lemoine.Extensions.Cnc.IEndCncValueExtension> m_endCncValueExtensions;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CacheCncValue).FullName);

    #region Members
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public CacheCncValue(IMachineModule machineModule)
    {
      m_machineModule = machineModule;

      // Extension initialization
      m_endCncValueExtensions = ExtensionManager
        .GetExtensions<Lemoine.Extensions.Cnc.IEndCncValueExtension> ()
        .Where (extension => extension.Initialize (m_machineModule))
        .ToList ();
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Set a cnc value, not stored in the database yet
    /// </summary>
    /// <param name="field"></param>
    /// <param name="cncValue"></param>
    public void SetCncValue(IField field, ICncValue cncValue)
    {
      Debug.Assert(field != null);
      m_cncValues[field.Id] = cncValue;
    }
    
    /// <summary>
    /// Get a cnc value from the cache
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="dateTime">Date/time of the new data to import</param>
    /// <returns></returns>
    public ICncValue GetStoredCncValue(IField field, DateTime dateTime)
    {
      Debug.Assert (null != field);

      ICncValue cncValue = null;
      if (m_cncValues.ContainsKey(field.Id)) {
        cncValue = m_cncValues[field.Id];
        Debug.Assert(null != cncValue);
        Debug.Assert(cncValue.MachineModule.Id == m_machineModule.Id);
        Debug.Assert(cncValue.Field.Id == field.Id);

        if (IsEndCncValueRequested (field, cncValue, dateTime)) {
          ResetCncValue (field.Id);
          cncValue = null;
        }
      }
      
      log.DebugFormat ("GetStoredCncValue: " +
                       "return {0} for machineModule={1} field={2}",
                       cncValue, m_machineModule, field);
      
      return cncValue;
    }
    
    /// <summary>
    /// Is a reset of the cnc value required ?
    /// 
    /// By default, it is required if it has the Stopped property
    /// or there is a gap
    /// 
    /// But it may be overwritten by an extension
    /// </summary>
    /// <param name="field"></param>
    /// <param name="cncValue"></param>
    /// <param name="dateTime">Date/time of the new data to import</param>
    /// <returns></returns>
    public bool IsEndCncValueRequested (IField field, ICncValue cncValue, DateTime dateTime)
    {
      return IsEndCncValueRequired (field.Code, cncValue.End, dateTime, cncValue.Stopped);
    }
    
    /// <summary>
    /// Is the end of the cnc value requested ?
    /// 
    /// By default, it is required if it has the Stopped property
    /// or there is a gap
    /// 
    /// But it may be overwritten by an extension
    /// </summary>
    /// <param name="fieldCode"></param>
    /// <param name="previousDateTime"></param>
    /// <param name="newDateTime"></param>
    /// <param name="previousStopped"></param>
    /// <returns></returns>
    public bool IsEndCncValueRequired (string fieldCode,
                                       DateTime previousDateTime, DateTime newDateTime,
                                       bool previousStopped)
    {
      foreach (var extension in m_endCncValueExtensions) {
        bool? result = extension.IsEndCncValueRequested (fieldCode,
                                                         previousDateTime,
                                                         newDateTime,
                                                         previousStopped);
        if (result.HasValue) {
          log.DebugFormat ("IsResetCncValueRequired: " +
                           "extension {0} returned {1}",
                           extension, result.Value);
          return result.Value;
        }
      }

      if (previousStopped) {
        log.DebugFormat ("IsResetCncValueRequired: " +
                         "previous stopped is true");
        return true;
      }
      
      TimeSpan maxCncValueGap = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (MAX_CNC_VALUE_GAP_KEY,
                               MAX_CNC_VALUE_GAP_DEFAULT);
      if (newDateTime < previousDateTime) {
        log.FatalFormat ("IsResetCncValueRequired: " +
                         "newDateTime {0} comes before previousDateTime {1}",
                         newDateTime, previousDateTime);
      }
      else if (maxCncValueGap < newDateTime.Subtract (previousDateTime)) { // Check the gap
        log.DebugFormat ("IsResetCncValueRequired: " +
                         "there is a gap {0}-{1}, " +
                         "discontinue the CncValue for field {2}",
                         previousDateTime, newDateTime,
                         fieldCode);
        return true;
      }

      return false;
    }
    
    /// <summary>
    /// Get the already stored Cnc Value for the specified machine module and field,
    /// and re-attach it for the current session.
    /// 
    /// This method must be run in a NHibernate session.
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="dateTime">Date/time of the new data to import</param>
    /// <returns></returns>
    public ICncValue GetStoredReattachedCncValue(IField field, DateTime dateTime)
    {
      Debug.Assert (null != field);

      ICncValue cncValue = GetStoredCncValue(field, dateTime);
      
      // Re-attach the cncValue
      if (cncValue != null && cncValue.Id != 0) {
        ModelDAOHelper.DAOFactory.CncValueDAO.UpgradeLock(cncValue);
      }

      return cncValue;
    }
    
    /// <summary>
    /// Reset a cnc value in the cache
    /// </summary>
    /// <param name="fieldId"></param>
    internal void ResetCncValue(int fieldId)
    {
      log.DebugFormat ("ResetCncValue: " +
                       "MachineModuleId={0} fieldId={1}",
                       m_machineModule.Id, fieldId);
      m_cncValues.Remove(fieldId);
      m_previousValues.Remove(fieldId);
      m_currentValues.Remove(fieldId);
    }
    
    /// <summary>
    /// Reload a cnc value from the database
    /// </summary>
    /// <param name="fieldId"></param>
    public void ReloadCncValue(int fieldId)
    {
      if (m_cncValues.ContainsKey(fieldId)) {
        ICncValue cncValue = m_cncValues[fieldId];
        Debug.Assert (null != cncValue);
        if (0 != cncValue.Id) {
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
            {
              cncValue = ModelDAOHelper.DAOFactory.CncValueDAO.FindById (cncValue.Id, cncValue.MachineModule);
            }
            log.DebugFormat ("ReloadCncValue: " +
                             "reloaded value is {0}",
                             cncValue);
            if (null != cncValue) {
              m_cncValues[fieldId] = cncValue;
            }
            else {
              ResetCncValue(fieldId);
            }
          }
          catch (Exception ex) {
            log.ErrorFormat ("ReloadCncValue: " +
                             "reloading cncvalue {0} failed with {1} " +
                             "=> reset it",
                             cncValue, ex);
            ResetCncValue(fieldId);
          }
          // Reset some previous and average values, because we are not sure they are still accurate
          // Some information is lost, there is probably a better process to make, but much more complex
          if (m_previousValues.ContainsKey (fieldId)) {
            log.WarnFormat ("ReloadCncValue: " +
                            "reset m_previousAverageValues for machineModuleId {0} fieldId {1}",
                            m_machineModule.Id, fieldId);
            m_previousValues.Remove(fieldId);
          }
          if (m_currentValues.ContainsKey (fieldId)) {
            log.WarnFormat ("ReloadCncValue: " +
                            "reset m_currentAverageValues for machineModuleId {0} fieldId {1}",
                            m_machineModule.Id, fieldId);
            m_currentValues.Remove (fieldId);
          }
        }
      }
    }
    
    /// <summary>
    /// Save or update a CNC value
    /// This method must be run inside a transaction
    /// </summary>
    /// <param name="cncValue"></param>
    public void SaveOrUpdateCncValue(ICncValue cncValue)
    {
      // Note: a NewValue CncValue may have a null length
      
      if (0 == cncValue.Id) {
        // For a new cncValue only,
        // remove the eventual previous cnc value with the same date
        // because a unique condition is set
        ICncValue previousValue = ModelDAOHelper.DAOFactory.CncValueDAO
          .FindByMachineModuleFieldBegin (cncValue.MachineModule, cncValue.Field, cncValue.Begin);
        if (previousValue != null) {
          if (TimeSpan.FromSeconds (1) <= previousValue.End.Subtract(previousValue.Begin)) {
            // CncValue with a not-null length, may be a bug somewhere, this should not happen
            log.ErrorFormat ("SaveOrUpdateCncValue: " +
                             "a previous value {0} was detected with a not null length " +
                             "=> this should not happen normally",
                             previousValue);
          }
          Debug.Assert (previousValue.End.Subtract (previousValue.Begin) < TimeSpan.FromSeconds (1));
          log.InfoFormat ("SaveOrUpdateCncValue: " +
                          "about to remove previous Cnc Value {0} with a null length",
                          previousValue);
          ModelDAOHelper.DAOFactory.CncValueDAO.MakeTransient (previousValue);
        }
      }
      
      // - save or update the new value
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent(cncValue);
    }
    
    /// <summary>
    /// Get a current value (average or max)
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public ICncValue GetCurrentValue(IField field)
    {
      return m_currentValues.ContainsKey(field.Id) ?
        m_currentValues[field.Id] : null;
    }
    
    /// <summary>
    /// Get a previous average value (average or max)
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public ICncValue GetPreviousValue(IField field)
    {
      return m_previousValues.ContainsKey(field.Id) ?
        m_previousValues[field.Id] : null;
    }
    
    /// <summary>
    /// Set a current average value (average or max)
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void SetCurrentValue(IField field, ICncValue value)
    {
      if (value == null) {
        // Reset
        m_currentValues.Remove(field.Id);
      } else {
        // Set
        m_currentValues[field.Id] = value;
      }
    }
    
    /// <summary>
    /// Set a previous value (average or max)
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void SetPreviousValue(IField field, ICncValue value)
    {
      if (value == null) {
        // Reset
        m_previousValues.Remove(field.Id);
      } else {
        // Set
        m_previousValues[field.Id] = value;
      }
    }
    #endregion // Methods
  }
}
