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
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
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
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      var t1 = dateTime.AddSeconds (1);
      if (t1 < after) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (t1), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
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
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
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
      Assert.IsTrue (response.NoData);
    }

    public void CheckNotApplicable ()
    {
      CheckNotApplicable (m_name, m_machine, m_dateTime);
    }

    public void CheckNotApplicable (string name, IMachine machine, DateTime dateTime)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (name, machine, dateTime);
      Assert.IsTrue (response.NoData);
      Assert.IsTrue (response.NotApplicable);
    }
  }
}
