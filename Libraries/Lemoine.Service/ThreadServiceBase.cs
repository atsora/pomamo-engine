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
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (ThreadServiceBase).FullName);

    #region Getters / Setters
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serviceName"></param>
    /// <param name="options"></param>
    /// <param name="args">Command line arguments</param>
    public ThreadServiceBase (IThreadService service, string serviceName, ServiceOptions options, string[] args)
    {
      m_service = service;
      m_options = options;
      m_args = args;
      this.ServiceName = serviceName;
    }
    #endregion

    #region ServiceBase methods
    #endregion

    #region Methods
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
        try {
          log.Debug ("SafeInitialize: try to stop the service (give it 5s)");
          this.Stop ();
          System.Threading.Thread.Sleep (5000); // 5s
        }
        catch (Exception ex1) {
          log.Fatal ("SafeInitialize: exception in Stop ()", ex1);
        }
        // - Try to exit
        Lemoine.Core.Environment.LogAndForceExit (ex, log);
      }
    }

    /// <summary>
    /// Run the corresponding service
    /// </summary>
    public void Run ()
    {
#if !NET48 && !NETCOREAPP
      var isWindows = true;
#else // NET48 || NETCOREAPP
      var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif // NET48 || NETCOREAPP
      // Run the service
      if (m_options.Debug || m_options.Interactive || !isWindows) { // Debug mode
        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => Stop ();
        if (System.Environment.UserInteractive) {
          RunInteractive ();
          this.OnStart (m_args);
          Thread.Sleep (Timeout.Infinite);
        }
      }
      else {
        if (m_options.NoConsole) {
          this.OnStart (m_args);
          Thread.Sleep (Timeout.Infinite);
        }
        else {
          // Normal mode, run the service normally
          System.ServiceProcess.ServiceBase.Run (new System.ServiceProcess.ServiceBase[] { this });
        }
      }
    }

    void RunInteractive ()
    {
      Console.WriteLine ();
      Console.WriteLine ($"Start the service {this.ServiceName} in interactive mode");
      Console.WriteLine ();

      OnStart (m_args);

      Console.WriteLine ();
      Console.WriteLine ("Press a key to stop the service");
      Console.ReadKey ();
      Console.WriteLine ();

      OnStop ();

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
#if NET45
      System.Configuration.Install.ManagedInstallerClass.InstallHelper (new string[] { Lemoine.Info.ProgramInfo.Name + ".exe" });
#else
      log.Error ("Install: Not supported framework");
      throw new NotSupportedException ("not supported framework");
#endif // NET45
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
#if NET45
      System.Configuration.Install.ManagedInstallerClass.InstallHelper (new string[] { "/u ", Lemoine.Info.ProgramInfo.Name + ".exe" });
#else
      log.Error ("Install: Not supported framework");
      throw new NotSupportedException ("not supported framework");
#endif // NET45
    }
#endregion
  }
}
