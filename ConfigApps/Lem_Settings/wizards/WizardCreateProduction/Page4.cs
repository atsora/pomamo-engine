// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of Page4.
  /// </summary>
  internal partial class Page4 : GenericWizardPage, IWizardPage
  {
    #region Members
    DateTime m_endDateFirstProduction;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Recurrences"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "The period specified, provided it is short enough to allow " +
          "a weekly or daily recurrence, can be repeated here. The same goals will be associated " +
          "with the copied periods."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page4()
    {
      InitializeComponent();
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
      m_endDateFirstProduction = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence;
      
      // Enabling radio buttons
      double totalDays = m_endDateFirstProduction
        .Subtract(data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence).TotalDays;
      radioDaily.Enabled = (totalDays <= 1);
      radioWeekly.Enabled = (totalDays <= 7);
      
      // Retrieve data
      dateEnd1.Value = dateEnd2.Value = data.Get<DateTime>(Item.RECURRENCE_END);
      dayInWeekPicker.DaysInWeek = data.Get<int>(Item.RECURRENCE_DAYS);
      
      // End time
      labelTime1.Text = labelTime2.Text = "at " + m_endDateFirstProduction.ToString("t");
      
      // Recurrence type
      int recurrenceType = data.Get<int>(Item.RECURRENCE_TYPE);
      if (recurrenceType == 1 && !radioDaily.Enabled) {
        recurrenceType = 0;
      }
      else if (recurrenceType == 2 && !radioWeekly.Enabled) {
        recurrenceType = 0;
      }

      switch (recurrenceType) {
        case 1:
          radioDaily.Checked = true;
          break;
        case 2:
          radioWeekly.Checked = true;
          DateEnd2ValueChanged(null, null);
          break;
        default:
          radioNo.Checked = true;
          break;
      }
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      int recurrenceType = 0;
      long ticks = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES)
        .EndDateTimeFirstRecurrence.TimeOfDay.Ticks;
      if (radioDaily.Checked) {
        recurrenceType = 1;
        data.Store(Item.RECURRENCE_END, dateEnd1.Value.Date.AddTicks(ticks));
      }
      if (radioWeekly.Checked) {
        recurrenceType = 2;
        data.Store(Item.RECURRENCE_END, dateEnd2.Value.Date.AddTicks(ticks));
      }
      data.Store(Item.RECURRENCE_TYPE, recurrenceType);
      data.Store(Item.RECURRENCE_DAYS, dayInWeekPicker.DaysInWeek);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // Recurrence end day has to be posterior to the production end date
      if (data.Get<int>(Item.RECURRENCE_TYPE) != 0 &&
          data.Get<DateTime>(Item.RECURRENCE_END).CompareTo(data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence) < 0) {
        string message = "recurrence end date must be posterior to the first production period end date (which is " +
          data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence.ToString("g") + ")";
        errors.Add(message);
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
      
      // No additional shifts added by the recurrence settings
      IList<IList<ItemDataShift>> list_shifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES)
        .GetRecurrences(data.Get<int>(Item.RECURRENCE_TYPE),
                        data.Get<DateTime>(Item.RECURRENCE_END),
                        data.Get<int>(Item.RECURRENCE_DAYS));
      if (data.Get<int>(Item.RECURRENCE_TYPE) != 0 && list_shifts.Count == 1) {
        warnings.Add("No additional shifts added by the recurrence settings.");
      }

      // Additional wols that are going to be deleted
      IList<IWorkOrderLine> wolsNormallyDeleted = data.Get<List<IWorkOrderLine>>(Item.WOLS_TO_DELETE);
      IList<IWorkOrderLine> otherWols = new List<IWorkOrderLine>();
      foreach (IList<ItemDataShift> shifts in list_shifts) {
        if (shifts.Count > 0) {
          DateTime startDateTime = shifts.First().m_startPeriod.ToUniversalTime();
          DateTime endDateTime = shifts.Last().m_endPeriod.ToUniversalTime();
          IDictionary<IWorkOrderLine, UtcDateTimeRange> dicWols = Item.GetWorkOrderLines(
            data.Get<ILine>(Item.LINE), startDateTime, endDateTime);
          
          foreach (IWorkOrderLine wol in dicWols.Keys) {
            if (!wolsNormallyDeleted.Contains(wol) && !otherWols.Contains(wol)) {
              otherWols.Add(wol);
            }
          }
        }
      }
      
      // Warning if additional productions are deleted
      if (otherWols.Count > 0) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            ILine line = data.Get<ILine>(Item.LINE);
            IList<string> woDisplays = new List<string>();
            foreach (IWorkOrderLine wol in otherWols) {
              
              woDisplays.Add(wol.WorkOrder.Display);
            }
            warnings.Add("The recurrence will remove the following production" + (woDisplays.Count > 1 ? "s" : "") + ":\n - \"" +
                         String.Join("\",\n - \"", woDisplays.ToArray()) + "\".");
          }
        }
      }
      
      // Store the other wols
      data.Store(Item.WOLS_TO_DELETE_RECURRENCE, otherWols);
      
      // Count the number of shifts with a conflict
      int count = 0;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IIntermediateWorkPieceTarget> iwpss = ModelDAOHelper.DAOFactory
            .IntermediateWorkPieceTargetDAO.FindByLine(data.Get<ILine>(Item.LINE));
          
          foreach (IList<ItemDataShift> shifts in list_shifts) {
            foreach (ItemDataShift shift in shifts) {
              if (shift.m_enabled && shift.Count > 0) {
                DateTime date = shift.m_day.Date;
                foreach (IIntermediateWorkPieceTarget iwpt in iwpss) {
                  if (iwpt.Line != null && iwpt.WorkOrder != null &&
                      object.Equals (iwpt.Shift, shift) && object.Equals(iwpt.Day, date) && iwpt.Number > 0) {
                    IWorkOrderLine wolTmp = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(iwpt.Line, iwpt.WorkOrder);
                    if (wolTmp != null &&  !wolsNormallyDeleted.Contains(wolTmp)) {
                      count++;
                      break;
                    }
                  }
                }
              }
            }
          }
        }
      }
      data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).ConflictNumberWithRecurrences = count;
      
      if (count > 0) {
        string plural = count > 1 ? "s" : "";
        warnings.Add(count + " shift" + plural + " will be used for several productions.");
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
      
      IList<IList<ItemDataShift>> list_shifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES)
        .GetRecurrences(data.Get<int>(Item.RECURRENCE_TYPE),
                        data.Get<DateTime>(Item.RECURRENCE_END),
                        data.Get<int>(Item.RECURRENCE_DAYS));
      int recurrenceNumber = list_shifts.Count;
      
      switch (data.Get<int>(Item.RECURRENCE_TYPE)) {
        case 1:
          summary.Add("daily recurrence until " + data.Get<DateTime>(Item.RECURRENCE_END).ToString("g"));
          if (dayInWeekPicker.DayCount == 7) {
          summary.Add("working days: all");
        }
        else {
            string plural = (dayInWeekPicker.DayCount > 1) ? "s" : "";
            summary.Add("working day" + plural + ": " + dayInWeekPicker.FormattedDays());
          }
          summary.Add("total number of productions planned: " + recurrenceNumber);
          break;
        case 2:
          summary.Add("weekly recurrence until " + data.Get<DateTime>(Item.RECURRENCE_END).ToString("g"));
          summary.Add("total number of productions planned: " + recurrenceNumber);
          break;
        default:
          summary.Add("no recurrences");
          break;
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Event reaction
    void RadioNoCheckedChanged(object sender, EventArgs e)
    {
      if (radioNo.Checked) {
        tableDay.Enabled = false;
        tableWeek.Enabled = false;
      }
    }
    
    void RadioDailyCheckedChanged(object sender, EventArgs e)
    {
      if (radioDaily.Checked) {
        tableDay.Enabled = true;
        tableWeek.Enabled = false;
      }
    }
    
    void RadioWeeklyCheckedChanged(object sender, EventArgs e)
    {
      if (radioWeekly.Checked) {
        tableDay.Enabled = false;
        tableWeek.Enabled = true;
        DateEnd2ValueChanged(null, null);
      }
    }
    
    void DateEnd1ValueChanged(object sender, EventArgs e)
    {
      if (dateEnd1.Value < m_endDateFirstProduction) {
        dateEnd1.Value = m_endDateFirstProduction;
      }
    }
    
    void DateEnd2ValueChanged(object sender, EventArgs e)
    {
      TimeSpan timeSpan = dateEnd2.Value.Date - m_endDateFirstProduction.Date;
      int numberOfDays = timeSpan.Days;
      if (numberOfDays < 0) {
        dateEnd2.Value = m_endDateFirstProduction.Date;
      }
      else if (numberOfDays % 7 != 0) {
        dateEnd2.Value = m_endDateFirstProduction.Date.AddDays(7 * (1 + numberOfDays / 7));
      }
    }
    #endregion // Event reactions
  }
}
