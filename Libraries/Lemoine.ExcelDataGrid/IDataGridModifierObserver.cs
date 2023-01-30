// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Description of IDataGridModifierObserver.
  /// </summary>
  public interface IDataGridModifierObserver
  {
    /// <summary>
    /// action to taken on region selection invalidation in DataGridModifier
    /// </summary>    
    void InvalidateRegionSelections();
  }
}
