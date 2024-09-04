// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Shifts involved in the production period"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Shifts are listed here for each production day and " +
          "can be enabled or disabled. By default, only defined shifts are checked.\n\n" +
          "Menus are present in the horizontal and vertical headers of the table to " +
          "check or uncheck a whole column or row."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
    {
      ResourceManager rm = new ResourceManager("WizardCreateProduction.Item", GetType().Assembly);
      
      InitializeComponent();
      scrollTable.CellChanged += OnCellChanged;
      
      // Menu in the vertical header
      IList<String> verticalMenu = new List<String>();
      verticalMenu.Add("Check all");
      verticalMenu.Add("Uncheck all");
      scrollTable.VerticalMenu = verticalMenu;
      scrollTable.VerticalMenuClicked += OnVMenuClicked;
      
      // Menu in the horizontal header
      IList<String> horizontalMenu = new List<String>();
      horizontalMenu.Add("Check all");
      horizontalMenu.Add("Uncheck all");
      scrollTable.HorizontalMenu = horizontalMenu;
      scrollTable.HorizontalMenuClicked += OnHMenuClicked;
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
      // Headers
      IList<string> hHeaders = new List<string>();
      IList<string> vHeaders = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
      {
        using (IDAOTransaction transaction = session.BeginTransaction("Settings.WizardCreateProduction.Page2.LoadPageFromData"))
        {
          // Horizontal header
          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          foreach (IShift shift in shifts) {
            hHeaders.Add(shift.Display);
          }

          // Vertical header
          DateTime startDay = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetDay (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence.ToUniversalTime());
          DateTime endDay = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetEndDay (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence.ToUniversalTime().AddTicks(-1));
          while (startDay.Date <= endDay.Date) {
            vHeaders.Add(startDay.ToShortDateString());
            startDay = startDay.AddDays(1);
          }
          transaction.Commit ();
        }
      }
      scrollTable.InitTable(vHeaders, hHeaders);
      
      // Fill the table
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      for (int i = 0; i < scrollTable.RowCount; i++) {
        for (int j = 0; j < scrollTable.ColumnCount; j++) {
          ItemDataShift currentItem = dataShifts[i * scrollTable.ColumnCount + j];
          scrollTable.SetValue(i, j, currentItem.m_enabled ? 1 : 0);
          if (currentItem.Conflict) {
            Image image = null;
            if (currentItem.m_enabled) {
              image = imageList.Images[1];
            }
            else {
              image = imageList.Images[0];
            }

            scrollTable.SetTooltip(i, j, image, "A production is already planned for this shift.");
          }
        }
      }
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      for (int i = 0; i < scrollTable.RowCount; i++) {
        for (int j = 0; j < scrollTable.ColumnCount; j++) {
          dataShifts[i * scrollTable.ColumnCount + j].m_enabled = (scrollTable.GetValue(i, j) > 0);
        }
      }
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // At least one shift is enabled
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      bool isEnabled = false;
      foreach (ItemDataShift shift in dataShifts) {
        isEnabled |= shift.m_enabled;
      }

      if (!isEnabled) {
        errors.Add("at least one shift must be enabled");
      }

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
      
      int conflictCount = 0;
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      foreach (ItemDataShift dataShift in dataShifts) {
        if (dataShift.m_enabled && dataShift.Conflict) {
          conflictCount++;
        }
      }

      if (conflictCount > 0) {
        string plural = conflictCount > 1 ? "s" : "";
        warnings.Add("Potentially, " + conflictCount + " shift" + plural + " will be used for different production periods.");
      }
      data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).ConflictNumberWithRecurrences = conflictCount;
      
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
      IList<string> summary = new List<string>();
      
      // List of ignored shifts
      IList<ItemDataShift> dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).Shifts;
      IDictionary<DateTime, IList<string>> ignoredShiftsPerDay = new Dictionary<DateTime, IList<string>>();
      foreach (ItemDataShift shift in dataShifts) {
        if (!shift.m_enabled) {
          DateTime date = shift.m_day;
          if (!ignoredShiftsPerDay.ContainsKey(date)) {
            ignoredShiftsPerDay[date] = new List<string>();
          }

          ignoredShiftsPerDay[date].Add("\"" + shift.m_shiftDisplay + "\"");
        }
      }
      
      if (ignoredShiftsPerDay.Count > 0) {
        foreach (DateTime date in ignoredShiftsPerDay.Keys) {
          string plural = ignoredShiftsPerDay[date].Count > 1 ? "s" : "";
          summary.Add(date.ToShortDateString() + ": ignored shift" + plural + " " +
                      String.Join(", ", ignoredShiftsPerDay[date].ToArray()));
        }
      } else {
        summary.Add("all shifts enabled");
      }

      return summary;
    }
    #endregion // Page methods
    
    #region Event reactions
    void OnCellChanged(int row, int column)
    {
      if (scrollTable.HasToolTip(row, column)) {
        Image image = null;
        if (scrollTable.GetValue(row, column) > 0) {
          image = imageList.Images[1];
        }
        else {
          image = imageList.Images[0];
        }

        scrollTable.SetTooltip(row, column, image, scrollTable.ToolTipText(row, column));
      }
    }
    
    void OnHMenuClicked(int column, int numItem)
    {
      for (int i = 0; i < scrollTable.RowCount; i++) {
        scrollTable.SetValue(i, column, (numItem == 0) ? 1 : 0);
      }
    }
    
    void OnVMenuClicked(int row, int numItem)
    {
      for (int i = 0; i < scrollTable.ColumnCount; i++) {
        scrollTable.SetValue(row, i, (numItem == 0) ? 1 : 0);
      }
    }
    #endregion // Event reactions
  }
}
