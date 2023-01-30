// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
//  public partial class Page3 : GenericConfiguratorPage, IConfiguratorPage
  public partial class Page3 : UserControl, IConfiguratorPage
  {
    #region Members
    AlarmManager m_alarmManager = null;
    Alarm m_currentAlert;
    static readonly ILog log = LogManager.GetLogger (typeof (Page3).FullName);
    #endregion // Members

    public event Action<IRevision> DataChangedEvent;
    protected void EmitDataChangedEvent (IRevision revision)
    {
      DataChangedEvent (revision);
    }

    public event Action<bool> ProtectAgainstQuit;

    public event Action<string, string, string> LogAction;
    protected void EmitLogAction (string functionName, string dataDescription, string result)
    {
      LogAction (functionName, dataDescription, result);
    }

    public IList<Type> EditableTypes { get; }

    public event Action<string, IList<string>, bool> DisplayPageEvent;
    protected void EmitDisplayPageEvent (string page, IList<string> errors, bool ignorePossible)
    {
      DisplayPageEvent (page, errors, ignorePossible);
    }

    public event Action<string> SetTitle;

    public virtual string GetPageAfterValidation (ItemData data)
    {
      return null;
    }

    public event Action<System.Drawing.Color, string> SpecifyHeader;
    protected void EmitSpecifyHeader (System.Drawing.Color color, string text)
    {
      SpecifyHeader (color, text);
    }

    public void OnTimeOut ()
    {
      // Nothing
    }
    protected void EmitProtectAgainstQuit (bool isProtected)
    {
      // In view mode this event is not connected
      if (ProtectAgainstQuit != null) {
        ProtectAgainstQuit (isProtected);
      }
    }
    protected void EmitSetTitle (string text)
    {
      SetTitle (text);
    }


    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Edit an alert"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "In this page you can edit an alert:\n\n" +
      " - the event type and level,\n" +
      " - the activation time,\n" +
      " - the machines involved,\n" +
      " - the users which are subscribed.";
      }
    }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public LemSettingsGlobal.PageFlag Flags
    {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3 ()
    {
      InitializeComponent ();
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
      // Event types
      foreach (string dataType in SingletonEventLevel.GetDataTypes ()) {
        comboBoxEventType.AddItem (SingletonEventLevel.GetDisplayedName (dataType), dataType);
      }

      // Machine list
      treeViewMachines.ClearOrders ();
      treeViewMachines.AddOrder ("Sort by department", new[] {
        "Company",
        "Department"
      });
      treeViewMachines.AddOrder ("Sort by category", new[] {
        "Company",
        "Category"
      });
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      EmitProtectAgainstQuit (true);
      m_alarmManager = data.Get<AlarmManager> (Item.ALARM_MANAGER);
      m_currentAlert = data.Get<Alarm> (Item.CURRENT_ALERT);
      treeViewMachines.SelectedOrder = data.Get<int> (Item.TREEVIEW_MACHINE_ORDER);
      listUsers.ResetEditBox ();

      // Fill the interface
      if (m_currentAlert == null) {
        EmitSetTitle ("Create an alert");

        // Based on the filter
        textBoxName.Text = "new alarm";
        checkBoxActivated.Checked = true;
        comboBoxEventType.SelectedValue = SingletonEventLevel.DEFAULT_EVENT;
        comboBoxLevel.SelectedIndex = 0;
        comboboxInputItems.SelectedIndex = 0;
        dayInWeekPicker.DaysInWeek = 127;
        checkBoxStart.Checked = false;
        checkBoxExpiration.Checked = false;
        checkPeriodInDay.Checked = false;

        FillMachines (data.Get<IList<IMachine>> (Item.FILTER_MACHINES));
        FillUsers (data.Get<IList<EmailWithName>> (Item.FILTER_USERS));
      }
      else {
        EmitSetTitle ("Edit an alert");

        // Based on the current alarm
        textBoxName.Text = m_currentAlert.AlarmName;
        checkBoxActivated.Checked = m_currentAlert.AlarmActivated;
        comboBoxEventType.SelectedValue = m_currentAlert.DataType;
        if (comboBoxEventType.SelectedValue == null) {
          comboBoxEventType.SelectedText = m_currentAlert.DataType;
        }

        // LoadLevels ();
        LoadItems ();
        comboBoxLevel.SelectedValue = m_currentAlert.EventLevel;

        if (m_currentAlert.DataType.Contains ("CncAlarmBy")
          || m_currentAlert.DataType.Equals ("ReserveCapacityInfo")) {
          textBoxFilter.Text = m_currentAlert.Filter;
        }
        else {
          comboboxInputItems.SelectedValue = m_currentAlert.Filter;
        }

        dayInWeekPicker.DaysInWeek = m_currentAlert.DaysInWeek;
        if (m_currentAlert.DateStart.HasValue) {
          dateTimeStartDate.Value = dateTimeStartTime.Value = m_currentAlert.DateStart.Value;
          checkBoxStart.Checked = true;
        }
        else {
          checkBoxStart.Checked = false;
        }

        if (m_currentAlert.DateEnd.HasValue) {
          dateTimeEndDate.Value = dateTimeEndTime.Value = m_currentAlert.DateEnd.Value;
          checkBoxExpiration.Checked = true;
        }
        else {
          checkBoxExpiration.Checked = false;
        }

        if (m_currentAlert.TimePeriod.IsFullDay ()) {
          checkPeriodInDay.Checked = false;
        }
        else {
          checkPeriodInDay.Checked = true;
          periodInDayStart.Time = m_currentAlert.TimePeriod.Begin;
          periodInDayEnd.Time = m_currentAlert.TimePeriod.End;
        }
        FillMachines (m_currentAlert.ListMachine);
        FillUsers (m_currentAlert.ListEmails);
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (Item.TREEVIEW_MACHINE_ORDER, treeViewMachines.SelectedOrder);
      data.Store (Item.ALERT_NAME, textBoxName.Text);
      data.Store (Item.ALERT_IS_ACTIVATED, checkBoxActivated.Checked);
      string eventType = (comboBoxEventType.SelectedValue ?? "").ToString ();
      if (string.IsNullOrEmpty (eventType)) {
        eventType = comboBoxEventType.SelectedText;
      }

      data.Store (Item.ALERT_EVENT_TYPE, eventType);
      data.Store (Item.ALERT_EVENT_LEVEL, comboBoxLevel.SelectedValue);


      log.Debug ($"SavePageInData: xxxxx {eventType.GetType ()}");


      if (eventType.Contains ("CncAlarmBy")
        || eventType.Equals ("ReserveCapacityInfo")) {
        data.Store (Item.ALERT_FILTER, textBoxFilter.Text);
      }
      else {
        data.Store (Item.ALERT_FILTER, comboboxInputItems.SelectedValue);
      }
      data.Store (Item.ALERT_DAYS_IN_WEEK, dayInWeekPicker.DaysInWeek);
      if (checkBoxStart.Checked) {
        data.Store (Item.ALERT_START_DATE, dateTimeStartDate.Value.Date.Add (dateTimeStartTime.Time));
      }
      else {
        data.Store (Item.ALERT_START_DATE, null);
      }

      if (checkBoxExpiration.Checked) {
        data.Store (Item.ALERT_EXPIRATION, dateTimeEndDate.Value.Date.Add (dateTimeEndTime.Time));
      }
      else {
        data.Store (Item.ALERT_EXPIRATION, null);
      }

      if (checkPeriodInDay.Checked) {
        data.Store (Item.ALERT_PERIOD, new TimePeriodOfDay (periodInDayStart.Time, periodInDayEnd.Time, false));
      }
      else {
        data.Store (Item.ALERT_PERIOD, new TimePeriodOfDay ());
      }

      data.Store (Item.ALERT_MACHINES, treeViewMachines.SelectedElements.Cast<IMachine> ().ToList ());
      data.Store (Item.ALERT_ALL_MACHINES, checkAllMachines.Checked);
      data.Store (Item.ALERT_USERS, listUsers.GetCheckedEmails ());
    }

    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation (ItemData data)
    {
      IList<string> errors = new List<string> ();

      // Validity of the name
      string name = data.Get<string> (Item.ALERT_NAME);
      if (String.IsNullOrEmpty (name)) {
        errors.Add ("the name cannot be empty");
      }
      else if (data.Get<string> (Item.ALERT_NAME).Contains ("_")) {
        errors.Add ("the underscore \"_\" is forbidden in the alert name");
      }
      else if (IsAlarmNameAlreadyTaken (name)) {
        errors.Add ("the name is already taken by another alert");
      }

      // At least one machine and one user
      if (!data.Get<bool> (Item.ALERT_ALL_MACHINES) &&
          data.Get<IList<IMachine>> (Item.ALERT_MACHINES).Count == 0) {
        errors.Add ("at least one machine must be selected");
      }

      if (data.Get<IList<EmailWithName>> (Item.ALERT_USERS).Count == 0) {
        errors.Add ("at least one user must be selected");
      }

      // Begin date must be before end date
      DateTime? start = data.Get<DateTime?> (Item.ALERT_START_DATE);
      DateTime? end = data.Get<DateTime?> (Item.ALERT_EXPIRATION);
      if (start.HasValue && end.HasValue && start.Value >= end.Value) {
        errors.Add ("the end date must be posterior to the start date");
      }

      // Time period: start must be before the end
      TimePeriodOfDay period = data.Get<TimePeriodOfDay> (Item.ALERT_PERIOD);
      if (!period.IsValid ()) {
        errors.Add ("the beginning of the period during the day must be before the end");
      }

      // The event type must be provided
      if (String.IsNullOrEmpty (data.Get<string> (Item.ALERT_EVENT_TYPE))) {
        errors.Add ("the event type must be provided");
      }

      // At least one day in the week
      if (data.Get<int> (Item.ALERT_DAYS_IN_WEEK) == 0) {
        errors.Add ("at least one day must be checked within the week");
      }

      return errors;
    }

    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public void Validate (ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      // Remove current alarm from the database
      if (m_currentAlert != null) {
        AlarmManager.RemoveAlarm (m_currentAlert);
      }

      // Prepare a new alarm and save it
      AlarmManager.SaveAlarm (GetConfiguredAlarm (data));
    }

    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public void ProcessAfterValidation (ItemData data)
    {
      //EmitDataChangedEvent(null);
    }
    #endregion // Page methods

    #region Private methods
    void FillMachines (IList<IMachine> selectedMachines)
    {
      treeViewMachines.ClearElements ();
      treeViewMachines.SelectedElements = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAllNotObsolete ();
          // Insert all machines
          foreach (var machine in machines) {
            treeViewMachines.AddElement (machine);
          }
        }
      }
      treeViewMachines.RefreshTreeview ();
      if (selectedMachines != null && selectedMachines.Count > 0) {
        treeViewMachines.SelectedElements = selectedMachines.Cast<IDisplayable> ().ToList ();
        treeViewMachines.Enabled = true;
        checkAllMachines.Checked = false;
      }
      else {
        treeViewMachines.Enabled = false;
        checkAllMachines.Checked = true;
      }
    }

    void FillUsers (IList<EmailWithName> selectedUsers)
    {
      IList<EmailWithName> emails = m_alarmManager.ListEmails;

      using (var suspendDrawing = new SuspendDrawing (listUsers)) {
        listUsers.Clear ();
        // Insert all users
        foreach (EmailWithName email in emails) {
          int index = listUsers.AddEmail (email);
          if (selectedUsers != null && selectedUsers.Contains (email)) {
            listUsers.SetChecked (index, true);
          }
        }
      }
    }

    bool IsAlarmNameAlreadyTaken (string name)
    {
      // Check among all emailconfigs (not related to this alarm) that the name is not used
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          IList<IEmailConfig> emailConfigs = ModelDAOHelper.DAOFactory.EmailConfigDAO.FindAll ();

          foreach (IEmailConfig emailConfig in emailConfigs) {
            if (m_currentAlert == null || !m_currentAlert.Corresponds (emailConfig)) {
              if (emailConfig.Editor == AlarmManager.SOFTWARE_NAME) {
                // Remove the part of the name after the undescore
                string alarmName = emailConfig.Name;
                int lastUnderScoreIndex = alarmName.LastIndexOf ("_");
                if (lastUnderScoreIndex != -1) {
                  alarmName = alarmName.Substring (0, lastUnderScoreIndex);
                }
                if (alarmName == name) {
                  return true;
                }
              }
              else {
                // Don't need to remove the suffix
                if (emailConfig.Name == name) {
                  return true;
                }
              }
            }
          }
        }
      }

      return false;
    }

    void LoadLevels ()
    {
      { // Level list
        comboBoxLevel.ClearItems ();
        var value = comboBoxEventType.SelectedValue ?? "";
        foreach (IEventLevel level in SingletonEventLevel.GetLevels (value.ToString ())) {
          comboBoxLevel.AddItem (level.Display, level);
        }

        comboBoxLevel.SelectedIndex = 0;
        stackedWidget.SelectedIndex = (comboBoxLevel.Count == 0) ? 1 : 0;
      }
    }

    void LoadItems ()
    {
      // Input items
      comboboxInputItems.ClearItems ();
      var value = comboBoxEventType.SelectedValue ?? "";
      foreach (string item in SingletonEventLevel.GetInputItems (value.ToString ())) {
        comboboxInputItems.AddItem (item, item);
      }
      comboboxInputItems.SelectedIndex = 0;

    }

    Alarm GetConfiguredAlarm (ItemData data)
    {
      var alarm = new Alarm (data.Get<string> (Item.ALERT_NAME));
      alarm.DataType = data.Get<string> (Item.ALERT_EVENT_TYPE);
      alarm.EventLevel = data.Get<IEventLevel> (Item.ALERT_EVENT_LEVEL);
      alarm.Filter = data.Get<string> (Item.ALERT_FILTER);
      alarm.AlarmActivated = data.Get<bool> (Item.ALERT_IS_ACTIVATED);

      IList<IMachine> machines = data.Get<bool> (Item.ALERT_ALL_MACHINES) ?
        new List<IMachine> () : data.Get<IList<IMachine>> (Item.ALERT_MACHINES);
      foreach (IMachine machine in machines) {
        alarm.AddMachine (machine);
      }

      IList<EmailWithName> emails = data.Get<IList<EmailWithName>> (Item.ALERT_USERS);
      foreach (EmailWithName email in emails) {
        alarm.AddEmail (email);
      }

      if (data.Get<DateTime?> (Item.ALERT_START_DATE).HasValue) {
        alarm.DateStart = data.Get<DateTime?> (Item.ALERT_START_DATE).Value;
      }

      if (data.Get<DateTime?> (Item.ALERT_EXPIRATION).HasValue) {
        alarm.DateEnd = data.Get<DateTime?> (Item.ALERT_EXPIRATION).Value;
      }

      alarm.TimePeriod = data.Get<TimePeriodOfDay> (Item.ALERT_PERIOD);
      alarm.DaysInWeek = data.Get<int> (Item.ALERT_DAYS_IN_WEEK);

      return alarm;
    }
    #endregion // Private methods

    #region Event reactions
    void CheckBoxStartCheckedChanged (object sender, EventArgs e)
    {
      dateTimeStartDate.Enabled = dateTimeStartTime.Enabled = checkBoxStart.Checked;
    }

    void CheckBoxExpirationCheckedChanged (object sender, EventArgs e)
    {
      dateTimeEndDate.Enabled = dateTimeEndTime.Enabled = checkBoxExpiration.Checked;
    }

    void ComboBoxEventTypeItemChanged (string arg1, object arg2)
    {
      LoadLevels ();
      LoadItems ();

      if (arg1.Contains ("Cnc alarms by")
        || arg1.Equals ("Reserve capacity alert")) {
        this.comboboxInputItems.Hide ();
        this.textBoxFilter.Show ();
        this.labelFilter.Show ();
      }
      else {
        this.comboboxInputItems.Show ();
        this.textBoxFilter.Hide ();
        this.labelFilter.Hide ();
      }
    }

    void ComboBoxEventTypeTextChanged (string arg1)
    {
      LoadLevels ();
      LoadItems ();
    }

    void CheckPeriodInDayCheckedChanged (object sender, EventArgs e)
    {
      periodInDayStart.Enabled = periodInDayEnd.Enabled = checkPeriodInDay.Checked;
    }

    void CheckAllMachinesCheckedChanged (object sender, EventArgs e)
    {
      treeViewMachines.Enabled = !checkAllMachines.Checked;
    }
    #endregion // Event reactions

    private void baseLayout_Paint (object sender, PaintEventArgs e)
    {

    }

    private void textBoxName_TextChanged (object sender, EventArgs e)
    {

    }

    private void textBoxFilter_TextChanged (object sender, EventArgs e)
    {

    }
  }
}
