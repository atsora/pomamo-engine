// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.BaseControls;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.I18N;
using Lemoine.Info;
using Lemoine.ModelDAO.Info;
using Pulse.Hosting;

namespace Lemoine.DataControls.GuiInitializer
{
  /// <summary>
  /// GuiInitializer
  /// </summary>
  public class GuiInitializer : IGuiInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (GuiInitializer).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    public GuiInitializer (IApplicationInitializer applicationInitializer, IFileRepoClientFactory fileRepoClientFactory = null)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    /// <summary>
    /// <see cref="IGuiInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InitializeGui (CancellationToken cancellationToken)
    {
      m_applicationInitializer.InitializeApplication (cancellationToken);

      try {
        ConfigSet.AddConfigReader (new ApplicationStateConfigReader ("user.", true));
      }
      catch (Exception ex) {
        log.Error ($"InitializeGui: AddConfigReader ApplicationStateConfigReader with prefix configset failed", ex);
      }

      if (null != m_fileRepoClientFactory) {
        m_fileRepoClientFactory.InitializeFileRepoClient ();
      }
    }
  }
}
