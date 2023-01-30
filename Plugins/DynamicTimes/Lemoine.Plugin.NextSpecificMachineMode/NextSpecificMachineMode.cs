// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.NextSpecificMachineMode
{
  public class NextSpecificMachineMode
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger<NextSpecificMachineMode> ();

    static readonly string TIMEOUT_KEY = "NextSpecificMachineMode.NextSpecificMachineMode.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    Configuration m_configuration = null;
    IEnumerable<IMachineMode> m_endMachineModes = new List<IMachineMode> ();
    IEnumerable<IMachineMode> m_cancelMachineModes = new List<IMachineMode> ();
    IEnumerable<IMachineMode> m_startUpMachineModes = new List<IMachineMode> ();

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.ErrorFormat ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        Debug.Assert (null != m_configuration); // Load successful
        return m_configuration.Name;
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("NextSpecificMachineMode.IsApplicableAt")) {
          var factAt = ModelDAOHelper.DAOFactory.FactDAO
            .FindAt (this.Machine, dateTime);
          if (null != factAt) {
            if (m_startUpMachineModes.Any (x => Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (x, factAt.CncMachineMode)))) {
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
            if (m_cancelMachineModes.Any (x => Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (x, factAt.CncMachineMode)))) {
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            return DynamicTimeApplicableStatus.YesAtDateTime;
          }
          else {
            if (IsFactAfter (dateTime)) {
              if (m_startUpMachineModes.Any (x => Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (x.Id, (int)MachineModeId.NoData)))) {
                return DynamicTimeApplicableStatus.YesAtDateTime;
              }
              if (m_cancelMachineModes.Any (x => Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (x.Id, (int)MachineModeId.NoData)))) {
                return DynamicTimeApplicableStatus.NoAtDateTime;
              }
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.Pending;
            }
          }
        }
      }
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      var range = new UtcDateTimeRange (limit
        .Intersects (hint)
        .Intersects (new UtcDateTimeRange (dateTime)));
      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Get: dateTime={0} limit={1} hint={2} => No data", dateTime, limit, hint);
        }
        return this.CreateNoData ();
      }
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (range.Lower.HasValue);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTime.NextActiveMachineMode", TransactionLevel.ReadCommitted, transactionLevelOptional: true)) {
          var step = TimeSpan.FromHours (4);
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindOverlapsRangeAscending (this.Machine, range, step);
          bool? startUpPhase = null;
          if (range.ContainsElement (dateTime)) {
            startUpPhase = true;
          }
          if (!m_startUpMachineModes.Any ()) {
            startUpPhase = false;
          }
          var monitoredMachine = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Machine.MonitoredMachineFromId (this.Machine.Id));
          var newHintLower = hint.Lower;
          foreach (var fact in ModelDAOHelper.DAOFactory.FactDAO.GetNoGap (monitoredMachine, range, facts)) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (!IsStartUpMachineMode (fact.CncMachineMode)) {
              startUpPhase = false;
            }
            if (IsEndMachineMode (fact.CncMachineMode)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: end at {fact.DateTimeRange.Lower} for machine mode {fact.CncMachineMode.Id}");
              }
              Debug.Assert (fact.DateTimeRange.Lower.HasValue);
              return this.CreateFinal (fact.DateTimeRange.Lower.Value);
            }
            if (IsCancelMachineMode (fact.CncMachineMode)) {
              if (IsStartUpPhase (startUpPhase, dateTime, range)) {
                startUpPhase = true;
              }
              else {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: cancel machine mode {fact.CncMachineMode.Id} detected at {fact.DateTimeRange.Lower} => return NoData");
                }
                return this.CreateNoData ();
              }
            }
            newHintLower = fact.DateTimeRange.Upper.Value;
          }
          if (limit.ContainsElement (newHintLower)) {
            var newHint = hint.Intersects (new UtcDateTimeRange (newHintLower));
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: new hint is {newHint}");
            }
            return this.CreateWithHint (new UtcDateTimeRange (newHint));
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: limit {limit} does not contain new hint lower {newHintLower} => no data");
            }
            return this.CreateNoData ();
          }
        }
      }
    }

    bool IsStartUpPhase (bool? startUpPhase, DateTime dateTime, UtcDateTimeRange range)
    {
      if (startUpPhase.HasValue) {
        return startUpPhase.Value;
      }
      Debug.Assert (Bound.Compare<DateTime> (dateTime, range.Lower) < 0);
      Debug.Assert (range.Lower.HasValue);
      var testRange = new UtcDateTimeRange (dateTime, range.Lower.Value);
      var step = TimeSpan.FromHours (4);
      var facts = ModelDAOHelper.DAOFactory.FactDAO
        .FindOverlapsRangeAscending (this.Machine, testRange, step);
      var monitoredMachine = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Machine.MonitoredMachineFromId (this.Machine.Id));
      var noGapFacts = ModelDAOHelper.DAOFactory.FactDAO.GetNoGap (monitoredMachine, range, facts);
      return noGapFacts.All (f => IsStartUpMachineMode (f.CncMachineMode));
    }

    bool IsEndMachineMode (IMachineMode machineMode)
    {
      return m_endMachineModes.Any (x => IsSubMachineMode (x, machineMode));
    }

    bool IsCancelMachineMode (IMachineMode machineMode)
    {
      return m_cancelMachineModes.Any (x => IsSubMachineMode (x, machineMode));
    }

    bool IsStartUpMachineMode (IMachineMode machineMode)
    {
      return m_startUpMachineModes.Any (x => IsSubMachineMode (x, machineMode));
    }

    bool IsSubMachineMode (IMachineMode ancestor, IMachineMode descendant)
    {
      return Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (ancestor, descendant));
    }

    bool IsFactAfter (DateTime dateTime)
    {
      var nextFact = ModelDAOHelper.DAOFactory.FactDAO
        .FindFirstFactAfter (this.Machine, dateTime);
      return (null != nextFact);
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) {
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      log = LogManager.GetLogger (typeof (NextSpecificMachineMode).FullName + "." + machine.Id);

      this.Machine = machine;

      if (string.IsNullOrEmpty (this.Name)) {
        // The configuration is loaded in Name.get
        return false;
      }

      Debug.Assert (null != m_configuration);
      if (!m_configuration.CheckMachineFilter (machine)) {
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_endMachineModes = m_configuration.EndMachineModeIds
          .Select (x => ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (x))
          .ToList ();
        m_cancelMachineModes = m_configuration.CancelMachineModeIds
          .Select (x => ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (x))
          .ToList ();
        m_startUpMachineModes = m_configuration.StartUpMachineModeIds
          .Select (x => ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (x))
          .ToList ();
      }

      return !string.IsNullOrEmpty (this.Name)
        && m_endMachineModes.Any ()
        && m_endMachineModes.All (x => (null != x))
        && m_cancelMachineModes.All (x => (null != x))
        && m_startUpMachineModes.All (x => (null != x));
    }
  }
}
