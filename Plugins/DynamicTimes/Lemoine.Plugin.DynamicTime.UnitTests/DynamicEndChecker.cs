// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using NUnit.Framework;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// DynamicEndChecker
  /// </summary>
  internal class DynamicEndChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (DynamicEndChecker).FullName);

    readonly string m_name;
    readonly IMachine m_machine;
    readonly DateTime m_dateTime;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DynamicEndChecker (string name, IMachine machine, DateTime dateTime)
    {
      m_name = name;
      m_machine = machine;
      m_dateTime = dateTime;
    }
    #endregion // Constructors

    public void CheckPending (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    public void CheckPending ()
    {
      CheckPending (m_name, m_machine, m_dateTime);
    }

    public void CheckAfter (string name, IMachine machine, DateTime dateTime, DateTime after)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      var t1 = dateTime.AddSeconds (1);
      if (t1 < after) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (t1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    public void CheckAfter (DateTime after)
    {
      CheckAfter (m_name, m_machine, m_dateTime, after);
    }

    public void CheckFinal (DateTime final) {
      CheckFinal (m_name, m_machine, m_dateTime, final);
    }

    public void CheckFinal (string name, IMachine machine, DateTime dateTime, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    public void CheckNoData ()
    {
      CheckNoData (m_name, m_machine, m_dateTime);
    }

    public void CheckNoData (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.That (response.NoData, Is.True);
    }

    public void CheckNotApplicable ()
    {
      CheckNotApplicable (m_name, m_machine, m_dateTime);
    }

    public void CheckNotApplicable (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.Multiple (() => {
        Assert.That (response.NoData, Is.True);
        Assert.That (response.NotApplicable, Is.True);
      });
    }
  }
}
