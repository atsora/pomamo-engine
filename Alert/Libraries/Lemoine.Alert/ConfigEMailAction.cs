// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Xml;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert
{
  /// <summary>
  /// E-mail action with:
  /// <item>an XSLT to build the body and the subject</item>
  /// <item>dynamic recipients from the table emailconfig</item>
  /// </summary>
  [Serializable]
  public class ConfigEMailAction : GenericEMailAction
  {
    XslDefinition m_dateTime;
    XslDefinition m_name;
    XslDefinition m_dataType;
    XslDefinition m_freeFilter;
    XslDefinition m_valueFilter;
    XslDefinition m_levelPriority;
    XslDefinition m_machineName;
    XslDefinition m_subject;
    XslDefinition m_body;

    [NonSerialized]
    XmlElement m_dataForEmailConfig;
    IList<IEmailConfig> m_matchedEmailConfigs;

    IEnumerable<IConfigEMailExtension> m_extensions;

    static readonly ILog log = LogManager.GetLogger (typeof (ConfigEMailAction).FullName);

    /// <summary>
    /// Date/time of the event
    /// 
    /// In UTC
    /// 
    /// If not defined, the current process time is considered
    /// </summary>
    [XmlElement]
    public XslDefinition UtcDateTime
    {
      get { return m_dateTime; }
      set { m_dateTime = value; }
    }

    /// <summary>
    /// Name of the config
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Date type
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition DataType
    {
      get { return m_dataType; }
      set { m_dataType = value; }
    }

    /// <summary>
    /// Free filter to use in the config
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition FreeFilter
    {
      get { return m_freeFilter; }
      set { m_freeFilter = value; }
    }

    /// <summary>
    /// Value filter to use in the config with an IConfigEMailExtension
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition ValueFilter
    {
      get { return m_valueFilter; }
      set { m_valueFilter = value; }
    }

    /// <summary>
    /// (Event) level priority
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition LevelPriority
    {
      get { return m_levelPriority; }
      set { m_levelPriority = value; }
    }

    /// <summary>
    /// Machine name
    /// 
    /// null: not set
    /// </summary>
    [XmlElement]
    public XslDefinition MachineName
    {
      get { return m_machineName; }
      set { m_machineName = value; }
    }

    /// <summary>
    /// Subject of the E-Mail
    /// </summary>
    [XmlElement]
    public XslDefinition Subject
    {
      get { return m_subject; }
      set { m_subject = value; }
    }

    /// <summary>
    /// Body of the E-Mail
    /// </summary>
    [XmlElement]
    public XslDefinition Body
    {
      get { return m_body; }
      set { m_body = value; }
    }

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfigEMailAction ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Get the extensions and load them if needed
    /// </summary>
    /// <returns></returns>
    IEnumerable<IConfigEMailExtension> GetExtensions ()
    {
      LoadExtensions ();
      return m_extensions;
    }

    /// <summary>
    /// Load the extensions
    /// </summary>
    void LoadExtensions ()
    {
      if (null == m_extensions) { // Initialization
        m_extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IConfigEMailExtension> ()); 
      }
    }

    /// <summary>
    /// Process a XslDefinition
    /// 
    /// If xslDefinition is null, null is returned
    /// </summary>
    /// <param name="xslDefinition"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    string GetFromXslDefinition (XslDefinition xslDefinition, XmlElement data)
    {
      if (null == xslDefinition) {
        return null;
      }

      XPathDocument xpd = new XPathDocument (new StringReader (data.OuterXml));
      StringWriter stringWriter = new StringWriter ();
      xslDefinition.Xslt.Transform (xpd.CreateNavigator (), null, stringWriter);
      return stringWriter.ToString ();
    }

    /// <summary>
    /// Implements <see cref="GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetSubject (XmlElement data)
    {
      return GetFromXslDefinition (m_subject, data);
    }

    /// <summary>
    /// Implements <see cref="GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetBody (XmlElement data)
    {
      return GetFromXslDefinition (m_body, data);
    }

    IList<IEmailConfig> GetMatchedEmailConfigs (XmlElement data)
    {
      if (data == m_dataForEmailConfig) { // Test the reference pointer here !
        return m_matchedEmailConfigs;
      }

      // Else the data is not in cache
      // Retrieve all the emailConfigs that may match data
      // - Process the required XslDefinitions
      var dateTime = GetDateTime (data);
      string name = GetFromXslDefinition (m_name, data);
      string dataType = GetFromXslDefinition (m_dataType, data);
      string freeFilter = GetFromXslDefinition (m_freeFilter, data);
      string valueFilter = GetFromXslDefinition (m_valueFilter, data);
      string levelPriorityString = GetFromXslDefinition (m_levelPriority, data);
      int levelPriority = 0;
      log.Debug($"GetMatchedEmailConfigs: +++++ name={name}, datatype={dataType}, freeFilter={freeFilter}, valueFilter={valueFilter}, levelPriority={levelPriorityString}");


      if (null != levelPriorityString) {
        if (!int.TryParse (levelPriorityString, out levelPriority)) {
          log.Error ($"GetMatchedEmailConfigs: impossible to parse priority {levelPriorityString}");
        }
      }
      string machineName = GetFromXslDefinition (m_machineName, data);
      // - Get all the emailConfigs, a list of event levels and the machine
      IList<IEmailConfig> activeEmailConfigs;
      IMachine machine = null;
      IList<IEventLevel> eventLevels = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        activeEmailConfigs = ModelDAOHelper.DAOFactory.EmailConfigDAO.FindActive ();

        if (!string.IsNullOrEmpty (machineName)) {
          machine = ModelDAOHelper.DAOFactory.MachineDAO.FindByName (machineName);
        }
        if (levelPriority > 0) {
          eventLevels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindByPriority (levelPriority);
        }
        // - Keep only the emailConfigs that match (inside the session because of Machine and MachineFilter)
        m_matchedEmailConfigs = new List<IEmailConfig> ();
        foreach (IEmailConfig emailConfig in activeEmailConfigs) {
          Debug.Assert (true == emailConfig.Active);
          log.Debug ($"GetMatchedEmailConfigs: ++++emailConfig name={emailConfig.Name} active={emailConfig.Active}, freeFilter={emailConfig.FreeFilter}, type={emailConfig.DataType}");

          if ((0 != dateTime.Ticks) && (false == CheckActivePeriod (emailConfig, dateTime))) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: event time {dateTime} UTC does not match config {emailConfig}");
            }
            continue;
          }
          if (!string.IsNullOrEmpty (name) && !object.Equals (name, emailConfig.Name)) {
            // Note: for the name, the filter is driven by the configuration file, not the database record
            // If the name is set in the configuration file, the database record must include this name
            // This is not the same for the other fields where the filter is driven by the database record
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: name does not match: {name} vs {emailConfig.Name}");
            }
            continue;
          }
          if (!string.IsNullOrEmpty (emailConfig.DataType) && !object.Equals (dataType, emailConfig.DataType)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: dataType does not match: {dataType} vs {emailConfig.DataType}");
            }
            continue;
          }
          if ((null != freeFilter) && !string.IsNullOrEmpty (emailConfig.FreeFilter) && !object.Equals (freeFilter, emailConfig.FreeFilter)) {
            // null != freeFilter => consider an exact free filter match
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: freeFilter does not match: {freeFilter} vs {emailConfig.FreeFilter}");
            }
            continue;
          }

          if ((null == freeFilter) && !string.IsNullOrEmpty (emailConfig.FreeFilter)) {
            // null == freeFilter => use CheckExtensionMatch
            if (!CheckExtensionMatch (dataType, emailConfig.FreeFilter, valueFilter)) {
              continue;
            }
          }
          if (levelPriority > emailConfig.MaxLevelPriority) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: priority does not match with MaxLevelPriority: {levelPriority} vs Max={emailConfig.MaxLevelPriority}");
            }
            continue;
          }
          if (eventLevels != null && eventLevels.Count > 0) {
            if (emailConfig.EventLevel != null && eventLevels.Count > 0 && !eventLevels.Contains (emailConfig.EventLevel)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetMatchedEmailConfigs: event level does not match: {emailConfig.EventLevel}");
              }
              continue;
            }
          }

          if ((null != emailConfig.Machine) && !object.Equals (machine, emailConfig.Machine)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: machine does not match: {machine} vs {emailConfig.Machine}");
            }
            continue;
          }
          if ((null != emailConfig.MachineFilter) &&
              ((null == machine) || !emailConfig.MachineFilter.IsMatch (machine))) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetMatchedEmailConfigs: machine {machine} does not match machine filter {emailConfig.MachineFilter}");
            }
            continue;
          }
          m_matchedEmailConfigs.Add (emailConfig);
        }
      }
      m_dataForEmailConfig = data;
      return m_matchedEmailConfigs;
    }

    bool CheckExtensionMatch (string dataType, string configInput, string v)
    {
      log.Debug ($"CheckExtensionMatch: ++++ dataType={dataType}, configInput={configInput}, v={v}");
      var extensions = GetExtensions ();
      foreach (var extension in GetExtensions ()
        .Where (e => string.Equals (e.DataType, dataType, StringComparison.InvariantCultureIgnoreCase))) {
        var match = extension.Match (configInput, v);
        log.Debug ($"CheckExtensionMatch: ++++ match={match}");
        if (match) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if a configuration is active at the specified UTC date/time
    /// </summary>
    /// <param name="config"></param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    bool CheckActivePeriod (IEmailConfig config, DateTime dateTime)
    {
      Debug.Assert (DateTimeKind.Utc == dateTime.Kind);
      Debug.Assert (0 < dateTime.Ticks);

      // - WeekDay
      DateTime day = dateTime.ToLocalTime ().Date;
      if (!config.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckActivePeriod: for dateTime={dateTime}, dayOfWeek={day.DayOfWeek} does not match config {config}");
        }
        return false;
      }

      // - TimePeriod
      if (false == config.TimePeriod.IsFullDay ()) {
        DateTime timePeriodBegin = day.Add (config.TimePeriod.Begin).ToUniversalTime ();
        if (dateTime < timePeriodBegin) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckActivityPeriod: for dateTime={dateTime} localDateTime={dateTime.ToLocalTime ()} time is before {config.TimePeriod.Begin} => config does not match");
          }
          return false;
        }
        DateTime timePeriodEnd = day.Add (config.TimePeriod.EndOffset).ToUniversalTime ();
        if (timePeriodEnd < dateTime) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckActivityPeriod: for dateTime={dateTime} localDateTime={dateTime.ToLocalTime ()} time is after {config.TimePeriod.End} => config does not match");
          }
          return false;
        }
      }

      // - BeginDateTime
      if (config.BeginDateTime.HasValue) {
        if (dateTime < config.BeginDateTime.Value) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckActivityPeriod: dateTime {dateTime} is before config begin datetime {config.BeginDateTime.Value}");
          }
          return false;
        }
      }

      // - EndDateTime
      if (config.EndDateTime.HasValue) {
        if (config.EndDateTime.Value <= dateTime) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckActivityPeriod: dateTime {dateTime} is after config end datetime {config.EndDateTime.Value}");
          }
          if (true == config.AutoPurge) {
            log.Info ($"CheckActivityPeriod: purge {config} because its validity is before event date/time {dateTime}");
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) // Same session as the caller
            {
              ModelDAOHelper.DAOFactory.EmailConfigDAO.UpgradeLock (config);
              using (IDAOTransaction transaction = session.BeginTransaction ()) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;
                ModelDAOHelper.DAOFactory.EmailConfigDAO.MakeTransient (config);
                transaction.Commit ();
              }
            }
          }
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Override <see cref="Lemoine.Alert.GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetTo (XmlElement data)
    {
      IList<IEmailConfig> emailConfigs = GetMatchedEmailConfigs (data);
      List<string> tos = new List<string> ();

      foreach (IEmailConfig emailConfig in emailConfigs) {
        if (!string.IsNullOrEmpty (emailConfig.To)) {
          tos.Add (emailConfig.To);
        }
      }

      if (0 == tos.Count) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTo: in the {emailConfigs.Count} configs that match, none has a recipient");
        }
        return "";
      }

      return string.Join (",", tos.ToArray ());
    }

    /// <summary>
    /// Override <see cref="Lemoine.Alert.GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetCc (XmlElement data)
    {
      IList<IEmailConfig> emailConfigs = GetMatchedEmailConfigs (data);
      List<string> ccs = new List<string> ();

      foreach (IEmailConfig emailConfig in emailConfigs) {
        if (null != emailConfig.Cc) {
          ccs.Add (emailConfig.Cc);
        }
      }

      return string.Join (",", ccs.ToArray ());
    }

    /// <summary>
    /// Override <see cref="Lemoine.Alert.GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetBcc (XmlElement data)
    {
      IList<IEmailConfig> emailConfigs = GetMatchedEmailConfigs (data);
      List<string> bccs = new List<string> ();

      foreach (IEmailConfig emailConfig in emailConfigs) {
        if (null != emailConfig.Bcc) {
          bccs.Add (emailConfig.Bcc);
        }
      }

      return string.Join (",", bccs.ToArray ());
    }

    /// <summary>
    /// Override <see cref="Lemoine.Alert.GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override DateTime GetDateTime (XmlElement data)
    {
      string dateTimeString = GetFromXslDefinition (m_dateTime, data);
      DateTime dateTime = DateTime.UtcNow;
      if (null == dateTimeString) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetMatchedEmailConfigs: no UtcDateTime XslDefinition => fallback to now {dateTime}");
        }
      }
      else if (false == DateTime.TryParse (dateTimeString, out dateTime)) { // && (null != dateTimeString)
        log.Error ($"GetMatchedEmailConfigs: error while parsing date/time {dateTimeString}");
      }
      else {
        dateTime = DateTime.SpecifyKind (dateTime, DateTimeKind.Utc);
      }

      return dateTime;
    }
  }
}
