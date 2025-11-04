// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of WebModelFactory.
  /// </summary>
  public class WebModelFactory: IModelFactory
  {
    #region Members
    readonly Lemoine.ModelDAO.IDAOFactory m_daoFactory = new WebDAOFactory ();
    #endregion // Members
    
    #region Constructors
    public WebModelFactory (string webServiceUrl)
    {
      WebServiceHelper.Url = webServiceUrl;
    }
    #endregion // Constructors

    #region IModelFactory implementation

    public IAcquisitionState CreateAcquisitionState (IMachineModule machineModule, string key)
    {
      throw new NotImplementedException ();
    }

    public IAcquisitionState CreateAcquisitionState (IMachineModule machineModule, AcquisitionStateKey key)
    {
      throw new NotImplementedException ();
    }

    public IActivityManual CreateActivityManual(IMonitoredMachine machine, IMachineMode machineMode, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public ICncAlarm CreateCncAlarm(IMachineModule machineModule, UtcDateTimeRange range, string alarmCncInfo,
                                    string alarmCncSubInfo, string alarmType, string alarmNumber)
    {
      throw new NotImplementedException();
    }

    public ICncAlarmSeverity CreateCncAlarmSeverity(string cncInfo, string name)
    {
      throw new NotImplementedException();
    }

    public ICncAlarmSeverityPattern CreateCncAlarmSeverityPattern(string cncInfo,
                                                                  CncAlarmSeverityPatternRules pattern,
                                                                  ICncAlarmSeverity severity)
    {
      throw new NotImplementedException();
    }

    public ICurrentCncAlarm CreateCurrentCncAlarm(IMachineModule machineModule, DateTime datetime, string alarmCncInfo,
                                                  string alarmCncSubInfo, string alarmType, string alarmNumber)
    {
      throw new NotImplementedException();
    }

    public IGlobalModificationLog CreateGlobalModificationLog(LogLevel level, string message, IGlobalModification modification)
    {
      throw new NotImplementedException();
    }

    public IMachineModificationLog CreateMachineModificationLog(LogLevel level, string message, IMachineModification modification)
    {
      throw new NotImplementedException();
    }

    public IApplicationState CreateApplicationState(string key)
    {
      throw new NotImplementedException();
    }

    public IAutoMachineStateTemplate CreateAutoMachineStateTemplate(IMachineMode machineMode, IMachineStateTemplate newMachineStateTemplate)
    {
      throw new NotImplementedException();
    }

    public IAutoSequence CreateAutoSequence(IMachineModule machineModule, ISequence sequence, DateTime begin)
    {
      throw new NotImplementedException();
    }

    public IAutoSequence CreateAutoSequence(IMachineModule machineModule, IOperation operation, DateTime begin)
    {
      throw new NotImplementedException();
    }

    public IBetweenCycles CreateBetweenCycles(IOperationCycle previousCycle, IOperationCycle nextCycle)
    {
      throw new NotImplementedException();
    }

    public ICadModel CreateCadModel()
    {
      throw new NotImplementedException();
    }

    public ICell CreateCell()
    {
      throw new NotImplementedException();
    }

    public ICncAcquisition CreateCncAcquisition()
    {
      throw new NotImplementedException();
    }

    public ICncValue CreateCncValue(IMachineModule machineModule, IField field, DateTime beginDateTime)
    {
      throw new NotImplementedException();
    }

    public ICompany CreateCompany()
    {
      throw new NotImplementedException();
    }

    public IComponent CreateComponentFromName(IProject project, string name, IComponentType type)
    {
      throw new NotImplementedException();
    }

    public IComponent CreateComponentFromCode(IProject project, string code, IComponentType type)
    {
      throw new NotImplementedException();
    }

    public IComponent CreateComponentFromType(IProject project, IComponentType type)
    {
      throw new NotImplementedException();
    }

    public IComponentIntermediateWorkPiece CreateComponentIntermediateWorkPiece(IComponent component, IIntermediateWorkPiece intermediateWorkPiece)
    {
      throw new NotImplementedException();
    }

    public IComponentIntermediateWorkPieceUpdate CreateComponentIntermediateWorkPieceUpdate(IComponent component, IIntermediateWorkPiece intermediateworkpiece, ComponentIntermediateWorkPieceUpdateModificationType type)
    {
      throw new NotImplementedException();
    }

    public IComponentMachineAssociation CreateComponentMachineAssociation(IMachine machine, IComponent component, DateTime begin, UpperBound<DateTime> end)
    {
      throw new NotImplementedException();
    }

    public IComponentMachineAssociation CreateComponentMachineAssociation(IMachine machine, IComponent component, UtcDateTimeRange range)
    {
      return new ComponentMachineAssociation (machine, range);
    }

    public IComponentType CreateComponentTypeFromName(string name)
    {
      throw new NotImplementedException();
    }

    public IComponentType CreateComponentTypeFromTranslationKey(string translationKey)
    {
      throw new NotImplementedException();
    }

    public IComputer CreateComputer(string name, string address)
    {
      throw new NotImplementedException();
    }

    public IConfig CreateConfig(string key)
    {
      throw new NotImplementedException();
    }

    public IConfig CreateConfig(AnalysisConfigKey key)
    {
      throw new NotImplementedException();
    }

    public ICurrentCncValue CreateCurrentCncValue(IMachineModule machineModule, IField field)
    {
      throw new NotImplementedException();
    }

    public ICurrentMachineMode CreateCurrentMachineMode(IMonitoredMachine machine)
    {
      throw new NotImplementedException();
    }

    public IDayTemplate CreateDayTemplate()
    {
      throw new NotImplementedException();
    }

    public IDaySlot CreateDaySlot(IDayTemplate dayTemplate, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IDayTemplateChange CreateDayTemplateChange(IDayTemplate dayTemplate, DateTime beginDateTime)
    {
      throw new NotImplementedException();
    }

    public IDayTemplateSlot CreateDayTemplateSlot(IDayTemplate dayTemplate, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IDeliverablePiece CreateDeliverablePiece(string serialID)
    {
      throw new NotImplementedException();
    }

    public IDepartment CreateDepartment()
    {
      throw new NotImplementedException();
    }

    public IDetectionAnalysisLog CreateDetectionAnalysisLog(LogLevel level, string message, IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IDetectionAnalysisLog CreateDetectionAnalysisLog(LogLevel level, string message, IMachine machine, IMachineModule machineModule)
    {
      throw new NotImplementedException();
    }

    public IDisplay CreateDisplay(string table)
    {
      throw new NotImplementedException();
    }

    public IEmailConfig CreateEmailConfig()
    {
      throw new NotImplementedException();
    }

    public IEventLevel CreateEventLevelFromName (int priority, string name)
    {
      throw new NotImplementedException();
    }

    public IEventLevel CreateEventLevelFromTranslationKey (int priority, string translationKey)
    {
      throw new NotImplementedException ();
    }

    public IEventMessage CreateEventMessage(IEventLevel level, string message)
    {
      throw new NotImplementedException();
    }
    public IEventMachineMessage CreateEventMachineMessage(IEventLevel level, IMachine machine, string message)
    {
      throw new NotImplementedException();
    }
    public IEventCncValue CreateEventCncValue(IEventLevel level, DateTime dateTime, string message, IMachineModule machineModule, IField field, object v, TimeSpan duration, IEventCncValueConfig config)
    {
      throw new NotImplementedException();
    }

    public IEventCncValueConfig CreateEventCncValueConfig(string name, IField field, IEventLevel level, string message, string condition)
    {
      throw new NotImplementedException();
    }

    public IEventLongPeriod CreateEventLongPeriod(IEventLevel level, DateTime dateTime, IMonitoredMachine monitoredMachine, IMachineMode machineMode, IMachineObservationState machineObservationState, TimeSpan triggerDuration)
    {
      throw new NotImplementedException();
    }

    public IEventLongPeriodConfig CreateEventLongPeriodConfig(TimeSpan triggerDuration, IEventLevel level)
    {
      throw new NotImplementedException();
    }

    public IEventToolLife CreateEventToolLife(IEventLevel level, EventToolLifeType type, DateTime dateTime, IMachineModule machineModule)
    {
      throw new NotImplementedException();
    }

    public IEventToolLifeConfig CreateEventToolLifeConfig(EventToolLifeType eventToolLifeType, IEventLevel level)
    {
      throw new NotImplementedException();
    }

    public IFact CreateFact(IMonitoredMachine machine, DateTime utcBegin, DateTime utcEnd, IMachineMode machineMode)
    {
      throw new NotImplementedException();
    }

    public IField CreateFieldFromName(string code, string name)
    {
      throw new NotImplementedException();
    }

    public IField CreateFieldFromTranslationKey (string code, string translationKey)
    {
      throw new NotImplementedException ();
    }

    public IFieldLegend CreateFieldLegend(IField field, string text, string color)
    {
      throw new NotImplementedException();
    }

    public IGoal CreateGoal(IGoalType type)
    {
      throw new NotImplementedException();
    }

    public IGoalType CreateGoalType(string name)
    {
      throw new NotImplementedException();
    }

    public IIntermediateWorkPiece CreateIntermediateWorkPiece(IOperation operation)
    {
      throw new NotImplementedException();
    }

    public IIntermediateWorkPieceOperationUpdate CreateIntermediateWorkPieceOperationUpdate(IIntermediateWorkPiece intermediateworkpiece, IOperation oldOperation, IOperation newOperation)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    public IIntermediateWorkPieceTarget CreateIntermediateWorkPieceTarget(IIntermediateWorkPiece intermediateWorkPiece, IComponent component, IWorkOrder workOrder, ILine line, DateTime? day, IShift shift)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    public IIsoFile CreateIsoFile(string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    public IIsoFileSlot CreateIsoFileSlot(IMachineModule machineModule, IIsoFile isoFile, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    [Obsolete ("Use CreateJobFromName or CreateJobFromCode instead", error: false)]
    public IJob CreateJob()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IJob CreateJobFromName (IWorkOrderStatus workOrderStatus, string name) => throw new NotImplementedException();

    /// <summary>
    /// <see cref="Lemoine.Model.IModelFactory">IModelFactory</see> implementation
    /// </summary>
    /// <returns></returns>
    public IJob CreateJobFromCode (IWorkOrderStatus workOrderStatus, string code) => throw new NotImplementedException();

    public IJob CreateJob(IProject project)
    {
      throw new NotImplementedException();
    }

    public IJob CreateJob(IWorkOrder workOrder)
    {
      throw new NotImplementedException();
    }

    public ILine CreateLine()
    {
      throw new NotImplementedException();
    }

    public ILineMachine CreateLineMachine(ILine line, IMachine machine, IOperation operation)
    {
      throw new NotImplementedException();
    }

    public ILinkOperation CreateLinkOperation(IMachine machine, LinkDirection direction, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IMachine CreateMachine()
    {
      throw new NotImplementedException();
    }

    public IMachineActivitySummary CreateMachineActivitySummary(IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode)
    {
      throw new NotImplementedException();
    }

    public IMachineActivitySummary CreateMachineActivitySummary(IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode, IShift shift)
    {
      throw new NotImplementedException();
    }

    public IMachineCategory CreateMachineCategory()
    {
      throw new NotImplementedException();
    }

    public IMachineCellUpdate CreateMachineCellUpdate(IMachine machine, ICell oldCell, ICell newCell)
    {
      throw new NotImplementedException();
    }

    public IMachineCompanyUpdate CreateMachineCompanyUpdate(IMachine machine, ICompany oldCompany, ICompany newCompany)
    {
      throw new NotImplementedException();
    }

    public IMachineDepartmentUpdate CreateMachineDepartmentUpdate(IMachine machine, IDepartment oldDepartment, IDepartment newDepartment)
    {
      throw new NotImplementedException();
    }

    public IMachineFilter CreateMachineFilter(string name)
    {
      throw new NotImplementedException();
    }

    public IMachineFilter CreateMachineFilter(string name, MachineFilterInitialSet initialSet)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(ICompany company, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(IDepartment department, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(IMachineCategory machineCategory, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(IMachineSubCategory machineSubCategory, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(ICell cell, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineFilterItem CreateMachineFilterItem(IMachine machine, MachineFilterRule rule)
    {
      throw new NotImplementedException();
    }

    public IMachineSubCategory CreateMachineSubCategory()
    {
      throw new NotImplementedException();
    }

    public IMachineMode CreateMachineModeFromName(string name, bool running)
    {
      throw new NotImplementedException();
    }

    public IMachineMode CreateMachineModeFromTranslationKey(string translationKey, bool running)
    {
      throw new NotImplementedException();
    }

    public IMachineModeDefaultReason CreateMachineModeDefaultReason(IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      throw new NotImplementedException();
    }

    public IMachineModule CreateMachineModuleFromName (IMonitoredMachine monitoredMachine, string name)
    {
      throw new NotImplementedException();
    }

    public IMachineModule CreateMachineModuleFromCode (IMonitoredMachine monitoredMachine, string code)
    {
      throw new NotImplementedException ();
    }

    public IMachineModuleAnalysisStatus CreateMachineModuleAnalysisStatus(IMachineModule machineModule)
    {
      throw new NotImplementedException();
    }

    public IMachineModuleActivity CreateMachineModuleActivity(IMachineModule machineModule, DateTime utcBegin, DateTime utcEnd, IMachineMode machineMode)
    {
      throw new NotImplementedException();
    }

    public IMachineModuleDetection CreateMachineModuleDetection(IMachineModule machineModule, DateTime dateTime)
    {
      throw new NotImplementedException();
    }

    public IMachineObservationState CreateMachineObservationState()
    {
      throw new NotImplementedException();
    }

    public IMachineObservationStateAssociation CreateMachineObservationStateAssociation(IMachine machine, IMachineObservationState observationState, DateTime beginDateTime)
    {
      throw new NotImplementedException();
    }

    public IMachineObservationStateAssociation CreateMachineObservationStateAssociation(IMachine machine, IMachineObservationState observationState, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IMachineOperationType CreateMachineOperationType(IMachine machine, IOperationType operationType)
    {
      throw new NotImplementedException();
    }

    public IMachineStateTemplate CreateMachineStateTemplate(string name)
    {
      throw new NotImplementedException();
    }

    public IMachineStateTemplateAssociation CreateMachineStateTemplateAssociation(IMachine machine, IMachineStateTemplate machineStateTemplate, DateTime beginDateTime)
    {
      throw new NotImplementedException();
    }

    public IMachineStateTemplateAssociation CreateMachineStateTemplateAssociation(IMachine machine, IMachineStateTemplate machineStateTemplate, UtcDateTimeRange range)
    {
      return new MachineStateTemplateAssociation (machine, machineStateTemplate, range);
    }

    public IMachineStateTemplateFlow CreateMachineStateTemplateFlow(IMachineStateTemplate from, IMachineStateTemplate to)
    {
      throw new NotImplementedException();
    }

    public IMachineStateTemplateRight CreateMachineStateTemplateRight(IMachineStateTemplate machineStateTemplate, IRole role, RightAccessPrivilege accessPrivilege)
    {
      throw new NotImplementedException();
    }

    public IMachineStatus CreateMachineStatus(IMonitoredMachine monitoredMachine)
    {
      throw new NotImplementedException();
    }

    public IMonitoredMachine CreateMonitoredMachine()
    {
      throw new NotImplementedException();
    }

    public IMonitoredMachine CreateMonitoredMachine(IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IMonitoredMachineAnalysisStatus CreateMonitoredMachineAnalysisStatus(IMonitoredMachine monitoredMachine)
    {
      throw new NotImplementedException();
    }

    public INamedConfig CreateNamedConfig(string name, string key)
    {
      throw new NotImplementedException();
    }

    public INonConformanceReason CreateNonConformanceReason(string name)
    {
      throw new NotImplementedException();
    }

    public INonConformanceReport CreateNonConformanceReport(IDeliverablePiece deliverablePiece, IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IObservationStateSlot CreateObservationStateSlot(IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IOperation CreateOperation(IOperationType operationType)
    {
      throw new NotImplementedException();
    }

    public IOperationCycle CreateOperationCycle(IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IOperationInformation CreateOperationInformation(IOperation operation, DateTime dateTime)
    {
      throw new NotImplementedException();
    }

    public IOperationMachineAssociation CreateOperationMachineAssociation(IMachine machine, DateTime begin)
    {
      throw new NotImplementedException();
    }

    public IOperationMachineAssociation CreateOperationMachineAssociation(IMachine machine, DateTime begin, IModification mainModification, bool partOfDetectionAnalysis)
    {
      throw new NotImplementedException();
    }

    public IOperationMachineAssociation CreateOperationMachineAssociation(IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IOperationMachineAssociation CreateOperationMachineAssociation(IMachine machine, UtcDateTimeRange range, IModification mainModification, bool partOfDetectionAnalysis)
    {
      throw new NotImplementedException();
    }

    public IOperationSlot CreateOperationSlot(IMachine machine, IOperation operation, IComponent component, IWorkOrder workOrder, ILine line, IManufacturingOrder manufacturingOrder, DateTime? day, IShift shift, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IOperationSlotSplit CreateOperationSlotSplit(IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IOperationCycleDeliverablePiece CreateOperationCycleDeliverablePiece(IDeliverablePiece deliverablePiece, IOperationCycle operationCycle)
    {
      throw new NotImplementedException();
    }

    public IOperationSourceWorkPiece CreateOperationSourceWorkPiece(IOperation operation, IIntermediateWorkPiece intermediateWorkPiece)
    {
      throw new NotImplementedException();
    }

    public IOperationType CreateOperationTypeFromName(string name)
    {
      throw new NotImplementedException();
    }

    public IPackage CreatePackage(string identifyingName)
    {
      throw new NotImplementedException();
    }

    public IPackagePluginAssociation CreatePackagePluginAssociation(IPackage package, IPlugin plugin, string name)
    {
      throw new NotImplementedException();
    }

    public IPart CreatePart(IComponentType componentType)
    {
      throw new NotImplementedException();
    }

    public IPart CreatePart(IComponent component)
    {
      throw new NotImplementedException();
    }

    public IPart CreatePart(IProject project)
    {
      throw new NotImplementedException();
    }

    public IPath CreatePath()
    {
      throw new NotImplementedException();
    }

    public IPath CreatePath (IOperation operation)
    {
      throw new NotImplementedException ();
    }

    public IPlugin CreatePlugin(string identifyingName)
    {
      throw new NotImplementedException();
    }

    public IProcessMachineStateTemplate CreateProcessMachineStateTemplate (IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }
    
    public IProductionAnalysisStatus CreateProductionAnalysisStatus(IMachine machine)
    {
      throw new NotImplementedException();
    }

    public IProductionInformationShift CreateProductionInformationShift(IMachine machine, DateTime day, IShift shift, IIntermediateWorkPiece intermediateWorkPiece, int checkedValue)
    {
      throw new NotImplementedException();
    }

    public IProductionState CreateProductionState (string color)
    {
      throw new NotImplementedException ();
    }

    public IProject CreateProjectFromCode(string code)
    {
      throw new NotImplementedException();
    }

    public IProject CreateProjectFromName(string name)
    {
      throw new NotImplementedException();
    }

    public IProjectComponentUpdate CreateProjectComponentUpdate(IComponent component, IProject oldProject, IProject newProject)
    {
      throw new NotImplementedException();
    }

    public IReason CreateReason(IReasonGroup reasonGroup)
    {
      throw new NotImplementedException();
    }

    public IReasonMachineAssociation CreateReasonMachineAssociation(IMachine machine, UtcDateTimeRange range)
    {
      return new ReasonMachineAssociation (machine, range);
    }

    public IReasonMachineAssociation CreateReasonMachineAssociation (IMachine machine, LowerBound<DateTime> begin, UpperBound<DateTime> end, string dynamic)
    {
      throw new NotImplementedException ();
    }

    public IReasonGroup CreateReasonGroup()
    {
      throw new NotImplementedException();
    }

    public IReasonSelection CreateReasonSelection(IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      throw new NotImplementedException();
    }

    public IReasonSelection CreateReasonSelection(IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, bool detailsRequired)
    {
      throw new NotImplementedException();
    }

    public IReasonSlot CreateReasonSlot(IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IReasonSummary CreateReasonSummary(IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IReason reason)
    {
      throw new NotImplementedException();
    }

    public IRevision CreateRevision()
    {
      return new Revision ();
    }

    public IRole CreateRoleFromName(string name)
    {
      throw new NotImplementedException();
    }

    public ISequence CreateSequence(string name)
    {
      throw new NotImplementedException();
    }

    public ISequence CreateSequence (string name, IOperation operation, IPath path)
    {
      throw new NotImplementedException ();
    }

    public ISequenceMilestone CreateSequenceMilestone (IMachineModule machineModule) => throw new NotImplementedException ();

    public ISequenceSlot CreateSequenceSlot(IMachineModule machineModule, ISequence sequence, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public ISerialNumberMachineStamp CreateSerialNumberMachineStamp(IMachine machine, string serialNumber, DateTime dateTime)
    {
      throw new NotImplementedException();
    }

    public ISerialNumberModification CreateSerialNumberModification(IMachine machine, string serialNumber, DateTime beginOrEndDateTime, bool isBegin, DateTime dateTime)
    {
      throw new NotImplementedException();
    }

    public IShift CreateShiftFromName(string name)
    {
      throw new NotImplementedException();
    }

    public IShift CreateShiftFromCode(string code)
    {
      throw new NotImplementedException();
    }

    public IShiftSlot CreateShiftSlot(IShiftTemplate shiftTemplate, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IShiftTemplate CreateShiftTemplate(string name)
    {
      throw new NotImplementedException();
    }

    public IShiftTemplateAssociation CreateShiftTemplateAssociation(IShiftTemplate shiftTemplate, DateTime beginDateTime)
    {
      throw new NotImplementedException();
    }

    public ISimpleOperation CreateSimpleOperation(IOperationType operationType)
    {
      throw new NotImplementedException();
    }

    public ISimpleOperation CreateSimpleOperation(IOperation operation)
    {
      throw new NotImplementedException();
    }

    public ISimpleOperation CreateSimpleOperation(IIntermediateWorkPiece intermediateWorkPiece)
    {
      throw new NotImplementedException();
    }

    public IStamp CreateStamp()
    {
      throw new NotImplementedException();
    }

    public IStampingConfigByName CreateStampingConfigByName (string name)
    {
      throw new NotImplementedException ();
    }

    public IStampingValue CreateStampingValue(ISequence sequence, IField field)
    {
      throw new NotImplementedException();
    }

    public ISynchronizationLog CreateSynchronizationLog(LogLevel level, string message, string xmlElement)
    {
      throw new NotImplementedException();
    }

    public IManufacturingOrder CreateManufacturingOrder(int taskId)
    {
      throw new NotImplementedException();
    }
    public IManufacturingOrderMachineAssociation CreateManufacturingOrderMachineAssociation (IMachine machine, IManufacturingOrder manufacturingOrder, UtcDateTimeRange range)
    {
      var association = new ManufacturingOrderMachineAssociation (machine, range);
      association.ManufacturingOrder = manufacturingOrder;
      return association;
    }
    
    public IToolLife CreateToolLife(IMachineModule machineModule, IToolPosition position, IUnit unit, Lemoine.Core.SharedData.ToolLifeDirection direction)
    {
      throw new NotImplementedException();
    }

    public IToolPosition CreateToolPosition(IMachineModule machineModule, string toolId)
    {
      throw new NotImplementedException();
    }

    public ITranslation CreateTranslation(string locale, string translationKey)
    {
      throw new NotImplementedException();
    }

    public IUnit CreateUnit()
    {
      throw new NotImplementedException();
    }

    public IUser CreateUser(string login, string password)
    {
      throw new NotImplementedException();
    }

    public IUserAttendance CreateUserAttendance(IUser user)
    {
      throw new NotImplementedException();
    }

    public IUserMachineAssociation CreateUserMachineAssociation(IUser user, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IUserMachineSlot CreateUserMachineSlot(IUser user, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IUserShiftAssociation CreateUserShiftAssociation(IUser user, UtcDateTimeRange range, IShift shift)
    {
      throw new NotImplementedException();
    }

    public IUserShiftSlot CreateUserShiftSlot(IUser user, UtcDateTimeRange range, IShift shift)
    {
      throw new NotImplementedException();
    }

    public IUserSlot CreateUserSlot(IUser user, UtcDateTimeRange range)
    {
      throw new NotImplementedException();
    }

    public IWorkOrder CreateWorkOrder(IWorkOrderStatus workOrderStatus, string workOrderName)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderLine CreateWorkOrderLine(ILine line, UtcDateTimeRange range, IWorkOrder workOrder)
    {
      throw new NotImplementedException();
    }

    public Lemoine.ModelDAO.IWorkOrderLineAssociation CreateWorkOrderLineAssociation(ILine line, DateTime beginTime, DateTime deadline)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation(IMachine machine, IWorkOrder workOrder, DateTime beginTime)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation(IMachine machine, IWorkOrder workOrder, UtcDateTimeRange range)
    {
      var association = new WorkOrderMachineAssociation (machine, range);
      association.WorkOrder = workOrder;
      return association;
    }

    public IWorkOrderMachineStamp CreateWorkOrderMachineStamp(IMachine machine, IWorkOrder workOrder, DateTime dateTime)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderProject CreateWorkOrderProject(IWorkOrder workOrder, IProject project)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderProjectUpdate CreateWorkOrderProjectUpdate(IWorkOrder workOrder, IProject project, WorkOrderProjectUpdateModificationType type)
    {
      throw new NotImplementedException();
    }

    public IWorkOrderStatus CreateWorkOrderStatusFromName(string name)
    {
      throw new NotImplementedException();
    }

    public ICncVariable CreateCncVariable (IMachineModule machineModule, UtcDateTimeRange range, string key, object v)
    {
      throw new NotImplementedException ();
    }

    public IIsoFileMachineModuleAssociation CreateIsoFileMachineModuleAssociation (IMachineModule machineModule, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }

    public IMachineCncVariable CreateMachineCncVariable (string cncVariableKey, object cncVariableValue)
    {
      throw new NotImplementedException ();
    }

    public IAutoReasonState CreateAutoReasonState (IMonitoredMachine machine, string key)
    {
      throw new NotImplementedException ();
    }

    public IService CreateService (IComputer computer, string name, string program, bool lemoine)
    {
      throw new NotImplementedException ();
    }

    public ICncDataImportLog CreateCncDataImportLog (LogLevel level, string message, IMachine machine)
    {
      throw new NotImplementedException ();
    }

    public ICncDataImportLog CreateCncDataImportLog (LogLevel level, string message, IMachine machine, IMachineModule machineModule)
    {
      throw new NotImplementedException ();
    }

    public IReasonProposal CreateReasonProposal (IReasonMachineAssociation reasonMachineAssociation, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }

    public IProductionRateSummary CreateProductionRateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState)
    {
      throw new NotImplementedException ();
    }

    public IProductionStateSummary CreateProductionStateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IProductionState productionState)
    {
      throw new NotImplementedException ();
    }

    public IRefreshToken CreateRefreshToken (IUser user, TimeSpan expiresIn)
    {
      throw new NotImplementedException ();
    }

    public ICustomer CreateCustomerFromName (string name)
    {
      throw new NotImplementedException ();
    }

    public ICustomer CreateCustomerFromCode (string code)
    {
      throw new NotImplementedException ();
    }

    public IWorkOrder CreateWorkOrderFromCode (IWorkOrderStatus workOrderStatus, string workOrderCode)
    {
      throw new NotImplementedException ();
    }

    public IOperationRevision CreateOperationRevision (IOperation operation)
    {
      throw new NotImplementedException ();
    }

    public IOperationModel CreateOperationModel (IOperationRevision operationRevision)
    {
      throw new NotImplementedException ();
    }

    public IOperationModel CreateOperationModel (IOperation operation)
    {
      throw new NotImplementedException ();
    }

    public IOperationDuration CreateOperationDuration (IOperation operation, DateTime? from = null)
    {
      throw new NotImplementedException ();
    }

    public IOperationDuration CreateOperationDuration (IOperationModel operationModel, DateTime? from = null)
    {
      throw new NotImplementedException ();
    }

    public ISequence CreateSequence (string name, IOperationModel operationModel, int pathNumber, ISequence before, ISequence after)
    {
      throw new NotImplementedException ();
    }

    public ISequenceDuration CreateSequenceDuration (ISequenceOperationModel sequenceOperationModel, TimeSpan duration, DateTime? from)
    {
      throw new NotImplementedException ();
    }

    public ISequenceOperationModel CreateSequenceOperationModel (ISequence sequence, IOperationModel operationModel, ISequence before, ISequence after)
    {
      throw new NotImplementedException ();
    }

    public IScrapReport CreateScrapReport (IOperationSlot operationSlot, UtcDateTimeRange dateTimeRange)
    {
      throw new NotImplementedException ();
    }

    public IScrapReasonReport CreateScrapReasonReport (IScrapReport scrapReport, INonConformanceReason reason, int quantity)
      => throw new NotImplementedException ();

    public Lemoine.ModelDAO.IDAOFactory DAOFactory {
      get { return m_daoFactory; }
    }

    #endregion
  }
}
