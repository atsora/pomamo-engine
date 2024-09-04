// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLinePlanning
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericViewPage, IViewPage
  {
    #region Members
    ILine m_lineDoubleClicked;
    DateTime m_currentDate;
    DistinctBrushes m_brushes;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Line planning per month"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Double-clicking on a day opens the details for a day.\n\n" +
          "Double-clicking on a line in a day shows the details of this line."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      InitializeComponent();
      Utils.SetDoubleBuffered(tableCalendar);
      for (int i = 0; i < 7; i++) {
        for (int j = 1; j < 7; j++) {
          CalendarCell cell = new CalendarCell();
          cell.MarkerDoubleClicked += OnMarkerDoubleClicked;
          cell.DayDoubleClicked += OnDayDoubleClicked;
          cell.Dock = DockStyle.Fill;
          cell.Margin = new Padding(1, 1, i == 6 ? 1: 0, j == 6 ? 1 : 0);
          tableCalendar.Controls.Add(cell, i, j);
        }
      }
      
      tableCalendar.CellPaint += PaintCellTableCalendar;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_lineDoubleClicked = data.Get<ILine>(Item.LINE);
      m_currentDate = data.Get<DateTime>(Item.DATE);
      m_brushes = data.Get<DistinctBrushes>(Item.BRUSHES);
      updateInterface();
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.DATE, m_currentDate);
      data.Store(Item.LINE, m_lineDoubleClicked);
    }
    #endregion // Page methods
    
    #region Private methods
    void updateInterface()
    {
      IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> wolsExtended;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Definition of the period of time corresponding to the month
          DateTime startDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime(
            new DateTime(m_currentDate.Year, m_currentDate.Month, 1));
          DateTime endDate = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime(
            new DateTime(m_currentDate.Year, m_currentDate.Month, 1).AddMonths(1));
          
          // WorkOrderLine list associated with their planned days / shifts
          wolsExtended = Item.GetExtendedWols(startDate, endDate);
        }
      }
      
      InitCalendar(wolsExtended);
      InitLegend(wolsExtended);
    }
    
    void InitCalendar(IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> wolsExtended)
    {
      int row = 1;
      int column = 0;
      CalendarCell cell;
      DateTime date = new DateTime(m_currentDate.Year, m_currentDate.Month, 1);
      labelDate.Text = date.ToString("MMMM yyyy");
      
      using (new SuspendDrawing(tableCalendar))
      {
        while (date.Month == m_currentDate.Month)
        {
          // Day of week, monday being 0
          column = ((int)date.DayOfWeek + 6) % 7;
          
          // Visibility of the first cells
          if (date.Day == 1) {
            for (int i = 0; i < column; i++) {
              cell = (CalendarCell)tableCalendar.GetControlFromPosition(i, row);
              cell.Visible = false;
            }
          }
          cell = (CalendarCell)tableCalendar.GetControlFromPosition(column, row);
          cell.UpdateCell(date, m_brushes, wolsExtended);
          cell.Visible = true;
          
          if (column == 6) {
            row++;
          }

          date = date.AddDays(1);
        }
        
        // Visibility of the last cells
        column = (column + 1) % 7;
        for (; row < 7; row++) {
          while (column < 7) {
            cell = tableCalendar.GetControlFromPosition(column, row) as CalendarCell;
            cell.Visible = false;
            column++;
          }
          column = 0;
        }
      }
    }
    
    // Within a session
    void InitLegend(IDictionary<IWorkOrderLine, IList<IIntermediateWorkPieceTarget>> wolsExtended)
    {
      // List of lines
      IList<ILine> lines = new List<ILine>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IWorkOrderLine wol in wolsExtended.Keys) {
            ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
            
            bool alreadyInList = false;
            foreach (ILine line in lines) {
              alreadyInList |= (line.Id == wol.Line.Id);
            }

            if (!alreadyInList) {
              lines.Add(wol.Line);
            }
          }
          lines = lines.OrderBy(o=>o.Display).ToList();
        }
      }
      
      using (new SuspendDrawing(verticalScrollLayout)) {
        // Style - reset
        verticalScrollLayout.Clear();
        verticalScrollLayout.ColumnCount = 2;
        verticalScrollLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
        verticalScrollLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        verticalScrollLayout.RowCount = lines.Count;

        // Create an element for each line
        int row = 0;
        foreach (ILine line in lines) {
          // Marker
          CalendarMarker marker = new CalendarMarker(m_brushes.GetBrush(line.Id), line);
          marker.Margin = new Padding(2, 2, 0, 0);
          marker.MarkerDoubleClicked += OnMarkerDoubleClicked;
          verticalScrollLayout.AddControl(marker, 0, row);

          // Text
          Label label = new Label();
          label.Text = line.Display;
          label.TextAlign = ContentAlignment.MiddleLeft;
          label.Dock = DockStyle.Fill;
          label.Height = 13;
          verticalScrollLayout.AddControl(label, 1, row++);
        }
      }
      verticalScrollLayout.UpdateScroll();
    }
    #endregion // Private methods
    
    #region Event reactions
    void PaintCellTableCalendar(object sender, TableLayoutCellPaintEventArgs e)
    {
      var panel = sender as System.Windows.Forms.TableLayoutPanel;
      e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
      var rectangle = e.CellBounds;
      using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
      {
        pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        
        if (e.Row == (panel.RowCount - 1)) {
          rectangle.Height -= 1;
        }

        if (e.Column == (panel.ColumnCount - 1)) {
          rectangle.Width -= 1;
        }

        e.Graphics.DrawRectangle(pen, rectangle);
      }
    }
    
    void ButtonPreviousMonthClick(object sender, EventArgs e)
    {
      m_currentDate = m_currentDate.AddMonths(-1);
      updateInterface();
    }
    
    void ButtonNextMonthClick(object sender, EventArgs e)
    {
      m_currentDate = m_currentDate.AddMonths(1);
      updateInterface();
    }
    
    void OnMarkerDoubleClicked(ILine line)
    {
      m_lineDoubleClicked = line;
      EmitDisplayPageEvent("Page3", null);
    }
    
    void OnDayDoubleClicked(DateTime date)
    {
      m_currentDate = date;
      EmitDisplayPageEvent("Page2", null);
    }
    #endregion // Event reactions
  }
}
