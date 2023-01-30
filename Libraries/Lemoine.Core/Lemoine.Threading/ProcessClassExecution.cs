// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Helper class to run a ProcessClass
  /// </summary>
  public abstract class ProcessClassExecution
  {
    static readonly TimeSpan DEFAULT_SLEEP_BEFORE_RESTART = TimeSpan.FromSeconds (2);
    
    #region Members
    string m_programName;
    IProcessClass m_processClass;
    TimeSpan m_sleepBeforeRestart = DEFAULT_SLEEP_BEFORE_RESTART;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProcessClassExecution).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated program name, without .exe
    /// </summary>
    public string ProgramName {
      get { return m_programName; }
    }
    
    /// <summary>
    /// Associated Process class
    /// </summary>
    public IProcessClass ProcessClass {
      get { return m_processClass; }
    }
    
    /// <summary>
    /// Time period to sleep before restarting a new process
    /// </summary>
    public TimeSpan SleepBeforeRestart {
      get
      {
        if (ProcessClass.SleepBeforeRestart.HasValue) {
          return ProcessClass.SleepBeforeRestart.Value;
        }
        else {
          return DEFAULT_SLEEP_BEFORE_RESTART;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="programName">Without .exe</param>
    /// <param name="processClass"></param>
    public ProcessClassExecution (string programName, IProcessClass processClass)
    {
      m_programName = programName;
      m_processClass = processClass;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the associated process name
    /// 
    /// Default is ProgramName + Suffix
    /// </summary>
    /// <returns></returns>
    public virtual string GetProcessName ()
    {
      return m_programName + this.GetSuffix ();
    }
    
    /// <summary>
    /// Get a suffix for the process and stamp names
    /// 
    /// Default is "-" + GetId()
    /// </summary>
    /// <returns></returns>
    public virtual string GetSuffix ()
    {
      return "-" + GetId ();
    }
    
    /// <summary>
    /// Reference ID that may be used for the suffix
    /// </summary>
    /// <returns></returns>
    public abstract int GetId ();
    
    /// <summary>
    /// Arguments that are specific to the generic ProcessClass:
    /// <item>Parent Process ID</item>
    /// <item>Stamp</item>
    /// 
    /// Default is "-s true -p ParentProcessId"
    /// </summary>
    /// <returns></returns>
    public virtual string GetProcessClassArguments ()
    {
      string processClassArguments = $"-s true -p {m_processClass.ParentProcessId}";
      if (m_processClass.Timeout.HasValue) {
        processClassArguments += $" -t {m_processClass.Timeout.Value}";
      }
      return processClassArguments;
    }
    
    /// <summary>
    /// Specific arguments
    /// 
    /// Default: none
    /// </summary>
    /// <returns></returns>
    public abstract string GetSpecificArguments ();
    
    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    public virtual ILog GetLogger ()
    {
      return LogManager.GetLogger(string.Format ("{0}.{1}",
                                                 typeof (ProcessClassExecution).FullName,
                                                 GetId ()));
    }

    /// <summary>
    /// Start the process related to the given ProcessClass
    /// 
    /// This method may raise an exception
    /// </summary>
    public void Start ()
    {
      // - Kill first any existing process
      string processName = this.GetProcessName ();
      Process[] processes = Process.GetProcessesByName (processName);
      foreach (Process process in processes) {
        if (!KillProcess (process.Id, log)) {
          log.ErrorFormat ("Start: " +
                           "Previous process {0} could not be stopped",
                           processName);
          return;
        }
        System.Threading.Thread.Sleep (this.SleepBeforeRestart);
      }
      
      // - Copy the executable
      string programDirectory = Directory.GetParent (ProgramInfo.AbsolutePath).FullName;
      Directory.SetCurrentDirectory (programDirectory);
      string currentDirectory = Directory.GetCurrentDirectory ();
      string srcConsoleProgramFileName  = string.Format ("{0}.exe",
                                                         this.ProgramName);
      string consoleProgramFileName = string.Format ("{0}.exe",
                                                     GetProcessName ());
      string srcLog4netConfig = string.Format ("{0}.exe.log4net",
                                               this.ProgramName);
      string log4netConfig = string.Format ("{0}.exe.log4net",
                                            GetProcessName ());
      string srcConfig = string.Format ("{0}.exe.config",
                                        this.ProgramName);
      string config = string.Format ("{0}.exe.config",
                                     GetProcessName ());
      string srcOptions = string.Format ("{0}.exe.options",
                                         this.ProgramName);
      string options = string.Format ("{0}.exe.options",
                                      GetProcessName ());
      try { // Program
        log.DebugFormat ("Start: " +
                         "copy {0} into {1}",
                         srcConsoleProgramFileName,
                         consoleProgramFileName);
        File.Copy (srcConsoleProgramFileName,
                   consoleProgramFileName,
                   true);
      }
      catch (IOException ex) {
        log.Error ($"Start: could not copy {srcConsoleProgramFileName} to {consoleProgramFileName} directory={currentDirectory} because the file is in use", ex);
        throw ex;
      }
      catch (Exception ex) {
        log.Fatal ($"Start: could not copy {srcConsoleProgramFileName} to {consoleProgramFileName} directory={currentDirectory}", ex);
        throw ex;
      }
      
      // - .exe.config
      if (!File.Exists (srcConfig)) {
        log.WarnFormat ("Start: " +
                        "config file {0} does not exist",
                        srcConfig);
      }
      else { // ProgramName.exe.config exists
        try { // Program.config
          if (!File.Exists (config)
              || File.GetLastWriteTimeUtc (config) < File.GetLastWriteTimeUtc (srcConfig)) {
            log.DebugFormat ("Start: " +
                             "copy {0} into {1}",
                             srcConfig,
                             config);
            File.Copy (srcConfig,
                       config,
                       true);
          }
        }
        catch (IOException ex) {
          log.Warn ($"Start: could not copy {srcConfig} to {config} directory={currentDirectory} because the file is in use", ex);
        }
        catch (Exception ex) {
          log.Warn ($"Start: could not copy {srcConfig} to {config} directory={currentDirectory}", ex);
        }
      }
      
      // .exe.options
      if (!File.Exists (srcOptions)) {
        log.WarnFormat ("Start: " +
                        "options file {0} does not exist",
                        srcOptions);
      }
      else { // ProgramName.exe.options exists
        try { // Program.options
          if (!File.Exists (options)
              || File.GetLastWriteTimeUtc (options) < File.GetLastWriteTimeUtc (srcOptions)) {
            log.DebugFormat ("Start: " +
                             "copy {0} into {1}",
                             srcOptions,
                             options);
            File.Copy (srcOptions,
                       options,
                       true);
          }
        }
        catch (IOException ex) {
          log.Warn ($"Start: could not copy {srcOptions} to {options} directory={currentDirectory} because the file is in use", ex);
        }
        catch (Exception ex) {
          log.Warn ($"Start: could not copy {srcOptions} to {options} directory={currentDirectory}", ex);
        }
      }
      
      // .exe.log4net
      if (!File.Exists (srcLog4netConfig)) {
        log.WarnFormat ("Start: " +
                        "log4net config file {0} does not exist",
                        srcLog4netConfig);
      }
      else { // ProgramName.exe.log4net exists
        try { // Program.log4net
          if (!File.Exists (log4netConfig)
              || File.GetLastWriteTimeUtc (log4netConfig) < File.GetLastWriteTimeUtc (srcLog4netConfig)) {
            log.DebugFormat ("Start: " +
                             "copy {0} into {1}",
                             srcLog4netConfig,
                             log4netConfig);
            File.Delete (log4netConfig);
            using (StreamReader read = File.OpenText (srcLog4netConfig))
              using (StreamWriter write = File.CreateText (log4netConfig))
            {
              while (!read.EndOfStream) {
                string line = read.ReadLine ();
                var modifiedLine = line
                  .Replace (ProgramName, GetProcessName ())
                  .Replace ("%property{ApplicationName}", "%property{ApplicationName}" + this.GetSuffix ());
                write.WriteLine (modifiedLine);
              }
            }
          }
        }
        catch (IOException ex) {
          log.Warn ($"Start: could not copy {srcLog4netConfig} to {log4netConfig} directory={currentDirectory} because the file is in use", ex);
        }
        catch (Exception ex) {
          log.Warn ($"Start: could not copy {srcLog4netConfig} to {log4netConfig} directory={currentDirectory} ", ex);
        }
      }
      
      // - Remove all the stamp file if it exists
      var stampFilePath = m_processClass.GetStampFilePath ();
      if (File.Exists (stampFilePath)) {
        File.Delete (stampFilePath);
      }
      
      // - Run a new process with the right parameters
      ProcessStartInfo startInfo = new ProcessStartInfo ();
      startInfo.FileName = consoleProgramFileName;
      startInfo.Arguments = string.Format ("{0} {1}",
                                           this.GetProcessClassArguments (),
                                           this.GetSpecificArguments ());
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardError = false;
      startInfo.RedirectStandardOutput = false;
      log.InfoFormat ("Start: " +
                      "run: {0} {1}",
                      consoleProgramFileName, startInfo.Arguments);
      Process.Start (startInfo);
    }
    
    /// <summary>
    /// Stop the process
    /// </summary>
    public void Abort ()
    {
      Process[] processes = Process.GetProcessesByName (GetProcessName ());
      foreach (Process process in processes) {
        log.DebugFormat ("Abort: " +
                         "kill process with PID={0} and name={1}",
                         process.Id, process.ProcessName);
        KillProcess (process.Id, log);
      }
    }
    
    /// <summary>
    /// Try to kill if needed a process
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="logger"></param>
    /// <returns>the process was successfully killed</returns>
    internal protected static bool KillProcess (int pid, ILog logger)
    {
      Process process = null;
      try {
        process = Process.GetProcessById (pid);
      }
      catch (ArgumentException ex) {
        log.Info ($"KillProcess: process with pid={pid} could not be retrieved => consider the process is already stopped.", ex);
      }
      catch (Exception ex) {
        log.Error ($"KillProcess: unexpected exception while trying to retrieve process with pid={pid}", ex);
        throw;
      }
      Debug.Assert (null != process);
      
      // 1st attempt: try to stop first properly the process
      try {
        logger.Info ($"KillProcess: Stop the process {pid}");
        process.EnableRaisingEvents = true;
        if (false == process.CloseMainWindow ()) {
          logger.Debug ("KillProcess: the close message could not be sent");
        }
        process.Close ();
      }
      catch (Exception ex) {
        logger.Error ("KillProcess: CloseMainWindow or Close returned exception", ex);
      }
      
      // Check if process is still valid, else reset it
      try {
        bool test = process.HasExited;
      }
      catch (InvalidOperationException) {
        logger.InfoFormat ("KillProcess: " +
                           "reset process with pid={0}",
                           pid);
        try {
          process = Process.GetProcessById (pid);
        }
        catch (ArgumentException ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"KillProcess: process with pid={pid} could not be retrieved => consider the process is already stopped", ex);
          }
        }
        catch (Exception ex) {
          log.Error ($"KillProcess: unexpected exception while trying to retrieve process with pid={pid}", ex);
          throw;
        }
      }
      catch (Exception ex) {
        logger.Error ("KillProcess: other exception while checking if process is still valid", ex);
      }
      
      // Check if the process was stopped after maximum 5 seconds
      try {
        if (!process.WaitForExit (5000)) {
          logger.Info ("KillProcess: " +
                       "Process is still running " +
                       "after 5s !");
          // Kill the process
        }
        else {
          logger.Info ("KillProcess: Nice, the process does not exist any more");
          return true;
        }
      }
      catch (InvalidOperationException ex1) {
        if (logger.IsInfoEnabled) {
          logger.Info ("KillProcess: WaitForExit returned an exception probably because the Close message could not sent", ex1);
        }
      }
      catch (Exception ex1) {
        if (logger.IsInfoEnabled) {
          logger.Info ("KillProcess: WaitForExit returned an exception this probably means the process does not exist any more, nice", ex1);
        }
        return true;
      }
      
      // If not, kill the process
      try {
        logger.Debug ("Kill the process");
        process.Kill ();
        logger.Info ("The process was successfully killed");
        return true;
      }
      catch (System.ComponentModel.Win32Exception ex) {
        if (logger.IsInfoEnabled) {
          logger.Info ("The process could not be terminated or is terminating => wait 10 s, else give up", ex);
        }
        try {
          if (!process.WaitForExit (10000)) {
            logger.Error ("Process is still running after 10s!", ex);
            return false;
          }
          else {
            logger.Info ("Nice, the process does not exist any more");
            return true;
          }
        }
        catch (Exception ex1) {
          if (logger.IsInfoEnabled) {
            logger.Info ("WaitForExit returned this probably means the process does not exist any more, nice", ex1);
          }
          return true;
        }
      }
      catch (NotSupportedException ex) {
        logger.Fatal ("The process is a local process this exception should not happen", ex);
        Debug.Assert (false);
        return false;
      }
      catch (InvalidOperationException ex) {
        logger.Warn ("The process is invalid => it can't be killed", ex);
        return true;
      }
    }
    #endregion // Methods
  }
}
