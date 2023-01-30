// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLinePlanning
{
  /// <summary>
  /// Description of View.
  /// </summary>
  internal class Item : GenericItem, IView
  {
    internal const string LINE = "line";
    internal const string DATE = "date";
    internal const string BRUSHES = "brushes";
    internal const string TIMELINE_START = "timeline_start";
    internal const string TIMELINE_END = "timeline_end";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Line planning"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "The planning of all lines is shown on a calendar.\n" +
          "The different quantities of parts to produce is displayed.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "plannings", "planings", "productions", "quantities", "quantity",
          "calendar", "targets", "shifts" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "view"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Production line"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Production periods"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IWorkOrderLine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IOperation)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IPart)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the view
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IViewPage> Pages {
      get {
        IList<IViewPage> pages = new List<IViewPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region View methods
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
      data.InitValue(LINE, typeof(ILine), null, true);
      data.InitValue(DATE, typeof(DateTime), DateTime.Now, true);
      data.InitValue(BRUSHES, typeof(DistinctBrushes), new DistinctBrushes(), false);
      
      // Data specific to Page3
      data.CurrentPageName = "Page3";
      DateTime date = DateTime.Today.AddDays(1);
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            var range = ModelDAOHelper.DAOFactory.DaySlotDAO.GetTodayRange();
            if (range != null && range.Lower.HasValue) {
              date = date.AddTicks(range.Lower.Value.TimeOfDay.Ticks);
            }

            transaction.Commit();
          }
        }
      } catch {}
      data.InitValue(TIMELINE_START, typeof(DateTime), date.AddDays(-1), false);
      data.InitValue(TIMELINE_END, typeof(DateTime), date.AddDays(29), false);
      
      return data;
    }
    #endregion // View methods

    #region Other methods
    /// <summary>
    /// WorkOrderLine list associated with their planned days / shifts.
    /// Already within a session
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> GetExtendedWols(DateTime startDate, DateTime endDate)
    {
      IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> wolsExtended =
        new Dictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>>();
      
      // Retrieve all WorkOrderLines within the period
      IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.GetListInRange(new UtcDateTimeRange (startDate,endDate));
      foreach (IWorkOrderLine wol in wols) {
        if (wol.Deadline > startDate && wol.BeginDateTime < endDate) {
          wolsExtended[wol] = new List<IIntermediateWorkPieceTarget>();
        }
      }
      
      // Retrieve all IntermediateWorkPieceTargets within the period
      IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory
        .IntermediateWorkPieceTargetDAO.GetListInRange(startDate, endDate.AddDays(-1));
      foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
        if (iwpt.WorkOrder != null && iwpt.Line != null && iwpt.Number > 0) {
          IWorkOrderLine wol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(iwpt.Line, iwpt.WorkOrder);
          if (wol != null) {
            if (!wolsExtended.ContainsKey(wol)) {
              wolsExtended[wol] = new List<IIntermediateWorkPieceTarget>();
            }

            wolsExtended[wol].Add(iwpt);
          }
        }
      }
      
      return wolsExtended;
    }
    
    #endregion // Other methods
  }
}
