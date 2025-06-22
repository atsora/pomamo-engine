// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// Track the changes in observation state slots
  /// </summary>
  public class SlotExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , Pulse.Extensions.Database.ISlotExtension
  {
    IEnumerable<IMachineModule> m_machineModules = null; // Warning: it may remain null (if not a monitored machine or plugin not active) !
    IEnumerable<IField> m_fields = null; // Warning: it may remain null (if the plugin is not active) or it can be empty (plugin active with no configured field)
    bool m_initialized = false;

    static readonly ILog log = LogManager.GetLogger(typeof (SlotExtension).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SlotExtension ()
      : base (new ConfigurationLoader ())
    {
    }

    #region ISlotExtension implementation
    public void InsertNewSlotsBegin(IPeriodAssociation association, UtcDateTimeRange range, System.Collections.Generic.IEnumerable<ISlot> existingSlots, System.Collections.Generic.IEnumerable<ISlot> newSlots)
    {
    }
    
    public void InsertNewSlotsEnd()
    {
    }
    
    public void AddSlot(Lemoine.Model.ISlot slot)
    {
      Debug.Assert (null != slot);
      
      if (slot is IObservationStateSlot) {
        AddModify (slot as IObservationStateSlot);
      }
    }

    public void RemoveSlot(Lemoine.Model.ISlot slot)
    {
      Debug.Assert (null != slot);
      
      if (slot is IObservationStateSlot) {
        Remove (slot as IObservationStateSlot);
      }
    }

    public void ModifySlot(Lemoine.Model.ISlot oldSlot, Lemoine.Model.ISlot newSlot)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (null != newSlot);
      
      if (newSlot is IObservationStateSlot) {
        Debug.Assert (null != oldSlot);
        if (!object.Equals (oldSlot.BeginDateTime, newSlot.BeginDateTime)) { // Change of begin date/time
          Remove (oldSlot as IObservationStateSlot);
          AddModify (newSlot as IObservationStateSlot);
        }
        else if (!object.Equals (oldSlot.EndDateTime, newSlot.EndDateTime)) { // Change of end date/time
          AddModify (newSlot as IObservationStateSlot);
        }
      }
    }

    #endregion // ISlotExtension
    
    void Initialize (IMachine machine)
    {
      if (m_initialized) {
        return;
      }
      
      var fieldIds = new HashSet<int>();

      var configurations = LoadConfigurations ();
      foreach (var configuration in configurations) {
        foreach (var cncFieldId in configuration.CncFieldIds) {
          fieldIds.Add (cncFieldId);
        }
      }
      var fields = new List<IField> ();
      foreach (var fieldId in fieldIds) {
        IField field = ModelDAOHelper.DAOFactory.FieldDAO
          .FindById (fieldId);
        if (null == field) {
          log.Warn ($"Initialize: no field with id {fieldId}");
        }
        else {
          fields.Add (field);
        }
      }
      m_fields = fields;
      
      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindByIdWithMachineModules (machine.Id);
      if (null == monitoredMachine) {
        log.Warn ($"Initialize: no monitored machine with id {machine.Id}");
        m_initialized = true;
        Debug.Assert (null != m_fields);
        return;
      }
      m_machineModules = monitoredMachine.MachineModules;
      
      Debug.Assert (null != m_machineModules);
      Debug.Assert (null != m_fields);
      m_initialized = true;
    }
    
    void AddModify (IObservationStateSlot slot)
    {
      Debug.Assert (null != slot);
      if (null == slot) {
        log.Fatal ("AddModify: slot is null");
        return;
      }
      
      if (null == slot.MachineObservationState) { // Postpone the process
        return;
      }
      
      if (!slot.BeginDateTime.HasValue) { // Do not record such slots
        return;
      }

      Initialize (slot.Machine);

      if (null == m_fields) { // Plugin not active
        return;
      }
      if (!m_fields.Any ()) { // No configured field
        return;
      }

      if (null == m_machineModules) { // Not a monitored machine: nothing to do
        return;
      }
      
      if (ExistsSameRange (slot)) { // And update the slot properties in the same time
        // A slot already exists with the same range: nothing to do
        return;
      }

      // Remove any data that are deprecated now
      Debug.Assert (null != slot.Machine);
      RemoveOverlapsNotSameRange (slot.Machine.Id, slot.DateTimeRange);
      
      if (CncByMachineModuleField.IsRangeValid (slot.DateTimeRange, log)
          && CncByMachineModuleField.IsObservationStateSlotValid (slot, log)) {
        Debug.Assert (null != m_machineModules);
        foreach (var machineModule in m_machineModules) {
          foreach (var field in m_fields) {
            var implementation = new CncByMachineModuleField (machineModule, field);
            implementation.Recompute (slot.DateTimeRange);
          }
        }
      }
    }
    
    void Remove (IObservationStateSlot slot)
    {
      // Do nothing, let AddModify handle this case later
    }
    
    void Remove (int machineId, int machineModuleId, int fieldId, DateTime startDateTime)
    {
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      using (var command = connection.CreateCommand ())
      {
        command.CommandText =
          string.Format (@"
DELETE FROM plugins.cncsummarybystateslot2_values
WHERE t.machineid={0} AND t.machinemoduleid={1}
  AND t.fieldid={2} AND t.startdatetime='{3}'
",
                         machineId, // 0: machineid
                         machineModuleId, // 1: machinemoduleid
                         fieldId, // 2: fieldid
                         startDateTime.ToString ("yyyy-MM-dd HH:mm:ss")
                        );
        try {
          command.ExecuteNonQuery();
        }
        catch (Exception ex) {
          log.ErrorFormat ("Remove: " +
                           "request error in SQL query {0} \n" +
                           "Exception: {1}",
                           command.CommandText,
                           ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Initialize() must be run before calling ExistsSameRange
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    bool ExistsSameRange (IObservationStateSlot slot)
    {
      Debug.Assert (m_initialized);
      
      Debug.Assert (null != slot);
      if (null == slot) {
        log.Error ("ExistsSameRange: null slot");
        return false;
      }
      
      if (null == m_machineModules) { // Not a monitored machine => no data
        return false;        
      }
      
      Debug.Assert (null != slot.MachineObservationState);
      if (null == slot.MachineObservationState) {
        log.Error ("ExistsSameRange: null machine observation state");
        return false;
      }
      
      UtcDateTimeRange range = slot.DateTimeRange;
      
      // In the table plugins.idletimeperperiod2, startdatetime and enddatetime are not null
      if (!range.Lower.HasValue) {
        return false;
      }
      if (!range.Upper.HasValue) {
        return false;
      }
      
      Debug.Assert (slot.BeginDay.HasValue);
      Debug.Assert (slot.EndDay.HasValue);
      
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      foreach (var machineModule in m_machineModules) {
        using (var command = connection.CreateCommand ())
        {
          command.CommandText =
            string.Format (@"
UPDATE plugins.cncsummarybystateslot2_values
SET startday='{4}', endday='{5}', machineobservationstateid={6}, shiftid={7}
WHERE machineid={0} AND machinemoduleid={1} AND startdatetime='{2}' AND enddatetime='{3}'
RETURNING id
",
                           slot.Machine.Id, // 0: machineid
                           machineModule.Id, // 1: machinemoduleid
                           range.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss"), // 2
                           range.Upper.Value.ToString ("yyyy-MM-dd HH:mm:ss"), // 3
                           slot.BeginDay.Value.ToString ("yyyy-MM-dd"), // 4:
                           slot.EndDay.Value.ToString ("yyyy-MM-dd"), // 5:
                           slot.MachineObservationState.Id, // 6:
                           (null == slot.Shift) ? "NULL": slot.Shift.Id.ToString () // 7:
                          );
          try {
            object result = command.ExecuteScalar ();
            if (null != result) {
              if (log.IsDebugEnabled) {
                log.Debug ($"ExistsSameRange: return true for machineModule {machineModule.Id}");
              }
              return true;
            }
          }
          catch (Exception ex) {
            log.Error ($"ExistsSameRange: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      }
      
      log.Debug ("ExistsSameRange: return false");
      return false;
    }

    /// <summary>
    /// Initialize() must be called before calling this method
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="range"></param>
    void RemoveOverlapsNotSameRange (int machineId, UtcDateTimeRange range)
    {
      Debug.Assert (range.Lower.HasValue);
      
      Debug.Assert (m_initialized);
      
      if (null == m_machineModules) { // Not a monitored machine, nothing to do
        return;
      }
      
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      foreach (var machineModule in m_machineModules) {
        using (var command = connection.CreateCommand ())
        {
          if (!range.Upper.HasValue) {
            command.CommandText =
              string.Format (@"
DELETE FROM plugins.cncsummarybystateslot2_values
WHERE machineid={0} AND machinemoduleid={1} AND enddatetime>'{2}'
",
                             machineId, // 0: machineid
                             machineModule.Id, // 1: machinemoduleid
                             range.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss")
                            );
            try {
              command.ExecuteNonQuery();
            }
            catch (Exception ex) {
              log.Error ($"RemoveOverlapsNotSameRange: request error in SQL query {command.CommandText}", ex);
              throw;
            }
            
          }
          else {
            command.CommandText =
              string.Format (@"
DELETE FROM plugins.cncsummarybystateslot2_values
WHERE machineid={0} AND machinemoduleid={1} AND enddatetime>'{2}' AND startdatetime<'{3}'
  AND NOT (startdatetime='{2}' AND enddatetime='{3}')
",
                             machineId, // 0: machineid
                             machineModule.Id, // 1: machinemoduleid
                             range.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss"),
                             range.Upper.Value.ToString ("yyyy-MM-dd HH:mm:ss")
                            );
            try {
              command.ExecuteNonQuery();
            }
            catch (Exception ex) {
              log.Error ($"RemoveOverlapsNotSameRange: request error in SQL query {command.CommandText}", ex);
              throw;
            }
          }
        }
      }
    }
  }
}
