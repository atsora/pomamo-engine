// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET45 || NET48
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

using Lemoine.Core.Log;

namespace Lem_AnalysisService
{
  /// <summary>
  /// Service installer
  /// 
  /// Only available with the .NET Framework
  /// </summary>
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
#region Members
    private ServiceProcessInstaller serviceProcessInstaller;
    private ServiceInstaller serviceInstaller;
#endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProjectInstaller).FullName);

#region Getters / Setters
    // TODO: Generate here with the Alt-Inser command the wanted getters / setters
    // TODO: Do not forget to add the description of the property with ///
#endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ProjectInstaller()
    {
      serviceProcessInstaller = new ServiceProcessInstaller();
      serviceInstaller = new ServiceInstaller();
      // TODO: Here you can set properties on serviceProcessInstaller or register event handlers
      serviceProcessInstaller.Account = ServiceAccount.LocalService;
      
      serviceInstaller.ServiceName = Program.SERVICE_NAME;
      this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
    }
#endregion // Constructors
  }
}

#endif // NET45 || NET48
