// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IFieldDAO">IFieldDAO</see>
  /// </summary>
  public class FieldDAO
    : VersionableNHibernateDAO<Field, IField, int>
    , IFieldDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FieldDAO).FullName);
    
    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IField field;

      // 01-49: stamping values, DbField
      // - 1: CadName
      field = new Field (1, "CadName");
      field.TranslationKey = "FieldCADModelName";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "CadModel";
      field.AssociatedProperty = "Name";
      InsertDefaultValue (field);
      // - 2: ProjectName
      field = new Field (2, "ProjectName");
      field.TranslationKey = "FieldProjectName";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "Project";
      field.AssociatedProperty = "Name";
      InsertDefaultValue (field);
      // - 3: ComponentName
      field = new Field (3, "ComponentName");
      field.TranslationKey = "FieldComponentName";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "Component";
      field.AssociatedProperty = "Name";
      InsertDefaultValue (field);
      // - 4: ComponentTypeCode
      field = new Field (4, "ComponentTypeCode");
      field.TranslationKey = "FieldComponentTypeCode";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "ComponentType";
      field.AssociatedProperty = "Code";
      InsertDefaultValue (field);
      // Deprecated: 5: ComponentTypeKey, 6: ComponentTypeId
      // - 7: OperationName
      field = new Field (7, "OperationName");
      field.TranslationKey = "FieldOperationName";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "Operation";
      field.AssociatedProperty = "Name";
      InsertDefaultValue (field);
      // - 8: OperationTypeCode
      field = new Field (8, "FieldOperationTypeCode");
      field.TranslationKey = "Field";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data | StampingDataType.DbField;
      field.AssociatedClass = "OperationType";
      field.AssociatedProperty = "Code";
      InsertDefaultValue (field);
      // Deprecated: 9: OperationTypeKey, 10: OperationTypeId, 20: ToolCode, 21: ToolName
      // - 22: ToolDiameter
      field = new Field (22, "ToolDiameter");
      field.TranslationKey = "FieldToolDiameter";
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data; // Previously also StampingDataType.DbField: Tool.Diameter
      InsertDefaultValue (field);
      // - 23: ToolRadius
      field = new Field (23, "ToolRadius");
      field.TranslationKey = "FieldToolRadius";
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data; // Previously aslo StampingDataType.DbField: Tool.Radius
      InsertDefaultValue (field);

      // 50-99: stamping values, Data
      // - Strategy
      field = new Field (40, "Strategy");
      field.TranslationKey = "FieldStrategy";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - Context
      field = new Field (41, "Context");
      field.TranslationKey = "FieldContext";
      field.Type = FieldType.String;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - Depth
      field = new Field (50, "Depth");
      field.TranslationKey = "FieldDepth"; // Previously: FieldProcessDepth
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - Width
      field = new Field (51, "Width");
      field.TranslationKey = "FieldWidth";
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - Tolerance
      field = new Field (52, "Tolerance");
      field.TranslationKey = "FieldTolerance"; // Previously: FieldProcessTolerance
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - Stock
      field = new Field (53, "Stock");
      field.TranslationKey = "FieldStock"; // Previously: FieldProcesssStock
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - MinimumToolLength
      field = new Field (54, "MinimumToolLength");
      field.TranslationKey = "FieldMinimumToolLength"; // Previously: FieldProcessMinimumToolLength
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - ProgrammedFeedrate
      field = new Field (55, "ProgrammedFeedrate");
      field.TranslationKey = "FieldProgrammedFeedrate"; // Previously: FieldProcessProgrammedFeedrate
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);
      // - ProgrammedSpindleSpeed
      field = new Field (56, "ProgrammedSpindleSpeed");
      field.TranslationKey = "FieldProgrammedSpindleSpeed"; // Previously: FieldProcessProgrammedSpindleSpeed
      field.Type = FieldType.Double;
      field.StampingDataType = StampingDataType.Data;
      InsertDefaultValue (field);

      // 100-105: base fields
      // - Feedrate
      field = new Field (100, "Feedrate");
      field.TranslationKey = "FieldFeedrate";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 200;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (1); // 1: UnitFeedrate (mm/min)
      InsertDefaultValue (field);
      // - SpindleSpeed
      field = new Field (101, "SpindleSpeed");
      field.TranslationKey = "FieldSpindleSpeed";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 50; // previously 20, but 50 is better
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (3); // 3: UnitRotationSpeed (RPM)
      InsertDefaultValue (field);
      // - SpindleLoad
      field = new Field (102, "SpindleLoad");
      field.TranslationKey = "FieldSpindleLoad";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 10; // previously 20, but even less would be better
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (4); // 4: UnitPercent (%)
      field.Active = false; // For the moment, set this to false, because the average is not meaningful
      InsertDefaultValue (field);
      // - FeedrateOverride
      field = new Field (103, "FeedrateOverride");
      field.TranslationKey = "FieldFeedrateOverride";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 5;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (4); // 4: UnitPercent (%)
      InsertDefaultValue (field);
      // - SpindleSpeedOverride
      field = new Field (104, "SpindleSpeedOverride");
      field.TranslationKey = "FieldSpindleSpeedOverride";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 5;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (4); // 4: UnitPercent (%)
      InsertDefaultValue (field);
      // - RapidTraverseOverride
      field = new Field (105, "RapidTraverseOverride");
      field.TranslationKey = "FieldRapidTraverseOverride";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 5;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (4); // 4: UnitPercent (%)
      InsertDefaultValue (field);
      
      // Cutting / Traverse
      // - Traverse
      field = new Field (106, "RapidTraverse");
      field.TranslationKey = "FieldRapidTraverse";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - CuttingFeedrate: 107, remove it !
      // - RapidTraverseRate
      field = new Field (108, "RapidTraverseRate");
      field.TranslationKey = "FieldRapidTraverseRate";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 200;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (1); // 1: UnitFeedrate (mm/min)
      InsertDefaultValue (field);
      
      // Program
      // - ProgramName
      field = new Field (109, "ProgramName");
      field.TranslationKey = "FieldProgramName";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - SubProgramName
      field = new Field (110, "SubProgramName");
      field.TranslationKey = "FieldSubProgramName";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - ProgramFileName
      field = new Field ((int)FieldId.ProgramFileName, "ProgramFileName"); // 133
      field.TranslationKey = "FieldProgramFileName";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - SubProgramFileName
      field = new Field ((int)FieldId.SubProgramFileName, "SubProgramFileName"); // 134
      field.TranslationKey = "FieldSubProgramFileName";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);

      // Feedrates in IPM
      // - FeedrateUS
      field = new Field (111, "FeedrateUS");
      field.TranslationKey = "FieldFeedrateUS";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 10;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (2); // 2: UnitFeedrateUS (IPM)
      InsertDefaultValue (field);
      // - CuttingFeedrateUS: 112, remove it !
      // - RapidTraverseRateUS
      field = new Field (113, "RapidTraverseRateUS");
      field.TranslationKey = "FieldRapidTraverseRateUS";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 10;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (2); // 2: UnitFeedrateUS (IPM)
      InsertDefaultValue (field);
      
      // Modes
      // - CncModes
      field = new Field (114, "CncModes");
      field.TranslationKey = "FieldCncModes";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - Hold
      field = new Field (115, "Hold");
      field.TranslationKey = "FieldHold";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      
      // Switches
      // - OptionalStopSwitch
      field = new Field (116, "OptionalStopSwitch");
      field.TranslationKey = "FieldOptionalStopSwitch";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - BlockDeleteSwitch
      field = new Field (117, "BlockDeleteSwitch");
      field.TranslationKey = "FieldBlockDeleteSwitch";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - DryRunSwitch
      field = new Field (118, "DryRun");
      field.TranslationKey = "FieldDryRun";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - ToolNumber
      field = new Field (119, "ToolNumber");
      field.TranslationKey = "FieldToolNumber";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - SingleBlock
      field = new Field (120, "SingleBlock");
      field.TranslationKey = "FieldSingleBlock";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - MachineLock (test mode)
      field = new Field (121, "MachineLock");
      field.TranslationKey = "FieldMachineLock";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - SpindleLoadPeak
      field = new Field (122, "SpindleLoadPeak");
      field.TranslationKey = "FieldSpindleLoadPeak";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Max;
      field.MinTime = TimeSpan.FromSeconds (2);
      field.LimitDeviation = 10; // %
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (4); // 4: UnitPercent (%)
      InsertDefaultValue (field);
      // - AlarmSignal
      field = new Field (123, "AlarmSignal");
      field.TranslationKey = "FieldAlarmSignal";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);

      // Pallet
      // - PalletNumber
      field = new Field (124, "PalletNumber");
      field.TranslationKey = "FieldPalletNumber";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - PalletReady
      field = new Field (125, "PalletReady");
      field.TranslationKey = "FieldPalletReady";
      field.Type = FieldType.Boolean;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);

      // StackLight
      field = new Field ((int)FieldId.StackLight, "StackLight"); // 126
      field.TranslationKey = "FieldStackLight";
      field.Type = FieldType.Int32;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);

      // Program comment
      // - ProgramComment
      field = new Field (127, "ProgramComment");
      field.TranslationKey = "FieldProgramComment";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      field.Active = false; // By default, not active
      InsertDefaultValue (field);
      // - SubProgramCommment
      field = new Field (128, "SubProgramComment");
      field.TranslationKey = "FieldSubProgramComment";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      field.Active = false; // By default, not active
      InsertDefaultValue (field);

      // Part count / Sequence number
      // - CncPartCount
      field = new Field (129, "CncPartCount");
      field.TranslationKey = "FieldCncPartCount";
      field.Type = FieldType.Int32;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      field.Active = false; // By default, not active
      InsertDefaultValue (field);
      // - CncSequenceNumber
      field = new Field (130, "CncSequenceNumber");
      field.TranslationKey = "FieldCncSequenceNumber";
      field.Type = FieldType.Int32;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      field.Active = false; // By default, not active
      InsertDefaultValue (field);
      // - BlockNumber
      field = new Field (131, "BlockNumber");
      field.TranslationKey = "FieldBlockNumber";
      field.Type = FieldType.Int32;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      field.Active = false; // By default, not active
      InsertDefaultValue (field);

      // Flow
      field = new Field ((int)FieldId.Flow, "Flow"); // 132
      field.TranslationKey = "FieldFlow";
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.MinTime = TimeSpan.FromSeconds (10);
      field.LimitDeviation = 10;
      field.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (18); // 18: UnitFlowRate (L/s)
      InsertDefaultValue (field);

      // Fanuc
      // - FanucAutoManualMode
      field = new Field (190, "FanucAutoManualMode");
      field.TranslationKey = "FieldFanucAutoManualMode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - FanucRunningStatus
      field = new Field (191, "FanucRunningStatus");
      field.TranslationKey = "FieldFanucRunningStatus";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - FanucMotionStatus
      field = new Field (192, "FanucMotionStatus");
      field.TranslationKey = "FieldFanucMotionStatus";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // MTConnect
      // - MTConnectControllerMode
      field = new Field (180, "MTConnectControllerMode");
      field.TranslationKey = "FieldMTConnectControllerMode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - MTConnectControllerMode
      field = new Field (181, "MTConnectExecution");
      field.TranslationKey = "FieldMTConnectExecution";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - MoriSeikiOperationMode
      field = new Field (182, "MoriSeikiOperationMode");
      field.TranslationKey = "FieldMoriSeikiOperationMode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // Heidenhain
      // - HeidenhainExecutionMode
      field = new Field (170, "HeidenhainExecutionMode");
      field.TranslationKey = "FieldHeidenhainExecutionMode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - HeidenhainProgramStatus
      field = new Field (171, "HeidenhainProgramStatus");
      field.TranslationKey = "FieldHeidenhainProgramStatus";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // Roeders
      // - RoedersMode
      field = new Field (160, "RoedersMode");
      field.TranslationKey = "FieldRoedersMode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // - RoedersState
      field = new Field (161, "RoedersState");
      field.TranslationKey = "FieldRoedersState";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
      // Selca
      // - SelcaNCStatusCode
      field = new Field (150, "SelcaNCStatusCode");
      field.TranslationKey = "FieldSelcaNCStatusCode";
      field.Type = FieldType.String;
      field.CncDataAggregationType = CncDataAggregationType.NewValue;
      InsertDefaultValue (field);
    }
    
    private void InsertDefaultValue (IField field)
    {
      if (null == FindById (field.Id)) { // the config does not exist => create it
        log.InfoFormat ("InsertDefaultValue: " +
                        "add id={0} Code={1}",
                        field.Id, field.Code);
        // Use a raw SQL Command, else the Id is resetted
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"INSERT INTO Field (fieldid, fieldcode, fieldtranslationkey, fielddescription, fieldtype, unitid, stampingdatatype, cncdataaggregationtype, fieldassociatedclass, fieldassociatedproperty, fieldcustom, fieldmintime, fieldlimitdeviation, fieldactive)
VALUES ({0}, '{1}', '{2}', {3}, '{4}', {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})",
                                               field.Id, field.Code, field.TranslationKey, // 0-2
                                               string.IsNullOrEmpty (field.Description) ? "NULL" : "'" + field.Description + "'", // 3
                                               field.Type.ToString (), // 4
                                               (null == field.Unit) ? "NULL" : field.Unit.Id.ToString (), // 5
                                               (!field.StampingDataType.HasValue || (0 == (int)field.StampingDataType.Value)) ? "NULL" : ((int)field.StampingDataType).ToString (), // 6
                                               (!field.CncDataAggregationType.HasValue || (CncDataAggregationType.None == field.CncDataAggregationType.Value)) ? "NULL" : ((int)field.CncDataAggregationType).ToString (), // 7
                                               string.IsNullOrEmpty (field.AssociatedClass) ? "NULL" : "'" + field.AssociatedClass + "'", // 8
                                               string.IsNullOrEmpty (field.AssociatedProperty) ? "NULL" : "'" + field.AssociatedProperty + "'", // 9
                                               field.Custom, // 10
                                               field.MinTime.HasValue ? field.MinTime.Value.TotalSeconds.ToString (CultureInfo.InvariantCulture) : "NULL", // 11
                                               field.LimitDeviation.HasValue ? field.LimitDeviation.Value.ToString (CultureInfo.InvariantCulture) : "NULL", // 12
                                               field.Active ? "TRUE" : "FALSE"); // 13
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          command.ExecuteNonQuery();
        }
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Find by code
    /// 
    /// There is no eager fetch of the unit here.
    /// 
    /// Note: this is registered to be cacheable with CacheMode.Get ;
    ///       the current session must not add a new field
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public IField FindByCode (string code)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Field> ()
        .Add (Restrictions.Eq ("Code", code))
        .SetCacheable (true)
        .SetCacheMode (CacheMode.Get)
        .UniqueResult<IField> ();
    }
    
    /// <summary>
    /// Find all active fields
    /// </summary>
    /// <returns></returns>
    public IList<IField> FindAllActive()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<Field>()
        .Add (Restrictions.Eq ("Active", true))
        .SetCacheable (true)
        .List<IField>();
    }

    /// <summary>
    /// Find all active fields
    /// 
    /// This request is not cacheable because of the eager fetch of the unit
    /// </summary>
    /// <returns></returns>
    public IList<IField> FindAllActiveWithUnit ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Field> ()
        .Add (Restrictions.Eq ("Active", true))
        .Fetch (SelectMode.Fetch, "Unit")
        .List<IField> ();
    }
  }
}
