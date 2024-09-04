// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Class retrieving all emails and alarms edited with this software
  /// from the database
  /// </summary>
  public class AlarmManager
  {
    #region Members
    readonly IList<Alarm> m_listAlarms;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AlarmManager).FullName);
    internal static readonly string SOFTWARE_NAME = "LemSettings - Email alert configurator";
    internal static readonly string OLD_SOFTWARE_NAME = "AlertConfigGUI";
    
    #region Getters / Setters
    /// <summary>
    /// List of all emails
    /// </summary>
    public List<EmailWithName> ListEmails { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public AlarmManager ()
    {
      ListEmails = new List<EmailWithName>();
      m_listAlarms = new List<Alarm>();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Reload all data from the database
    /// </summary>
    public void loadData()
    {
      LoadEmailsFromUserTable();
      LoadAlarmsAndAppendRemainingEmails();
    }
    
    void LoadEmailsFromUserTable()
    {
      ListEmails.Clear();
      
      // Emails coming from usertable
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IUser> users = ModelDAOHelper.DAOFactory.UserDAO.FindAll();
          foreach (IUser user in users) {
            if (!String.IsNullOrEmpty(user.EMail)) {
              var emailWithName = new EmailWithName(user.EMail);
              emailWithName.Name = user.Name;
              if (!ListEmails.Contains(emailWithName)) {
                ListEmails.Add(emailWithName);
              }
            }
          }
        }
      }
    }
    
    void LoadAlarmsAndAppendRemainingEmails()
    {
      m_listAlarms.Clear();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IEmailConfig> emailConfigs = ModelDAOHelper.DAOFactory.EmailConfigDAO.FindAllForConfig();
          if (emailConfigs == null) {
            return;
          }

          foreach (IEmailConfig emailConfig in emailConfigs) {
            if (emailConfig.FreeFilter == SOFTWARE_NAME || emailConfig.FreeFilter == OLD_SOFTWARE_NAME ||
                emailConfig.Editor == SOFTWARE_NAME)
            {
              // Alarm name
              string alarmName = emailConfig.Name;
              int lastUnderScoreIndex = alarmName.LastIndexOf("_");
              if (lastUnderScoreIndex != -1) {
                alarmName = alarmName.Substring(0, lastUnderScoreIndex);
              }
              
              // Creation of an alarm with attibutes
              var alarm = new Alarm(alarmName);
              alarm.AddId(emailConfig.Id);
              if (emailConfig.Machine != null) {
                alarm.AddMachine(emailConfig.Machine);
              }

              alarm.AlarmActivated = emailConfig.Active;
              if (emailConfig.BeginDateTime != null) {
                alarm.DateStart = emailConfig.BeginDateTime.Value.ToLocalTime();
              }
              if (emailConfig.EndDateTime != null) {
                alarm.DateEnd = emailConfig.EndDateTime.Value.ToLocalTime();
              }
              alarm.TimePeriod = emailConfig.TimePeriod;
              alarm.DataType = emailConfig.DataType;
              alarm.EventLevel = emailConfig.EventLevel;
              alarm.Filter = emailConfig.FreeFilter;
              alarm.DaysInWeek = (int)emailConfig.WeekDays;
              
              // Emails
              if (!string.IsNullOrEmpty(emailConfig.To)) {
                string[] emails = emailConfig.To.Split(',');
                foreach (string email in emails) {
                  int indexEmail = ListEmails.IndexOf(email);
                  if (indexEmail == -1) {
                    ListEmails.Add(email);
                    indexEmail = ListEmails.Count - 1;
                  }
                  alarm.AddEmail(ListEmails[indexEmail]);
                }
              }
              
              // Add or merge the alarm if one alarm has already the same name
              // (machines are added to an existing alarm)
              int indexOfAlarm = m_listAlarms.IndexOf(alarm);
              if (indexOfAlarm != -1) {
                m_listAlarms[indexOfAlarm].Merge(alarm);
              } else {
                m_listAlarms.Add(alarm);
              }
            }
          }
        }
      }
      
      ListEmails.Sort();
    }
    
    /// <summary>
    /// Return true if the machine is targeted by an alarm
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public bool IsMachineLinked(IMachine machine, string dataType)
    {
      foreach (Alarm alarm in m_listAlarms) {
        if ((dataType == "" || alarm.DataType == dataType) &&
            alarm.ContainsMachine(machine)) {
          return true;
        }
      }
      
      return false;
    }
    
    /// <summary>
    /// Return true if the email is used by an alarm
    /// </summary>
    /// <param name="email"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public bool IsEmailLinked(EmailWithName email, string dataType)
    {
      foreach (Alarm alarm in m_listAlarms) {
        if ((dataType == "" || alarm.DataType == dataType) &&
            alarm.ContainsEmail(email)) {
          return true;
        }
      }
      
      return false;
    }
    
    /// <summary>
    /// Retrieve the alarm list filtered
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="level"></param>
    /// <param name="filter"></param>
    /// <param name="listMachines"></param>
    /// <param name="listEMails"></param>
    /// <returns></returns>
    public List<Alarm> getListAlarms(string dataType, IEventLevel level, string filter, IList<IMachine> listMachines, IList<EmailWithName> listEMails)
    {
      var listAlarms = new List<Alarm>();
      
      foreach (Alarm alarm in m_listAlarms) {
        if ((dataType == "" || alarm.DataType == dataType) &&
            (listMachines.Count == 0 || alarm.ContainsMachine(listMachines)) &&
            (listEMails.Count == 0 || alarm.ContainsEmail(listEMails)) &&
            (level == null || object.Equals(alarm.EventLevel, level)) &&
            (filter == "") || object.Equals (alarm.Filter, filter)
           ) {
          listAlarms.Add(alarm);
        }
      }
      
      return listAlarms;
    }
    
    /// <summary>
    /// Remove an alarm from the database
    /// </summary>
    /// <param name="alarm"></param>
    static public void RemoveAlarm(Alarm alarm)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IList<IEmailConfig> emailConfigs = ModelDAOHelper.DAOFactory.EmailConfigDAO.FindAll();
          
          foreach (IEmailConfig emailConfig in emailConfigs) {
            if (alarm.Corresponds(emailConfig)) {
              ModelDAOHelper.DAOFactory.EmailConfigDAO.MakeTransient(emailConfig);
            }
          }
          
          transaction.Commit();
        }
      }
    }
    
    /// <summary>
    /// Save the alarm in the database
    /// </summary>
    /// <param name="alarm"></param>
    static public void SaveAlarm(Alarm alarm)
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.EventLevelDAO.Lock (alarm.EventLevel);
            
            if (alarm.ListMachine == null || alarm.ListMachine.Count == 0) {
              AddEmailConfig(alarm, null);
            } else {
              foreach (IMachine machine in alarm.ListMachine) {
                AddEmailConfig (alarm, machine);
              }
            }
            
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("Save alarm: " +
                         "got exception {0}",
                         ex);
        MessageBox.Show (@"Add alarm failed with the following error:
" + ex,
                         "Add alarm error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
      }
    }
    
    static void AddEmailConfig(Alarm alarm, IMachine machine)
    {
      if (machine != null) {
        ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
      }

      // Create a new emailConfig and add basic information
      IEmailConfig emailConfig = ModelDAOHelper.ModelFactory.CreateEmailConfig();
      emailConfig.Name = alarm.AlarmName + "_" + (machine == null ? "" : machine.Id.ToString());
      emailConfig.DataType = alarm.DataType;
      emailConfig.EventLevel = alarm.EventLevel;
      emailConfig.FreeFilter = alarm.Filter;
      emailConfig.Machine = machine;
      
      // Temporal information
      emailConfig.BeginDateTime = alarm.DateStart;
      emailConfig.EndDateTime = alarm.DateEnd;
      emailConfig.TimePeriod = alarm.TimePeriod;
      emailConfig.WeekDays = (WeekDay)alarm.DaysInWeek;

      // Email
      var listEMails = new List<string>();
      foreach (EmailWithName email in alarm.ListEmails) {
        listEMails.Add(email.Email);
      }

      emailConfig.To = string.Join(",", listEMails.ToArray());

      // Other configuration
      emailConfig.AutoPurge = true;
      emailConfig.Editor = SOFTWARE_NAME;
      emailConfig.Active = alarm.AlarmActivated;
      
      // Save
      ModelDAOHelper.DAOFactory.EmailConfigDAO.MakePersistent(emailConfig);
    }
    #endregion // Methods
  }
}