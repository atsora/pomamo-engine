// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Description of ProcessOrThreadClass.
  /// </summary>
  public abstract class ProcessOrThreadClass: ThreadClass, IProcessClass, IThreadClass
  {
    bool m_useStampFile = false;
    int m_parentProcessId = 0;

    static readonly ILog log = LogManager.GetLogger(typeof (ProcessOrThreadClass).FullName);

    #region Getters / Setters
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
      var stampFileName = GetStampFileName ();
      Debug.Assert (!string.IsNullOrEmpty (stampFileName));
      var stampFileDirectory = Lemoine.Info.PulseInfo.CommonConfigurationDirectory;
      if (log.IsErrorEnabled && !Directory.Exists (stampFileDirectory)) {
        log.Error ($"GetStampFilePath: {stampFileDirectory} does not exist (it should)");
      }
      return Path.Combine (stampFileDirectory, stampFileName);
    }

    /// <summary>
    /// Set the thread as active:
    /// <item>(update LastExecution to now)</item>
    /// <item>If the parent process ID is defined and the parent process
    /// does not exist any more, exit</item>
    /// </summary>
    public override void SetActive ()
    {
      if (this.CancellationToken.IsCancellationRequested) {
        GetLogger ().Warn ("SetActive: cancellation was requested, throw OperationCancelledException");
        CancellationToken.ThrowIfCancellationRequested ();
      }

      base.UpdateLastExecution ();
      
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
          && (this.StartDateTime.Add (this.Timeout.Value) < DateTime.UtcNow)) {
        GetLogger ().Warn ($"SetActive: timeout detected after {DateTime.UtcNow.Subtract (this.StartDateTime)} when timeout is {this.Timeout.Value} => abort the thread or the process");
        if (0 != m_parentProcessId) {
          GetLogger ().Warn ("SetActive: exit because of timeout");
          Lemoine.Core.Environment.ForceExit ();
        }
        try {
          System.Threading.Thread.CurrentThread.Abort ();
        }
        catch (PlatformNotSupportedException) {
          GetLogger ().Info ("SetActive: CurrentThread.Abort () not supported on this platform");
        }
        throw new AbortException ("Timeout was reached in SetActive.");
      }
    }
    #endregion // Methods
  }
}
