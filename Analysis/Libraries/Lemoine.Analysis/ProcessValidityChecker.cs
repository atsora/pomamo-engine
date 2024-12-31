// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Analysis
{
  public sealed class ProcessValidityChecker
  {
    readonly ILog m_log;
    readonly MonitoredMachineActivityAnalysis m_machineActivityAnalysis;
    readonly DateTime m_startDateTime;
    readonly TimeSpan m_minTime;
    readonly DateTime m_maxDateTime;
    DateTime m_resetDateTime;
    DateTime m_minDateTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineActivityAnalysis"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="minTime">min time per machine module</param>
    /// <param name="log"></param>
    public ProcessValidityChecker (MonitoredMachineActivityAnalysis machineActivityAnalysis, DateTime maxDateTime, TimeSpan minTime, ILog log)
    {
      m_machineActivityAnalysis = machineActivityAnalysis;
      m_startDateTime = DateTime.UtcNow;
      m_minTime = minTime;
      m_maxDateTime = maxDateTime;
      m_log = log;
      ResetStartDateTime ();
    }

    /// <summary>
    /// Reset the start date/time
    /// </summary>
    public void ResetStartDateTime ()
    {
      m_resetDateTime = DateTime.UtcNow;
      m_minDateTime = m_resetDateTime
        .Add (m_minTime);
    }

    /// <summary>
    /// Check if the process is still valid
    /// </summary>
    /// <returns></returns>
    public bool IsValid ()
    {
      var now = DateTime.UtcNow;
      if ((m_minDateTime < now) && (m_maxDateTime < now)) {
        var timeSpent = now.Subtract (m_startDateTime);
        m_log.Warn ($"IsValid: the time spent is already {timeSpent} => return false");
        return false;
      }

      if (m_machineActivityAnalysis.IsPauseRequested ()) {
        m_log.Info ("IsValid: interrupt the analysis " +
                    "because the modification analysis Id is run on the same monitored machine");
        return false;
      }

      return true;
    }
  }


}
