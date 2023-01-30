// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.AutoReason
{
  /// <summary>
  /// This class takes care of the AutoReason analysis
  /// of a unique monitored machine
  /// </summary>
  public class MachineAutoReasonAnalysis : ProcessOrThreadClass, IProcessOrThreadClass, IChecked
  {
    static readonly string BETWEEN_PLUGINS_SLEEP_KEY = "AutoReason.Sleep.BetweenPlugins";
    static readonly TimeSpan BETWEEN_PLUGINS_SLEEP_DEFAULT = TimeSpan.FromTicks (0);

    static readonly string ERROR_SLEEP_KEY = "AutoReason.Sleep.Error";
    static readonly TimeSpan ERROR_SLEEP_DEFAULT = TimeSpan.FromSeconds (1.0);

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "AutoReason.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (5);

    #region Members
    bool m_initialized = false;
    IMonitoredMachine m_machine;

    IEnumerable<Lemoine.Extensions.AutoReason.IAutoReasonExtension> m_extensions = null;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (MachineAutoReasonAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated monitored machine
    /// </summary>
    public IMonitoredMachine Machine
    {
      get { return m_machine; }
      private set
      {
        if (null == value) {
          log.Fatal ("Machine: " +
                     "it can't be null");
          throw new ArgumentNullException ();
        }
        else {
          m_machine = value;
          log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineAutoReasonAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public MachineAutoReasonAnalysis (IMonitoredMachine machine)
    {
      this.Machine = machine;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Run the analysis for the monitored machine
    /// 
    /// There is a check so that this is run only by one unique thread
    /// 
    /// This is a WaitCallback that can be used by a ThreadPool
    /// </summary>
    public void RunAnalysis (CancellationToken cancellationToken)
    {
      SetActive ();

      try { // m_running has just been hold, else the block above would have returned
        ResumeCheck ();

        SetActive ();

        DateTime refTime = DateTime.UtcNow;
        log.Debug ("RunAnalysis: run MakeAnalysis, the lock m_running has just been acquired");
        SetActive ();
        MakeAnalysis (cancellationToken);
        SetActive ();
      }
      catch (OperationCanceledException ex) {
        log.Error ($"RunAnalysis: operationCanceledException, cancel requested={cancellationToken.IsCancellationRequested}", ex);
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        if (ExceptionTest.IsStale (ex, log)) {
          log.Warn ("RunAnalysis: Stale exception {0}, skip it and try again later", ex);
        }
        else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
          var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
            .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
          log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
          this.Sleep (temporaryWithDelayExceptionSleepTime, cancellationToken);
        }
        else if (ExceptionTest.IsTemporary (ex, log)) {
          log.Warn ("RunAnalysis: temporary (serialization) failure => try again later", ex);
        }
        else if (ExceptionTest.RequiresExit (ex, log)) {
          // OutOfMemory or NHibernate.TransactionException which is not temporary
          // (TransactionSerializationFailure or TimeoutFailure)
          log.Error ($"RunAnalysis: Exception with inner exception {ex.InnerException} requires to exit, give up", ex);
          this.SetExitRequested ();
          throw new Lemoine.Threading.AbortException ("Exception requires to exit", ex);
        }
        else {
          log.Error ("RunAnalysis: an other exception occurred", ex);
        }
      }
      finally { // Free any potential m_running lock
        log.Debug ("RunAnalysis: completed");
        PauseCheck ();
      }
    }

    /// <summary>
    /// Initialize
    /// </summary>
    protected virtual void Initialize ()
    {
      Debug.Assert (null != m_machine);

      if (!m_initialized) {
        log.Debug ("Initialize /B");

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // TODO: Initialize...

          SetActive ();
        }

        log.Debug ("Initialize: initialize the extensions");
        m_extensions = Lemoine.Extensions.ExtensionManager.GetExtensions<Lemoine.Extensions.AutoReason.IAutoReasonExtension> ()
          .Where (extension => extension.Initialize (m_machine, this))
          .ToList ();

        log.Debug ("Initialize /E");
        m_initialized = true;
      }
    }

    /// <summary>
    /// Make the analysis without checking the thread concurrency
    /// 
    /// This method is public for the unit tests
    /// </summary>
    public virtual void MakeAnalysis (CancellationToken cancellationToken)
    {
      Debug.Assert (null != m_machine);

      Initialize ();

      if (log.IsDebugEnabled) {
        log.Debug ("MakeAnalysis /B");
      }

      // Run the analysis of the extensions
      var betweenPluginsSleepTime = Lemoine.Info.ConfigSet.LoadAndGet (BETWEEN_PLUGINS_SLEEP_KEY, BETWEEN_PLUGINS_SLEEP_DEFAULT);
      foreach (var extension in m_extensions) {
        if (cancellationToken.IsCancellationRequested) {
          log.Error ("MakeAnalysis: cancellation requested");
          return;
        }
        try {
          RunAnalysis (extension);
          this.Sleep (betweenPluginsSleepTime, cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"MakeAnalysis: RunAnalysis of {extension}", ex);
          var errorSleep = Lemoine.Info.ConfigSet.LoadAndGet (ERROR_SLEEP_KEY, ERROR_SLEEP_DEFAULT);
          this.Sleep (errorSleep, cancellationToken);
        }
      }
    }

    void RunAnalysis (IAutoReasonExtension extension)
    {
      string assemblyFullName = "";
      DateTime? startDateTime = null;
      if (log.IsDebugEnabled) {
        startDateTime = DateTime.UtcNow;
        assemblyFullName = extension.GetType ().FullName;
        log.DebugFormat ("MakeAnalysis: RunOnce on extension {0}", assemblyFullName);
      }

      var pluginName = assemblyFullName.Replace ("Lemoine.Plugin.", "");
      var performanceKey = "AutoReason." + pluginName + "." + this.Machine.Id;
      using (var performanceTracker = new PerfTracker (performanceKey)) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          extension.RunOnce ();
        } // Session
      } // PerformanceTracker
      SetActive ();

      if (log.IsDebugEnabled) { // With the performance tracker, this is not really required any more
        TimeSpan? duration = null;
        if (string.IsNullOrEmpty (assemblyFullName)) {
          assemblyFullName = extension.GetType ().FullName;
        }
        if (startDateTime.HasValue) {
          duration = DateTime.UtcNow.Subtract (startDateTime.Value);
        }
        log.DebugFormat ("MakeAnalysis: RunOnce on extension {0} completed in {1}",
          assemblyFullName, duration);
      }
    }
    #endregion // Methods

    #region Implementation of ProcessClass
    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override string GetStampFileName ()
    {
      return "AutoReasonAnalysisStamp-" + m_machine.Id;
    }

    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      RunAnalysis (cancellationToken);
    }

    /// <summary>
    /// <see cref="ProcessClass">ProcessClass</see> implementation
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // Implementation of ProcessClass
  }
}
