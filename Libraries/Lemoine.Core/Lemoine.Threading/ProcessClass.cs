// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Lemoine.Core.Log;
using Lemoine.Info;

namespace Lemoine.Threading
{
  /// <summary>
  /// Base <see cref="ILoggedProcessClass">ILoggedProcessClass</see> implementation
  /// </summary>
  public abstract class ProcessClass: ILoggedProcessClass, IChecked, ILogged
  {
    #region Members
    DateTime m_startDateTime = DateTime.UtcNow;
    TimeSpan? m_timeout = null;
    TimeSpan? m_notRespondingTimeout = null;
    TimeSpan? m_sleepBeforeRestart = null;
    bool m_useStampFile = true;
    int m_parentProcessId = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProcessClass).FullName);

    #region Getters / Setters
    /// <summary>
    /// Time after which the thread kills itself.
    /// 
    /// null: not set
    /// </summary>
    public TimeSpan? Timeout {
      get { return m_timeout; }
      set { m_timeout = value; }
    }

    /// <summary>
    /// Not responding timeout (null: unknown / not set)
    /// </summary>
    public TimeSpan? NotRespondingTimeout {
      get { return m_notRespondingTimeout; }
      set { m_notRespondingTimeout = value; }
    }
    
    /// <summary>
    /// Time to sleep before restarting a malfunctioning thread (null: unknown / not set)
    /// </summary>
    public TimeSpan? SleepBeforeRestart {
      get { return m_sleepBeforeRestart; }
      set { m_sleepBeforeRestart = value; }
    }

    /// <summary>
    /// Use a stamp file to check a process
    /// </summary>
    public bool UseStampFile
    {
      get { return m_useStampFile; }
      set { m_useStampFile = value; }
    }
    
    /// <summary>
    /// Parent process ID
    /// </summary>
    public int ParentProcessId
    {
      get { return m_parentProcessId; }
      set { m_parentProcessId = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the stamp file to use to check a process
    /// </summary>
    public abstract string GetStampFileName ();

    /// <summary>
    /// Return the stamp file path to use to check a process
    /// </summary>
    /// <returns></returns>
    public virtual string GetStampFilePath ()
    {
      string stampFileName = GetStampFileName ();
      return Path.Combine (Lemoine.Info.PulseInfo.CommonConfigurationDirectory, stampFileName);
    }

    /// <summary>
    /// Set the thread as active
    /// 
    /// If the parent process ID is defined and the parent process
    /// does not exist any more, exit
    /// </summary>
    public virtual void SetActive ()
    {
      GetLogger ().Debug ($"SetActive: set the process is active at {DateTime.UtcNow}");
      
      if (m_useStampFile) {
        // Update the timestamp
        var stampFilePath = GetStampFilePath ();
        if (!File.Exists (stampFilePath)) {
          using (FileStream stream = File.Create (stampFilePath))
          { }
        }
        File.SetLastWriteTimeUtc (stampFilePath, DateTime.UtcNow);
      }

      if (0 != m_parentProcessId) {
        try {
          Process process = Process.GetProcessById (m_parentProcessId);
        }
        catch (Exception ex) {
          GetLogger ().Info ($"SetActive: parent process with PID {m_parentProcessId} does not exist any more => exit", ex);
          System.Environment.Exit (0);
          this.Sleep (TimeSpan.FromSeconds (2));
          GetLogger ().Fatal ($"SetActive: not stopped after 2 seconds => force exit");
          Lemoine.Core.Environment.ForceExit ();
        }
      }
      
      // Check if Timeout was reached
      if (this.Timeout.HasValue
          && (m_startDateTime.Add (this.Timeout.Value) < DateTime.UtcNow)) {
        GetLogger ().Warn ($"SetActive: timeout detected after {DateTime.UtcNow.Subtract (m_startDateTime)} when timeout is {this.Timeout.Value} => exit");
        Lemoine.Core.Environment.ForceExit ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck()
    {
      // It is not possible to pause the check of a process
      return;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck()
    {
      // It is not possible to pause the check of a process
      return;
    }

    /// <summary>
    /// <see cref="IProcessClass"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="apartmentState"></param>
    public void Start (CancellationToken? cancellationToken = null, ApartmentState apartmentState = ApartmentState.MTA)
    {
      Run (cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Main method
    /// 
    /// Optionally, a specific file is used
    /// to monitor if the process is still working
    /// 
    /// If ParentProcessId is not 0,
    /// the program exists in case the corresponding process
    /// is not running any more
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected abstract void Run (CancellationToken cancellationToken);
    
    /// <summary>
    /// <see cref="Lemoine.Threading.IThreadClass" />
    /// </summary>
    /// <returns></returns>
    public bool IsCheckInPause ()
    {
      // It is not possible to pause the check of a process
      return false;
    }

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    public abstract ILog GetLogger ();
    #endregion // Methods
  }
}
