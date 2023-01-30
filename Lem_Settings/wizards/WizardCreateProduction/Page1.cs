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
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericWizardPage, IWizardPage
  {
    #region Members
    LineBarData m_barData = new LineBarData();
    bool m_noRefresh;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Production period"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "You can enter here the beginning and the end of the production.\n\n" +
          "In a following page, this production can be repeated daily (if the period duration is less than or equal to 24 hours), " +
          "or weekly (if the period duration is less than or equal to 7 full days).\n\n" +
          "In the timeline are represented each production period (colored segments) with their targets per day (ellipses covering a day)."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IWorkOrder));
        types.Add(typeof(IWorkOrderLine));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      InitializeComponent();
      timelinesWidget.AddTimeline("", m_barData);
      timelinesWidget.SelectedPeriodChanged += OnSelectedPeriodChanged;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
      // Nothing
    }
    
    /// <summary>
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Load line and period
      m_noRefresh = true;
      m_barData.Line = data.Get<ILine>(Item.LINE);
      dateEnd.Value = timeEnd.Value = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence;
      dateStart.Value = timeStart.Value = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence;
      m_noRefresh = false;
      
      timelinesWidget.SetPeriod(data.Get<DateTime>(Item.TIMELINE_START), data.Get<DateTime>(Item.TIMELINE_END));
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Store in local time
      data.Store(Item.TIMELINE_START, timelinesWidget.StartDateTime);
      data.Store(Item.TIMELINE_END, timelinesWidget.EndDateTime);
      data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence =
        dateStart.Value.Date.Add(timeStart.Value.TimeOfDay);
      data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence =
        dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // Order of start and end date
      if (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence >=
          data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence) {
        errors.Add("the end date of the production must be posterior to its beginning");
      }
      else {
        // Line already in use?
        IDictionary<IWorkOrderLine, UtcDateTimeRange> dicWols = Item.GetWorkOrderLines(
          data.Get<ILine>(Item.LINE),
          data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence.ToUniversalTime(),
          data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence.ToUniversalTime());
        
        // Wols that can/cannot be deleted
        IList<IWorkOrderLine> deletableWols = new List<IWorkOrderLine>();
        IList<string> remainingWols = new List<string>();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction("Settings.WizardCreateProduction.Page1.GetErrorsToGoNext"))
          {
            DateTime todayDateTime = ModelDAOHelper.DAOFactory.TimeConfigDAO
              .GetTodayEndUtcDateTime();
            foreach (IWorkOrderLine wol in dicWols.Keys) {
              if (dicWols[wol].ContainsElement(todayDateTime)) {
                remainingWols.Add(wol.WorkOrder.Display);
              }
              else {
                deletableWols.Add(wol);
              }
            }
            transaction.Commit ();
          }
        }
        
        if (remainingWols.Count > 0) {
          errors.Add("line already in use for \"" + String.Join("\", \"", remainingWols.ToArray()) + "\"");
        }
        else {
          data.Store(Item.WOLS_TO_DELETE, deletableWols);
        }
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
      
      // Warning if productions are deleted
      IList<IWorkOrderLine> wols = data.Get<List<IWorkOrderLine>>(Item.WOLS_TO_DELETE);
      if (wols.Count > 0) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<string> woDisplays = new List<string>();
            foreach (IWorkOrderLine wol in wols) {
              ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
              ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock(wol.WorkOrder);
              woDisplays.Add(wol.WorkOrder.Display);
            }
            warnings.Add("The following production" + (woDisplays.Count > 1 ? "s" : "") +
                         " will be removed:\n - \"" +
                         String.Join("\",\n - \"", woDisplays.ToArray()) + "\".");
          }
        }
      }
      
      // Adjust recurrence end date if necessary
      if (data.Get<DateTime>(Item.RECURRENCE_END) < data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence) {
        data.Store(Item.RECURRENCE_END, data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence);
      }

      // Preparation of the shift list
      ItemDataListShift dataShifts = data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction("Settings.WizardCreateProduction.Page1.GetWarnings"))
        {
          // Start and end day
          DateTime startDay = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetDay (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES)
                     .StartDateTimeFirstRecurrence.ToUniversalTime());
          DateTime endDay = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetEndDay (data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES)
                        .EndDateTimeFirstRecurrence.ToUniversalTime().AddTicks(-1));
          
          // List of shifts
          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          
          // Conflicts with another production of the current line (wol to be deleted are ignored)
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.FindByLine(line);
          
          dataShifts.PrepareNewInsertion();
          while (startDay.Date <= endDay.Date) {
            if (shifts.Count > 0) {
              foreach (IShift shift in shifts) {
                ItemDataShift item = new ItemDataShift(shift.Id, shift.Display, startDay);

                foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
                  if (iwpt.Line != null && iwpt.WorkOrder != null &&
                      object.Equals (iwpt.Shift, shift) && object.Equals(iwpt.Day, startDay) && iwpt.Number > 0) {
                    IWorkOrderLine wolTmp = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(line, iwpt.WorkOrder);
                    if (wolTmp != null && !wols.Contains(wolTmp)) {
                      item.Conflict = true;
                      break;
                    }
                  }
                }
                
                // Enabled by default?
                item.m_enabled = ModelDAOHelper.DAOFactory.ShiftSlotDAO.IsDefined(shift, startDay);
                
                dataShifts.Add(item);
              }
            } else {
              dataShifts.Add(new ItemDataShift(-1, "", startDay));
            }

            startDay = startDay.AddDays(1);
          }
          
          transaction.Commit ();
        }
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
      bool shiftDefined = false;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          shiftDefined = (ModelDAOHelper.DAOFactory.ShiftDAO.FindAll().Count > 0);
        }
      }

      return (shiftDefined ? "Page2" : "Page3");
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      summary.Add("start of the first period: " +
                  data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).StartDateTimeFirstRecurrence.ToString("g"));
      summary.Add("end of the first period: " +
                  data.Get<ItemDataListShift>(Item.SHIFT_QUANTITIES).EndDateTimeFirstRecurrence.ToString("g"));
      
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void RefreshTimeline()
    {
      if (m_noRefresh) {
        return;
      }

      timelinesWidget.SetSelectedPeriod(dateStart.Value.Date.Add(timeStart.Value.TimeOfDay),
                                        dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay));
      timelinesWidget.Draw();
    }
    #endregion // Private methods
    
    #region Events
    void DateStartValueChanged(object sender, EventArgs e)
    {
      if (m_noRefresh) {
        return;
      }

      if (dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay) < dateStart.Value.Date.Add(timeStart.Value.TimeOfDay)) {
        dateEnd.Value = dateStart.Value;
        timeEnd.Value = timeStart.Value;
      }
      RefreshTimeline();
    }
    
    void DateEndValueChanged(object sender, EventArgs e)
    {
      if (m_noRefresh) {
        return;
      }

      if (dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay) < dateStart.Value.Date.Add(timeStart.Value.TimeOfDay)) {
        dateStart.Value = dateEnd.Value;
        timeStart.Value = timeEnd.Value;
      }
      RefreshTimeline();
    }
    
    void TimeStartValueChanged(object sender, EventArgs e)
    {
      if (m_noRefresh) {
        return;
      }

      if (dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay) < dateStart.Value.Date.Add(timeStart.Value.TimeOfDay)) {
        dateEnd.Value = dateStart.Value;
        timeEnd.Value = timeStart.Value;
      }
      RefreshTimeline();
    }
    
    void TimeEndValueChanged(object sender, EventArgs e)
    {
      if (m_noRefresh) {
        return;
      }

      if (dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay) < dateStart.Value.Date.Add(timeStart.Value.TimeOfDay)) {
        dateStart.Value = dateEnd.Value;
        timeStart.Value = timeEnd.Value;
      }
      RefreshTimeline();
    }
    
    void OnSelectedPeriodChanged(DateTime? start, DateTime? end)
    {
      if (start.HasValue) {
        dateStart.Value = timeStart.Value = start.Value;
      }

      if (end.HasValue) {
        dateEnd.Value = timeEnd.Value = end.Value;
      }
    }
    #endregion // Events
  }
}
