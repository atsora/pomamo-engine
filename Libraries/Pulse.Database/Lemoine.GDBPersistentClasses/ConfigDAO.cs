// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate.Criterion;
using Lemoine.Info;
using Lemoine.Net;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IConfigDAO">IConfigDAO</see>
  /// </summary>
  public class ConfigDAO
    : VersionableNHibernateDAO<Config, IConfig, int>
    , IConfigDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal bool InsertDefaultValues (IList<long> migrations)
    {
      if (!migrations.Contains (804)) {
        return false;
      }

      IConfig config;
      // - Global
      string firstDayOfWeekKey = ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.FirstDayOfWeek);
      if (null == GetConfig (firstDayOfWeekKey)) {
        config = new Config (firstDayOfWeekKey,
                             "Global config: first day of the week",
                             DayOfWeek.Monday);
        InsertDefaultValue (config);
      }
      string calendarWeekRuleKey = ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.CalendarWeekRule);
      if (null == GetConfig (calendarWeekRuleKey)) {
        config = new Config (calendarWeekRuleKey,
                             "Global config: calendar week rule (Iso, FirstDay, FirstFourDayWeek, FirstFullWeek)",
                             "Iso");
        InsertDefaultValue (config);
      }

      // - Analysis
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ActivityAnalysisFrequency),
                           "Analysis config: frequency at which the activity analysis is run",
                           TimeSpan.FromSeconds (3),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationMargin),
                           "Analysis config: margin for auto-operations (applied at the end of an activity)",
                           TimeSpan.FromSeconds (40),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CleanDeletedModifications),
                           "Analysis config: clean deleted modifications option",
                           true,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CreateComponentOperationLink),
                           "Analysis config: should a new Component or Part automatically associated to an Operation ?",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CreateWorkOrderComponentLink),
                           "Analysis config: should a new Work Order automatically associated to a Part or Component ?",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.Every),
                           "Analysis config: how often is run the modification analysis. This is a minimum value. There is no sleep time if the process takes more time than this value.",
                           TimeSpan.FromSeconds (1),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.LinkOperationMaximumTime),
                           "Analysis config: time after which the link operation is not applied any more",
                           TimeSpan.FromDays (1),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxThreadsInPool),
                           "Analysis config: maximum number of threads to use in the thread pool of the activity analysis",
                           1024,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxRunningMachineThreads),
                           "Analysis config: maximum number of monitored machine analysis threads that can be run simulaneously",
                           16,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ObsoleteTime),
                           "Analysis config: time after which a record is considered obsolete in case it is still pending",
                           TimeSpan.FromDays (2),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationSame),
                           "Analysis config: time after which no auto operation should be set between two same operations",
                           TimeSpan.FromDays (1),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoComponentSame),
                           "Analysis config: time after which no auto component should be set between two operations that refer to the same component",
                           TimeSpan.FromDays (1),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoWorkOrderSame),
                           "Analysis config: time after which no auto operation should be set between two operations that refer to the same work order",
                           TimeSpan.FromDays (1),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationCycleAssociationMargin),
                           "Analysis config: margin used to fully associate a cycle to an operation (applied at the begin of the cycle)",
                           TimeSpan.FromSeconds (20),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption),
                           "Analysis config: option to split the operation slots - 1: by day - 2/3: by global shift - 4/5: by machine shift",
                           0,
                           active: false);
      {
        IConfig splitByShift = GetConfig ("Analysis.SplitOperationSlotByShift");
        if ((null != splitByShift) && ((bool)splitByShift.Value)) {
          config.Value = 3;
        }
        else {
          IConfig splitByDay = GetConfig ("Analysis.SplitOperationSlotByDay");
          if ((null != splitByDay) && ((bool)splitByDay.Value)) {
            config.Value = 1;
          }
        }
      }
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.SplitCycleSummaryByShift),
                           "Analysis config: option to split the cycle summaries by shift (if SplitOperationSlotByShift is active)",
                           true,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ModificationTimeout),
                           "Analysis config: timeout for the modification analysis",
                           TimeSpan.FromSeconds (40),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ModificationStepTimeout),
                           "Analysis config: timeout for one step of the modification analysis",
                           TimeSpan.FromSeconds (8),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.SequenceGapLimit),
                           "Analysis config: maximum gap to extend two identical sequences",
                           TimeSpan.FromHours (24),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd),
                           "Analysis config: extend an already full cycle in case a new cycle end is detected",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.CurrentTimeFrameDuration),
                           "Analysis config: current time frame duration that must be considered for current slots",
                           TimeSpan.FromMinutes (1),
                           active: false);
      InsertDefaultValue (config);
      // - MinTemplateProcessDateTime
      string minTemplateProcessDateTimeKey =
        ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MinTemplateProcessDateTime);
      if (null == GetConfig (minTemplateProcessDateTimeKey)) {
        config = new Config (minTemplateProcessDateTimeKey,
                             "Analysis config: min template process date/time",
                             new DateTime (2020, 01, 01, 00, 00, 00, DateTimeKind.Utc),
                             active: false);
        IFact oldestFact = new FactDAO ()
          .FindOldest ();
        if (null != oldestFact) {
          config.Value = (DateTime)oldestFact.Begin;
          config.Active = true;
        }
        InsertDefaultValue (config);
      }
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxDaySlotProcess),
                           "Analysis config: duration from now for which the day slot are processed",
                           TimeSpan.FromDays (365),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MaxShiftSlotProcess),
                           "Analysis config: duration from now for which the shift slot are processed",
                           TimeSpan.FromDays (365),
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendOperationPropagation),
                           "Analysis config: extend operation propagation options - 1: work order - 2: component - 4: line - 8: task",
                           (int)PropagationOption.All,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.AutoOperationPropagation),
                           "Analysis config: auto-operation propagation options - 1: work order - 2: component - 4: line - 8: task",
                           (int)PropagationOption.All,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.LineManagement),
                           "Analysis config: line management activation",
                           false,
                           active: false);
      {
        IList<ILine> lines = new LineDAO ().FindAll ();
        if (0 < lines.Count) {
          config.Value = true;
          config.Active = true;
        }
      }
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.TaskManagement),
                           "Analysis config: task management activation",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotRunTime),
                           "Analysis config: track the run time in the operation slots",
                           true,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotProductionDuration),
                           "Analysis config: track the production duration in the operation slots",
                           true,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MemoryPercentageStopNewThreads),
                           "Analysis config: % of the memory after which no new thread is created",
                           50,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.MemoryPercentageExit),
                           "Analysis config: % of the memory after which the service is stopped",
                           68,
                           active: false);
      InsertDefaultValue (config);

      // - cnc
      config = new Config (ConfigKeys.GetCncConfigKey (CncConfigKey.CncDataUseProcess),
                           "Cnc config: use by default processes instead of threads in Lem_CncDataService",
                           false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetCncConfigKey (CncConfigKey.ToolLifeDataObsoleteTime),
                           "Cnc config: time after which the data related to the tool life management are considered as expired",
                           TimeSpan.FromMinutes (1));
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetCncConfigKey (CncConfigKey.StampFromProgramNameBlock),
                           "Cnc config: try to determine the stamp from the program name and the block number",
                           false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetCncConfigKey (CncConfigKey.SpindleLoadPeakLimit),
                           "Cnc config: spindle load peak limit",
                           60);
      InsertDefaultValue (config);

      // - datastructure
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath),
                           "DataStructure config: allow only a single path per operation",
                           true,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob),
                           "Work Order + Project = Job",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart),
                           "Project + Component = Part",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.IntermediateWorkPieceOperationIsSimpleOperation),
                           "Intermediate Work Piece + Operation = Simple Operation",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent),
                           "Project/Component/Part => 1 Work Order",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation),
                           "Operation => 1 Project/Component/Part",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder),
                           "Work Order => 1 Project/Part",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly),
                           "Project/Component/Part <= Operation only",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly),
                           "Work Order <= Project/Component/Part only",
                           false,
                           active: false);
      InsertDefaultValue (config);
      config = new Config (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromLine),
                           "Line => 1 Part/Component",
                           true,
                           active: false);
      InsertDefaultValue (config);

      // - Gui.Dbm
      config = new Config (ConfigKeys.GetDbmConfigKey (DbmConfigKey.MOSWithReason),
                           "Input the Machine Operation State with the Reason",
                           false);
      InsertDefaultValue (config);

      // operation explorer
      config = new Config (ConfigKeys.GetOperationExplorerConfigKey (OperationExplorerConfigKey.PartAtTheTop),
                           "Operation Explorer: part appears at the top of the operation tree",
                           false,
                           active: false);
      InsertDefaultValue (config);

      // web service
      config = new Config (ConfigKeys.GetWebServiceConfigKey (WebServiceConfigKey.GapLastOperationSlot),
                           "Web service config: maximum allowed gap to consider an operation slot as containing valid operation information",
                           TimeSpan.FromMinutes (10));
      InsertDefaultValue (config);

      // net.mail
      config = new Config (MailSettingsConfigKey.Host.GetConfigKey (),
                           "Smtp server host name",
                           Lemoine.ModelDAO.Info.NotSetConfigValue.String, // "not set": specific string to tell a config is not set in database
                           active: false);
      InsertDefaultValue (config);
      config = new Config (MailSettingsConfigKey.EnableSsl.GetConfigKey (),
                           "Smtp: use SSL to encrypt the connection",
                           false);
      InsertDefaultValue (config);
      config = new Config (MailSettingsConfigKey.Port.GetConfigKey (),
                           "Smtp server port",
                           25);
      InsertDefaultValue (config);
      config = new Config (MailSettingsConfigKey.UserName.GetConfigKey (),
                           "Smtp connection user name",
                           ""); // Empty: not set, do not use any credentials
      InsertDefaultValue (config);
      config = new Config (MailSettingsConfigKey.Password.GetConfigKey (),
                           "Smtp connection password",
                           "");
      InsertDefaultValue (config);
      config = new Config (MailSettingsConfigKey.From.GetConfigKey (),
                           "Default mail sender",
                           Lemoine.ModelDAO.Info.NotSetConfigValue.String, // "not set": specific string to tell a config is not set in database
                           active: false);
      InsertDefaultValue (config);

      // i18n
      Lemoine.I18N.LocaleSettings.SetLanguageFromConfigSetIfNotManuallySet ();
      config = new Config ("i18n.locale.default",
                           "Default locale code",
                           System.Globalization.CultureInfo.CurrentCulture.Name);
      InsertDefaultValue (config);

      // Stamping
      config = new Config ("Stamping.Regex.CadName",
                           "Stamping config: default CAD name regex",
                           "(?<ProjectName>[0-9]+)_(?<ComponentName>.+)",
                           active: false);
      InsertDefaultValue (config);
      config = new Config ("Stamping.Regex.FileName",
                           "Stamping config: default file name regex",
                           "(?<ProjectName>[0-9]+)_(?<ComponentName>.+)",
                           active: false);
      InsertDefaultValue (config);
      config = new Config ("Stamping.Time.Factor",
                           "Stamping config: default time factor for sequence duration",
                           1.0,
                           active: false);
      InsertDefaultValue (config);
      config = new Config ("Stamping.Time.MilestoneFrequency",
                           "Stamping config: default milestone frequency",
                           TimeSpan.FromMinutes (2),
                           active: false);
      InsertDefaultValue (config);
      config = new Config ("Stamping.Axis.DefaultMaxVelocity",
                           "Stamping config: default axis max velocity in mm/s for the X, Y and Z axis",
                           1200000.0, // 20 000 mm/min
                           active: false);
      InsertDefaultValue (config);
      config = new Config ("Stamping.Axis.DefaultUnit",
                           "Stamping config: default axis unit, 1: Mm, 2: In",
                           2,
                           active: false);
      InsertDefaultValue (config);

      return true;
    }

    private void InsertDefaultValue (IConfig config)
    {
      if (null == GetConfig (config.Key)) { // the config does not exist => create it
        log.InfoFormat ("InsertDefaultValue: " +
                        "add key={0} config={1}",
                        config.Key, config);
        MakePersistent (config);
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Get the config for the specified key
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetConfig (string key)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Config> ()
        .Add (Restrictions.Eq ("Key", key))
        .SetCacheable (true)
        .UniqueResult<IConfig> ();
    }

    /// <summary>
    /// Get the global config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetCalendarConfig (CalendarConfigKey key)
    {
      return GetConfig (ConfigKeys.GetCalendarConfigKey (key));
    }

    /// <summary>
    /// Get the calendar config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetCalendarConfigValue (CalendarConfigKey key)
    {
      IConfig config = GetCalendarConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetCalendarConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("GlobalConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the analysis config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetAnalysisConfig (AnalysisConfigKey key)
    {
      return GetConfig (ConfigKeys.GetAnalysisConfigKey (key));
    }

    /// <summary>
    /// Get the analysis config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetAnalysisConfigValue (AnalysisConfigKey key)
    {
      IConfig config = GetAnalysisConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetAnalysisConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("AnalysisConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the CNC config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetCncConfig (CncConfigKey key)
    {
      return GetConfig (ConfigKeys.GetCncConfigKey (key));
    }

    /// <summary>
    /// Get the CNC config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetCncConfigValue (CncConfigKey key)
    {
      IConfig config = GetCncConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetCncConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("CncConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the OperationExplorer config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetOperationExplorerConfig (OperationExplorerConfigKey key)
    {
      return GetConfig (ConfigKeys.GetOperationExplorerConfigKey (key));
    }

    /// <summary>
    /// Get the OperationExplorer config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetOperationExplorerConfigValue (OperationExplorerConfigKey key)
    {
      IConfig config = GetOperationExplorerConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetOperationExplorerConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("OperationExplorerConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the Dbm config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetDbmConfig (DbmConfigKey key)
    {
      return GetConfig (ConfigKeys.GetDbmConfigKey (key));
    }

    /// <summary>
    /// Get the Dbm config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetDbmConfigValue (DbmConfigKey key)
    {
      IConfig config = GetDbmConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetDbmConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("DbmConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the datastructure config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetDataStructureConfig (DataStructureConfigKey key)
    {
      return GetConfig (ConfigKeys.GetDataStructureConfigKey (key));
    }

    /// <summary>
    /// Get the datastructure config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetDataStructureConfigValue (DataStructureConfigKey key)
    {
      IConfig config = GetDataStructureConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetDataStructureConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("DataStructureConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Get the webservice config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig GetWebServiceConfig (WebServiceConfigKey key)
    {
      return GetConfig (ConfigKeys.GetWebServiceConfigKey (key));
    }

    /// <summary>
    /// Get the webservice config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetWebServiceConfigValue (WebServiceConfigKey key)
    {
      IConfig config = GetWebServiceConfig (key);
      if (null == config) {
        log.ErrorFormat ("GetWebServiceConfigValue: " +
                         "got a null config for {0}, this should not happen",
                         key);
        throw new Exception ("WebServiceConfig unknown");
      }
      return config.Value;
    }

    /// <summary>
    /// Force a configuration, without first checking the previous configuration value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="activated"></param>
    public void SetConfig (string key, object v, bool activated)
    {
      IConfig config = GetConfig (key);
      if (null == config) { // The configuration does not exist => create a new one
        log.DebugFormat ("SetConfig: " +
                         "create a new configuration for key={0}",
                         key);
        config = ModelDAOHelper.ModelFactory.CreateConfig (key);
        config.Value = v;
        config.Active = activated;
        MakePersistent (config);
      }
      else { // The configuration already exists => update it
        log.DebugFormat ("SetConfig: " +
                         "the configuration already exists for key={0}, " +
                         "=> update value from {1} to {2}",
                         key,
                         config.Value, v);
        config.Value = v;
        config.Active = activated;
        MakePersistent (config);
      }
    }

    /// <summary>
    /// Find the config where the key is like a given filter
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IList<IConfig> FindLike (string filter)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Config> ()
        .Add (Restrictions.Like ("Key", filter))
        .SetCacheable (true)
        .List<IConfig> ();
    }
  }
}
