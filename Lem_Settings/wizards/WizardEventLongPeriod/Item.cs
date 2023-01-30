// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventLongPeriod
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string CHOICE_DONE = "choice_done";
    internal const string CLEAR_FIRST = "clear_first";
    internal const string MONITORED_MACHINES = "monitored_machines";
    internal const string ALL_MONITORED_MACHINES = "all_monitored_machines";
    internal const string MACHINE_MODES = "machine_modes";
    internal const string ALL_MACHINE_MODES = "all_machine_modes";
    internal const string MOSS = "machine_observation_states";
    internal const string ALL_MOSS = "all_machine_observation_states";
    internal const string DURATION_LEVELS = "duration_levels";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Create long period events"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Use this wizard to create long period events, which can then be linked to alerts. " +
          "Long period events are triggered when a machine has been in a particular state for too long " +
          "(the time being configurable).\n\n" +
          "This event can be used to detect a long inactivity period, or a long activity period without " +
          "a tool change for instance.\n\n" +
          "You can choose to clear all long period events that have been previously defined.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "events", "long", "periods", "idle", "time", "inactivity", "durations" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Notifications"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Events"; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IEventLongPeriodConfig)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IEvent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineMode)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineObservationState)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IEventLevel)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new Page0());
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        pages.Add(new Page4());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      data.InitValue(CLEAR_FIRST, typeof(bool), false, true);
      data.InitValue(MONITORED_MACHINES, typeof(IList<IMonitoredMachine>), new List<IMonitoredMachine>(), true);
      data.InitValue(ALL_MONITORED_MACHINES, typeof(bool), true, true);
      data.InitValue(MACHINE_MODES, typeof(IList<IMachineMode>), new List<IMachineMode>(), true);
      data.InitValue(ALL_MACHINE_MODES, typeof(bool), true, true);
      data.InitValue(MOSS, typeof(IList<IMachineObservationState>), new List<IMachineObservationState>(), true);
      data.InitValue(ALL_MOSS, typeof(bool), true, true);
      data.InitValue(DURATION_LEVELS, typeof(IDictionary<IEventLevel, TimeSpan>), new Dictionary<IEventLevel, TimeSpan>(), true);
      
      // Specific data for page 0
      data.CurrentPageName = "Page0";
      data.InitValue(CHOICE_DONE, typeof(bool), false, false);
      
      return data;
    }
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          
          // Retrieve and lock data
          IList<IMonitoredMachine> momas = Item.GetAndLockMonitoredMachines(data);
          IList<IMachineMode> mamos = Item.GetAndLockMachineModes(data);
          IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
          IDictionary<IEventLevel, TimeSpan> eventLevelDurations =
            data.Get<IDictionary<IEventLevel, TimeSpan>>(Item.DURATION_LEVELS);
          foreach (IEventLevel eventLevel in eventLevelDurations.Keys) {
            ModelDAOHelper.DAOFactory.EventLevelDAO.Lock(eventLevel);
          }

          // Clear all or just what is going to be overriden
          IList<IEventLongPeriodConfig> periodEvents;
          if (data.Get<bool>(Item.CLEAR_FIRST)) {
            periodEvents = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.FindAll();
          }
          else {
            periodEvents = Item.GetEventsToBeOverriden(momas, mamos, moss, eventLevelDurations.Values,
                                                      eventLevelDurations.Keys);
          }

          foreach (IEventLongPeriodConfig periodEvent in periodEvents) {
            ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.MakeTransient(periodEvent);
          }

          // Create new period events
          foreach (IMonitoredMachine moma in momas) {
            foreach (IMachineMode mamo in mamos) {
              foreach (IMachineObservationState mos in moss) {
                foreach (IEventLevel eventLevel in eventLevelDurations.Keys) {
                  IEventLongPeriodConfig config = ModelDAOHelper.ModelFactory.CreateEventLongPeriodConfig(
                    eventLevelDurations[eventLevel], eventLevel);
                  config.MachineObservationState = mos;
                  config.MachineMode = mamo;
                  config.MonitoredMachine = moma;
                  ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.MakePersistent(config);
                }
              }
            }
          }
          
          transaction.Commit ();
        }
      }
    }
    #endregion // Wizard methods
    
    #region Private methods
    /// <summary>
    /// Get and lock all monitored machines
    /// The returned list is never null or empty, but can comprise a value "null"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static public IList<IMonitoredMachine> GetAndLockMonitoredMachines(ItemData data)
    {
      IList<IMonitoredMachine> momas;
      if (data.Get<bool>(Item.ALL_MONITORED_MACHINES)) {
        momas = new List<IMonitoredMachine>();
        momas.Add(null);
      } else {
        momas = data.Get<IList<IMonitoredMachine>>(Item.MONITORED_MACHINES);
        foreach (IMonitoredMachine moma in momas) {
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock(moma);
        }
      }
      return momas;
    }
    
    /// <summary>
    /// Get and lock all machine modes
    /// The returned list is never null or empty, but can comprise a value "null"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static public IList<IMachineMode> GetAndLockMachineModes(ItemData data)
    {
      IList<IMachineMode> mamos;
      if (data.Get<bool>(Item.ALL_MACHINE_MODES)) {
        mamos = new List<IMachineMode>();
        mamos.Add(null);
      } else {
        mamos = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
        foreach (IMachineMode mamo in mamos) {
          ModelDAOHelper.DAOFactory.MachineModeDAO.Lock(mamo);
        }
      }
      return mamos;
    }
    
    /// <summary>
    /// Get and lock all machine observation states
    /// The returned list is never null or empty, but can comprise a value "null"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static public IList<IMachineObservationState> GetAndLockMachineObservationStates(ItemData data)
    {
      IList<IMachineObservationState> moss;
      if (data.Get<bool>(Item.ALL_MOSS)) {
        moss = new List<IMachineObservationState>();
        moss.Add(null);
      } else {
        moss = data.Get<IList<IMachineObservationState>>(Item.MOSS);
        foreach (IMachineObservationState mos in moss) {
          ModelDAOHelper.DAOFactory.MachineObservationStateDAO.Lock(mos);
        }
      }
      return moss;
    }
    
    /// <summary>
    /// Get all long event periods that will be overriden
    /// </summary>
    /// <param name="momas"></param>
    /// <param name="mamos"></param>
    /// <param name="moss"></param>
    /// <param name="timeSpans"></param>
    /// <returns></returns>
    static public IList<IEventLongPeriodConfig> GetEventsToBeOverriden(ICollection<IMonitoredMachine> momas,
                                                                       ICollection<IMachineMode> mamos,
                                                                       ICollection<IMachineObservationState> moss,
                                                                       ICollection<TimeSpan> timeSpans,
                                                                       ICollection<IEventLevel> levels)
    {
      IList<IEventLongPeriodConfig> elpcs = new List<IEventLongPeriodConfig>();
      
      foreach (IMonitoredMachine moma in momas) {
        foreach (IMachineMode mamo in mamos) {
          foreach (IMachineObservationState mos in moss) {
            // Same timespans will be removed
            foreach (TimeSpan timeSpan in timeSpans) {
              IList<IEventLongPeriodConfig> configs = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.FindByKey(moma, mamo, mos, timeSpan);
              foreach (IEventLongPeriodConfig config in configs) {
                if (!elpcs.Contains(config)) {
                  elpcs.Add(config);
                }
              }
            }
            
            // Same levels will be removed
            foreach (IEventLevel level in levels) {
              IList<IEventLongPeriodConfig> configs = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.FindByKey(moma, mamo, mos, level);
              foreach (IEventLongPeriodConfig config in configs) {
                if (!elpcs.Contains(config)) {
                  elpcs.Add(config);
                }
              }
            }
          }
        }
      }
      
      return elpcs;
    }
    #endregion // Private methods
  }
}
