// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// Description of CncByMachineModuleField.
  /// </summary>
  internal class CncByMachineModuleField
  {
    static readonly string UPSERT_ACTIVE_KEY = "upsert.active";
    static readonly bool UPSERT_ACTIVE_DEFAULT = false; // false until conflict errors are propagated to the parent table in case the table is partitioned

    static readonly string MAX_OBSERVATION_STATE_SLOT_DURATION_KEY = "plugin.CncSummaryByStateSlot2.MaxObservationStateSlotDuration";
    static readonly TimeSpan MAX_OBSERVATION_STATE_SLOT_DURATION_DEFAULT = TimeSpan.FromHours (12); // Consider a worker does not work more than 12 hours
    
    static readonly string LIMIT_TO_DEFINED_SHIFT_ONLY_KEY = "plugin.CncSummaryByStateSlot2.LimitToDefinedShiftOnly";
    static readonly bool LIMIT_TO_DEFINED_SHIFT_ONLY_DEFAULT = true;
    
    static readonly string LIMIT_TO_PRODUCTION_ONLY_KEY = "plugin.CncSummaryByStateSlot2.LimitToProductionOnly";
    static readonly bool LIMIT_TO_PRODUCTION_ONLY_DEFAULT = true;

    #region Members
    readonly IMachineModule m_machineModule;
    readonly IField m_field;
    
    // Cache
    IObservationStateSlot m_observationStateSlot = null;
    double m_cacheValue;
    TimeSpan m_cacheDuration;
    UtcDateTimeRange m_cacheRange;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CncByMachineModuleField).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncByMachineModuleField (IMachineModule machineModule, IField field)
    {
      m_machineModule = machineModule;
      m_field = field;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Process a new cnc value
    /// 
    /// To run inside a transaction
    /// </summary>
    /// <param name="v"></param>
    /// <param name="range"></param>
    public void Process (double v, UtcDateTimeRange range)
    {
      Debug.Assert (range.Lower.HasValue);
      
      DateTime observationStateSlotDateTime = range.Lower.Value;
      while (range.ContainsElement (observationStateSlotDateTime)) {
        LoadCache (observationStateSlotDateTime);
        Debug.Assert (null != m_observationStateSlot);
        
        // Compute the new data for the observationstateslot in cache
        CompleteCache (range, v);
        
        if (!m_observationStateSlot.EndDateTime.HasValue) {
          break;
        }
        else {
          observationStateSlotDateTime = m_observationStateSlot.EndDateTime.Value;
        }
      }
    }
    
    /// <summary>
    /// Store the data before a commit
    /// </summary>
    public void Store ()
    {
      Store (m_observationStateSlot, m_cacheValue, m_cacheDuration);
    }
    
    /// <summary>
    /// Recompute the data for the specified date/time range
    /// </summary>
    /// <param name="range"></param>
    public void Recompute (UtcDateTimeRange range)
    {
      Debug.Assert (range.Lower.HasValue);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("CncByMachineModuleField.Recompute"))
      {
        DateTime observationStateSlotDateTime = range.Lower.Value;
        while (range.ContainsElement (observationStateSlotDateTime)) {
          log.DebugFormat ("Recompute: " +
                           "observation state slot date/time is {0}",
                           observationStateSlotDateTime);
          ComputePeriod (observationStateSlotDateTime);
          Debug.Assert (null != m_observationStateSlot);
          if (!m_observationStateSlot.EndDateTime.HasValue) {
            break;
          }
          else {
            Debug.Assert (Bound.Compare<DateTime> (observationStateSlotDateTime,
                                                   m_observationStateSlot.EndDateTime.Value) < 0);
            observationStateSlotDateTime = m_observationStateSlot.EndDateTime.Value;
          }
        }
        
        transaction.Commit ();
      }
    }
    
    /// <summary>
    /// Clean the cache
    /// </summary>
    public void CleanCache ()
    {
      m_observationStateSlot = null;
      m_cacheValue = 0.0;
      m_cacheRange = new UtcDateTimeRange ();
      m_cacheDuration = TimeSpan.FromSeconds (0);
    }
    
    /// <summary>
    /// Compute the data for the observation state slot that contains the argument from
    /// </summary>
    /// <param name="from"></param>
    void ComputePeriod (DateTime from)
    {
      LoadCache (from);
      Debug.Assert (null != m_observationStateSlot);
      
      // Store this new data
      Store (m_observationStateSlot, m_cacheValue, m_cacheDuration);
    }
    
    internal static bool IsRangeValid (UtcDateTimeRange range, ILog log)
    {
      if (range.IsEmpty ()) {
        log.ErrorFormat ("IsRangeValid: " +
                         "range is empty, return false");
        return false;
      }
      
      if (!range.Duration.HasValue) {
        log.InfoFormat ("IsRangeValid: " +
                        "range has no duration " +
                        "=> return false");
        return false;
      }
      Debug.Assert (range.Lower.HasValue); // else no duration
      Debug.Assert (range.Upper.HasValue); // else no duration
      
      TimeSpan maxObservationStateSlotDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (MAX_OBSERVATION_STATE_SLOT_DURATION_KEY,
                               MAX_OBSERVATION_STATE_SLOT_DURATION_DEFAULT);
      if (maxObservationStateSlotDuration < range.Duration.Value) {
        log.InfoFormat ("IsRangeValid: " +
                        "range {0} is too long (longer than {1}) " +
                        "=> return false",
                        range.Duration.Value, maxObservationStateSlotDuration);
        return false;
      }
      
      return true;
    }
    
    internal static bool IsObservationStateSlotValid (IObservationStateSlot observationStateSlot, ILog log)
    {
      Debug.Assert (null != observationStateSlot);
      if (null == observationStateSlot) {
        log.ErrorFormat ("IsObservationStateSlotValid: " +
                         "null slot => return false");
        return false;
      }
      
      if (null == observationStateSlot.MachineObservationState) {
        log.DebugFormat ("IsObservationStateSlotValid: " +
                         "null machine observation state => return false");
        return false;
      }
      
      bool limitToDefinedShiftOnly = Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (LIMIT_TO_DEFINED_SHIFT_ONLY_KEY,
                           LIMIT_TO_DEFINED_SHIFT_ONLY_DEFAULT);
      if (limitToDefinedShiftOnly && (null == observationStateSlot.Shift)) {
        log.DebugFormat ("IsObservationStateSlotValid: " +
                         "null shift => return false");
        return false;
      }
      
      bool limitToProductionOnly = Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (LIMIT_TO_PRODUCTION_ONLY_KEY,
                           LIMIT_TO_PRODUCTION_ONLY_DEFAULT);
      if (limitToProductionOnly
          && (!observationStateSlot.Production.HasValue
              || !observationStateSlot.Production.Value)) {
        log.DebugFormat ("IsObservationStateSlotValid: " +
                         "not a production period => return false");
        return false;
      }
      
      return true;
    }
    
    void LoadCache (DateTime dateTime)
    {
      LoadCache (dateTime, null);
    }
    
    void LoadCache (DateTime dateTime, IObservationStateSlot proposedObservationStateSlot)
    {
      if (false == CheckCacheValidity (dateTime)) {
        // If the cache is not empty, store first the data before switching to the next observationstateslot
        if (null != m_observationStateSlot) {
          Store ();
        }
        
        // Clean the cache
        CleanCache ();
        
        // Load the observation state slot at dateTime
        if ( (null != proposedObservationStateSlot)
            && (proposedObservationStateSlot.DateTimeRange.ContainsElement (dateTime))) {
          // Proposed observation state slot is ok
          m_observationStateSlot = proposedObservationStateSlot;
        }
        else {
          m_observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (m_machineModule.MonitoredMachine, dateTime);
          if (null == m_observationStateSlot) {
            log.FatalFormat ("LoadCache: " +
                             "no observation state slot found at {0}",
                             dateTime);
            Debug.Assert (false);
            throw new Exception ("Missing observation state slot");
          }
        }
        Debug.Assert (null != m_observationStateSlot);
        
        if (!IsRangeValid (m_observationStateSlot.DateTimeRange, log)) {
          log.DebugFormat ("LoadCache: " +
                           "range {0} is not valid => return",
                           m_observationStateSlot.DateTimeRange);
          return;
        }
        Debug.Assert (m_observationStateSlot.DateTimeRange.Lower.HasValue);
        Debug.Assert (m_observationStateSlot.DateTimeRange.Upper.HasValue);
        if (!IsObservationStateSlotValid (m_observationStateSlot, log)) {
          log.DebugFormat ("LoadCache: " +
                           "observation state slot is not valid => return");
          return;
        }
        
        // Get all the cnc values in this period
        // and load the cache
        IList<ICncValue> cncValues = ModelDAOHelper.DAOFactory.CncValueDAO
          .FindByMachineFieldDateRange (m_machineModule, m_field, m_observationStateSlot.DateTimeRange);
        foreach (ICncValue cncValue in cncValues) {
          CompleteCache (cncValue);
        }
      }
      else { // Cache validity is ok
        Debug.Assert (null != m_observationStateSlot);
        // Check though observation state slot has not been updated since
        var observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindById (m_observationStateSlot.Id, m_machineModule.MonitoredMachine);
        if ( (null == observationStateSlot) || (observationStateSlot.Version != m_observationStateSlot.Version)) {
          // The observation state slot was updated
          // => invalid the cache
          log.InfoFormat ("LoadCache: " +
                          "the observation state slot {0} was updated",
                          observationStateSlot);
          CleanCache ();
          LoadCache (dateTime, observationStateSlot);
        }
      }
    }
    
    /// <summary>
    /// If true is returned, m_observationStateSlot is not null
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    bool CheckCacheValidity (DateTime dateTime)
    {
      // Check the cache is valid
      if ( (null != m_observationStateSlot)
          && (m_observationStateSlot.DateTimeRange.ContainsElement (dateTime))) { // The cache is ok
        // Note: using the Lock method may raise a NonUniqueObjectException
        IObservationStateSlot databaseSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindById (m_observationStateSlot.Id, m_observationStateSlot.Machine);
        if ( (null != databaseSlot) && (databaseSlot.Version == m_observationStateSlot.Version)) {
          m_observationStateSlot = databaseSlot;
          Debug.Assert (null != m_observationStateSlot);
          return true;
        }
        else {
          log.WarnFormat ("CheckCacheValidity: " +
                          "the observation state slot was updated " +
                          "=> invalid the cache");
          m_observationStateSlot = null;
          return false;
        }
      }
      else {
        return false;
      }
    }
    
    double ComputeNewAverage (double oldValue, TimeSpan oldDuration, double newValue, TimeSpan newDuration)
    {
      TimeSpan totalDuration = oldDuration.Add (newDuration);
      return oldDuration.TotalSeconds / totalDuration.TotalSeconds * oldValue
        + newDuration.TotalSeconds / totalDuration.TotalSeconds * newValue;
    }
    
    /// <summary>
    /// Complete the cache with a cnc value
    /// 
    /// Before calling this method, m_observationStateSlot must be not null
    /// </summary>
    /// <param name="cncValue"></param>
    void CompleteCache (ICncValue cncValue)
    {
      CompleteCache (cncValue.DateTimeRange, (double)cncValue.Value);
    }

    /// <summary>
    /// Before calling this method, m_observationStateSlot must be not null
    /// </summary>
    /// <param name="range"></param>
    /// <param name="v"></param>
    void CompleteCache (UtcDateTimeRange range, double v)
    {
      Debug.Assert (null != m_observationStateSlot);
      
      if (null == m_observationStateSlot) { // Empty the cache
        log.FatalFormat ("CompleteCache: " +
                         "m_observationStateSlot is null");
        return;
      }
      
      UtcDateTimeRange intersection =
        new UtcDateTimeRange (range.Intersects (m_observationStateSlot.DateTimeRange));
      if (!intersection.IsEmpty ()) {
        Debug.Assert (intersection.Duration.HasValue);
        if (m_cacheRange.IsEmpty ()) {
          m_cacheValue = v;
          m_cacheRange = intersection;
          m_cacheDuration = intersection.Duration.Value;
        }
        else {
          Debug.Assert (m_cacheRange.Duration.HasValue);
          Debug.Assert (!m_cacheRange.Overlaps (intersection));
          TimeSpan duration = intersection.Duration.Value;
          m_cacheValue = ComputeNewAverage (m_cacheValue, m_cacheDuration,
                                            v, duration);
          m_cacheRange = new UtcDateTimeRange (m_cacheRange.Lower, intersection.Upper);
          Debug.Assert (m_cacheRange.Lower.HasValue);
          Debug.Assert (m_cacheRange.Upper.HasValue);
          m_cacheDuration = m_cacheDuration.Add (duration);
        }
      }
    }
    
    void Store (IObservationStateSlot observationStateSlot, double v, TimeSpan duration)
    {
      if (null == observationStateSlot) { // Nothing to store
        return;
      }
      
      // Note: with PostgreSQL 9.5, INSERT ... ON CONFLICT UPDATE can be used
      
      // Note: startdatetime and enddatetime must be replaced with a tsrange
      //       If begin or end has no value, there may be some problems
      
      if (!IsRangeValid (m_observationStateSlot.DateTimeRange, log)) {
        log.DebugFormat ("Store: " +
                         "do nothing because the range {0} is not valid",
                         m_observationStateSlot.DateTimeRange);
        return;
      }
      Debug.Assert (observationStateSlot.DateTimeRange.Lower.HasValue);
      Debug.Assert (observationStateSlot.DateTimeRange.Upper.HasValue);
      if (!IsObservationStateSlotValid (m_observationStateSlot, log)) {
        log.DebugFormat ("Store: " +
                         "do nothing because observation state slot is not valid");
        return;
      }
      Debug.Assert (null != observationStateSlot.MachineObservationState);
      
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      using (var command = connection.CreateCommand ())
      {
        if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (UPSERT_ACTIVE_KEY, UPSERT_ACTIVE_DEFAULT)
            && ModelDAOHelper.DAOFactory.IsPostgreSQLVersionGreaterOrEqual (90500)) { // PostgreSQL >= 9.5, use upsert
          command.CommandText =
            string.Format (@"
INSERT INTO plugins.cncsummarybystateslot2_values (machineid, machinemoduleid, fieldid,
  startdatetime, startday, enddatetime, endday, machineobservationstateid, shiftid, average, computedseconds)
  VALUES ({0}, {1}, {2}, '{3}'::timestamp without time zone, '{4}'::date,
    '{5}'::timestamp without time zone, '{6}':: date, {7}, {8}::integer, {9}, {10})
ON CONFLICT ON CONSTRAINT cncsummarybystateslot2_values_machineid_machinemoduleid_fie_key
  DO UPDATE SET machineid={0}, machinemoduleid={1}, startday='{4}'::date, enddatetime='{5}'::timestamp without time zone,
      endday='{6}'::date,
      machineobservationstateid={7},
      shiftid={8}::integer,
      average={9}, computedseconds={10}",
                           m_machineModule.MonitoredMachine.Id, // 0: machineid
                           m_machineModule.Id, // 1: machinemoduleid
                           m_field.Id, // 2: fieldid
                           observationStateSlot.BeginDateTime.Value.ToString ("yyyy-MM-dd HH:mm:ss"), // 3: startdatetime
                           observationStateSlot.BeginDay.Value.ToString ("yyyy-MM-dd"), // 4: startday
                           observationStateSlot.EndDateTime.Value.ToString ("yyyy-MM-dd HH:mm:ss"), // 5: enddatetime
                           observationStateSlot.EndDay.Value.ToString ("yyyy-MM-dd"), // 6: endday
                           observationStateSlot.MachineObservationState.Id, // 7: machineobservationstateid
                           (null == m_observationStateSlot.Shift) ? "NULL": m_observationStateSlot.Shift.Id.ToString (), // 8: shiftid
                           v.ToString (CultureInfo.InvariantCulture), // 9: average
                           duration.TotalSeconds.ToString (CultureInfo.InvariantCulture) // 10: computedseconds
                          );
        }
        else { // PostgreSQL < 9.5, upsert is not available
          command.CommandText = string.Format (@"
WITH new_values (machineid, machinemoduleid, fieldid, startdatetime, startday, enddatetime, endday,
  machineobservationstateid, shiftid, average, computedseconds) AS (
  VALUES
  ({0}, {1}, {2}, '{3}'::timestamp without time zone, '{4}'::date, '{5}'::timestamp without time zone, '{6}'::date,
  {7}, {8}::integer, {9}, {10})
),
upsert AS
(
  UPDATE plugins.cncsummarybystateslot2_values t
  SET startday=n.startday, enddatetime=n.enddatetime, endday=n.endday,
      machineobservationstateid=n.machineobservationstateid,
      shiftid=n.shiftid,
      average=n.average, computedseconds=n.computedseconds
  FROM new_values n
  WHERE t.machineid={0} AND t.machinemoduleid={1}
    AND t.fieldid={2} AND t.startdatetime=n.startdatetime
  RETURNING t.*
)
INSERT INTO plugins.cncsummarybystateslot2_values (machineid, machinemoduleid, fieldid,
  startdatetime, startday, enddatetime, endday, machineobservationstateid, shiftid, average, computedseconds)
SELECT machineid, machinemoduleid, fieldid, startdatetime, startday, enddatetime, endday,
  machineobservationstateid, shiftid, average, computedseconds
FROM new_values
WHERE NOT EXISTS (SELECT 1 FROM upsert up WHERE up.machineid=new_values.machineid
  AND up.machinemoduleid=new_values.machinemoduleid
  AND up.fieldid=new_values.fieldid
  AND up.startdatetime=new_values.startdatetime)",
                                               m_machineModule.MonitoredMachine.Id,
                                               m_machineModule.Id,
                                               m_field.Id,
                                               observationStateSlot.BeginDateTime.Value.ToString ("yyyy-MM-dd HH:mm:ss"),
                                               observationStateSlot.BeginDay.Value.ToString ("yyyy-MM-dd"),
                                               observationStateSlot.EndDateTime.Value.ToString ("yyyy-MM-dd HH:mm:ss"),
                                               observationStateSlot.EndDay.Value.ToString ("yyyy-MM-dd"),
                                               observationStateSlot.MachineObservationState.Id,
                                               (null == m_observationStateSlot.Shift) ? "NULL": m_observationStateSlot.Shift.Id.ToString (),
                                               v.ToString (CultureInfo.InvariantCulture),
                                               duration.TotalSeconds.ToString (CultureInfo.InvariantCulture)
                                              );
        }
        try {
          command.ExecuteNonQuery();
        }
        catch (Exception ex) {
          log.ErrorFormat ("Store: " +
                           "request error in SQL query {0} \n" +
                           "Exception: {1}",
                           command.CommandText,
                           ex);
          throw;
        }
      }
    }
    #endregion // Methods
  }
}
