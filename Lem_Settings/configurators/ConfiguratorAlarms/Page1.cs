// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    Alarm m_currentAlert = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Alert list"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Email alerts are listed here. By default everything is displayed " +
          "unless you filter them with the button \"Filter\".\n\n" +
          "Each alert can be edited or deleted with the corresponding button.\n\n" +
          "You can also create a new alert."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      InitializeComponent();
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
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      var alarmManager = data.Get<AlarmManager>(Item.ALARM_MANAGER);
      alarmManager.loadData();
      IList<Alarm> alarms = alarmManager.getListAlarms(
        data.Get<string>(Item.FILTER_EVENT_TYPE),
        data.Get<IEventLevel>(Item.FILTER_LEVEL),
        data.Get<string>(Item.FILTER_ITEM),
        data.Get<IList<IMachine>>(Item.FILTER_MACHINES),
        data.Get<IList<EmailWithName>>(Item.FILTER_USERS));
      
      verticalScroll.Clear();
      bool alternate = false;
      if (alarms.Count > 0) {
        foreach (Alarm alarm in alarms) {
          var cell = new AlarmCell(alarm, true);
          cell.EditTriggered += OnAlarmEditTriggered;
          cell.DeleteTriggered += OnAlarmDeleteTriggered;
          cell.Dock = DockStyle.Fill;
          if (alternate) {
            cell.BackColor = LemSettingsGlobal.COLOR_ALTERNATE_ROW;
          }

          verticalScroll.AddControl(cell);
          
          alternate = !alternate;
        }
        stackedWidget.SelectedIndex = 0;
      } else {
        stackedWidget.SelectedIndex = 1;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.CURRENT_ALERT, m_currentAlert);
    }
    #endregion // Page methods
    
    #region Event reactions
    void ButtonFilterClick(object sender, EventArgs e)
    {
      EmitDisplayPageEvent("Page2", null);
    }
    
    void ButtonNewClick(object sender, EventArgs e)
    {
      // Creation of a new alarm
      m_currentAlert = null;
      EmitDisplayPageEvent("Page3", null);
    }
    
    void OnAlarmEditTriggered(Alarm alarm)
    {
      m_currentAlert = alarm;
      EmitDisplayPageEvent("Page3", null);
    }
    
    void OnAlarmDeleteTriggered(Alarm alarm)
    {
      if (MessageBoxCentered.Show(this, "Are you sure to delete this alert?",
                                  "Confirmation", MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
        AlarmManager.RemoveAlarm(alarm);
        EmitDisplayPageEvent("Page1", null);
      }
    }
    #endregion // Event reactions
  }
}
