// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.SwitchByDetectedCyclesProduction
{
  public abstract class Switch
    : MultipleInstanceConfigurableExtension<Configuration>
  {
    readonly ILog log = LogManager.GetLogger (typeof (Switch).FullName);

    string m_suffix;
    Configuration m_configuration = null;
    string m_applicablePrefix;

    protected Switch (string suffix)
    {
      Debug.Assert (!string.IsNullOrEmpty (suffix));
      m_suffix = suffix;
    }

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
        if ((null != m_configuration) && (null != m_configuration.NamePrefix)) {
          return m_configuration.NamePrefix + m_suffix;
        }
        else {
          return m_suffix;
        }
      }
    }

    public bool IsApplicable ()
    {
      var name = GetApplicableName ();
      return Lemoine.Business.DynamicTimes.DynamicTime
        .IsApplicable (name, this.Machine);
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      var name = GetApplicableName ();
      return Lemoine.Business.DynamicTimes.DynamicTime
        .IsApplicableAt (name, this.Machine, dateTime);
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var name = GetApplicableName ();
      return Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, this.Machine, dateTime, hint, limit);
    }

    string GetApplicableName ()
    {
      return m_applicablePrefix + m_suffix;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return CacheTimeOut.NoCache.GetTimeSpan ();
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;

      if (string.IsNullOrEmpty (this.Name)) {
        // The configuration is loaded in Name.get
        return false;
      }

      Debug.Assert (null != m_configuration);
      if (!m_configuration.CheckMachineFilter (machine)) {
        return false;
      }

      var now = DateTime.UtcNow;
      var range = new UtcDateTimeRange (now.Subtract (m_configuration.Period), now);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("SwitchByDetectedCyclesProduction.Initialize")) {
          var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindFirstFullOverlapsRange (machine, range, 1, true);
          if (cycles.Any ()) {
            m_applicablePrefix = m_configuration.WithCyclesPrefix;
          }
          else {
            m_applicablePrefix = m_configuration.NoCyclePrefix;
          }
        }
      }

      return !string.IsNullOrEmpty (m_applicablePrefix);
    }
  }
}
