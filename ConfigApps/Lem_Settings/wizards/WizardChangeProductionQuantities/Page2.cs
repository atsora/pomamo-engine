// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Members
    Quantities m_quantities = null;
    bool m_preparation = true;
    IList<int> m_copyRow = new List<int>();
    IList<int> m_copyColumn = new List<int>();
    ILine m_line = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Quantities of parts to produce"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "You can change here the different quantities of parts to produce. " +
          "An empty value means that you selected several periods and different numbers of parts were found.\n\n" +
          "By default, only shifts involved in the periods are displayed. If additional shifts are desired, " +
          "first check \"Display all shifts\" and then fill a row with quantities.\nTo remove them, simply set " +
          "all quantities of a row to 0.\n\n" +
          "To save time, convenient functions are available in the menus associated to each row and each column."; } }
    
    IList<IComponentIntermediateWorkPiece> Ciwps { get; set; }
    IList<int> DayNumbers { get; set; }
    IList<IShift> Shifts { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
    {
      InitializeComponent();
      
      Ciwps = new List<IComponentIntermediateWorkPiece>();
      DayNumbers = new List<int>();
      Shifts = new List<IShift>();
      
      // Menu in the vertical header
      IList<string> menu = new List<string>();
      menu.Add("Copy");
      menu.Add("Paste");
      menu.Add("Duplicate to green rows");
      menu.Add("Cancel changes");
      menu.Add("Set to 0");
      scrollTable.VerticalMenu = menu;
      
      // Menu in the horizontal header
      IList<String> horizontalMenu = new List<String>();
      horizontalMenu.Add("Copy");
      horizontalMenu.Add("Paste");
      horizontalMenu.Add("Duplicate column");
      horizontalMenu.Add("Cancel changes");
      horizontalMenu.Add("Set to 0");
      scrollTable.HorizontalMenu = horizontalMenu;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_line = data.Get<ILine>(Item.LINE);
      m_quantities = data.Get<Quantities>(Item.QUANTITIES);
      
      bool withShift;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        withShift = (ModelDAOHelper.DAOFactory.ShiftDAO.FindAll().Count > 0);
      }

      if (!withShift) {
        m_quantities.DisplayAllShifts = true;
        checkDispAllShifts.Hide();
        horizontalBar.Hide();
        baseLayout.RowStyles[1].Height = 0;
        baseLayout.RowStyles[2].Height = 0;
      } else {
        checkDispAllShifts.Show();
        horizontalBar.Show();
        baseLayout.RowStyles[1].Height = 2;
        baseLayout.RowStyles[2].Height = 24;
      }
      
      using (new SuspendDrawing(scrollTable)) {
        FillTable ();
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Everything already stored in QUANTITIES
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      data.Get<Quantities>(Item.QUANTITIES).GetErrors(ref errors);
      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      if (!data.Get<Quantities>(Item.QUANTITIES).GetSumPerOperation().HasValue) {
        warnings.Add("The total number of parts produced is not the same for all operations.");
      }

      if (data.Get<Quantities>(Item.QUANTITIES).HasConflict(data.Get<ILine>(Item.LINE))) {
        warnings.Add("Some shifts or days will be used for several production periods.");
      }

      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      return data.Get<Quantities>(Item.QUANTITIES).GetSummary();
    }
    #endregion // Page methods
    
    #region Private methods
    void FillTable()
    {
      m_preparation = true;
      
      // Vertical header
      DayNumbers.Clear();
      Shifts.Clear();
      IDictionary<string, IList<string>> verticalHeader = new Dictionary<string, IList<string>>();
      
      bool withShift = false;
      IDictionary<string, IList<RowQuantities>> quantities = m_quantities.GetQuantities(m_line);
      int dayNumber = 0;
      foreach (string day in quantities.Keys) {
        IList<string> listTmp = new List<string>();
        foreach (RowQuantities rowQuantities in quantities[day]) {
          listTmp.Add(rowQuantities.ShiftName);
          DayNumbers.Add(dayNumber);
          Shifts.Add(rowQuantities.Shift);
          withShift |= (rowQuantities.Shift != null);
        }
        
        verticalHeader[day] = listTmp;
        dayNumber++;
      }
      
      // Horizontal header
      Ciwps.Clear();
      IList<string> horizontalHeader = new List<string>();
      foreach (IComponentIntermediateWorkPiece ciwp in m_quantities.ComponentIntermediateWorkPieces.Keys) {
        Ciwps.Add(ciwp);
        horizontalHeader.Add(m_quantities.ComponentIntermediateWorkPieces[ciwp]);
      }
      
      // Apply headers
      scrollTable.InitTable(verticalHeader, horizontalHeader);
      scrollTable.SecondVerticalHeaderWidth = withShift ? 80 : 0;
      
      // Fill with quantities
      if (verticalHeader.Count > 0 && horizontalHeader.Count > 0) {
        int row = 0;
        foreach (string day in quantities.Keys) {
          foreach (RowQuantities rowQuantity in quantities[day]) {
            for (int col = 0; col < Ciwps.Count; col++) {
              if (rowQuantity.Quantities.ContainsKey(Ciwps[col])) {
                if (rowQuantity.ModifiedStatus[Ciwps[col]]) {
                  // Modified value
                  scrollTable.SetValue(row, col, rowQuantity.Quantities[Ciwps[col]]);
                  scrollTable.SetBold(row, col, true);
                } else {
                  // Original value, which can be nothing if several values were found
                  if (rowQuantity.DifferentValues[Ciwps[col]]) {
                    scrollTable.ResetValue(row, col);
                  }
                  else {
                    scrollTable.SetValue(row, col, rowQuantity.Quantities[Ciwps[col]]);
                  }
                }
                
                // Number of external targets
                if (rowQuantity.NumberOfExternalTargets > 0) {
                  AddTooltip (row, col);
                }
              }
              else {
                scrollTable.SetValue(row, col, 0);
              }
            }
            row++;
          }
        }
      }
      
      // Compute sums
      for (int col = 0; col < scrollTable.ColumnCount; col++) {
        ComputeSum (col);
      }

      m_preparation = false;
    }
    
    void CopyRow(int row)
    {
      m_copyRow.Clear();
      for (int i = 0; i < scrollTable.ColumnCount; i++) {
        m_copyRow.Add(scrollTable.GetValue(row, i));
      }
    }
    
    void PasteRow(int row)
    {
      int n = m_copyRow.Count;
      if (n > scrollTable.ColumnCount) {
        n = scrollTable.ColumnCount;
      }

      for (int i = 0; i < n; i++) {
        scrollTable.SetValue(row, i, m_copyRow[i]);
      }
    }
    
    void DuplicateRow(int row)
    {
      CopyRow(row);
      for (int i = 0; i < scrollTable.RowCount; i++) {
        // Same kind of shift?
        if (Object.Equals(Shifts[i], Shifts[row]) && i != row) {
          // Paste only if used
          for (int j = 0; j < scrollTable.ColumnCount; j++) {
            if (scrollTable.GetValue(i, j) != 0) {
              PasteRow(i);
              break;
            }
          }
        }
      }
    }
    
    void RemoveRowChanges(int row)
    {
      for (int i = 0; i < scrollTable.ColumnCount; i++) {
        m_quantities.DeleteModifications(DayNumbers[row], Shifts[row], Ciwps[i]);
      }

      using (new SuspendDrawing(scrollTable)) {
        FillTable ();
      }
    }
    
    void SetRowTo0(int row)
    {
      for (int i = 0; i < scrollTable.ColumnCount; i++) {
        scrollTable.SetValue(row, i, 0);
      }
    }
    
    void CopyColumn(int column)
    {
      m_copyColumn.Clear();
      for (int i = 0; i < scrollTable.RowCount; i++) {
        m_copyColumn.Add(scrollTable.GetValue(i, column));
      }
    }
    
    void PasteColumn(int column)
    {
      int n = m_copyColumn.Count;
      if (n > scrollTable.RowCount) {
        n = scrollTable.RowCount;
      }

      for (int i = 0; i < n; i++) {
        scrollTable.SetValue(i, column, m_copyColumn[i]);
      }
    }
    
    void DuplicateColumn(int column)
    {
      CopyColumn(column);
      for (int i = 0; i < scrollTable.ColumnCount; i++) {
        if (i != column) {
          PasteColumn (i);
        }
      }
    }
    
    void RemoveColumnChanges(int column)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        m_quantities.DeleteModifications(DayNumbers[i], Shifts[i], Ciwps[column]);
      }

      using (new SuspendDrawing(scrollTable)) {
        FillTable ();
      }
    }
    
    void SetColumnTo0(int column)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        scrollTable.SetValue(i, column, 0);
      }
    }
    
    void ComputeSum(int col)
    {
      int sum = 0;
      bool defined = true;
      for (int row = 0; row < scrollTable.RowCount; row++) {
        sum += scrollTable.GetValue(row, col);
        defined &= scrollTable.IsValueDefined(row, col);
      }
      
      if (defined) {
        scrollTable.SetFooterText(col, sum.ToString());
      }
      else {
        scrollTable.SetFooterText(col, "-");
      }
    }
    
    void AddTooltip(int row, int col)
    {
      if (scrollTable.GetValue(row, col) == 0) {
        scrollTable.SetTooltip(row, col, imageList.Images[0],
                               "this shift is used by another production period");
      }
      else {
        scrollTable.SetTooltip(row, col, imageList.Images[1],
                               "this shift will be used for several production periods");
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void CheckDispAllShiftsCheckedChanged(object sender, EventArgs e)
    {
      if (m_preparation) {
        return;
      }

      m_quantities.DisplayAllShifts = checkDispAllShifts.Checked;
      using (new SuspendDrawing(scrollTable)) {
        FillTable ();
      }
    }
    
    void ScrollTableCellChanged(int row, int col)
    {
      if (m_preparation) {
        return;
      }

      scrollTable.SetBold(row, col, true);
      if (scrollTable.HasToolTip(row, col)) {
        AddTooltip (row, col);
      }

      m_quantities.SetQuantity(DayNumbers[row], Shifts[row], Ciwps[col], scrollTable.GetValue(row, col));
      
      ComputeSum(col);
    }
    
    void ScrollTableHorizontalMenuClicked(int column, int numAction)
    {
      switch (numAction) {
          case 0: CopyColumn(column);          break;
          case 1: PasteColumn(column);         break;
          case 2: DuplicateColumn(column);     break;
          case 3: RemoveColumnChanges(column); break;
          case 4: SetColumnTo0(column);        break;
      }
    }
    
    void ScrollTableVerticalMenuClicked(int row, int numAction)
    {
      switch (numAction) {
          case 0: CopyRow(row);          break;
          case 1: PasteRow(row);         break;
          case 2: DuplicateRow(row);     break;
          case 3: RemoveRowChanges(row); break;
          case 4: SetRowTo0(row);        break;
      }
    }
    
    void ScrollTableVerticalMenuOpen(int row)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        // Same kind of shift?
        if (Object.Equals(Shifts[i], Shifts[row])) {
          // Highlight only if used
          for (int j = 0; j < scrollTable.ColumnCount; j++) {
            if (scrollTable.GetValue(i, j) != 0) {
              scrollTable.SetRowColor(i, Color.LightGreen);
              break;
            }
          }
        }
      }
    }
    
    void ScrollTableVerticalMenuClosed(int row)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        scrollTable.ResetRowColor(i);
      }
    }
    #endregion // Event reactions
  }
}
