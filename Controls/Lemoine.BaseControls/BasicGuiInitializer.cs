// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// BasicGuiInitializer
  /// </summary>
  public class BasicGuiInitializer: IGuiInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (BasicGuiInitializer).FullName);

    readonly Action m_applicationInitialization;

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicGuiInitializer (Action applicationInitialization = null)
    {
      m_applicationInitialization = applicationInitialization;
    }

    /// <summary>
    /// <see cref="IGuiInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public void InitializeGui (CancellationToken cancellationToken)
    {
      if (null != m_applicationInitialization) {
        m_applicationInitialization ();
      }
    }
  }
}
