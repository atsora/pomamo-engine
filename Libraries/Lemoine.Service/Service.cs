// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using System.Threading;

using CommandLine;
using Lemoine.Core.Log;

namespace Lemoine.Service
{
  /// <summary>
  /// Lemoine service.
  /// This is the abstract base class for all the Lemoine services.
  /// 
  /// Do not forget to implement the following methods if needed:
  /// <item>Initialize or OnStart</item>
  /// <item>OnStop</item>
  /// <item>Dispose</item>
  /// </summary>
  [Obsolete ("Implement IThreadService and use ThreadServiceBase instead", false)]
  public abstract class Service : System.ServiceProcess.ServiceBase
  {
    #region Members
    readonly string[] args;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (Service).FullName);

    #region Getters / Setters
    /// <summary>
    /// Debug mode (-d or -debug options)
    /// </summary>
    public bool DebugMode { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="args">Command line arguments</param>
    protected Service (string serviceName, string[] args)
      : this (args)
    {
      this.ServiceName = serviceName;
    }

    /// <summary>
    /// Deprecated constructor
    /// </summary>
    /// <param name="args">Command line arguments</param>
    protected Service (string[] args)
    {
      this.args = args;
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
    /// Safe initialize (run by the initialize thread)
    /// </summary>
    protected virtual void SafeInitialize ()
    {
      try {
        Initialize ();
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
    /// Method that is run in a thread after OnStart...
    /// </summary>
    protected virtual void Initialize ()
    {
      log.FatalFormat ("Initialize: should be overwritten if the base OnStart method is used");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Run the corresponding service
    /// </summary>
    public void Run ()
    {
      // Parse the arguments
      try {
        var options = ServiceOptions.Parse (args);
        DebugMode = options.Debug;
#if NET45
        if (options.Install) {
          this.Install ();
          return;
        }
        else if (options.Remove) {
          this.Remove ();
          return;
        }
#endif // NET45
      }
      catch (Exception ex) {
        log.ErrorFormat ("Service: " +
                         "exception {0} raised",
                         ex);
        System.Console.Error.WriteLine (String.Format ("Exception {0} raised",
                                                       ex));
        throw;
      }

#if !NET48 && !NETCOREAPP
      var isWindows = true;
#else // NET48 || NETCOREAPP
      var isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#endif // NET48 || NETCOREAPP
      // Run the service
      if (DebugMode || !isWindows
        || (System.Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)) { // Debug mode
        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => Stop ();
        this.OnStart (args);
        Thread.Sleep (Timeout.Infinite);
      }
      else { // Normal mode, run the service normally
        System.ServiceProcess.ServiceBase.Run (new System.ServiceProcess.ServiceBase[] { this });
      }
    }

    /// <summary>
    /// Install the service
    /// 
    /// Only available with the .NET framework.
    /// 
    /// To be removed later. The replacement is to use Windows installer XML
    /// </summary>
    public void Install ()
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
    public void Remove ()
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
