// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventToolLife
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string CHOICE = "choice";
    internal const string MACHINE_FILTERS = "machine_filters";
    internal const string NO_MACHINE_FILTERS = "no_machine_filters";
    internal const string MOSS = "machine_observation_states";
    internal const string ALL_MOSS = "all_machine_observation_states";
    internal const string TYPE_LEVELS = "type_levels";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Create tool life events"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Use this wizard to create tool life events, which can then be linked to alerts.\n\n" +
          "Tool life events are for example: a tool registration or unregistration, a tool change, " +
          "an abnormality regarding the remaining/current life of a tool, a breakage.\n\n" +
          "It is possible to define which machines can trigger these events, associated with their state.\n\n" +
          "You can choose to clear all tool life events that have been previously defined.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "events", "tools", "life", "remaining", "current", "breakage", "durations",
          "toolpots", "magazines" };
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
        dic[typeof(IEventToolLifeConfig)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IEvent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineFilter)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
      data.InitValue(CHOICE, typeof(int), 0, true);
      data.InitValue(MACHINE_FILTERS, typeof(IList<IMachineFilter>), new List<IMachineFilter>(), true);
      data.InitValue(NO_MACHINE_FILTERS, typeof(bool), true, true);
      data.InitValue(ALL_MOSS, typeof(bool), false, true);
      
      var productionMoss = new List<IMachineObservationState>();
      var types = new Dictionary<EventToolLifeType, IEventLevel>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var moss = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindAll();
          foreach (var mos in moss) {
            if (mos.IsProduction) {
              productionMoss.Add(mos);
            }
          }

          // Alert level
          var level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById(1);
          if (level != null) {
            types[EventToolLifeType.CurrentLifeDecreased] = level;
            types[EventToolLifeType.RestLifeIncreased] = level;
            types[EventToolLifeType.TotalLifeIncreased] = level;
          }
          
          // Error level
          level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById(2);
          if (level != null) {
            types[EventToolLifeType.ExpirationReached] = level;
            types[EventToolLifeType.StatusChangeToDefinitelyUnavailable] = level;
          }
          
          // Warning level
          level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById(3);
          if (level != null) {
            types[EventToolLifeType.WarningReached] = level;
            types[EventToolLifeType.StatusChangeToTemporaryUnavailable] = level;
            types[EventToolLifeType.TotalLifeDecreased] = level;
          }
          
          // Notice level
          level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById(4);
          if (level != null) {
            types[EventToolLifeType.CurrentLifeReset] = level;
            types[EventToolLifeType.RestLifeReset] = level;
            types[EventToolLifeType.StatusChangeToAvailable] = level;
            types[EventToolLifeType.WarningChanged] = level;
          }
          
          // Info level
          level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById(5);
          if (level != null) {
            types[EventToolLifeType.ToolRegistration] = level;
            types[EventToolLifeType.ToolRemoval] = level;
            types[EventToolLifeType.ToolMoved] = level;
          }
        }
      }
      data.InitValue(MOSS, typeof(IList<IMachineObservationState>), productionMoss, true);
      data.InitValue(TYPE_LEVELS, typeof(IDictionary<EventToolLifeType, IEventLevel>), types, true);
      
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

          // Clear all or just what is going to be overriden
          IList<IEventToolLifeConfig> toolEvents;
          if (data.Get<int>(Item.CHOICE) == 2 || data.Get<int>(Item.CHOICE) == 3) {
            toolEvents = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAll();
          }
          else {
            toolEvents = Item.GetEventsToBeOverriden(mafis, moss, etlts);
          }

          foreach (IEventToolLifeConfig toolEvent in toolEvents) {
            ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.MakeTransient(toolEvent);
          }

          // Create new period events
          if (data.Get<int>(Item.CHOICE) != 3) {
            foreach (IMachineFilter mafi in mafis) {
              foreach (IMachineObservationState mos in moss) {
                foreach (EventToolLifeType eventType in eventLevelTypes.Keys) {
                  if (eventLevelTypes[eventType] != null) {
                    IEventToolLifeConfig config = ModelDAOHelper.ModelFactory.CreateEventToolLifeConfig(
                      eventType, eventLevelTypes[eventType]);
                    config.MachineObservationState = mos;
                    config.MachineFilter = mafi;
                    ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.MakePersistent(config);
                  }
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
    static public IList<IMachineFilter> GetAndLockMachineFilters(ItemData data)
    {
      IList<IMachineFilter> mafis;
      if (data.Get<bool>(Item.NO_MACHINE_FILTERS)) {
        mafis = new List<IMachineFilter>();
        mafis.Add(null);
      } else {
        mafis = data.Get<IList<IMachineFilter>>(Item.MACHINE_FILTERS);
        foreach (IMachineFilter mafi in mafis) {
          ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(mafi);
        }
      }
      return mafis;
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
    /// Get all tool life events that will be overriden
    /// </summary>
    /// <param name="mafis"></param>
    /// <param name="moss"></param>
    /// <returns></returns>
    static public IList<IEventToolLifeConfig> GetEventsToBeOverriden(ICollection<IMachineFilter> mafis,
                                                                     ICollection<IMachineObservationState> moss,
                                                                     ICollection<EventToolLifeType> etlts)
    {
      IList<IEventToolLifeConfig> elpcs = new List<IEventToolLifeConfig>();
      
      foreach (IMachineFilter mafi in mafis) {
        foreach (IMachineObservationState mos in moss) {
          IList<IEventToolLifeConfig> configs = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindByKey(mafi, mos);
          foreach (IEventToolLifeConfig config in configs) {
            if (!elpcs.Contains(config) && etlts.Contains(config.Type)) {
              elpcs.Add(config);
            }
          }
        }
      }
      
      return elpcs;
    }
    #endregion // Private methods
  }
}
