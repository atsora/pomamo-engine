// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Service
{
  /// <summary>
  /// Windows ServiceBase implementation using a IThreadService.
  /// </summary>
  public class ThreadServiceBase : System.ServiceProcess.ServiceBase
  {
    #region Members
    readonly ServiceOptions m_options;
    readonly string[] m_args;
    readonly IThreadService m_service;
    readonly string m_serviceName;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (ThreadServiceBase).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serviceName"></param>
    /// <param name="args">Command line arguments</param>
    public ThreadServiceBase (IThreadService service, string serviceName, ServiceOptions options, string[] args)
    {
      m_service = service;
      m_options = options;
      m_args = args;
      m_serviceName = serviceName;
#if NET48 || NETCOREAPP
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
#else // !(NET48 || NETCOREAPP)
      {
#endif // !(NET48 || NETCOREAPP)

        this.ServiceName = serviceName;
      }
    }

    /// <summary>
    /// Start this service.
    /// </summary>
    protected override void OnStart (string[] args)
    {
      Thread initializeThread = new Thread (new ThreadStart (SafeInitialize));
      initializeThread.Start ();
    }

    /// <summary>
    /// Stop this service
    /// </summary>
    protected override void OnStop ()
    {
      m_service.OnStop ();
    }

    /// <summary>
    /// Safe initialize (run by the initialize thread)
    /// </summary>
    protected virtual void SafeInitialize ()
    {
      try {
        m_service.Initialize ();
      }
      catch (Exception ex) {
        log.Fatal ("SafeInitialize: exception => exit", ex);
        // - Try first to stop the service
#if NET48 || NETCOREAPP
        if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
#else // !(NET48 || NETCOREAPP)
        {
#endif // !(NET48 || NETCOREAPP)
          try {
            log.Debug ("SafeInitialize: try to stop the service (give it 5s)");
            this.Stop ();
            System.Threading.Thread.Sleep (5000); // 5s
          }
          catch (Exception ex1) {
            log.Fatal ("SafeInitialize: exception in Stop ()", ex1);
          }
        }
        // - Try to exit
        try {
          LogManager.Shutdown ();
          System.Environment.Exit (1);
        }
        catch (Exception ex2) {
          log.Fatal ("Could not make the application stop", ex2);
        }
      }
    }

    /// <summary>
    /// Run the corresponding service
    /// </summary>
    public void Run ()
    {
#if NET40
      var isWindows = true;
#else // !NET40
      var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif // !NET40
      // Run the service
      if (!isWindows || m_options.Debug || m_options.Interactive) { // Debug mode
        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => OnCancel ();
        if (System.Environment.UserInteractive) {
          RunInteractive ();
        }
        else {
          this.OnStart (m_args);
          Thread.Sleep (Timeout.Infinite);
        }
      }
      else { // Normal mode, run the service normally
        System.ServiceProcess.ServiceBase.Run (new System.ServiceProcess.ServiceBase[] { this });
      }
    }

    void OnCancel ()
    {
#if NET40
      var isWindows = true;
#else // !NET40
      var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif // !NET40
      if (isWindows) {
        Stop ();
      }
      else {
        m_service.OnStop ();
      }
    }

    void RunInteractive ()
    {
      Console.WriteLine ();
      Console.WriteLine ($"Start the service {m_serviceName} in interactive mode");
      Console.WriteLine ();

#if NULLABLE
      MethodInfo?
#else // !NULLABLE
      MethodInfo
#endif // !NULLABLE
        onStartMethod = typeof (System.ServiceProcess.ServiceBase).GetType ().GetMethod ("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);
      onStartMethod?.Invoke (this, m_args);

      Console.WriteLine ();
      Console.WriteLine ("Press a key to stop the service");
      Console.ReadKey ();
      Console.WriteLine ();

#if NULLABLE
      MethodInfo?
#else // !NULLABLE
      MethodInfo
#endif // !NULLABLE
        onStopMethod = typeof (System.ServiceProcess.ServiceBase).GetMethod ("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);
      onStopMethod?.Invoke (this, null);

      Console.WriteLine ();
      Console.WriteLine ("Stopped");
    }

    /// <summary>
    /// Install the service
    /// 
    /// Only available with the .NET framework.
    /// 
    /// To be removed later. The replacement is to use Windows installer XML
    /// </summary>
    public static void Install ()
    {
#if NET48
      System.Configuration.Install.ManagedInstallerClass.InstallHelper (new string[] { Lemoine.Info.ProgramInfo.Name + ".exe" });
#else // !NET48
      log.Error ("Install: Not supported framework");
      throw new NotSupportedException ("not supported framework");
#endif // NET48
    }

    /// <summary>
    /// Remove the service
    /// 
    /// Only available with the .NET framework
    /// 
    /// To be removed later. The replacement is to use Windows installer XML
    /// </summary>
    public static void Remove ()
    {
#if NET48
      System.Configuration.Install.ManagedInstallerClass.InstallHelper (new string[] { "/u ", Lemoine.Info.ProgramInfo.Name + ".exe" });
#else // !NET48
      log.Error ("Install: Not supported framework");
      throw new NotSupportedException ("not supported framework");
#endif // NET48
    }
  }
}
