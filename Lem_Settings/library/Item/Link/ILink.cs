// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of ILink.
  /// </summary>
  public interface ILink : IItem
  {
    /// <summary>
    /// Url to open
    /// </summary>
    String UrlLink { get; }
  }
}
