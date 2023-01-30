// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateProduction
{
  /// <summary>
  /// Creation of objects so that the planning of a line is represented:
  /// - segments showing the extent of productions
  /// - ellipses showing each target per day
  /// </summary>
  public class LineBarData: IBarObjectFactory
  {
    #region Getters / Setters
    /// <summary>
    /// Line being represented
    /// </summary>
    public ILine Line { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LineBarData()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Create all BarObjects included in a period
    /// </summary>
    /// <param name="start">beginning of the period</param>
    /// <param name="end">end of the period</param>
    /// <returns></returns>
    public IList<BarObject> CreateBarObjects(DateTime start, DateTime end)
    {
      IList<BarObject> barObjects = new List<BarObject>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
      {
        // All workorder lines planned
        IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.GetListInRange(Line, new UtcDateTimeRange(start, end));
        foreach (IWorkOrderLine wol in wols) {
          if (wol.Deadline.ToLocalTime() > start && wol.BeginDateTime.Value.ToLocalTime() < end) {
            barObjects.Add(new BarSegment(wol.BeginDateTime.Value.ToLocalTime(), wol.Deadline.ToLocalTime(),
                                          wol.Id, "Production: " + wol.WorkOrder.Display));
          }
        }
        
        // All configured targets
        IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
          .GetListInRange(start.Date.AddDays(-1), end.Date.AddDays(1));
        
        // Group configured targets by day, shift and workorder
        IDictionary<DateTime, IDictionary<IShift, IDictionary<IWorkOrder, IList<IIntermediateWorkPieceTarget>>>> dicIwpts =
          new Dictionary<DateTime, IDictionary<IShift, IDictionary<IWorkOrder, IList<IIntermediateWorkPieceTarget>>>>();
        foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
          if (iwpt.Number > 0 && iwpt.Line != null &&
              iwpt.WorkOrder != null && iwpt.Day.HasValue) {
            DateTime day = iwpt.Day.Value;
            if (!dicIwpts.ContainsKey(day)) {
              dicIwpts[day] = new Dictionary<IShift, IDictionary<IWorkOrder, IList<IIntermediateWorkPieceTarget>>>();
            }

            if (!dicIwpts[day].ContainsKey(iwpt.Shift)) {
              dicIwpts[day][iwpt.Shift] = new Dictionary<IWorkOrder, IList<IIntermediateWorkPieceTarget>>();
            }

            if (!dicIwpts[day][iwpt.Shift].ContainsKey(iwpt.WorkOrder)) {
              dicIwpts[day][iwpt.Shift][iwpt.WorkOrder] = new List<IIntermediateWorkPieceTarget>();
            }

            dicIwpts[day][iwpt.Shift][iwpt.WorkOrder].Add(iwpt);
          }
        }
        
        // Create bar cycles and sort them by day
        IDictionary<DateTime, IList<BarEllipse>> dicEllipses = new Dictionary<DateTime, IList<BarEllipse>>();
        foreach (DateTime day in dicIwpts.Keys)
        {
          if (!dicEllipses.ContainsKey(day)) {
            dicEllipses[day] = new List<BarEllipse>();
          }

          foreach (IShift shift in dicIwpts[day].Keys) {
            foreach (IWorkOrder wo in dicIwpts[day][shift].Keys) {
              IWorkOrderLine wol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(Line, wo);
              if (wol != null) {
                IList<IIntermediateWorkPieceTarget> iwptsTmp = dicIwpts[day][shift][wo];
                
                string text = "Production \"" + wo.Display + "\":";
                foreach (IIntermediateWorkPieceTarget iwpt in iwptsTmp) {
                  text += "\n - " + iwpt.Number + " part" + (iwpt.Number > 0 ? "s" : "") +
                    " for " + iwpt.IntermediateWorkPiece.Display;
                }

                text += "\n";
                
                if (shift != null) {
                  text += "\nShift: " + shift.Display;
                }

                text += "\nDay: " + day.ToString("d");
                
                DateTime dayBegin, dayEnd;
                using (IDAOTransaction transaction = session.BeginTransaction ("LineBarData"))
                {
                  dayBegin = ModelDAOHelper.DAOFactory.DaySlotDAO
                    .GetDayBegin (day).ToLocalTime ();
                  dayEnd = ModelDAOHelper.DAOFactory.DaySlotDAO
                    .GetDayEnd (day).ToLocalTime ();
                  transaction.Commit ();
                }
                dicEllipses[day].Add(new BarEllipse(
                  dayBegin, dayEnd,
                  wol.Id, text));
              }
            }
          }
          
          // Compute the vertical position of a circle and store them in barObjects
          foreach (IList<BarEllipse> barEllipses in dicEllipses.Values) {
            int count = barEllipses.Count;
            for (int i = 0; i < barEllipses.Count; i++) {
              barEllipses[i].VerticalPosition = (double)(i + 1) / (barEllipses.Count + 1);
              barObjects.Add(barEllipses[i]);
            }
          }
        }
      }
      
      return barObjects;
    }
    #endregion // Methods
  }
}
