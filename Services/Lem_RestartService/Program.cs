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
        log.Error ($"RestartService: the service {serviceName} could not be stopped", ex);
        System.Console.WriteLine (errorMessage);
      }
      
      // - Check the process is not here any more
      //   Else kill it
      int pid = 0;
      try {
        pid = GetServicePID (serviceName);
      }
      catch (Exception ex) {
        log.Error ($"RestartService: exception in GetServicePID", ex);
      }
      if (0 == pid) {
        log.Debug ($"RestartService: no process for {serviceName}");
      }
      else {
        if (!KillProcess (pid)) {
          log.Error ($"RestartService: process with pid={pid} could not be stopped");
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
        log.Error ($"RestartService: the service {serviceName} could not be started", ex);
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
        log.Warn ($"StopWatchingService: could not get the status of the service, it is probably not installed ! => give up (service not running)", ex);
        return false;
      }
      
      try {
        if ( (status != ServiceControllerStatus.Running)
            && (status != ServiceControllerStatus.StartPending)
            && (status != ServiceControllerStatus.ContinuePending)) {
          return false;
        }
        else { // The service was running
          log.Debug ($"StopWatchingService: the watching service was running");
          
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
            log.Error ($"StopWatchingService: exception in GetServicePID", ex);
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
        log.Warn ($"StopWatchingService: the watching service could not be stopped", ex);
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
        log.Error ($"StartWatchingService: the watching service is probably not installed, => give up", ex);
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
        log.Debug ($"KillProcess: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      
      // 1st attempt: try to stop first properly the process
      try {
        log.Info ("KillProcess: Stop the process");
        process.EnableRaisingEvents = true;
        if (false == process.CloseMainWindow ()) {
          log.Debug ("KillProcess: the close message could not be sent");
        }
        process.Close ();
      }
      catch (Exception ex) {
        log.Error ("KillProcess: CloseMainWindow or Close returned exception", ex);
      }
      
      // Check if the process was stopped after maximum 5 seconds
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        log.Debug ($"KillProcess: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      try {
        if (!process.WaitForExit (5000)) {
          log.Info ("Process is still running after 5s !");
          // Kill the process
        }
        else {
          log.Info ("KillProcess: Nice, the process does not exist any more");
          return true;
        }
      }
      catch (Exception ex1) {
        log.Info ($"KillProcess: WaitForExit returned an exception check if the process still exists", ex1);
        try {
          process = Process.GetProcessById (pid);
        }
        catch (Exception ex2) {
          log.Info ($"KillProcess: process pid={pid} does not exist any more, => return true", ex2);
          return true;
        }
      }
      
      // If not, kill the process
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        log.Debug ($"KillProcess: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      try {
        log.Debug ("KillProcess: Kill the process");
        process.Kill ();
        log.Info ("The process was successfully killed");
        return true;
      }
      catch (System.ComponentModel.Win32Exception ex) {
        log.Info ("KillProcess: The process could not be terminated or is terminating => wait 10 s, else give up", ex);
        try {
          if (!process.WaitForExit (10000)) {
            log.Error ($"KillProcess: Process is still running after 10s !", ex);
            return false;
          }
          else {
            log.Info ("KillProcess: Nice, the process does not exist any more");
            return true;
          }
        }
        catch (Exception ex1) {
          log.Info ("KillProcess: WaitForExit returned an exception check if the process still exists", ex1);
          try {
            process = Process.GetProcessById (process.Id);
          }
          catch (Exception ex2) {
            log.Info ($"KillProcess: process pid={process.Id} does not exist any more, => return true", ex2);
            return true;
          }
          log.Error ($"KillProcess: the process pid={process.Id} could not be killed, may be because the used privilege was not high enough", ex1);
          return false;
        }
      }
      catch (NotSupportedException ex) {
        log.Fatal ($"KillProcess: The process is a local process this exception should not happen", ex);
        Debug.Assert (false);
        return false;
      }
      catch (InvalidOperationException ex) {
        log.Warn ($"KillProcess: The process is invalid => it can't be killed", ex);
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
        log.Error ($"GetServicePID: the registry key of the service {serviceName} does not exist");
        throw new ArgumentException ("service does not exist");
      }
      else { // null != rk
        object serviceImagePath = registryKey.GetValue("ImagePath");
        if (null == serviceImagePath) {
          log.Error ($"GetServicePID: the registry key for service {serviceName} does not have the value ImagePath The registry key is {registryKey}");
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
          
          log.Debug ($"GetServicePID: got Path for Service {serviceName} = {path}");
          string exe = Path.GetFileNameWithoutExtension (path);
          System.Diagnostics.Process[] processes =
            System.Diagnostics.Process.GetProcessesByName(exe);
          if (processes.Length <= 0)  {
            log.Debug ($"GetServicePID: there is no process with the name {exe}, may be it is stopped => return 0");
            return 0;
          }
          else { // 0 < processes.Length
            if (1 < processes.Length) {
              log.WarnFormat ("GetServicePID: " +
                              "there are {0} processes for {1}",
                              processes.Length, exe);
            }
            log.DebugFormat ("GetServicePID: " +
                             "first PID is {0} for service {1}",
                             processes [0].Id, serviceName);
            return processes [0].Id;
          }
        }
      }
    }
    
  }
}
