// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventToolLife
{
  /// <summary>
  /// Description of Page4.
  /// </summary>
  internal partial class Page4 : GenericWizardPage, IWizardPage
  {
    #region Members
    readonly IList<EventLevelCell> m_eventLevelCells = new List<EventLevelCell>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Event level per event type"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        return "All types of events related to the life of a tool are listed here. " +
          "You can enable or disable them by checking or unchecking them.\n\n" +
          "For each selected event type, please choose the level associated.";
      }
    }
    
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
      // All event types
      var eventTypes = Enum.GetValues(typeof(EventToolLifeType));
      
      verticalScroll.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          
          IList<IEventLevel> levels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll();
          (levels as List<IEventLevel>).Sort((x, y) => x.Priority.CompareTo(y.Priority));
          
          // Typical alert level
          verticalScroll.AddControl(
            CreateGroup("Alert", levels, new [] {
                          EventToolLifeType.CurrentLifeDecreased,
                          EventToolLifeType.RestLifeIncreased,
                          EventToolLifeType.TotalLifeIncreased}));
          
          // Typical error level
          verticalScroll.AddControl(
            CreateGroup("Error", levels, new [] {
                          EventToolLifeType.ExpirationReached,
                          EventToolLifeType.StatusChangeToDefinitelyUnavailable}));
          
          // Typical warning level
          verticalScroll.AddControl(
            CreateGroup("Warning", levels, new [] {
                          EventToolLifeType.WarningReached,
                          EventToolLifeType.StatusChangeToTemporaryUnavailable,
                          EventToolLifeType.TotalLifeDecreased}));
          
          // Typical notice level
          verticalScroll.AddControl(
            CreateGroup("Notice", levels, new [] {
                          EventToolLifeType.CurrentLifeReset,
                          EventToolLifeType.RestLifeReset,
                          EventToolLifeType.StatusChangeToAvailable,
                          EventToolLifeType.WarningChanged}));
          
          // Typical info level
          verticalScroll.AddControl(
            CreateGroup("Info", levels, new [] {
                          EventToolLifeType.ToolRegistration,
                          EventToolLifeType.ToolMoved,
                          EventToolLifeType.ToolRemoval}));
        }
      }
    }
    
    Control CreateGroup(string levelName, IList<IEventLevel> levels, EventToolLifeType[] eventToolLifeTypes)
    {
      // Create a groupbox
      var groupBox = new GroupBox();
      groupBox.Text = "Typical events with the \"" + levelName + "\" level";
      groupBox.Font = new Font(groupBox.Font, FontStyle.Bold);
      groupBox.Dock = DockStyle.Fill;
      groupBox.Height = 28 * eventToolLifeTypes.Length + 18;
      
      // Comprising a layout
      var layout = new System.Windows.Forms.TableLayoutPanel();
      layout.Dock = DockStyle.Fill;
      groupBox.Controls.Add(layout);
      
      // Comprising cells
      foreach (var eventToolLifeType in eventToolLifeTypes) {
        var cell = new EventLevelCell(eventToolLifeType, levels);
        cell.Font = new Font(cell.Font, FontStyle.Regular);
        m_eventLevelCells.Add(cell);
        cell.Dock = DockStyle.Fill;
        layout.Controls.Add(cell);
      }
      
      return groupBox;
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      IDictionary<EventToolLifeType, IEventLevel> eventLevelTypes =
        data.Get<IDictionary<EventToolLifeType, IEventLevel>>(Item.TYPE_LEVELS);
      foreach (var cell in m_eventLevelCells) {
        if (eventLevelTypes.ContainsKey(cell.EventType)) {
          cell.EventLevel = eventLevelTypes[cell.EventType];
          cell.Checked = true;
        } else {
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
      IDictionary<EventToolLifeType, IEventLevel> eventLevelTypes =
        new Dictionary<EventToolLifeType, IEventLevel>();
      foreach (var cell in m_eventLevelCells) {
        if (cell.Checked) {
          eventLevelTypes[cell.EventType] = cell.EventLevel;
        }
      }
      data.Store(Item.TYPE_LEVELS, eventLevelTypes);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      IDictionary<EventToolLifeType, IEventLevel> eventLevelTypes =
        data.Get<IDictionary<EventToolLifeType, IEventLevel>>(Item.TYPE_LEVELS);
      
      // At least one level must be selected
      if (eventLevelTypes.Keys.Count == 0) {
        errors.Add("at least one event type must be selected");
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
      
      // Existing tool life events may be overriden
      if (data.Get<int>(Item.CHOICE) == 1) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            
            // Retrieve and lock data
            IList<IMachineFilter> mafis = Item.GetAndLockMachineFilters(data);
            IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
            
            IDictionary<EventToolLifeType, IEventLevel> eventLevelTypes =
              data.Get<IDictionary<EventToolLifeType, IEventLevel>>(Item.TYPE_LEVELS);
            IList<EventToolLifeType> etlts = new List<EventToolLifeType>();
            foreach (var key in eventLevelTypes.Keys) {
              if (eventLevelTypes[key] != null) {
                etlts.Add(key);
              }
            }

            // Get all corresponding existing tool life events
            int count = Item.GetEventsToBeOverriden(mafis, moss, etlts).Count;
            if (count > 0) {
              warnings.Add(String.Format("{0} existing tool life event{1} will be overwritten.",
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
      
      IDictionary<EventToolLifeType, IEventLevel> eventLevelTypes =
        data.Get<IDictionary<EventToolLifeType, IEventLevel>>(Item.TYPE_LEVELS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (EventToolLifeType eventType in eventLevelTypes.Keys) {
            if (eventLevelTypes[eventType] != null) {
              summary.Add(String.Format("{0} \u2192 {1}", eventType.Name(),
                                        eventLevelTypes[eventType].Display));
            }
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
