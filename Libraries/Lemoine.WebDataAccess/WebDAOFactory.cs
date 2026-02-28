// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of WebDAOFactory.
  /// </summary>
  public class WebDAOFactory : IDAOFactory
  {
    #region IDatabaseConectionStatus
    /// <summary>
    /// <see cref="IDatabaseConnectionStatus"/>
    /// </summary>
    public bool IsDatabaseConnectionUp
    {
      get {
        try {
          CheckBasicConnection ();
          return true;
        }
        catch (Exception) {
          return false;
        }
      }
    }
    #endregion // IDatabaseConnectionStatus

    #region IDAOFactory implementation
    public void CheckBasicConnection ()
    {
      WebServiceHelper.Execute (new Lemoine.WebClient.RequestUrl ("/Test"));
    }

    public IDAOSession OpenSession ()
    {
      return new WebDAOSession ();
    }
    public System.Data.Common.DbConnection GetConnection ()
    {
      throw new NotImplementedException ();
    }
    public void Flush ()
    {
      throw new NotImplementedException ();
    }

    public void Evict (object o)
    { 
    }

    public bool IsSessionActive ()
    {
      throw new NotImplementedException ();
    }
    public bool IsTransactionActive ()
    {
      throw new NotImplementedException ();
    }
    /// <summary>
    /// Is the transaction read-only ?
    /// 
    /// In case of error, false is returned
    /// </summary>
    /// <returns></returns>
    public bool IsTransactionReadOnly ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Get the server version number, for example: 90501
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request fails or if the result is not a valid integer</exception>
    public int GetPostgreSQLVersionNum ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Check if the server version is greater than the specified version
    /// 
    /// If the version could not be retrieved, false is returned (no exception is returned)
    /// </summary>
    /// <param name="versionNumber"></param>
    /// <returns></returns>
    public bool IsPostgreSQLVersionGreaterOrEqual (int versionNumber)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Return the server version, for example: 9.05.01
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the SQL request fails</exception>
    public string GetPostgreSQLVersion ()
    {
      throw new NotImplementedException ();
    }

    public bool IsInitialized (object proxy)
    {
      throw new NotImplementedException ();
    }

    public void Initialize (object proxy)
    {
      throw new NotImplementedException ();
    }

    public int GetCurrentConnectionId ()
    {
      throw new NotImplementedException ();
    }

    public bool KillActiveConnection (int connectionId)
    {
      throw new NotImplementedException ();
    }

    public void PushMessage (string message)
    {
      throw new NotImplementedException ();
    }

    public void ExecuteNonQuery (string sql)
    {
      throw new NotImplementedException ();
    }

    public IAcquisitionStateDAO AcquisitionStateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IActivityManualDAO ActivityManualDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncAlarmDAO CncAlarmDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncAlarmSeverityDAO CncAlarmSeverityDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncAlarmSeverityPatternDAO CncAlarmSeverityPatternDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICurrentCncAlarmDAO CurrentCncAlarmDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncAlarmColorDAO CncAlarmColorDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IAnalysisLogDAO AnalysisLogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IGlobalModificationLogDAO GlobalModificationLogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModificationLogDAO MachineModificationLogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IApplicationStateDAO ApplicationStateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IAutoMachineStateTemplateDAO AutoMachineStateTemplateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IAutoSequenceDAO AutoSequenceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IBetweenCyclesDAO BetweenCyclesDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICadModelDAO CadModelDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICellDAO CellDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncAcquisitionDAO CncAcquisitionDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncValueDAO CncValueDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICncValueColorDAO CncValueColorDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICompanyDAO CompanyDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IComponentDAO ComponentDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IComponentIntermediateWorkPieceDAO ComponentIntermediateWorkPieceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IComponentIntermediateWorkPieceUpdateDAO ComponentIntermediateWorkPieceUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IComponentMachineAssociationDAO ComponentMachineAssociationDAO
    {
      get {
        return new ComponentMachineAssociationDAO ();
      }
    }
    public IComponentTypeDAO ComponentTypeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IComputerDAO ComputerDAO
    {
      get {
        return new ComputerDAO ();
      }
    }

    public IConfigDAO ConfigDAO => new ConfigDAO ();

    public ICurrentCncValueDAO CurrentCncValueDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ICurrentMachineModeDAO CurrentMachineModeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDaySlotDAO DaySlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDayTemplateChangeDAO DayTemplateChangeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDayTemplateDAO DayTemplateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDayTemplateSlotDAO DayTemplateSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDeliverablePieceDAO DeliverablePieceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDepartmentDAO DepartmentDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDetectionAnalysisLogDAO DetectionAnalysisLogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IDisplayDAO DisplayDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEmailConfigDAO EmailConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventDAO EventDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventLevelDAO EventLevelDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IEventMessageDAO EventMessageDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IEventMachineMessageDAO EventMachineMessageDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IEventCncValueDAO EventCncValueDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IEventCncValueConfigDAO EventCncValueConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventLongPeriodDAO EventLongPeriodDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventLongPeriodConfigDAO EventLongPeriodConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventToolLifeDAO EventToolLifeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IEventToolLifeConfigDAO EventToolLifeConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IFactDAO FactDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IFieldDAO FieldDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IFieldLegendDAO FieldLegendDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IGoalDAO GoalDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IGoalTypeDAO GoalTypeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IIsoFileDAO IsoFileDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IIsoFileSlotDAO IsoFileSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IIntermediateWorkPieceDAO IntermediateWorkPieceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IIntermediateWorkPieceOperationUpdateDAO IntermediateWorkPieceOperationUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IIntermediateWorkPieceTargetDAO IntermediateWorkPieceTargetDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IJobDAO JobDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ILineDAO LineDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ILineMachineDAO LineMachineDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ILinkOperationDAO LinkOperationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ILogDAO LogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineDAO MachineDAO
    {
      get {
        return new MachineDAO ();
      }
    }
    public IMachineActivitySummaryDAO MachineActivitySummaryDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineCategoryDAO MachineCategoryDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineCellUpdateDAO MachineCellUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineCompanyUpdateDAO MachineCompanyUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineDepartmentUpdateDAO MachineDepartmentUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineFilterDAO MachineFilterDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineMonitoringTypeDAO MachineMonitoringTypeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineStateTemplateDAO MachineStateTemplateDAO
    {
      get { return new MachineStateTemplateDAO (); }
    }
    public IMachineStateTemplateAssociationDAO MachineStateTemplateAssociationDAO
    {
      get { return new MachineStateTemplateAssociationDAO (); }
    }
    public IMachineStateTemplateFlowDAO MachineStateTemplateFlowDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineStateTemplateRightDAO MachineStateTemplateRightDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineStatusDAO MachineStatusDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineSubCategoryDAO MachineSubCategoryDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineObservationStateDAO MachineObservationStateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineObservationStateAssociationDAO MachineObservationStateAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModeDAO MachineModeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModeDefaultReasonDAO MachineModeDefaultReasonDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModeSlotDAO MachineModeSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IMachineModeColorSlotDAO MachineModeColorSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IMachineModuleDAO MachineModuleDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModuleActivityDAO MachineModuleActivityDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModuleAnalysisStatusDAO MachineModuleAnalysisStatusDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineModuleDetectionDAO MachineModuleDetectionDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMachineStateTemplateSlotDAO MachineStateTemplateSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IModificationDAO ModificationDAO
    {
      get {
        return new ModificationDAO ();
      }
    }
    public IGlobalModificationDAO GlobalModificationDAO
    {
      get {
        return new GlobalModificationDAO ();
      }
    }
    public IMachineModificationDAO MachineModificationDAO
    {
      get {
        return new MachineModificationDAO ();
      }
    }
    public IMonitoredMachineDAO MonitoredMachineDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IMonitoredMachineAnalysisStatusDAO MonitoredMachineAnalysisStatusDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public INamedConfigDAO NamedConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public INonConformanceReasonDAO NonConformanceReasonDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public INonConformanceReportDAO NonConformanceReportDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IObservationStateSlotDAO ObservationStateSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationDAO OperationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationCycleDAO OperationCycleDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationInformationDAO OperationInformationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationMachineAssociationDAO OperationMachineAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationSlotDAO OperationSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationSlotSplitDAO OperationSlotSplitDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationTypeDAO OperationTypeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IOperationCycleDeliverablePieceDAO OperationCycleDeliverablePieceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IPackageDAO PackageDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IPackagePluginAssociationDAO PackagePluginAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IPartDAO PartDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IPathDAO PathDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IPluginDAO PluginDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProcessMachineStateTemplateDAO ProcessMachineStateTemplateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProductionAnalysisStatusDAO ProductionAnalysisStatusDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProductionInformationDAO ProductionInformationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProductionInformationShiftDAO ProductionInformationShiftDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProductionStateDAO ProductionStateDAO => throw new NotImplementedException ();
    public IProjectComponentUpdateDAO ProjectComponentUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IProjectDAO ProjectDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonDAO ReasonDAO
    {
      get {
        return new ReasonDAO ();
      }
    }
    public IReasonGroupDAO ReasonGroupDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonMachineAssociationDAO ReasonMachineAssociationDAO
    {
      get {
        return new ReasonMachineAssociationDAO ();
      }
    }
    public IReasonSelectionDAO ReasonSelectionDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonSlotDAO ReasonSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonOnlySlotDAO ReasonOnlySlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonColorSlotDAO ReasonColorSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IReasonSelectionSlotDAO ReasonSelectionSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IReasonSummaryDAO ReasonSummaryDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IRevisionDAO RevisionDAO
    {
      get {
        return new RevisionDAO ();
      }
    }
    public IRightDAO RightDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IRoleDAO RoleDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IRunningSlotDAO RunningSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ISequenceDAO SequenceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ISequenceMilestoneDAO SequenceMilestoneDAO => throw new NotImplementedException ();
    public ISequenceSlotDAO SequenceSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ISerialNumberMachineStampDAO SerialNumberMachineStampDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ISerialNumberModificationDAO SerialNumberModificationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IServiceDAO ServiceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IShiftDAO ShiftDAO
    {
      get { return new ShiftDAO (); }
    }
    public IShiftSlotDAO ShiftSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IShiftTemplateDAO ShiftTemplateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IShiftTemplateAssociationDAO ShiftTemplateAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IShiftTemplateSlotDAO ShiftTemplateSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ISimpleOperationDAO SimpleOperationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IStampDAO StampDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IStampingConfigByNameDAO StampingConfigByNameDAO => throw new NotImplementedException ();

    public IStampingValueDAO StampingValueDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ITaskDAO TaskDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ITaskMachineAssociationDAO TaskMachineAssociationDAO
    {
      get {
        return new TaskMachineAssociationDAO ();
      }
    }
    public ITimeConfigDAO TimeConfigDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IToolDAO ToolDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IToolLifeDAO ToolLifeDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IToolPositionDAO ToolPositionDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public ITranslationDAO TranslationDAO
    {
      get {
        return new TranslationDAO ();
      }
    }
    public IUnitDAO UnitDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserDAO UserDAO
    {
      get {
        return new UserDAO ();
      }
    }
    public IUserAttendanceDAO UserAttendanceDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserMachineAssociationDAO UserMachineAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserMachineSlotDAO UserMachineSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserShiftAssociationDAO UserShiftAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserShiftSlotDAO UserShiftSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IUserSlotDAO UserSlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderDAO WorkOrderDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderLineDAO WorkOrderLineDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderLineAssociationDAO WorkOrderLineAssociationDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderMachineAssociationDAO WorkOrderMachineAssociationDAO
    {
      get { return new WorkOrderMachineAssociationDAO (); }
    }
    public IWorkOrderMachineStampDAO WorkOrderMachineStampDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderStatusDAO WorkOrderStatusDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderProjectUpdateDAO WorkOrderProjectUpdateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public IWorkOrderToOperationOnlySlotDAO WorkOrderToOperationOnlySlotDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public ICncVariableDAO CncVariableDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IMachineCncVariableDAO MachineCncVariableDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IAutoReasonStateDAO AutoReasonStateDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public ICncDataImportLogDAO CncDataImportLogDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public ConnectionStatus Status
    {
      get {
        return ConnectionStatus.Started;
      }
    }

    public IReasonProposalDAO ReasonProposalDAO
    {
      get {
        throw new NotImplementedException ();
      }
    }

    public IProductionRateSummaryDAO ProductionRateSummaryDAO => throw new NotImplementedException ();

    public IProductionStateSummaryDAO ProductionStateSummaryDAO => throw new NotImplementedException ();

    public IRefreshTokenDAO RefreshTokenDAO => throw new NotImplementedException ();

    public ICustomerDAO CustomerDAO => throw new NotImplementedException ();

    public IOperationModelDAO OperationModelDAO => throw new NotImplementedException ();

    public IOperationRevisionDAO OperationRevisionDAO => throw new NotImplementedException ();

    public IWorkOrderProjectDAO WorkOrderProjectDAO => throw new NotImplementedException ();

    #endregion
  }
}
