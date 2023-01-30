// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Microsoft.Win32;

namespace Lem_RestartService
{
  [SupportedOSPlatform ("windows")]
  class Program
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Program).FullName);
    
    static readonly TimeSpan STOP_TIMEOUT = TimeSpan.FromSeconds (30);

    /// <summary>
    /// Program entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      try {
        var result = CommandLine.Parser.Default.ParseArguments<Options> (args);

        result.WithNotParsed (errors =>
        {
          Environment.ExitCode = 1;
          return;
        });

        result.WithParsed (opt =>
        {
          var parameters = opt.Parameters;
          if (null != parameters) {
            Lemoine.Info.ConfigSet.AddCommandLineParameters (parameters);
          }
          LogManager.AddLog4net ();

          bool restartWatchingService = StopWatchingService ();
          foreach (string serviceName in opt.ServiceNames) {
            log.InfoFormat ("Main: " +
                            "about to restart {0}",
                            serviceName);
            RestartService (serviceName);
          }
          if (restartWatchingService) {
            StartWatchingService ();
          }
        });
      }
      catch (Exception ex) {
        log.Error ("Main: options parsing  exception", ex);
        System.Console.Error.WriteLine ($"Options parsing exception {ex} raised");
        Environment.ExitCode = 1;
      }
    }
    
    static void RestartService (string serviceName)
    {
      // - Stop the service
      ServiceController sc = new ServiceController  (serviceName);
      try {
        if (sc.Status != ServiceControllerStatus.Stopped) {
          sc.Stop();
          DateTime stopTime = DateTime.UtcNow;
          while (sc.Status != ServiceControllerStatus.Stopped) {
            System.Threading.Thread.Sleep(600);
            sc.Refresh();
            if (STOP_TIMEOUT < DateTime.UtcNow.Subtract (stopTime)) {
              log.WarnFormat ("RestartService: " +
                              "the service could not be stopped before after {0} " +
                              "=> give up, and try to kill it instead",
                              STOP_TIMEOUT);
              break;
            }
          }
        }
      } catch (Exception ex) {
        string errorMessage = "The service " + serviceName + " could not be stopped. " +
          "Did you run the program with the Administrator privilege ?";
        log.ErrorFormat ("RestartService: " +
                         "the service could not be stopped because of {0}, " +
                         "error message={1}",
                         ex, errorMessage);
        System.Console.WriteLine (errorMessage);
      }
      
      // - Check the process is not here any more
      //   Else kill it
      int pid = 0;
      try {
        pid = GetServicePID (serviceName);
      }
      catch (Exception ex) {
        log.ErrorFormat ("RestartService: " +
                         "GetServicePID raised {0}",
                         ex);
      }
      if (0 == pid) {
        log.DebugFormat ("RestartService: " +
                         "no process for {0}",
                         serviceName);
      }
      else {
        if (!KillProcess (pid)) {
          log.ErrorFormat ("RestartService: " +
                           "process with pid={0} could not be stopped",
                           pid);
        }
        Thread.Sleep (TimeSpan.FromSeconds (1));
      }
      
      // - The start again the service
      try {
        if (sc.Status != ServiceControllerStatus.Running) {
          sc.Start();
          while (sc.Status != ServiceControllerStatus.Running) {
            System.Threading.Thread.Sleep(600);
            sc.Refresh();
          }
        }
      } catch (Exception ex) {
        string errorMessage = "The service " + serviceName + " could not be started. " +
          "Did you run the program with the Administrator privilege ?";
        log.ErrorFormat ("RestartService: " +
                         "the service could not be started because of {0}, " +
                         "error message={1}",
                         ex, errorMessage);
        System.Console.WriteLine (errorMessage);
      }
    }
    
    /// <summary>
    /// Stop the watching service and returns true if it was running,
    /// else return false
    /// </summary>
    /// <returns></returns>
    static bool StopWatchingService ()
    {
      ServiceController controlSC = new ServiceController ("Lem_Control");
      
      ServiceControllerStatus status;
      try {
        status = controlSC.Status;
      }
      catch (Exception ex) {
        log.WarnFormat ("StopWatchingService: " +
                        "could not get the status of the service, " +
                        "it is probably not installed ! " +
                        "=> give up (service not running) " +
                        "{0}",
                        ex);
        return false;
      }
      
      try {
        if ( (status != ServiceControllerStatus.Running)
            && (status != ServiceControllerStatus.StartPending)
            && (status != ServiceControllerStatus.ContinuePending)) {
          return false;
        }
        else { // The service was running
          log.DebugFormat ("StopWatchingService: " +
                           "the watching service was running");
          
          // - Stop the service
          controlSC.Stop();
          while (controlSC.Status != ServiceControllerStatus.Stopped) {
            System.Threading.Thread.Sleep(600);
            controlSC.Refresh();
          }

          // - Kill the process if needed
          int pid = 0;
          try {
            pid = GetServicePID (controlSC.ServiceName);
          }
          catch (Exception ex) {
            log.ErrorFormat ("StopWatchingService: " +
                             "GetServicePID raised {0}",
                             ex);
          }
          if (0 == pid) {
            log.DebugFormat ("StopWatchingService: " +
                             "no process for {0}",
                             controlSC.ServiceName);
          }
          else {
            if (!KillProcess (pid)) {
              log.ErrorFormat ("StopWatchingService: " +
                               "Process {0} could not be stopped",
                               pid);
            }
            Thread.Sleep (TimeSpan.FromSeconds (1));
          }
        }
      }
      catch (Exception ex) {
        log.WarnFormat ("StopWatchingService: " +
                        "the watching service could not be stopped because of " +
                        "{0}",
                        ex);
      }
      
      return true;
    }
    
    static void StartWatchingService ()
    {
      ServiceController controlSC = new ServiceController ("Lem_Control");
      try {
        if (controlSC.Status != ServiceControllerStatus.Stopped) {
          log.ErrorFormat ("StartWatchingService: " +
                           "the status {0} is not stopped, " +
                           "=> do not start the watching service",
                           controlSC.Status);
          return;
        }
        else { // Status == Stopped
          controlSC.Start();
          while (controlSC.Status != ServiceControllerStatus.Running) {
            System.Threading.Thread.Sleep(600);
            controlSC.Refresh();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("StartWatchingService: " +
                         "the watching service is probably not installed, " +
                         "=> give up " +
                         "{0}",
                         ex);
        return;
      }
    }

    /// <summary>
    /// Try to kill if needed a process
    /// </summary>
    /// <param name="pid"></param>
    /// <returns>the process was successfully killed</returns>
    static bool KillProcess (int pid)
    {
      Process process;
      
      try {
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        log.DebugFormat ("KillProcess: " +
                         "process with pid {0} can't be found " +
                         "=> it probably does not exist any more, return true " +
                         "{0}",
                         ex);
        return true;
      }
      
      // 1st attempt: try to stop first properly the process
      try {
        log.Info ("KillProcess: " +
                  "Stop the process");
        process.EnableRaisingEvents = true;
        if (false == process.CloseMainWindow ()) {
          log.Debug ("KillProcess: " +
                     "the close message could not be sent");
        }
        process.Close ();
      }
      catch (Exception ex) {
        log.ErrorFormat ("KillProcess: " +
                         "CloseMainWindow or Close returned exception {0}",
                         ex);
      }
      
      // Check if the process was stopped after maximum 5 seconds
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        log.DebugFormat ("KillProcess: " +
                         "process with pid {0} can't be found " +
                         "=> it probably does not exist any more, return true " +
                         "{0}",
                         ex);
        return true;
      }
      try {
        if (!process.WaitForExit (5000)) {
          log.Info ("Process is still running " +
                    "after 5s !");
          // Kill the process
        }
        else {
          log.InfoFormat ("KillProcess: " +
                          "Nice, the process " +
                          "does not exist any more");
          return true;
        }
      }
      catch (Exception ex1) {
        log.InfoFormat ("KillProcess: " +
                        "WaitForExit returned {0} " +
                        "check if the process still exists",
                        ex1);
        try {
          process = Process.GetProcessById (pid);
        }
        catch (Exception ex2) {
          log.InfoFormat ("KillProcess: " +
                          "process pid={0} does not exist any more, " +
                          "=> return true " +
                          "{1}",
                          pid,
                          ex2);
          return true;
        }
      }
      
      // If not, kill the process
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        log.DebugFormat ("KillProcess: " +
                         "process with pid {0} can't be found " +
                         "=> it probably does not exist any more, return true " +
                         "{0}",
                         ex);
        return true;
      }
      try {
        log.Debug ("KillProcess: " +
                   "Kill the process");
        process.Kill ();
        log.Info ("The process was successfully killed");
        return true;
      }
      catch (System.ComponentModel.Win32Exception ex) {
        log.InfoFormat ("KillProcess: " +
                        "The process could not be terminated " +
                        "or is terminating " +
                        "=> wait 10 s, else give up " +
                        "{0}",
                        ex);
        try {
          if (!process.WaitForExit (10000)) {
            log.ErrorFormat ("KillProcess: " +
                             "Process is still running " +
                             "after 10s ! " +
                             "{0}",
                             ex);
            return false;
          }
          else {
            log.InfoFormat ("KillProcess: " +
                            "Nice, the process " +
                            "does not exist any more");
            return true;
          }
        }
        catch (Exception ex1) {
          log.InfoFormat ("KillProcess: " +
                          "WaitForExit returned {0} " +
                          "check if the process still exists",
                          ex1);
          try {
            process = Process.GetProcessById (process.Id);
          }
          catch (Exception) {
            log.InfoFormat ("KillProcess: " +
                            "process pid={0} does not exist any more, " +
                            "=> return true",
                            process.Id);
            return true;
          }
          log.ErrorFormat ("KillProcess: " +
                           "the process pid={0} could not be killed, " +
                           "may be because the used privilege was not high enough",
                           process.Id);
          return false;
        }
      }
      catch (NotSupportedException ex) {
        log.FatalFormat ("KillProcess: " +
                         "The process is a local process " +
                         "this exception should not happen " +
                         "{0}",
                         ex);
        Debug.Assert (false);
        return false;
      }
      catch (InvalidOperationException ex) {
        log.WarnFormat ("KillProcess: " +
                        "The process is invalid " +
                        "=> it can't be killed " +
                        "{0}",
                        ex);
        return true;
      }
    }
    
    /// <summary>
    /// Get the first pid that may correspond to the service name
    /// 
    /// If there is no such process, 0 is returned
    /// 
    /// An exception may be also thrown
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    static int GetServicePID (string serviceName)
    {
      RegistryKey registryKey = Registry.LocalMachine
        .OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\"+serviceName, false);
      if (null == registryKey) {
        log.ErrorFormat ("GetServicePID: " +
                         "the registry key of the service {0} does not exist",
                         serviceName);
        throw new ArgumentException ("service does not exist");
      }
      else { // null != rk
        object serviceImagePath = registryKey.GetValue("ImagePath");
        if (null == serviceImagePath) {
          log.ErrorFormat ("GetServicePID: " +
                           "the registry key for service {0} does not have the value ImagePath " +
                           "The registry key is {1}",
                           serviceName,
                           registryKey);
          throw new ApplicationException ("No ImagePath");
        }
        else { // null != serviceImagePath
          // fetch path of executable associated to service
          string serviceImagePathAsString = serviceImagePath.ToString().Trim(' ');
          string path;
          // if path start with quotation marks, fetch what's inside as executable path
          if (serviceImagePathAsString[0] == '"') {
            string[] pathSplitOnQuotationMark = serviceImagePathAsString.Split('"');
            path = pathSplitOnQuotationMark[1];
          } else {
            // otherwise fetch until first space
            string[] pathAndParameters = serviceImagePathAsString.Split(' ');
            path = pathAndParameters[0];
          }
          
          log.DebugFormat ("GetServicePID: " +
                           "Got Path for Service {0} = {1}",
                           serviceName, path);
          string exe = Path.GetFileNameWithoutExtension (path);
          System.Diagnostics.Process[] processes =
            System.Diagnostics.Process.GetProcessesByName(exe);
          if (processes.Length <= 0)  {
            log.DebugFormat ("GetServicePID: " +
                             "there is no process with the name {0}, " +
                             "may be it is stopped " +
                             "=> return 0",
                             exe);
            return 0;
          }
          else { // 0 < processes.Length
            if (1 < processes.Length) {
              log.WarnFormat ("GetServicePID: " +
                              "there are {0} processes for {1}",
                              processes.Length, exe);
            }
            log.DebugFormat ("GetServicePID: " +
                             "first PID is {0} for servie {1}",
                             processes [0].Id, serviceName);
            return processes [0].Id;
          }
        }
      }
    }
    
  }
}
