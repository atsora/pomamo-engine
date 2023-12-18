// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET40 || NET45 || NET48
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

using Lemoine.Core.Log;

namespace Lem_CncService
{
  /// <summary>
  /// Project installer
  /// </summary>
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
    #region Members
    readonly ServiceProcessInstaller serviceProcessInstaller;
    readonly ServiceInstaller serviceInstaller;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (ProjectInstaller).FullName);

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
    #endregion
  }
}
#endif // NET40 || NET45 || NET48
