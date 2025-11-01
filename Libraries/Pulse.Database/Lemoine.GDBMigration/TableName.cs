// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Business.Reason;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Table name constants
  /// </summary>
  public class TableName
  {
    /// <summary>
    /// analysissatus table name
    /// </summary>
    public static readonly string ACQUISITION_STATE = "acquisitionstate";
    /// <summary>
    /// analysissatus table name
    /// </summary>
    public static readonly string ANALYSIS_STATUS = "analysisstatus";
    /// <summary>
    /// applicationstate table name
    /// </summary>
    public static readonly string APPLICATION_STATE = "applicationstate";
    /// <summary>
    /// autoreasonstate table name
    /// </summary>
    public static readonly string AUTO_REASON_STATE = "autoreasonstate";
    /// <summary>
    /// config table name
    /// </summary>
    public static readonly string CONFIG = "config";
    /// <summary>
    /// namedconfig table name
    /// </summary>
    public static readonly string NAMED_CONFIG = "namedconfig";
    /// <summary>
    /// translation table name
    /// </summary>
    public static readonly string TRANSLATION = "translation";
    /// <summary>
    /// display table name
    /// </summary>
    public static readonly string DISPLAY = "display";
    /// <summary>
    /// computer table name
    /// </summary>
    public static readonly string COMPUTER = "computer";
    /// <summary>
    /// EmailConfig table name
    /// </summary>
    public static readonly string EMAIL_CONFIG = "EmailConfig";
    /// <summary>
    /// machinemonitoringtype table name
    /// </summary>
    public static readonly string MACHINE_MONITORING_TYPE = "machinemonitoringtype";
    /// <summary>
    /// machine table name
    /// </summary>
    public static readonly string MACHINE = "machine";
    /// <summary>
    /// monitoredmachine table name
    /// </summary>
    public static readonly string MONITORED_MACHINE = "monitoredmachine";
    /// <summary>
    /// machinemodule table name
    /// </summary>
    public static readonly string MACHINE_MODULE = "machinemodule";
    /// <summary>
    /// cncacquisition table name
    /// </summary>
    public static readonly string CNC_ACQUISITION = "cncacquisition";
    /// <summary>
    /// deliverablepiece table name
    /// </summary>
    public static readonly string DELIVERABLE_PIECE = "deliverablepiece";
    /// <summary>
    /// operationcycledeliverablepiece table name
    /// </summary>
    public static readonly string OPERATION_CYCLE_DELIVERABLE_PIECE = "operationcycledeliverablepiece";
    
    /// <summary>
    /// deliverablepiecemachineassociation table name
    /// </summary>
    public static readonly string DELIVERABLE_PIECE_MACHINE_ASSOCIATION = "deliverablepiecemachineassociation";

    /// <summary>
    /// workordermachinestamp table name
    /// </summary>
    public static readonly string WORK_ORDER_MACHINE_STAMP = "workordermachinestamp";

    /// <summary>
    /// serialnumbermachinestamp table name
    /// </summary>
    public static readonly string SERIAL_NUMBER_MACHINE_STAMP = "serialnumbermachinestamp";
    
    /// <summary>
    /// serialnumbermodification table name
    /// </summary>
    public static readonly string SERIAL_NUMBER_MODIFICATION = "serialnumbermodification";
    
    /// <summary>
    /// department table name
    /// </summary>
    public static readonly string DEPARTMENT = "department";
    /// <summary>
    /// company table name
    /// </summary>
    public static readonly string COMPANY = "company";
    /// <summary>
    /// cell table name
    /// </summary>
    public static readonly string CELL = "cell";
    /// <summary>
    /// machinecategory table name
    /// </summary>
    public static readonly string MACHINE_CATEGORY = "machinecategory";
    /// <summary>
    /// machinesubcategory table name
    /// </summary>
    public static readonly string MACHINE_SUB_CATEGORY = "machinesubcategory";
    /// <summary>
    /// shopfloordisplay table name
    /// </summary>
    public static readonly string SHOP_FLOOR_DISPLAY = "shopfloordisplay";
    /// <summary>
    /// machinefilter table name
    /// </summary>
    public static readonly string MACHINE_FILTER = "machinefilter";
    /// <summary>
    /// machinefilteritem table name
    /// </summary>
    public static readonly string MACHINE_FILTER_ITEM = "machinefilteritem";
    /// <summary>
    /// machine mode category table name
    /// </summary>
    public static readonly string MACHINE_MODE_CATEGORY = "machinemodecategory";
    /// <summary>
    /// machine mode table name
    /// </summary>
    public static readonly string MACHINE_MODE = "machinemode";
    /// <summary>
    /// workorderstatus table name
    /// </summary>
    public static readonly string WORK_ORDER_STATUS = "workorderstatus";
    /// <summary>
    /// workorder table name
    /// </summary>
    public static readonly string WORK_ORDER = "workorder";
    /// <summary>
    /// project table name
    /// </summary>
    public static readonly string PROJECT = "project";
    /// <summary>
    /// WorkOrderProject table name
    /// </summary>
    public static readonly string WORK_ORDER_PROJECT = "workorderproject";
    /// <summary>
    /// component type table name
    /// </summary>
    public static readonly string COMPONENT_TYPE = "componenttype";
    /// <summary>
    /// component table name
    /// </summary>
    public static readonly string COMPONENT = "component";
    /// <summary>
    /// customer table
    /// </summary>
    public static readonly string CUSTOMER = "customer";
    /// <summary>
    /// intermediateworkpiece table name
    /// </summary>
    public static readonly string INTERMEDIATE_WORK_PIECE = "intermediateworkpiece";
    /// <summary>
    /// componentintermediateworkpiece table name
    /// </summary>
    public static readonly string COMPONENT_INTERMEDIATE_WORK_PIECE = "componentintermediateworkpiece";
    /// <summary>
    /// operationtype table name
    /// </summary>
    public static readonly string OPERATION_TYPE = "operationtype";
    /// <summary>
    /// operation table name
    /// </summary>
    public static readonly string OPERATION = "operation";
    /// <summary>
    /// operation information table name
    /// </summary>
    public static readonly string OPERATION_INFORMATION = "operationinformation";
    /// <summary>
    /// old sequence table name
    /// </summary>
    public static readonly string OLD_SEQUENCE = "process";
    /// <summary>
    /// new sequence table name
    /// </summary>
    public static readonly string SEQUENCE = "sequence";
    /// <summary>
    /// old path table name
    /// </summary>
    public static readonly string PATH_OLD = "path";
    /// <summary>
    /// path table name
    /// </summary>
    public static readonly string PATH = "pathtable";
    /// <summary>
    /// CAD Model table name
    /// </summary>
    public static readonly string CAD_MODEL = "cadmodel";
    /// <summary>
    /// ISO File table name
    /// </summary>
    public static readonly string ISO_FILE = "isofile";
    /// <summary>
    /// Tool table name
    /// </summary>
    public static readonly string TOOL = "tool";
    /// <summary>
    /// Tool life table name
    /// </summary>
    public static readonly string TOOL_DATA_ACQUISITION = "tooldataacquisition";
    /// <summary>
    /// Tool position table name
    /// </summary>
    public static readonly string TOOL_POSITION = "toolposition";
    /// <summary>
    /// Tool life table name
    /// </summary>
    public static readonly string TOOL_LIFE = "toollife";
    /// <summary>
    /// Event tool life table name
    /// </summary>
    public static readonly string EVENT_TOOL_LIFE = "eventtoollife";
    /// <summary>
    /// Event tool life config table name
    /// </summary>
    public static readonly string EVENT_TOOL_LIFE_CONFIG = "eventtoollifeconfig";
    /// <summary>
    /// Unit table name
    /// </summary>
    public static readonly string UNIT = "unit";
    /// <summary>
    /// Field table name
    /// </summary>
    public static readonly string FIELD = "field";
    /// <summary>
    /// FieldLegend table name
    /// </summary>
    public static readonly string FIELD_LEGEND = "fieldlegend";
    /// <summary>
    /// Stamping Values table name
    /// </summary>
    public static readonly string STAMPING_VALUE = "stampingvalue";
    /// <summary>
    /// Stamp table name
    /// </summary>
    public static readonly string STAMP = "stamp";
    /// <summary>
    /// NCProgramCode table name
    /// </summary>
    public static readonly string NC_PROGRAM_CODE = "ncprogramcode";
    /// <summary>
    /// machineobservationstate table name
    /// </summary>
    public static readonly string MACHINE_OBSERVATION_STATE = "machineobservationstate";
    /// <summary>
    /// machinestatetemplate table name
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE = "machinestatetemplate";
    /// <summary>
    /// machinestatetemplatecategory table name
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_CATEGORY = "machinestatetemplatecategory";
    /// <summary>
    /// machinestatetemplateitem table name
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_ITEM = "machinestatetemplateitem";
    /// <summary>
    /// machinestatetemplatestop table name
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_STOP = "machinestatetemplatestop";
    /// <summary>
    /// machinestatetemplateflow
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_FLOW = "machinestatetemplateflow";
    /// <summary>
    /// automachinestatetemplate
    /// </summary>
    public static readonly string AUTO_MACHINE_STATE_TEMPLATE = "automachinestatetemplate";
    /// <summary>
    /// updater table name
    /// </summary>
    public static readonly string UPDATER = "updater";
    /// <summary>
    /// user table name
    /// </summary>
    public static readonly string USER = "usertable";
    /// <summary>
    /// productionstate table name
    /// </summary>
    public static readonly string PRODUCTION_STATE = "productionstate";
    /// <summary>
    /// reason table name
    /// </summary>
    public static readonly string REASON = "reason";
    /// <summary>
    /// reason group table name
    /// </summary>
    public static readonly string REASON_GROUP = "reasongroup";
    /// <summary>
    /// machinemodefaultreason table name
    /// </summary>
    public static readonly string MACHINE_MODE_DEFAULT_REASON = "machinemodedefaultreason";
    /// <summary>
    /// reasonselection table name
    /// </summary>
    public static readonly string REASON_SELECTION = "reasonselection";
    /// <summary>
    /// modification table name
    /// </summary>
    public static readonly string MODIFICATION = "modification";
    /// <summary>
    /// modificationstatus table name
    /// </summary>
    public static readonly string MODIFICATION_STATUS = "modificationstatus";
    /// <summary>
    /// modification table name
    /// </summary>
    public static readonly string GLOBAL_MODIFICATION = "globalmodification";
    /// <summary>
    /// modificationstatus table name
    /// </summary>
    public static readonly string GLOBAL_MODIFICATION_STATUS = "globalmodificationstatus";
    /// <summary>
    /// modification table name
    /// </summary>
    public static readonly string MACHINE_MODIFICATION = "machinemodification";
    /// <summary>
    /// modificationstatus table name
    /// </summary>
    public static readonly string MACHINE_MODIFICATION_STATUS = "machinemodificationstatus";
    /// <summary>
    /// revision table name
    /// </summary>
    public static readonly string REVISION = "revision";
    /// <summary>
    /// old modification table processdetection (now sequencedetection) name
    /// This is a temporary table to use until the stamping detection table is operational
    /// </summary>
    public static readonly string OLD_SEQUENCE_DETECTION = "processdetection";
    /// <summary>
    /// Obsolete modification table sequencedetection (previously processdetection) name
    /// This was a temporary table to use until the stamping detection table is operational
    /// </summary>
    public static readonly string SEQUENCE_DETECTION = "sequencedetection";
    /// <summary>
    /// Obsolete modification table stampdetection name
    /// </summary>
    public static readonly string STAMP_DETECTION = "stampdetection";
    /// <summary>
    /// Obsolete modification table isofileenddetection name
    /// </summary>
    public static readonly string ISO_FILE_END_DETECTION = "isofileenddetection";
    /// <summary>
    /// modification table operationcycleinformation name
    /// </summary>
    public static readonly string OPERATION_CYCLE_INFORMATION = "operationcycleinformation";
    /// <summary>
    /// modification table operationcycleperiod name
    /// </summary>
    public static readonly string OPERATION_CYCLE_PERIOD = "operationcycleperiod";
    /// <summary>
    /// Obsolete modification table activitydetection (from the machine)
    /// </summary>
    public static readonly string ACTIVITY_DETECTION = "activitydetection";
    /// <summary>
    /// modification table activitymanual
    /// </summary>
    public static readonly string ACTIVITY_MANUAL = "activitymanual";
    /// <summary>
    /// modification table linkoperation
    /// </summary>
    public static readonly string LINK_OPERATION = "linkoperation";
    /// <summary>
    /// modification table userattendance
    /// </summary>
    public static readonly string USER_ATTENDANCE = "userattendance";
    /// <summary>
    /// modification table machineobservationstateassociation
    /// </summary>
    public static readonly string MACHINE_OBSERVATION_STATE_ASSOCIATION = "machineobservationstateassociation";
    /// <summary>
    /// modification table machinestatetemplateassociation
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_ASSOCIATION = "machinestatetemplateassociation";
    /// <summary>
    /// modification table processmachinestatetemplate
    /// </summary>
    public static readonly string PROCESS_MACHINE_STATE_TEMPLATE = "processmachinestatetemplate";
    /// <summary>
    /// modification table reasonmachineassociation
    /// </summary>
    public static readonly string REASON_MACHINE_ASSOCIATION = "reasonmachineassociation";
    /// <summary>
    /// helper table for the reasonmachineassociation table with the effective date/time range
    /// </summary>
    public static readonly string REASON_PROPOSAL = "reasonproposal";
    /// <summary>
    /// modification table shiftmachineassociation
    /// </summary>
    public static readonly string SHIFT_MACHINE_ASSOCIATION = "shiftmachineassociation";
    /// <summary>
    /// modification table productioninformationshift
    /// </summary>
    public static readonly string PRODUCTION_INFORMATION_SHIFT = "productioninformationshift";
    /// <summary>
    /// modification table productioninformationdatetime
    /// </summary>
    public static readonly string PRODUCTION_INFORMATION = "productioninformationdatetime";
    /// <summary>
    /// log table cncdataimportlog
    /// </summary>
    public static readonly string CNC_DATA_IMPORT_LOG = "cncdataimportlog";
    /// <summary>
    /// log table analysislog
    /// </summary>
    public static readonly string ANALYSIS_LOG = "analysislog";
    /// <summary>
    /// deprecated log table analysislog, removed in migration 1002
    /// </summary>
    public static readonly string OLD_ANALYSIS_LOG = "oldanalysislog";
    /// <summary>
    /// global modification log
    /// </summary>
    public static readonly string GLOBAL_MODIFICATION_LOG = "globalmodificationlog";
    /// <summary>
    /// machine modification log
    /// </summary>
    public static readonly string MACHINE_MODIFICATION_LOG = "machinemodificationlog";
    /// <summary>
    /// log table detectionanalysislog
    /// </summary>
    public static readonly string DETECTION_ANALYSIS_LOG = "detectionanalysislog";
    /// <summary>
    /// log table maintenancelog
    /// </summary>
    public static readonly string MAINTENANCE_LOG = "maintenancelog";
    /// <summary>
    /// log table synchronizationlog
    /// </summary>
    public static readonly string SYNCHRONIZATION_LOG = "synchronizationlog";
    /// <summary>
    /// old machinemodulestatus table name
    /// </summary>
    public static readonly string OLD_MACHINE_MODULE_STATUS = "machinemodulestatus";
    /// <summary>
    /// monitoredmachineanalysisstatus table name
    /// </summary>
    public static readonly string MONITORED_MACHINE_ANALYSIS_STATUS = "monitoredmachineanalysisstatus";
    /// <summary>
    /// old autosequence = autoprocess table name
    /// </summary>
    public static readonly string OLD_AUTO_SEQUENCE = "autoprocess";
    /// <summary>
    /// new autosequence (previously autoprocess) table name
    /// </summary>
    public static readonly string AUTO_SEQUENCE = "autosequence";
    /// <summary>
    /// machinemoduledetection table name
    /// </summary>
    public static readonly string MACHINE_MODULE_DETECTION = "machinemoduledetection";
    /// <summary>
    /// detectiontimestamp table name
    /// </summary>
    public static readonly string DETECTION_TIMESTAMP = "detectiontimestamp";
    /// <summary>
    /// sequenceslot table name
    /// </summary>
    public static readonly string SEQUENCE_SLOT = "sequenceslot";
    /// <summary>
    /// operationcycle table name
    /// </summary>
    public static readonly string OPERATION_CYCLE = "operationcycle";
    /// <summary>
    /// betweencycles table name
    /// </summary>
    internal static readonly string BETWEEN_CYCLES = "betweencycles";
    /// <summary>
    /// machinestatus table name
    /// </summary>
    public static readonly string MACHINE_STATUS = "machinestatus";
    /// <summary>
    /// operationslotsplit table name
    /// </summary>
    public static readonly string OPERATION_SLOT_SPLIT = "operationslotsplit";
    /// <summary>
    /// machinemoduleanalysisstatus table name
    /// </summary>
    public static readonly string MACHINE_MODULE_ANALYSIS_STATUS = "machinemoduleanalysisstatus";
    /// <summary>
    /// analysis table machinemoduleactivitysummary
    /// </summary>
    public static readonly string MACHINE_ACTIVITY_SUMMARY = "machineactivitysummary";
    /// <summary>
    /// analysis table userslot
    /// </summary>
    public static readonly string USER_SLOT = "userslot";
    /// <summary>
    /// analysis table observationstateslot
    /// </summary>
    public static readonly string OBSERVATION_STATE_SLOT = "observationstateslot";
    /// <summary>
    /// analysis table reasonslot
    /// </summary>
    public static readonly string REASON_SLOT = "reasonslot";
    /// <summary>
    /// analysis table productionratesummary
    /// </summary>
    public static readonly string PRODUCTION_RATE_SUMMARY = "productionratesummary";
    /// <summary>
    /// analysis table productionstatesummary
    /// </summary>
    public static readonly string PRODUCTION_STATE_SUMMARY = "productionstatesummary";
    /// <summary>
    /// analysis table reasonsummary
    /// </summary>
    public static readonly string REASON_SUMMARY = "reasonsummary";
   
    /// <summary>
    /// analysis table workorderslot
    /// </summary>
    public static readonly string WORK_ORDER_SLOT = "workorderslot";
    /// <summary>
    /// analysis table operationslot
    /// </summary>
    public static readonly string OPERATION_SLOT = "operationslot";
    
    /// <summary>
    /// analysis table intermediateworkpiecesummary
    /// </summary>
    public static readonly string INTERMEDIATE_WORK_PIECE_SUMMARY = "intermediateworkpiecesummary";
    /// <summary>
    /// analysis table intermediateworkpiecetarget
    /// </summary>
    public static readonly string INTERMEDIATE_WORK_PIECE_TARGET = "intermediateworkpiecetarget";
    /// <summary>
    /// analysis table iwpbymachinesummary
    /// </summary>
    public static readonly string INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY = "iwpbymachinesummary";
    
    /// <summary>
    /// analysis table cycledurationsummary (now in a specific plugin)
    /// </summary>
    public static readonly string CYCLE_DURATION_SUMMARY = "cycledurationsummary";
    /// <summary>
    /// analysis table cyclecountsummary (now in a specific plugin)
    /// </summary>
    public static readonly string CYCLE_COUNT_SUMMARY = "cyclecountsummary";
    
    /// <summary>
    /// event table eventlevel
    /// </summary>
    public static readonly string EVENT_LEVEL = "eventlevel";
    /// <summary>
    /// event config table eventlongperiodconfig
    /// </summary>
    public static readonly string EVENT_LONG_PERIOD_CONFIG = "eventlongperiodconfig";
    /// <summary>
    /// event table eventlongperiod
    /// </summary>
    public static readonly string EVENT_LONG_PERIOD = "eventlongperiod";
    /// <summary>
    /// event config table eventcncvalueconfig
    /// </summary>
    public static readonly string EVENT_CNC_VALUE_CONFIG = "eventcncvalueconfig";
    /// <summary>
    /// event table eventcncvalue
    /// </summary>
    public static readonly string EVENT_CNC_VALUE = "eventcncvalue";
    /// <summary>
    /// abstract table event
    /// </summary>
    public static readonly string EVENT = "event";
    /// <summary>
    /// abstract event table eventmachine
    /// </summary>
    public static readonly string EVENT_MACHINE = "eventmachine";
    /// <summary>
    /// generic event table eventmachinegeneric
    /// </summary>
    public static readonly string EVENT_MACHINE_GENERIC = "eventmachinegeneric";
    /// <summary>
    /// abstract event table eventmachinemodule
    /// </summary>
    public static readonly string EVENT_MACHINE_MODULE = "eventmachinemodule";
    /// <summary>
    /// generic event table eventmachinemodule
    /// </summary>
    public static readonly string EVENT_MACHINE_MODULE_GENERIC = "eventmachinemodulegeneric";
    
    /// <summary>
    /// new fact table
    /// </summary>
    public static readonly string FACT = "fact";
    /// <summary>
    /// machinemoduleactivity table name
    /// </summary>
    public static readonly string MACHINE_MODULE_ACTIVITY = "machinemoduleactivity";
    /// <summary>
    /// cncvalue table name
    /// </summary>
    public static readonly string CNC_VALUE = "cncvalue";
    
    /// <summary>
    /// currentcncvalue table name
    /// </summary>
    public static readonly string CURRENT_CNC_VALUE = "currentcncvalue";
    /// <summary>
    /// currentmachinemode table name
    /// </summary>
    public static readonly string CURRENT_MACHINE_MODE = "currentmachinemode";
    
    /// <summary>
    /// shift table name
    /// </summary>
    public static readonly string SHIFT = "shift";
    /// <summary>
    /// shiftchange table name
    /// </summary>
    public static readonly string SHIFT_CHANGE = "shiftchange";
    /// <summary>
    /// shiftslot table name
    /// </summary>
    public static readonly string SHIFT_SLOT = "shiftslot";
    /// <summary>
    /// shiftslotbreak table name
    /// </summary>
    public static readonly string SHIFT_SLOT_BREAK = "shiftslotbreak";
    /// <summary>
    /// shifttemplate table name
    /// </summary>
    public static readonly string SHIFT_TEMPLATE = "shifttemplate";
    /// <summary>
    /// shifttemplateitem table name
    /// </summary>
    public static readonly string SHIFT_TEMPLATE_ITEM = "shifttemplateitem";
    /// <summary>
    /// shifttemplatebreak table name
    /// </summary>
    public static readonly string SHIFT_TEMPLATE_BREAK = "shifttemplatebreak";
    /// <summary>
    /// shifttemplateassociation table name
    /// </summary>
    public static readonly string SHIFT_TEMPLATE_ASSOCIATION = "shifttemplateassociation";
    /// <summary>
    /// shifttemplateslot table name
    /// </summary>
    public static readonly string SHIFT_TEMPLATE_SLOT = "shifttemplateslot";
    /// <summary>
    /// machinecompanyupdate table name
    /// </summary>
    public static readonly string MACHINE_COMPANY_UPDATE = "machinecompanyupdate";
    /// <summary>
    /// machinedepartmentupdate table name
    /// </summary>
    public static readonly string MACHINE_DEPARTMENT_UPDATE = "machinedepartmentupdate";
    /// <summary>
    /// machinecellupdate table name
    /// </summary>
    public static readonly string MACHINE_CELL_UPDATE = "machinecellupdate";
    /// <summary>
    /// projectcomponentupdate table name
    /// </summary>
    public static readonly string PROJECT_COMPONENT_UPDATE = "projectcomponentupdate";
    
    /// <summary>
    /// operationmachineassociation table
    /// </summary>
    public static readonly string OPERATION_MACHINE_ASSOCIATION = "operationmachineassociation";
    /// <summary>
    /// componentmachineassociation table
    /// </summary>
    public static readonly string COMPONENT_MACHINE_ASSOCIATION = "componentmachineassociation";
    /// <summary>
    /// workordermachineassociation table
    /// </summary>
    public static readonly string WORKORDER_MACHINE_ASSOCIATION = "workordermachineassociation";
    
    /// <summary>
    /// usershiftassociation table
    /// </summary>
    public static readonly string USER_SHIFT_ASSOCIATION = "usershiftassociation";
    /// <summary>
    /// usermachineassociation table
    /// </summary>
    public static readonly string USER_MACHINE_ASSOCIATION = "usermachineassociation";
    /// <summary>
    /// usermachineassociationitem table
    /// </summary>
    public static readonly string USER_MACHINE_ASSOCIATION_MACHINE = "usermachineassociationmachine";
    /// <summary>
    /// usershiftslot table
    /// </summary>
    public static readonly string USER_SHIFT_SLOT = "usershiftslot";
    /// <summary>
    /// usermachineslot table
    /// </summary>
    public static readonly string USER_MACHINE_SLOT = "usermachineslot";
    /// <summary>
    /// usermachineslotmachine table
    /// </summary>
    public static readonly string USER_MACHINE_SLOT_MACHINE = "usermachineslotmachine";
    /// <summary>
    /// role table
    /// </summary>
    public static readonly string ROLE = "role";
    /// <summary>
    /// machinestatetemplateright table
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_RIGHT = "machinestatetemplateright";
    /// <summary>
    /// nonconformancereason table
    /// </summary>
    public static readonly string NON_CONFORMANCE_REASON = "nonconformancereason";
    /// <summary>
    /// nonconformancereport table
    /// </summary>
    public static readonly string NON_CONFORMANCE_REPORT = "nonconformancereport";
    /// <summary>
    /// ISO File slot table name
    /// </summary>
    public static readonly string ISO_FILE_SLOT = "isofileslot";
    /// <summary>
    /// Old Line table name
    /// </summary>
    public static readonly string LINE_OLD = "line";
    /// <summary>
    /// Line table name
    /// </summary>
    public static readonly string LINE = "linetable";
    /// <summary>
    /// LineMachine table name
    /// </summary>
    public static readonly string LINE_MACHINE = "linemachine";
    /// <summary>
    /// LineComponent table name
    /// </summary>
    public static readonly string LINE_COMPONENT = "linecomponent";
    /// <summary>
    /// OperationSourceWorkPiece table name
    /// </summary>
    public static readonly string OPERATION_SOURCE_WORKPIECE = "operationsourceworkpiece";
    /// <summary>
    /// WorkOrderLine table name
    /// </summary>
    public static readonly string WORK_ORDER_LINE = "workorderline";
    /// <summary>
    /// WorkOrderLineQuantity table name
    /// </summary>
    public static readonly string WORK_ORDER_LINE_QUANTITY = "workorderlinequantity";
    /// <summary>
    /// WorkOrderLineAssociation table name
    /// </summary>
    public static readonly string WORK_ORDER_LINE_ASSOCIATION = "workorderlineassociation";
    /// <summary>
    /// WorkOrderLineAssociationQuantity table name
    /// </summary>
    public static readonly string WORK_ORDER_LINE_ASSOCIATION_QUANTITY = "workorderlineassociationquantity";
    /// <summary>
    /// daytemplate table name
    /// </summary>
    public static readonly string DAY_TEMPLATE = "daytemplate";
    /// <summary>
    /// daytemplateitem table name
    /// </summary>
    public static readonly string DAY_TEMPLATE_ITEM = "daytemplateitem";
    /// <summary>
    /// daytemplateslot table name
    /// </summary>
    public static readonly string DAY_TEMPLATE_SLOT = "daytemplateslot";
    /// <summary>
    /// dayslot table name
    /// </summary>
    public static readonly string DAY_SLOT = "dayslot";
    /// <summary>
    /// daytemplatechange table name
    /// </summary>
    public static readonly string DAY_TEMPLATE_CHANGE = "daytemplatechange";
    /// <summary>
    /// goal table name
    /// </summary>
    public static readonly string GOAL = "goal";
    /// <summary>
    /// goaltype table name
    /// </summary>
    public static readonly string GOALTYPE = "goaltype";
    /// <summary>
    /// Deprecated task view name (now manuforder)
    /// </summary>
    public static readonly string TASK = "task";
    /// <summary>
    /// Deprecated full implementation of the task view (now manuforder1)
    /// </summary>
    public static readonly string TASK_FULL = "taskfull";
    /// <summary>
    /// Deprecated taskstatus table name (now manuforderstatus)
    /// </summary>
    public static readonly string TASK_STATUS = "taskstatus";
    /// <summary>
    /// Deprecated taskmachineassociation modification table name (now manufordermachineassociation)
    /// </summary>
    public static readonly string TASK_MACHINE_ASSOCIATION = "taskmachineassociation";
    /// <summary>
    /// manuforder (manufacturing order) view name
    /// </summary>
    public static readonly string MANUFACTURING_ORDER = "manuforder";
    /// <summary>
    /// Full implementation of the manuforder view
    /// </summary>
    public static readonly string MANUFACTURING_ORDER_IMPLEMENTATION = "manuforder1";
    /// <summary>
    /// manuforderstatus table name
    /// </summary>
    public static readonly string MANUFACTURING_ORDER_STATUS = "manuforderstatus";
    /// <summary>
    /// manufordermachineassociation modification table name
    /// </summary>
    public static readonly string MANUFACTURING_ORDER_MACHINE_ASSOCIATION = "manufordermachineassociation";
    /// <summary>
    /// productionanalysisstatus table name
    /// </summary>
    public static readonly string PRODUCTION_ANALYSIS_STATUS = "productionanalysisstatus";
    /// <summary>
    /// plugin table
    /// </summary>
    public static readonly string PLUGIN = "plugin";
    /// <summary>
    /// extension table
    /// </summary>
    public static readonly string PACKAGE_PLUGIN_ASSOCIATION = "packagepluginassociation";
    /// <summary>
    /// package table
    /// </summary>
    public static readonly string PACKAGE = "package";
    /// <summary>
    /// cnc alarm table
    /// </summary>
    public static readonly string CNC_ALARM = "cncalarm";
    /// <summary>
    /// cnc alarm severity table
    /// </summary>
    public static readonly string CNC_ALARM_SEVERITY = "cncalarmseverity";
    /// <summary>
    /// cnc alarm severity pattern table
    /// </summary>
    public static readonly string CNC_ALARM_SEVERITY_PATTERN = "cncalarmseveritypattern";
    /// <summary>
    /// current cnc alarm table
    /// </summary>
    public static readonly string CURRENT_CNC_ALARM = "currentcncalarm";
    /// <summary>
    /// cnc variable table
    /// </summary>
    public static readonly string CNC_VARIABLE = "cncvariable";
    /// <summary>
    /// machinecncvariable table
    /// </summary>
    public static readonly string MACHINE_CNC_VARIABLE = "machinecncvariable";
    /// <summary>
    /// sequencemilestone table
    /// </summary>
    public static readonly string SEQUENCE_MILESTONE = "sequencemilestone";
    /// <summary>
    /// refreshtoken table
    /// </summary>
    public static readonly string REFRESH_TOKEN = "refreshtoken";
    /// <summary>
    /// stampingconfig table
    /// </summary>
    public static readonly string STAMPING_CONFIG_BY_NAME = "stampingconfigbyname";
    /// <summary>
    /// scrapreport table
    /// </summary>
    public static readonly string SCRAP_REPORT = "scrapreport";
    /// <summary>
    /// scrapreasonreport table
    /// </summary>
    public static readonly string SCRAP_REASON_REPORT = "scrapreasonreport";
  }
}
