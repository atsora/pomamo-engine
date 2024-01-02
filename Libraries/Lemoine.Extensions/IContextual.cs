// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Interface for the extensions with a context
  /// </summary>
  public interface IContextual
  {
    /// <summary>
    /// Context of the extension
    /// </summary>
    PluginContext Context
    {
      get; set;
    }
  }
}
