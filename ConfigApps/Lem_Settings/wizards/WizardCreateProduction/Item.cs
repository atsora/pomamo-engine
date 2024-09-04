// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of Wizard.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string LINE = "line";
    internal const string PRODUCTION_NAME = "production_name";
    internal const string PRODUCTION_QUANTITY = "production_quantity";
    internal const string SHIFT_QUANTITIES = "shift_quantities";
    internal const string RECURRENCE_TYPE = "recurrence_type";
    internal const string RECURRENCE_END = "recurrence_end";
    internal const string RECURRENCE_DAYS = "recurrence_days";
    internal const string TIMELINE_START = "timeline_start";
    internal const string TIMELINE_END = "timeline_end";
    internal const string WOLS_TO_DELETE = "productions_to_delete";
    internal const string WOLS_TO_DELETE_RECURRENCE = "productions_to_delete_recurrence";
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public override string Title { get { return "Plan a production period"; } }
    
    /// <summary>
    /// Description
    /// </summary>
    public override string Description {
      get {
        return "A production period can be planned for an existing line " +
          "by specifying the number of parts to produce for each shift, from a start date to an end date.\n" +
          "Daily and weekly recurrences are possible.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "productions", "shifts", "targets", "quantities", "quantity",
          "parts", "periods", "goals" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }

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
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IWorkOrderLine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IWorkOrder)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new PageLine());
        pages.Add(new PageAttributes());
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
      data.InitValue(LINE, typeof(ILine), null, true);
      data.InitValue(PRODUCTION_NAME, typeof(string), GetFromConfig("name_pattern"), true);
      data.InitValue(PRODUCTION_QUANTITY, typeof(int), 1000, true);
      DateTime productionStart = DateTime.Today.AddDays(1);
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            var range = ModelDAOHelper.DAOFactory.DaySlotDAO.GetTodayRange();
            if (range != null && range.Lower.HasValue) {
              productionStart = productionStart.AddTicks(range.Lower.Value.TimeOfDay.Ticks);
            }

            transaction.Commit();
          }
        }
      } catch {}
      DateTime productionEnd = productionStart.AddDays(1);
      var dataListShift = new ItemDataListShift();
      dataListShift.StartDateTimeFirstRecurrence = productionStart;
      dataListShift.EndDateTimeFirstRecurrence = productionEnd;
      data.InitValue(SHIFT_QUANTITIES, typeof(ItemDataListShift), dataListShift, true);
      data.InitValue(RECURRENCE_TYPE, typeof(int), 0, true);
      data.InitValue(RECURRENCE_END, typeof(DateTime), productionEnd, true);
      data.InitValue(RECURRENCE_DAYS, typeof(int), 127, true);
      data.InitValue(WOLS_TO_DELETE, typeof(List<IWorkOrderLine>), new List<IWorkOrderLine>(), true);
      data.InitValue(WOLS_TO_DELETE_RECURRENCE, typeof(List<IWorkOrderLine>), new List<IWorkOrderLine>(), true);
      
      // Data specific to Page1
      data.CurrentPageName = "Page1";
      data.InitValue(TIMELINE_START, typeof(DateTime), productionStart.AddDays(-1), false);
      data.InitValue(TIMELINE_END, typeof(DateTime), productionEnd.AddDays(28), false);
      
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
      // Wol deletion (revisions are not monitored)
      DeleteWols(data.Get<ILine>(LINE), data.Get<List<IWorkOrderLine>>(WOLS_TO_DELETE));
      DeleteWols(data.Get<ILine>(LINE), data.Get<List<IWorkOrderLine>>(WOLS_TO_DELETE_RECURRENCE));
      
      // Create the wols
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          revision = ModelDAOHelper.ModelFactory.CreateRevision();
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent(revision);
          
          // Load and lock the line
          ILine line = data.Get<ILine>(LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          if (line.Components != null) {
            foreach (IComponent component in line.Components) {
              ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);
              if (component.Part != null) {
                ModelDAOHelper.DAOFactory.PartDAO.Lock(component.Part);
              }
            }
          }
          
          // Compute all shifts per recurrence
          IList<IList<ItemDataShift>> recurrent_shifts =
            data.Get<ItemDataListShift>(SHIFT_QUANTITIES).GetRecurrences(
              data.Get<int>(RECURRENCE_TYPE),
              data.Get<DateTime>(RECURRENCE_END),
              data.Get<int>(RECURRENCE_DAYS));
          int numRecurrence = 1;
          foreach (IList<ItemDataShift> shifts in recurrent_shifts)
          {
            // Name of the workorder
            string woName = data.Get<string>(PRODUCTION_NAME);
            StoreInConfig("name_pattern", woName);
            woName = GetName(woName, shifts.First().m_startPeriod, numRecurrence++, line.Name);
            
            // Create a WorkOrder
            CreateWol(revision, woName, line, shifts, data.Get<int>(PRODUCTION_QUANTITY));
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
    
    #region Other methods
    IList<string> m_listName = new List<string>();
    string GetName(string name, DateTime dateTime, int numRecurrence, string lineName)
    {
      if (numRecurrence == 1) {
        m_listName.Clear();
      }

      name = name.Replace("<%L%>", lineName)
        .Replace("<%Y%>", dateTime.Year.ToString())
        .Replace("<%M%>", dateTime.Month.ToString())
        .Replace("<%D%>", dateTime.Day.ToString())
        .Replace("<%W%>", CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
          dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString())
        .Replace("<%R%>", numRecurrence.ToString());
      
      if (m_listName.Contains(name)) {
        int increment = 2;
        while (m_listName.Contains(name + " (" + increment + ")")) {
          increment++;
        }

        name += " (" + increment + ")";
      }
      m_listName.Add(name);
      
      return name;
    }
    
    /// <summary>
    /// Get all workorderlines overlapping the specified period
    /// A workorderline period is defined this way:
    /// - beginning is the beginning of the workorderline
    /// - the end is the beginning of the day of the last intermediate work piece summary
    /// (+ 1 tick so that a new production will be strictly posterior => the last day cannot
    /// be fully used by the following production)
    /// </summary>
    /// <param name="line"></param>
    /// <param name="periodStart">beginning of the period</param>
    /// <param name="periodEnd">end of the period</param>
    /// <returns></returns>
    public static IDictionary<IWorkOrderLine, UtcDateTimeRange> GetWorkOrderLines(ILine line, DateTime periodStart, DateTime periodEnd)
    {
      IDictionary<IWorkOrderLine, UtcDateTimeRange> wols = new Dictionary<IWorkOrderLine, UtcDateTimeRange>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction("Settings.WizardCreateProduction.GetWorkOrderLines")) {
          // Get all wols overlapping the period [dateStart - dateEnd]
          IList<IWorkOrderLine> workOrderLines = null;
          workOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
            .GetListInRange(line, new UtcDateTimeRange(periodStart.ToUniversalTime(), periodEnd.ToUniversalTime()));
          
          // Compute the periods based on Begin + iwpss
          if (workOrderLines != null) {
            foreach (IWorkOrderLine wol in workOrderLines) {
              // Initialize start and end based on the begin datetime of the wol
              DateTime startDateTime = wol.BeginDateTime.Value;
              DateTime endDateTime = startDateTime;
              DateTime? endOfWol = wol.EndDateTime.NullableValue;
              
              // Find iwpss
              IList<IIntermediateWorkPieceTarget> iwpts =
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.FindByWorkOrderLine(wol.WorkOrder, wol.Line);
              foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
                if (iwpt.Number > 0 && iwpt.Day.HasValue) {
                  DateTime dayStart = ModelDAOHelper.DAOFactory.DaySlotDAO
                    .GetDayBegin (iwpt.Day.Value);
                  if ((!endOfWol.HasValue || dayStart <= endOfWol.Value) && dayStart > endDateTime) {
                    endDateTime = dayStart;
                  }
                }
              }
              
              // Keep overlapping periods only
              if (endDateTime > startDateTime && endDateTime > periodStart && startDateTime < periodEnd) {
                wols[wol] = new UtcDateTimeRange(startDateTime, endDateTime);
              }
            }
          }
          transaction.Commit ();
        }
      }
      
      return wols;
    }
    
    void DeleteWols(ILine line, IList<IWorkOrderLine> wols)
    {
      foreach (IWorkOrderLine wol in wols) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
            ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock(wol.WorkOrder);
            ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
            
            // Create a revision
            IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision();
            ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent(revision);
            
            // Delete all intermediate workpiece targets related to the line and workorder
            IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .FindByWorkOrderLine(wol.WorkOrder, line);
            foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.MakeTransient(iwpt);
            }

            // Delete the association
            DeleteWol (revision, line, wol);
            
            transaction.Commit();
          }
        }
      }
    }
    
    void DeleteWol(IRevision revision, ILine line, IWorkOrderLine wol)
    {
      // Remove the workorder from the component(s)
      IList<IComponent> components = line.Components.ToList();
      foreach (IComponent component in components) {
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);
        if (component.Part != null) {
          ModelDAOHelper.DAOFactory.PartDAO.Lock(component.Part);
          component.Part.RemoveWorkOrder(wol.WorkOrder);
        }
      }
      
      // Warning: Do not delete the work order !
      // Because there may be references in operation slot and some other summary tables
      // to the work orders
      // But they can be renamed...
      wol.WorkOrder.Name = wol.WorkOrder.Name + "(removed)";
      ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (wol.WorkOrder);
      
      // Find the previous wol
      IWorkOrderLine previousWol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAt(
        line, wol.BeginDateTime.Value.AddTicks(-1));
      
      if (previousWol != null)
      {
        // Postpone the end of the previous workorderline
        IWorkOrderLineAssociation association = ModelDAOHelper.ModelFactory.CreateWorkOrderLineAssociation(
          line, previousWol.BeginDateTime.Value, previousWol.Deadline);
        association.End = new UpperBound<DateTime>(wol.EndDateTime.NullableValue);
        association.WorkOrder = previousWol.WorkOrder;
        association.Quantity = previousWol.Quantity;
        foreach (IWorkOrderLineQuantity iwlq in previousWol.IntermediateWorkPieceQuantities.Values) {
          association.SetIntermediateWorkPieceQuantity(iwlq.IntermediateWorkPiece, iwlq.Quantity);
        }

        association.Revision = revision;
        ModelDAOHelper.DAOFactory.WorkOrderLineAssociationDAO.MakePersistent(association);
      }
      else
      {
        // Delete the period by a null value
        IWorkOrderLineAssociation association = ModelDAOHelper.ModelFactory.CreateWorkOrderLineAssociation(
          line, wol.BeginDateTime.Value, wol.Deadline);
        association.End = new UpperBound<DateTime>(wol.EndDateTime.NullableValue);
        association.WorkOrder = null;
        association.Quantity = 0;
        association.Revision = revision;
        ModelDAOHelper.DAOFactory.WorkOrderLineAssociationDAO.MakePersistent(association);
      }
    }
    
    void CreateWol(IRevision revision, String woName, ILine line, IList<ItemDataShift> shifts, int quantity)
    {
      // Create a WorkOrder
      IWorkOrder workOrder = ModelDAOHelper.ModelFactory.CreateWorkOrder(
        ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindById(1), // 1 is undefined
        woName);
      ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent(workOrder);
      
      // Create a WorkOrderLineAssociation
      IWorkOrderLineAssociation association = ModelDAOHelper.ModelFactory.CreateWorkOrderLineAssociation(
        line, shifts.First().m_startPeriod.ToUniversalTime(),
        shifts.Last().m_endPeriod.ToUniversalTime());
      ModelDAOHelper.DAOFactory.WorkOrderLineAssociationDAO.MakePersistent(association);
      association.Quantity = quantity;
      association.Revision = revision;
      association.WorkOrder = workOrder;
      
      // For each component
      int firstIndexQtt = 0;
      foreach (IComponent component in line.Components) {
        if (component.Part != null) {
          component.Part.AddWorkOrder(workOrder);
        }

        // For each IntermediateWorkPiece
        for (int numIwp = 0; numIwp < component.ComponentIntermediateWorkPieces.Count; numIwp++) {
          IIntermediateWorkPiece iwp = component.ComponentIntermediateWorkPieces.ToList()[numIwp].IntermediateWorkPiece;
          association.SetIntermediateWorkPieceQuantity(iwp, quantity);
        }
        
        // For each shift
        foreach (ItemDataShift shift in shifts) {
          if (shift.m_enabled) {
            // For each IntermediateWorkPiece
            for (int numIwp = 0; numIwp < component.ComponentIntermediateWorkPieces.Count; numIwp++) {
              if (shift.m_quantities[firstIndexQtt + numIwp] > 0) {
                IIntermediateWorkPiece iwp = component.ComponentIntermediateWorkPieces.ToList()[numIwp].IntermediateWorkPiece;
                
                // Create an IntermediateWorkPieceSummary
                // No need to check if an existing object comprises the same properties
                // -> here, the workorder is always new
                IIntermediateWorkPieceTarget iwpTarget = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPieceTarget(
                  iwp, component, workOrder, line, shift.m_day,
                  ModelDAOHelper.DAOFactory.ShiftDAO.FindById(shift.m_shiftID));
                iwpTarget.Number = shift.m_quantities[firstIndexQtt + numIwp];
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.MakePersistent(iwpTarget);
              }
            }
          }
        }
        
        firstIndexQtt += component.ComponentIntermediateWorkPieces.Count;
      }
    }
    #endregion // Private methods
  }
}
