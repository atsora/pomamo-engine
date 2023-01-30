// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Item that can be selected in a configuration dialog
  /// </summary>
  public interface ISelectionable
  {
    /// <summary>
    /// Text to display in a selection dialog
    /// </summary>
    string SelectionText { get; }
  }
}
