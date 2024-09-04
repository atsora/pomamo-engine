// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal abstract class AbstractItem : GenericItem
  {
    internal const string TIMELINE_START = "timeline_start";
    internal const string TIMELINE_END = "timeline_end";
    internal const string TIMELINE_UPDATE = "timeline_update";
    internal const string SELECTED_ITEMS = "selected_items";
    internal const string PERIOD_START = "period_start";
    internal const string PERIOD_END = "period_end";
    internal const string PERIOD_HAS_END = "period_has_end";
    
    #region Getters / Setters
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags { get { return LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected AbstractItem() : base() {}
    #endregion // Constructors

    #region Configurator methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherData)
    {
      BarSegment.ResetLegend();
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      DateTime periodStart = DateTime.Today.AddDays(1);
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            var range = ModelDAOHelper.DAOFactory.DaySlotDAO.GetTodayRange();
            if (range != null && range.Lower.HasValue) {
              periodStart = periodStart.AddTicks(range.Lower.Value.TimeOfDay.Ticks);
            }

            transaction.Commit();
          }
        }
      } catch {}
      
      data.InitValue(PERIOD_START, typeof(DateTime), periodStart, true);
      data.InitValue(PERIOD_END, typeof(DateTime), periodStart.AddHours(6), true);
      data.InitValue(PERIOD_HAS_END, typeof(bool), true, true);
      data.InitValue(TIMELINE_UPDATE, typeof(bool), true, false);
      
      // Specific data for page 1
      data.CurrentPageName = "Page1";
      data.InitValue(TIMELINE_START, typeof(DateTime), periodStart.AddDays(-1), false);
      data.InitValue(TIMELINE_END, typeof(DateTime), periodStart.AddDays(1), false);
      
      // Specific initialization
      InitializeData(data, otherData);
      
      return data;
    }
    #endregion // Configurator methods
    
    protected abstract void InitializeData(ItemData data, ItemData otherData);
  }
}
