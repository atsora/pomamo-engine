// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// DataGrid modifications (and undo managament)
  /// DO NOT USE this with a DataGridView which has
  /// property AllowUserToAddRows set to true
  /// since the code below may try to delete
  /// the last row and this won't work with
  /// the user controlled row
  /// </summary>
  [XmlRoot("DataGridModifier")]
  [XmlInclude(typeof(GridModificationEvent)), XmlInclude(typeof(GridSelectionEvent))]
  public class DataGridModifier
  {
    #region Members
    DataGridView m_dataGridView;
    IDictionary<string, GridSelectionEvent> m_regions = new Dictionary<string, GridSelectionEvent>();
    Stack<GridModificationEvent> m_stack = new Stack<GridModificationEvent>();
    ISet<IDataGridModifierObserver> m_observers = new HashSet<IDataGridModifierObserver>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DataGridModifier).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// target DataGridView
    /// </summary>
    [XmlIgnore]
    public DataGridView GridView {
      get { return m_dataGridView; }
      set { m_dataGridView = value; }
    }
    
    /// <summary>
    /// stack of registered modification events
    /// for macro / undo
    /// </summary>
    [XmlIgnore]
    public Stack<GridModificationEvent> EventStack {
      get { return m_stack; }
      set { m_stack = value; }
    }

    /// <summary>
    /// map from region names to regions
    /// </summary>
    [XmlIgnore]
    public IDictionary<string, GridSelectionEvent> Regions {
      get { return m_regions; }
      set { m_regions = value; }
    }
    
    /// <summary>
    /// used for XML serialization of regions
    /// </summary>
    [XmlArray("RegionArray")]
    [XmlArrayItem("Region")]
    public GridSelectionEvent[] RegionArray {
      get {
        // inverse order of ToArray
        GridSelectionEvent[] regionArray = new GridSelectionEvent[Regions.Count];
        int i = 0;
        foreach(KeyValuePair<string, GridSelectionEvent> nameRegionPair in Regions) {
          regionArray[i] = nameRegionPair.Value;
          i++;
        }
        return regionArray;
      }
      
      set {
        GridSelectionEvent[] regionArray = (GridSelectionEvent[]) value;
        foreach (GridSelectionEvent region in regionArray)
          Regions.Add(region.RegionName, region);
      }
    }

    /// <summary>
    /// used for XML serialization of event stack
    /// </summary>
    [XmlArray("ModificationArray")]
    [XmlArrayItem("Modification")]
    public GridModificationEvent[] EventArray {
      get {
        // inverse order of ToArray
        GridModificationEvent[] evtArray = new GridModificationEvent[m_stack.Count];
        int i = evtArray.Length-1;
        foreach (GridModificationEvent evt in m_stack) {
          evtArray[i] = evt;
          i--;
        }
        return evtArray;
      }
      set {
        GridModificationEvent[] eventArray = (GridModificationEvent[]) value;
        foreach (GridModificationEvent evt in eventArray)
          EventStack.Push(evt);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DataGridModifier () {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Checks whether there is an event to undo
    /// </summary>
    public bool SomethingToUndo() {
      return EventStack.Count > 0;
    }
    
    /// <summary>
    /// Undo last registered event
    /// </summary>
    public void Undo() {
      if (EventStack.Count > 0) {
        GridModificationEvent gridModificationEvent = EventStack.Peek();
        if (gridModificationEvent.isUndoable()) {
          EventStack.Pop();
          this.invalidateRegionSelections();
          gridModificationEvent.undo(this);
        } else MessageBox.Show("Cannot undo any further");
      }
    }
    
    /// <summary>
    /// undo all registered event
    /// </summary>
    public void UndoAll() {
      // TODO: ask for confirmation
      GridModificationEvent gridModificationEvent;
      this.invalidateRegionSelections();
      while((EventStack.Count >0) && (gridModificationEvent = EventStack.Peek()).isUndoable()) {
        EventStack.Pop();
        gridModificationEvent.undo(this);
      }
    }
    
    private void invalidateRegionSelections() {
      this.Regions = new Dictionary<string, GridSelectionEvent>();
      NotifyInvalidate(); // notify observers of region selections invalidation
    }

    /// <summary>
    /// Add observer for region selection invalidation
    /// </summary>
    ///
    public void AddInvalidateRegionObserver(IDataGridModifierObserver observer) {
      m_observers.Add(observer);
    }
    
    /// <summary>
    /// Notify observers of a region selection invalidation
    /// </summary>
    void NotifyInvalidate() {
      foreach(IDataGridModifierObserver observer in m_observers) {
        observer.InvalidateRegionSelections();
      }
    }
    
    private static void getRectangularCellSelection(DataGridViewSelectedCellCollection selectedCells,
                                                    out int minRowIndex, out int maxRowIndex,
                                                    out int minColumnIndex, out int maxColumnIndex)
    {
      DataGridViewCell cell = selectedCells[0];
      minRowIndex = maxRowIndex = cell.RowIndex;
      minColumnIndex = maxColumnIndex = cell.ColumnIndex;
      for(int i = 1 ; i < selectedCells.Count ; i++) {
        DataGridViewCell currentCell = selectedCells[i];
        int currentRowIndex = currentCell.RowIndex;
        int currentColumnIndex = currentCell.ColumnIndex;
        if (currentRowIndex > maxRowIndex) maxRowIndex = currentRowIndex;
        if (currentRowIndex < minRowIndex) minRowIndex = currentRowIndex;
        if (currentColumnIndex > maxColumnIndex) maxColumnIndex = currentColumnIndex;
        if (currentColumnIndex < minColumnIndex) minColumnIndex = currentColumnIndex;
      }
    }
    
    /// <summary>
    /// select a cell range as a region with a given name
    /// </summary>
    public GridSelectionEvent SelectRegion(string name) {
      int selectedCellCount = GridView.GetCellCount(DataGridViewElementStates.Selected);
      if (selectedCellCount >= 1)  {
        DataGridViewSelectedCellCollection selectedCells = GridView.SelectedCells;
        if ((selectedCells != null)) {
          int minRowIndex, maxRowIndex , minColumnIndex, maxColumnIndex;
          getRectangularCellSelection(selectedCells, out minRowIndex, out maxRowIndex,
                                      out minColumnIndex, out maxColumnIndex);
          GridSelectionEvent gridSelectionEvent =
            new GridSelectionEvent(name, minRowIndex, maxRowIndex,
                                   minColumnIndex, maxColumnIndex);
          this.Regions.Remove(name);
          this.Regions.Add(name, gridSelectionEvent);
          return gridSelectionEvent;
        }
      }
      return null;
    }
    
    /// <summary>
    /// shift a column upwards (and register it)
    /// </summary>
    /// <param name="colIndex">index of colum</param>
    public void ShiftUpColumn(int colIndex)
    {
      GridModificationEvent.ShiftColumnEvent shiftColumnEvent =
        new GridModificationEvent.ShiftColumnEvent(colIndex, true);
      
      if (shiftColumnEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(shiftColumnEvent);
      }
    }
    
    /// <summary>
    /// shift a column downwards (and register it)
    /// </summary>
    /// <param name="colIndex">index of colum</param>
    public void ShiftDownColumn(int colIndex)
    {
      GridModificationEvent.ShiftColumnEvent shiftColumnEvent =
        new GridModificationEvent.ShiftColumnEvent(colIndex, false);
      if (shiftColumnEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(shiftColumnEvent);
      }
    }
    
    /// <summary>
    /// delete column at given index (and register it)
    /// </summary>
    /// <param name="colIndex">index of column</param>
    public void DeleteColumn(int colIndex)
    {
      GridModificationEvent.DeleteColumnEvent deleteColumnEvent =
        new GridModificationEvent.DeleteColumnEvent(colIndex);
      if (deleteColumnEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(deleteColumnEvent);
      }
    }

    /// <summary>
    /// insert column at given index (and register it)
    /// </summary>
    /// <param name="colIndex">index of column</param>
    public void InsertColumn(int colIndex)
    {
      GridModificationEvent.InsertColumnEvent insertColumnEvent = 
        new GridModificationEvent.InsertColumnEvent(colIndex);
      
      if (insertColumnEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(insertColumnEvent);
      }
    }

    /// <summary>
    /// insert row at given index (and register it)
    /// </summary>
    /// <param name="rowIndex">index of row</param>
    public void InsertRow(int rowIndex)
    {
      GridModificationEvent.InsertRowEvent insertRowEvent =
        new GridModificationEvent.InsertRowEvent(rowIndex);
      if (insertRowEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(insertRowEvent);
      }
    }

    
    /// <summary>
    /// prune out all empty rows and columns (and register it)
    /// </summary>
    public void PruneRowCol()
    {
      GridModificationEvent.PruneEvent pruneEvent = new GridModificationEvent.PruneEvent();
      if (pruneEvent.perform(this, false)) {
        invalidateRegionSelections();
        EventStack.Push(pruneEvent);
      }
    }

    /// <summary>
    /// register a row deletion
    /// only performs actual row deletion if <paramref name="replay"/> is true
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="replay"></param>
    public void DeleteRow(int rowIndex, bool replay)
    {
      GridModificationEvent.DeleteRowEvent deleteRowEvent =
        new GridModificationEvent.DeleteRowEvent(rowIndex);
      if (deleteRowEvent.perform(this, replay)) {
        invalidateRegionSelections();
        EventStack.Push(deleteRowEvent);
      }
    }
    
    /// <summary>
    /// set cell value (and register it)
    /// </summary>
    public void SetCellValue(int cellRow, int cellColumn, string input)
    {
      GridModificationEvent.ChangeEvent ev =
        new GridModificationEvent.ChangeEvent(cellColumn,
                                              cellRow,
                                              input);
      if (ev.perform(this, false)) EventStack.Push(ev);
      
    }
    
    /// <summary>
    /// set cell value (and register it)
    /// </summary>
    public void CopyToCell(int sourceCellRow, int sourceCellColumn,
                           int targetCellRow, int targetCellColumn)
    {
      GridModificationEvent.CopyToEvent ev =
        new GridModificationEvent.CopyToEvent(sourceCellRow, sourceCellColumn,
                                              targetCellRow, targetCellColumn);
      if (ev.perform(this, false)) EventStack.Push(ev);
    }
    
    /// <summary>
    /// Load and play a macro from file <paramref name="fileName" />
    /// </summary>
    /// <param name="fileName"></param>
    public void LoadMacro(string fileName) {
      XmlSerializer xs = new XmlSerializer(typeof(DataGridModifier));
      DataGridModifier dgm;
      using (StreamReader read = new StreamReader(fileName))
      {
        dgm = (DataGridModifier) xs.Deserialize(read);
      }
      
      foreach(GridModificationEvent ev in dgm.EventArray) {
        if (ev.perform(this, true)) {
          EventStack.Push(ev);
        }
      }
      
      // set regions at end once dataGridView has been transformed
      this.Regions.Clear();
      foreach(KeyValuePair<string, GridSelectionEvent> namedRegion in dgm.Regions) {
        if (regionWithinBounds(namedRegion.Value)) this.Regions.Add(namedRegion);
      }

    }
    
    private bool regionWithinBounds(GridSelectionEvent region)
    {
      return ((region.MaxRow < GridView.Rows.Count)
              && (region.MaxColumn < GridView.Columns.Count));
    }
    
    /// <summary>
    /// Save current stack of grid modifications as
    /// a macro in target file <paramref name="fileName" />
    /// </summary>
    /// <param name="fileName"></param>
    public void SaveMacro(string fileName)
    {
      XmlSerializer xs = new XmlSerializer(typeof(DataGridModifier));
      using (StreamWriter wr = new StreamWriter(fileName))
      {
        xs.Serialize(wr, this);
      }
    }
    
    /// <summary>
    /// load a set of regions (their name must be in regionsToLoad
    /// when this argument is not null)
    /// </summary>
    /// <param name="regions"></param>
    /// <param name="regionsToLoad"></param>
    public void LoadRegions(IList<GridSelectionEvent> regions, string[] regionsToLoad) {
      foreach (GridSelectionEvent region in regions) {
        bool shouldAdd = true;
        if (regionsToLoad != null) {
          shouldAdd = false;
          for(int i = 0 ; i < regionsToLoad.Length ; i++) {
            if (regionsToLoad[i] == region.RegionName) { shouldAdd = true; break; }
          }
        }
        if ((shouldAdd) && (regionWithinBounds(region)))
          Regions.Add(new KeyValuePair<string, GridSelectionEvent>(region.RegionName, region));
      }
    }
    #endregion // Methods
  }
}
