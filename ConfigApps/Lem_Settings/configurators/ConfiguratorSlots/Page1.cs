// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IPartPage1 m_part = null;
    bool m_viewMode = false;
    bool m_askForRefresh = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Timeline"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        string txt = m_part.HelpSub;
        if (!m_viewMode) {
          txt += "\n\nThe button \"Add slot\" shows a page where you will be able to configure " +
            "a modification on the timeline.";
          if (!String.IsNullOrEmpty(m_part.ElementsName)) {
            txt += " The modification will be applied to all " + m_part.ElementsName + " displayed (not the others).";
          }
        }
        txt += "\n\nBy using the mouse wheel and holding the \"ctrl\" key you will be able to zoom in the graphics.";
        return txt;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1(IPartPage1 part)
    {
      m_part = part;
      InitializeComponent();
      timelinesWidget.PeriodChanged += PeriodChanged;
      timelinesWidget.SelectedPeriodChanged += SelectedPeriodChanged;
      if (!String.IsNullOrEmpty(m_part.ElementsName)) {
        buttonFilter.Text = "Change " + m_part.ElementsName;
      }
      else {
        buttonFilter.Hide();
      }
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
      m_askForRefresh = true; // Force a refresh on the next "LoadPageFromData"
      m_viewMode = context.ViewMode;
      if (m_viewMode) {
        baseLayout.ColumnStyles[3].Width = 0;
        baseLayout.ColumnStyles[1].Width = 120;
        buttonModify.Hide();
      } else {
        baseLayout.ColumnStyles[1].Width = 60;
        baseLayout.ColumnStyles[3].Width = 60;
        buttonModify.Show();
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      if (data.Get<bool>(AbstractItem.TIMELINE_UPDATE) || m_askForRefresh) {
        timelinesWidget.Clear();
        IList<object> items = data.Get<List<object>>(AbstractItem.SELECTED_ITEMS);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            foreach (object item in items) {
              m_part.AddTimeLine(timelinesWidget, item);
            }
          }
        }

        timelinesWidget.SetPeriod(data.Get<DateTime>(AbstractItem.TIMELINE_START),
                                  data.Get<DateTime>(AbstractItem.TIMELINE_END));
      }
      m_askForRefresh = false;
      
      // Selected period
      if (data.Get<bool>(AbstractItem.PERIOD_HAS_END)) {
        timelinesWidget.SetSelectedPeriod(data.Get<DateTime>(AbstractItem.PERIOD_START),
                                          data.Get<DateTime>(AbstractItem.PERIOD_END));
      } else {
        timelinesWidget.SetSelectedPeriod(data.Get<DateTime>(AbstractItem.PERIOD_START), null);
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.TIMELINE_UPDATE, false);
      data.Store(AbstractItem.TIMELINE_START, timelinesWidget.StartDateTime);
      data.Store(AbstractItem.TIMELINE_END, timelinesWidget.EndDateTime);
      data.Store(AbstractItem.PERIOD_START, dateStart.Value.Date.Add(timeStart.Value.TimeOfDay));
      data.Store(AbstractItem.PERIOD_END, dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay));
      data.Store(AbstractItem.PERIOD_HAS_END, checkEnd.Checked);
    }
    
    void PeriodChanged(DateTime startDateTime, DateTime endDateTime)
    {
      // Update legend
      IDictionary<Brush, string> brushes = BarSegment.GetLegend();
      using (new SuspendDrawing(verticalScrollLayout))
      {
        // Clear the content
        verticalScrollLayout.Clear();
        
        // Populate
        verticalScrollLayout.ColumnCount = 2;
        verticalScrollLayout.RowCount = brushes.Count;
        int row = 0;
        foreach (Brush brush in brushes.Keys) {
          Marker marker = new Marker();
          marker.Brush = brush;
          marker.Width = 16;
          marker.Height = 16;
          marker.Margin = new Padding(0);
          verticalScrollLayout.AddControl(marker, 0, row);
          
          Label label = new Label();
          label.AutoEllipsis = true;
          label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
          label.Text = brushes[brush];
          label.Margin = new Padding(0);
          verticalScrollLayout.AddControl(label, 1, row++);
        }
      }
    }
    #endregion // Page methods
    
    #region Event reactions
    void ButtonFilterClick(object sender, EventArgs e)
    {
      EmitDisplayPageEvent("Page2", null);
    }
    
    void ButtonModifyClick(object sender, EventArgs e)
    {
      IList<string> errors = new List<string>();
      if (checkEnd.Checked && dateStart.Value.Date.Add(timeStart.Value.TimeOfDay) >=
          dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay)) {
        errors.Add("end date must be posterior to the start date");
      }

      EmitDisplayPageEvent ("Page3", errors);
    }
    
    void SelectedPeriodChanged(DateTime? start, DateTime? end)
    {
      if (start.HasValue) {
        dateStart.Value = timeStart.Value = start.Value;
      }
      else {
        dateStart.Value = timeStart.Value = new DateTime(1970, 1, 1);
      }

      if (end.HasValue) {
        dateEnd.Value = timeEnd.Value = end.Value;
      }

      checkEnd.Checked = end.HasValue;
    }
    
    void CheckEndCheckedChanged(object sender, EventArgs e)
    {
      dateEnd.Enabled = timeEnd.Enabled = checkEnd.Checked;
      SelectedPeriodChanged(sender, e);
    }
    
    void SelectedPeriodChanged(object sender, EventArgs e)
    {
      DateTime start = dateStart.Value.Date.Add(timeStart.Value.TimeOfDay);
      DateTime? end = null;
      if (checkEnd.Checked) {
        end = dateEnd.Value.Date.Add(timeEnd.Value.TimeOfDay);
      }

      if (!end.HasValue || end.Value > start) {
        timelinesWidget.SetSelectedPeriod(start, end);
      }
    }
    #endregion // Event reactions
  }
}
