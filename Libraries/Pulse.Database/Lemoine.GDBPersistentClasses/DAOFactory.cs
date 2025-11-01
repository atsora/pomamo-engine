// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Diagnostics;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// NHibernate DAO Factory
  /// </summary>
  public class DAOFactory : Lemoine.ModelDAO.IDAOFactory, IDatabaseConnection
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DAOFactory).FullName);

    #region Members
    readonly IPersistentClassModel m_persistentClassModel;
    volatile int m_postgreSQLVersionNumCache = 0; // 0: not set
    #endregion // Members

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="persistentClassModel">not null</param>
    public DAOFactory (IPersistentClassModel persistentClassModel)
    {
      Debug.Assert (null != persistentClassModel);

      m_persistentClassModel = persistentClassModel;
      Lemoine.Core.ExceptionManagement.ExceptionTest
        .AddTest (new Lemoine.GDBUtils.DatabaseException ());
    }
    #endregion // Constructor

    /// <summary>
    /// <see cref="IDatabaseConnectionStatus"/>
    /// </summary>
    public bool IsDatabaseConnectionUp => this.GetStatus ().Equals (ConnectionStatus.Started);

    /// <summary>
    /// Open a IDAOSession
    /// </summary>
    /// <returns></returns>
    public IDAOSession OpenSession ()
    {
      try {
        return new DAOSession ();
      }
      catch (Exception ex) {
        log.Error ("OpenSession: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public ConnectionStatus Status => this.GetStatus ();

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    /// <param name="proxy"></param>
    public void Initialize (object proxy)
    {
      this.InitializeProxy (proxy);
    }

    /// <summary>
    /// Check if an proxy object has been initialized / is not lazy
    /// </summary>
    /// <param name="proxy"></param>
    /// <returns></returns>
    public bool IsInitialized (object proxy)
    {
      return NHibernateUtil.IsInitialized (proxy);
    }

    /// <summary>
    /// Is a session active ?
    /// </summary>
    /// <returns></returns>
    public bool IsSessionActive ()
    {
      return this.IsDatabaseSessionActive ();
    }

    /// <summary>
    /// Get the server version number, for example: 90501
    /// 
    /// If it is available in cache, get it from the cache
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request fails or if the result is not a valid integer</exception>
    public int GetPostgreSQLVersionNum ()
    {
      if (0 != m_postgreSQLVersionNumCache) {
        return m_postgreSQLVersionNumCache;
      }

      m_postgreSQLVersionNumCache = this.GetPostgreSQLVersionNumber ();
      return m_postgreSQLVersionNumCache;
    }

    /// <summary>
    /// Check a very basic connection
    /// 
    /// Should be fast, even if the full connection is not initialized
    /// </summary>
    /// <exception cref="System.Exception">Connection problem</exception>
    public void CheckBasicConnection ()
    {
      this.CheckBasicRequest ();
    }

    /// <summary>
    /// For the unit tests, store in database the data in the accumulator
    /// </summary>
    public static void EmptyAccumulators ()
    {
      AnalysisAccumulator.Store ("EmptyAccumulators");
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    /// <param name="message"></param>
    public void PushMessage (string message)
    {
      AnalysisAccumulator.PushMessage (message);
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public void Flush ()
    {
      this.FlushData ();
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    /// <param name="o"></param>
    public void Evict (object o)
    {
      this.EvictData (o);
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IAcquisitionStateDAO AcquisitionStateDAO
    {
      get { return new AcquisitionStateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IActivityManualDAO ActivityManualDAO
    {
      get { return new ActivityManualDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncAlarmDAO CncAlarmDAO
    {
      get { return new CncAlarmDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICurrentCncAlarmDAO CurrentCncAlarmDAO
    {
      get { return new CurrentCncAlarmDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncAlarmSeverityDAO CncAlarmSeverityDAO
    {
      get { return new CncAlarmSeverityDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncAlarmSeverityPatternDAO CncAlarmSeverityPatternDAO
    {
      get { return new CncAlarmSeverityPatternDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IAnalysisLogDAO AnalysisLogDAO
    {
      get { return new AnalysisLogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IGlobalModificationLogDAO GlobalModificationLogDAO
    {
      get { return new GlobalModificationLogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModificationLogDAO MachineModificationLogDAO
    {
      get { return new MachineModificationLogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IApplicationStateDAO ApplicationStateDAO
    {
      get { return new ApplicationStateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IAutoReasonStateDAO AutoReasonStateDAO
    {
      get { return new AutoReasonStateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IAutoMachineStateTemplateDAO AutoMachineStateTemplateDAO
    {
      get { return new AutoMachineStateTemplateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IAutoSequenceDAO AutoSequenceDAO
    {
      get { return new AutoSequenceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IBetweenCyclesDAO BetweenCyclesDAO
    {
      get { return new BetweenCyclesDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICadModelDAO CadModelDAO
    {
      get { return new CadModelDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICellDAO CellDAO
    {
      get { return new CellDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncAcquisitionDAO CncAcquisitionDAO
    {
      get { return new CncAcquisitionDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncDataImportLogDAO CncDataImportLogDAO
    {
      get { return new CncDataImportLogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncValueDAO CncValueDAO
    {
      get { return new CncValueDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICncVariableDAO CncVariableDAO
    {
      get { return new CncVariableDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICompanyDAO CompanyDAO
    {
      get { return new CompanyDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComponentDAO ComponentDAO
    {
      get { return new ComponentDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComponentMachineAssociationDAO ComponentMachineAssociationDAO
    {
      get { return new ComponentMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComponentIntermediateWorkPieceDAO ComponentIntermediateWorkPieceDAO
    {
      get { return new ComponentIntermediateWorkPieceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComponentIntermediateWorkPieceUpdateDAO ComponentIntermediateWorkPieceUpdateDAO
    {
      get { return new ComponentIntermediateWorkPieceUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComponentTypeDAO ComponentTypeDAO
    {
      get { return new ComponentTypeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IComputerDAO ComputerDAO
    {
      get { return new ComputerDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IConfigDAO ConfigDAO
    {
      get { return new ConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICurrentCncValueDAO CurrentCncValueDAO
    {
      get { return new CurrentCncValueDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ICurrentMachineModeDAO CurrentMachineModeDAO
    {
      get { return new CurrentMachineModeDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public ICustomerDAO CustomerDAO => new CustomerDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDaySlotDAO DaySlotDAO
    {
      get { return new DaySlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDayTemplateChangeDAO DayTemplateChangeDAO
    {
      get { return new DayTemplateChangeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDayTemplateDAO DayTemplateDAO
    {
      get { return new DayTemplateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDayTemplateSlotDAO DayTemplateSlotDAO
    {
      get { return new DayTemplateSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDepartmentDAO DepartmentDAO
    {
      get { return new DepartmentDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDetectionAnalysisLogDAO DetectionAnalysisLogDAO
    {
      get { return new DetectionAnalysisLogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDeliverablePieceDAO DeliverablePieceDAO
    {
      get { return new DeliverablePieceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IDisplayDAO DisplayDAO
    {
      get { return new DisplayDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEmailConfigDAO EmailConfigDAO
    {
      get { return new EmailConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventDAO EventDAO
    {
      get { return new EventDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventLevelDAO EventLevelDAO
    {
      get { return new EventLevelDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventCncValueDAO EventCncValueDAO
    {
      get { return new EventCncValueDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventCncValueConfigDAO EventCncValueConfigDAO
    {
      get { return new EventCncValueConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventLongPeriodDAO EventLongPeriodDAO
    {
      get { return new EventLongPeriodDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventLongPeriodConfigDAO EventLongPeriodConfigDAO
    {
      get { return new EventLongPeriodConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventMessageDAO EventMessageDAO
    {
      get { return new EventMessageDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventMachineMessageDAO EventMachineMessageDAO
    {
      get { return new EventMachineMessageDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventToolLifeDAO EventToolLifeDAO
    {
      get { return new EventToolLifeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IEventToolLifeConfigDAO EventToolLifeConfigDAO
    {
      get { return new EventToolLifeConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IFactDAO FactDAO
    {
      get { return new FactDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IFieldDAO FieldDAO
    {
      get { return new FieldDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IGoalDAO GoalDAO
    {
      get { return new GoalDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IGoalTypeDAO GoalTypeDAO
    {
      get { return new GoalTypeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IIsoFileDAO IsoFileDAO
    {
      get { return new IsoFileDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IIntermediateWorkPieceDAO IntermediateWorkPieceDAO
    {
      get { return new IntermediateWorkPieceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IIntermediateWorkPieceOperationUpdateDAO IntermediateWorkPieceOperationUpdateDAO
    {
      get { return new IntermediateWorkPieceOperationUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IIntermediateWorkPieceTargetDAO IntermediateWorkPieceTargetDAO
    {
      get { return new IntermediateWorkPieceTargetDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IIsoFileSlotDAO IsoFileSlotDAO
    {
      get { return new IsoFileSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IJobDAO JobDAO
    {
      get { return new JobDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ILineDAO LineDAO
    {
      get { return new LineDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ILineMachineDAO LineMachineDAO
    {
      get { return new LineMachineDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ILinkOperationDAO LinkOperationDAO
    {
      get { return new LinkOperationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ILogDAO LogDAO
    {
      get { return new LogDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineDAO MachineDAO
    {
      get { return new MachineDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IFieldLegendDAO FieldLegendDAO
    {
      get { return new FieldLegendDAO (); }
    }


    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineActivitySummaryDAO MachineActivitySummaryDAO
    {
      get { return new MachineActivitySummaryDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineCategoryDAO MachineCategoryDAO
    {
      get { return new MachineCategoryDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineCellUpdateDAO MachineCellUpdateDAO
    {
      get { return new MachineCellUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineCncVariableDAO MachineCncVariableDAO
    {
      get { return new MachineCncVariableDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineCompanyUpdateDAO MachineCompanyUpdateDAO
    {
      get { return new MachineCompanyUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineDepartmentUpdateDAO MachineDepartmentUpdateDAO
    {
      get { return new MachineDepartmentUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineFilterDAO MachineFilterDAO
    {
      get { return new MachineFilterDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModeDAO MachineModeDAO
    {
      get { return new MachineModeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModeDefaultReasonDAO MachineModeDefaultReasonDAO
    {
      get { return new MachineModeDefaultReasonDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModuleDAO MachineModuleDAO
    {
      get { return new MachineModuleDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModuleActivityDAO MachineModuleActivityDAO
    {
      get { return new MachineModuleActivityDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModuleAnalysisStatusDAO MachineModuleAnalysisStatusDAO
    {
      get { return new MachineModuleAnalysisStatusDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModuleDetectionDAO MachineModuleDetectionDAO
    {
      get { return new MachineModuleDetectionDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineMonitoringTypeDAO MachineMonitoringTypeDAO
    {
      get { return new MachineMonitoringTypeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineObservationStateDAO MachineObservationStateDAO
    {
      get { return new MachineObservationStateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineObservationStateAssociationDAO MachineObservationStateAssociationDAO
    {
      get { return new MachineObservationStateAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStateTemplateDAO MachineStateTemplateDAO
    {
      get { return new MachineStateTemplateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStateTemplateAssociationDAO MachineStateTemplateAssociationDAO
    {
      get { return new MachineStateTemplateAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStateTemplateFlowDAO MachineStateTemplateFlowDAO
    {
      get { return new MachineStateTemplateFlowDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStateTemplateRightDAO MachineStateTemplateRightDAO
    {
      get { return new MachineStateTemplateRightDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStateTemplateSlotDAO MachineStateTemplateSlotDAO
    {
      get
      {
        const string IMPLEMENTATION_KEY = "Database.MachineStateTemplateSlotDAO.Implementation";
        const int IMPLEMENTATION_EXTEND_DIFFERENT = 0;
        const int IMPLEMENTATION_EXTEND_PROGRESSIVELY = 1;
        const int IMPLEMENTATION_FROM_VIEW = 2;
        const int IMPLEMENTATION_DEFAULT = IMPLEMENTATION_EXTEND_DIFFERENT;

        int implementation = Lemoine.Info.ConfigSet.LoadAndGet<int> (IMPLEMENTATION_KEY, IMPLEMENTATION_DEFAULT);
        switch (implementation) {
        case IMPLEMENTATION_EXTEND_DIFFERENT:
          return new MachineStateTemplateSlotDAOExtendDifferent ();
        case IMPLEMENTATION_EXTEND_PROGRESSIVELY:
          return new MachineStateTemplateSlotDAOExtendProgressively ();
        case IMPLEMENTATION_FROM_VIEW:
          return new MachineStateTemplateSlotDAOFromView ();
        default:
          log.FatalFormat ("MachineStateTemplateSlotDAO: " +
                           "unknown implementation {0}",
                           implementation);
          throw new InvalidOperationException ();
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineStatusDAO MachineStatusDAO
    {
      get { return new MachineStatusDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineSubCategoryDAO MachineSubCategoryDAO
    {
      get { return new MachineSubCategoryDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IModificationDAO ModificationDAO
    {
      get { return new ModificationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IGlobalModificationDAO GlobalModificationDAO
    {
      get { return new GlobalModificationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMachineModificationDAO MachineModificationDAO
    {
      get { return new MachineModificationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMonitoredMachineDAO MonitoredMachineDAO
    {
      get { return new MonitoredMachineDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IMonitoredMachineAnalysisStatusDAO MonitoredMachineAnalysisStatusDAO
    {
      get { return new MonitoredMachineAnalysisStatusDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public INamedConfigDAO NamedConfigDAO
    {
      get { return new NamedConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public INonConformanceReasonDAO NonConformanceReasonDAO
    {
      get { return new NonConformanceReasonDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public INonConformanceReportDAO NonConformanceReportDAO
    {
      get { return new NonConformanceReportDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IObservationStateSlotDAO ObservationStateSlotDAO
    {
      get { return new ObservationStateSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationDAO OperationDAO
    {
      get { return new OperationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationCycleDAO OperationCycleDAO
    {
      get { return new OperationCycleDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationModelDAO OperationModelDAO => new OperationModelDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationRevisionDAO OperationRevisionDAO => new OperationRevisionDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationInformationDAO OperationInformationDAO
    {
      get { return new OperationInformationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationMachineAssociationDAO OperationMachineAssociationDAO
    {
      get { return new OperationMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationSlotDAO OperationSlotDAO
    {
      get { return new OperationSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationSlotSplitDAO OperationSlotSplitDAO
    {
      get { return new OperationSlotSplitDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationTypeDAO OperationTypeDAO
    {
      get { return new OperationTypeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IOperationCycleDeliverablePieceDAO OperationCycleDeliverablePieceDAO
    {
      get { return new OperationCycleDeliverablePieceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IPackageDAO PackageDAO
    {
      get { return new PackageDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IPackagePluginAssociationDAO PackagePluginAssociationDAO
    {
      get { return new PackagePluginAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IPartDAO PartDAO
    {
      get { return new PartDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IPathDAO PathDAO
    {
      get { return new OpPathDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IPluginDAO PluginDAO
    {
      get { return new PluginDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProcessMachineStateTemplateDAO ProcessMachineStateTemplateDAO
    {
      get { return new ProcessMachineStateTemplateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProductionAnalysisStatusDAO ProductionAnalysisStatusDAO
    {
      get { return new ProductionAnalysisStatusDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProductionInformationDAO ProductionInformationDAO
    {
      get { return new ProductionInformationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProductionInformationShiftDAO ProductionInformationShiftDAO
    {
      get { return new ProductionInformationShiftDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProjectComponentUpdateDAO ProjectComponentUpdateDAO
    {
      get { return new ProjectComponentUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProjectDAO ProjectDAO
    {
      get { return new ProjectDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IProductionRateSummaryDAO ProductionRateSummaryDAO => new ProductionRateSummaryDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IProductionStateDAO ProductionStateDAO
    {
      get { return new ProductionStateDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IProductionStateSummaryDAO ProductionStateSummaryDAO => new ProductionStateSummaryDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonSlotDAO ReasonSlotDAO
    {
      get { return new ReasonSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonSummaryDAO ReasonSummaryDAO
    {
      get { return new ReasonSummaryDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonDAO ReasonDAO
    {
      get { return new ReasonDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonMachineAssociationDAO ReasonMachineAssociationDAO
    {
      get { return new ReasonMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IReasonProposalDAO ReasonProposalDAO
    {
      get { return new ReasonProposalDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonGroupDAO ReasonGroupDAO
    {
      get { return new ReasonGroupDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IRefreshTokenDAO RefreshTokenDAO => new RefreshTokenDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementationN
    /// </summary>
    public IRevisionDAO RevisionDAO
    {
      get { return new RevisionDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IReasonSelectionDAO ReasonSelectionDAO
    {
      get { return new ReasonSelectionDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IRightDAO RightDAO
    {
      get { return new RightDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IRoleDAO RoleDAO
    {
      get { return new RoleDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ISequenceDAO SequenceDAO
    {
      get { return new OpSequenceDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public ISequenceMilestoneDAO SequenceMilestoneDAO
    {
      get { return new SequenceMilestoneDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ISequenceSlotDAO SequenceSlotDAO
    {
      get { return new SequenceSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ISerialNumberMachineStampDAO SerialNumberMachineStampDAO
    {
      get { return new SerialNumberMachineStampDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ISerialNumberModificationDAO SerialNumberModificationDAO
    {
      get { return new SerialNumberModificationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IServiceDAO ServiceDAO
    {
      get { return new ServiceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IShiftDAO ShiftDAO
    {
      get { return new ShiftDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IShiftSlotDAO ShiftSlotDAO
    {
      get { return new ShiftSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IShiftTemplateDAO ShiftTemplateDAO
    {
      get { return new ShiftTemplateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IShiftTemplateAssociationDAO ShiftTemplateAssociationDAO
    {
      get { return new ShiftTemplateAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IShiftTemplateSlotDAO ShiftTemplateSlotDAO
    {
      get { return new ShiftTemplateSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ISimpleOperationDAO SimpleOperationDAO
    {
      get { return new SimpleOperationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IStampDAO StampDAO
    {
      get { return new StampDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IStampingConfigByNameDAO StampingConfigByNameDAO => new StampingConfigByNameDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IStampingValueDAO StampingValueDAO
    {
      get { return new StampingValueDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IManufacturingOrderDAO ManufacturingOrderDAO
    {
      get { return new ManufacturingOrderDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IManufacturingOrderMachineAssociationDAO ManufacturingOrderMachineAssociationDAO
    {
      get { return new ManufacturingOrderMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ITimeConfigDAO TimeConfigDAO
    {
      get { return new TimeConfigDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IToolDAO ToolDAO
    {
      get { return new ToolDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IToolLifeDAO ToolLifeDAO
    {
      get { return new ToolLifeDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IToolPositionDAO ToolPositionDAO
    {
      get { return new ToolPositionDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public ITranslationDAO TranslationDAO
    {
      get { return new TranslationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUnitDAO UnitDAO
    {
      get { return new UnitDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserDAO UserDAO
    {
      get { return new UserDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserAttendanceDAO UserAttendanceDAO
    {
      get { return new UserAttendanceDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserMachineAssociationDAO UserMachineAssociationDAO
    {
      get { return new UserMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserMachineSlotDAO UserMachineSlotDAO
    {
      get { return new UserMachineSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserShiftAssociationDAO UserShiftAssociationDAO
    {
      get { return new UserShiftAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserShiftSlotDAO UserShiftSlotDAO
    {
      get { return new UserShiftSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IUserSlotDAO UserSlotDAO
    {
      get { return new UserSlotDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderDAO WorkOrderDAO
    {
      get { return new WorkOrderDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory"/>
    /// </summary>
    public IWorkOrderProjectDAO WorkOrderProjectDAO => new WorkOrderProjectDAO ();

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderLineDAO WorkOrderLineDAO
    {
      get { return new WorkOrderLineDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderLineAssociationDAO WorkOrderLineAssociationDAO
    {
      get { return new WorkOrderLineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderMachineAssociationDAO WorkOrderMachineAssociationDAO
    {
      get { return new WorkOrderMachineAssociationDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderMachineStampDAO WorkOrderMachineStampDAO
    {
      get { return new WorkOrderMachineStampDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderStatusDAO WorkOrderStatusDAO
    {
      get { return new WorkOrderStatusDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderProjectUpdateDAO WorkOrderProjectUpdateDAO
    {
      get { return new WorkOrderProjectUpdateDAO (); }
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IDAOFactory">IDAOFactory</see> implementation
    /// </summary>
    public IWorkOrderToOperationOnlySlotDAO WorkOrderToOperationOnlySlotDAO
    {
      get { return new WorkOrderToOperationOnlySlotDAO (); }
    }

    /// <summary>
    /// <see cref="IDAOFactory" />
    /// </summary>
    public IScrapReportDAO ScrapReportDAO => new ScrapReportDAO ();
  }
}
