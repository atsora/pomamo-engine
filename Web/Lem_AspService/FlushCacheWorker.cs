// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Config;
using Lemoine.Core.Cache;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_AspService
{
  /// <summary>
  /// Background service worker to flush regularly the cache
  /// </summary>
  public class FlushCacheWorker : BackgroundServiceWorker
  {
    static readonly string CONFIG_UPDATE_CHECKER_KEY = "WebService.ConfigUpdateChecker";
    static readonly string CONFIG_UPDATE_CHECKER_DEFAULT = "";

    static readonly string COLLECT_MAX_MEMORY_KEY = "WebService.Memory.Collect.MaxMemory";
    static readonly long COLLECT_MAX_MEMORY_DEFAULT = 1024 * 1024 * 1000; // 1000 MB

    static readonly string COLLECT_MIN_FREQUENCY_KEY = "WebService.Memory.Collect.MinFrequency";
    static readonly TimeSpan COLLECT_MIN_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);

    static readonly string CACHE_CLEAN_FREQUENCY_KEY = "WebService.Cache.Clean.Frequency";
    static readonly TimeSpan CACHE_CLEAN_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (3);

    static readonly string CACHE_CLEAN_MAX_MEMORY_KEY = "WebService.Cache.Clean.MaxMemory"; // in bytes
    static readonly long CACHE_CLEAN_MAX_MEMORY_DEFAULT = 1024 * 1024 * 800; // 800 MB

    static readonly string CACHE_CLEAN_MIN_FREQUENCY_KEY = "WebService.Cache.Clean.MinFrequency";
    static readonly TimeSpan CACHE_CLEAN_MIN_FREQUENCY_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string CACHE_FLUSH_FREQUENCY_KEY = "WebService.Cache.Flush.Frequency";
    static readonly TimeSpan CACHE_FLUSH_FREQUENCY_DEFAULT = TimeSpan.FromHours (12);

    static readonly string CACHE_FLUSH_MAX_MEMORY_KEY = "WebService.Cache.Flush.MaxMemory"; // in bytes
    static readonly long CACHE_FLUSH_MAX_MEMORY_DEFAULT = 1024 * 1024 * 1400; // 1400 MB

    static readonly string CACHE_FLUSH_MIN_FREQUENCY_KEY = "WebService.Cache.Flush.MinFrequency";
    static readonly TimeSpan CACHE_FLUSH_MIN_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);

    static readonly string DAY_SLOT_CACHE_CLEAR_FREQUENCY_KEY = "WebService.DaySlotCache.ClearFrequency";
    static readonly TimeSpan DAY_SLOT_CACHE_CLEAR_FREQUENCY_DEFAULT = TimeSpan.FromHours (8); // Three times a day

    static readonly string CACHE_SLEEP_TIME_KEY = "WebService.Cache.SleepTime";
    static readonly TimeSpan CACHE_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string MEMORY_PERCENTAGE_EXIT_KEY = "WebService.MemoryPercentageExit";
    static readonly int MEMORY_PERCENTAGE_EXIT_DEFAULT = 68;
    // 68% < 70% because of:  
    // https://blogs.msdn.microsoft.com/tom/2008/04/10/chat-question-memory-limits-for-32-bit-and-64-bit-processes/

    static readonly string MEMORY_MAX_EXIT_KEY = "WebService.MemoryMaxExit"; // in bytes
    static readonly long MEMORY_MAX_EXIT_DEFAULT = (long)1024 * 1024 * 2600; // 2600 MB
    // Note: according to  
    // https://blogs.msdn.microsoft.com/tom/2008/04/10/chat-question-memory-limits-for-32-bit-and-64-bit-processes/
    // for a 64-bit process, 2800 MB if using a 4 GB process or more if more RAM (around 70% of RAM + Pagefile)
    // So if needed this limit could be increased

    readonly ILog log = LogManager.GetLogger<FlushCacheWorker> ();

    #region Members
    readonly IConfigUpdateChecker m_configUpdateChecker = new ConfigUpdateChecker ();
    ICacheClientWithCleanExtension m_cacheClient;
    DateTime m_lastCollect = DateTime.UtcNow;
    TimeSpan m_cacheCleanFrequency = CACHE_CLEAN_FREQUENCY_DEFAULT;
    DateTime m_lastClean = DateTime.UtcNow;
    TimeSpan m_cacheFlushFrequency = CACHE_FLUSH_FREQUENCY_DEFAULT;
    DateTime m_lastFlush = DateTime.UtcNow;
    TimeSpan m_daySlotCacheClearFrequency = DAY_SLOT_CACHE_CLEAR_FREQUENCY_DEFAULT;
    #endregion // Members

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="webServiceThread">not null</param>
    public FlushCacheWorker (ICacheClientWithCleanExtension cacheClient)
    {
      m_cacheClient = cacheClient;
      // TODO: use dependency injection instead
      m_configUpdateChecker = Lemoine.Info.ConfigSet.LoadAndGet (CONFIG_UPDATE_CHECKER_KEY, CONFIG_UPDATE_CHECKER_DEFAULT).ToLowerInvariant () switch { 
        "dummy" => new DummyConfigUpdateChecker (),
        _ => new ConfigUpdateChecker ()
      };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task RunAsync (CancellationToken stoppingToken)
    {
      try {
        m_cacheCleanFrequency = (TimeSpan)Lemoine.Info.ConfigSet
          .LoadAndGet (CACHE_CLEAN_FREQUENCY_KEY,
                       CACHE_CLEAN_FREQUENCY_DEFAULT);
        m_cacheFlushFrequency = (TimeSpan)Lemoine.Info.ConfigSet
          .LoadAndGet (CACHE_FLUSH_FREQUENCY_KEY,
                       CACHE_FLUSH_FREQUENCY_DEFAULT);
        m_daySlotCacheClearFrequency = (TimeSpan)Lemoine.Info.ConfigSet
          .LoadAndGet (DAY_SLOT_CACHE_CLEAR_FREQUENCY_KEY,
                       DAY_SLOT_CACHE_CLEAR_FREQUENCY_DEFAULT);
        this.SleepTime = (TimeSpan)Lemoine.Info.ConfigSet
          .LoadAndGet (CACHE_SLEEP_TIME_KEY,
                       CACHE_SLEEP_TIME_DEFAULT);

        long maxMemoryExit = (long)Lemoine.Info.ConfigSet.LoadAndGet (MEMORY_MAX_EXIT_KEY,
                                                                      MEMORY_MAX_EXIT_DEFAULT);
        if (log.IsDebugEnabled) {
          log.Debug ($"RunAsync: max memory exit from configuration is {maxMemoryExit}");
        }
        long memoryLimitExit;
        try {
          var totalPhysicalMemory = Lemoine.Info.ComputerInfo.TotalPhysicalMemory;
          var memoryPercentageExit = Lemoine.Info.ConfigSet.LoadAndGet (MEMORY_PERCENTAGE_EXIT_KEY,
                                                      MEMORY_PERCENTAGE_EXIT_DEFAULT);
          memoryLimitExit = (long)(totalPhysicalMemory / 100)
            * (int)memoryPercentageExit;
          if (log.IsDebugEnabled) {
            log.Debug ($"RunAsync: temporary memory limit exit is {memoryLimitExit} from total {totalPhysicalMemory} and percentage {memoryPercentageExit}");
          }
          if (maxMemoryExit < memoryLimitExit) {
            memoryLimitExit = maxMemoryExit;
          }
        }
        catch (Exception ex) {
          log.Error ($"RunAsync: exception in trying to get memoryLimitExit, fallback to maxMemoryExit {maxMemoryExit}", ex);
          memoryLimitExit = maxMemoryExit;
        }
        log.Info ($"RunAsync: memory limit exit is {memoryLimitExit / 1024 / 1024} MB");

        DateTime cacheDateTime = DateTime.UtcNow.Add (m_cacheCleanFrequency);
        DateTime flushDateTime = DateTime.UtcNow.Add (m_cacheFlushFrequency);
        DateTime daySlotCacheClearDateTime = DateTime.UtcNow.Add (m_daySlotCacheClearFrequency);
        while (!stoppingToken.IsCancellationRequested && !this.Interrupted) {
          RunOnce (memoryLimitExit, ref cacheDateTime, ref flushDateTime, ref daySlotCacheClearDateTime);
          SetActive ();
          await Sleep (stoppingToken);
        }
      }
      catch (Exception ex) {
        try {
          if (stoppingToken.IsCancellationRequested) {
            log.Warn ($"RunAsync: cancellation requested for stopping token, return");
          }
          log.Fatal ("RunAsync: unexpected exception", ex);
        }
        catch (Exception) { }
        throw;
      }
    }

    void RunOnce (long memoryLimitExit, ref DateTime cacheDateTime, ref DateTime flushDateTime, ref DateTime daySlotCacheClearDateTime)
    {
      try {
        long physicalMemory = Lemoine.Info.ProgramInfo.GetPhysicalMemory ();
        CheckMemory (memoryLimitExit, physicalMemory);
        CheckConfigUpdate ();
        CheckClearDaySlotCache (ref daySlotCacheClearDateTime);
        CheckFlushCache (ref flushDateTime, physicalMemory);
        CheckCleanCache (ref cacheDateTime, physicalMemory);
        CheckCollect (physicalMemory);
        SetActive ();
      }
      catch (Exception ex) {
        try {
          log.Fatal ("RunOnce: exception, but try to continue", ex);
        }
        catch (Exception) { }

        CheckException (ex);
      }
    }

    void CheckException (Exception ex)
    {
      if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
        Exit (ex);
      }
    }

    void CheckMemory (long memoryLimitExit, long physicalMemory)
    {
      if (memoryLimitExit < physicalMemory) {
        log.Fatal ($"CheckMemory: memory limit {memoryLimitExit} is reached with {physicalMemory} => kill the service");
        Exit ();
      }
    }

    void CheckCollect (long physicalMemory)
    {
      if (((long)Lemoine.Info.ConfigSet.LoadAndGet (COLLECT_MAX_MEMORY_KEY, COLLECT_MAX_MEMORY_DEFAULT) < physicalMemory)
           && ((TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (COLLECT_MIN_FREQUENCY_KEY, COLLECT_MIN_FREQUENCY_DEFAULT)
                < DateTime.UtcNow.Subtract (m_lastCollect))) {
        log.Info ($"CheckCollect: collect, because the physical memory {physicalMemory} reached the limit");
        SetActive ();
        Collect ();
      }
    }

    void CheckCleanCache (ref DateTime cacheDateTime, long physicalMemory)
    {
      if ((TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (CACHE_CLEAN_MIN_FREQUENCY_KEY, CACHE_CLEAN_MIN_FREQUENCY_DEFAULT)
           < DateTime.UtcNow.Subtract (m_lastClean)) {
        if (cacheDateTime <= DateTime.UtcNow) { // Clean !
          log.Info ($"CheckCleanCache: clean the cache, because requested at {cacheDateTime}, physical memory={physicalMemory}");
          SetActive ();
          CleanCache ();
          cacheDateTime = DateTime.UtcNow.Add (m_cacheCleanFrequency);
        }
        else if ((long)Lemoine.Info.ConfigSet.LoadAndGet (CACHE_CLEAN_MAX_MEMORY_KEY, CACHE_CLEAN_MAX_MEMORY_DEFAULT) < physicalMemory) {
          log.Info ($"CheckCleanCache: clean the cache, because the physical memory {physicalMemory} reached the limit");
          SetActive ();
          CleanCache ();
          cacheDateTime = DateTime.UtcNow.Add (m_cacheCleanFrequency);
        }
      }
    }

    void CheckFlushCache (ref DateTime flushDateTime, long physicalMemory)
    {
      if ((TimeSpan)Lemoine.Info.ConfigSet.LoadAndGet (CACHE_FLUSH_MIN_FREQUENCY_KEY, CACHE_FLUSH_MIN_FREQUENCY_DEFAULT)
            < DateTime.UtcNow.Subtract (m_lastFlush)) {
        if (flushDateTime <= DateTime.UtcNow) { // Flush !
          if (log.IsInfoEnabled) {
            log.Info ($"CheckFlushCache: about to flush the cache because requested at {flushDateTime}");
          }
          SetActive ();
          FlushCache ();
          Collect ();
          flushDateTime = DateTime.UtcNow.Add (m_cacheFlushFrequency);
        }
        else if ((long)Lemoine.Info.ConfigSet.LoadAndGet (CACHE_FLUSH_MAX_MEMORY_KEY, CACHE_FLUSH_MAX_MEMORY_DEFAULT)
                  < physicalMemory) {
          if (log.IsInfoEnabled) {
            log.Info ($"CheckFlushCache: flush the cache, because the physical memory {physicalMemory} reached the limit");
          }
          SetActive ();
          FlushCache ();
          Collect ();
          flushDateTime = DateTime.UtcNow.Add (m_cacheFlushFrequency);
        }
      }
    }

    void CheckClearDaySlotCache (ref DateTime daySlotCacheClearDateTime)
    {
      if (daySlotCacheClearDateTime <= DateTime.UtcNow) { // Clear the DaySlotCache
        log.DebugFormat ("CheckClearDaySlotCache: about to clear the day slot cache");
        SetActive ();
        ClearDaySlotCache ();
        daySlotCacheClearDateTime = DateTime.UtcNow.Add (m_daySlotCacheClearFrequency);
      }
    }

    void Collect ()
    {
      try {
        GC.Collect ();
        m_lastCollect = DateTime.UtcNow;
      }
      catch (Exception ex) {
        log.Fatal ($"Collect: unexpected error occurred", ex);
      }
    }

    void CleanCache ()
    {
      try {
        m_cacheClient.CleanCache ();
        m_lastClean = DateTime.UtcNow;
      }
      catch (Exception ex) {
        log.Fatal ($"CleanCache: unexpected error occurred", ex);
      }
    }

    void FlushCache ()
    {
      try {
        m_cacheClient.FlushAll ();
        m_lastFlush = DateTime.UtcNow;
        m_lastClean = DateTime.UtcNow;
      }
      catch (Exception ex) {
        log.Fatal ($"FlushCache: unexpected error occurred", ex);
      }
    }

    void ClearDaySlotCache ()
    {
      try {
        Lemoine.GDBPersistentClasses.DaySlotCache.Clear ();
      }
      catch (Exception ex) {
        log.Fatal ($"ClearDaySlotCache: unexpected error occurred", ex);
      }
    }

    void CheckConfigUpdate ()
    {
      if (!m_configUpdateChecker.Check ()) {
        log.Error ("CheckConfigUpdate: config update, exit");
        Exit ();
      }
    }
    public override ILog GetLogger ()
    {
      return log;
    }
  }
}
