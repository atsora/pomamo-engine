// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Sortable data grid view.
  /// </summary>
  public class SortableDataGridView : DataGridView
  {
    /// <summary>
    /// Sort the data grid view when the user clicks on the column header (overrides the event)
    /// </summary>
    /// <param name="levent"></param>
    protected override void OnColumnHeaderMouseClick (DataGridViewCellMouseEventArgs levent)
    {
      base.OnColumnHeaderMouseClick (levent);
      Sort (levent.ColumnIndex);
    }
    
    /// <summary>
    /// Set all columns as sortable once the binding is complete
    /// </summary>
    /// <param name="levent"></param>
    protected override void OnDataBindingComplete (DataGridViewBindingCompleteEventArgs levent)
    {
      base.OnDataBindingComplete (levent);
      foreach (DataGridViewColumn column in this.Columns) {
        if (column.SortMode != DataGridViewColumnSortMode.NotSortable) {
          column.SortMode = DataGridViewColumnSortMode.Programmatic;
        }
      }
    }
    
    /// <summary>
    /// Sort the data grid view from a column index
    /// </summary>
    /// <param name="columnIndex"></param>
    protected void Sort (int columnIndex)
    {
      Sort (this.Columns [columnIndex]);
    }
    
    /// <summary>
    /// Sort the data grid view from a column
    /// </summary>
    protected void Sort (DataGridViewColumn column)
    {
      if (DataGridViewColumnSortMode.NotSortable == column.SortMode) {
        return;
      }
      DataGridViewColumn selectedColumn = column;
      DataGridViewColumn oldSortedColumn = this.SortedColumn;
      ListSortDirection direction;
      
      // if oldSortedColum is null, the DataGridView is not sorted
      if (oldSortedColumn != null) {
        // sort same column again, reversing the order
        if (oldSortedColumn == selectedColumn &&
            this.SortOrder == SortOrder.Ascending) {
          direction = ListSortDirection.Descending;
        }
        else {
          // sort a new column an remove the old SortGlyph
          direction = ListSortDirection.Ascending;
          oldSortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
        }
      }
      else {
        direction = ListSortDirection.Ascending;
      }
      
      // sort selected column
      this.Sort(selectedColumn, direction);
      selectedColumn.HeaderCell.SortGlyphDirection =
        direction == ListSortDirection.Ascending ?
        SortOrder.Ascending : SortOrder.Descending;
    }
  }
}
