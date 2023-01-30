// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of Alarm.
  /// </summary>
  public class Alarm
  {
    #region Members
    int m_daysInWeek;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Alarm).FullName);

    #region Getters / Setters
    /// <summary>
    /// Alarm name
    /// </summary>
    public string AlarmName { get; set; }
    
    /// <summary>
    /// Activated state of the alarm
    /// </summary>
    public bool AlarmActivated { get; set; }
    
    /// <summary>
    /// Time period during the day when the alarm is activated
    /// </summary>
    public TimePeriodOfDay TimePeriod { get; set; }
    
    /// <summary>
    /// Start date time of the alarm.
    /// Not activated before.
    /// Null if not used
    /// </summary>
    public DateTime? DateStart { get; set; }
    
    /// <summary>
    /// End date time of the alarm.
    /// Not activated then.
    /// Null if not used.
    /// </summary>
    public DateTime? DateEnd { get; set; }
    
    /// <summary>
    /// True if the alarm is expired
    /// </summary>
    public bool IsExpired { get {
        return DateEnd != null && DateEnd < DateTime.Now;
      }
    }
    
    /// <summary>
    /// List of all machines targeted by the alarm.
    /// If null -> all machines
    /// </summary>
    public List<IMachine> ListMachine { get; private set; }
    
    /// <summary>
    /// List of all emails used by the alarm.
    /// </summary>
    public List<EmailWithName> ListEmails { get; private set; }
    
    /// <summary>
    /// Event type of the alarm (event long period, cnc value...)
    /// </summary>
    public string DataType { get; set; }
    
    /// <summary>
    /// Days in week, in the range [0 ; 127]
    /// </summary>
    public int DaysInWeek {
      get { return m_daysInWeek; }
      set {
        if (value >= 0 && value < 128) {
          m_daysInWeek = value;
        }
        else {
          m_daysInWeek = 127;
        }
      }
    }
    
    /// <summary>
    /// Event level among those permitted by the event type
    /// (critical, information, notice...)
    /// </summary>
    public IEventLevel EventLevel { get; set; }
    
    /// <summary>
    /// Filter
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// Ids of the emailConfig rows in the database
    /// that compose the alarm
    /// </summary>
    public List<int> ListId { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor of the alarm, taking as parameter its name
    /// </summary>
    /// <param name="name"></param>
    public Alarm(string name)
    {
      AlarmName = name;
      AlarmActivated = true;
      TimePeriod = new TimePeriodOfDay();
      DateEnd = null;
      ListId = new List<int>();
      ListMachine = null;
      ListEmails = new List<EmailWithName>();
      m_daysInWeek = 127;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an emailConfig id to the alarm
    /// </summary>
    /// <param name="id"></param>
    public void AddId(int id)
    {
      if (!ListId.Contains(id)) {
        ListId.Add(id);
      }
    }
    
    /// <summary>
    /// Add an email to the alarm, if not already present
    /// </summary>
    /// <param name="email"></param>
    public void AddEmail(EmailWithName email)
    {
      if (!ListEmails.Contains(email)) {
        ListEmails.Add(email);
      }
    }
    
    /// <summary>
    /// Remove an email from the alarm, if present
    /// </summary>
    /// <param name="email"></param>
    public void RemoveEmail(EmailWithName email)
    {
      ListEmails.Remove(email);
    }
    
    /// <summary>
    /// Add a machine to the alarm, if not already present
    /// </summary>
    /// <param name="machine"></param>
    public void AddMachine(IMachine machine)
    {
      if (machine != null) {
        if (ListMachine == null) {
          ListMachine = new List<IMachine>();
        }

        if (!ListMachine.Contains(machine)) {
          ListMachine.Add(machine);
        }
      }
    }
    
    /// <summary>
    /// Remove a machine from the alarm, if present
    /// </summary>
    /// <param name="machine"></param>
    public void RemoveMachine(IMachine machine)
    {
      if (ListMachine != null) {
        ListMachine.Remove(machine);
      }
    }
    
    /// <summary>
    /// Merge two alarms having the same name
    /// Machines, Ids and emails are merged
    /// </summary>
    /// <param name="other"></param>
    public void Merge(Alarm other)
    {
      IList<IMachine> otherMachines = other.ListMachine ?? new List<IMachine>();
      foreach (IMachine machine in otherMachines) {
        this.AddMachine(machine);
      }
      
      IList<EmailWithName> otherEmails = other.ListEmails;
      foreach (EmailWithName email in otherEmails) {
        this.AddEmail(email);
      }
      
      IList<int> otherIds = other.ListId;
      foreach (int id in otherIds) {
        this.AddId(id);
      }
    }
    
    /// <summary>
    /// Alarms are equal if they have the same name
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      var other = obj as Alarm;
      if (other == null) {
        return false;
      }

      return this.AlarmName == other.AlarmName;
    }
    
    /// <summary>
    /// Hashcode of an alarm
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (AlarmName != null) {
          hashCode += 1000000007 * AlarmName.GetHashCode();
        }

        hashCode += 1000000009 * AlarmActivated.GetHashCode();
        hashCode += 1000000021 * TimePeriod.GetHashCode();
        hashCode += 1000000087 * DateStart.GetHashCode();
        hashCode += 1000000093 * DateEnd.GetHashCode();
        if (ListMachine != null) {
          hashCode += 1000000097 * ListMachine.GetHashCode();
        }

        if (ListEmails != null) {
          hashCode += 1000000103 * ListEmails.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// Textual description of an alarm
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("[Alarm AlarmName={0}, AlarmActivated={1}, PeriodStart={2}, PeriodEnd={3}, DateStart={4}, DateEnd={5}, ListMachineId={6}, ListEmails={7}]",
                           AlarmName, AlarmActivated, TimePeriod.Begin, TimePeriod.End, DateStart, DateEnd, ListMachine, ListEmails);
    }
    
    /// <summary>
    /// Formatted description of an alarm, which is displayed in the GUI
    /// </summary>
    /// <param name="numLine"></param>
    /// <returns></returns>
    public string GetLine(int numLine)
    {
      string text = "";
      
      switch (numLine) {
        case 1:
          text = AlarmName + " (" + SingletonEventLevel.GetDisplayedName(DataType) + ", " + EventLevel.Display + ")";
          if (!AlarmActivated) {
          text += " - not activated";
        }

        break;
        case 2:
          text = FormatMachines();
          break;
        case 3:
          text = FormatUsers();
          break;
        case 4:
          text = FormatPeriods();
          break;
        case 5:
          text = FormatBeginEnd();
          break;
      }
      
      return text;
    }
    
    string FormatMachines()
    {
      string text = "";
      
      if (ListMachine == null || ListMachine.Count == 0 ||
          (ListMachine.Count == 1 && ListMachine[0] == null)) {
        text = "All machines";
      }
      else {
        int nbMachines = ListMachine.Count;
        if (nbMachines > 1) {
          text = "Machines: ";
        }
        else {
          text = "Machine: ";
        }

        if (nbMachines == 0) {
          text += "-";
        }

        IList<string> listName = new List<string>();
        foreach (IMachine machine in ListMachine) {
          listName.Add(machine.Display);
        }

        text += String.Join(", ", listName.ToArray());
      }
      
      return text;
    }
    
    string FormatUsers()
    {
      string text = "";
      
      int nbUsers = ListEmails.Count;
      if (nbUsers > 1) {
        text = "Users: ";
      }
      else {
        text = "User: ";
      }

      if (nbUsers == 0) {
        text += "-";
      }

      IList<string> listUser = new List<string>();
      foreach (EmailWithName email in ListEmails) {
        listUser.Add(email.toShortString());
      }
      text += String.Join(", ", listUser.ToArray());
      
      return text;
    }
    
    private string FormatPeriods()
    {
      string text = "";
      
      if (m_daysInWeek <= 0) {
        text = "Never";
      }

      if (m_daysInWeek >= 127) {
        text = "Every day";
      }
      else {
        text = "Only ";
        IList<string> days = new List<string>();
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Mon) != 0) {
          days.Add("monday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Tue) != 0) {
          days.Add("tuesday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Wed) != 0) {
          days.Add("wednesday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Thu) != 0) {
          days.Add("thursday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Fri) != 0) {
          days.Add("friday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Sat) != 0) {
          days.Add("saturday");
        }
        if ((m_daysInWeek & (int)DayInWeekPicker.Days.Sun) != 0) {
          days.Add("sunday");
        }
        text += String.Join(", ", days.ToArray());
      }
      
      return text;
    }
    
    private string FormatBeginEnd()
    {
      string text = "";
      
      DateTime? dateStart = DateStart;
      if (DateStart <= DateTime.Now) {
        dateStart = null;
      }

      if (DateEnd != null && DateEnd < DateTime.Now) {
        text = "Expired on " + String.Format("{0:d}", DateEnd);
      } else if (dateStart == null) {
        if (DateEnd == null) {
          // No date start and no date end
          text = "No expiration date";
        } else {
          // Only a date end
          text = "Expiration date: " + String.Format("{0:d}", DateEnd);
        }
      } else {
        if (DateEnd == null) {
          // Only a date start
          text = "From " + String.Format("{0:d}", dateStart);
        } else {
          // Date start and date end
          text = "From " + String.Format("{0:d}", dateStart) +
            " to " + String.Format("{0:d}", DateEnd);
        }
      }
      
      if (!TimePeriod.IsFullDay()) {
        text += ", working period: " +
          new DateTime(TimePeriod.Begin.Ticks).ToString("HH:mm") + " - " +
          new DateTime(TimePeriod.End.Ticks).ToString("HH:mm");
      }
      
      return text;
    }
    
    /// <summary>
    /// Return true if the alarm contains the machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool ContainsMachine(IMachine machine)
    {
      return ListMachine == null || ListMachine.Count == 0 ||
        (ListMachine.Count == 1 && ListMachine[0] == null) ||
        ListMachine.Contains(machine);
    }
    
    /// <summary>
    /// Return true if the alarm contains one of the machines present in the machine list
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    public bool ContainsMachine(IList<IMachine> machines)
    {
      foreach (IMachine machine in machines) {
        if (ContainsMachine(machine)) {
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Return true if the alarm contains the email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool ContainsEmail(EmailWithName email)
    {
      return ListEmails.Contains(email);
    }
    
    /// <summary>
    /// Return true if the alarm contains one of the emails present in the email list
    /// </summary>
    /// <param name="emails"></param>
    /// <returns></returns>
    public bool ContainsEmail(IList<EmailWithName> emails)
    {
      foreach (EmailWithName email in emails) {
        if (ContainsEmail(email)) {
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Return true if the alarm is related to an emailConfig
    /// </summary>
    /// <param name="emailConfig"></param>
    /// <returns></returns>
    public bool Corresponds(IEmailConfig emailConfig)
    {
      return ListId.Contains(emailConfig.Id);
    }
    #endregion // Methods
  }
}
