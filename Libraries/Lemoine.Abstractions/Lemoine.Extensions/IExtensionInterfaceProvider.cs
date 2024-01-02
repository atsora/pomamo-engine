// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Interface to be implemented by libraries that provide extension interfaces
  /// </summary>
  public interface IExtensionInterfaceProvider
  {
    /// <summary>
    /// Load the extension interfaces
    /// </summary>
    void Load ();
  }
}
