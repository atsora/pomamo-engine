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

namespace Lem_CncDataService
{
  /// <summary>
  /// Installer class of the service
  /// </summary>
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
    #region Members
    readonly ServiceProcessInstaller m_serviceProcessInstaller;
    readonly ServiceInstaller m_serviceInstaller;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProjectInstaller).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ProjectInstaller()
    {
      m_serviceProcessInstaller = new ServiceProcessInstaller();
      m_serviceInstaller = new ServiceInstaller();
      // TODO: Here you can set properties on serviceProcessInstaller or register event handlers
      m_serviceProcessInstaller.Account = ServiceAccount.LocalService;
      
      m_serviceInstaller.ServiceName = Program.SERVICE_NAME;
      this.Installers.AddRange(new Installer[] { m_serviceProcessInstaller, m_serviceInstaller });
    }
    #endregion // Constructors
  }
}
#endif // NET45 || NET48
