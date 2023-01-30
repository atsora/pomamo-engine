// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of Quantities.
  /// </summary>
  public class Quantities
  {
    #region Members
    readonly IList<WolData> m_wolDatas = new List<WolData>();
    bool m_extentIssue = false;
    IDictionary<int, NullableDictionary<IShift, IDictionary<IComponentIntermediateWorkPiece, int>>> m_modifs =
      new Dictionary<int, NullableDictionary<IShift, IDictionary<IComponentIntermediateWorkPiece, int>>>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Quantities).FullName);

    #region Getters / Setters
    /// <summary>
    /// True if all shifts are displayed
    /// </summary>
    public bool DisplayAllShifts { get; set; }
    
    /// <summary>
    /// Period that may comprise all productions, in days
    /// </summary>
    public int DayNumber { get; private set; }
    
    /// <summary>
    /// Number of possible extra days
    /// </summary>
    public int ExtraDays { get; private set; }
    
    /// <summary>
    /// Get the first day of the productions
    /// 0 is sunday, 6 is saturday
    /// -1 is undefined (not all productions have the same first day)
    /// </summary>
    public int FirstDayOfWeek { get; private set; }
    
    /// <summary>
    /// Get all component intermediate workpieces
    /// a name is associated to an id
    /// </summary>
    public IDictionary<IComponentIntermediateWorkPiece, string> ComponentIntermediateWorkPieces { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Quantities()
    {
      DisplayAllShifts = false;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the data
    /// </summary>
    /// <param name="line"></param>
    /// <param name="wos"></param>
    public void Init(ILine line, IList<IWorkOrder> wos)
    {
      m_modifs.Clear();
      m_wolDatas.Clear();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          
          // Retrieve all WorkOrderLines with iwpt
          foreach (IWorkOrder wo in wos) {
            m_wolDatas.Add(new WolData(line, wo));
          }

          // Retrieve all intermediate workpieces
          ComponentIntermediateWorkPieces = new Dictionary<IComponentIntermediateWorkPiece, string>();
          foreach (IComponent component in line.Components) {
            foreach (IComponentIntermediateWorkPiece ciwp in component.ComponentIntermediateWorkPieces) {
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(ciwp.IntermediateWorkPiece);
              ComponentIntermediateWorkPieces[ciwp] = ciwp.IntermediateWorkPiece.Display;
            }
          }
        }
      }
      
      // Check time ranges
      int greatestPeriodWithExtra = -2;
      int shortestFullPossibleDays = -2;
      foreach (WolData wolData in m_wolDatas) {
        if (wolData.PlannedProductionWithExtraDays > greatestPeriodWithExtra) {
          greatestPeriodWithExtra = wolData.PlannedProductionWithExtraDays;
        }

        if (shortestFullPossibleDays == -2 || shortestFullPossibleDays == -1 ||
            (wolData.FullPossibleDays != -1 && wolData.FullPossibleDays < shortestFullPossibleDays)) {
          shortestFullPossibleDays = wolData.FullPossibleDays;
        }
      }
      
      if ((shortestFullPossibleDays != -1 && greatestPeriodWithExtra > shortestFullPossibleDays) || greatestPeriodWithExtra == -1) {
        m_extentIssue = true;
      }
      else {
        // Greatest period without extra
        int greatestPeriodWithoutExtra = -2;
        foreach (WolData wolData in m_wolDatas) {
          if (wolData.PlannedProductionDays > greatestPeriodWithoutExtra) {
            greatestPeriodWithoutExtra = wolData.PlannedProductionDays;
          }
        }

        DayNumber = greatestPeriodWithoutExtra;
        ExtraDays = (shortestFullPossibleDays == -1) ? -1 : shortestFullPossibleDays - greatestPeriodWithoutExtra;
        if (ExtraDays == -1 || ExtraDays > 7) {
          ExtraDays = 7;
        }

        m_extentIssue = false;
      }

      // Compute the first day of week
      FirstDayOfWeek = -2;
      foreach (WolData wolData in m_wolDatas) {
        if (FirstDayOfWeek == -2) {
          FirstDayOfWeek = wolData.FirstDayOfWeek;
        }
        else if (FirstDayOfWeek != wolData.FirstDayOfWeek) {
          FirstDayOfWeek = -1;
          break;
        }
      }
      if (FirstDayOfWeek == -2) {
        FirstDayOfWeek = -1;
      }
    }
    
    /// <summary>
    /// After the method "Init" is called, this method possibly adds errors to an existing list
    /// </summary>
    /// <param name="errors"></param>
    public void GetErrors(ref IList<string> errors)
    {
      // Primary checks
      if (m_wolDatas.Count == 0) {
        errors.Add("no production periods found");
      } else {
        if (m_extentIssue) {
          errors.Add("some periods are not compatible");
        }

        if (ComponentIntermediateWorkPieces.Keys.Count == 0) {
          errors.Add("no pieces are produced by the line");
        }
      }
    }
    
    /// <summary>
    /// Return the sum found per operation
    /// if null, the sum is not the same for all operations
    /// </summary>
    /// <returns></returns>
    public int? GetSumPerOperation()
    {
      // Compute the different sums
      IDictionary<string, IList<RowQuantities>> qtts = GetQuantities(null);
      IDictionary<IComponentIntermediateWorkPiece, int> sums = new Dictionary<IComponentIntermediateWorkPiece, int>();
      foreach (IList<RowQuantities> rows in qtts.Values) {
        foreach (RowQuantities row in rows) {
          foreach (IComponentIntermediateWorkPiece ciwp in row.Quantities.Keys) {
            if (!sums.ContainsKey(ciwp)) {
              sums[ciwp] = 0;
            }

            sums[ciwp] += row.Quantities[ciwp];
            if (!row.ModifiedStatus[ciwp] && row.DifferentValues[ciwp]) {
              return null;
            }
          }
        }
      }
      
      // Same everywhere?
      int sum = -1;
      foreach (int sumTmp in sums.Values) {
        if (sum == -1) {
          sum = sumTmp;
        }
        else if (sum != sumTmp) {
          return null;
        }
      }
      
      if (sum == -1) {
        return 0;
      }
      else {
        return sum;
      }
    }
    
    /// <summary>
    /// Return true if conflicts are present for the line
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public bool HasConflict(ILine line)
    {
      IDictionary<string, IList<RowQuantities>> qtts = GetQuantities(line);
      foreach (IList<RowQuantities> rows in qtts.Values) {
        foreach (RowQuantities row in rows) {
          if (row.NumberOfExternalTargets > 0 && !row.Empty) {
            return true;
          }
        }
      }

      return false;
    }
    
    /// <summary>
    /// Retrieve quantities of intermediate work pieces
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public IDictionary<string, IList<RowQuantities>> GetQuantities(ILine line)
    {
      // Prepare the container
      var quantities = new Dictionary<string, NullableDictionary<IShift, RowQuantities>>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          if (line != null) {
            ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          }

          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          for (int i = 0; i < DayNumber + ExtraDays; i++)
          {
            var dic = new NullableDictionary<IShift, RowQuantities>();
            
            // For each day, prepare a row per shift
            foreach (IShift shift in shifts) {
              dic[shift] = new RowQuantities(shift, shift.Display, ComponentIntermediateWorkPieces.Keys);
              
              // Initialization of the number of targets
              if (line != null) {
                int maxNumberOfTarget = 0;
                foreach (WolData wolData in m_wolDatas) {
                  int count = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                    .CountTargetsByLineShift(line, wolData.StartDate.AddDays(i), shift);
                  if (count > maxNumberOfTarget) {
                    maxNumberOfTarget = count;
                  }
                }
                
                dic[shift].NumberOfExternalTargets = maxNumberOfTarget;
              }
            }
            if (shifts.Count == 0) {
              dic[null] = new RowQuantities(null, "", ComponentIntermediateWorkPieces.Keys);
              
              // Initialization of the number of targets
              if (line != null) {
                int maxNumberOfTarget = 0;
                foreach (WolData wolData in m_wolDatas) {
                  int count = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                    .CountTargetsByLineShift(line, wolData.StartDate.AddDays(i), null);
                  if (count > maxNumberOfTarget) {
                    maxNumberOfTarget = count;
                  }
                }
                
                dic[null].NumberOfExternalTargets = maxNumberOfTarget;
              }
            }
            
            quantities[GetDayStr(i, false)] = dic;
          }
        }
      }
      
      // Retrieve quantities from the intermediate workpiece summaries
      bool firstFill = true;
      foreach (WolData wolData in m_wolDatas) {
        wolData.FillQuantities(ref quantities, firstFill);
        firstFill = false;
      }
      
      // Report the modifications
      foreach (int dayNumber in m_modifs.Keys) {
        foreach (IShift shift in m_modifs[dayNumber].Keys) {
          RowQuantities rowQuantities = quantities[quantities.Keys.ToList()[dayNumber]][shift];
          bool modified = false;
          foreach (IComponentIntermediateWorkPiece ciwp in m_modifs[dayNumber][shift].Keys) {
            rowQuantities.Quantities[ciwp] = m_modifs[dayNumber][shift][ciwp];
            rowQuantities.ModifiedStatus[ciwp] = true;
            modified = true;
          }
          if (modified && rowQuantities.NumberOfExternalTargets > 0) {
            rowQuantities.NumberOfExternalTargets += 1;
          }
        }
      }

      // Hide unused shifts if asked
      if (!DisplayAllShifts)
      {
        // List of used shifts
        IList<IShift> usedShifts = new List<IShift>();
        foreach (var dayQuantity in quantities.Values) {
          foreach (IShift shift in dayQuantity.Keys) {
            if (!usedShifts.Contains(shift)) {
              if (!dayQuantity[shift].Empty) {
                usedShifts.Add(shift);
              }
            }
          }
        }
        
        if (usedShifts.Count > 0) {
          // Remove unused shifts
          foreach (var dayQuantity in quantities.Values) {
            IList<IShift> shifts = dayQuantity.Keys.ToList();
            foreach (IShift shift in shifts) {
              if (!usedShifts.Contains(shift)) {
                dayQuantity.Remove(shift);
              }
            }
          }
        }
      }

      // Conversion
      IDictionary<string, IList<RowQuantities>> quantitiesRet = new Dictionary<string, IList<RowQuantities>>();
      foreach (string date in quantities.Keys) {
        quantitiesRet[date] = new List<RowQuantities>();
        foreach (IShift shift in quantities[date].Keys) {
          quantitiesRet[date].Add(quantities[date][shift]);
        }
      }
      
      return quantitiesRet;
    }
    
    /// <summary>
    /// Set a quantity
    /// </summary>
    /// <param name="dayNumber">Day number</param>
    /// <param name="shift">Shift, may be null</param>
    /// <param name="ciwp">component intermediate workpiece</param>
    /// <param name="newQuantity">quantity of the intermediate workpiece</param>
    public void SetQuantity(int dayNumber, IShift shift, IComponentIntermediateWorkPiece ciwp, int newQuantity)
    {
      if (!m_modifs.ContainsKey(dayNumber)) {
        m_modifs[dayNumber] = new NullableDictionary<IShift, IDictionary<IComponentIntermediateWorkPiece, int>>();
      }

      if (!m_modifs[dayNumber].ContainsKey(shift)) {
        m_modifs[dayNumber][shift] = new Dictionary<IComponentIntermediateWorkPiece, int>();
      }

      m_modifs[dayNumber][shift][ciwp] = newQuantity;
    }
    
    /// <summary>
    /// Delete a quantity modification
    /// </summary>
    /// <param name="date">day of the shift</param>
    /// <param name="shift">shift</param>
    /// <param name="ciwp">component intermediate workpiece</param>
    public void DeleteModifications(int date, IShift shift, IComponentIntermediateWorkPiece ciwp)
    {
      if (m_modifs.ContainsKey(date)) {
        if (m_modifs[date].ContainsKey(shift)) {
          if (m_modifs[date][shift].ContainsKey(ciwp)) {
            m_modifs[date][shift].Remove(ciwp);
          }

          if (m_modifs[date][shift].Count == 0) {
            m_modifs[date].Remove(shift);
          }
        }
        if (m_modifs[date].Count == 0) {
          m_modifs.Remove(date);
        }
      }
    }
    
    /// <summary>
    /// Textual description of the data
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary()
    {
      IList<string> summary = new List<string>();
      
      var shiftsDisplay = new Dictionary<IShift, string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          foreach (IShift shift in shifts) {
            shiftsDisplay[shift] = shift.Display;
          }
        }
      }
      
      List<int> dayNumbers = m_modifs.Keys.ToList();
      dayNumbers.Sort();
      foreach (int dayNumber in dayNumbers) {
        List<IShift> shifts = m_modifs[dayNumber].Keys.ToList();
        shifts.OrderBy(o => o.Id);
        bool hasModif = false;
        foreach (IShift shift in shifts) {
          string text = GetDayStr(dayNumber, true);
          if (shift != null) {
            text += " - shift \"" + shiftsDisplay[shift] + "\"";
          }

          List<IComponentIntermediateWorkPiece> ciwps = m_modifs[dayNumber][shift].Keys.ToList();
          ciwps.OrderBy(o => o.Order);
          foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
            int qtt = m_modifs[dayNumber][shift][ciwp];
            string plural = (qtt > 0) ? "s" : "";
            text += "\n" + qtt + " part" + plural + " for \"" + ComponentIntermediateWorkPieces[ciwp] + "\"";
            hasModif = true;
          }
          if (hasModif) {
            summary.Add(text);
          }
        }
      }
      
      if (summary.Count == 0) {
        summary.Add("no changes");
      }

      return summary;
    }
    
    /// <summary>
    /// Save all modifications
    /// This method has to be called in a transaction
    /// </summary>
    public void SaveModifications()
    {
      foreach (WolData wolData in m_wolDatas)
      {
        // WorkOrderLine
        IWorkOrderLine wol = wolData.WorkOrderLine;
        ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
        
        foreach (int dayNumber in m_modifs.Keys)
        {
          // Day
          DateTime day = wolData.StartDate.AddDays(dayNumber);
          
          foreach (IShift shift in m_modifs[dayNumber].Keys)
          {
            // Shift
            if (shift != null) {
              ModelDAOHelper.DAOFactory.ShiftDAO.Lock(shift);
            }

            foreach (IComponentIntermediateWorkPiece ciwp in m_modifs[dayNumber][shift].Keys)
            {
              // Intermediate work piece and component
              IIntermediateWorkPiece iwp = ciwp.IntermediateWorkPiece;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(iwp);
              IComponent component = ciwp.Component;
              ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);

              // We find or we create a new intermediate work piece target
              IIntermediateWorkPieceTarget iwpt = ModelDAOHelper.DAOFactory
                .IntermediateWorkPieceTargetDAO.FindByKey(iwp, component, wol.WorkOrder, wol.Line, day, shift);
              if (iwpt == null) {
                iwpt = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPieceTarget(iwp, component, wol.WorkOrder, wol.Line, day, shift);
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.MakePersistent(iwpt);
              }
              
              // Modification of the target
              iwpt.Number = m_modifs[dayNumber][shift][ciwp];
            }
          }
        }
      }
    }
    
    string GetDayStr(int numDay, bool oneLine)
    {
      string strRet;
      
      if (m_wolDatas.Count == 1) {
        strRet = m_wolDatas[0].StartDate.AddDays(numDay).ToShortDateString();
      }
      else {
        strRet = "Day " + (numDay + 1);
      }

      if (FirstDayOfWeek != -1) {
        if (oneLine) {
          strRet += ", ";
        }
        else {
          strRet += "\n";
        }

        switch ((FirstDayOfWeek + numDay) % 7) {
          case 0:
            strRet += "Sun";
            if (oneLine || numDay < DayNumber) {
            strRet += "day";
          }

          break;
          case 1:
            strRet += "Mon";
            if (oneLine || numDay < DayNumber) {
            strRet += "day";
          }

          break;
          case 2:
            strRet += "Tue";
            if (oneLine || numDay < DayNumber) {
            strRet += "day";
          }

          break;
          case 3:
            strRet += "Wed";
            if (oneLine || numDay < DayNumber) {
            strRet += "nesday";
          }

          break;
          case 4:
            strRet += "Thu";
            if (oneLine || numDay < DayNumber) {
            strRet += "rsday";
          }

          break;
          case 5:
            strRet += "Fri";
            if (oneLine || numDay < DayNumber) {
            strRet += "day";
          }

          break;
          case 6:
            strRet += "Sat";
            if (oneLine || numDay < DayNumber) {
            strRet += "urday";
          }

          break;
        }
        if (!oneLine && numDay >= DayNumber) {
          strRet += ".";
        }
      }
      
      if (numDay >= DayNumber) {
        if (FirstDayOfWeek == -1 && !oneLine) {
          strRet += "\n";
        }
        else {
          strRet += " ";
        }

        strRet += "(extra)";
      }
      
      return strRet;
    }
    #endregion // Methods
  }
}
