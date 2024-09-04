// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  internal partial class Page3 : GenericWizardPage, IWizardPage
  {
    #region Members
    IList<int> m_copyRow = new List<int>();
    IList<int> m_copyColumn = new List<int>();
    readonly IList<int> m_shiftIds = new List<int>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Number of parts to produce"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "The amount of parts to produce per shift or per day, " +
          "for each operation, can be adjusted according to your needs. The amounts might take " +
          "into account your current buffer status, the performance expected by shift, etc.\n\n" +
          "Menus are present in the vertical and horizontal headers of the table to facilitate " +
          "the configuration."; } }
    
    bool UpdateSumEnabled { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
    {
      InitializeComponent();
      
      // Menu in the vertical header
      IList<string> menu = new List<string>();
      menu.Add("Copy");
      menu.Add("Paste");
      menu.Add("Duplicate to green rows");
      menu.Add("Set to 0");
      scrollTable.VerticalMenu = menu;
      
      // Menu in the horizontal header
      IList<String> horizontalMenu = new List<String>();
      horizontalMenu.Add("Copy");
      horizontalMenu.Add("Paste");
      horizontalMenu.Add("Duplicate column");
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
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Initialize
      UpdateSumEnabled = false;
      m_copyRow.Clear();
      m_copyColumn.Clear();
      
      // Adjust the number of quantities
      ItemDataListShift dataListShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES);
      int operationCount = 0;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          foreach (IComponent component in line.Components) {
            ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);
            operationCount += component.ComponentIntermediateWorkPieces.Count;
          }
        }
      }
      dataListShifts.SetOperationCount(operationCount, data.Get<int>(Item.PRODUCTION_QUANTITY));
      
      // Horizontal header
      IList<string> hHeaders = GetPartList(data);

      // Vertical header
      m_shiftIds.Clear();
      bool multipleShiftPerDay = false;
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      IDictionary<string, IList<string>> vHeaders = new Dictionary<string, IList<string>>();
      {
        IList<string> subVHeaders = null;
        DateTime currentDate = new DateTime();

        foreach (ItemDataShift dataShift in dataShifts) {
          if (dataShift.m_enabled) {
            DateTime shiftDay = dataShift.m_day;
            if (currentDate.Date != shiftDay || subVHeaders == null) {
              if (subVHeaders != null) {
                vHeaders[currentDate.ToShortDateString()] = subVHeaders;
                if (subVHeaders.Count > 1) {
                  multipleShiftPerDay = true;
                }
              }
              currentDate = shiftDay;
              subVHeaders = new List<string>();
            }

            subVHeaders.Add(dataShift.m_shiftDisplay);
            m_shiftIds.Add(dataShift.m_shiftID);
          }
        }
        
        vHeaders[currentDate.ToShortDateString()] = subVHeaders;
        if (subVHeaders != null && subVHeaders.Count > 1) {
          multipleShiftPerDay = true;
        }
      }
      
      // Apply headers
      scrollTable.InitTable(vHeaders, hHeaders);
      if (multipleShiftPerDay) {
        scrollTable.SecondVerticalHeaderWidth = 80;
      }
      else {
        scrollTable.SecondVerticalHeaderWidth = 0;
      }

      // Fill the values
      int row = 0;
      foreach (ItemDataShift dataShift in dataShifts) {
        if (dataShift.m_enabled) {
          int column = 0;
          foreach (int val in dataShift.m_quantities) {
            scrollTable.SetValue(row, column++, val);
          }

          row++;
        }
      }
      
      UpdateSumEnabled = true;
      
      // Sum of all columns
      for (int i = 0; i < hHeaders.Count; i++) {
        ScrollTableCellChanged (0, i);
      }
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Save values
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      int row = 0;
      foreach (ItemDataShift dataShift in dataShifts) {
        if (dataShift.m_enabled) {
          for (int column = 0; column < dataShift.m_quantities.Count; column++) {
            dataShift.m_quantities[column] = scrollTable.GetValue(row, column);
          }

          row++;
        }
      }
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      if (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence
          .Subtract(data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence).TotalDays > 7) {
        return null;
      }
      else {
        return "Page4";
      }
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // At least one shift should have quantities (> 0)
      if (!data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).HasQuantities) {
        errors.Add("at least one shift / day must comprise a quantity > 0");
      }

      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      int sum = -1;
      for (int col = 0; col < scrollTable.ColumnCount; col++) {
        int sumTmp = 0;
        for (int i = 0; i < scrollTable.RowCount; i++) {
          sumTmp += scrollTable.GetValue(i, col);
        }

        if (sum == -1 || sum == sumTmp) {
          sum = sumTmp;
        }
        else {
          warnings.Add("The total number of parts produced is not the same for all operations.");
          break;
        }
      }
      
      return warnings;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();

      // List of parts
      IList<string> parts = GetPartList(data);

      // Description of quantities per day
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;

      IList<string> subLines = null;
      DateTime currentDate = new DateTime();

      foreach (ItemDataShift dataShift in dataShifts) {
        if (dataShift.m_enabled) {
          DateTime shiftDay = dataShift.m_day;

          if (currentDate.Date != shiftDay || subLines == null) {
            if (subLines != null) {
              summary.Add(currentDate.ToShortDateString() + "\n" +
                          String.Join("\n", subLines.ToArray()));
            }
            currentDate = shiftDay;
            subLines = new List<string>();
          }

          IList<string> quantities = new List<string>();
          for (int i = 0; i < parts.Count; i++) {
            quantities.Add(parts[i] + ": " + dataShift.m_quantities[i]);
          }

          String prefix = "";
          if (dataShift.m_shiftDisplay != "") {
            prefix = "(" + dataShift.m_shiftDisplay + ") ";
          }

          subLines.Add(prefix + String.Join(", ", quantities.ToArray()));
        }
      }
      if (subLines != null) {
        summary.Add(currentDate.ToShortDateString() + "\n" + String.Join("\n", subLines.ToArray()));
      }

      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    IList<string> GetPartList(ItemData data)
    {
      IList<string> parts = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          foreach (IComponent component in line.Components) {
            ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);
            
            IList<IComponentIntermediateWorkPiece> ciwps = component.ComponentIntermediateWorkPieces.ToList();
            foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(ciwp.IntermediateWorkPiece);
              ModelDAOHelper.DAOFactory.OperationDAO.Lock(ciwp.IntermediateWorkPiece.Operation);
              parts.Add(ciwp.IntermediateWorkPiece.Operation.Name);
            }
          }
        }
      }
      return parts;
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
        if (i != row && m_shiftIds[i] == m_shiftIds[row]) {
          PasteRow (i);
        }
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
    
    void SetColumnTo0(int column)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        scrollTable.SetValue(i, column, 0);
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void ScrollTableCellChanged(int row, int column)
    {
      if (UpdateSumEnabled) {
        // Update sum
        int rowCount = scrollTable.RowCount;
        int sum = 0;
        for (int i = 0; i < rowCount; i++) {
          sum += scrollTable.GetValue(i, column);
        }

        scrollTable.SetFooterText(column, sum.ToString());
      }
    }
    
    void OnMenuClicked(int row, int numAction)
    {
      switch (numAction) {
        case 0:
          // Copy row
          CopyRow(row);
          break;
        case 1:
          // Paste row
          PasteRow(row);
          break;
        case 2:
          // Duplicate row
          DuplicateRow(row);
          break;
        case 3:
          // Set row to 0
          SetRowTo0(row);
          break;
      }
    }
    
    void OnHMenuClicked(int column, int numAction)
    {
      switch (numAction) {
        case 0:
          // Copy column
          CopyColumn(column);
          break;
        case 1:
          // Paste column
          PasteColumn(column);
          break;
        case 2:
          // Duplicate column
          DuplicateColumn(column);
          break;
        case 3:
          // Set column to 0
          SetColumnTo0(column);
          break;
      }
    }
    
    void ScrollTableVerticalMenuClosed(int row)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        scrollTable.ResetRowColor(i);
      }
    }
    
    void ScrollTableVerticalMenuOpen(int row)
    {
      // Highlight same kind of shift
      for (int i = 0; i < scrollTable.RowCount; i++) {
        if (m_shiftIds[i] == m_shiftIds[row]) {
          scrollTable.SetRowColor(i, Color.LightGreen);
        }
      }
    }
    #endregion // Event reactions
  }
}
