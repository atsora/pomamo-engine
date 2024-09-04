// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using System.Linq;
using System.Drawing;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of PageView.
  /// </summary>
  internal partial class PageView : GenericViewPage, IViewPage
  {
    /// <summary>
    /// Focus desired
    /// </summary>
    public enum FocusState
    {
      /// <summary>
      /// All kind of focus
      /// </summary>
      FOCUS_STATE_ALL,

      /// <summary>
      /// Only alarms that are important
      /// </summary>
      FOCUS_STATE_ONLY_IMPORTANT,

      /// <summary>
      /// Only alarms that we don't care
      /// </summary>
      FOCUS_STATE_ONLY_IRRELEVANT,

      /// <summary>
      /// Only alarms that we don't know if they are important or irrelevant
      /// </summary>
      FOCUS_STATE_ONLY_UNKNOWN
    }

    #region Members
    string m_currentCnc = "";
    FocusState m_currentFocus = FocusState.FOCUS_STATE_ALL;
    UtcDateTimeRange m_dateRange = null;
    bool m_blockSignals = true;
    IList<ICncAlarm> m_alarms = new List<ICncAlarm> ();
    IList<ICurrentCncAlarm> m_currentAlarms = new List<ICurrentCncAlarm> ();
    IList<ICncAlarm> m_alarmsFiltered = new List<ICncAlarm> ();
    IList<ICurrentCncAlarm> m_currentAlarmsFiltered = new List<ICurrentCncAlarm> ();
    IDictionary<int, bool?> m_severityFocusById = new Dictionary<int, bool?> ();
    IDictionary<int, string> m_severityNameById = new Dictionary<int, string> ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Alarms"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Current alarms are listed by day with their associated severity. " +
          "Alarms detected as important are displayed in red while alarms detected as irrelevant are in grey.\n\n" +
          "You can filter the alarms based on their CNC and / or importance.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageView ()
    {
      InitializeComponent ();

      // Focus state
      comboFocus.AddItem ("All", FocusState.FOCUS_STATE_ALL);
      comboFocus.AddItem ("Yes", FocusState.FOCUS_STATE_ONLY_IMPORTANT);
      comboFocus.AddItem ("No", FocusState.FOCUS_STATE_ONLY_IRRELEVANT);
      comboFocus.AddItem ("Unknown", FocusState.FOCUS_STATE_ONLY_UNKNOWN);
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context)
    {
      m_blockSignals = true;

      // All CNC types
      comboCnc.ClearItems ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var cncTypes = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindAllCncTypes ();
          foreach (var cncType in cncTypes) {
            comboCnc.AddItem (cncType, cncType);
          }
        }
      }
      comboCnc.InsertItem ("All", "", 0);

      // Load severities
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var severities = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindAll ();
          foreach (var severity in severities) {
            m_severityFocusById[severity.Id] = severity.Focus;
            m_severityNameById[severity.Id] = severity.Name;
          }
        }
      }

      m_blockSignals = false;
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_blockSignals = true;

      // Current focus
      m_currentFocus = data.Get<FocusState> (ItemView.CURRENT_FOCUS);
      comboFocus.SelectedValue = m_currentFocus;

      // Current cnc
      m_currentCnc = data.Get<string> (ItemView.CURRENT_CNC);
      if (m_currentCnc == null || !comboCnc.ContainsObject (m_currentCnc)) {
        m_currentCnc = "";
      }

      comboCnc.SelectedValue = m_currentCnc;
      m_blockSignals = false;

      // Date range
      m_dateRange = data.Get<UtcDateTimeRange> (ItemView.DATE_RANGE);

      // Display the alarms
      UpdateAlarms ();
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemView.CURRENT_CNC, m_currentCnc);
      data.Store (ItemView.CURRENT_FOCUS, m_currentFocus);
      data.Store (ItemView.DATE_RANGE, m_dateRange);
    }

    /// <summary>
    /// Method called every second
    /// </summary>
    public override void OnTimeOut ()
    {
      if (!m_dateRange.Upper.HasValue) {
        UpdateAlarms ();
      }
    }
    #endregion // Page methods

    #region Private methods
    void UpdateAlarms ()
    {
      if (m_blockSignals) {
        return;
      }

      if (m_dateRange.Upper.HasValue) {
        // Previous alarm slots
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            m_alarms = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindByCncRange (m_currentCnc, m_dateRange).ToList ();
          }
        }
        m_currentAlarms = null;
        m_currentAlarmsFiltered = null;
        buttonNextDay.Enabled = true;
        labelDate.Text = m_dateRange.Lower.Value.ToLocalTime ().ToString ("G") + " - " +
          m_dateRange.Upper.Value.ToLocalTime ().ToString ("G");
      }
      else {
        // Current alarms
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            m_currentAlarms = ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO.FindByCncWithSeverity (m_currentCnc);
          }
        }
        m_alarms = null;
        m_alarmsFiltered = null;
        buttonNextDay.Enabled = false;
        labelDate.Text = "now";
      }

      // Next step: filter them
      FilterAlarms ();
    }

    void FilterAlarms ()
    {
      if (m_blockSignals) {
        return;
      }

      if (m_currentAlarms != null) {
        // Filter current alarms
        switch (m_currentFocus) {
        case FocusState.FOCUS_STATE_ALL:
          m_currentAlarmsFiltered = m_currentAlarms;
          break;
        case FocusState.FOCUS_STATE_ONLY_IMPORTANT:
          m_currentAlarmsFiltered = new List<ICurrentCncAlarm> ();
          foreach (var alarm in m_currentAlarms) {
            if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id) &&
                m_severityFocusById[alarm.Severity.Id] == true) {
              m_currentAlarmsFiltered.Add (alarm);
            }
          }

          break;
        case FocusState.FOCUS_STATE_ONLY_IRRELEVANT:
          m_currentAlarmsFiltered = new List<ICurrentCncAlarm> ();
          foreach (var alarm in m_currentAlarms) {
            if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id) &&
                m_severityFocusById[alarm.Severity.Id] == false) {
              m_currentAlarmsFiltered.Add (alarm);
            }
          }

          break;
        case FocusState.FOCUS_STATE_ONLY_UNKNOWN:
          m_currentAlarmsFiltered = new List<ICurrentCncAlarm> ();
          foreach (var alarm in m_currentAlarms) {
            if (alarm.Severity == null || !m_severityFocusById.ContainsKey (alarm.Severity.Id) ||
                m_severityFocusById[alarm.Severity.Id] == null) {
              m_currentAlarmsFiltered.Add (alarm);
            }
          }

          break;
        }
      }
      else if (m_alarms != null) {
        // Filter alarms with a slot
        switch (m_currentFocus) {
        case FocusState.FOCUS_STATE_ALL:
          m_alarmsFiltered = m_alarms;
          break;
        case FocusState.FOCUS_STATE_ONLY_IMPORTANT:
          m_alarmsFiltered = new List<ICncAlarm> ();
          foreach (var alarm in m_alarms) {
            if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id) &&
                m_severityFocusById[alarm.Severity.Id] == true) {
              m_alarmsFiltered.Add (alarm);
            }
          }

          break;
        case FocusState.FOCUS_STATE_ONLY_IRRELEVANT:
          m_alarmsFiltered = new List<ICncAlarm> ();
          foreach (var alarm in m_alarms) {
            if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id) &&
                m_severityFocusById[alarm.Severity.Id] == false) {
              m_alarmsFiltered.Add (alarm);
            }
          }

          break;
        case FocusState.FOCUS_STATE_ONLY_UNKNOWN:
          m_alarmsFiltered = new List<ICncAlarm> ();
          foreach (var alarm in m_alarms) {
            if (alarm.Severity == null || !m_severityFocusById.ContainsKey (alarm.Severity.Id) ||
                m_severityFocusById[alarm.Severity.Id] == null) {
              m_alarmsFiltered.Add (alarm);
            }
          }

          break;
        }
      }
      else {
        throw new Exception ("Both lists are null");
      }

      if (m_alarmsFiltered != null && m_alarmsFiltered.Count == 0 ||
          m_currentAlarmsFiltered != null && m_currentAlarmsFiltered.Count == 0) {
        // Show "no alarms"
        stacked.SelectedIndex = 1;
      }
      else {
        // Update the grid
        UpdateGrid ();
        stacked.SelectedIndex = 0;
      }
    }

    void UpdateGrid ()
    {
      if (m_blockSignals) {
        return;
      }

      // First clear the table
      dataGrid.Rows.Clear ();

      // Column "CNC" only visible when "All" is chosen for CNC
      dataGrid.Columns[0].Visible = (comboCnc.SelectedIndex == 0);

      // Add a row for each alarm
      if (m_alarmsFiltered != null) {
        // Show "period"
        dataGrid.Columns[7].Visible = true;
        dataGrid.Columns[8].Visible = false;

        foreach (var alarm in m_alarmsFiltered) {
          dataGrid.Rows.Add (
            alarm.CncInfo,
            alarm.MachineModule.Name,
            alarm.Type,
            alarm.Number,
            FormatProperties (alarm.Properties),
            alarm.Message,
            alarm.Severity == null ? "-" :
              (m_severityNameById.ContainsKey (alarm.Severity.Id) ? m_severityNameById[alarm.Severity.Id] : "-"),
            alarm.DateTimeRange == null ? "-" : alarm.DateTimeRange.ToLocalTime ().ToString (),
            "-");

          if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id)) {
            var cellStyle = dataGrid.Rows[dataGrid.Rows.Count - 1].DefaultCellStyle;
            if (m_severityFocusById[alarm.Severity.Id] == true) {
              cellStyle.ForeColor = Color.Red;
              cellStyle.BackColor = Color.PapayaWhip;
            }
            else if (m_severityFocusById[alarm.Severity.Id] == false) {
              cellStyle.ForeColor = SystemColors.ControlDarkDark;
              cellStyle.BackColor = SystemColors.ControlLight;
            }
          }
        }
      }
      else if (m_currentAlarmsFiltered != null) {
        // Show "last seen"
        dataGrid.Columns[7].Visible = false;
        dataGrid.Columns[8].Visible = true;

        foreach (var alarm in m_currentAlarmsFiltered) {
          dataGrid.Rows.Add (
            alarm.CncInfo,
            alarm.MachineModule.Name,
            alarm.Type,
            alarm.Number,
            FormatProperties (alarm.Properties),
            alarm.Message,
            alarm.Severity == null ? "-" :
              (m_severityNameById.ContainsKey (alarm.Severity.Id) ? m_severityNameById[alarm.Severity.Id] : "-"),
            "-",
            alarm.DateTime.ToLocalTime ().ToString ());

          if (alarm.Severity != null && m_severityFocusById.ContainsKey (alarm.Severity.Id)) {
            var cellStyle = dataGrid.Rows[dataGrid.Rows.Count - 1].DefaultCellStyle;
            if (m_severityFocusById[alarm.Severity.Id] == true) {
              cellStyle.ForeColor = Color.Red;
              cellStyle.BackColor = Color.PapayaWhip;
            }
            else if (m_severityFocusById[alarm.Severity.Id] == false) {
              cellStyle.ForeColor = SystemColors.ControlDarkDark;
              cellStyle.BackColor = SystemColors.ControlLight;
            }
          }
        }
      }
    }

    string FormatProperties (IDictionary<string, object> properties)
    {
      string txt = "";

      foreach (var key in properties.Keys) {
        if (txt != "") {
          txt += ", ";
        }

        txt += key + ": " + (properties[key] ?? "null");
      }

      return txt;
    }
    #endregion // Private methods

    #region Event reactions
    void ComboCncItemChanged (string arg1, object arg2)
    {
      m_currentCnc = (string)arg2;
      UpdateAlarms ();
    }

    void ComboFocusItemChanged (string arg1, object arg2)
    {
      m_currentFocus = (FocusState)arg2;
      FilterAlarms ();
    }

    void ButtonPreviousDayClick (object sender, EventArgs e)
    {
      if (m_blockSignals) {
        return;
      }

      // Go to the previous day
      try {
        if (m_dateRange.Upper.HasValue) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              var slot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindAt (m_dateRange.Lower.Value.AddHours (-1));
              if (slot != null) {
                m_dateRange = slot.DateTimeRange;
              }

              transaction.Commit ();
            }
          }
        }
        else {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_dateRange = ModelDAOHelper.DAOFactory.DaySlotDAO.GetTodayRange ();
              transaction.Commit ();
            }
          }
        }
      }
      catch (Exception) {
        buttonPreviousDay.Enabled = false;
      }

      buttonNextDay.Enabled = true;
      UpdateAlarms ();
    }

    void ButtonNextDayClick (object sender, EventArgs e)
    {
      if (m_blockSignals) {
        return;
      }

      // Go to the next day
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            var todayRange = ModelDAOHelper.DAOFactory.DaySlotDAO.GetTodayRange ();
            if (object.Equals (m_dateRange, todayRange)) {
              m_dateRange = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));
            }
            else {
              var slot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindAt (m_dateRange.Upper.Value.AddHours (1));
              if (slot != null) {
                m_dateRange = slot.DateTimeRange;
              }
            }
            transaction.Commit ();
          }
        }
      }
      catch (Exception) {
        buttonNextDay.Enabled = false;
      }

      buttonPreviousDay.Enabled = true;
      UpdateAlarms ();
    }
    #endregion // Event reactions
  }
}
