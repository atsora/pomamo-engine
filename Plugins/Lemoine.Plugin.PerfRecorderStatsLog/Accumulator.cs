// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.PerfRecorderStatsLog
{
  /// <summary>
  /// Accumulator
  /// </summary>
  internal class Accumulator
  {
    readonly ILog log = LogManager.GetLogger (typeof (Accumulator).FullName);

    readonly string m_key;
    readonly string m_logPrefix;
    readonly ILog m_perfLog;

    object m_lock = new object ();
    int m_count;
    TimeSpan m_max;
    TimeSpan m_min;
    TimeSpan m_average;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="logPrefix"></param>
    public Accumulator (string key, string logPrefix)
    {
      m_key = key;
      m_logPrefix = logPrefix ?? "";
      m_perfLog = LogManager.GetLogger (m_logPrefix + m_key);
    }
    #endregion // Constructors

    public void Record (TimeSpan duration)
    {
      lock (m_lock) {
        if (0 == m_count) {
          m_min = duration;
          m_max = duration;
          m_average = duration;
        }
        else {
          if (duration < m_min) {
            m_min = duration;
          }
          if (m_max < duration) {
            m_max = duration;
          }
          m_average = TimeSpan.FromTicks ((m_count * m_average.Ticks + duration.Ticks) / (m_count + 1));
        }
        ++m_count;
      }
    }

    void Reset ()
    {
      lock (m_lock) {
        m_count = 0;
        m_min = TimeSpan.FromTicks (0);
        m_max = TimeSpan.FromTicks (0);
        m_average = TimeSpan.FromTicks (0);
      }
    }

    /// <summary>
    /// Write in the log
    /// </summary>
    public void Write (TimeSpan period)
    {
      lock (m_lock) {
        if (m_perfLog.IsInfoEnabled) {
          m_perfLog.InfoFormat ("average={0} min={1} max={2} count={3} period={4}", m_average, m_min, m_max, m_count, period);
        }
        Reset ();
      }
    }
  }
}
