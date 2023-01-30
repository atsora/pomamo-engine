// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using System.Threading;
using Lemoine.Threading;
using System.Threading.Tasks;

namespace Lemoine.Plugin.PerfRecorderStatsLog
{
  public sealed class PerfRecorderExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IPerfRecorderExtension
    , IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (PerfRecorderExtension).FullName);

    Configuration m_configuration;
    DateTime m_lastRecord = DateTime.UtcNow;
    ReaderWriterLock m_lastRecordLock = new ReaderWriterLock ();
    IDictionary<string, Accumulator> m_accumulators = new Dictionary<string, Accumulator> ();
    ReaderWriterLock m_accumulatorsLock = new ReaderWriterLock ();

    public void Dispose ()
    {
      using (var readHolder = new ReadLockHolder (m_lastRecordLock)) {
        Write (DateTime.UtcNow.Subtract (m_lastRecord));
      }
    }

    public bool Initialize ()
    {
      return LoadConfiguration (out m_configuration);
    }

    public void Record (string key, TimeSpan duration)
    {
      // Write or not ?
      using (var readHolder = new ReadLockHolder (m_lastRecordLock)) {
        var now = DateTime.UtcNow;
        if (m_lastRecord.Add (m_configuration.Frequency) <= now) {
          using (var upgradeHolder = new UpgradeLockHolder (readHolder)) {
            if (m_lastRecord.Add (m_configuration.Frequency) <= now) {
              Write (now.Subtract (m_lastRecord));
              m_lastRecord = DateTime.UtcNow;
            }
          }
        }
      }

      var accumulator = GetAccumulator (key);
      accumulator.Record (duration);
    }

    Accumulator GetAccumulator (string key)
    {
      using (var readHolder = new ReadLockHolder (m_accumulatorsLock)) {
        Accumulator accumulator;
        if (m_accumulators.TryGetValue (key, out accumulator)) {
          return accumulator;
        }
        else {
          using (var upgradeHolder = new UpgradeLockHolder (readHolder)) {
            if (m_accumulators.TryGetValue (key, out accumulator)) {
              return accumulator;
            }
            else {
              var newAccumulator = new Accumulator (key, m_configuration.LogPrefix);
              m_accumulators[key] = newAccumulator;
              return newAccumulator;
            }
          }
        }
      }
    }

    public void Write (TimeSpan period)
    {
      using (var readHolder = new ReadLockHolder (m_accumulatorsLock)) {
        foreach (var accumulator in m_accumulators.Values) {
          accumulator.Write (period);
        }
      }
    }

    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
    }

    public Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }
  }
}
