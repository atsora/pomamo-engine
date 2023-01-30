// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET40 || NET45 || NET48
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Lem_AutoReasonService
{
  /// <summary>
  /// Installer class of the service
  /// 
  /// Only available with the .NET Framework
  /// </summary>
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
    #region Members
    readonly ServiceProcessInstaller m_serviceProcessInstaller;
    readonly ServiceInstaller m_serviceInstaller;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ProjectInstaller()
    {
      m_serviceProcessInstaller = new ServiceProcessInstaller();
      m_serviceInstaller = new ServiceInstaller();
      m_serviceProcessInstaller.Account = ServiceAccount.LocalService;
      
      m_serviceInstaller.ServiceName = Program.SERVICE_NAME;
      this.Installers.AddRange(new Installer[] { m_serviceProcessInstaller, m_serviceInstaller });
    }
    #endregion // Constructors
  }
}
#endif // NET40 || NET45 || NET48
