// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Info;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Org.BouncyCastle.Asn1.BC;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.ServiceTools
{
  /// <summary>
  /// Service start-up type
  /// </summary>
  public enum StartupType
  {
    /// <summary>
    /// Automatic
    /// </summary>
    Auto,
    /// <summary>
    /// Manual
    /// </summary>
    Manual,
    /// <summary>
    /// Disabled
    /// </summary>
    Disabled,
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown,
  }

  /// <summary>
  /// Extended ServiceController class that allows to get / set:
  /// - the description
  /// - the startup type
  /// </summary>
  [SupportedOSPlatform ("windows")]
  public class WindowsServiceController : IServiceController
  {
    static readonly string DEPENDENCIES_LOCAL_KEY = "WindowsServiceController.Dependencies.Local";
    static readonly bool DEPENDENCIES_LOCAL_DEFAULT = true;

    static readonly string DEPENDENCIES_REMOTE_KEY = "WindowsServiceController.Dependencies.Remote";
    static readonly bool DEPENDENCIES_REMOTE_DEFAULT = false;

    static readonly string REMOTE_DESCRIPTION_KEY = "WindowsServiceController.Description.Remote";
    static readonly bool REMOTE_DESCRIPTION_DEFAULT = true;

    static readonly string PING_FIRST_KEY = "WindowsServiceController.PingFirst";
    static readonly bool PING_FIRST_DEFAULT = true;

    static readonly string PING_TIMEOUT_KEY = "WindowsServiceController.Ping.Timeout";
    static readonly int PING_TIMEOUT_DEFAULT = 50;

    static readonly TimeSpan STOP_TIMEOUT = TimeSpan.FromSeconds (20);

    static readonly TimeSpan CHECK_DELAY = TimeSpan.FromMilliseconds (150);

    ILog log = LogManager.GetLogger (typeof (WindowsServiceController).FullName);
    static ILog slog = LogManager.GetLogger (typeof (WindowsServiceController).FullName);

    static IDictionary<string, bool> s_machineLocal = new ConcurrentDictionary<string, bool> ();

    readonly bool m_supportDependencies;
    readonly IList<string> m_dependencies = new List<string> ();
    readonly IList<string> m_dependson = new List<string> ();
    bool m_local = false;
    readonly string m_serviceName;
    readonly string? m_machineName;
    readonly ServiceController m_serviceController;

    /// <summary>
    /// Associated machine name
    /// 
    /// <see cref="ServiceController.MachineName"/>
    /// </summary>
    public string MachineName => m_serviceController.MachineName;

    /// <summary>
    /// Associated service name
    /// 
    /// <see cref="ServiceController.ServiceName"/>
    /// </summary>
    public string ServiceName => m_serviceName;

    /// <summary>
    /// Associated Wmi Path
    /// </summary>
    public string WmiPath => $"\\\\{this.MachineName}\\root\\cimv2:Win32_Service.Name='{this.ServiceName}'";

    /// <summary>
    /// Base constructor for a local service
    /// </summary>
    /// <param name="name">Name of the service</param>
    public WindowsServiceController (string name, bool supportDependencies = false)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"WindowsServiceController: name={name} dependencies={supportDependencies}");
      }
      m_serviceName = name;
      m_machineName = null;
      m_supportDependencies = supportDependencies;
      m_local = true;
      m_serviceController = new ServiceController (name);
      if (supportDependencies) {
        InitializeDependencies ();
      }
    }

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="name">Name of the service</param>
    /// <param name="machineName">Name of the machine</param>
    public WindowsServiceController (string name, string machineName, bool supportDependencies = false)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"WindowsServiceController: name={name} on {machineName} dependencies={supportDependencies}");
      }
      m_serviceName = name;
      m_machineName = machineName;
      m_supportDependencies = supportDependencies;
      m_local = false;
      if (machineName.Equals (".")) {
        m_local = true;
      }
      else {
        bool? localCache = (bool?)s_machineLocal[machineName];
        if (localCache is null) {
          m_local = ComputerInfo.IsLocal (machineName);
          s_machineLocal[machineName] = m_local;
        }
        else {
          m_local = localCache.Value;
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"WindowsServiceController: service {name} on {machineName} => local={m_local}");
      }

      m_serviceController = new System.ServiceProcess.ServiceController (name, machineName);
      if (supportDependencies) {
        InitializeDependencies ();
      }
    }

    void InitializeDependencies ()
    {
      m_dependencies.Clear ();
      m_dependson.Clear ();

      bool manageDependencies;
      if (m_local) {
        manageDependencies = Lemoine.Info.ConfigSet
        .LoadAndGet (DEPENDENCIES_LOCAL_KEY, DEPENDENCIES_LOCAL_DEFAULT);
      }
      else {
        manageDependencies = Lemoine.Info.ConfigSet
        .LoadAndGet (DEPENDENCIES_REMOTE_KEY, DEPENDENCIES_REMOTE_DEFAULT);
      }
      if (manageDependencies) {
        if (m_serviceController is not null && RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
          try {
            foreach (var sc in m_serviceController.DependentServices) {
              m_dependencies.Add (sc.DisplayName);
            }

            foreach (var sc in m_serviceController.ServicesDependedOn) {
              m_dependson.Add (sc.DisplayName);
            }
          }
          catch (Exception ex) {
            //Will throw if the service is not installed
            log.Error ($"InitializeDependencies: exception for service {this.ServiceName}", ex);
          }
        }
      }
    }

    /// <summary>
    /// List of services dependent on this service.
    /// </summary>
    public IList<string> DependentServicesNames
    {
      get {
        if (m_supportDependencies) {
          return m_dependencies;
        }
        else {
          throw new NotSupportedException ();
        }
      }
    }

    /// <summary>
    /// List of services this service depends on.
    /// </summary>
    public IList<string> DependsOnServicesNames
    {
      get {
        if (m_supportDependencies) {
          return m_dependson;
        }
        else {
          throw new NotSupportedException ();
        }
      }
    }
    
    /// <summary>
    /// Description of the service
    /// </summary>
    public string Description => Task.Run (GetDescriptionAsync).Result;

    /// <summary>
    /// Description of the service
    /// </summary>
    public async Task<string> GetDescriptionAsync ()
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        return "";
      }

      if (!m_local) {
        var remoteDescription = Lemoine.Info.ConfigSet
          .LoadAndGet (REMOTE_DESCRIPTION_KEY, REMOTE_DESCRIPTION_DEFAULT);
        if (!remoteDescription) {
          return "Remote service (unknown)";
        }
        if (!(await CheckPingAsync ())) {
          return "Remote service (ping ko)";
        }
      }

      try {
        var p = new ManagementPath (WmiPath);
        var ManagementObj = new ManagementObject (p);
        return ManagementObj["Description"]?.ToString () ?? "";
      }
      catch (Exception ex) {
        log.Error ($"GetDescriptionAsync: error requesting WMI Path {WmiPath}", ex);
        throw;
      }
    }

    /// <summary>
    /// Startup type of the service: Automatic, Manual, Disabled or Unknown
    /// </summary>
    public StartupType StartupType
    {
      get => Enum.Parse<StartupType> (this.StartMode);
      set { this.StartMode = value.ToString (); }
    }

    /// <summary>
    /// Startup type of the service: Auto/Automatic, Manual, Disabled or Unknown
    /// </summary>
    public string StartMode
    {
      get {
        if (m_local) {
          try {
            var p = new ManagementPath (WmiPath);
            var ManagementObj = new ManagementObject (p);
            return ManagementObj["StartMode"]?.ToString () ?? "Unknown";
          }
          catch (Exception ex) {
            log.Error ($"StartMode.get: error requesting Wmi path={WmiPath}", ex);
            throw;
          }
        }
        else { //
               // TODO: Too slow for remote services
               //       Getting the value for remote services should be done
               //       only when it is necessary
          return "Unknown";
        }
      }

      set {
        if (!new List<string> { "Auto", "Automatic", "Manual", "Disabled" }.Contains (value)) {
          log.Error ($"StartMode.set: invalid value {value}");
          throw new ArgumentException ("The valid values are Auto, Automatic, Manual or Disabled");
        }
        string startMode;
        if (value.Equals ("Auto")) {
          startMode = "Automatic";
        }
        else {
          startMode = value;
        }

        if (this.ServiceName is not null) {
          try {
            var p = new ManagementPath (WmiPath);
            var ManagementObj = new ManagementObject (p);
            //we will use the invokeMethod method of the ManagementObject class

            object[] parameters = new object[1];
            parameters[0] = startMode;
            ManagementObj.InvokeMethod ("ChangeStartMode", parameters);
          }
          catch (Exception ex) {
            log.Error ($"StartMode.set: error requesting WMI path {WmiPath}", ex);
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Check ping
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckPingAsync ()
    {
      if (m_local) {
        return true;
      }

      if (string.IsNullOrEmpty (m_machineName)) {
        return true;
      }

      var pingFirst = Lemoine.Info.ConfigSet
        .LoadAndGet (PING_FIRST_KEY, PING_FIRST_DEFAULT);
      if (!pingFirst) {
        return true;
      }

      var ping = new System.Net.NetworkInformation.Ping ();
      try {
        var timeout = Lemoine.Info.ConfigSet
          .LoadAndGet (PING_TIMEOUT_KEY, PING_TIMEOUT_DEFAULT);
        var reply = await ping.SendPingAsync (this.MachineName, timeout);
        var pingOk = (System.Net.NetworkInformation.IPStatus.Success == reply.Status);
        log.Debug ($"CheckPingAsync: ping answer is {reply.Status}");
        return pingOk;
      }
      catch (ArgumentNullException ex) {
        log.Error ($"CheckPingAsync: empty address {m_machineName}", ex);
        return true;
      }
      catch (System.Net.Sockets.SocketException ex) {
        log.Error ($"CheckPingAsync: socket exception, the address {m_machineName} is not valid", ex);
        return false;
      }
      catch (ObjectDisposedException ex) {
        log.Error ("CheckPingAsync: Object disposed exception", ex);
        throw;
      }
      catch (InvalidOperationException ex) {
        log.Error ("CheckPingAsync: invalid operation exception", ex);
        if (ex.InnerException is System.Net.Sockets.SocketException) {
          log.Error ($"CheckPingAsync: inner socket exception, the address {m_machineName} is not valid", ex);
          return false;
        }
        else {
          throw;
        }
      }
      catch (Exception ex) {
        log.Error ("CheckPingAsync: unexpected error", ex);
        throw;
      }
    }

    #region ServiceController methods and properties
    /// <summary>
    /// Status as returned by <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.Status"/>
    /// </summary>
    public ServiceControllerStatus Status => m_serviceController.Status;

    /// <summary>
    /// Display name as returned by <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.DisplayName"/>
    /// </summary>
    public string DisplayName => m_serviceController.DisplayName;

    /// <summary>
    /// Basic Start method as in <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.Start"/>
    /// </summary>
    public void Start ()
    {
      m_serviceController.Start ();
    }

    /// <summary>
    /// Basic Stop method as in <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.Stop"/>
    /// </summary>
    public void Stop ()
    {
      m_serviceController.Stop ();
    }

    /// <summary>
    /// Basic Continue method as in <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.Continue"/>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Continue ()
    {
      m_serviceController.Continue ();
    }

    /// <summary>
    /// Basic Refresh method as in <see cref="ServiceController"/>
    /// 
    /// <see cref="ServiceController.Refresh"/>
    /// </summary>
    public void Refresh ()
    {
      m_serviceController.Refresh ();
    }
    #endregion // ServiceController methods

    /// <summary>
    /// Check if the service really exists
    /// </summary>
    public bool IsInstalled
    {
      get {
        try {
          var status = this.Status;
          if (log.IsDebugEnabled) {
            log.Debug ($"IsInstalled: the status exists and is {status}");
          }
          return true;
        }
        catch (Exception) {
          return false;
        }
      }
    }

    /// <summary>
    /// Is the service running or at least starting ?
    /// 
    /// An exception is raised if the service does not exist
    /// </summary>
    public bool Running
    {
      get {
        this.Refresh ();
        return this.Status switch {
          ServiceControllerStatus.Stopped => false,
          ServiceControllerStatus.StartPending => true,
          ServiceControllerStatus.StopPending => false,
          ServiceControllerStatus.Running => true,
          ServiceControllerStatus.ContinuePending => true,
          ServiceControllerStatus.PausePending => false,
          ServiceControllerStatus.Paused => false,
          _ => throw new NotImplementedException (),
        };
      }
    }

    /// <summary>
    /// Start a service asynchronously, whichever its status was
    /// </summary>
    /// <returns></returns>
    public async Task StartServiceAsync (CancellationToken cancellationToken = default)
    {
      try {
        cancellationToken.ThrowIfCancellationRequested ();
        this.Refresh ();
        cancellationToken.ThrowIfCancellationRequested ();

        switch (this.Status) {
        case ServiceControllerStatus.Running:
          break;
        case ServiceControllerStatus.StartPending:
        case ServiceControllerStatus.ContinuePending:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: starting (status={this.Status})");
          }
          while (this.Status != ServiceControllerStatus.Running) {
            await Task.Delay (CHECK_DELAY, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            this.Refresh ();
          }
          break;
        case ServiceControllerStatus.StopPending:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: stop pending, wait it is stopped first");
          }
          while (this.Status != ServiceControllerStatus.Stopped) {
            await Task.Delay (CHECK_DELAY, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            this.Refresh ();
          }
          goto case ServiceControllerStatus.Stopped;
        case ServiceControllerStatus.Stopped:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: stopped, start it");
          }
          await CheckExistingAndStartAsync (cancellationToken);
          while (this.Status != ServiceControllerStatus.Running) {
            await Task.Delay (CHECK_DELAY, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            Refresh ();
          }
          break;
        case ServiceControllerStatus.Paused:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: paused, start it");
          }
          Continue ();
          while (this.Status != ServiceControllerStatus.Running) {
            await Task.Delay (CHECK_DELAY, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            Refresh ();
          }
          break;
        case ServiceControllerStatus.PausePending:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: pause pending, wait it is stopped first");
          }
          while (this.Status != ServiceControllerStatus.Paused) {
            await Task.Delay (CHECK_DELAY, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            Refresh ();
          }
          goto case ServiceControllerStatus.Paused;
        }
      }
      catch (OperationCanceledException ex) {
        log.Warn ($"StartServiceAsync: the service {this.ServiceName} could not be started since the operation was canceled", ex);
        throw;
      }
      catch (Exception ex) {
        log.Error ($"StartServiceAsync: the service {this.ServiceName} could not be started", ex);
        throw;
      }
    }

    /// <summary>
    /// Restart a service
    /// </summary>
    public async Task RestartAsync ()
    {
      try {
        switch (this.Status) {
        case ServiceControllerStatus.Stopped:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: stopped, start it");
          }
          await this.CheckExistingAndStartAsync ();
          while (this.Status != ServiceControllerStatus.Running) {
            await Task.Delay (CHECK_DELAY);
            this.Refresh ();
          }
          return;
        case ServiceControllerStatus.StopPending:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: stop pending, wait it is stopped first");
          }
          while (this.Status != ServiceControllerStatus.Stopped) {
            await Task.Delay (CHECK_DELAY);
            this.Refresh ();
          }
          goto case ServiceControllerStatus.Stopped;
        case ServiceControllerStatus.Running:
          return;
        case ServiceControllerStatus.StartPending:
        case ServiceControllerStatus.ContinuePending:
          if (log.IsDebugEnabled) {
            log.Debug ($"StartServiceAsync: starting (status={this.Status})");
          }
          while (this.Status != ServiceControllerStatus.Running) {
            await Task.Delay (CHECK_DELAY);
            this.Refresh ();
          }
          return;
        case ServiceControllerStatus.PausePending:
          // TODO: ...
          break;
        case ServiceControllerStatus.Paused:
          this.Continue ();
          return;
        }
      }
      catch (Exception ex) {
        log.Error ($"RestartOnWindowsAsync: exception", ex);
        throw;
      }
    }


    async Task CheckExistingAndStartAsync (CancellationToken cancellationToken = default)
    {
      if (m_local) {
        try {
          await KillExistingProcessAsync (cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"CheckExistingAndStart: exception", ex);
        }
      }
      StartService ();
    }

    void StartService ()
    {
      try {
        this.Start ();
      }
      catch (System.InvalidOperationException ex) {
        if (ex.InnerException is System.ComponentModel.Win32Exception) {
          var win32exception = ex.InnerException as System.ComponentModel.Win32Exception;
          if (win32exception is not null && win32exception.NativeErrorCode == 0x00000420) { // An instance of the service is already running.
            log.Info ($"Start: instance already running", ex);
            return;
          }
          else {
            log.Error ($"Start: win32exception", ex);
            throw;
          }
        }
      }
      catch (System.ComponentModel.Win32Exception ex) {
        if (ex.NativeErrorCode == 0x00000420) { // An instance of the service is already running.
          log.Info ($"Start: instance already running", ex);
          return;
        }
        else {
          log.Error ($"Start: win32exception", ex);
          throw;
        }
      }
      catch (Exception ex) {
        log.Error ("Start: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// An exception is raised if the service could not be killed
    /// </summary>
    /// <returns>a process was killed</returns>
    public async Task<bool> KillExistingProcessAsync (CancellationToken cancellationToken = default)
    {
      int pid = 0;
      try {
        pid = GetServicePID ();
      }
      catch (Exception ex) {
        log.Error ($"KillExistingProcessAsync: GetServicePID raised an exception for {this.ServiceName}", ex);
      }
      if (0 == pid) {
        log.Debug ($"KillExistingProcessAsync: no process for {this.ServiceName}");
        return false;
      }
      else {
        if (!(await KillProcessAsync (pid, cancellationToken))) {
          log.Error ($"KillExistingProcessAsync: process with pid={pid} could not be stopped");
          throw new Exception ("Service still alive");
        }
        else { // Ok !
          return true;
        }
      }
    }

    /// <summary>
    /// Try to kill if needed a process
    /// </summary>
    /// <param name="pid"></param>
    /// <returns>the process was successfully killed</returns>
    static async Task<bool> KillProcessAsync (int pid, CancellationToken cancellationToken = default)
    {
      Process process;

      try {
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        slog.Debug ($"KillProcessAsync: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      cancellationToken.ThrowIfCancellationRequested ();

      // 1st attempt: try to stop first properly the process
      try {
        slog.Info ("KillProcessAsync: Stop the process");
        process.EnableRaisingEvents = true;
        if (false == process.CloseMainWindow ()) {
          slog.Debug ("KillProcessAsync: the close message could not be sent");
        }
        process.Close ();
      }
      catch (Exception ex) {
        slog.Error ("KillProcessAsync: CloseMainWindow or Close returned an exception", ex);
      }
      cancellationToken.ThrowIfCancellationRequested ();

      // Check if the process was stopped after maximum 5 seconds
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        slog.Debug ($"KillProcessAsync: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      try {
        var delayedCancellation = new CancellationTokenSource (TimeSpan.FromSeconds (5));
        using (var linkedCancellationTokenSource = CancellationTokenSource
          .CreateLinkedTokenSource (delayedCancellation.Token, cancellationToken)) {
          await process.WaitForExitAsync (linkedCancellationTokenSource.Token);
          cancellationToken.ThrowIfCancellationRequested ();
          if (delayedCancellation.IsCancellationRequested) {
            slog.Info ("KillProcessAsync: Process is still running after 5s !");
            // Kill the process
          }
          else {
            slog.Info ("KillProcessAsync: Nice, the process does not exist any more");
            return true;
          }
        }
      }
      catch (OperationCanceledException ex1) {
        slog.Info ($"KillProcessAsync: operation canceled", ex1);
        throw;
      }
      catch (Exception ex1) {
        slog.Info ("KillProcessAsync: WaitForExit returned an exception check if the process still exists", ex1);
        cancellationToken.ThrowIfCancellationRequested ();
        try {
          process = Process.GetProcessById (pid);
        }
        catch (Exception ex2) {
          slog.Info ($"KillProcessAsync: process pid={pid} does not exist any more, => return true", ex2);
          return true;
        }
      }

      cancellationToken.ThrowIfCancellationRequested ();

      // If not, kill the process
      try { // Required after Close ()
        process = Process.GetProcessById (pid);
      }
      catch (Exception ex) {
        slog.Debug ($"KillProcessAsync: process with pid {pid} can't be found => it probably does not exist any more, return true", ex);
        return true;
      }
      cancellationToken.ThrowIfCancellationRequested ();
      try {
        slog.Debug ("KillProcessAsync: Kill the process");
        process.Kill ();
        slog.Info ("The process was successfully killed");
        return true;
      }
      catch (System.ComponentModel.Win32Exception ex) {
        slog.Info ($"KillProcessAsync: The process could not be terminated or is terminating => wait 10 s, else give up", ex);
        try {
          var delayedCancellation = new CancellationTokenSource (TimeSpan.FromSeconds (10));
          using (var linkedCancellationTokenSource = CancellationTokenSource
            .CreateLinkedTokenSource (delayedCancellation.Token, cancellationToken)) {
            cancellationToken.ThrowIfCancellationRequested ();
            await process.WaitForExitAsync (delayedCancellation.Token);
            if (delayedCancellation.IsCancellationRequested) {
              slog.Error ($"KillProcessAsync: Process is still running after 10s !", ex);
              return false;
            }
            else {
              slog.Info ("KillProcessAsync: Nice, the process does not exist any more");
              return true;
            }
          }
        }
        catch (OperationCanceledException ex1) {
          slog.Info ($"KillProcessAsync: operation canceled", ex1);
          throw;
        }
        catch (Exception ex1) {
          slog.Info ($"KillProcessAsync: WaitForExit returned an exception check if the process still exists", ex1);
          try {
            process = Process.GetProcessById (process.Id);
          }
          catch (Exception) {
            slog.Info ($"KillProcessAsync: process pid={process.Id} does not exist any more, => return true");
            return true;
          }
          slog.Error ($"KillProcessAsync: the process pid={process.Id} could not be killed, may be because the used privilege was not high enough");
          return false;
        }
      }
      catch (NotSupportedException ex) {
        slog.Fatal ($"KillProcessAsync: The process is a local process this exception should not happen", ex);
        Debug.Assert (false);
        return false;
      }
      catch (InvalidOperationException ex) {
        slog.Warn ($"KillProcessAsync: The process is invalid => it can't be killed", ex);
        return true;
      }
      catch (OperationCanceledException ex) {
        slog.Info ($"KillProcessAsync: operation canceled", ex);
        throw;
      }
    }

    /// <summary>
    /// Get the first pid that may correspond to the service name
    /// 
    /// If there is no such process, 0 is returned
    /// 
    /// An exception may be also thrown
    /// </summary>
    /// <returns></returns>
    public int GetServicePID ()
    {
      RegistryKey? registryKey = Registry.LocalMachine
        .OpenSubKey ("SYSTEM\\CurrentControlSet\\Services\\" + this.ServiceName, false);
      if (registryKey is null) {
        log.Error ($"GetServicePID: the registry key of the service {this.ServiceName} does not exist");
        throw new ArgumentException ("service does not exist");
      }
      else { // registryKey not null
        object? serviceImagePath = registryKey.GetValue ("ImagePath");
        if (serviceImagePath is null) {
          log.Error ($"GetServicePID: the registry key for service {this.ServiceName} does not have the value ImagePath. The registry key is {registryKey}");
          throw new ApplicationException ("No ImagePath");
        }
        else { // null != serviceImagePath
          string path = serviceImagePath.ToString ()?.Trim (new char[] { '"', }) ?? throw new InvalidOperationException ();
          if (slog.IsDebugEnabled) {
            log.Debug ($"GetServicePID: got Path for Service {this.ServiceName} = {path}");
          }
          string exe = Path.GetFileNameWithoutExtension (path);
          System.Diagnostics.Process[] processes =
            System.Diagnostics.Process.GetProcessesByName (exe);
          if (processes.Length <= 0) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetServicePID: there is no process with the name {exe}, may be it is stopped => return 0");
            }
            return 0;
          }
          else { // 0 < processes.Length
            if (1 < processes.Length) {
              log.Warn ($"GetServicePID: there are {processes.Length} processes for {exe}");
            }
            if (log.IsDebugEnabled) {
              log.Debug ($"GetServicePID: first PID is {processes[0].Id} for service {this.ServiceName}");
            }
            return processes[0].Id;
          }
        }
      }
    }

    async Task StopAsync ()
    {
      try {
        this.Stop ();
      }
      catch (System.InvalidOperationException ex) {
        if (ex.InnerException is System.ComponentModel.Win32Exception) {
          var win32exception = ex.InnerException as System.ComponentModel.Win32Exception;
          if (win32exception is not null && win32exception.NativeErrorCode == 0x00000425) { // The service cannot accept control messages at this time.
            log.Info ($"StopAsync: cannot accept control messages, try again in 1 second", ex);
            await Task.Delay (1000);
            await StopAsync ();
          }
          else if (win32exception is not null && win32exception.NativeErrorCode == 0x00000888) { // The service has not been started..
            log.Info ($"StopAsync: The service has not been started", ex);
            return;
          }
          else {
            log.Error ($"StopAsync: win32exception", ex);
            throw;
          }
        }
      }
      catch (System.ComponentModel.Win32Exception win32exception) {
        if (win32exception.NativeErrorCode == 0x00000425) { // The service cannot accept control messages at this time.
          log.Info ($"StopAsync: cannot accept control messages, try again in 1 second", win32exception);
          await Task.Delay (1000);
          await StopAsync ();
        }
        else if (win32exception.NativeErrorCode == 0x00000888) { // The service has not been started..
          log.Info ($"StopAsync: The service has not been started", win32exception);
          return;
        }
        else {
          log.Error ($"StopAsync: win32exception", win32exception);
          throw;
        }
      }
      catch (Exception ex) {
        log.Error ($"StopAsync: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Stop a service asynchronously
    /// 
    /// Kill it if required
    /// </summary>
    /// <returns></returns>
    public async Task StopServiceAsync ()
    {
      // - Stop the service
      string serviceName = this.ServiceName;

      ServiceControllerStatus status;
      try {
        status = this.Status;
      }
      catch (Exception ex) {
        log.Warn ($"StopServiceAsync: could not get the status of the service, it is probably not installed ! => give up (service not running)", ex);
        throw;
      }

      try {
        if (status.Equals (ServiceControllerStatus.Stopped)) { // Already stopped
          return;
        }
        await StopAsync ();
        DateTime stopTime = DateTime.UtcNow;
        while (this.Status != ServiceControllerStatus.Stopped) {
          await Task.Delay (CHECK_DELAY);
          this.Refresh ();
          if (STOP_TIMEOUT < DateTime.UtcNow.Subtract (stopTime)) {
            log.Warn ($"StopServiceAsync: the service could not be stopped before after {STOP_TIMEOUT} => give up, and try to kill it instead");
            break;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"StopServiceAsync: the service {serviceName} could not be stopped", ex);
        if (!m_local) { // If this is not a local computer, give up at once
          throw;
        }
      }

      if (!m_local) { // Remote computer
        // Consider the service is stopped
        return;
      }

      // This is a local computer
      Debug.Assert (m_local);
      // - Check the process is not here any more
      //   Else kill it
      await KillExistingProcessAsync ();
    }

    /// <summary>
    /// Specific ToString method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return $"[WindowsServiceController {this.ServiceName} on {this.MachineName}]";
    }
  }
}
