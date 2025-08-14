// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.Model
{
  /// <summary>
  /// Factory for the model classes
  /// </summary>
  public interface IModelFactory
  {
    /// <summary>
    /// Associated DAOFactory
    /// </summary>
    IDAOFactory DAOFactory { get; }

    /// <summary>
    /// Create a new acquisition state
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    IAcquisitionState CreateAcquisitionState (IMachineModule machineModule, AcquisitionStateKey key);

    /// <summary>
    /// Create an ActivityManual object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineMode"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IActivityManual CreateActivityManual (IMonitoredMachine machine,
                                          IMachineMode machineMode,
                                          UtcDateTimeRange range);

    /// <summary>
    /// Create a CncAlarm object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="alarmCncInfo"></param>
    /// <param name="alarmCncSubInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="alarmNumber"></param>
    /// <returns></returns>
    ICncAlarm CreateCncAlarm (IMachineModule machineModule,
                             UtcDateTimeRange range,
                             string alarmCncInfo,
                             string alarmCncSubInfo,
                             string alarmType,
                             string alarmNumber);

    /// <summary>
    /// Create a CurrentCncAlarm object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="datetime"></param>
    /// <param name="alarmCncInfo"></param>
    /// <param name="alarmCncSubInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="alarmNumber"></param>
    /// <returns></returns>
    ICurrentCncAlarm CreateCurrentCncAlarm (IMachineModule machineModule,
                                           DateTime datetime,
                                           string alarmCncInfo,
                                           string alarmCncSubInfo,
                                           string alarmType,
                                           string alarmNumber);

    /// <summary>
    /// Create a CncAlarmSeverity object
    /// </summary>
    /// <param name="cncInfo">not null or empty</param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    ICncAlarmSeverity CreateCncAlarmSeverity (
      string cncInfo,
      string name);

    /// <summary>
    /// Create a CncAlarmSeverityPattern object
    /// </summary>
    /// <param name="cncInfo">not null or empty</param>
    /// <param name="rules">not null</param>
    /// <param name="severity">not null</param>
    /// <returns></returns>
    ICncAlarmSeverityPattern CreateCncAlarmSeverityPattern (
      string cncInfo,
      CncAlarmSeverityPatternRules rules,
      ICncAlarmSeverity severity);

    /// <summary>
    /// Create an GlobalModificationLog object
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    IGlobalModificationLog CreateGlobalModificationLog (LogLevel level,
                                                        string message,
                                                        IGlobalModification modification);

    /// <summary>
    /// Create a MachineModificationLog object
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    IMachineModificationLog CreateMachineModificationLog (LogLevel level,
                                                          string message,
                                                          IMachineModification modification);

    /// <summary>
    /// Create a ApplicationState object
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IApplicationState CreateApplicationState (string key);

    /// <summary>
    /// Create an AutoReasonState object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    IAutoReasonState CreateAutoReasonState (IMonitoredMachine machine, string key);

    /// <summary>
    /// Create a new AutoMachineStateTemplate object
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="newMachineStateTemplate">not null</param>
    /// <returns></returns>
    IAutoMachineStateTemplate CreateAutoMachineStateTemplate (IMachineMode machineMode, IMachineStateTemplate newMachineStateTemplate);

    /// <summary>
    /// Create a new AutoSequence object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="sequence"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    IAutoSequence CreateAutoSequence (IMachineModule machineModule, ISequence sequence, DateTime begin);

    /// <summary>
    /// Create a new AutoSequence object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="operation"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    IAutoSequence CreateAutoSequence (IMachineModule machineModule, IOperation operation, DateTime begin);

    /// <summary>
    /// Create a new BetweenCycles object
    /// </summary>
    /// <param name="previousCycle"></param>
    /// <param name="nextCycle"></param>
    /// <returns></returns>
    IBetweenCycles CreateBetweenCycles (IOperationCycle previousCycle, IOperationCycle nextCycle);

    /// <summary>
    /// Create a CadModel object
    /// </summary>
    /// <returns></returns>
    ICadModel CreateCadModel ();

    /// <summary>
    /// Create a Cell object
    /// </summary>
    /// <returns></returns>
    ICell CreateCell ();

    /// <summary>
    /// Create a CncAcquisition object
    /// </summary>
    /// <returns></returns>
    ICncAcquisition CreateCncAcquisition ();

    /// <summary>
    /// Create a CncDataImportLog
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    ICncDataImportLog CreateCncDataImportLog (LogLevel level,
                                                      string message,
                                                      IMachine machine);

    /// <summary>
    /// Create a CncDataImportLog
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    ICncDataImportLog CreateCncDataImportLog (LogLevel level,
                                                      string message,
                                                      IMachine machine,
                                                      IMachineModule machineModule);

    /// <summary>
    /// Create a CncValue object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    ICncValue CreateCncValue (IMachineModule machineModule, IField field, DateTime beginDateTime);


    /// <summary>
    /// Create a CncVariable object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    ICncVariable CreateCncVariable (IMachineModule machineModule, UtcDateTimeRange range, string key, object v);

    /// <summary>
    /// Create a Company object
    /// </summary>
    /// <returns></returns>
    ICompany CreateCompany ();

    /// <summary>
    /// Create a Component object given its name
    /// </summary>
    /// <param name="project"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IComponent CreateComponentFromName (IProject project, string name, IComponentType type);

    /// <summary>
    /// Create a Component object given its code
    /// </summary>
    /// <param name="project"></param>
    /// <param name="code"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IComponent CreateComponentFromCode (IProject project, string code, IComponentType type);

    /// <summary>
    /// Create a Component object given its type
    /// </summary>
    /// <param name="project"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IComponent CreateComponentFromType (IProject project, IComponentType type);

    // Note: to create a ComponentIntermediateWorkPiece, use the method IComponent.AddIntermediateWorkPiece instead

    /// <summary>
    /// Create a new ComponentIntermediateWorkPieceUpdate
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IComponentIntermediateWorkPieceUpdate CreateComponentIntermediateWorkPieceUpdate (IComponent component, IIntermediateWorkPiece intermediateworkpiece, ComponentIntermediateWorkPieceUpdateModificationType type);

    /// <summary>
    /// Create a ComponentMachineAssociation object
    ///
    /// For some existing unit tests
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="component"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    [Obsolete ("User the method with the range argument instead")]
    IComponentMachineAssociation CreateComponentMachineAssociation (IMachine machine,
                                                                    IComponent component,
                                                                    DateTime begin,
                                                                    UpperBound<DateTime> end);

    /// <summary>
    /// Create a ComponentMachineAssociation object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="component"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IComponentMachineAssociation CreateComponentMachineAssociation (IMachine machine,
                                                                    IComponent component,
                                                                    UtcDateTimeRange range);

    /// <summary>
    /// Create a ComponentType object given its name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IComponentType CreateComponentTypeFromName (string name);

    /// <summary>
    /// Create a ComponentType object given its translation key
    /// </summary>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    IComponentType CreateComponentTypeFromTranslationKey (string translationKey);

    /// <summary>
    /// Create a Computer object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    IComputer CreateComputer (string name, string address);

    /// <summary>
    /// Create a Config object
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig CreateConfig (string key);

    /// <summary>
    /// Create an Analysis Config object
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig CreateConfig (AnalysisConfigKey key);

    /// <summary>
    /// Create a current cnc value
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    ICurrentCncValue CreateCurrentCncValue (IMachineModule machineModule, IField field);

    /// <summary>
    /// Create a current machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    ICurrentMachineMode CreateCurrentMachineMode (IMonitoredMachine machine);

    /// <summary>
    /// Create a customer from name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    ICustomer CreateCustomerFromName (string name);

    /// <summary>
    /// Create a customer from code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    ICustomer CreateCustomerFromCode (string code);

    /// <summary>
    /// Create a DayTemplate object
    /// </summary>
    /// <returns></returns>
    IDayTemplate CreateDayTemplate ();

    /// <summary>
    /// Create a DaySlot object
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IDaySlot CreateDaySlot (IDayTemplate dayTemplate, UtcDateTimeRange range);

    /// <summary>
    /// Create a DayTemplateChange object
    /// </summary>
    /// <param name="dayTemplate">not null</param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    IDayTemplateChange CreateDayTemplateChange (IDayTemplate dayTemplate,
                                                DateTime beginDateTime);

    /// <summary>
    /// Create a DayTemplateSlot object
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IDayTemplateSlot CreateDayTemplateSlot (IDayTemplate dayTemplate, UtcDateTimeRange range);

    /// <summary>
    /// Create a DeliverablePiece object
    /// </summary>
    /// <returns></returns>
    IDeliverablePiece CreateDeliverablePiece (string serialID);

    /// <summary>
    /// Create a Department object
    /// </summary>
    /// <returns></returns>
    IDepartment CreateDepartment ();

    /// <summary>
    /// Create a DetectionAnalysisLog
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    IDetectionAnalysisLog CreateDetectionAnalysisLog (LogLevel level,
                                                      string message,
                                                      IMachine machine);

    /// <summary>
    /// Create a DetectionAnalysisLog
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IDetectionAnalysisLog CreateDetectionAnalysisLog (LogLevel level,
                                                      string message,
                                                      IMachine machine,
                                                      IMachineModule machineModule);

    /// <summary>
    /// Create a Display object
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    IDisplay CreateDisplay (string table);

    /// <summary>
    /// Create an EmailConfig object
    /// </summary>
    /// <returns></returns>
    IEmailConfig CreateEmailConfig ();

    /// <summary>
    /// Create an EventLevel object given its name
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    IEventLevel CreateEventLevelFromName (int priority, string name);

    /// <summary>
    /// Create an EventLevel object given its translation key
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="translationKey">not null or empty</param>
    /// <returns></returns>
    IEventLevel CreateEventLevelFromTranslationKey (int priority, string translationKey);

    /// <summary>
    /// Create an EventCncValue object
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
    IEventCncValue CreateEventCncValue (IEventLevel level,
                                        DateTime dateTime,
                                        string message,
                                        IMachineModule machineModule,
                                        IField field,
                                        object v,
                                        TimeSpan duration,
                                        IEventCncValueConfig config);

    /// <summary>
    /// Create an EventCncValueConfig object
    /// </summary>
    /// <param name="name">not null or empty</param>
    /// <param name="field"></param>
    /// <param name="level"></param>
    /// <param name="message">not null or empty</param>
    /// <param name="condition">not null or empty</param>
    /// <returns></returns>
    IEventCncValueConfig CreateEventCncValueConfig (string name,
                                                   IField field,
                                                   IEventLevel level,
                                                   string message,
                                                   string condition);

    /// <summary>
    /// Create an EventLongPeriod object
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="monitoredMachine"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="triggerDuration"></param>
    /// <returns></returns>
    IEventLongPeriod CreateEventLongPeriod (IEventLevel level,
                                            DateTime dateTime,
                                            IMonitoredMachine monitoredMachine,
                                            IMachineMode machineMode,
                                            IMachineObservationState machineObservationState,
                                            TimeSpan triggerDuration);

    /// <summary>
    /// Create an EventLongPeriodConfig object
    /// </summary>
    /// <param name="triggerDuration"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    IEventLongPeriodConfig CreateEventLongPeriodConfig (TimeSpan triggerDuration,
                                                       IEventLevel level);

    /// <summary>
    /// Create an EventMessage
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    IEventMessage CreateEventMessage (IEventLevel level, string message);

    /// <summary>
    /// Create an EventMachineMessage
    /// </summary>
    /// <param name="level">not null</param>
    /// <param name="machine">not null</param>
    /// <param name="message"></param>
    /// <returns></returns>
    IEventMachineMessage CreateEventMachineMessage (IEventLevel level, IMachine machine, string message);

    /// <summary>
    /// Create an EventToolLife object
    /// </summary>
    /// <param name="level"></param>
    /// <param name="type"></param>
    /// <param name="dateTime"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IEventToolLife CreateEventToolLife (IEventLevel level, EventToolLifeType type,
                                       DateTime dateTime, IMachineModule machineModule);

    /// <summary>
    /// Create an EventToolLife object
    /// </summary>
    /// <param name="eventToolLifeType"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    IEventToolLifeConfig CreateEventToolLifeConfig (EventToolLifeType eventToolLifeType, IEventLevel level);

    /// <summary>
    /// Create a Fact object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcBegin"></param>
    /// <param name="utcEnd"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IFact CreateFact (IMonitoredMachine machine,
                      DateTime utcBegin,
                      DateTime utcEnd,
                      IMachineMode machineMode);

    /// <summary>
    /// Create a Field object
    /// </summary>
    /// <param name="code">not null or empty</param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    IField CreateFieldFromName (string code, string name);

    /// <summary>
    /// Create a Field object
    /// </summary>
    /// <param name="code">not null or empty</param>
    /// <param name="translationKey">not null or empty</param>
    /// <returns></returns>
    IField CreateFieldFromTranslationKey (string code, string translationKey);

    /// <summary>
    /// Create a FieldLegend object
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="text">Can't be null</param>
    /// <param name="color">Can't be null</param>
    /// <returns></returns>
    IFieldLegend CreateFieldLegend (IField field, string text, string color);

    /// <summary>
    /// Create a Goal object
    /// </summary>
    /// <param name="type">Can't be null</param>
    /// <returns></returns>
    IGoal CreateGoal (IGoalType type);

    /// <summary>
    /// Create a GoalType object
    /// </summary>
    /// <param name="name">Can't be null</param>
    /// <returns></returns>
    IGoalType CreateGoalType (string name);

    /// <summary>
    /// Create an IntermediateWorkPiece object
    /// </summary>
    /// <returns></returns>
    /// <param name="operation">Operation used to make this intermediate work piece</param>
    /// <returns></returns>
    IIntermediateWorkPiece CreateIntermediateWorkPiece (IOperation operation);

    /// <summary>
    /// Create a new IntermediateWorkPieceOperationUpdate
    /// </summary>
    /// <param name="intermediateworkpiece"></param>
    /// <param name="oldOperation"></param>
    /// <param name="newOperation"></param>
    /// <returns></returns>
    IIntermediateWorkPieceOperationUpdate CreateIntermediateWorkPieceOperationUpdate (IIntermediateWorkPiece intermediateworkpiece, IOperation oldOperation, IOperation newOperation);

    /// <summary>
    /// Create an IntermediateWorkPieceTarget object
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    IIntermediateWorkPieceTarget CreateIntermediateWorkPieceTarget (IIntermediateWorkPiece intermediateWorkPiece,
                                                                    IComponent component,
                                                                    IWorkOrder workOrder,
                                                                    ILine line,
                                                                    DateTime? day,
                                                                    IShift shift);

    /// <summary>
    /// Create an IsoFile object
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IIsoFile CreateIsoFile (string name);

    /// <summary>
    /// Create a new IsoFileMachineModuleAssociation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IIsoFileMachineModuleAssociation CreateIsoFileMachineModuleAssociation (IMachineModule machineModule,
      UtcDateTimeRange range);

    /// <summary>
    /// Create new IsoFileSlot
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="isoFile">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IIsoFileSlot CreateIsoFileSlot (IMachineModule machineModule,
                                   IIsoFile isoFile,
                                   UtcDateTimeRange range);

    /// <summary>
    /// Create a new Job
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use CreateJobFromName or CreateJobFromCode instead", error: false)]
    IJob CreateJob ();

    /// <summary>
    /// Create a new Job from the name
    /// </summary>
    /// <returns></returns>
    IJob CreateJobFromName (IWorkOrderStatus workOrderStatus, string name);

    /// <summary>
    /// Create a new Job from the name
    /// </summary>
    /// <returns></returns>
    IJob CreateJobFromCode (IWorkOrderStatus workOrderStatus, string code);

    /// <summary>
    /// Create a new Job from a Project
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    IJob CreateJob (IProject project);

    /// <summary>
    /// Create a new Job from a WorkOrder
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    IJob CreateJob (IWorkOrder workOrder);

    /// <summary>
    /// Create a new Line
    /// </summary>
    /// <returns></returns>
    ILine CreateLine ();

    /// <summary>
    /// Create a new LineMachine
    /// </summary>
    /// <param name="line"></param>
    /// <param name="machine"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    ILineMachine CreateLineMachine (ILine line, IMachine machine, IOperation operation);

    /// <summary>
    /// Create a LinkOperation object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    ILinkOperation CreateLinkOperation (IMachine machine,
                                        LinkDirection direction,
                                        UtcDateTimeRange range);

    /// <summary>
    /// Create a Machine object
    /// </summary>
    /// <returns></returns>
    IMachine CreateMachine ();

    /// <summary>
    /// Create a MachineActivitySummary object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IMachineActivitySummary CreateMachineActivitySummary (IMachine machine,
                                                          DateTime day,
                                                          IMachineObservationState machineObservationState,
                                                          IMachineMode machineMode);

    /// <summary>
    /// Create a MachineActivitySummary object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    IMachineActivitySummary CreateMachineActivitySummary (IMachine machine,
                                                          DateTime day,
                                                          IMachineObservationState machineObservationState,
                                                          IMachineMode machineMode,
                                                          IShift shift);

    /// <summary>
    /// Create a MachineCategory object
    /// </summary>
    /// <returns></returns>
    IMachineCategory CreateMachineCategory ();

    /// <summary>
    /// Create a new MachineCellUpdate object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCell"></param>
    /// <param name="newCell"></param>
    /// <returns></returns>
    IMachineCellUpdate CreateMachineCellUpdate (IMachine machine,
                                                ICell oldCell,
                                                ICell newCell);

    /// <summary>
    /// Create a new MachineCncVariable
    /// </summary>
    /// <param name="cncVariableKey"></param>
    /// <param name="cncVariableValue"></param>
    /// <returns></returns>
    IMachineCncVariable CreateMachineCncVariable (string cncVariableKey, object cncVariableValue);

    /// <summary>
    /// Create a new MachineCompanyUpdate object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCompany"></param>
    /// <param name="newCompany"></param>
    /// <returns></returns>
    IMachineCompanyUpdate CreateMachineCompanyUpdate (IMachine machine,
                                                      ICompany oldCompany,
                                                      ICompany newCompany);

    /// <summary>
    /// Create a new MachineDepartmentUpdate object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldDepartment"></param>
    /// <param name="newDepartment"></param>
    /// <returns></returns>
    IMachineDepartmentUpdate CreateMachineDepartmentUpdate (IMachine machine,
                                                            IDepartment oldDepartment,
                                                            IDepartment newDepartment);

    /// <summary>
    /// Create a MachineFilter object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="initialSet"></param>
    /// <returns></returns>
    IMachineFilter CreateMachineFilter (string name, MachineFilterInitialSet initialSet);

    /// <summary>
    /// Create a MachineFilterCompany object
    /// </summary>
    /// <param name="company">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (ICompany company, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineFilterDepartment object
    /// </summary>
    /// <param name="department">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (IDepartment department, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineFilterMachineCategory object
    /// </summary>
    /// <param name="machineCategory">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (IMachineCategory machineCategory, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineFilterMachineSubCategory object
    /// </summary>
    /// <param name="machineSubCategory">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (IMachineSubCategory machineSubCategory, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineFilterCell object
    /// </summary>
    /// <param name="cell">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (ICell cell, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineFilterMachine object
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="rule"></param>
    /// <returns></returns>
    IMachineFilterItem CreateMachineFilterItem (IMachine machine, MachineFilterRule rule);

    /// <summary>
    /// Create a MachineSubCategory object
    /// </summary>
    /// <returns></returns>
    IMachineSubCategory CreateMachineSubCategory ();

    /// <summary>
    /// Create a MachineMode object given its name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="running"></param>
    /// <returns></returns>
    IMachineMode CreateMachineModeFromName (string name,
                                            bool running);

    /// <summary>
    /// Create a MachineMode object given its translation key
    /// </summary>
    /// <param name="translationKey"></param>
    /// <param name="running"></param>
    /// <returns></returns>
    IMachineMode CreateMachineModeFromTranslationKey (string translationKey,
                                                      bool running);

    /// <summary>
    /// Create a MachineModeDefaultReason
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    IMachineModeDefaultReason CreateMachineModeDefaultReason (IMachineMode machineMode,
                                                             IMachineObservationState machineObservationState);

    /// <summary>
    /// Create a MachineModule given its name
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    IMachineModule CreateMachineModuleFromName (IMonitoredMachine monitoredMachine, string name);

    /// <summary>
    /// Create a MachineModule given its code
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="code">not null or empty</param>
    /// <returns></returns>
    IMachineModule CreateMachineModuleFromCode (IMonitoredMachine monitoredMachine, string code);

    /// <summary>
    /// Create a MachineModuleAnalysisStatus
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IMachineModuleAnalysisStatus CreateMachineModuleAnalysisStatus (IMachineModule machineModule);

    /// <summary>
    /// Create a MachineModuleActivity object
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="utcBegin"></param>
    /// <param name="utcEnd"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IMachineModuleActivity CreateMachineModuleActivity (IMachineModule machineModule,
                                                        DateTime utcBegin,
                                                        DateTime utcEnd,
                                                        IMachineMode machineMode);

    /// <summary>
    /// Create a MachineModuleDetection
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IMachineModuleDetection CreateMachineModuleDetection (IMachineModule machineModule, DateTime dateTime);

    /// <summary>
    /// Create a MachineObservationState
    /// </summary>
    /// <returns></returns>
    IMachineObservationState CreateMachineObservationState ();

    /// <summary>
    /// Create a MachineObservationStateAssociation
    /// for a Machine / MachineObservationState / Begin date time
    ///
    /// For some existing unit tests
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="observationState"></param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    IMachineObservationStateAssociation
      CreateMachineObservationStateAssociation (IMachine machine,
                                                IMachineObservationState observationState,
                                                DateTime beginDateTime);

    /// <summary>
    /// Create a MachineObservationStateAssociation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="observationState"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IMachineObservationStateAssociation
      CreateMachineObservationStateAssociation (IMachine machine,
                                                IMachineObservationState observationState,
                                                UtcDateTimeRange range);

    /// <summary>
    /// Create a MachineOperationType
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    IMachineOperationType CreateMachineOperationType (IMachine machine,
                                                      IOperationType operationType);

    /// <summary>
    /// Create a MachineStateTemplate
    /// </summary>
    /// <returns></returns>
    IMachineStateTemplate CreateMachineStateTemplate (string name);

    /// <summary>
    /// Create a MachineStateTemplateAssociation
    ///
    /// For some existing unit tests only
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    IMachineStateTemplateAssociation
      CreateMachineStateTemplateAssociation (IMachine machine,
                                             IMachineStateTemplate machineStateTemplate,
                                             DateTime beginDateTime);

    /// <summary>
    /// Create a MachineStateTemplateAssociation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IMachineStateTemplateAssociation
      CreateMachineStateTemplateAssociation (IMachine machine,
                                             IMachineStateTemplate machineStateTemplate,
                                             UtcDateTimeRange range);

    /// <summary>
    /// Create a new MachineStateTemplateFlow
    /// </summary>
    /// <param name="from">not null</param>
    /// <param name="to">not null</param>
    /// <returns></returns>
    IMachineStateTemplateFlow
      CreateMachineStateTemplateFlow (IMachineStateTemplate from,
                                      IMachineStateTemplate to);

    /// <summary>
    /// Create a MachineStateTemplateRight
    /// </summary>
    /// <param name="machineStateTemplate">if null, it is applicable to all machine state templates</param>
    /// <param name="role">if null, it is applicable to all roles</param>
    /// <param name="accessPrivilege"></param>
    /// <returns></returns>
    IMachineStateTemplateRight
      CreateMachineStateTemplateRight (IMachineStateTemplate machineStateTemplate,
                                       IRole role,
                                       RightAccessPrivilege accessPrivilege);

    /// <summary>
    /// Create a MachineStatus
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    IMachineStatus CreateMachineStatus (IMonitoredMachine monitoredMachine);

    /// <summary>
    /// Create a MonitoredMachine
    /// </summary>
    /// <returns></returns>
    IMonitoredMachine CreateMonitoredMachine ();

    /// <summary>
    /// Create a MonitoredMachine based on a machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IMonitoredMachine CreateMonitoredMachine (IMachine machine);

    /// <summary>
    /// Create a MonitoredMachineAnalysisStatus
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    IMonitoredMachineAnalysisStatus CreateMonitoredMachineAnalysisStatus (IMonitoredMachine monitoredMachine);

    /// <summary>
    /// Create a NamedConfig object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    INamedConfig CreateNamedConfig (string name, string key);

    /// <summary>
    /// Create new NonConformanceReason
    /// </summary>
    /// <param name="name">not null and not empty</param>
    /// <returns></returns>
    INonConformanceReason CreateNonConformanceReason (string name);

    /// <summary>
    /// Create new NonConformanceReport
    /// </summary>
    /// <param name="deliverablePiece"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    INonConformanceReport CreateNonConformanceReport (IDeliverablePiece deliverablePiece, IMachine machine);

    /// <summary>
    /// Create an ObservationStateSlot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IObservationStateSlot CreateObservationStateSlot (IMachine machine,
                                                      UtcDateTimeRange range);

    /// <summary>
    /// Create a new Operation
    /// </summary>
    /// <param name="operationType"></param>
    /// <returns></returns>
    IOperation CreateOperation (IOperationType operationType);

    /// <summary>
    /// Create a new operation revision
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IOperationRevision CreateOperationRevision (IOperation operation);

    /// <summary>
    /// Create a new operation revision
    /// </summary>
    /// <param name="operationRevision"></param>
    /// <returns></returns>
    IOperationModel CreateOperationModel (IOperationRevision operationRevision);

    /// <summary>
    /// Create a new operation model considering the latest revision
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IOperationModel CreateOperationModel (IOperation operation);

    /// <summary>
    /// Create a new OperationCycle
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IOperationCycle CreateOperationCycle (IMachine machine);

    /// <summary>
    /// Create a new OperationInformation
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationInformation CreateOperationInformation (IOperation operation, DateTime dateTime);

    /// <summary>
    /// Create a new OperationMachineAssociation
    ///
    /// For the unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                    DateTime begin);

    /// <summary>
    /// Create a new OperationMachineAssociation with a main modification that is used for the logs for example
    ///
    /// For the unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <returns></returns>
    IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                    DateTime begin,
                                                                    IModification mainModification,
                                                                    bool partOfDetectionAnalysis);

    /// <summary>
    /// Create a new OperationMachineAssociation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                    UtcDateTimeRange range);

    /// <summary>
    /// Create a new OperationMachineAssociation with a main modification that is used for the logs for example
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <returns></returns>
    IOperationMachineAssociation CreateOperationMachineAssociation (IMachine machine,
                                                                    UtcDateTimeRange range,
                                                                    IModification mainModification,
                                                                    bool partOfDetectionAnalysis);

    /// <summary>
    /// Create an OperationSlot
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
    IOperationSlot CreateOperationSlot (IMachine machine,
                                        IOperation operation,
                                        IComponent component,
                                        IWorkOrder workOrder,
                                        ILine line,
                                        IManufacturingOrder manufacturingOrder,
                                        DateTime? day,
                                        IShift shift,
                                        UtcDateTimeRange range);

    /// <summary>
    /// Create an OperationSlotSplit
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IOperationSlotSplit CreateOperationSlotSplit (IMachine machine);

    /// <summary>
    /// Create an OperationCycleDeliverablePiece association
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <param name="deliverablePiece"></param>
    /// <returns></returns>
    IOperationCycleDeliverablePiece
      CreateOperationCycleDeliverablePiece (IDeliverablePiece deliverablePiece,
                                           IOperationCycle operationCycle);

    /// <summary>
    /// Create a new operation duration setting that is applicable from an optional date/time
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="from"></param>
    /// <returns></returns>
    IOperationDuration CreateOperationDuration (IOperation operation, DateTime? from = null);

    /// <summary>
    /// Create a new operation duration setting that is specific to an operation model and applicable from an optional date/time
    /// </summary>
    /// <param name="operationModel">not null</param>
    /// <param name="from"></param>
    /// <returns></returns>
    IOperationDuration CreateOperationDuration (IOperationModel operationModel, DateTime? from = null);

    /// <summary>
    /// Create an OperationSourceWorkPiece
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    IOperationSourceWorkPiece CreateOperationSourceWorkPiece (IOperation operation,
                                                              IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Create an OperationType from a name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IOperationType CreateOperationTypeFromName (string name);

    /// <summary>
    /// Create a package
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    IPackage CreatePackage (string identifyingName);

    /// <summary>
    /// Create a package / plugin association
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    IPackagePluginAssociation CreatePackagePluginAssociation (IPackage package, IPlugin plugin, string name);

    /// <summary>
    /// Create a new Part
    /// </summary>
    /// <param name="componentType"></param>
    /// <returns></returns>
    IPart CreatePart (IComponentType componentType);

    /// <summary>
    /// Create a Part from a component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IPart CreatePart (IComponent component);

    /// <summary>
    /// Create a Part from a project
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    IPart CreatePart (IProject project);

    /// <summary>
    /// Create a new Path
    /// 
    /// Deprecated, <see cref="CreatePath(IOperation)"/>
    /// </summary>
    /// <returns></returns>
    IPath CreatePath ();

    /// <summary>
    /// Create a new path
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IPath CreatePath (IOperation operation);

    /// <summary>
    /// Create a plugin
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <returns></returns>
    IPlugin CreatePlugin (string identifyingName);

    /// <summary>
    /// Create a new ProcessMachineStateTemplate
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IProcessMachineStateTemplate CreateProcessMachineStateTemplate (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Create a new ProductionAnalysisStatus
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IProductionAnalysisStatus CreateProductionAnalysisStatus (IMachine machine);

    /// <summary>
    /// Create a new ProductionInformationShift
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="checkedValue"></param>
    /// <returns></returns>
    IProductionInformationShift CreateProductionInformationShift (IMachine machine,
                                                                  DateTime day,
                                                                  IShift shift,
                                                                  IIntermediateWorkPiece intermediateWorkPiece,
                                                                  int checkedValue);

    /// <summary>
    /// Create a new project with a specified code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    IProject CreateProjectFromCode (string code);

    /// <summary>
    /// Create a new project with a specified name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IProject CreateProjectFromName (string name);

    /// <summary>
    /// Create a new ProjectComponentUpdate
    /// </summary>
    /// <param name="component"></param>
    /// <param name="oldProject"></param>
    /// <param name="newProject"></param>
    /// <returns></returns>
    IProjectComponentUpdate CreateProjectComponentUpdate (IComponent component, IProject oldProject, IProject newProject);

    /// <summary>
    /// Create a new ProductionState
    /// </summary>
    /// <param name="color">not null or empty</param>
    /// <returns></returns>
    IProductionState CreateProductionState (string color);

    /// <summary>
    /// Create a ProductionRateSummary
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    IProductionRateSummary CreateProductionRateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState);

    /// <summary>
    /// Create a ProductionStateSummary
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="productionState">not null</param>
    /// <returns></returns>
    IProductionStateSummary CreateProductionStateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IProductionState productionState);

    /// <summary>
    /// Create a new Reason
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <returns></returns>
    IReason CreateReason (IReasonGroup reasonGroup);

    /// <summary>
    /// Create a new ReasonMachineAssociation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IReasonMachineAssociation CreateReasonMachineAssociation (IMachine machine,
                                                              UtcDateTimeRange range);

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
    IReasonMachineAssociation CreateReasonMachineAssociation (IMachine machine,
                                                              LowerBound<DateTime> begin,
                                                              UpperBound<DateTime> end,
                                                              string dynamic);

    /// <summary>
    /// Create a new ReasonGroup
    /// </summary>
    /// <returns></returns>
    IReasonGroup CreateReasonGroup ();

    /// <summary>
    /// Create a new reason machine association
    /// </summary>
    /// <param name="reasonMachineAssociation">not null, with a not null reason and a kind=Auto or Manual</param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IReasonProposal CreateReasonProposal (IReasonMachineAssociation reasonMachineAssociation,
      UtcDateTimeRange range);

    /// <summary>
    /// Create a ReasonSelection
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    IReasonSelection CreateReasonSelection (IMachineMode machineMode,
                                           IMachineObservationState machineObservationState);

    /// <summary>
    /// Create a ReasonSelection
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <param name="detailsRequired"></param>
    /// <returns></returns>
    IReasonSelection CreateReasonSelection (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, bool detailsRequired);

    /// <summary>
    /// Create a ReasonSlot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonSlot CreateReasonSlot (IMachine machine,
                                  UtcDateTimeRange range);

    /// <summary>
    /// Create a ReasonSummary
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IReasonSummary CreateReasonSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IReason reason);

    /// <summary>
    /// Create a new refresh token for the specified user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    IRefreshToken CreateRefreshToken (IUser user, TimeSpan expiresIn);

    /// <summary>
    /// Create a Revision
    /// </summary>
    /// <returns></returns>
    IRevision CreateRevision ();

    /// <summary>
    /// Create a Role object given its name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IRole CreateRoleFromName (string name);

    /// <summary>
    /// Create a new Sequence
    /// 
    /// Deprecated, use <see cref="CreateSequence(string, IOperation, IPath)"/>
    /// </summary>
    /// <returns></returns>
    ISequence CreateSequence (String name);

    /// <summary>
    /// Create a new sequence
    /// </summary>
    /// <param name="name"></param>
    /// <param name="operation">not null</param>
    /// <param name="path">not null</param>
    /// <returns></returns>
    ISequence CreateSequence (string name, IOperation operation, IPath path);

    /// <summary>
    /// Create a new sequence at a given position for a specific operation model
    /// </summary>
    /// <param name="name"></param>
    /// <param name="operationModel">not null</param>
    /// <param name="pathNumber"></param>
    /// <param name="before">nullable</param>
    /// <param name="after">nullable</param>
    /// <returns></returns>
    ISequence CreateSequence (string name, IOperationModel operationModel, int pathNumber, ISequence before, ISequence after);

    /// <summary>
    /// Create a new sequence duration from an optional date/time
    /// </summary>
    /// <param name="sequenceOperationModel"></param>
    /// <param name="duration"></param>
    /// <param name="from">if null, consider now</param>
    /// <returns></returns>
    ISequenceDuration CreateSequenceDuration (ISequenceOperationModel sequenceOperationModel, TimeSpan duration, DateTime? from);

    /// <summary>
    /// Create a new sequence milestone
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    ISequenceMilestone CreateSequenceMilestone (IMachineModule machineModule);

    /// <summary>
    /// Associate a sequence to a new operation model
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <param name="operationModel">not null</param>
    /// <param name="before">nullable</param>
    /// <param name="after">nullable</param>
    /// <returns></returns>
    ISequenceOperationModel CreateSequenceOperationModel (ISequence sequence, IOperationModel operationModel, ISequence before, ISequence after);

    /// <summary>
    /// Create a SequenceSlot
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="sequence">nullable</param>
    /// <param name="range"></param>
    /// <returns></returns>
    ISequenceSlot CreateSequenceSlot (IMachineModule machineModule,
                                      ISequence sequence,
                                      UtcDateTimeRange range);

    /// <summary>
    /// Create a SerialNumberMachineStamp object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="serialNumber"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    ISerialNumberMachineStamp CreateSerialNumberMachineStamp (IMachine machine,
                                                              string serialNumber,
                                                              DateTime dateTime);

    /// <summary>
    /// Create a SerialNumberModification object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="serialNumber"></param>
    /// <param name="beginOrEndDateTime"></param>
    /// <param name="isBegin"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    ISerialNumberModification CreateSerialNumberModification (IMachine machine,
                                                             string serialNumber,
                                                             DateTime beginOrEndDateTime,
                                                             bool isBegin,
                                                             DateTime dateTime);

    /// <summary>
    /// Create a new service
    /// </summary>
    /// <param name="computer">not null</param>
    /// <param name="name">not null or empty</param>
    /// <param name="program">not null or empty</param>
    /// <param name="lemoine"></param>
    /// <returns></returns>
    IService CreateService (IComputer computer, string name, string program, bool lemoine);

    /// <summary>
    /// Create a new Shift from a specified name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IShift CreateShiftFromName (string name);

    /// <summary>
    /// Create a new Shift from a specified code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    IShift CreateShiftFromCode (string code);

    /// <summary>
    /// Create a new shiftslot
    /// </summary>
    /// <param name="shiftTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IShiftSlot CreateShiftSlot (IShiftTemplate shiftTemplate,
                                UtcDateTimeRange range);

    /// <summary>
    /// Create a ShiftTemplate
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IShiftTemplate CreateShiftTemplate (string name);

    /// <summary>
    /// Create a ShiftTemplateAssociation
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="beginDateTime"></param>
    /// <returns></returns>
    IShiftTemplateAssociation CreateShiftTemplateAssociation (IShiftTemplate shiftTemplate,
                                                              DateTime beginDateTime);

    /// <summary>
    /// Create a new SimpleOperation
    /// </summary>
    /// <param name="operationType"></param>
    /// <returns></returns>
    ISimpleOperation CreateSimpleOperation (IOperationType operationType);

    /// <summary>
    /// Create a new SimpleOperation from an Operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    ISimpleOperation CreateSimpleOperation (IOperation operation);

    /// <summary>
    /// Create a new SimpleOperation from an IntermediateWorkPiece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    ISimpleOperation CreateSimpleOperation (IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Create a new Stamp
    /// </summary>
    /// <returns></returns>
    IStamp CreateStamp ();

    /// <summary>
    /// Create a new <see cref="IStampingConfigByName"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IStampingConfigByName CreateStampingConfigByName (string name);

    /// <summary>
    /// Create a new StampingValue
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    IStampingValue CreateStampingValue (ISequence sequence, IField field);

    /// <summary>
    /// Create a new SynchronizationLog
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="xmlElement"></param>
    /// <returns></returns>
    ISynchronizationLog CreateSynchronizationLog (LogLevel level,
                                                  string message,
                                                  string xmlElement);

    /// <summary>
    /// Create a manufacturing order
    /// </summary>
    /// <param name="manufacturingOrderId"></param>
    /// <returns></returns>
    IManufacturingOrder CreateManufacturingOrder (int manufacturingOrderId);

    /// <summary>
    /// Create a new manufacturing order machine association modification
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IManufacturingOrderMachineAssociation CreateManufacturingOrderMachineAssociation (IMachine machine,
                                                         IManufacturingOrder manufacturingOrder,
                                                         UtcDateTimeRange range);

    /// <summary>
    /// Create a new toollife
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="position"></param>
    /// <param name="unit"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    IToolLife CreateToolLife (IMachineModule machineModule, IToolPosition position,
                             IUnit unit, Lemoine.Core.SharedData.ToolLifeDirection direction);

    /// <summary>
    /// Create a new ToolPosition
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="toolId"></param>
    /// <returns></returns>
    IToolPosition CreateToolPosition (IMachineModule machineModule, string toolId);

    /// <summary>
    /// Create a new Translation
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    ITranslation CreateTranslation (string locale, string translationKey);

    /// <summary>
    /// Create a new Unit
    /// </summary>
    /// <returns></returns>
    IUnit CreateUnit ();

    /// <summary>
    /// Create a User
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password">nullable</param>
    /// <returns></returns>
    IUser CreateUser (string login, string password);

    /// <summary>
    /// Create a new UserAttendance
    /// </summary>
    /// <param name="user">Not null</param>
    /// <returns></returns>
    IUserAttendance CreateUserAttendance (IUser user);

    /// <summary>
    /// Create a new UserMachineAssociation
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IUserMachineAssociation CreateUserMachineAssociation (IUser user, UtcDateTimeRange range);

    /// <summary>
    /// Create a new UserMachineSlot
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IUserMachineSlot CreateUserMachineSlot (IUser user, UtcDateTimeRange range);

    /// <summary>
    /// Create a new UserShiftAssociation
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    IUserShiftAssociation CreateUserShiftAssociation (IUser user, UtcDateTimeRange range, IShift shift);

    /// <summary>
    /// Create a new UserShiftSlot
    /// </summary>
    /// <param name="user">Not null</param>
    /// <param name="range"></param>
    /// <param name="shift">Not null</param>
    /// <returns></returns>
    IUserShiftSlot CreateUserShiftSlot (IUser user, UtcDateTimeRange range, IShift shift);

    /// <summary>
    /// Create a new UserSlot
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IUserSlot CreateUserSlot (IUser user, UtcDateTimeRange range);

    /// <summary>
    /// Create a new WorkOrder
    /// </summary>
    /// <param name="workOrderStatus">not null</param>
    /// <param name="workOrderName">not null or empty</param>
    /// <returns></returns>
    IWorkOrder CreateWorkOrder (IWorkOrderStatus workOrderStatus, string workOrderName);

    /// <summary>
    /// Create a new work order from its code
    /// </summary>
    /// <param name="workOrderStatus">not null</param>
    /// <param name="workOrderCode">not null or empty</param>
    /// <returns></returns>
    IWorkOrder CreateWorkOrderFromCode (IWorkOrderStatus workOrderStatus, string workOrderCode);

    /// <summary>
    /// Create a new WorkOrderLine
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    IWorkOrderLine CreateWorkOrderLine (ILine line, UtcDateTimeRange range, IWorkOrder workOrder);

    /// <summary>
    /// Create a new WorkOrderLineAssociation
    /// </summary>
    /// <param name="line"></param>
    /// <param name="beginTime"></param>
    /// <param name="deadline"></param>
    /// <returns></returns>
    IWorkOrderLineAssociation CreateWorkOrderLineAssociation (ILine line, DateTime beginTime, DateTime deadline);

    /// <summary>
    /// Create a new WorkOrderMachineAssociation
    ///
    /// For the unit tests only
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="beginTime"></param>
    /// <returns></returns>
    IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation (IMachine machine,
                                                                   IWorkOrder workOrder,
                                                                   DateTime beginTime);
    /// <summary>
    /// Create a new WorkOrderMachineAssociation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    IWorkOrderMachineAssociation CreateWorkOrderMachineAssociation (IMachine machine,
                                                                   IWorkOrder workOrder,
                                                                   UtcDateTimeRange range);

    /// <summary>
    /// Create a WorkOrderMachineStamp object
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="workOrder"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IWorkOrderMachineStamp CreateWorkOrderMachineStamp (IMachine machine,
                                                        IWorkOrder workOrder,
                                                        DateTime dateTime);


    /// <summary>
    /// Create a new WorkOrderProject
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    IWorkOrderProject CreateWorkOrderProject (IWorkOrder workOrder, IProject project);

    /// <summary>
    /// Create a new WorkOrderProjectUpdate
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IWorkOrderProjectUpdate CreateWorkOrderProjectUpdate (IWorkOrder workOrder, IProject project, WorkOrderProjectUpdateModificationType type);

    /// <summary>
    /// Create a new WorkOrderStatus
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IWorkOrderStatus CreateWorkOrderStatusFromName (string name);
  }
}
