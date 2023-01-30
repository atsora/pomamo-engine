// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO Factory interface
  /// </summary>
  public interface IDAOFactory : IDatabaseConnection, IDatabaseConnectionStatus
  {
    /// <summary>
    /// Get the connection status
    /// </summary>
    /// <returns></returns>
    ConnectionStatus Status { get; }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="o"></param>
    void Initialize (object o);

    /// <summary>
    /// Is a session active ?
    /// </summary>
    /// <returns></returns>
    bool IsSessionActive ();

    /// <summary>
    /// Check a very basic connection
    /// 
    /// Should be fast, even if the full connection is not initialized
    /// </summary>
    /// <exception cref="System.Exception">Connection problem</exception>
    void CheckBasicConnection ();

    /// <summary>
    /// Push a message that will be sent after the commit
    /// </summary>
    /// <param name="message"></param>
    void PushMessage (string message);

    /// <summary>
    /// Flush the data
    /// </summary>
    void Flush ();

    /// <summary>
    /// Evict a data
    /// </summary>
    /// <param name="o"></param>
    void Evict (object o);

    /// <summary>
    /// Get a IAcquisitionStateDAO
    /// </summary>
    IAcquisitionStateDAO AcquisitionStateDAO { get; }

    /// <summary>
    /// Get a IActivityManualDAO
    /// </summary>
    IActivityManualDAO ActivityManualDAO { get; }

    /// <summary>
    /// Get a ICncAlarmSeverityDAO
    /// </summary>
    ICncAlarmSeverityDAO CncAlarmSeverityDAO { get; }

    /// <summary>
    /// Get a ICncAlarmSeverityPatternDAO
    /// </summary>
    ICncAlarmSeverityPatternDAO CncAlarmSeverityPatternDAO { get; }

    /// <summary>
    /// Get a ICncAlarmDAO
    /// </summary>
    ICncAlarmDAO CncAlarmDAO { get; }

    /// <summary>
    /// Get a ICurrentCncAlarmDAO
    /// </summary>
    ICurrentCncAlarmDAO CurrentCncAlarmDAO { get; }

    /// <summary>
    /// Get a IAnalysisLogDAO
    /// </summary>
    IAnalysisLogDAO AnalysisLogDAO { get; }

    /// <summary>
    /// Get a IGlobalModificationLogDAO
    /// </summary>
    IGlobalModificationLogDAO GlobalModificationLogDAO { get; }

    /// <summary>
    /// Get a IMachineModificationLogDAO
    /// </summary>
    IMachineModificationLogDAO MachineModificationLogDAO { get; }

    /// <summary>
    /// Get a IApplicationStateDAO
    /// </summary>
    IApplicationStateDAO ApplicationStateDAO { get; }

    /// <summary>
    /// Get a IAutoReasonStateDAO
    /// </summary>
    IAutoReasonStateDAO AutoReasonStateDAO { get; }

    /// <summary>
    /// Get a IAutoMachineStateTemplateDAO
    /// </summary>
    IAutoMachineStateTemplateDAO AutoMachineStateTemplateDAO { get; }

    /// <summary>
    /// Get a IAutoSequenceDAO
    /// </summary>
    IAutoSequenceDAO AutoSequenceDAO { get; }

    /// <summary>
    /// Get a IBetweenCyclesDAO
    /// </summary>
    IBetweenCyclesDAO BetweenCyclesDAO { get; }

    /// <summary>
    /// Get a ICadModeleDAO
    /// </summary>
    ICadModelDAO CadModelDAO { get; }

    /// <summary>
    /// Get a ICellDAO
    /// </summary>
    ICellDAO CellDAO { get; }

    /// <summary>
    /// Get a ICncAcquisitionDAO
    /// </summary>
    ICncAcquisitionDAO CncAcquisitionDAO { get; }

    /// <summary>
    /// Get a ICncDataImportLogDAO
    /// </summary>
    ICncDataImportLogDAO CncDataImportLogDAO { get; }

    /// <summary>
    /// Get a ICncValueDAO
    /// </summary>
    ICncValueDAO CncValueDAO { get; }

    /// <summary>
    /// Get a ICncVariableDAO
    /// </summary>
    ICncVariableDAO CncVariableDAO { get; }

    /// <summary>
    /// Get a ICompanyDAO
    /// </summary>
    ICompanyDAO CompanyDAO { get; }

    /// <summary>
    /// Get a IComponentDAO
    /// </summary>
    IComponentDAO ComponentDAO { get; }

    /// <summary>
    /// Get a IComponentIntermediateWorkPieceDAO
    /// </summary>
    IComponentIntermediateWorkPieceDAO ComponentIntermediateWorkPieceDAO { get; }

    /// <summary>
    /// Get a IComponentIntermediateWorkPieceUpdateDAO
    /// </summary>
    IComponentIntermediateWorkPieceUpdateDAO ComponentIntermediateWorkPieceUpdateDAO { get; }

    /// <summary>
    /// Get a IComponentMachineAssociationDAO
    /// </summary>
    IComponentMachineAssociationDAO ComponentMachineAssociationDAO { get; }

    /// <summary>
    /// Get a IComponentTypeDAO
    /// </summary>
    IComponentTypeDAO ComponentTypeDAO { get; }

    /// <summary>
    /// Get a IComputerDAO
    /// </summary>
    IComputerDAO ComputerDAO { get; }

    /// <summary>
    /// Get a IConfigDAO
    /// </summary>
    IConfigDAO ConfigDAO { get; }

    /// <summary>
    /// Get a ICurrentCncValueModeDAO
    /// </summary>
    ICurrentCncValueDAO CurrentCncValueDAO { get; }

    /// <summary>
    /// Get a ICurrentMachineModeDAO
    /// </summary>
    ICurrentMachineModeDAO CurrentMachineModeDAO { get; }

    /// <summary>
    /// Get a ICustomerDAO
    /// </summary>
    ICustomerDAO CustomerDAO { get; }

    /// <summary>
    /// Get a IDaySlotDAO
    /// </summary>
    IDaySlotDAO DaySlotDAO { get; }

    /// <summary>
    /// Get a IDayTemplateChangeDAO
    /// </summary>
    IDayTemplateChangeDAO DayTemplateChangeDAO { get; }

    /// <summary>
    /// Get a IDayTemplateDAO
    /// </summary>
    IDayTemplateDAO DayTemplateDAO { get; }

    /// <summary>
    /// Get a IDayTemplateSlotDAO
    /// </summary>
    IDayTemplateSlotDAO DayTemplateSlotDAO { get; }

    /// <summary>
    /// Get a IDeliverablePieceDAO
    /// </summary>
    IDeliverablePieceDAO DeliverablePieceDAO { get; }

    /// <summary>
    /// Get a IDepartmentDAO
    /// </summary>
    IDepartmentDAO DepartmentDAO { get; }

    /// <summary>
    /// Get a IDetectionAnalysisLogDAO
    /// </summary>
    IDetectionAnalysisLogDAO DetectionAnalysisLogDAO { get; }

    /// <summary>
    /// Get a IDisplayDAO
    /// </summary>
    IDisplayDAO DisplayDAO { get; }

    /// <summary>
    /// Get a IEmailConfigDAO
    /// </summary>
    IEmailConfigDAO EmailConfigDAO { get; }

    /// <summary>
    /// Get a IEventDAO
    /// </summary>
    IEventDAO EventDAO { get; }

    /// <summary>
    /// Get a IEventLevelDAO
    /// </summary>
    IEventLevelDAO EventLevelDAO { get; }

    /// <summary>
    /// Get a IEventCncValueDAO
    /// </summary>
    IEventCncValueDAO EventCncValueDAO { get; }

    /// <summary>
    /// Get a IEventCncValueConfigDAO
    /// </summary>
    IEventCncValueConfigDAO EventCncValueConfigDAO { get; }

    /// <summary>
    /// Get a IEventLongPeriodDAO
    /// </summary>
    IEventLongPeriodDAO EventLongPeriodDAO { get; }

    /// <summary>
    /// Get a IEventLongPeriodConfigDAO
    /// </summary>
    IEventLongPeriodConfigDAO EventLongPeriodConfigDAO { get; }

    /// <summary>
    /// Get a IEventMessageDAO
    /// </summary>
    IEventMessageDAO EventMessageDAO { get; }

    /// <summary>
    /// Get a IEventMachineMessageDAO
    /// </summary>
    IEventMachineMessageDAO EventMachineMessageDAO { get; }

    /// <summary>
    /// Get a IEventToolLifeDAO
    /// </summary>
    IEventToolLifeDAO EventToolLifeDAO { get; }

    /// <summary>
    /// Get a IEventToolLifeConfigDAO
    /// </summary>
    IEventToolLifeConfigDAO EventToolLifeConfigDAO { get; }

    /// <summary>
    /// Get a IFactDAO
    /// </summary>
    IFactDAO FactDAO { get; }

    /// <summary>
    /// Get a IFieldDAO
    /// </summary>
    IFieldDAO FieldDAO { get; }

    /// <summary>
    /// Get a IFieldLegendDAO
    /// </summary>
    IFieldLegendDAO FieldLegendDAO { get; }

    /// <summary>
    /// Get a IGoalDAO
    /// </summary>
    IGoalDAO GoalDAO { get; }

    /// <summary>
    /// Get a IGoalTypeDAO
    /// </summary>
    IGoalTypeDAO GoalTypeDAO { get; }

    /// <summary>
    /// Get a IsoFileDAO
    /// </summary>
    IIsoFileDAO IsoFileDAO { get; }

    /// <summary>
    /// Get an IIsoFileSlot
    /// </summary>
    IIsoFileSlotDAO IsoFileSlotDAO { get; }

    /// <summary>
    /// Get a IIntermediateWorkPieceDAO
    /// </summary>
    IIntermediateWorkPieceDAO IntermediateWorkPieceDAO { get; }

    /// <summary>
    /// Get a IIntermediateWorkPieceOperationUpdateDAO
    /// </summary>
    IIntermediateWorkPieceOperationUpdateDAO IntermediateWorkPieceOperationUpdateDAO { get; }

    /// <summary>
    /// Get a IIntermediateWorkPieceTargetDAO
    /// </summary>
    IIntermediateWorkPieceTargetDAO IntermediateWorkPieceTargetDAO { get; }

    /// <summary>
    /// Get a IJobDAO
    /// </summary>
    IJobDAO JobDAO { get; }

    /// <summary>
    /// Get a LineDao
    /// </summary>
    ILineDAO LineDAO { get; }

    /// <summary>
    /// Get a LineMachineDao
    /// </summary>
    ILineMachineDAO LineMachineDAO { get; }

    /// <summary>
    /// Get a ILinkOperationDAO
    /// </summary>
    ILinkOperationDAO LinkOperationDAO { get; }

    /// <summary>
    /// Get a ILogDAO
    /// </summary>
    ILogDAO LogDAO { get; }

    /// <summary>
    /// Get a IMachineDAO
    /// </summary>
    IMachineDAO MachineDAO { get; }

    /// <summary>
    /// Get a IMachineActivitySummaryDAO
    /// </summary>
    IMachineActivitySummaryDAO MachineActivitySummaryDAO { get; }

    /// <summary>
    /// Get a IMachineCategoryDAO
    /// </summary>
    IMachineCategoryDAO MachineCategoryDAO { get; }

    /// <summary>
    /// Get a IMachineCellUpdateDAO
    /// </summary>
    IMachineCellUpdateDAO MachineCellUpdateDAO { get; }

    /// <summary>
    /// Get a IMachineCncVariableDAO
    /// </summary>
    IMachineCncVariableDAO MachineCncVariableDAO { get; }

    /// <summary>
    /// Get a IMachineCompanyUpdateDAO
    /// </summary>
    IMachineCompanyUpdateDAO MachineCompanyUpdateDAO { get; }

    /// <summary>
    /// Get a IMachineDepartmentUpdateDAO
    /// </summary>
    IMachineDepartmentUpdateDAO MachineDepartmentUpdateDAO { get; }

    /// <summary>
    /// Get a IMachineFilterDAO
    /// </summary>
    IMachineFilterDAO MachineFilterDAO { get; }

    /// <summary>
    /// Get a IMachineModeDAO
    /// </summary>
    IMachineModeDAO MachineModeDAO { get; }

    /// <summary>
    /// Get a IMachineModeDefaultReasonDAO
    /// </summary>
    IMachineModeDefaultReasonDAO MachineModeDefaultReasonDAO { get; }

    /// <summary>
    /// Get a IMachineModuleDAO
    /// </summary>
    IMachineModuleDAO MachineModuleDAO { get; }

    /// <summary>
    /// Get a IMachineModuleActivityDAO
    /// </summary>
    IMachineModuleActivityDAO MachineModuleActivityDAO { get; }

    /// <summary>
    /// Get a IMachineModuleAnalysisStatusDAO
    /// </summary>
    IMachineModuleAnalysisStatusDAO MachineModuleAnalysisStatusDAO { get; }

    /// <summary>
    /// Get a IMachineModuleDetectionDAO
    /// </summary>
    IMachineModuleDetectionDAO MachineModuleDetectionDAO { get; }

    /// <summary>
    /// Get a IMachineMonitoringTypeDAO
    /// </summary>
    IMachineMonitoringTypeDAO MachineMonitoringTypeDAO { get; }

    /// <summary>
    /// Get a IMachineStateTemplateDAO
    /// </summary>
    IMachineStateTemplateDAO MachineStateTemplateDAO { get; }

    /// <summary>
    /// Get a IMachineStateTemplateAssociationDAO
    /// </summary>
    IMachineStateTemplateAssociationDAO MachineStateTemplateAssociationDAO { get; }

    /// <summary>
    /// Get a IMachineStateTemplateFlowDAO
    /// </summary>
    IMachineStateTemplateFlowDAO MachineStateTemplateFlowDAO { get; }

    /// <summary>
    /// Get a IMachineStateTemplateRightDAO
    /// </summary>
    IMachineStateTemplateRightDAO MachineStateTemplateRightDAO { get; }

    /// <summary>
    /// Get a IMachineStateTemplateSlotDAO
    /// </summary>
    IMachineStateTemplateSlotDAO MachineStateTemplateSlotDAO { get; }

    /// <summary>
    /// Get a IMachineStatusDAO
    /// </summary>
    IMachineStatusDAO MachineStatusDAO { get; }

    /// <summary>
    /// Get a IMachineSubCategoryDAO
    /// </summary>
    IMachineSubCategoryDAO MachineSubCategoryDAO { get; }

    /// <summary>
    /// Get a IMachineObservationStateDAO
    /// </summary>
    IMachineObservationStateDAO MachineObservationStateDAO { get; }

    /// <summary>
    /// Get a IMachineObservationStateDAO
    /// </summary>
    IMachineObservationStateAssociationDAO MachineObservationStateAssociationDAO { get; }

    /// <summary>
    /// Get a IModificationDAO
    /// </summary>
    IModificationDAO ModificationDAO { get; }

    /// <summary>
    /// Get a IGlobalModificationDAO
    /// </summary>
    IGlobalModificationDAO GlobalModificationDAO { get; }

    /// <summary>
    /// Get a IMachineModificationDAO
    /// </summary>
    IMachineModificationDAO MachineModificationDAO { get; }

    /// <summary>
    /// Get a IMonitoredMachineDAO
    /// </summary>
    IMonitoredMachineDAO MonitoredMachineDAO { get; }

    /// <summary>
    /// Get a IMonitoredMachineAnalysisStatusDAO
    /// </summary>
    IMonitoredMachineAnalysisStatusDAO MonitoredMachineAnalysisStatusDAO { get; }

    /// <summary>
    /// Get a INamedConfigDAO
    /// </summary>
    INamedConfigDAO NamedConfigDAO { get; }

    /// <summary>
    /// Get a INonCorformanceReasonDAO
    /// </summary>
    INonConformanceReasonDAO NonConformanceReasonDAO { get; }

    /// <summary>
    /// Get a INonCorformanceReportDAO
    /// </summary>
    INonConformanceReportDAO NonConformanceReportDAO { get; }

    /// <summary>
    /// Get a IObservationStateSlotDAO
    /// </summary>
    IObservationStateSlotDAO ObservationStateSlotDAO { get; }

    /// <summary>
    /// Get a IOperationDAO
    /// </summary>
    IOperationDAO OperationDAO { get; }

    /// <summary>
    /// Get a IOperationCycleDAO
    /// </summary>
    IOperationCycleDAO OperationCycleDAO { get; }

    /// <summary>
    /// Get a IOperationModelDAO
    /// </summary>
    IOperationModelDAO OperationModelDAO { get; }

    /// <summary>
    /// Get a IOperationRevisionDAO
    /// </summary>
    IOperationRevisionDAO OperationRevisionDAO { get; }

    /// <summary>
    /// Get a IOperationInformationDAO
    /// </summary>
    IOperationInformationDAO OperationInformationDAO { get; }

    /// <summary>
    /// Get a IOperationMachineAssociationDAO
    /// </summary>
    IOperationMachineAssociationDAO OperationMachineAssociationDAO { get; }

    /// <summary>
    /// Get a IOperationSlotDAO
    /// </summary>
    IOperationSlotDAO OperationSlotDAO { get; }

    /// <summary>
    /// Get a IOperationSlotSplitDAO
    /// </summary>
    IOperationSlotSplitDAO OperationSlotSplitDAO { get; }

    /// <summary>
    /// Get a IOperationTypeDAO
    /// </summary>
    IOperationTypeDAO OperationTypeDAO { get; }

    /// <summary>
    /// Get a IOperationCycleDeliverablePieceDAO
    /// </summary>
    IOperationCycleDeliverablePieceDAO OperationCycleDeliverablePieceDAO { get; }

    /// <summary>
    /// Get a IPackageDAO
    /// </summary>
    IPackageDAO PackageDAO { get; }

    /// <summary>
    /// Get a IPackagePluginAssociation
    /// </summary>
    IPackagePluginAssociationDAO PackagePluginAssociationDAO { get; }

    /// <summary>
    /// Get a IPartDAO
    /// </summary>
    IPartDAO PartDAO { get; }

    /// <summary>
    /// Get a IPathDAO
    /// </summary>
    IPathDAO PathDAO { get; }

    /// <summary>
    /// Get a IPluginDAO
    /// </summary>
    IPluginDAO PluginDAO { get; }

    /// <summary>
    /// Get a IProcessMachineStateTemplateDAO
    /// </summary>
    IProcessMachineStateTemplateDAO ProcessMachineStateTemplateDAO { get; }

    /// <summary>
    /// Get a IProductionAnalysisStatusDAO
    /// </summary>
    IProductionAnalysisStatusDAO ProductionAnalysisStatusDAO { get; }

    /// <summary>
    /// Get a IProductionInformationDAO
    /// </summary>
    IProductionInformationDAO ProductionInformationDAO { get; }

    /// <summary>
    /// Get a IProductionInformationShiftDAO
    /// </summary>
    IProductionInformationShiftDAO ProductionInformationShiftDAO { get; }

    /// <summary>
    /// Get a IProductionStateDAO
    /// </summary>
    IProductionStateDAO ProductionStateDAO { get; }

    /// <summary>
    /// Get a IProductionRateSummaryDAO
    /// </summary>
    IProductionRateSummaryDAO ProductionRateSummaryDAO { get; }

    /// <summary>
    /// Get a IProductionStateSummaryDAO
    /// </summary>
    IProductionStateSummaryDAO ProductionStateSummaryDAO { get; }

    /// <summary>
    /// Get a IProjectComponentUpdateDAO
    /// </summary>
    IProjectComponentUpdateDAO ProjectComponentUpdateDAO { get; }

    /// <summary>
    /// Get a IProjectDAO
    /// </summary>
    IProjectDAO ProjectDAO { get; }

    /// <summary>
    /// Get a IReasonDAO
    /// </summary>
    IReasonDAO ReasonDAO { get; }

    /// <summary>
    /// Get a IReasonGroupDAO
    /// </summary>
    IReasonGroupDAO ReasonGroupDAO { get; }

    /// <summary>
    /// Get a IReasonMachineAssociationDAO
    /// </summary>
    IReasonMachineAssociationDAO ReasonMachineAssociationDAO { get; }

    /// <summary>
    /// Get a IReasonProposalDAO
    /// </summary>
    IReasonProposalDAO ReasonProposalDAO { get; }

    /// <summary>
    /// Get a IReasonSelectionDAO
    /// </summary>
    IReasonSelectionDAO ReasonSelectionDAO { get; }

    /// <summary>
    /// Get a IReasonSlotDAO
    /// </summary>
    IReasonSlotDAO ReasonSlotDAO { get; }

    /// <summary>
    /// Get a IReasonSummaryDAO
    /// </summary>
    IReasonSummaryDAO ReasonSummaryDAO { get; }

    /// <summary>
    /// Get a IRefreshTokenDAO
    /// </summary>
    IRefreshTokenDAO RefreshTokenDAO { get; }

    /// <summary>
    /// Get a IRevisionDAO
    /// </summary>
    IRevisionDAO RevisionDAO { get; }

    /// <summary>
    /// Get a IRightDAO
    /// </summary>
    IRightDAO RightDAO { get; }

    /// <summary>
    /// Get a IRoleDAO
    /// </summary>
    IRoleDAO RoleDAO { get; }

    /// <summary>
    /// Get a ISequenceDAO
    /// </summary>
    ISequenceDAO SequenceDAO { get; }

    /// <summary>
    /// Get a ISequenceMilestoneDAO
    /// </summary>
    ISequenceMilestoneDAO SequenceMilestoneDAO { get; }

    /// <summary>
    /// Get a ISequenceSlotDAO
    /// </summary>
    ISequenceSlotDAO SequenceSlotDAO { get; }

    /// <summary>
    /// Get a ISerialNumberMachineStampDAO
    /// </summary>
    ISerialNumberMachineStampDAO SerialNumberMachineStampDAO { get; }

    /// <summary>
    /// Get a ISerialNumberModificationDAO
    /// </summary>
    ISerialNumberModificationDAO SerialNumberModificationDAO { get; }

    /// <summary>
    /// Get a IServiceDAO
    /// </summary>
    IServiceDAO ServiceDAO { get; }

    /// <summary>
    /// Get a IShiftDAO
    /// </summary>
    IShiftDAO ShiftDAO { get; }

    /// <summary>
    /// Get a IShiftSlotDAO
    /// </summary>
    IShiftSlotDAO ShiftSlotDAO { get; }

    /// <summary>
    /// Get a IShiftTemplateDAO
    /// </summary>
    IShiftTemplateDAO ShiftTemplateDAO { get; }

    /// <summary>
    /// Get a IShiftTemplateAssociationDAO
    /// </summary>
    IShiftTemplateAssociationDAO ShiftTemplateAssociationDAO { get; }

    /// <summary>
    /// Get a IShiftTemplateSlotDAO
    /// </summary>
    IShiftTemplateSlotDAO ShiftTemplateSlotDAO { get; }

    /// <summary>
    /// Get a ISimpleOperationDAO
    /// </summary>
    ISimpleOperationDAO SimpleOperationDAO { get; }

    /// <summary>
    /// Get a IStampDAO
    /// </summary>
    IStampDAO StampDAO { get; }

    /// <summary>
    /// Get a IStampDAO
    /// </summary>
    IStampingConfigByNameDAO StampingConfigByNameDAO { get; }

    /// <summary>
    /// Get a IStampingValueDAO
    /// </summary>
    IStampingValueDAO StampingValueDAO { get; }

    /// <summary>
    /// Get a ITaskDAO
    /// </summary>
    ITaskDAO TaskDAO { get; }

    /// <summary>
    /// Get a ITaskMachineAssociationDAO
    /// </summary>
    ITaskMachineAssociationDAO TaskMachineAssociationDAO { get; }

    /// <summary>
    /// Get a ITimeConfigDAO
    /// </summary>
    ITimeConfigDAO TimeConfigDAO { get; }

    /// <summary>
    /// Get a IToolDAO
    /// </summary>
    IToolDAO ToolDAO { get; }

    /// <summary>
    /// Get a ToolLifeDAO
    /// </summary>
    IToolLifeDAO ToolLifeDAO { get; }

    /// <summary>
    /// Get a ToolPositionDAO
    /// </summary>
    IToolPositionDAO ToolPositionDAO { get; }

    /// <summary>
    /// Get a ITranslationDAO
    /// </summary>
    ITranslationDAO TranslationDAO { get; }

    /// <summary>
    /// Get a IUnitDAO
    /// </summary>
    IUnitDAO UnitDAO { get; }

    /// <summary>
    /// Get a IUserDAO
    /// </summary>
    IUserDAO UserDAO { get; }

    /// <summary>
    /// Get a IUserAttendanceDAO
    /// </summary>
    IUserAttendanceDAO UserAttendanceDAO { get; }

    /// <summary>
    /// Get a IUserMachineAssociationDAO
    /// </summary>
    IUserMachineAssociationDAO UserMachineAssociationDAO { get; }

    /// <summary>
    /// Get a IUserMachineSlotDAO
    /// </summary>
    IUserMachineSlotDAO UserMachineSlotDAO { get; }

    /// <summary>
    /// Get a IUserShiftAssociationDAO
    /// </summary>
    IUserShiftAssociationDAO UserShiftAssociationDAO { get; }

    /// <summary>
    /// Get a IUserShiftSlotDAO
    /// </summary>
    IUserShiftSlotDAO UserShiftSlotDAO { get; }

    /// <summary>
    /// Get a IUserSlotDAO
    /// </summary>
    IUserSlotDAO UserSlotDAO { get; }

    /// <summary>
    /// Get a IWorkOrderDAO
    /// </summary>
    IWorkOrderDAO WorkOrderDAO { get; }

    /// <summary>
    /// Get a IWorkOrderLineDAO
    /// </summary>
    IWorkOrderLineDAO WorkOrderLineDAO { get; }

    /// <summary>
    /// Get a IWorkOrderLineAssociationDAO
    /// </summary>
    IWorkOrderLineAssociationDAO WorkOrderLineAssociationDAO { get; }

    /// <summary>
    /// Get a IWorkOrderMachineAssociationDAO
    /// </summary>
    IWorkOrderMachineAssociationDAO WorkOrderMachineAssociationDAO { get; }

    /// <summary>
    /// Get a IWorkOrderMachineStampDAO
    /// </summary>
    IWorkOrderMachineStampDAO WorkOrderMachineStampDAO { get; }

    /// <summary>
    /// Get a IWorkOrderProjectDAO
    /// </summary>
    IWorkOrderProjectDAO WorkOrderProjectDAO { get; }

    /// <summary>
    /// Get a IWorkOrderStatusDAO
    /// </summary>
    IWorkOrderStatusDAO WorkOrderStatusDAO { get; }

    /// <summary>
    /// Get a IWorkOrderProjectUpdateDAO
    /// </summary>
    IWorkOrderProjectUpdateDAO WorkOrderProjectUpdateDAO { get; }

    /// <summary>
    /// Get a IWorkOrderToOperationOnlySlotDAO
    /// </summary>
    IWorkOrderToOperationOnlySlotDAO WorkOrderToOperationOnlySlotDAO { get; }
  }
}
