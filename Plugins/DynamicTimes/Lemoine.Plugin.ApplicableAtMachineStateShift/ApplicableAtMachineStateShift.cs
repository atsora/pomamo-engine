// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;

namespace Lemoine.Plugin.ApplicableAtMachineStateShift
{
  /// <summary>
  /// ApplicableAtMachineStateShift
  /// </summary>
  public class ApplicableAtMachineStateShift
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicableAtMachineStateShift).FullName);

    Configuration m_configuration = null;
    string m_redirectName;
    IEnumerable<IDynamicTimeExtension> m_redirectExtensions = null;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicableAtMachineStateShift ()
    {
    }

    public IMachine Machine { get; private set; }

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

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var machineStateShiftEnd = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.MachineState.MachineShiftSlotEnd (this.Machine, dateTime));
      if (!machineStateShiftEnd.HasValue) {
        log.Error ($"Get: no machine shift for machine {this.Machine.Id} at {dateTime}");
        return this.CreateNotApplicable ();
      }
      var machineStateShiftLimit = new UtcDateTimeRange (dateTime, machineStateShiftEnd.Value, "[]");
      var effectiveLimit = new UtcDateTimeRange (limit.Intersects (machineStateShiftLimit));
      var response = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime (m_redirectName, this.Machine, dateTime, hint, effectiveLimit);
      if (response.NoData) {
        return this.CreateNotApplicable ();
      }
      return response;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) { // This is known...
        return TimeSpan.MaxValue;
      }

      var redirectExtensions = GetRedirectExtensions ();
      if (!redirectExtensions.Any ()) {
        log.Error ($"GetCacheTimeOut: no extension {m_redirectName} is registered");
        return CacheTimeOut.Config.GetTimeSpan ();
      }
      return redirectExtensions.Min (ext => ext.GetCacheTimeout (data));
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

      m_redirectName = m_configuration.RedirectName;

      return !string.IsNullOrEmpty (this.Name) && !string.IsNullOrEmpty (m_redirectName);
    }

    IEnumerable<IDynamicTimeExtension> GetRedirectExtensions ()
    {
      if (null == m_redirectExtensions) {
        m_redirectExtensions = Lemoine.Business.DynamicTimes.DynamicTime.GetExtensions (m_redirectName, this.Machine);
      }
      return m_redirectExtensions;
    }

    public bool IsApplicable ()
    {
      return GetRedirectExtensions ().Any (ext => Lemoine.Business.DynamicTimes.DynamicTime.IsApplicable (ext));
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      var redirectExtensions = GetRedirectExtensions ();
      if (!redirectExtensions.Any ()) {
        log.Error ($"IsApplicableAt: no redirect extension");
        return DynamicTimeApplicableStatus.Never;
      }
      var machineStateShiftEnd = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.MachineState.MachineShiftSlotEnd (this.Machine, dateTime));
      if (!machineStateShiftEnd.HasValue) {
        log.Error ($"IsApplicableAt: no machine shift for machine {this.Machine.Id} at {dateTime}");
        var redirectIsApplicableAt = Lemoine.Business.DynamicTimes.DynamicTime.IsApplicableAt (redirectExtensions, this.Machine, dateTime);
        if (redirectIsApplicableAt.Equals (DynamicTimeApplicableStatus.Never)) {
          return DynamicTimeApplicableStatus.Never;
        }
        else {
          return DynamicTimeApplicableStatus.NoAtDateTime;
        }
      }
      return Lemoine.Business.DynamicTimes.DynamicTime.IsApplicableAt (redirectExtensions, this.Machine, dateTime);
    }
    #endregion // Constructors

  }
}
