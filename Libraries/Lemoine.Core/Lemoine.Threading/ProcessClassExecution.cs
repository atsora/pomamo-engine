// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Lemoine.Info;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Threading
{
  /// <summary>
  /// Helper class to run a ProcessClass
  /// </summary>
  public abstract class ProcessClassExecution
  {
    static readonly TimeSpan DEFAULT_SLEEP_BEFORE_RESTART = TimeSpan.FromSeconds (2);

    readonly string m_programName;
    readonly IProcessClass m_processClass;

    static readonly ILog log = LogManager.GetLogger (typeof (ProcessClassExecution).FullName);

    /// <summary>
    /// Associated program name, without .exe
    /// </summary>
    public string ProgramName => m_programName;

    /// <summary>
    /// Associated Process class
    /// </summary>
    public IProcessClass ProcessClass => m_processClass;

    /// <summary>
    /// Time period to sleep before restarting a new process
    /// </summary>
    public TimeSpan SleepBeforeRestart
    {
      get {
        if (ProcessClass.SleepBeforeRestart.HasValue) {
          return ProcessClass.SleepBeforeRestart.Value;
        }
        else {
          return DEFAULT_SLEEP_BEFORE_RESTART;
        }
      }
    }

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
    public virtual ILog GetLogger () => LogManager.GetLogger ($"{typeof (ProcessClassExecution).FullName}.{GetId ()}");

    /// <summary>
    /// Start the process related to the given ProcessClass
    /// 
    /// This method may raise an exception
    /// </summary>
    public void Start ()
    {
      // - Kill first any existing process
      var processName = this.GetProcessName ();
      var processes = Process.GetProcessesByName (processName);
      foreach (var process in processes) {
        if (!KillProcess (process.Id, log)) {
          log.Error ($"Start: previous process {processName} could not be stopped");
          return;
        }
        System.Threading.Thread.Sleep (this.SleepBeforeRestart);
      }
      var dotNetProcesses = Process.GetProcessesByName ("dotnet");
      foreach (var process in dotNetProcesses) {
        bool isMatch = false;
        try {
          var arguments = process.StartInfo.Arguments;
          isMatch = !string.IsNullOrEmpty (arguments)
            && (arguments.IndexOf (processName, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        catch (Exception ex) {
          log.Debug ($"Start: unable to read dotnet process arguments for pid={process.Id}", ex);
        }

        if (!isMatch) {
          continue;
        }

        if (!KillProcess (process.Id, log)) {
          log.Error ($"Start: previous process {processName} could not be stopped");
          return;
        }
        Thread.Sleep (this.SleepBeforeRestart);
      }

      // - Copy the executable
      var programDirectory = Directory.GetParent (ProgramInfo.AbsolutePath).FullName;
      Directory.SetCurrentDirectory (programDirectory);
      var currentDirectory = Directory.GetCurrentDirectory ();
      var suffixes = new string[] { ".exe", ".exe.log4net", ".exe.config", ".exe.options",
        ".dll", ".dll.config", ".dll.defaultoptions", ".dll.options", ".dll.log4net",
        ".nh.cfg.xml", ".runtimeconfig.json", ".deps.json" };

      if (log.IsFatalEnabled) {
        if (!File.Exists ($"{this.ProgramName}.exe") && !File.Exists ($"{this.ProgramName}.dll")) {
          log.Fatal ($"Start: neither {this.ProgramName}.exe nor {this.ProgramName}.dll exist");
        }
      }

      foreach (var suffix in suffixes) {
        var src = $"{this.ProgramName}{suffix}";
        var dest = $"{processName}{suffix}";
        if (!File.Exists (src)) {
          log.Info ($"Start: file {src} does not exist");
        }
        else { // src exists
          try { // Program.config
            if (!File.Exists (dest)
                || File.GetLastWriteTimeUtc (dest) < File.GetLastWriteTimeUtc (src)) {
              if (suffix.EndsWith ("log4net")) {
                log.Debug ($"Start: copy/adapt {src} into {dest}");
                File.Delete (dest);
                using (StreamReader read = File.OpenText (src))
                using (StreamWriter write = File.CreateText (dest)) {
                  while (!read.EndOfStream) {
                    string line = read.ReadLine ();
                    var modifiedLine = line
                      .Replace (ProgramName, GetProcessName ())
                      .Replace ("%property{ApplicationName}", "%property{ApplicationName}" + this.GetSuffix ());
                    write.WriteLine (modifiedLine);
                  }
                }
              }
              else {
                log.Debug ($"Start: copy {src} into {dest}");
                File.Copy (src,
                           dest,
                           true);
              }
            }
            else if (log.IsDebugEnabled) {
              log.Debug ($"Start: {src} and {dest} have already the same write time");
            }
          }
          catch (IOException ex) {
            log.Error ($"Start: could not copy {src} to {dest} directory={currentDirectory} because the file is in use", ex);
          }
          catch (Exception ex) {
            log.Error ($"Start: could not copy {src} to {dest} directory={currentDirectory}", ex);
          }
        }
      }

      // - Remove all the stamp file if it exists
      var stampFilePath = m_processClass.GetStampFilePath ();
      if (File.Exists (stampFilePath)) {
        File.Delete (stampFilePath);
      }

      // - Run a new process with the right parameters
      var startInfo = new ProcessStartInfo ();
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardError = false;
      startInfo.RedirectStandardOutput = false;
      if (File.Exists ($"{processName}.dll")) {
        startInfo.FileName = "dotnet";
        startInfo.Arguments = $"{processName}.dll {this.GetProcessClassArguments ()} {this.GetSpecificArguments ()}";
        log.Info ($"Start: run: dotnet {startInfo.Arguments}");
      }
      else {
        startInfo.FileName = $"{processName}.exe";
        startInfo.Arguments = $"{this.GetProcessClassArguments ()} {this.GetSpecificArguments ()}";
        log.Info ($"Start: run: {processName}.exe {startInfo.Arguments}");
      }
      Process.Start (startInfo);
    }

    /// <summary>
    /// Stop the process
    /// </summary>
    public void Abort ()
    {
      Process[] processes = Process.GetProcessesByName (GetProcessName ());
      foreach (Process process in processes) {
        log.Debug ($"Abort: kill process with PID={process.Id} and name={process.ProcessName}");
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

      if (process == null) {
        return true;
      }

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
        logger.Info ($"KillProcess: reset process with pid={pid}");
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
  }
}
