// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Security.Cryptography.X509Certificates;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Column name constants
  /// 
  /// Warning ! Try to prefix the column name by the main table name to ease the use of the natural joins
  /// </summary>
  public class ColumnName
  {
    /// <summary>
    /// Locale of the translation table
    /// </summary>
    public static readonly string LOCALE = "Locale";
    /// <summary>
    /// Translation key of the translation table
    /// </summary>
    public static readonly string TRANSLATION_KEY = "TranslationKey";
    /// <summary>
    /// Translation value of the translation table
    /// </summary>
    public static readonly string TRANSLATION_VALUE = "TranslationValue";

    /// <summary>
    /// Reference to a day slot
    /// </summary>
    public static readonly string DAY = "day";
    
    /// <summary>
    /// ID of the cnc alarm table
    /// </summary>
    public static readonly string CNC_ALARM_ID = TableName.CNC_ALARM + "id";
    
    /// <summary>
    /// ID of the cnc alarm severity table
    /// </summary>
    public static readonly string CNC_ALARM_SEVERITY_ID = TableName.CNC_ALARM_SEVERITY + "id";
    
    /// <summary>
    /// ID of the cnc alarm severity pattern table
    /// </summary>
    public static readonly string CNC_ALARM_SEVERITY_PATTERN_ID = TableName.CNC_ALARM_SEVERITY_PATTERN + "id";
    
    /// <summary>
    /// ID of the computer table
    /// </summary>
    public static readonly string COMPUTER_ID = "computerid";
    
    /// <summary>
    /// ID of the company table
    /// </summary>
    public static readonly string COMPANY_ID = "companyid";
    
    /// <summary>
    /// ID of the deliverablepiece table
    /// </summary>
    public static readonly string DELIVERABLE_PIECE_ID = "deliverablepieceid";
    /// <summary>
    /// Code (serial number) column of the deliverablepiece table
    /// </summary>
    public static readonly string DELIVERABLE_PIECE_CODE = "deliverablepiececode";
    /// <summary>
    /// version column of the deliverablepiece table
    /// </summary>
    public static readonly string DELIVERABLE_PIECE_VERSION = "deliverablepieceversion";
    
    /// <summary>
    /// ID of the department table
    /// </summary>
    public static readonly string DEPARTMENT_ID = "departmentid";
    /// <summary>
    /// ID of the cell table
    /// </summary>
    public static readonly string CELL_ID = TableName.CELL + "id";
    /// <summary>
    /// ID of the machinecategory table
    /// </summary>
    public static readonly string MACHINE_CATEGORY_ID = "machinecategoryid";
    /// <summary>
    /// ID of the machinesubcategory table
    /// </summary>
    public static readonly string MACHINE_SUB_CATEGORY_ID = "machinesubcategoryid";
    /// <summary>
    /// ID of the machine table
    /// </summary>
    public static readonly string MACHINE_ID = "machineid";
    /// <summary>
    /// ID of the machine filter table
    /// </summary>
    public static readonly string MACHINE_FILTER_ID = "machinefilterid";
    /// <summary>
    /// ID of a machine filter item table
    /// </summary>
    public static readonly string MACHINE_FILTER_ITEM_ID = "machinefilteritemid";
    /// <summary>
    /// Order of a machine filter item table
    /// </summary>
    public static readonly string MACHINE_FILTER_ITEM_ORDER = TableName.MACHINE_FILTER + "itemorder";
    /// <summary>
    /// Order of a machine filter item table
    /// </summary>
    public static readonly string MACHINE_FILTER_ITEM_RULE = TableName.MACHINE_FILTER + "itemrule";
    
    /// <summary>
    /// ID of the machinemodule table
    /// </summary>
    public static readonly string MACHINE_MODULE_ID = "machinemoduleid";
    
    /// <summary>
    /// ID of the cncacquisition table
    /// </summary>
    public static readonly string CNC_ACQUISITION_ID = "cncacquisitionid";
    
    /// <summary>
    /// ID of the machinemodecategory table
    /// </summary>
    public static readonly string MACHINE_MODE_CATEGORY_ID = TableName.MACHINE_MODE_CATEGORY + "id";

    /// <summary>
    /// ID of the machinemode table
    /// </summary>
    public static readonly string MACHINE_MODE_ID = "machinemodeid";
    
    /// <summary>
    /// ID of the work order table
    /// </summary>
    public static readonly string WORK_ORDER_ID = "workorderid";
    
    /// <summary>
    /// ID of the project table
    /// </summary>
    public static readonly string PROJECT_ID = "projectid";
    
    /// <summary>
    /// IF of the workorderproject table
    /// </summary>
    public static readonly string WORK_ORDER_PROJECT_ID = "workorderprojectid";
    
    /// <summary>
    /// ID of the component table
    /// </summary>
    public static readonly string COMPONENT_ID = "componentid";

    /// <summary>
    /// ID of the customer table
    /// </summary>
    public static readonly string CUSTOMER_ID = "customerid";
    
    /// <summary>
    /// ID of the intermediateworkpiece table
    /// </summary>
    public static readonly string INTERMEDIATE_WORK_PIECE_ID = "intermediateworkpieceid";
    
    /// <summary>
    /// ID of the componentintermediateworkpiece table
    /// </summary>
    public static readonly string COMPONENT_INTERMEDIATE_WORK_PIECE_ID = "componentintermediateworkpieceid";
    
    /// <summary>
    /// ID of the operation table
    /// </summary>
    public static readonly string OPERATION_ID = "operationid";
    
    /// <summary>
    /// ID of the old sequence (= process) table
    /// </summary>
    public static readonly string OLD_SEQUENCE_ID = "processid";
    /// <summary>
    /// ID of the sequence table
    /// </summary>
    public static readonly string SEQUENCE_ID = "sequenceid";
    
    /// <summary>
    /// ID of the path table
    /// </summary>
    public static readonly string PATH_ID = "pathid";

    /// <summary>
    /// version of the path table
    /// </summary>
    public static readonly string PATH_VERSION = "pathversion";

    /// <summary>
    /// number of the path table
    /// </summary>
    public static readonly string PATH_NUMBER = "pathnumber";
    
    /// <summary>
    /// ID of the stamp table
    /// </summary>
    public static readonly string STAMP_ID = "stampid";
    
    /// <summary>
    /// Key of the ncprogramcode table
    /// </summary>
    public static readonly string NC_PROGRAM_CODE_KEY = TableName.NC_PROGRAM_CODE + "key";
    
    /// <summary>
    /// ID of the cadmodel table
    /// </summary>
    public static readonly string CAD_MODEL_ID = "cadmodelid";
    
    /// <summary>
    /// ID of the unit table
    /// </summary>
    public static readonly string UNIT_ID = "unitid";
    
    /// <summary>
    /// ID of the isofile table
    /// </summary>
    public static readonly string ISO_FILE_ID = "isofileid";
    
    /// <summary>
    /// ID of the tool table
    /// </summary>
    public static readonly string TOOL_ID = TableName.TOOL + "id";
    
    /// <summary>
    /// ID of the tool position table
    /// </summary>
    public static readonly string TOOL_POSITION_ID = TableName.TOOL_POSITION + "id";
    
    /// <summary>
    /// ID of the tool life table
    /// </summary>
    public static readonly string TOOL_LIFE_ID = TableName.TOOL_LIFE + "id";
    
    /// <summary>
    /// ID of the tool data acquisition table
    /// </summary>
    public static readonly string TOOL_DATA_ACQUISITION_ID = TableName.TOOL_DATA_ACQUISITION + "id";
    
    /// <summary>
    /// ID of the field table
    /// </summary>
    public static readonly string FIELD_ID = "fieldid";
    
    /// <summary>
    /// ID of the machine observation state table
    /// </summary>
    public static readonly string MACHINE_OBSERVATION_STATE_ID = "machineobservationstateid";
    
    /// <summary>
    /// ID of the machine state template table
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_ID = "machinestatetemplateid";
    
    /// <summary>
    /// ID of the machine state template category table
    /// </summary>
    public static readonly string MACHINE_STATE_TEMPLATE_CATEGORY_ID = "machinestatetemplatecategoryid";
    
    /// <summary>
    /// ID of the user table
    /// </summary>
    public static readonly string USER_ID = "userid";

    /// <summary>
    /// ID of the productionstate table
    /// </summary>
    public static readonly string PRODUCTION_STATE_ID = $"{TableName.PRODUCTION_STATE}id";

    /// <summary>
    /// ID of the reason table
    /// </summary>
    public static readonly string REASON_ID = "reasonid";
    
    /// <summary>
    /// Extra reason
    /// </summary>
    public static readonly string REASON_DETAILS = "reasondetails";
    
    /// <summary>
    /// ID of the reasongroup table
    /// </summary>
    public static readonly string REASON_GROUP_ID = "reasongroupid";
    
    /// <summary>
    /// ID of the revision table
    /// </summary>
    public static readonly string REVISION_ID = "revisionid";
    /// <summary>
    /// ID of the modification tables
    /// </summary>
    public static readonly string MODIFICATION_ID = "modificationid";
    /// <summary>
    /// Date/time in the modification table
    /// </summary>
    public static readonly string MODIFICATION_DATETIME = "modificationdatetime";
    /// <summary>
    /// Priority in the modification table
    /// </summary>
    public static readonly string MODIFICATION_PRIORITY = "modificationpriority";
    
    /// <summary>
    /// Id of the old auto-process (now autosequence) table
    /// </summary>
    public static readonly string OLD_AUTO_SEQUENCE_ID = "autoprocessid";
    /// <summary>
    /// Id of the new autosequence (previously autoprocess) table
    /// </summary>
    public static readonly string AUTO_SEQUENCE_ID = "autosequenceid";
    
    /// <summary>
    /// Id of the table operationcycle
    /// </summary>
    public static readonly string OPERATION_CYCLE_ID = "operationcycleid";
    
    /// <summary>
    /// Id of the table operationcycledeliverablepiece
    /// </summary>
    public static readonly string OPERATION_CYCLE_DELIVERABLE_PIECE_ID = "operationcycledeliverablepieceid";
    /// <summary>
    /// Version of the table operationcycledeliverablepiece
    /// </summary>
    public static readonly string OPERATION_CYCLE_DELIVERABLE_PIECE_VERSION = "operationcycledeliverablepieceversion";
    
    /// <summary>
    /// serial number
    /// </summary>
    public static readonly string SERIAL_NUMBER = "serialnumber";
    
    /// <summary>
    /// Id of the table workorderslot
    /// </summary>
    public static readonly string WORK_ORDER_SLOT_ID = "workorderslotid";
    /// <summary>
    /// Id of the table operationslot
    /// </summary>
    public static readonly string OPERATION_SLOT_ID = "operationslotid";
    /// <summary>
    /// Id of the table observationstateslot
    /// </summary>
    public static readonly string OBSERVATION_STATE_SLOT_ID = "observationstateslotid";
    /// <summary>
    /// Id of the table userslot
    /// </summary>
    public static readonly string USER_SLOT_ID = "userslotid";
    /// <summary>
    /// Id of the table reasonslot
    /// </summary>
    public static readonly string REASON_SLOT_ID = "reasonslotid";
    /// <summary>
    /// Version of the table workorderslot
    /// </summary>
    public static readonly string WORK_ORDER_SLOT_VERSION = "workorderslotversion";
    /// <summary>
    /// Version of the table operationslot
    /// </summary>
    public static readonly string OPERATION_SLOT_VERSION = "operationslotversion";
    /// <summary>
    /// Version of the table observationstateslot
    /// </summary>
    public static readonly string OBSERVATION_STATE_SLOT_VERSION = "observationstateslotversion";
    /// <summary>
    /// Version of the table userslot
    /// </summary>
    public static readonly string USER_SLOT_VERSION = "userslotversion";
    /// <summary>
    /// Version of the table reasonslot
    /// </summary>
    public static readonly string REASON_SLOT_VERSION = "reasonslotversion";
    
    /// <summary>
    /// Id of the table machineactivitysummary
    /// </summary>
    public static readonly string MACHINE_ACTIVITY_SUMMARY_ID = "machineactivitysummaryid";
    /// <summary>
    /// Version of the table machineactivitysummary
    /// </summary>
    public static readonly string MACHINE_ACTIVITY_SUMMARY_VERSION = "machineactivitysummaryversion";
    /// <summary>
    /// Id of the table reasonsummary
    /// </summary>
    public static readonly string REASON_SUMMARY_ID = "reasonsummaryid";
    /// <summary>
    /// Version of the table reasonsummary
    /// </summary>
    public static readonly string REASON_SUMMARY_VERSION = "reasonsummaryversion";
    
    /// <summary>
    /// ID of a log table
    /// </summary>
    public static readonly string LOG_ID = "logid";
    
    /// <summary>
    /// Id of eventcncvalueconfig table
    /// </summary>
    public static readonly string EVENT_CNC_VALUE_CONFIG_ID = "eventcncvalueconfigid";
    /// <summary>
    /// Id of eventlongperiodconfig table
    /// </summary>
    public static readonly string EVENT_LONG_PERIOD_CONFIG_ID = "eventlongperiodconfigid";
    /// <summary>
    /// Id of eventtoollifeconfig table
    /// </summary>
    public static readonly string EVENT_TOOL_LIFE_CONFIG_ID = "eventtoollifeconfigid";
    /// <summary>
    /// Id of all the event tables
    /// </summary>
    public static readonly string EVENT_ID = "eventid";
    /// <summary>
    /// Level of an event
    /// </summary>
    public static readonly string EVENT_LEVEL_ID = "eventlevelid";
    /// <summary>
    /// Date/time of an event
    /// </summary>
    public static readonly string EVENT_DATETIME = "eventdatetime";
    /// <summary>
    /// Type of the event
    /// </summary>
    public static readonly string EVENT_TYPE = "eventtype";
    /// <summary>
    /// Data of the event
    /// </summary>
    public static readonly string EVENT_DATA = "eventdata";
    /// <summary>
    /// Trigger duration of an event
    /// </summary>
    public static readonly string EVENT_TRIGGER_DURATION = "eventtriggerduration";
    
    /// <summary>
    /// Id of the new fact table
    /// </summary>
    public static readonly string FACT_ID = "factid";
    
    /// <summary>
    /// Id of the stampingvalue table
    /// </summary>
    public static readonly string STAMPINGVALUE_ID = "stampingvalueid";

    /// <summary>
    /// Version of the stampingvalue table
    /// </summary>
    public static readonly string STAMPINGVALUE_VERSION = "stampingvalueversion";
    
    /// <summary>
    /// "stamping value as string" column
    /// </summary>
    public static readonly string STAMPINGVALUE_STRING = "stampingvaluestring";
    
    /// <summary>
    /// "stamping value as int" column
    /// </summary>
    public static readonly string STAMPINGVALUE_INT = "stampingvalueint";
    
    /// <summary>
    /// "stamping value as double" column
    /// </summary>
    public static readonly string STAMPINGVALUE_DOUBLE = "stampingvaluedouble";
    
    /// <summary>
    /// Id of the shift table
    /// </summary>
    public static readonly string SHIFT_ID = "shiftid";
    
    /// <summary>
    /// Id of the shiftslot table
    /// </summary>
    public static readonly string SHIFT_SLOT_ID = "shiftslotid";
    
    /// <summary>
    /// Id of the shifttemplate table
    /// </summary>
    public static readonly string SHIFT_TEMPLATE_ID = "shifttemplateid";
    
    /// <summary>
    /// Id of the machinemoduledetection table
    /// </summary>
    public static readonly string MACHINE_MODULE_DETECTION_ID = TableName.MACHINE_MODULE_DETECTION + "id";
    
    /// <summary>
    /// Id of the role table
    /// </summary>
    public static readonly string ROLE_ID = "roleid";
    
    /// <summary>
    /// Id of the right table
    /// </summary>
    public static readonly string RIGHT_ID = "rightid";
    
    /// <summary>
    /// access privilege of the right table
    /// </summary>
    public static readonly string RIGHT_ACCESS_PRIVILEGE = "rightaccessprivilege";
    
    /// <summary>
    /// Id of the nonconformancereason table
    /// </summary>
    public static readonly string NON_CONFORMANCE_REASON_ID = TableName.NON_CONFORMANCE_REASON + "id";
        
    /// <summary>
    /// Id of the nonconformancereport table
    /// </summary>
    public static readonly string NON_CONFORMANCE_REPORT_ID = TableName.NON_CONFORMANCE_REPORT + "id";
    
    /// <summary>
    /// Id of the line table
    /// </summary>
    public static readonly string LINE_ID = TableName.LINE_OLD + "id";
    
    /// <summary>
    /// Id of the line component table
    /// </summary>
    public static readonly string LINE_COMPONENT_ID = TableName.LINE_COMPONENT + "id";
    
    /// <summary>
    /// Id of the linemachine table
    /// </summary>
    public static readonly string LINE_MACHINE_ID = TableName.LINE_MACHINE + "id";
    
    /// <summary>
    /// Id of the workorderline table
    /// </summary>
    public static readonly string WORK_ORDER_LINE_ID = TableName.WORK_ORDER_LINE + "id";
    
    /// <summary>
    /// Id of the workorderlinequantity table
    /// </summary>
    public static readonly string WORK_ORDER_LINE_QUANTITY_ID = TableName.WORK_ORDER_LINE_QUANTITY + "id";
    
    /// <summary>
    /// Id of the operationsourceworkpiece table
    /// </summary>
    public static readonly string OPERATION_SOURCE_WORKPIECE_ID = TableName.OPERATION_SOURCE_WORKPIECE + "id";
    
    /// <summary>
    /// Id of the daytemplate table
    /// </summary>
    public static readonly string DAY_TEMPLATE_ID = TableName.DAY_TEMPLATE + "id";
    
    /// <summary>
    /// Id of the task table
    /// </summary>
    public static readonly string TASK_ID = TableName.TASK + "id";
    
    /// <summary>
    /// Id of the taskstatus table
    /// </summary>
    public static readonly string TASK_STATUS_ID = TableName.TASK_STATUS + "id";
    
    /// <summary>
    /// Id of the plugin table
    /// </summary>
    public static readonly string PLUGIN_ID = TableName.PLUGIN + "id";
    
    /// <summary>
    /// Id of the extension table
    /// </summary>
    public static readonly string PACKAGE_PLUGIN_ASSOCIATION_ID = TableName.PACKAGE_PLUGIN_ASSOCIATION + "id";
    
    /// <summary>
    /// Id of the package table
    /// </summary>
    public static readonly string PACKAGE_ID = TableName.PACKAGE + "id";

    /// <summary>
    /// Id of the acquisitionstate table
    /// </summary>
    public static readonly string ACQUISITION_STATE_ID = TableName.ACQUISITION_STATE + "id";

    /// <summary>
    /// Id of the stampingconfigbyname table
    /// </summary>
    public static readonly string STAMPING_CONFIG_BY_NAME_ID = TableName.STAMPING_CONFIG_BY_NAME + "id";
    /// <summary>
    /// stamping config column of stampingconfigbyname table
    /// </summary>
    public static readonly string STAMPING_CONFIG = "stampingconfig";
  }
}
