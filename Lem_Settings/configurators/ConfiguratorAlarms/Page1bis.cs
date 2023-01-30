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
  /// Description of Page1bis.
  /// </summary>
  internal partial class Page1bis : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    Alarm m_currentAlert = null;
    bool m_initializing = true;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Alert list"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "First choose an email and all alerts linked to it will be displayed.\n\n" +
          "Each alert can be edited or deleted with the corresponding button.\n\n" +
          "You can also create a new alert."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1bis()
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
      // Get the alarm manager
      var alarmManager = data.Get<AlarmManager>(Item.ALARM_MANAGER);
      alarmManager.loadData();
      
      // Fill all alarms in the combobox
      IList<EmailWithName> emails = alarmManager.ListEmails;
      using (var suspendDrawing = new SuspendDrawing(comboboxEmailFilter))
      {
        comboboxEmailFilter.ClearItems();

        // Insert all users
        foreach (EmailWithName email in emails) {
          string txt = email.Email;
          if (!string.IsNullOrEmpty(email.Name)) {
            txt += " (" + email.Name + ")";
          }

          comboboxEmailFilter.AddItem(txt, email);
        }
        comboboxEmailFilter.InsertItem("Emails...", null, 0);
      }
      
      m_initializing = true;
      if (data.Get<IList<EmailWithName>>(Item.FILTER_USERS) != null &&
          data.Get<IList<EmailWithName>>(Item.FILTER_USERS).Count == 1) {
        comboboxEmailFilter.SelectedValue = data.Get<IList<EmailWithName>>(Item.FILTER_USERS)[0];

        IList<Alarm> alarms = alarmManager.getListAlarms(
          data.Get<string>(Item.FILTER_EVENT_TYPE),
          data.Get<IEventLevel>(Item.FILTER_LEVEL),
          data.Get<string>(Item.FILTER_ITEM),
          data.Get<IList<IMachine>>(Item.FILTER_MACHINES),
          data.Get<IList<EmailWithName>>(Item.FILTER_USERS));
        
        if (alarms.Count > 0) {
          verticalScroll.Clear();
          bool alternate = false;
          foreach (Alarm alarm in alarms) {
            var cell = new AlarmCell(alarm, false);
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
          stackedWidget.SelectedIndex = 2;
        }
      } else {
        comboboxEmailFilter.SelectedValue = null;
        stackedWidget.SelectedIndex = 1;
      }
      m_initializing = false;
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.CURRENT_ALERT, m_currentAlert);
      
      var emails = new List<EmailWithName>();
      if (comboboxEmailFilter.SelectedValue != null) {
        emails.Add((EmailWithName)comboboxEmailFilter.SelectedValue);
      }

      data.Store(Item.FILTER_USERS, emails);
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
        EmitDisplayPageEvent("Page1bis", null);
      }
    }
    
    void ComboboxEmailFilterItemChanged(string arg1, object arg2)
    {
      if (!m_initializing) {
        EmitDisplayPageEvent ("Page1bis", null);
      }
    }
    #endregion // Event reactions
  }
}
