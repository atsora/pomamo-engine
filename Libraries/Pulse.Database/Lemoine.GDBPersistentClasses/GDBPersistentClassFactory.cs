// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using System.Threading;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of IModelFactory
  /// </summary>
  public class GDBPersistentClassFactory : IModelFactory
  {
    static readonly string MAX_NB_CONNECTION_INIT_ATTEMPT_KEY = "GDB.MaxNbConnectionInitAttempt";
    static readonly int MAX_NB_CONNECTION_INIT_ATTEMPT_DEFAULT = 300; // ~10 minutes with 2s delay
    static readonly string CONNECTION_INIT_ATTEMPT_SLEEP_KEY = "GDB.ConnectionInitAttemptSleep";
    static readonly TimeSpan CONNECTION_INIT_ATTEMPT_SLEEPT_DEFAULT = TimeSpan.FromSeconds (2.0);

    #region Members
    readonly IPersistentClassModel m_persistentClassModel;
    readonly Lemoine.ModelDAO.IDAOFactory m_daoFactory;
    #endregion // Members

    /// <summary>
    /// Associated DAOFactory
    /// </summary>
    public Lemoine.ModelDAO.IDAOFactory DAOFactory
    {
      get { return m_daoFactory; }
    }

    /// <summary>
    /// Constructor (without any connection to the database)
    /// </summary>
    public GDBPersistentClassFactory ()
      : this (new PersistentClassModel ())
    {
    }

    /// <summary>
    /// Constructor (without any connection to the database)
    /// </summary>
    /// <param name="persistentClassModel">not null</param>
    public GDBPersistentClassFactory (IPersistentClassModel persistentClassModel)
    {
      m_persistentClassModel = persistentClassModel;
      NHibernateHelper.AddPersistentClassModel (m_persistentClassModel);
      m_daoFactory = new DAOFactory (m_persistentClassModel);
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="cancellationToken"></param>
    public static void CreateModelFactoryAndInitializeConnection (CancellationToken? cancellationToken = null)
    {
      var maxNbAttempt = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_NB_CONNECTION_INIT_ATTEMPT_KEY, MAX_NB_CONNECTION_INIT_ATTEMPT_DEFAULT);
      var sleepDuration = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CONNECTION_INIT_ATTEMPT_SLEEP_KEY, CONNECTION_INIT_ATTEMPT_SLEEPT_DEFAULT);
      CreateModelFactoryAndInitializeConnection (maxNbAttempt, sleepDuration, cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    public static void CreateModelFactoryAndInitializeConnection (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      var sleepDuration = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CONNECTION_INIT_ATTEMPT_SLEEP_KEY, CONNECTION_INIT_ATTEMPT_SLEEPT_DEFAULT);
      CreateModelFactoryAndInitializeConnection (maxNbAttempt, sleepDuration, cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="cancellationToken"></param>
    public static async System.Threading.Tasks.Task CreateModelFactoryAndInitializeConnectionAsync (CancellationToken? cancellationToken = null)
    {
      var maxNbAttempt = Lemoine.Info.ConfigSet.LoadAndGet<int> (MAX_NB_CONNECTION_INIT_ATTEMPT_KEY, MAX_NB_CONNECTION_INIT_ATTEMPT_DEFAULT);
      var sleepDuration = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CONNECTION_INIT_ATTEMPT_SLEEP_KEY, CONNECTION_INIT_ATTEMPT_SLEEPT_DEFAULT);
      await CreateModelFactoryAndInitializeConnectionAsync (maxNbAttempt, sleepDuration, cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="cancellationToken"></param>
    public static async System.Threading.Tasks.Task CreateModelFactoryAndInitializeConnectionAsync (int maxNbAttempt, CancellationToken? cancellationToken = null)
    {
      var sleepDuration = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CONNECTION_INIT_ATTEMPT_SLEEP_KEY, CONNECTION_INIT_ATTEMPT_SLEEPT_DEFAULT);
      await CreateModelFactoryAndInitializeConnectionAsync (maxNbAttempt, sleepDuration, cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="maxNbAttempt">Use 0 not to try to connect at once</param>
    /// <param name="sleepDuration"></param>
    /// <param name="cancellationToken"></param>
    public static void CreateModelFactoryAndInitializeConnection (int maxNbAttempt, TimeSpan sleepDuration, CancellationToken cancellationToken)
    {
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();

      InitializeConnection (maxNbAttempt, sleepDuration, cancellationToken);
    }

    /// <summary>
    /// Wait the database connection is valid
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="sleepDuration"></param>
    /// <param name="cancellationToken"></param>
    public static void InitializeConnection (int maxNbAttempt, TimeSpan sleepDuration, CancellationToken cancellationToken)
    {
      ILog log = LogManager.GetLogger (typeof (GDBPersistentClassFactory).FullName);

      for (int i = 1; i <= maxNbAttempt; ++i) {
        if (cancellationToken.IsCancellationRequested) {
          log.Warn ("InitializeConnection: cancellation requested");
          return;
        }
        try {
          // Initialize the NHibernateSessionFactory
          // if it has not been done before,
          // so that it is made in the main thread
          var sessionFactory = NHibernateHelper.SessionFactory;
          if (log.IsDebugEnabled) {
            log.Debug ("InitializeConnection: connection initialization successful");
          }
          return;
        }
        catch (Exception ex) {
          log.Error ($"InitializeConnection: connection initialization {i} failed with error", ex);
          if (maxNbAttempt == i) {
            log.Error ($"InitializeConnection: last connection initialization attempt {i} reached", ex);
            throw;
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeConnection: about to sleep {sleepDuration} after attempt {i}");
        }
        cancellationToken.WaitHandle.WaitOne (sleepDuration);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"InitializeConnection: maxNbAttempt {maxNbAttempt} was reached");
      }
    }

    /// <summary>
    /// Create the Model factory for GDBPersistent classes
    /// and initialize a first database connection
    /// so that it is done in the main thread
    /// 
    /// If the connection can't initialized, an exception is returned
    /// </summary>
    /// <param name="maxNbAttempt">Use 0 not to try to connect at once</param>
    /// <param name="sleepDuration"></param>
    /// <param name="cancellationToken"></param>
    public static async System.Threading.Tasks.Task CreateModelFactoryAndInitializeConnectionAsync (int maxNbAttempt, TimeSpan sleepDuration, CancellationToken cancellationToken)
    {
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();

      await InitializeConnectionAsync (maxNbAttempt, sleepDuration, cancellationToken);
    }

    /// <summary>
    /// Wait the database connection is valid
    /// </summary>
    /// <param name="maxNbAttempt"></param>
    /// <param name="sleepDuration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task InitializeConnectionAsync (int maxNbAttempt, TimeSpan sleepDuration, CancellationToken cancellationToken)
    {
      ILog log = LogManager.GetLogger (typeof (GDBPersistentClassFactory).FullName);

      for (int i = 1; i <= maxNbAttempt; ++i) {
        if (cancellationToken.IsCancellationRequested) {
          log.Warn ("InitializeConnectionAsync: cancellation requested");
          return;
        }
        try {
          // Initialize the NHibernateSessionFactory
          // if it has not been done before,
          // so that it is made in the main thread
          NHibernate.ISessionFactory sessionFactory = await
            NHibernateHelper.GetSessionFactoryAsync ();
          if (log.IsDebugEnabled) {
            log.Debug ("InitializeConnectionAsync: connection initialization successful");
          }
          return;
        }
        catch (Exception ex) {
          log.Error ($"InitializeConnectionAsync: connection initialization {i} failed with error", ex);
          if (maxNbAttempt == i) {
            log.Error ($"InitializeConnectionAsync: last connection initialization attempt {i} reached", ex);
            throw;
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"InitializeConnectionAsync: about to sleep {sleepDuration} after attempt {i}");
        }
        await System.Threading.Tasks.Task.Delay (sleepDuration, cancellationToken);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"InitializeConnectionAsync: maxNbAttempt {maxNbAttempt} was reached");
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public IAcquisitionState CreateAcquisitionState (IMachineModule machineModule, AcquisitionStateKey key)
    {
      return new AcquisitionState (machineModule, key);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineMode"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IActivityManual CreateActivityManual (IMonitoredMachine machine, IMachineMode machineMode, UtcDateTimeRange range)
    {
      return new ActivityManual (machine, machineMode, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="alarmCncInfo"></param>
    /// <param name="alarmCncSubInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="alarmNumber"></param>
    /// <returns></returns>
    public ICncAlarm CreateCncAlarm (IMachineModule machineModule,
                                    UtcDateTimeRange range,
                                    string alarmCncInfo,
                                    string alarmCncSubInfo,
                                    string alarmType,
                                    string alarmNumber)
    {
      return new CncAlarm (machineModule, range, alarmCncInfo, alarmCncSubInfo, alarmType, alarmNumber);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="cncInfo">not null or empty</param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    public ICncAlarmSeverity CreateCncAlarmSeverity (string cncInfo, string name)
    {
      return new CncAlarmSeverity (cncInfo, name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="cncInfo">not null or empty</param>
    /// <param name="rules">not null</param>
    /// <param name="severity">not null</param>
    /// <returns></returns>
    public ICncAlarmSeverityPattern CreateCncAlarmSeverityPattern (
      string cncInfo, CncAlarmSeverityPatternRules rules, ICncAlarmSeverity severity)
    {
      return new CncAlarmSeverityPattern (cncInfo, rules, severity);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="datetime"></param>
    /// <param name="alarmCncInfo"></param>
    /// <param name="alarmCncSubInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="alarmNumber"></param>
    /// <returns></returns>
    public ICurrentCncAlarm CreateCurrentCncAlarm (IMachineModule machineModule,
                                                  DateTime datetime,
                                                  string alarmCncInfo,
                                                  string alarmCncSubInfo,
                                                  string alarmType,
                                                  string alarmNumber)
    {
      return new CurrentCncAlarm (machineModule, datetime, alarmCncInfo, alarmCncSubInfo, alarmType, alarmNumber);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IGlobalModificationLog CreateGlobalModificationLog (LogLevel level, string message, IGlobalModification modification)
    {
      return new GlobalModificationLog (level, message, modification);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    public IMachineModificationLog CreateMachineModificationLog (LogLevel level, string message, IMachineModification modification)
    {
      return new MachineModificationLog (level, message, modification);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public IAutoReasonState CreateAutoReasonState (IMonitoredMachine machine, string key)
    {
      return new AutoReasonState (machine, key);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IApplicationState CreateApplicationState (string key)
    {
      return new ApplicationState (key);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="newMachineStateTemplate">not null</param>
    /// <returns></returns>
    public IAutoMachineStateTemplate CreateAutoMachineStateTemplate (IMachineMode machineMode,
                                                                     IMachineStateTemplate newMachineStateTemplate)
    {
      return new AutoMachineStateTemplate (machineMode, newMachineStateTemplate);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="sequence">not null</param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public IAutoSequence CreateAutoSequence (IMachineModule machineModule, ISequence sequence, DateTime begin)
    {
      return new AutoSequence (machineModule, sequence, begin);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="operation">not null</param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public IAutoSequence CreateAutoSequence (IMachineModule machineModule, IOperation operation, DateTime begin)
    {
      return new AutoSequence (machineModule, operation, begin);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="previousCycle"></param>
    /// <param name="nextCycle"></param>
    /// <returns></returns>
    public IBetweenCycles CreateBetweenCycles (IOperationCycle previousCycle, IOperationCycle nextCycle)
    {
      return new BetweenCycles (previousCycle, nextCycle);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public ICadModel CreateCadModel ()
    {
      return new CadModel ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public ICell CreateCell ()
    {
      return new Cell ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public ICncAcquisition CreateCncAcquisition ()
    {
      return new CncAcquisition ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public ICncDataImportLog CreateCncDataImportLog (LogLevel level, string message, IMachine machine)
    {
      return CreateCncDataImportLog (level, message, machine, null);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine">not null</param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public ICncDataImportLog CreateCncDataImportLog (LogLevel level, string message, IMachine machine, IMachineModule machineModule)
    {
      Debug.Assert (null != machine);
      return new CncDataImportLog (level,
                                       message,
                                       machine,
                                       machineModule);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    public ICncValue CreateCncValue (IMachineModule machineModule, IField field, DateTime beginDateTime)
    {
      return new CncValue (machineModule, field, beginDateTime);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public ICncVariable CreateCncVariable (IMachineModule machineModule, UtcDateTimeRange range, string key, object v)
    {
      return new CncVariable (machineModule, range, key, v);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public ICompany CreateCompany ()
    {
      return new Company ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="project"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IComponent CreateComponentFromName (IProject project, string name, IComponentType type)
    {
      IComponent component = new Component (project, type) {
        Name = name
      };
      return component;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="project"></param>
    /// <param name="code"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IComponent CreateComponentFromCode (IProject project, string code, IComponentType type)
    {
      IComponent component = new Component (project, type) {
        Code = code
      };
      return component;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="project"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IComponent CreateComponentFromType (IProject project, IComponentType type)
    {
      IComponent component = new Component (project, type);
      return component;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public IComponentIntermediateWorkPiece CreateComponentIntermediateWorkPiece (IComponent component, IIntermediateWorkPiece intermediateWorkPiece)
    {
      return new ComponentIntermediateWorkPiece (component,
                                                 intermediateWorkPiece);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// For some existing unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="component"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    [Obsolete ("User the method with the range argument instead")]
    public IComponentMachineAssociation CreateComponentMachineAssociation (IMachine machine, IComponent component, DateTime begin, UpperBound<DateTime> end)
    {
      IComponentMachineAssociation componentMachineAssociation =
        new ComponentMachineAssociation (machine, begin);
      componentMachineAssociation.Component = component;
      componentMachineAssociation.End = end;
      return componentMachineAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="component"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IComponentMachineAssociation CreateComponentMachineAssociation (IMachine machine, IComponent component, UtcDateTimeRange range)
    {
      IComponentMachineAssociation componentMachineAssociation =
        new ComponentMachineAssociation (machine, range) {
          Component = component
        };
      return componentMachineAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IComponentType CreateComponentTypeFromName (string name)
    {
      IComponentType componentType = new ComponentType ();
      componentType.Name = name;
      return componentType;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    public IComponentType CreateComponentTypeFromTranslationKey (string translationKey)
    {
      IComponentType componentType = new ComponentType ();
      componentType.TranslationKey = translationKey;
      return componentType;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// /// <param name="address"></param>
    /// <returns></returns>
    public IComputer CreateComputer (string name, string address)
    {
      IComputer computer = new Computer ();
      computer.Name = System.Environment.MachineName;
      computer.Address = System.Net.Dns.GetHostName ();
      return computer;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig CreateConfig (string key)
    {
      return new Config (key);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IConfig CreateConfig (AnalysisConfigKey key)
    {
      return CreateConfig (ConfigKeys.GetAnalysisConfigKey (key));
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public ICurrentCncValue CreateCurrentCncValue (IMachineModule machineModule, IField field)
    {
      return new CurrentCncValue (machineModule, field);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    public ICurrentMachineMode CreateCurrentMachineMode (IMonitoredMachine monitoredMachine)
    {
      return new CurrentMachineMode (monitoredMachine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ICustomer CreateCustomerFromName (string name)
    {
      var customer = new Customer ();
      customer.Name = name;
      return customer;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public ICustomer CreateCustomerFromCode (string code)
    {
      var customer = new Customer ();
      customer.Code = code;
      return customer;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IDayTemplate CreateDayTemplate ()
    {
      return new DayTemplate ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IDayTemplateChange CreateDayTemplateChange (IDayTemplate dayTemplate,
                                                       DateTime beginDateTime)
    {
      return new DayTemplateChange (dayTemplate, beginDateTime);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IDaySlot CreateDaySlot (IDayTemplate dayTemplate,
                                   UtcDateTimeRange range)
    {
      return new DaySlot (dayTemplate, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IDayTemplateSlot CreateDayTemplateSlot (IDayTemplate dayTemplate,
                                                   UtcDateTimeRange range)
    {
      return new DayTemplateSlot (dayTemplate, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="serialID"></param>
    /// <returns></returns>
    public IDeliverablePiece CreateDeliverablePiece (string serialID)
    {
      IDeliverablePiece deliverablePiece = new DeliverablePiece ();
      deliverablePiece.Code = serialID;
      return deliverablePiece;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IDepartment CreateDepartment ()
    {
      return new Department ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IDetectionAnalysisLog CreateDetectionAnalysisLog (LogLevel level, string message, IMachine machine)
    {
      return new DetectionAnalysisLog (level,
                                       message,
                                       machine,
                                       null);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public IDetectionAnalysisLog CreateDetectionAnalysisLog (LogLevel level, string message, IMachine machine, IMachineModule machineModule)
    {
      return new DetectionAnalysisLog (level,
                                       message,
                                       machine,
                                       machineModule);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public IDisplay CreateDisplay (string table)
    {
      return new Display (table);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IEmailConfig CreateEmailConfig ()
    {
      return new EmailConfig ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    public IEventLevel CreateEventLevelFromName (int priority, string name)
    {
      var eventLevel = new EventLevel (priority);
      eventLevel.Name = name;
      return eventLevel;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="translationKey">not null or empty</param>
    /// <returns></returns>
    public IEventLevel CreateEventLevelFromTranslationKey (int priority, string translationKey)
    {
      var eventLevel = new EventLevel (priority);
      eventLevel.TranslationKey = translationKey;
      return eventLevel;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="message"></param>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="v"></param>
    /// <param name="duration"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public IEventCncValue CreateEventCncValue (IEventLevel level,
                                               DateTime dateTime,
                                               string message,
                                               IMachineModule machineModule,
                                               IField field,
                                               object v,
                                               TimeSpan duration,
                                               IEventCncValueConfig config)
    {
      return new EventCncValue (level, dateTime, message, machineModule, field, v, duration, config);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="field"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public IEventCncValueConfig CreateEventCncValueConfig (string name,
                                                          IField field,
                                                          IEventLevel level,
                                                          string message,
                                                          string condition)
    {
      return new EventCncValueConfig (name, field, level, message, condition);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="monitoredMachine"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="triggerDuration"></param>
    /// <returns></returns>
    public IEventLongPeriod CreateEventLongPeriod (IEventLevel level, DateTime dateTime, IMonitoredMachine monitoredMachine, IMachineMode machineMode, IMachineObservationState machineObservationState, TimeSpan triggerDuration)
    {
      return new EventLongPeriod (level, dateTime, monitoredMachine, machineMode, machineObservationState, triggerDuration);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="triggerDuration"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public IEventLongPeriodConfig CreateEventLongPeriodConfig (TimeSpan triggerDuration, IEventLevel level)
    {
      return new EventLongPeriodConfig (triggerDuration, level);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public IEventMessage CreateEventMessage (IEventLevel level, string message)
    {
      return new EventMessage (level, DateTime.UtcNow, message);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="machine">not null</param>
    /// <param name="message"></param>
    /// <returns></returns>
    public IEventMachineMessage CreateEventMachineMessage (IEventLevel level, IMachine machine, string message)
    {
      return new EventMachineMessage (level, DateTime.UtcNow, machine, message);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="type"></param>
    /// <param name="dateTime"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public IEventToolLife CreateEventToolLife (IEventLevel level, EventToolLifeType type,
                                              DateTime dateTime, IMachineModule machineModule)
    {
      return new EventToolLife (level, type, dateTime, machineModule);
    }

    /// <summary>
    /// Create an EventToolLife object
    /// </summary>
    /// <param name="eventToolLifeType"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public IEventToolLifeConfig CreateEventToolLifeConfig (EventToolLifeType eventToolLifeType, IEventLevel level)
    {
      return new EventToolLifeConfig (eventToolLifeType, level);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcFrom"></param>
    /// <param name="utcTo"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public IFact CreateFact (IMonitoredMachine machine,
                             DateTime utcFrom,
                             DateTime utcTo,
                             IMachineMode machineMode)
    {
      return new Fact (machine, utcFrom, utcTo, machineMode);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="code">Field code. Not null or empty</param>
    /// <param name="name">Not null or empty</param>
    /// <returns></returns>
    public IField CreateFieldFromName (string code, string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (code));
      Debug.Assert (!string.IsNullOrEmpty (name));

      var field = new Field (code);
      field.Name = name;
      return field;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="code">Field code. Not null or empty</param>
    /// <param name="translationKey">Not null or empty</param>
    /// <returns></returns>
    public IField CreateFieldFromTranslationKey (string code, string translationKey)
    {
      Debug.Assert (!string.IsNullOrEmpty (code));
      Debug.Assert (!string.IsNullOrEmpty (translationKey));

      var field = new Field (code);
      field.TranslationKey = translationKey;
      return field;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="text">Can't be null</param>
    /// <param name="color">Can't be null</param>
    /// <returns></returns>
    public IFieldLegend CreateFieldLegend (IField field, string text, string color)
    {
      return new FieldLegend (field, text, color);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="type">Can't be null</param>
    /// <returns></returns>
    public IGoal CreateGoal (IGoalType type)
    {
      return new Goal (type);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name">Can't be null</param>
    /// <returns></returns>
    public IGoalType CreateGoalType (string name)
    {
      return new GoalType (name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IIntermediateWorkPiece CreateIntermediateWorkPiece (IOperation operation)
    {
      return new IntermediateWorkPiece (operation);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="oldOperation"></param>
    /// <param name="newOperation"></param>
    /// <returns></returns>
    public IIntermediateWorkPieceOperationUpdate CreateIntermediateWorkPieceOperationUpdate (IIntermediateWorkPiece intermediateworkpiece,
                                                                                            IOperation oldOperation,
                                                                                            IOperation newOperation)
    {
      return new IntermediateWorkPieceOperationUpdate (intermediateworkpiece, oldOperation, newOperation);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IIntermediateWorkPieceTarget CreateIntermediateWorkPieceTarget (IIntermediateWorkPiece intermediateworkpiece,
                                                                           IComponent component,
                                                                           IWorkOrder workOrder,
                                                                           ILine line,
                                                                           DateTime? day,
                                                                           IShift shift)
    {
      return new IntermediateWorkPieceTarget (intermediateworkpiece, component, workOrder, line, day, shift);
    }

    /// <summary>
    /// Create a new ComponentIntermediateWorkPieceUpdate
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IComponentIntermediateWorkPieceUpdate CreateComponentIntermediateWorkPieceUpdate (IComponent component, IIntermediateWorkPiece intermediateworkpiece, ComponentIntermediateWorkPieceUpdateModificationType type)
    {
      return new ComponentIntermediateWorkPieceUpdate (component, intermediateworkpiece, type);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IIsoFile CreateIsoFile (string name)
    {
      return new IsoFile (name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IIsoFileMachineModuleAssociation CreateIsoFileMachineModuleAssociation (IMachineModule machineModule,
      UtcDateTimeRange range)
    {
      return new IsoFileMachineModuleAssociation (machineModule, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="isoFile">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IIsoFileSlot CreateIsoFileSlot (IMachineModule machineModule,
                                          IIsoFile isoFile,
                                          UtcDateTimeRange range)
    {
      IIsoFileSlot isoFileSlot = new IsoFileSlot (machineModule, range);
      isoFileSlot.IsoFile = isoFile;
      return isoFileSlot;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    [Obsolete ("Use CreateJobFromName or CreateJobFromCode instead", error: false)]
    public IJob CreateJob ()
    {
      return new Job ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IJob CreateJobFromName (IWorkOrderStatus workOrderStatus, string name)
    {
      return new Job () {
        Status = workOrderStatus,
        Name = name
      };
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IJob CreateJobFromCode (IWorkOrderStatus workOrderStatus, string code)
    {
      return new Job () {
        Status = workOrderStatus,
        Code = code
      };
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public IJob CreateJob (IProject project)
    {
      return new Job (project);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IJob CreateJob (IWorkOrder workOrder)
    {
      return new Job (workOrder);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public ILine CreateLine ()
    {
      return new Line ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="line"></param>
    /// <param name="machine"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public ILineMachine CreateLineMachine (ILine line, IMachine machine, IOperation operation)
    {
      return new LineMachine (line, machine, operation);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public ILinkOperation CreateLinkOperation (IMachine machine, LinkDirection direction, UtcDateTimeRange range)
    {
      LinkOperation linkOperation = new LinkOperation (machine, direction, range);
      return linkOperation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMachine CreateMachine ()
    {
      return new Machine ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public IMachineActivitySummary CreateMachineActivitySummary (IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode)
    {
      return new MachineActivitySummary (machine, day, machineObservationState, machineMode);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IMachineActivitySummary CreateMachineActivitySummary (IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode, IShift shift)
    {
      return new MachineActivitySummary (machine, day, machineObservationState, machineMode, shift);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMachineCategory CreateMachineCategory ()
    {
      return new MachineCategory ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCell"></param>
    /// <param name="newCell"></param>
    /// <returns></returns>
    public IMachineCellUpdate CreateMachineCellUpdate (IMachine machine,
                                                       ICell oldCell,
                                                       ICell newCell)
    {
      return new MachineCellUpdate (machine, oldCell, newCell);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="cncVariableKey"></param>
    /// <param name="cncVariableValue"></param>
    /// <returns></returns>
    public IMachineCncVariable CreateMachineCncVariable (string cncVariableKey, object cncVariableValue)
    {
      return new MachineCncVariable (cncVariableKey, cncVariableValue);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCompany"></param>
    /// <param name="newCompany"></param>
    /// <returns></returns>
    public IMachineCompanyUpdate CreateMachineCompanyUpdate (IMachine machine,
                                                             ICompany oldCompany,
                                                             ICompany newCompany)
    {
      return new MachineCompanyUpdate (machine, oldCompany, newCompany);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldDepartment"></param>
    /// <param name="newDepartment"></param>
    /// <returns></returns>
    public IMachineDepartmentUpdate CreateMachineDepartmentUpdate (IMachine machine,
                                                                   IDepartment oldDepartment,
                                                                   IDepartment newDepartment)
    {
      return new MachineDepartmentUpdate (machine, oldDepartment, newDepartment);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="initialSet"></param>
    /// <returns></returns>
    public IMachineFilter CreateMachineFilter (string name, MachineFilterInitialSet initialSet)
    {
      return new MachineFilter (name, initialSet);
    }

    /// <summary>
    /// Create a MachineFilterCompany object
    /// </summary>
    /// <param name="company">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (ICompany company, MachineFilterRule rule)
    {
      return new MachineFilterCompany (company, rule);
    }

    /// <summary>
    /// Create a MachineFilterDepartment object
    /// </summary>
    /// <param name="department">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (IDepartment department, MachineFilterRule rule)
    {
      return new MachineFilterDepartment (department, rule);
    }

    /// <summary>
    /// Create a MachineFilterMachineCategory object
    /// </summary>
    /// <param name="machineCategory">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (IMachineCategory machineCategory, MachineFilterRule rule)
    {
      return new MachineFilterMachineCategory (machineCategory, rule);
    }

    /// <summary>
    /// Create a MachineFilterMachineSubCategory object
    /// </summary>
    /// <param name="machineSubCategory">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (IMachineSubCategory machineSubCategory, MachineFilterRule rule)
    {
      return new MachineFilterMachineSubCategory (machineSubCategory, rule);
    }

    /// <summary>
    /// Create a MachineFilterCell object
    /// </summary>
    /// <param name="cell">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (ICell cell, MachineFilterRule rule)
    {
      return new MachineFilterCell (cell, rule);
    }

    /// <summary>
    /// Create a MachineFilterMachine object
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IMachineFilterItem CreateMachineFilterItem (IMachine machine, MachineFilterRule rule)
    {
      return new MachineFilterMachine (machine, rule);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="running"></param>
    /// <returns></returns>
    public IMachineMode CreateMachineModeFromName (string name, bool running)
    {
      IMachineMode machineMode = new MachineMode ();
      machineMode.Name = name;
      machineMode.Running = running;
      return machineMode;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="translationKey"></param>
    /// <param name="running"></param>
    /// <returns></returns>
    public IMachineMode CreateMachineModeFromTranslationKey (string translationKey, bool running)
    {
      IMachineMode machineMode = new MachineMode ();
      machineMode.TranslationKey = translationKey;
      machineMode.Running = running;
      return machineMode;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public IMachineModeDefaultReason CreateMachineModeDefaultReason (IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      return new MachineModeDefaultReason (machineMode, machineObservationState);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    public IMachineModule CreateMachineModuleFromName (IMonitoredMachine monitoredMachine, string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      var machineModule = new MachineModule (monitoredMachine);
      machineModule.Name = name;
      return machineModule;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="code">not null or empty</param>
    /// <returns></returns>
    public IMachineModule CreateMachineModuleFromCode (IMonitoredMachine monitoredMachine, string code)
    {
      Debug.Assert (!string.IsNullOrEmpty (code));

      var machineModule = new MachineModule (monitoredMachine);
      machineModule.Code = code;
      return machineModule;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public IMachineModuleAnalysisStatus CreateMachineModuleAnalysisStatus (IMachineModule machineModule)
    {
      return new MachineModuleAnalysisStatus (machineModule);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="utcFrom"></param>
    /// <param name="utcTo"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public IMachineModuleActivity CreateMachineModuleActivity (IMachineModule machineModule,
                                                               DateTime utcFrom,
                                                               DateTime utcTo,
                                                               IMachineMode machineMode)
    {
      return new MachineModuleActivity (machineModule, utcFrom, utcTo, machineMode);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IMachineModuleDetection CreateMachineModuleDetection (IMachineModule machineModule,
                                                                 DateTime dateTime)
    {
      return new MachineModuleDetection (machineModule, dateTime);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    public IMonitoredMachineAnalysisStatus CreateMonitoredMachineAnalysisStatus (IMonitoredMachine monitoredMachine)
    {
      return new MonitoredMachineAnalysisStatus (monitoredMachine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// For some existing unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="observationState"></param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    public IMachineObservationStateAssociation CreateMachineObservationStateAssociation (IMachine machine,
                                                                                         IMachineObservationState observationState,
                                                                                         DateTime beginDateTime)
    {
      IMachineObservationStateAssociation machineObservationStateAssociation =
        new MachineObservationStateAssociation (machine, beginDateTime);
      machineObservationStateAssociation.MachineObservationState = observationState;
      return machineObservationStateAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="observationState"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IMachineObservationStateAssociation CreateMachineObservationStateAssociation (IMachine machine,
                                                                                         IMachineObservationState observationState,
                                                                                         UtcDateTimeRange range)
    {
      IMachineObservationStateAssociation machineObservationStateAssociation =
        new MachineObservationStateAssociation (machine, range);
      machineObservationStateAssociation.MachineObservationState = observationState;
      return machineObservationStateAssociation;
    }


    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMachineObservationState CreateMachineObservationState ()
    {
      return new MachineObservationState ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public IMachineOperationType CreateMachineOperationType (IMachine machine, IOperationType operationType)
    {
      return new MachineOperationType (machine, operationType);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMachineStateTemplate CreateMachineStateTemplate (string name)
    {
      return new MachineStateTemplate (name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// For some existing unit tests only
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    public IMachineStateTemplateAssociation
      CreateMachineStateTemplateAssociation (IMachine machine,
                                             IMachineStateTemplate machineStateTemplate,
                                             DateTime beginDateTime)
    {
      IMachineStateTemplateAssociation association =
        new MachineStateTemplateAssociation (machine,
                                             machineStateTemplate,
                                             beginDateTime);
      return association;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IMachineStateTemplateAssociation
      CreateMachineStateTemplateAssociation (IMachine machine,
                                             IMachineStateTemplate machineStateTemplate,
                                             UtcDateTimeRange range)
    {
      IMachineStateTemplateAssociation association =
        new MachineStateTemplateAssociation (machine,
                                             machineStateTemplate,
                                             range);
      return association;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="from">not null</param>
    /// <param name="to">not null</param>
    /// <returns></returns>
    public IMachineStateTemplateFlow
      CreateMachineStateTemplateFlow (IMachineStateTemplate from,
                                      IMachineStateTemplate to)
    {
      return new MachineStateTemplateFlow (from, to);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineStateTemplate">if null, it is applicable to all machine state templates</param>
    /// <param name="role">if null, it is applicable to all roles</param>
    /// <param name="accessPrivilege"></param>
    /// <returns></returns>
    public IMachineStateTemplateRight
      CreateMachineStateTemplateRight (IMachineStateTemplate machineStateTemplate,
                                       IRole role,
                                       RightAccessPrivilege accessPrivilege)
    {
      IMachineStateTemplateRight right =
        new MachineStateTemplateRight (machineStateTemplate, role, accessPrivilege);
      return right;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    public IMachineStatus CreateMachineStatus (IMonitoredMachine monitoredMachine)
    {
      return new MachineStatus (monitoredMachine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMachineSubCategory CreateMachineSubCategory ()
    {
      return new MachineSubCategory ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IMonitoredMachine CreateMonitoredMachine ()
    {
      return new MonitoredMachine ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IMonitoredMachine CreateMonitoredMachine (IMachine machine)
    {
      return new MonitoredMachine (machine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public INamedConfig CreateNamedConfig (string name, string key)
    {
      return new NamedConfig (name, key);
    }

    /// <summary>
    /// Create new NonConformanceReport
    /// </summary>
    /// <param name="name">not null and not empty</param>
    /// <returns></returns>
    public INonConformanceReason CreateNonConformanceReason (string name)
    {
      return new NonConformanceReason (name);
    }

    /// <summary>
    /// Create new NonConformanceReport
    /// </summary>
    /// <param name="deliverablePiece"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public INonConformanceReport CreateNonConformanceReport (IDeliverablePiece deliverablePiece,
                                                            IMachine machine)
    {
      INonConformanceReport nonConformanceReport = new NonConformanceReport (deliverablePiece, machine);
      return nonConformanceReport;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IObservationStateSlot CreateObservationStateSlot (IMachine machine, UtcDateTimeRange range)
    {
      return new ObservationStateSlot (machine, range);
    }

    /// <summary>
    /// Create a new Operation
    /// </summary>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public IOperation CreateOperation (IOperationType operationType)
    {
      return new Operation (operationType);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public IOperationDuration CreateOperationDuration (IOperation operation, DateTime? from = null) => new OperationDuration (operation, from);

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operationModel"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public IOperationDuration CreateOperationDuration (IOperationModel operationModel, DateTime? from = null) => new OperationDuration (operationModel, from);

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationInformation CreateOperationInformation (IOperation operation, DateTime dateTime)
    {
      return new OperationInformation (operation, dateTime);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IOperationCycle CreateOperationCycle (IMachine machine)
    {
      return new OperationCycle (machine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// 
    /// For the unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                           DateTime begin)
    {
      return new OperationMachineAssociation (machine, begin);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// For the unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfModificationAnalysis"></param>
    /// <returns></returns>
    public IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                           DateTime begin,
                                                                           IModification mainModification,
                                                                           bool partOfModificationAnalysis)
    {
      return new OperationMachineAssociation (machine, begin, mainModification, partOfModificationAnalysis);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                           UtcDateTimeRange range)
    {
      return new OperationMachineAssociation (machine, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfModificationAnalysis"></param>
    /// <returns></returns>
    public IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                           UtcDateTimeRange range,
                                                                           IModification mainModification,
                                                                           bool partOfModificationAnalysis)
    {
      return new OperationMachineAssociation (machine, range, mainModification, partOfModificationAnalysis);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operationRevision"></param>
    /// <returns></returns>
    public IOperationModel CreateOperationModel (IOperationRevision operationRevision) => new OperationModel (operationRevision);

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IOperationModel CreateOperationModel (IOperation operation) => new OperationModel (operation);

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IOperationRevision CreateOperationRevision (IOperation operation) => new OperationRevision (operation);

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operation"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IOperationSlot CreateOperationSlot (IMachine machine,
                                              IOperation operation,
                                              IComponent component,
                                              IWorkOrder workOrder,
                                              ILine line,
                                              IManufacturingOrder manufacturingOrder,
                                              DateTime? day,
                                              IShift shift,
                                              UtcDateTimeRange range)
    {
      return new OperationSlot (machine, operation, component, workOrder, line, manufacturingOrder, day, shift, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IOperationSlotSplit CreateOperationSlotSplit (IMachine machine)
    {
      return new OperationSlotSplit (machine);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="deliverablePiece"></param>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    public IOperationCycleDeliverablePiece
      CreateOperationCycleDeliverablePiece (IDeliverablePiece deliverablePiece,
                                           IOperationCycle operationCycle)
    {
      return new OperationCycleDeliverablePiece (deliverablePiece, operationCycle);
    }


    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public IOperationSourceWorkPiece CreateOperationSourceWorkPiece (IOperation operation, IIntermediateWorkPiece intermediateWorkPiece)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IOperationType CreateOperationTypeFromName (string name)
    {
      IOperationType operationType = new OperationType ();
      operationType.Name = name;
      return operationType;
    }

    /// <summary>
    /// Create a package
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    public IPackage CreatePackage (string identifyingName)
    {
      return new Package (identifyingName);
    }

    /// <summary>
    /// Create a package / plugin association
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public IPackagePluginAssociation CreatePackagePluginAssociation (IPackage package, IPlugin plugin, string name)
    {
      return new PackagePluginAssociation (package, plugin, name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public IPart CreatePart (IComponentType componentType)
    {
      return new Part (componentType);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IPart CreatePart (IComponent component)
    {
      return new Part (component);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public IPart CreatePart (IProject project)
    {
      return new Part (project);
    }

    /// <summary>
    /// Create a new Path
    /// </summary>
    /// <returns></returns>
    public IPath CreatePath ()
    {
      IPath path = new OpPath ();
      return path;
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IPath CreatePath (IOperation operation)
    {
      var path = new OpPath (operation);
      return path;
    }

    /// <summary>
    /// Create a plugin
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    public IPlugin CreatePlugin (string identifyingName)
    {
      return new Plugin (identifyingName);
    }

    /// <summary>
    /// Create a new ProcessMachineStateTemplate
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IProcessMachineStateTemplate CreateProcessMachineStateTemplate (IMachine machine, UtcDateTimeRange range)
    {
      return new ProcessMachineStateTemplate (machine, range);
    }

    /// <summary>
    /// Create a new ProductionAnalysisStatus
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IProductionAnalysisStatus CreateProductionAnalysisStatus (IMachine machine)
    {
      return new ProductionAnalysisStatus (machine);
    }

    /// <summary>
    /// Create a new ProductionInformationShift
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="checkedValue"></param>
    /// <returns></returns>
    public IProductionInformationShift CreateProductionInformationShift (IMachine machine,
                                                                         DateTime day,
                                                                         IShift shift,
                                                                         IIntermediateWorkPiece intermediateWorkPiece,
                                                                         int checkedValue)
    {
      return new ProductionInformationShift (machine,
                                             day,
                                             shift,
                                             intermediateWorkPiece,
                                             checkedValue);
    }

    /// <summary>
    /// Create a new project from a code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public IProject CreateProjectFromCode (string code)
    {
      IProject project = new Project ();
      project.Code = code;
      return project;
    }

    /// <summary>
    /// Create a new project from a name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IProject CreateProjectFromName (string name)
    {
      IProject project = new Project ();
      project.Name = name;
      return project;
    }

    /// <summary>
    /// Create a new ProjectComponentUpdate
    /// </summary>
    /// <param name="component"></param>
    /// <param name="oldProject"></param>
    /// <param name="newProject"></param>
    /// <returns></returns>
    public IProjectComponentUpdate CreateProjectComponentUpdate (IComponent component, IProject oldProject, IProject newProject)
    {
      return new ProjectComponentUpdate (component, oldProject, newProject);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public IProductionRateSummary CreateProductionRateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState)
    {
      return new ProductionRateSummary (machine, day, shift, machineObservationState);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    public IProductionState CreateProductionState (string color)
    {
      return new ProductionState (color);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    public IProductionStateSummary CreateProductionStateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IProductionState productionState)
    {
      return new ProductionStateSummary (machine, day, shift, machineObservationState, productionState);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <returns></returns>
    public IReason CreateReason (IReasonGroup reasonGroup)
    {
      return new Reason (reasonGroup);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IReasonGroup CreateReasonGroup ()
    {
      return new ReasonGroup ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    public IReasonMachineAssociation CreateReasonMachineAssociation (IMachine machine,
                                                                     UtcDateTimeRange range)
    {
      return new ReasonMachineAssociation (machine, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// Specific constructor when begin may be equal to end
    /// because of the use of dynamic times
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="dynamic">dynamic times, not null or empty</param>
    /// <returns></returns>
    public IReasonMachineAssociation CreateReasonMachineAssociation (IMachine machine,
                                                                     LowerBound<DateTime> begin,
                                                                     UpperBound<DateTime> end,
                                                                     string dynamic)
    {
      return new ReasonMachineAssociation (machine, begin, end, dynamic);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="reasonMachineAssociation"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IReasonProposal CreateReasonProposal (IReasonMachineAssociation reasonMachineAssociation,
      UtcDateTimeRange range)
    {
      return new ReasonProposal (reasonMachineAssociation, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public IReasonSelection CreateReasonSelection (IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      return new ReasonSelection (machineMode, machineObservationState);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <param name="detailsRequired"></param>
    /// <returns></returns>
    public IReasonSelection CreateReasonSelection (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, bool detailsRequired)
    {
      return new ReasonSelection (machineMode, machineObservationState, reason, detailsRequired);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IReasonSlot CreateReasonSlot (IMachine machine, UtcDateTimeRange range)
    {
      return new ReasonSlot (machine, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public IReasonSummary CreateReasonSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IReason reason)
    {
      return new ReasonSummary (machine, day, shift, machineObservationState, reason);
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="user"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public IRefreshToken CreateRefreshToken (IUser user, TimeSpan timeSpan)
    {
      return new RefreshToken (user, timeSpan);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IRevision CreateRevision ()
    {
      IRevision revision = new Revision ();
      return revision;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IRole CreateRoleFromName (string name)
    {
      IRole role = new Role ();
      role.Name = name;
      return role;
    }

    /// <summary>
    /// Create a new Sequence
    /// </summary>
    /// <returns></returns>
    public ISequence CreateSequence (String name)
    {
      ISequence sequence = new OpSequence ();
      sequence.Name = name;
      return sequence;
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="operation"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public ISequence CreateSequence (string name, IOperation operation, IPath path)
    {
      var sequence = new OpSequence (operation, path);
      sequence.Name = name;
      return sequence;
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="operationModel"></param>
    /// <param name="pathNumber"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    public ISequence CreateSequence (string name, IOperationModel operationModel, int pathNumber, ISequence before, ISequence after)
    {
      // TODO: ...
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="sequenceOperationModel"></param>
    /// <param name="estimatedDuration"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public ISequenceDuration CreateSequenceDuration (ISequenceOperationModel sequenceOperationModel, TimeSpan estimatedDuration, DateTime? from)
    {
      // TODO: ...
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public ISequenceMilestone CreateSequenceMilestone (IMachineModule machineModule)
    {
      var sequenceMilestone = new SequenceMilestone (machineModule);
      return sequenceMilestone;
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="operationModel"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ISequenceOperationModel CreateSequenceOperationModel (ISequence sequence, IOperationModel operationModel, ISequence before, ISequence after)
    {
      // TODO: ...
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="sequence"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public ISequenceSlot CreateSequenceSlot (IMachineModule machineModule,
                                             ISequence sequence,
                                             UtcDateTimeRange range)
    {
      return new SequenceSlot (machineModule, sequence, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="serialNumber"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public ISerialNumberMachineStamp CreateSerialNumberMachineStamp (IMachine machine, string serialNumber, DateTime dateTime)
    {
      ISerialNumberMachineStamp serialNumberMachineStamp = new SerialNumberMachineStamp (machine, serialNumber, dateTime);
      return serialNumberMachineStamp;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="serialNumber"></param>
    /// <param name="beginOrEndDateTime"></param>
    /// <param name="isBegin"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public ISerialNumberModification CreateSerialNumberModification (IMachine machine,
                                                                    string serialNumber,
                                                                    DateTime beginOrEndDateTime,
                                                                    bool isBegin,
                                                                    DateTime dateTime)
    {
      ISerialNumberModification serialNumberModification =
        new SerialNumberModification (machine, serialNumber, beginOrEndDateTime, isBegin, dateTime);
      return serialNumberModification;
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    /// <param name="computer">not null</param>
    /// <param name="name">not null or empty</param>
    /// <param name="program">not null or empty</param>
    /// <param name="lemoine"></param>
    /// <returns></returns>
    public IService CreateService (IComputer computer, string name, string program, bool lemoine)
    {
      Debug.Assert (null != computer);
      Debug.Assert (!string.IsNullOrEmpty (name));
      Debug.Assert (!string.IsNullOrEmpty (program));

      var service = new Service ();
      service.Computer = computer;
      service.Name = name;
      service.Program = program;
      service.Lemoine = lemoine;
      return service;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public IShift CreateShiftFromCode (string code)
    {
      IShift shift = new Shift ();
      shift.Code = code;
      return shift;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IShift CreateShiftFromName (string name)
    {
      IShift shift = new Shift ();
      shift.Name = name;
      return shift;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="shiftTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IShiftSlot CreateShiftSlot (IShiftTemplate shiftTemplate,
                                       UtcDateTimeRange range)
    {
      IShiftSlot shiftSlot = new ShiftSlot (shiftTemplate, range);
      return shiftSlot;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IShiftTemplate CreateShiftTemplate (string name)
    {
      IShiftTemplate shiftTemplate = new ShiftTemplate (name);
      return shiftTemplate;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    public IShiftTemplateAssociation CreateShiftTemplateAssociation (IShiftTemplate shiftTemplate,
                                                                     DateTime beginDateTime)
    {
      IShiftTemplateAssociation shiftTemplateAssociation = new ShiftTemplateAssociation (shiftTemplate,
                                                                                         beginDateTime);
      return shiftTemplateAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public ISimpleOperation CreateSimpleOperation (IOperationType operationType)
    {
      return new SimpleOperation (operationType);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public ISimpleOperation CreateSimpleOperation (IOperation operation)
    {
      return new SimpleOperation (operation);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public ISimpleOperation CreateSimpleOperation (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return new SimpleOperation (intermediateWorkPiece);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    public IStamp CreateStamp ()
    {
      return new Stamp ();
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IStampingConfigByName CreateStampingConfigByName (string name)
    {
      return new StampingConfigByName (name);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public IStampingValue CreateStampingValue (ISequence sequence, IField field)
    {
      return new StampingValue (sequence, field);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="xmlElement"></param>
    /// <returns></returns>
    public ISynchronizationLog CreateSynchronizationLog (LogLevel level,
                                                         string message,
                                                         string xmlElement)
    {
      return new SynchronizationLog (level,
                                     message,
                                     xmlElement);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="manufacturingOrderId"></param>
    /// <returns></returns>
    public IManufacturingOrder CreateManufacturingOrder (int manufacturingOrderId)
    {
      var manufacturingOrder = new ManufacturingOrder (manufacturingOrderId);
      return manufacturingOrder;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IManufacturingOrderMachineAssociation CreateManufacturingOrderMachineAssociation (IMachine machine,
      IManufacturingOrder manufacturingOrder,
      UtcDateTimeRange range)
    {
      var machineAssociation = new ManufacturingOrderMachineAssociation (machine,
                                                              range);
      machineAssociation.ManufacturingOrder = manufacturingOrder;
      return machineAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="position"></param>
    /// <param name="unit"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public IToolLife CreateToolLife (IMachineModule machineModule, IToolPosition position,
                                    IUnit unit, Lemoine.Core.SharedData.ToolLifeDirection direction)
    {
      return new ToolLife (machineModule, position, unit, direction);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="toolId"></param>
    /// <returns></returns>
    public IToolPosition CreateToolPosition (IMachineModule machineModule, string toolId)
    {
      return new ToolPosition (machineModule, toolId);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    public ITranslation CreateTranslation (string locale, string translationKey)
    {
      return new Translation (locale, translationKey);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IUnit CreateUnit ()
    {
      return new Unit ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public IUser CreateUser (string login, string password)
    {
      return new User (login, password);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public IUserAttendance CreateUserAttendance (IUser user)
    {
      return new UserAttendance (user);
    }

    /// <summary>
    /// Create a new UserMachineAssociation
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IUserMachineAssociation CreateUserMachineAssociation (IUser user,
                                                                 UtcDateTimeRange range)
    {
      IUserMachineAssociation association = new UserMachineAssociation (user, range);
      return association;
    }

    /// <summary>
    /// Create a new UserMachineSlot
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IUserMachineSlot CreateUserMachineSlot (IUser user,
                                                   UtcDateTimeRange range)
    {
      IUserMachineSlot slot = new UserMachineSlot (user, range);
      return slot;
    }

    /// <summary>
    /// Create a new UserShiftAssociation
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IUserShiftAssociation CreateUserShiftAssociation (IUser user,
                                                             UtcDateTimeRange range,
                                                             IShift shift)
    {
      IUserShiftAssociation association = new UserShiftAssociation (user, range);
      association.Shift = shift;
      return association;
    }

    /// <summary>
    /// Create a new UserShiftSlot
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <param name="shift">Not null</param>
    /// <returns></returns>
    public IUserShiftSlot CreateUserShiftSlot (IUser user,
                                               UtcDateTimeRange range,
                                               IShift shift)
    {
      IUserShiftSlot slot = new UserShiftSlot (user, range);
      slot.Shift = shift;
      return slot;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IUserSlot CreateUserSlot (IUser user, UtcDateTimeRange range)
    {
      return new UserSlot (user, range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="workOrderStatus"></param>
    /// <param name="workOrderName"></param>
    /// <returns></returns>
    public IWorkOrder CreateWorkOrder (IWorkOrderStatus workOrderStatus, string workOrderName)
    {
      Debug.Assert (null != workOrderStatus);
      Debug.Assert (!string.IsNullOrEmpty (workOrderName));

      IWorkOrder workOrder = new WorkOrder ();
      workOrder.Status = workOrderStatus;
      workOrder.Name = workOrderName;
      return workOrder;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="workOrderStatus"></param>
    /// <param name="workOrderCode"></param>
    /// <returns></returns>
    public IWorkOrder CreateWorkOrderFromCode (IWorkOrderStatus workOrderStatus, string workOrderCode)
    {
      Debug.Assert (null != workOrderStatus);
      Debug.Assert (!string.IsNullOrEmpty (workOrderCode));

      IWorkOrder workOrder = new WorkOrder ();
      workOrder.Status = workOrderStatus;
      workOrder.Code = workOrderCode;
      return workOrder;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="range"></param>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IWorkOrderLine CreateWorkOrderLine (ILine line, UtcDateTimeRange range, IWorkOrder workOrder)
    {
      IWorkOrderLine workOrderLine = new WorkOrderLine (line, range, workOrder);
      return workOrderLine;
    }

    /// <summary>
    /// Create a new WorkOrderLineAssociation
    /// </summary>
    /// <param name="line"></param>
    /// <param name="beginTime"></param>
    /// <param name="deadline"></param>
    /// <returns></returns>
    public IWorkOrderLineAssociation CreateWorkOrderLineAssociation (ILine line, DateTime beginTime, DateTime deadline)
    {
      return new WorkOrderLineAssociation (line, beginTime, deadline);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    ///
    /// For the unit tests
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="beginTime"></param>
    /// <returns></returns>
    public IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation (IMachine machine,
                                                                          IWorkOrder workOrder,
                                                                          DateTime beginTime)
    {
      IWorkOrderMachineAssociation workOrderMachineAssociation = new WorkOrderMachineAssociation (machine,
                                                                                                 beginTime);
      workOrderMachineAssociation.WorkOrder = workOrder;
      return workOrderMachineAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation (IMachine machine,
                                                                          IWorkOrder workOrder,
                                                                          UtcDateTimeRange range)
    {
      IWorkOrderMachineAssociation workOrderMachineAssociation = new WorkOrderMachineAssociation (machine,
                                                                                                 range);
      workOrderMachineAssociation.WorkOrder = workOrder;
      return workOrderMachineAssociation;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IWorkOrderMachineStamp CreateWorkOrderMachineStamp (IMachine machine,
                                                               IWorkOrder workOrder,
                                                               DateTime dateTime)
    {
      IWorkOrderMachineStamp workOrderMachineStamp = new WorkOrderMachineStamp (machine, workOrder, dateTime);
      return workOrderMachineStamp;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public IWorkOrderProject CreateWorkOrderProject (IWorkOrder workOrder, IProject project)
    {
      return new WorkOrderProject (workOrder, project);
    }

    /// <summary>
    /// Create a new WorkOrderProjectUpdate
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IWorkOrderProjectUpdate CreateWorkOrderProjectUpdate (IWorkOrder workOrder, IProject project, WorkOrderProjectUpdateModificationType type)
    {
      return new WorkOrderProjectUpdate (workOrder, project, type);
    }

    /// <summary>
    /// Create a new WorkOrderStatus
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IWorkOrderStatus CreateWorkOrderStatusFromName (string name)
    {
      IWorkOrderStatus workOrderStatus = new WorkOrderStatus ();
      workOrderStatus.Name = name;
      return workOrderStatus;
    }

    /// <summary>
    /// <see cref="IModelFactory"/>
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="dateTimeRange"></param>
    /// <returns></returns>
    public IScrapReport CreateScrapReport (IOperationSlot operationSlot, UtcDateTimeRange dateTimeRange)
    {
      var scrapReport = new ScrapReport (operationSlot, dateTimeRange);
      return scrapReport;
    }

    /// <summary>
    /// Create new <see cref="IScrapReasonReport"/>
    /// </summary>
    public IScrapReasonReport CreateScrapReasonReport (IScrapReport scrapReport, INonConformanceReason reason, int quantity)
    {
      var scrapReasonReport = new ScrapReasonReport (scrapReport, reason, quantity);
      return scrapReasonReport;
    }
  }
}
