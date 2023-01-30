// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of WolData.
  /// </summary>
  public class WolData
  {
    class IwptData
    {
      public IComponentIntermediateWorkPiece Ciwp { get; private set; }
      public DateTime Day { get; private set; }
      public int ShiftId { get; private set; }
      public int Target { get ; private set; }
      public ILine Line { get; private set; }
      public IShift Shift { get; private set; }
      
      public IwptData(IIntermediateWorkPieceTarget iwpt)
      {
        // Find component intermediate work piece
        Ciwp = ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.FindByComponentAndIwp(
          iwpt.Component, iwpt.IntermediateWorkPiece);
        
        if (iwpt.Day.HasValue) {
          Day = iwpt.Day.Value;
        }

        if (iwpt.Shift != null) {
          ShiftId = iwpt.Shift.Id;
        }
        else {
          ShiftId = -1;
        }

        Target = iwpt.Number;
        
        Line = iwpt.Line;
        Shift = iwpt.Shift;
      }
    }
    
    #region Members
    IList<IwptData> m_iwpts = new List<IwptData>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WolData).FullName);

    #region Getters / Setters
    /// <summary>
    /// WorkOrderLine
    /// </summary>
    public IWorkOrderLine WorkOrderLine { get; private set; }
    
    /// <summary>
    /// Duration of the production in days, without extra shifts
    /// </summary>
    public int PlannedProductionDays { get; private set; }
    
    /// <summary>
    /// Duration of the production in days, including extra shifts
    /// </summary>
    public int PlannedProductionWithExtraDays { get; private set; }
    
    /// <summary>
    /// Duration of the production in days, including extra shifts, + possible other days
    /// -1 means an unlimited number of days
    /// </summary>
    public int FullPossibleDays { get; private set; }
    
    /// <summary>
    /// Get the first day of the production
    /// 0 is sunday, 6 is saturday
    /// </summary>
    public int FirstDayOfWeek { get; private set; }
    
    /// <summary>
    /// Start date of the production (taking into account the cut-off day)
    /// </summary>
    public DateTime StartDate { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// (within a session)
    /// </summary>
    public WolData(ILine line, IWorkOrder workOrder)
    {
      // Find the workOrderLine and intermediateworkpiece targets
      WorkOrderLine = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(line, workOrder);
      IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
        .FindByWorkOrderLine(workOrder, line);
      foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
        if (iwpt.Day.HasValue && iwpt.Number > 0) {
          m_iwpts.Add(new IwptData(iwpt));
        }
      }

      // Compute the different number of production days
      DateTime firstDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay(WorkOrderLine.BeginDateTime.Value);
      DateTime lastDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay(WorkOrderLine.Deadline.AddTicks(-1));
      PlannedProductionDays = lastDate.Subtract(firstDate).Days + 1;
      
      foreach (IwptData iwps in m_iwpts) {
        if (iwps.Day > lastDate) {
          lastDate = iwps.Day;
        }
      }

      PlannedProductionWithExtraDays = lastDate.Subtract(firstDate).Days + 1;
      
      // Compute the full extent with extra possible days
      if (WorkOrderLine.EndDateTime.HasValue) {
        FullPossibleDays = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay(WorkOrderLine.EndDateTime.Value.AddTicks(-1))
          .Subtract(firstDate).Days + 1;
      }
      else {
        FullPossibleDays = -1;
      }

      // Store starting day
      StartDate = firstDate;
      FirstDayOfWeek = (int)firstDate.DayOfWeek;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Fill the quantities of the wol
    /// </summary>
    /// <param name="quantities"></param>
    /// <param name="firstFill"></param>
    public void FillQuantities(ref Dictionary<string, NullableDictionary<IShift, RowQuantities>> quantities, bool firstFill)
    {
      IList<string> keys = quantities.Keys.ToList();
      
      // First we consider non null values different
      foreach(string key in keys) {
        foreach (RowQuantities row in quantities[key].Values) {
          row.SetNotNullModified();
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IwptData iwpt in m_iwpts) {
            ModelDAOHelper.DAOFactory.ShiftDAO.Lock(iwpt.Shift);
            int dayNumber = GetDayNumber(iwpt.Day);
            
            // Do the day and shift exist?
            if (keys.Count > dayNumber && quantities[keys[dayNumber]].ContainsKey(iwpt.Shift)) {
              RowQuantities rowQuantities = quantities[keys[dayNumber]][iwpt.Shift];
              
              // Add a quantity and check if another value is already present
              if (rowQuantities.Quantities.ContainsKey(iwpt.Ciwp) && !firstFill) {
                rowQuantities.DifferentValues[iwpt.Ciwp] = (rowQuantities.Quantities[iwpt.Ciwp] != iwpt.Target);
              } else {
                rowQuantities.Quantities[iwpt.Ciwp] = iwpt.Target;
                rowQuantities.ModifiedStatus[iwpt.Ciwp] = false;
                rowQuantities.DifferentValues[iwpt.Ciwp] = false;
              }
              
              // If number of targets is 1, we set it to 0.
              // More than 2, it remains higher than 1 => conflict
              if (rowQuantities.NumberOfExternalTargets == 1) {
                rowQuantities.NumberOfExternalTargets = 0;
              }
            }
          }
        }
      }
    }
    
    int GetDayNumber(DateTime date)
    {
      return date.Subtract(StartDate).Days;
    }
    #endregion // Methods
  }
}
