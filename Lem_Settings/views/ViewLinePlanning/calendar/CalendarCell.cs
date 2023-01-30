// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLinePlanning
{
  /// <summary>
  /// Description of CalendarCell.
  /// </summary>
  public partial class CalendarCell : UserControl
  {
    #region Events
    public event Action<ILine> MarkerDoubleClicked;
    public event Action<DateTime> DayDoubleClicked;
    #endregion // Events
    
    #region Members
    DateTime m_currentDate;
    IList<CalendarMarker> m_markers = new List<CalendarMarker>();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CalendarCell()
    {
      InitializeComponent();
      verticalScrollLayout.ContainerDoubleClicked += EmitDayDoubleClick;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current date of the cell
    /// </summary>
    /// <param name="date"></param>
    /// <param name="brushes"></param>
    /// <param name="wolsExtended"></param>
    public void UpdateCell(DateTime date, DistinctBrushes brushes, IDictionary<IWorkOrderLine,
                           IList<IIntermediateWorkPieceTarget>> wolsExtended)
    {
      m_currentDate = date;
      verticalScrollLayout.Title = m_currentDate.Day.ToString();
      verticalScrollLayout.Clear();
      
      // Definition of the period
      DateTime startDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime(m_currentDate);
      DateTime endDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime(m_currentDate.AddDays(1));
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        foreach (IWorkOrderLine wol in wolsExtended.Keys) {
          ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
          
          // Wol present for the current day?
          bool valid = wol.Deadline > startDate && wol.BeginDateTime < endDate;
          if (!valid)
          {
            // Check within the IntermediateWorkPieceSummaries
            foreach (IIntermediateWorkPieceTarget iwpt in wolsExtended[wol]) {
              valid |= (iwpt.Day.Value.Date == m_currentDate.Date);
            }
          }
          
          if (valid) {
            string toolTip = "Line: " + wol.Line.Display + "\n" +
              "Production name: " + wol.WorkOrder.Display + "\n" +
              "Planned period: " + (wol.BeginDateTime.HasValue
                                    ? wol.BeginDateTime.Value.ToLocalTime().ToString("g")
                                    : "-oo")
              + " → " +
              wol.Deadline.ToLocalTime().ToString("g");
            var marker = new CalendarMarker(brushes.GetBrush(wol.Line.Id), toolTip, wol.Line);
            marker.Margin = new Padding(0, 0, 0, 0);
            marker.MarkerDoubleClicked += OnMarkerDoubleClicked;
            verticalScrollLayout.AddControl(marker);
          }
        }
      }
      
      verticalScrollLayout.UpdateScroll();
    }
    #endregion // Methods
    
    #region Event reactions
    void OnMarkerDoubleClicked(ILine line)
    {
      MarkerDoubleClicked(line);
    }
    
    void EmitDayDoubleClick()
    {
      DayDoubleClicked(m_currentDate);
    }
    #endregion // Event reactions
  }
}
