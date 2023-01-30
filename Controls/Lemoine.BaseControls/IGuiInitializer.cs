// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Gui application initializer
  /// </summary>
  public interface IGuiInitializer
  {
    /// <summary>
    /// Initialize the Gui application
    /// </summary>
    /// <param name="cancellationToken"></param>
    void InitializeGui (CancellationToken cancellationToken);
  }
}
