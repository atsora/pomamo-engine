// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Description of GridModificationEvent.
  /// note that derived classes are XML serializable
  /// </summary>
  [XmlType("GridModificationEvent")]
  [XmlInclude(typeof(ChangeEvent)), XmlInclude(typeof(PruneEvent)),
   XmlInclude(typeof(CopyToEvent)),
   XmlInclude(typeof(ShiftColumnEvent)), XmlInclude(typeof(DeleteColumnEvent)),
   XmlInclude(typeof(DeleteRowEvent)), XmlInclude(typeof(InsertRowEvent))]
  public abstract class GridModificationEvent
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GridModificationEvent).FullName);
    
    /// <summary>
    /// cell modification event
    /// </summary>
    [XmlType("InsertEvent")]
    public class ChangeEvent : GridModificationEvent {
      
      #region Members
      int m_selectedCellColumn;
      int m_selectedCellRow;
      object m_oldCellValue;
      string m_cellValue;
      #endregion // Members

      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public ChangeEvent() {}
      
      /// <summary>
      /// build change event from a cell column/row/content
      /// </summary>
      /// <param name="column"></param>
      /// <param name="row"></param>
      /// <param name="content"></param>
      public ChangeEvent(int column, int row, string content) {
        SelectedCellColumn = column;
        SelectedCellRow = row;
        CellValue = content;
      }
      #endregion // Constructors
      
      #region Getters / Setters
      /// <summary>
      /// modified cell column
      /// </summary>
      [XmlAttribute("column")]
      public int SelectedCellColumn {
        get { return m_selectedCellColumn; }
        set { m_selectedCellColumn = value; }
      }
      
      /// <summary>
      /// modified cell row
      /// </summary>
      [XmlAttribute("row")]
      public int SelectedCellRow {
        get { return m_selectedCellRow; }
        set { m_selectedCellRow = value; }
      }
      
      /// <summary>
      /// modified cell old value (to undo)
      /// </summary>
      [XmlIgnore]
      public object OldCellValue {
        get { return m_oldCellValue; }
        set { m_oldCellValue = value; }
      }
      
      /// <summary>
      /// modified cell new value
      /// </summary>
      [XmlAttribute("value")]
      public string CellValue {
        get { return m_cellValue; }
        set { m_cellValue = value; }

      }
      #endregion Getters / Setters
      
      #region Methods
      /// <summary>
      /// to undo, recover old cell value
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        dataGridMod.GridView.Rows[SelectedCellRow].Cells[SelectedCellColumn].Value = this.OldCellValue;
      }
      
      /// <summary>
      /// if column/row index correspond to a cell,
      /// change cell value to new value and remember old value
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay) {
        if (!((SelectedCellRow < dataGridMod.GridView.Rows.Count )
              && (SelectedCellColumn < dataGridMod.GridView.Rows[SelectedCellRow].Cells.Count)))
          return false;
        
        DataGridViewCell selectedCell =
          dataGridMod.GridView.Rows[SelectedCellRow].Cells[SelectedCellColumn];
        this.OldCellValue = selectedCell.Value;
        selectedCell.Value = CellValue;
        return true;
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable() {
        return true;
      }
      
      #endregion // Methods
    }

    /// <summary>
    /// copy between two cells event
    /// </summary>
    [XmlType("CopyToEvent")]
    public class CopyToEvent : GridModificationEvent {
      
      #region Members
      int m_sourceCellColumn;
      int m_sourceCellRow;
      int m_targetCellColumn;
      int m_targetCellRow;
      object m_oldCellValue;
      
      #endregion // Members
      
      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public CopyToEvent() {}
      
      /// <summary>
      /// build change event from a cell column/row/content
      /// </summary>
      /// <param name="sourceRow"></param>
      /// <param name="sourceCol"></param>
      /// <param name="targetRow"></param>
      /// <param name="targetCol"></param>
      public CopyToEvent(int sourceRow, int sourceCol,
                         int targetRow, int targetCol)
      {
        SourceCellRow = sourceRow;
        SourceCellColumn = sourceCol;
        TargetCellRow = targetRow;
        TargetCellColumn = targetCol;
      }
      #endregion // Constructors
      
      #region Getters / Setters

      /// <summary>
      /// source cell column
      /// </summary>
      [XmlAttribute("sourceColumn")]
      public int SourceCellColumn {
        get { return m_sourceCellColumn; }
        set { m_sourceCellColumn = value; }
      }
      
      /// <summary>
      /// source cell row
      /// </summary>
      [XmlAttribute("sourceRow")]
      public int SourceCellRow {
        get { return m_sourceCellRow; }
        set { m_sourceCellRow = value; }
      }

      /// <summary>
      /// target cell column
      /// </summary>
      [XmlAttribute("targetColumn")]
      public int TargetCellColumn {
        get { return m_targetCellColumn; }
        set { m_targetCellColumn = value; }
      }
      
      /// <summary>
      /// target cell row
      /// </summary>
      [XmlAttribute("targetRow")]
      public int TargetCellRow {
        get { return m_targetCellRow; }
        set { m_targetCellRow = value; }
      }
      
      /// <summary>
      /// target cell old value (to undo)
      /// </summary>
      [XmlIgnore]
      public object OldCellValue {
        get { return m_oldCellValue; }
        set { m_oldCellValue = value; }
      }
      
      #endregion Getters / Setters
      
      #region Methods
      /// <summary>
      /// to undo, recover savec cell value for target cell
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        dataGridMod.GridView.Rows[TargetCellRow].Cells[TargetCellColumn].Value = this.OldCellValue;
      }
      
      /// <summary>
      /// if column/row index correspond to cells for target and source,
      /// change target cell value to source cell value and remember target cell old value
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay) {
        if (!((SourceCellRow < dataGridMod.GridView.Rows.Count )
              && (SourceCellColumn < dataGridMod.GridView.Rows[SourceCellRow].Cells.Count)
              && (TargetCellRow < dataGridMod.GridView.Rows.Count )
              && (TargetCellColumn < dataGridMod.GridView.Rows[TargetCellRow].Cells.Count)))
          return false;
        
        DataGridViewCell sourceCell =
          dataGridMod.GridView.Rows[SourceCellRow].Cells[SourceCellColumn];
        DataGridViewCell targetCell =
          dataGridMod.GridView.Rows[TargetCellRow].Cells[TargetCellColumn];
        this.OldCellValue = targetCell.Value;
        targetCell.Value = sourceCell.Value;
        return true;
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable() {
        return true;
      }
      
      #endregion // Methods
    }
    
    /// <summary>
    /// remove all columns and rows which do not
    /// contain an "interesting" cell value (null, empty string ...)
    /// </summary>
    [XmlType("PruneEvent")]
    public class PruneEvent : GridModificationEvent {
      
      /// <summary>
      /// class used to memorize a column index / structure
      /// (for undoing column deletion)
      /// </summary>
      public class ColumnInfo {

        int m_index;
        DataGridViewColumn m_column;
        
        /// <summary>
        /// column index
        /// </summary>
        public int Index { get { return m_index; }  set { m_index = value; } }
        
        /// <summary>
        /// column
        /// </summary>
        public DataGridViewColumn Column {
          get { return m_column; }
          set { m_column = value; }
        }
      }
      
      #region Members
      /// <summary>
      /// list of deleted columns (in fact column info)
      /// </summary>
      IList<ColumnInfo> m_deletedColumns = new List<ColumnInfo>();
      
      /// <summary>
      /// list of deleted row index
      /// </summary>
      IList<int> m_deletedRows = new List<int>();
      
      #endregion // Members
      
      #region Getters/Setters
      
      /// <summary>
      /// deleted columns
      /// </summary>
      [XmlIgnore]
      public IList<ColumnInfo> DeletedColumns {
        get { return m_deletedColumns; }
        set { m_deletedColumns = value; }
      }

      /// <summary>
      /// deleted rows
      /// </summary>
      [XmlIgnore]
      public IList<int> DeletedRows {
        get { return m_deletedRows; }
        set { m_deletedRows = value; }
      }
      #endregion // Getters/Setters
      
      #region Methods
      /// <summary>
      /// to undo, restore columns and rows
      /// in reverse of the deletion order
      /// Note that cell contents are restored
      /// to an empty string and not to their
      /// previous values (which may be null etc)
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod)
      {
        int i;
        ColumnInfo[] reverseColumnInfo = new ColumnInfo[DeletedColumns.Count];
        i = 0;
        foreach(ColumnInfo columnInfo in DeletedColumns) {
          reverseColumnInfo[DeletedColumns.Count - i - 1] = columnInfo;
          i++;
        }

        foreach (ColumnInfo columnInfo in reverseColumnInfo) {
          dataGridMod.GridView.Columns.Insert(columnInfo.Index, columnInfo.Column);
        }

        int[] reverseIndex = new int[DeletedRows.Count];
        i = 0;
        foreach(int rowIndex in DeletedRows) {
          reverseIndex[DeletedRows.Count - i - 1] = rowIndex;
          i++;
        }

        foreach(int rowIndex in reverseIndex) {
          dataGridMod.GridView.Rows.Insert(rowIndex, new string[dataGridMod.GridView.Columns.Count]);
        }
      }
      
      /// <summary>
      /// always undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable() {
        return true;
      }
      
      /// <summary>
      /// check whether a cell should be considered
      /// as interesting
      /// </summary>
      /// <param name="cell"></param>
      /// <returns></returns>
      private bool isCellNonEmpty(DataGridViewCell cell) {
        
        return ((cell != null) &&
                !(cell.Value is DBNull) &&
                !(cell.Value == null) &&
                ((string) cell.Value != ""));
      }
      
      /// <summary>
      /// perform uninteresting/empty rows and columns deletion
      /// and remembers enough information to allow undoing
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay) {
        // prune out empty rows
        IList<DataGridViewRow> rowsToDelete = new List<DataGridViewRow>();
        foreach(DataGridViewRow row in dataGridMod.GridView.Rows) {
          bool foundNonEmptyCell = false;
          int cellIndex = 0;
          while ((!foundNonEmptyCell) && (cellIndex < row.Cells.Count)) {
            DataGridViewCell cell = row.Cells[cellIndex];
            if (isCellNonEmpty(cell)) foundNonEmptyCell = true;
            else cellIndex++;
          }
          if (!foundNonEmptyCell) rowsToDelete.Add(row);
        }
        
        foreach(DataGridViewRow row in rowsToDelete) {
          DeletedRows.Add(row.Index);
          dataGridMod.GridView.Rows.Remove(row);
        }
        
        // prune out empty columns
        IList<DataGridViewColumn> columnsToDelete = new List<DataGridViewColumn>();
        foreach(DataGridViewColumn column in dataGridMod.GridView.Columns) {
          bool foundNonEmptyCell = false;
          int cellIndex = 0;
          while((!foundNonEmptyCell) && (cellIndex < dataGridMod.GridView.Rows.Count)) {
            DataGridViewCell cell = dataGridMod.GridView.Rows[cellIndex].Cells[column.Index];
            if (isCellNonEmpty(cell)) foundNonEmptyCell = true;
            else cellIndex++;
          }
          if (!foundNonEmptyCell) columnsToDelete.Add(column);
        }
        
        foreach (DataGridViewColumn column in columnsToDelete) {
          ColumnInfo columnInfo = new ColumnInfo();
          columnInfo.Index = column.Index;
          columnInfo.Column = column;
          DeletedColumns.Add(columnInfo);
          dataGridMod.GridView.Columns.Remove(column);
        }
        
        return ((columnsToDelete.Count > 0) || (rowsToDelete.Count > 0));
      }
      
      #endregion // Methods
    }

    /// <summary>
    /// event for column shifting (one cell upwards or downwards)
    /// </summary>
    [XmlType("ShiftColumnEvent")]
    public class ShiftColumnEvent : GridModificationEvent {
      
      /// <summary>
      /// inverse of a shift action function prototype
      /// </summary>
      public delegate void inverse(DataGridView dataGridView, DataGridViewCell cell) ;
      
      #region Members
      int m_index;
      bool m_direction;
      DataGridViewCell m_removedCell;
      inverse m_inverseHandler;
      #endregion // Members
      
      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public ShiftColumnEvent() {}
      
      /// <summary>
      /// shift of column with index <paramref name="col" />
      /// - upwards if <paramref name="direction" /> is true
      /// - else downwards
      /// </summary>
      /// <param name="col"></param>
      /// <param name="direction"></param>
      public ShiftColumnEvent(int col, bool direction) {
        this.Index = col;
        this.Direction = direction;
      }
      #endregion // Constructors
      
      #region Getters/Setters
      /// <summary>
      /// shifted column index
      /// </summary>
      [XmlAttribute("column")]
      public int Index {
        get { return m_index; }
        set { m_index = value; }
      }
      
      /// <summary>
      /// shifted column direction (true is up, false is down)
      /// </summary>
      [XmlAttribute("direction")]
      public bool Direction {
        get { return m_direction; }
        set { m_direction = value; }
      }
      
      /// <summary>
      /// shifted-out cell (memorized for undoing)
      /// </summary>
      [XmlIgnore]
      public DataGridViewCell RemovedCell {
        get { return m_removedCell; }
        set { m_removedCell = value; }
      }
      #endregion // Getters/Setters
      
      #region Methods
      
      /// <summary>
      /// function to call in order to inverse a shift
      /// </summary>
      [XmlIgnore]
      public inverse InverseHandler {
        get { return m_inverseHandler; }
        set { m_inverseHandler = value; }
      }
      
      /// <summary>
      /// to undo a shift, call the corresponding handler with
      /// relevant arguments
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        InverseHandler(dataGridMod.GridView, RemovedCell);
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable()
      {
        return true;
      }
      
      /// <summary>
      /// shift the selected column down
      /// the shifted-in cell value is the value of <paramref name="cell" />
      /// which may be null (in this case the shifted-in cell value is an empty string)
      /// </summary>
      /// <param name="dataGridView"></param>
      /// <param name="cell"></param>
      void shiftDown(DataGridView dataGridView, DataGridViewCell cell) {
        for(int i = dataGridView.Rows.Count-1 ; i > 0 ; i--)
          dataGridView.Rows[i].Cells[Index].Value = dataGridView.Rows[i-1].Cells[Index].Value;
        dataGridView.Rows[0].Cells[Index].Value = (cell == null ? "" : cell.Value);
      }

      /// <summary>
      /// shift the selected column down
      /// the shifted-in cell value is the value of <paramref name="cell" />
      /// which may be null (in this case the shifted-in cell value is an empty string)
      /// </summary>
      /// <param name="dataGridView"></param>
      /// <param name="cell"></param>
      void shiftUp(DataGridView dataGridView, DataGridViewCell cell) {
        for(int i = 0 ; i < dataGridView.Rows.Count-1 ; i++)
          dataGridView.Rows[i].Cells[Index].Value = dataGridView.Rows[i+1].Cells[Index].Value;
        dataGridView.Rows[dataGridView.Rows.Count-1].Cells[Index].Value =
          (cell == null ? "" : cell.Value);
      }
      
      /// <summary>
      /// perform a column shift (if selected column exists)
      /// and memorizes enough information (shifted-out cell content) to undo it
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay) {
        if (Direction)
        {
          if (!((0 < dataGridMod.GridView.Rows.Count)
                && (Index < dataGridMod.GridView.Rows[0].Cells.Count)))
            return false;

          DataGridViewCell shiftCell = dataGridMod.GridView.Rows[0].Cells[Index];
          DataGridViewCell memCell = (DataGridViewCell) shiftCell.Clone();
          memCell.Value = shiftCell.Value;
          shiftUp(dataGridMod.GridView, null);
          this.RemovedCell = memCell;
          this.InverseHandler = this.shiftDown;
        } else {
          if (!((0 < dataGridMod.GridView.Rows.Count)
                && (Index < dataGridMod.GridView.Rows[dataGridMod.GridView.Rows.Count-1].Cells.Count)))
            return false;
          
          DataGridViewCell shiftCell =
            dataGridMod.GridView.Rows[dataGridMod.GridView.Rows.Count-1].Cells[Index];
          DataGridViewCell memCell = (DataGridViewCell) shiftCell.Clone();
          memCell.Value = shiftCell.Value;
          shiftDown(dataGridMod.GridView, null);
          this.RemovedCell = memCell;
          this.InverseHandler = this.shiftUp;
        }
        return true;
      }
      #endregion  // Methods
    }
    
    /// <summary>
    /// event for column deletion
    /// </summary>
    [XmlType("DeleteColumnEvent")]
    public class DeleteColumnEvent : GridModificationEvent {
      
      #region Members
      int m_index;
      DataGridViewColumn m_column;
      object[] m_cellValues;
      #endregion // Members
      
      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public DeleteColumnEvent() {}
      
      /// <summary>
      /// build a delete column event for column index <paramref name="colIndex" />
      /// </summary>
      /// <param name="colIndex"></param>
      public DeleteColumnEvent(int colIndex) {
        this.Index = colIndex;
      }
      #endregion // Constructors
      
      #region Getters/Setters
      /// <summary>
      /// deleted column index
      /// </summary>
      [XmlAttribute("column")]
      public int Index {
        get { return m_index; }
        set { m_index = value; }
      }
      /// <summary>
      /// deleted column logical information
      /// </summary>
      [XmlIgnore]
      public DataGridViewColumn Column {
        get { return m_column; }
        set { m_column = value; }
      }
      
      /// <summary>
      /// deleted column cell values
      /// need to be memorized since a DataGridViewColumn
      /// (in opposition with a DataGridViewRow)
      /// is a logical object which does not contain
      /// cell content information
      /// </summary>
      [XmlIgnore]
      public object[] CellValues {
        get { return m_cellValues; }
        set { m_cellValues = value; }
      }
      #endregion // Getters/Setters
      
      #region Methods
      
      /// <summary>
      /// to undo a column deletion
      /// insert the (logical) memorized column
      /// *and* update its memorized content
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        int i = 0;
        dataGridMod.GridView.Columns.Insert(Index, Column);
        foreach(object cell in this.CellValues) {
          dataGridMod.GridView.Rows[i].Cells[Index].Value = cell;
          i++;
        }
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable()
      {
        return true;
      }
      
      /// <summary>
      /// first memorize the column logical structure *and* content
      /// then remove it from grid
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay)
      {
        if (!(Index < dataGridMod.GridView.Columns.Count))
          return false;
        
        DataGridViewColumn column = dataGridMod.GridView.Columns[Index];
        this.CellValues = new object[dataGridMod.GridView.Rows.Count];
        
        for(int i = 0 ; i < dataGridMod.GridView.Rows.Count ; i++) {
          DataGridViewCell cell = dataGridMod.GridView.Rows[i].Cells[column.Index];
          this.CellValues[i] = (cell == null ? "" : cell.Value);
        }
        
        dataGridMod.GridView.Columns.Remove(column);
        this.Column = column;
        return true;
      }
      #endregion // Methods
    }
    
    /// <summary>
    /// event for column inserttion
    /// </summary>
    [XmlType("InsertColumnEvent")]
    public class InsertColumnEvent : GridModificationEvent {
      
      #region Members
      int m_index;
      #endregion // Members

      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public InsertColumnEvent() {}
      
      /// <summary>
      /// build an insert column event for column index <paramref name="colIndex" />
      /// </summary>
      /// <param name="colIndex"></param>
      public InsertColumnEvent(int colIndex) {
        this.Index = colIndex;
      }
      #endregion // Constructors

      #region Getters/Setters
      /// <summary>
      /// inserted column index
      /// </summary>
      [XmlAttribute("column")]
      public int Index {
        get { return m_index; }
        set { m_index = value; }
      }
      #endregion // Getters/Setters

      #region Methods
      
      /// <summary>
      /// undo a column insertion
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        dataGridMod.GridView.Columns.RemoveAt(Index);
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable()
      {
        return true;
      }
      
      /// <summary>
      /// insert an empty column in grid
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay)
      {
        if (!(Index <= dataGridMod.GridView.Columns.Count))
          return false;
        DataGridViewColumn insertedColumn = new DataGridViewColumn();
        insertedColumn.CellTemplate = new DataGridViewTextBoxCell();
        dataGridMod.GridView.Columns.Insert(Index, insertedColumn);
        
        return true;
      }
      #endregion // Methods
      
    }
    
    /// <summary>
    /// event for row deletion
    /// </summary>
    [XmlType("DeleteRowEvent")]
    public class DeleteRowEvent : GridModificationEvent {
      
      #region Members
      int m_index;
      object[] m_cellValues;
      #endregion // Members
      
      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public DeleteRowEvent() {}
      
      /// <summary>
      /// build a row deletion event on row index <paramref name="rowIndex" />
      /// </summary>
      /// <param name="rowIndex"></param>
      public DeleteRowEvent(int rowIndex) {
        this.Index = rowIndex;
      }
      #endregion // Constructors
      
      #region Getters/Setters
      /// <summary>
      /// deleted row index
      /// </summary>
      [XmlAttribute("row")]
      public int Index {
        get { return m_index; }
        set { m_index = value; }
      }
      
      /// <summary>
      /// deleted row values
      /// </summary>
      [XmlIgnore]
      public object[] CellValues {
        get { return m_cellValues; }
        set { m_cellValues = value; }
      }
      #endregion // Getters/Setters
      
      #region Members
      /// <summary>
      /// to undo, insert the row at the memorized row index
      /// using the memorized cell values
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        dataGridMod.GridView.Rows.Insert(this.Index, this.CellValues);
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable()
      {
        return true;
      }
      
      /// <summary>
      /// memorize row index and content
      /// if <paramref name="replay" /> is set to true,
      /// do the actual row grid deletion
      /// otherwise it should be performed automatically
      /// (the user manually deletes the row from the grid)
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay)
      {
        if (!(Index < dataGridMod.GridView.Rows.Count))
          return false;
        
        DataGridViewRow row = dataGridMod.GridView.Rows[Index];
        object[] cellValues = new object[row.Cells.Count];
        
        for(int i = 0 ; i < row.Cells.Count ; i++) {
          cellValues[i] = row.Cells[i].Value;
        }

        this.CellValues = cellValues;
        
        // row removal only performed on replay
        // ("user" removal performed automatically)
        
        if (replay) dataGridMod.GridView.Rows.RemoveAt(Index);
        
        return true;
      }
      #endregion // Members
    }
    
    /// <summary>
    /// event for row insertion
    /// </summary>
    [XmlType("InsertRowEvent")]
    public class InsertRowEvent : GridModificationEvent {
      
      #region Members
      int m_index;
      
      #endregion // Members
      
      #region Constructors
      /// <summary>
      /// default constructor
      /// </summary>
      public InsertRowEvent() {}
      
      /// <summary>
      /// build a row insertion event on row index <paramref name="rowIndex" />
      /// </summary>
      /// <param name="rowIndex"></param>
      public InsertRowEvent(int rowIndex) {
        this.Index = rowIndex;
      }
      #endregion // Constructors
      
      #region Getters/Setters
      /// <summary>
      /// inserted row index
      /// </summary>
      [XmlAttribute("row")]
      public int Index {
        get { return m_index; }
        set { m_index = value; }
      }
      #endregion // Getters/Setters
      
      #region Members
      /// <summary>
      /// to undo, delete the row at the memorized row index
      /// </summary>
      /// <param name="dataGridMod"></param>
      public override void undo(DataGridModifier dataGridMod) {
        dataGridMod.GridView.Rows.RemoveAt(this.Index);
      }
      
      /// <summary>
      /// it's undoable
      /// </summary>
      /// <returns></returns>
      public override bool isUndoable()
      {
        return true;
      }
      
      /// <summary>
      /// memorize row index and create new row content
      /// </summary>
      /// <param name="dataGridMod"></param>
      /// <param name="replay"></param>
      /// <returns></returns>
      public override bool perform(DataGridModifier dataGridMod, bool replay)
      {
        if (!(Index < dataGridMod.GridView.Rows.Count))
          return false;
        
        DataGridViewRow row = new DataGridViewRow();
        
        string[] cellValues = new string[dataGridMod.GridView.Columns.Count];
        
        for(int i = 0 ; i < row.Cells.Count ; i++) {
          cellValues[i] = "";
        }
        
        dataGridMod.GridView.Rows.Insert(Index, cellValues);

        return true;
      }
      #endregion // Members
      
    }
    
    #region Constructors
    /// <summary>
    /// Grid modification event default constructor
    /// </summary>
    public GridModificationEvent () {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// undo grid modification event
    /// </summary>
    /// <param name="dataGridModifier"></param>
    public abstract void undo(DataGridModifier dataGridModifier);
    
    /// <summary>
    /// check whether grid modification event can be undone
    /// </summary>
    /// <returns></returns>
    public abstract bool isUndoable();
    
    /// <summary>
    /// perform grid modification
    /// </summary>
    /// <param name="dataGridModifier"></param>
    /// <param name="replay"></param>
    /// <returns></returns>
    public abstract bool perform(DataGridModifier dataGridModifier, bool replay);
    #endregion // Methods
  }
}
