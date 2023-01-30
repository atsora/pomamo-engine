// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventLongPeriod
{
  /// <summary>
  /// Description of Page4.
  /// </summary>
  internal partial class Page4 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Trigger duration per event levels"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Check here the different levels needed for the new events to be created.\n\n" +
          "For each event level, specify the duration of inactivity that triggers it.\n\n" +
          "Note: a higher value means a lower priority."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IEventLevel));
        return types;
      }
    }
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
    public void Initialize(ItemContext context)
    {
      verticalScroll.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IEventLevel> levels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll();
          (levels as List<IEventLevel>).Sort((x, y) => x.Priority.CompareTo(y.Priority));
          foreach (IEventLevel level in levels) {
            var control = new EventLevelCell(level);
            control.Dock = DockStyle.Fill;
            verticalScroll.AddControl(control);
          }
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      IDictionary<IEventLevel, TimeSpan> eventLevelDurations =
        data.Get<IDictionary<IEventLevel, TimeSpan>>(Item.DURATION_LEVELS);
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as EventLevelCell;
        if (eventLevelDurations.ContainsKey(cell.EventLevel)) {
          cell.Time = eventLevelDurations[cell.EventLevel];
          cell.Checked = true;
        } else {
          cell.Time = new TimeSpan(0, 0, 0);
          cell.Checked = false;
        }
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IDictionary<IEventLevel, TimeSpan> eventLevelDurations = new Dictionary<IEventLevel, TimeSpan>();
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as EventLevelCell;
        if (cell.Checked) {
          eventLevelDurations[cell.EventLevel] = cell.Time;
        }
      }
      data.Store(Item.DURATION_LEVELS, eventLevelDurations);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      IDictionary<IEventLevel, TimeSpan> eventLevelDurations =
        data.Get<IDictionary<IEventLevel, TimeSpan>>(Item.DURATION_LEVELS);
      
      // At least one level must be selected
      if (eventLevelDurations.Keys.Count == 0) {
        errors.Add("at least one event level must be selected");
      }

      // Trigger durations cannot be 0
      foreach (TimeSpan timeSpan in eventLevelDurations.Values) {
        if (timeSpan.Ticks == 0) {
          errors.Add("trigger durations cannot be 0");
          break;
        }
      }
      
      // Trigger durations must be different
      IList<TimeSpan> timeSpans = new List<TimeSpan>();
      foreach (TimeSpan timeSpan in eventLevelDurations.Values) {
        if (timeSpans.Contains(timeSpan)) {
          errors.Add("trigger durations must be different");
          break;
        } else {
          timeSpans.Add(timeSpan);
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
      
      // Existing long period events may be overriden
      if (!data.Get<bool>(Item.CLEAR_FIRST)) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            
            // Retrieve and lock data
            IList<IMonitoredMachine> momas = Item.GetAndLockMonitoredMachines(data);
            IList<IMachineMode> mamos = Item.GetAndLockMachineModes(data);
            IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
            IDictionary<IEventLevel, TimeSpan> eventLevelDurations =
              data.Get<IDictionary<IEventLevel, TimeSpan>>(Item.DURATION_LEVELS);
            foreach (IEventLevel eventLevel in eventLevelDurations.Keys) {
              ModelDAOHelper.DAOFactory.EventLevelDAO.Lock(eventLevel);
            }

            // Get all corresponding existing long period events
            int count = Item.GetEventsToBeOverriden(momas, mamos, moss, eventLevelDurations.Values,
                                                   eventLevelDurations.Keys).Count;
            if (count > 0) {
              warnings.Add(String.Format("{0} existing long period event{1} will be overwritten.",
                                         count, count > 1 ? "s" : ""));
            }
          }
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
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      IDictionary<IEventLevel, TimeSpan> eventLevelDurations =
        data.Get<IDictionary<IEventLevel, TimeSpan>>(Item.DURATION_LEVELS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IEventLevel eventLevel in eventLevelDurations.Keys) {
            ModelDAOHelper.DAOFactory.EventLevelDAO.Lock(eventLevel);
            summary.Add(String.Format("{0} \u2192 {1}", eventLevel.Display, eventLevelDurations[eventLevel]));
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
