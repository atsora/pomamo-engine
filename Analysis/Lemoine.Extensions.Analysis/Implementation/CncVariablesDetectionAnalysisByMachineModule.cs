// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Extensions.Database;
using Pulse.Extensions.Database.Impl.CycleDetectionStatus;
using Pulse.Extensions.Database.Impl.OperationDetectionStatus;

namespace Pulse.Extensions.Analysis.Implementation
{
  /// <summary>
  /// Base class to process new sequences
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class CncVariablesDetectionAnalysisByMachineModule<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , IOperationDetectionStatusExtension
    , ICycleDetectionStatusExtension
    , IDetectionAnalysisByMachineModuleExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, ICycleDetectionStatusConfiguration, IOperationDetectionStatusConfiguration, new()
  {
    ILog log;

    TConfiguration m_configuration;
    IMachineModule m_machineModule;
    IOperationDetection m_operationDetection;
    IOperationCycleDetection m_operationCycleDetection;
    ICycleDetectionStatusExtension m_cycleDetectionStatus;
    IOperationDetectionStatusExtension m_operationDetectionStatus;
    ISequenceDetection m_sequenceDetection;
    ISequenceMilestoneDetection m_sequenceMilestoneDetection;
    string m_sequenceVariableKey;
    string m_milestoneVariableKey;
    SequenceKind m_previousSequenceKind = SequenceKind.Machining; // default

    /// <summary>
    /// Associated configuration
    /// </summary>
    protected TConfiguration Configuration
    {
      get { return m_configuration; }
    }

    /// <summary>
    /// Associated machine module
    /// </summary>
    protected IMachineModule MachineModule
    {
      get { return m_machineModule; }
    }

    /// <summary>
    /// Associated monitored machine
    /// </summary>
    protected IMonitoredMachine Machine
    {
      get { return m_machineModule.MonitoredMachine; }
    }

    /// <summary>
    /// Sequence variable key
    /// </summary>
    protected virtual string SequenceVariableKey
    {
      get { return m_sequenceVariableKey; }
    }

    /// <summary>
    /// Milestone variable key
    /// </summary>
    protected virtual string MilestoneVariableKey
    {
      get { return m_milestoneVariableKey; }
    }

    /// <summary>
    /// Associated operation detection
    /// </summary>
    protected IOperationDetection OperationDetection
    {
      get { return m_operationDetection; }
    }

    /// <summary>
    /// Associated operation cycle detection
    /// </summary>
    protected IOperationCycleDetection OperationCycleDetection
    {
      get { return m_operationCycleDetection; }
    }

    /// <summary>
    /// Associated sequence detection
    /// </summary>
    protected ISequenceDetection SequenceDetection
    {
      get { return m_sequenceDetection; }
    }

    /// <summary>
    /// Associated sequence milestone detection
    /// </summary>
    protected ISequenceMilestoneDetection SequenceMilestoneDetection
    {
      get { return m_sequenceMilestoneDetection; }
    }

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel
    {
      get; set;
    }

    /// <summary>
    /// Cycle detection status priority
    /// </summary>
    public int CycleDetectionStatusPriority
    {
      get {
        return m_cycleDetectionStatus.CycleDetectionStatusPriority;
      }
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    protected ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Initialize implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="operationDetection"></param>
    /// <param name="operationCycleDetection"></param>
    /// <param name="sequenceDetection"></param>
    /// <param name="sequenceMilestoneDetection"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMachineModule machineModule, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection, ISequenceDetection sequenceDetection, ISequenceMilestoneDetection sequenceMilestoneDetection)
    {
      m_machineModule = machineModule;
      var monitoredMachine = m_machineModule.MonitoredMachine;
      m_operationDetection = operationDetection;
      m_operationCycleDetection = operationCycleDetection;
      m_sequenceDetection = sequenceDetection;
      m_sequenceMilestoneDetection = sequenceMilestoneDetection;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{monitoredMachine.Id}.{m_machineModule.Id}");
      if (!Initialize (machineModule.MonitoredMachine)) {
        log.Debug ("Initialize: initialize with IMachine failed => return false (skip)");
        return false;
      }
      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: bad configuration, skip this instance");
        return false;
      }
      m_sequenceVariableKey = machineModule.SequenceVariable;
      m_milestoneVariableKey = machineModule.MilestoneVariable;
      return true;
    }

    /// <summary>
    /// ProcessDetection implementation
    /// </summary>
    /// <param name="detection"></param>
    public virtual void ProcessDetection (IMachineModuleDetection detection)
    {
      // Note: if no plugin returns true for IDetectionAnalysisExtension.UseUniqueSerializableTransaction,
      // then this is not run in a transaction
      // because we suppose this is ok to process a machine module detection twice
      Debug.Assert (null != detection);

      if ((null != detection.CustomType)
        && (detection.CustomType.Equals ("CncVariables", StringComparison.InvariantCultureIgnoreCase))) {
        var cncVariableKeys = Lemoine.Collections.EnumerableString.ParseListString<string> (detection.CustomValue);
        if (cncVariableKeys.Contains (this.SequenceVariableKey)) {
          ProcessSequenceDetection (detection, cncVariableKeys.Contains (this.MilestoneVariableKey));
        }
        else if (cncVariableKeys.Contains (this.MilestoneVariableKey)) {
          ProcessMilestoneDetection (detection, cncVariableKeys.Contains (this.SequenceVariableKey));
        }
      }
    }

    void ProcessSequenceDetection (IMachineModuleDetection detection, bool includeMilestoneVariable)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.ProcessSequenceDetection")) {
          var newCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
            .FindAt (detection.MachineModule, this.SequenceVariableKey, detection.DateTime);
          if (null == newCncVariable) {
            log.Error ($"ProcessSequenceDetection: no cnc variable {this.SequenceVariableKey} at {detection.DateTime}");
          }
          else if (TriggerSequenceVariableAction (newCncVariable.Value)) {
            var oldCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
              .FindAt (detection.MachineModule, this.SequenceVariableKey, detection.DateTime.AddSeconds (-1));
            if ((null != oldCncVariable) && object.Equals (newCncVariable.Value, oldCncVariable.Value)) {
              log.DebugFormat ("ProcessSequenceDetection: no cnc variable change");
            }
            else {
              var oldSequenceVariableValue = oldCncVariable?.Value;
              ICncVariable milestoneVariable = null;
              if (includeMilestoneVariable) {
                milestoneVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
                  .FindAt (detection.MachineModule, this.MilestoneVariableKey, detection.DateTime);
              }
              try {
                RunSequenceVariableAction (oldSequenceVariableValue, newCncVariable.Value, detection.DateTime, milestoneVariable?.Value);
              }
              catch (Exception ex) {
                log.Error ("ProcessSequenceDetection: exception in RunSequenceVariableAction", ex);
                throw;
              }
            }
          }
          transaction.Commit ();
        } // Transaction
      } // Session
    }

    void ProcessMilestoneDetection (IMachineModuleDetection detection, bool includeSequenceVariable)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.ProcessMilestoneDetection")) {
          var newCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
            .FindAt (detection.MachineModule, this.MilestoneVariableKey, detection.DateTime);
          if (null == newCncVariable) {
            log.ErrorFormat ("ProcessMilestoneDetection: no cnc variable {0} at {1}",
              this.SequenceVariableKey, detection.DateTime);
          }
          else if (TriggerMilestoneVariableAction (newCncVariable.Value)) {
            var oldCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
              .FindAt (detection.MachineModule, this.SequenceVariableKey, detection.DateTime.AddSeconds (-1));
            if ((null != oldCncVariable) && object.Equals (newCncVariable.Value, oldCncVariable.Value)) {
              log.DebugFormat ("ProcessMilestoneDetection: no cnc variable change");
            }
            else {
              var oldSequenceVariableValue = (null != oldCncVariable) ? oldCncVariable.Value : null;
              ICncVariable sequenceVariable = null;
              if (includeSequenceVariable) {
                sequenceVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
                  .FindAt (detection.MachineModule, this.SequenceVariableKey, detection.DateTime);
              }
              try {
                RunMilestoneVariableAction (oldSequenceVariableValue, newCncVariable.Value, detection.DateTime, sequenceVariable?.Value);
              }
              catch (Exception ex) {
                log.Error ("ProcessMilestoneDetection: exception in RunMilestoneVariableAction", ex);
                throw;
              }
            }
          }
          transaction.Commit ();
        } // Transaction
      } // Session
    }

    /// <summary>
    /// To override: condition to trigger the action considering the sequence variable
    /// </summary>
    /// <param name="newSequenceVariableValue"></param>
    /// <returns></returns>
    protected abstract bool TriggerSequenceVariableAction (object newSequenceVariableValue);

    /// <summary>
    /// Action of the sequence variable
    /// </summary>
    /// <param name="oldSequenceVariableValue"></param>
    /// <param name="newSequenceVariableValue"></param>
    /// <param name="dateTime"></param>
    /// <param name="milestoneVariableValue"></param>
    protected abstract void RunSequenceVariableAction (object oldSequenceVariableValue, object newSequenceVariableValue, DateTime dateTime, object milestoneVariableValue);

    /// <summary>
    /// To override: condition to trigger the action considering the milestone variable
    /// </summary>
    /// <param name="newMilestoneVariableValue"></param>
    /// <returns></returns>
    protected abstract bool TriggerMilestoneVariableAction (object newMilestoneVariableValue);

    /// <summary>
    /// Action of the milestone variable
    /// </summary>
    /// <param name="oldMilestoneVariableValue"></param>
    /// <param name="newMilestoneVariableValue"></param>
    /// <param name="dateTime"></param>
    /// <param name="sequenceVariableValue"></param>
    protected abstract void RunMilestoneVariableAction (object oldMilestoneVariableValue, object newMilestoneVariableValue, DateTime dateTime, object sequenceVariableValue);

    #region IOperationDetectionStatusExtension
    /// <summary>
    /// <see cref="IOperationDetectionStatusExtension"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 machine.Id));
      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: bad configuration, skip this instance");
        return false;
      }

      if (!CheckMachineFilter (machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {machine.Id} does not match machine filter => return false (skip this extension)");
        }
        return false;
      }
      m_operationDetectionStatus = new OperationDetectionStatusFromAnalysisStatus (m_configuration.OperationDetectionStatusPriority);
      if (!m_operationDetectionStatus.Initialize (machine)) {
        log.Error ("Initialize: initialization of operation detection status failed");
        return false;
      }
      m_cycleDetectionStatus = new CycleDetectionStatusFromAnalysisStatus (m_configuration.CycleDetectionStatusPriority);
      if (!m_cycleDetectionStatus.Initialize (machine)) {
        log.Error ("Initialize: initialization of cycle detection status failed");
        return false;
      }
      return true;
    }

    /// <summary>
    /// <see cref="IOperationDetectionStatusExtension"/>
    /// </summary>
    public int OperationDetectionStatusPriority
    {
      get {
        return m_operationDetectionStatus.OperationDetectionStatusPriority;
      }
    }



    /// <summary>
    /// <see cref="IOperationDetectionStatusExtension"/>
    /// </summary>
    /// <returns></returns>
    public virtual DateTime? GetOperationDetectionDateTime ()
    {
      return m_operationDetectionStatus.GetOperationDetectionDateTime ();
    }
    #endregion // IOperationDetectionStatusExtension

    bool CheckMachineFilter (IMachine machine)
    {
      IMachineFilter machineFilter = null;
      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return true;
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CncVariablesDetectionAnalysis.InitializeConfiguration.MachineFilter")) {
            int machineFilterId = m_configuration.MachineFilterId;
            machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.ErrorFormat ("Initialize: " +
                               "machine filter id {0} does not exist",
                               machineFilterId);
              return false;
            }
            else {
              if (!machineFilter.IsMatch (machine)) {
                return false;
              }
            }
          }
        }
        return true;
      }
    }

    /// <summary>
    /// Check if cncVariableValue has changed since previous recorded value for a given cncVariable
    /// Give old and new value if changed
    /// </summary>
    /// <param name="detection"></param>
    /// <param name="cncVariable"></param>
    /// <param name="inDetection"></param>
    /// <param name="oldCncVariable"></param>
    /// <param name="newCncVariable"></param>
    /// <returns></returns>
    public bool CheckCncVariableChange (IMachineModuleDetection detection, string cncVariable, bool inDetection,
      out ICncVariable oldCncVariable, out ICncVariable newCncVariable)
    {
      bool variableChange = false;
      newCncVariable = null;
      oldCncVariable = null;
      log.DebugFormat ("CheckCncVariableChange: cncVariable={0}", cncVariable);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CncVariablesDetectionAnalysis.CheckCncVariableChange")) {
          newCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
          .FindAt (detection.MachineModule, cncVariable, detection.DateTime);
          if (null == newCncVariable) {
            log.ErrorFormat ("CheckCncVariableChange: no cnc variable {0} at {1}", cncVariable, detection.DateTime);
          }
          else {
            // get previous CncVariable only if variable is in detection.
            if (null != newCncVariable.Value && inDetection) {
              oldCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
                .FindAt (detection.MachineModule, cncVariable, detection.DateTime.AddSeconds (-1));
              if ((null != oldCncVariable) && object.Equals (newCncVariable.Value, oldCncVariable.Value)) {
                log.DebugFormat ("CheckCncVariableChange: no cnc variable {0} change", cncVariable);
              }
              else {
                variableChange = true;
              }
            }
          }
        } // transaction
      }
      return variableChange;
    } // CheckCncVariableChange


    /// <summary>
    /// returns value of latest occurence of the specified key in CncVariable table
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetVariableString (DateTime dateTime, string key)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DetectionMakino.GetVariableString")) {
          var cncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
              .FindAt (m_machineModule, key, dateTime);

          if (null == cncVariable) {
            log.WarnFormat ("GetVariableString: no variable {0} at {1}, give up", key, dateTime);
            return null;
          }
          return cncVariable.Value.ToString ();
        }
      }
    }

    /// <summary>
    /// returns int value of latest occurence of the specified key in CncVariable table
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="key"></param>
    /// <returns></returns>    
    public int GetVariableInt (DateTime dateTime, string key)
    {
      var s = GetVariableString (dateTime, key);
      if (string.IsNullOrEmpty (s)) {
        return -1;
      }
      else {
        return int.Parse (s);
      }
    }

    /// <summary>
    /// Extract operation name from comment
    /// Regex must contain a "operation" group
    /// </summary>
    /// <param name="programOperationcomment"></param>
    /// <param name="operationRegex"></param>
    /// <returns></returns>    
    public string GetOperationNameFromComment (string programOperationcomment, Regex operationRegex)
    {
      log.DebugFormat ("GetOperationNameFromComment: programOperationcomment={0}", programOperationcomment);

      var match = operationRegex.Match (programOperationcomment.Trim ());
      string result = null;
      if (!match.Success) {
        log.WarnFormat ("GetOperationNameFromComment: no match with regex, invalid operationComment {0}", programOperationcomment);
      }
      else {
        if (match.Groups["operation"].Success) {
          result = match.Groups["operation"].Value.Trim ();
        }
      }
      return result;
    }

    /// <summary>
    /// Extract operation name and revision name from comment
    /// Regex must contain a "operation" and "revision" group
    /// </summary>
    /// <param name="programOperationcomment"></param>
    /// <param name="operationRegex"></param>
    /// <param name="operationName"></param>
    /// <param name="revisionName"></param>
    /// <returns></returns>    
    public bool GetOperationNameFromComment (string programOperationcomment, Regex operationRegex, out string operationName, out string revisionName)
    {
      log.DebugFormat ("GetOperationNameFromComment: programOperationcomment={0}", programOperationcomment);
      bool result = false;
      operationName = null;
      revisionName = null;

      if (!String.IsNullOrEmpty (programOperationcomment)) {
        var operationMatch = operationRegex.Match (programOperationcomment.Trim ());
        if (!operationMatch.Success) {
          log.WarnFormat ("GetOperationNameFromComment: no match with regex, invalid operationComment {0}", programOperationcomment);
        }
        else {
          if (operationMatch.Groups["operation"].Success) {
            operationName = operationMatch.Groups["operation"].Value.Trim ();
            log.DebugFormat ("GetOperationNameFromComment: operationName={0}", operationName);
            result = true;

            // check revision only if operation is ok
            if (operationMatch.Groups["revision"].Success) {
              revisionName = operationMatch.Groups["revision"].Value.Trim ();
              log.DebugFormat ("GetOperationNameFromComment: revisionName={0}", revisionName);
            }
            else {
              log.DebugFormat ("GetOperationNameFromComment: bad revision ");
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Extract operation items from comment
    /// Regex must contain one or several 'itemX' group, starting at 0. item0, item1, item2... 
    /// </summary>
    /// <param name="programOperationcomment"></param>
    /// <param name="operationRegex"></param>
    /// <param name="operationItems"></param>
    /// <returns></returns>    
    public bool GetOperationItemsFromComment (string programOperationcomment, Regex operationRegex, out List<string> operationItems)
    {
      log.Debug ($"GetOperationItemsFromComment: programOperationcomment={programOperationcomment}, regex={operationRegex}");
      bool result = false;
      operationItems = new List<string> ();

      if (!String.IsNullOrEmpty (programOperationcomment)) {
        var operationMatch = operationRegex.Match (programOperationcomment.Trim ());
        if (!operationMatch.Success) {
          log.Warn ($"GetOperationItemsFromComment: no match with regex, invalid operationComment {programOperationcomment}");
        }
        else {
          // check each item group, starting at item0
          int itemIndex = 0;
          string itemIndexName = "item" + itemIndex;
          foreach (string groupItem in operationRegex.GetGroupNames ()) {
            log.Debug ($"GetOperationItemsFromComment: looking for item {groupItem}");
            if (-1 != groupItem.IndexOf ("item")) {
              if (operationMatch.Groups[groupItem].Success) {
                string itemValue = operationMatch.Groups[groupItem].Value.Trim ();
                log.Debug ($"GetOperationItemsFromComment: item{itemIndex}={itemValue}");
                operationItems.Add (itemValue);
                log.Debug ($"GetOperationItemsFromComment: item{itemIndex}={itemValue} added");
                result = true;
              }
              else {
                log.Debug ($"GetOperationItemsFromComment: no match for item {itemIndex}");
                result = false;
                break;
              }
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Get the previous machining operation from program comment in cncvariable
    /// Parse previous operation until having a machining operation, but max 5 tries
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="programCommentVariable"></param>
    /// <returns></returns>  
    public IMachineCncVariable GetPreviousMachiningCncVariable (DateTime dateTime, string programCommentVariable)
    {
      log.DebugFormat ("GetPreviousMachiningProgramComment");
      string programComment = null;
      IMachineCncVariable machineCncVariable = null;
      int nbTries = 0;
      int maxTries = 5;
      DateTime previousDateTime = dateTime.AddSeconds (-1);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("DetectionMakino.GetPreviousMachiningProgramComment")) {
          while (null == programComment && nbTries < maxTries) {
            var cncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
             .FindAt (m_machineModule, programCommentVariable, previousDateTime);
            // check if in machinecncvariable
            if (null != cncVariable) {
              log.DebugFormat ("GetPreviousMachiningProgramComment: cncvariable: {0}", cncVariable.Value.ToString ());
              machineCncVariable = ModelDAOHelper.DAOFactory.MachineCncVariableDAO
                .FindByKeyValue (m_machineModule.MonitoredMachine, programCommentVariable, cncVariable.Value);
              if (null != machineCncVariable) {
                log.DebugFormat ("GetPreviousMachiningProgramComment: cncvariable in machineCncVariable");
                log.DebugFormat ("GetPreviousMachiningProgramComment: opid={0}", machineCncVariable.Operation);
                // program comment match previously seen machining operation
                programComment = cncVariable.Value.ToString ();

              }
              else {
                log.DebugFormat ("GetPreviousMachiningProgramComment: cncvariable not in machineCncVariable");
                nbTries++;
                previousDateTime = cncVariable.DateTimeRange.Lower.Value.AddSeconds (-1);
              }
            }
            else {
              log.DebugFormat ("GetPreviousMachiningProgramComment: no cncvariable with program comment");
              break;
            }
          }
          if (null == programComment) {
            log.DebugFormat ("GetPreviousMachiningProgramComment: no cncvariable with same program comment found");
          }
          transaction.Commit ();
        }
      }
      return machineCncVariable;
    }

    /// <summary>
    /// Get operation from config if exist, and create it if not exist
    /// Update machineCncVariable with latest operation code
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="revisionName"></param>
    /// <param name="previousOperation"></param>
    /// <param name="programComment"></param>
    /// <param name="machineFilter"></param>
    /// <param name="programCommentVariable"></param>
    /// <param name="operationNameVariable"></param>
    public IOperation GetOrCreateOperation (string operationName, string operationCode, bool checkCode, string revisionName, IOperation previousOperation, string programComment,
      IMachineFilter machineFilter, string programCommentVariable, string operationNameVariable)
    {
      IOperation operation = null;
      String partName = null;
      int separatorPosition = operationName.IndexOf ('-');
      if (separatorPosition > 0 && separatorPosition < operationName.Length - 1) {
        partName = operationName.Substring (operationName.IndexOf ('-') + 1);
      }
      else {
        partName = operationName; // by default
      }
      // if revisionName is available add it to operationName
      string actualOperationName = (string.IsNullOrEmpty (revisionName) ? operationName
        : operationName + "_" + revisionName);

      operation = GetOrCreateOperation (actualOperationName, operationCode, checkCode, false, partName, true, programComment, partName,
                                        machineFilter, programCommentVariable, operationNameVariable);


      return operation;
    }

    /// <summary>
    /// Get operation from config if exist, and create it if not exist
    /// Create job / part only if createJob / createPart is true
    /// Update machineCncVariable with latest operation code
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="operationCode"></param>
    /// <param name="createJob"></param>
    /// <param name="projectName"></param>
    /// <param name="createPart"></param>
    /// <param name="componentName"></param>
    /// <param name="programComment"></param>
    /// <param name="machineFilter"></param>
    /// <param name="programCommentVariable"></param>
    /// <param name="operationNameVariable"></param>
    public IOperation GetOrCreateOperation (string operationName, string operationCode, bool checkCode, bool createJob, string projectName, bool createPart, string componentName, string programComment, IMachineFilter machineFilter, string programCommentVariable, string operationNameVariable)
    {
      if (string.IsNullOrEmpty (operationName) || string.IsNullOrEmpty (projectName)) {
        log.Error ($"GetOrCreateOperation: Null project or operation. operation={operationName} project={projectName}");
        return null;
      }

      IOperation operation = null;
      ISimpleOperation simpleOperation = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.GetOrCreateOperation")) {

          log.Debug ($"GetOrCreateOperation: operation={operationName}");
          IComponentType componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);

          // get or create project
          IProject project = null;
          var projectList = ModelDAOHelper.DAOFactory.ProjectDAO.FindAll ().Where (p => string.Equals (projectName, p.Name, StringComparison.InvariantCulture));
          if (null == projectList || 0 == projectList.Count ()) {
            log.Debug ($"GetOrCreateOperation: create project {projectName}");
            project = ModelDAOHelper.ModelFactory.CreateProjectFromName (projectName);
            ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
          }
          else {
            // if several projects with same name (why?), take first one
            project = projectList.FirstOrDefault ();
          }
          if (null == project) {
            log.Error ($"GetOrCreateOperation: failed to create project {projectName} ");
            transaction.Rollback ();
            return null;
          }

          // get or create workorder
          IWorkOrder workorder = null;
          var workorderList = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindAll ().Where (w => string.Equals (projectName, w.Name, StringComparison.InvariantCulture));
          if (null == workorderList || 0 == workorderList.Count ()) {
            log.Debug ($"GetOrCreateOperation: create workorder {projectName}");
            workorder = ModelDAOHelper.ModelFactory.CreateWorkOrder (ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindById (1), projectName);
            project.AddWorkOrder (workorder);
            ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workorder);
          }
          else {
            // if several workorders with same name (why?), take first one
            workorder = workorderList.FirstOrDefault ();
          }
          if (null == workorder) {
            log.Error ($"GetOrCreateOperation: failed to create workorder {projectName}");
            transaction.Rollback ();
            return null;
          }

          // get or create job if needed
          if (createJob) {
            IJob job = ModelDAOHelper.DAOFactory.JobDAO.FindAll ().Where (j => string.Equals (projectName, j.Name,
              StringComparison.InvariantCulture)).FirstOrDefault ();
            if (null == job) {
              log.Debug ($"GetOrCreateOperation: create job {projectName}");
              job = ModelDAOHelper.ModelFactory.CreateJob (project);
              ModelDAOHelper.DAOFactory.JobDAO.MakePersistent (job);
              if (null == job) {
                log.Error ($"GetOrCreateOperation: failed to create job {projectName} ");
                transaction.Rollback ();
                return null;
              }
            }
          }


          // get or create component
          IComponent component = ModelDAOHelper.DAOFactory.ComponentDAO.FindByNameAndProject (componentName, project);
          if (null == component) {
            log.DebugFormat ($"GetOrCreateOperation: create component {componentName}");
            component = ModelDAOHelper.ModelFactory.CreateComponentFromName (project, componentName, componentType);
            ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
          }
          if (null == component) {
            log.Error ($"GetOrCreateOperation: failed to create component {componentName}, for project {projectName} ");
            transaction.Rollback ();
            return null;
          }

          // get or create part if needed
          if (createPart) {
            IPart part = null;
            part = ModelDAOHelper.DAOFactory.PartDAO.FindByName (componentName);
            if (null == part) {
              log.Debug ($"GetOrCreateOperation: create part {componentName}");
              part = ModelDAOHelper.ModelFactory.CreatePart (component);
              if (!string.IsNullOrEmpty (projectName)) {
                part.Project.Name = projectName;
              }
              ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
              if (null == part) {
                log.Error ($"GetOrCreateOperation: failed to create part {projectName} ");
                transaction.Rollback ();
                return null;
              }
            }
          }
          log.Debug ($"GetOrCreateOperation: find latest operation from component {component}");
          IOperation latestOperation = null;
          var intermediateWorkPieces = component.ComponentIntermediateWorkPieces
             .Select (ciwp => ciwp.IntermediateWorkPiece)
             .Where (iwp => string.Equals (operationName, iwp.Name, StringComparison.InvariantCulture));
          if (intermediateWorkPieces.Any ()) {
            log.Debug ($"GetOrCreateOperation: find latest operation from iwp");
            latestOperation = intermediateWorkPieces.Select (iwp => iwp.Operation).OrderBy (op => op.Id).LastOrDefault ();
          }
          else {
            log.Debug ($"GetOrCreateOperation: fail to find latest operation");
          }
          /*
          // find latest operation in DB from component
          log.Debug ($"GetOrCreateOperation: find latest operation in DB from component {component}");
          ISimpleOperation latestOperation = null;
          IEnumerable<ISimpleOperation> simpleOperations = ModelDAOHelper.DAOFactory.SimpleOperationDAO.FindAll ()
             .Where (sop => sop.Component.Id == component.Id && sop.Operation.Name.Equals (operationName));


          log.Debug ($"GetOrCreateOperation: find latest one from list");
          ModelDAOHelper.DAOFactory.Initialize (simpleOperations);
          if (null != simpleOperations) {
            log.Debug ($"GetOrCreateOperation: list not null");
            log.Debug ($"GetOrCreateOperation: list count={simpleOperations.Count()}");
            log.Debug ($"GetOrCreateOperation: sort list and get first");
            if (0 < simpleOperations.Count ()) {
              latestOperation = simpleOperations.OrderByDescending (sop => sop.OperationId).FirstOrDefault ();
            }
            else {
              log.Debug ($"GetOrCreateOperation: lsit empty skip");
              transaction.Rollback ();
              return null;
            }
          }
          else {
            log.Debug ($"GetOrCreateOperation: latest operation null or empty, skip");
            transaction.Rollback ();
            return null;
          }
          */

          log.Debug ($"GetOrCreateOperation: latest operation done {latestOperation}");

          // if operation not found or operation code is different (and checkCode is true), force create a new operation
          // if (null == latestOperation || ForceCreateNewOperation (latestOperation.Operation)) {
          //if (null == latestOperation || !latestOperation.Operation.Code.Equals (operationCode)) {
          if (null == latestOperation || (checkCode && !(null != latestOperation.Code && latestOperation.Code.Equals (operationCode)))) {
            // create a new operation
            log.DebugFormat ("GetOrCreateOperation: create operation {0}", operationName);
            var operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO
              .FindById (1); // undefined
            simpleOperation = ModelDAOHelper.ModelFactory.CreateSimpleOperation (operationType);
            simpleOperation.Name = operationName;
            simpleOperation.Code = operationCode;
            // simpleOperation.Code = actualOperationName;
            simpleOperation.Operation.Lock = false;
            simpleOperation.Operation.MachineFilter = machineFilter;
            ModelDAOHelper.DAOFactory.SimpleOperationDAO.MakePersistent (simpleOperation);
            var componentIntermediateWorkPiece = component.AddIntermediateWorkPiece (simpleOperation.IntermediateWorkPiece);
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (componentIntermediateWorkPiece);
            operation = simpleOperation.Operation;
            var path = ModelDAOHelper.ModelFactory.CreatePath ();
            path.Operation = operation;
            path.Number = 1;
            ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);
          }
          else {
            // operation already exists
            log.DebugFormat ("GetOrCreateOperation: existing operation {0}", operationName);
            operation = latestOperation;
          }

          // initialize operation items
          InitializeOperationItems (operation);

          if (null != operation) {
            // create a new operation in machineCncVariable
            log.DebugFormat ("GetOrCreateOperation: create a new operation in machineCncVariable");
            var machineCncVariable1 = ModelDAOHelper.ModelFactory
            .CreateMachineCncVariable (operationNameVariable, operationName);
            machineCncVariable1.MachineFilter = machineFilter;
            machineCncVariable1.Operation = operation;
            machineCncVariable1.Component = component;
            ModelDAOHelper.DAOFactory.MachineCncVariableDAO.MakePersistent (machineCncVariable1);

            log.DebugFormat ("GetOrCreateOperation: create a program comment in machineCncVariable");
            var machineCncVariable3 = ModelDAOHelper.ModelFactory
              .CreateMachineCncVariable (programCommentVariable, programComment);
            machineCncVariable3.MachineFilter = machineFilter;
            machineCncVariable3.Operation = operation;
            machineCncVariable3.Component = component;
            ModelDAOHelper.DAOFactory.MachineCncVariableDAO.MakePersistent (machineCncVariable3);
          }
          transaction.Commit ();
        }
      }
      return operation;
    }

    /// <summary>
    /// Get tools from program file content and create corresponding sequences.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="programFolder"></param>
    /// <param name="programFileName"></param>
    /// <param name="sequenceRegex"></param>
    /// <param name="dateTime"></param>
    public List<ISequence> CreateSequencesFromProgramFile (IOperation operation, string programFolder, string programFileName, Regex sequenceRegex, DateTime dateTime)
    {
      int sequenceCount = 0;
      m_previousSequenceKind = SequenceKind.Machining; // reinit to default
      List<ISequence> sequenceList = new List<ISequence> ();
      List<string> sequenceStringList = new List<string> ();
      int sequenceStringCount = 0;
      ISequence newSequence = null;

      log.DebugFormat ("CreateSequencesFromProgramFile: op={0}, folder={1}, file={2}", operation.Name, programFolder, programFileName);
      string fullPath = null;
      try {
        fullPath = Path.Combine (programFolder, programFileName);
        if (null != fullPath) {
          using (StreamReader reader = new StreamReader (fullPath)) {
            while (!reader.EndOfStream) {
              string line = reader.ReadLine ().Trim ();
              //log.DebugFormat ("CreateSequencesFromProgramFile: line={0}", line);
              var match = sequenceRegex.Match (line);
              if (match.Success) {
                if (match.Groups["sequenceName"].Success) {
                  log.DebugFormat ("CreateSequencesFromProgramFile: matching line={0}", line);
                  string sequenceName = match.Groups["sequenceName"].Value.Trim ();
                  sequenceStringList.Add (sequenceName);
                }
              }
            }
          }
          // remove multiple M0/M1 at end and keep only the first after last machineing sequence
          sequenceStringCount = sequenceStringList.Count;
          if (sequenceStringCount > 0) {
            for (int i = sequenceStringCount - 1; i > 1; i--) {
              if (sequenceStringList[i].StartsWith ("M") && sequenceStringList[i - 1].StartsWith ("M")) {
                log.DebugFormat ("CreateSequencesFromProgramFile: remove {0} at {1}, because {2} at {3}",
                  sequenceStringList[i], i, sequenceStringList[i - 1], i - 1);
                sequenceStringList.RemoveAt (i);
              }
              else {
                // last Txx machining found
                break;
              }
            }
          }
          // create actual sequences from list
          for (int i = 0; i < sequenceStringList.Count; i++) {
            log.DebugFormat ("CreateSequencesFromProgramFile: order={0}, sequence={1}", sequenceCount, sequenceStringList[i]);
            newSequence = CreateNewSequenceAtEnd (operation, sequenceStringList[i], dateTime);
            if (null != newSequence) {
              // add sequence in program sequence list
              sequenceList.Add (newSequence);
              sequenceCount++;
            }
          }
        }
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Lemoine.Threading.AbortException) {
        throw;
      }
      catch (Exception e) {
        log.ErrorFormat ("CreateSequencesFromProgramFile: failed to read file: {0}, {1}", fullPath, e);
      }
      log.DebugFormat ("CreateSequencesFromProgramFile: found {0} sequences", sequenceCount);
      return sequenceList;
    }

    /// <summary>
    /// create new sequence at end of operation sequences list.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="sequenceName"></param>
    /// <param name="dateTime"></param>    
    public ISequence CreateNewSequenceAtEnd (IOperation operation, string sequenceName, DateTime dateTime)
    {
      return CreateNewSequenceAtEnd (operation, sequenceName, null, null, dateTime);
    }

    /// <summary>
    /// create new sequence at end of operation sequences list.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="sequenceName"></param>
    /// <param name="operationStep"></param>
    /// <param name="pathNumber"></param>
    /// <param name="dateTime"></param>    
    public ISequence CreateNewSequenceAtEnd (IOperation operation, string sequenceName, int? operationStep, int? pathNumber, DateTime dateTime)
    {
      ISequence sequence = null;
      string toolNumber = null;
      SequenceKind sequenceKind = SequenceKind.Machining;
      log.Debug ($"CreateNewSequenceAtEnd: operation={operation.Name}, name={sequenceName}, step={operationStep}, path={pathNumber}");
      // do not create sequence if operation is locked
      if (operation.Lock) {
        log.ErrorFormat ("CreateNewSequenceAtEnd: operation {0} locked, give up", operation.Name);
        return null;
      }

      // 
      if (sequenceName.StartsWith ("M0") && !sequenceName.StartsWith ("M01")) {
        sequenceKind = SequenceKind.Stop;
      }
      else {
        if (sequenceName.StartsWith ("M1") || sequenceName.StartsWith ("M01")) {
          sequenceKind = SequenceKind.OptionalStop;
        }
        else {
          if (sequenceName.StartsWith ("M60")) {
            sequenceKind = SequenceKind.AutoPalletChange;
          }
          else {
            if (sequenceName.StartsWith ("T")) {
              sequenceKind = SequenceKind.Machining;
              toolNumber = sequenceName.Substring (1);
            }
          }
        }
      }

      if (null == pathNumber) {
        pathNumber = 1;   // default if not specified
      }

      // do not create new sequence if same kind
      if (sequenceKind != m_previousSequenceKind || sequenceKind == SequenceKind.Machining) {
        log.DebugFormat ("CreateNewSequenceAtEnd: sequencekind={0}, tool={1}", sequenceKind.ToString (), toolNumber);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.CreateNewSequenceAtEnd")) {
            // int order = (null == operation.Sequences) ? 1 : operation.Sequences.Count + 1;

            var path = operation.Paths.Where (p => p.Number == pathNumber).FirstOrDefault ();
            int order = 0;
            if (null == operation.Sequences) {
              order = 1;
            }
            else {
              order = (int)(operation.Sequences.Where (s => s.Path.Id == path.Id).Count ()) + 1;
            }
            log.Debug ($"CreateNewSequenceAtEnd: order={order}");
            sequence = ModelDAOHelper.ModelFactory.CreateSequence (sequenceName);
            sequence.ToolNumber = toolNumber;
            sequence.Operation = operation;
            sequence.Order = order;
            sequence.Path = path;
            sequence.Kind = sequenceKind;
            sequence.OperationStep = operationStep;
            //sequence.AutoOnly = (sequenceKind == SequenceKind.Machining);
            ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);
            ModelDAOHelper.DAOFactory.Initialize (sequence);
            ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
            transaction.Commit ();
          }
        }
        m_previousSequenceKind = sequenceKind;
      }
      else {
        log.DebugFormat ("CreateNewSequenceAtEnd: same sequencekind, ignore it");  // TODO
      }
      return sequence;
    }

    /// <summary>
    /// create new sequence before existing sequences.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="sequenceName"></param>
    /// <param name="dateTime"></param>    
    public ISequence CreateNewSequenceAtBegin (IOperation operation, string sequenceName)
    {
      ISequence sequence = null;
      string toolNumber = null;
      SequenceKind sequenceKind = SequenceKind.Machining;
      log.DebugFormat ("CreateNewSequenceAtBegin: operation={0}, name={1}",
        operation.Name, sequenceName);
      // do not create sequence if operation is locked
      if (operation.Lock) {
        log.ErrorFormat ("CreateNewSequenceAtBegin: operation {0} locked, give up", operation.Name);
        return null;
      }

      // 
      if (sequenceName.StartsWith ("M0") && !sequenceName.StartsWith ("M01")) {
        sequenceKind = SequenceKind.Stop;
      }
      else {
        if (sequenceName.StartsWith ("M1") || sequenceName.StartsWith ("M01")) {
          sequenceKind = SequenceKind.OptionalStop;
        }
        else {
          if (sequenceName.StartsWith ("M60")) {
            sequenceKind = SequenceKind.AutoPalletChange;
          }
          else {
            if (sequenceName.StartsWith ("T")) {
              sequenceKind = SequenceKind.Machining;
              toolNumber = sequenceName.Substring (1);
            }
          }
        }
      }

      log.DebugFormat ("CreateNewSequenceAtBegin: sequencekind={0}, tool={1}", sequenceKind.ToString (), toolNumber);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.CreateNewSequenceAtBegin")) {
          // shift existing sequences orders
          foreach (var sequenceItem in operation.Sequences) {
            sequenceItem.Order = sequenceItem.Order + 1;
            ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequenceItem);
          }
          // create sequence at beginning
          int order = 1;
          var path = operation.Paths.First ();
          sequence = ModelDAOHelper.ModelFactory.CreateSequence (sequenceName);
          sequence.ToolNumber = toolNumber;
          sequence.Operation = operation;
          sequence.Order = order;
          sequence.Path = path;
          sequence.Kind = sequenceKind;
          //sequence.AutoOnly = (sequenceKind == SequenceKind.Machining);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);

          ModelDAOHelper.DAOFactory.Initialize (sequence);
          ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
          transaction.Commit ();
        }
      }
      return sequence;
    }

    /// <summary>
    /// create new sequence before existing sequences.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="sequenceName"></param>
    /// <param name="dateTime"></param>    
    ISequence CreateNewSequence (IOperation operation, int order, int toolNumber, SequenceKind sequenceKind)
    {
      ISequence sequence = null;
      string sequenceName = null;
      log.DebugFormat ("CreateNewSequenceAtBegin: operation={0}, name={1}",
        operation.Name, sequenceName);
      // do not create sequence if operation is locked
      if (operation.Lock) {
        log.ErrorFormat ("CreateNewSequenceAtBegin: operation {0} locked, give up", operation.Name);
        return null;
      }
      string sequencePrefix = null;
      switch (sequenceKind) {
        case SequenceKind.OptionalStop:
          sequencePrefix = "M1T";
          break;
        case SequenceKind.Stop:
          sequencePrefix = "M0T";
          break;
        default:
          sequencePrefix = "T";
          break;
      }
      sequenceName = sequencePrefix + toolNumber;


      log.Debug ($"CreateNewSequenceAtBegin: sequence={sequencePrefix}{toolNumber}");
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.CreateNewSequenceAtBegin")) {
          // shift existing sequences orders
          var afterSequences = operation.Sequences
           .Where (s => order <= s.Order);
          foreach (var afterSequence in afterSequences) {
            afterSequence.Order = afterSequence.Order + 1;
            ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (afterSequence);
          }
          // create sequence at beginning
          var path = operation.Paths.First ();
          sequence = ModelDAOHelper.ModelFactory.CreateSequence (sequenceName);
          sequence.ToolNumber = toolNumber.ToString ();
          sequence.Operation = operation;
          sequence.Order = order;
          sequence.Path = path;
          sequence.Kind = sequenceKind;
          //sequence.AutoOnly = (sequenceKind == SequenceKind.Machining);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);

          ModelDAOHelper.DAOFactory.Initialize (sequence);
          ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
          transaction.Commit ();
        }
      }
      return sequence;
    }

    /// <summary>
    /// Check if program file contains the revision name, the line after "revisionTag" (E.g. "UPLOADED/REVISED")
    /// </summary>
    /// <param name="programFolder"></param>
    /// <param name="programFileName"></param>
    /// <param name="revisionDate"></param>
    /// <param name="revisionTag"></param>
    /// <param name="revisionDateFormatRegex"></param>
    /// <param name="maxLinesToRead">max line to read to find tag, 0 if no limit</param>
    public bool CheckRevisionNameInProgramFile (string programFolder, string programFileName, string revisionDate,
      string revisionTag, Regex revisionDateFormatRegex, int maxLinesToRead)
    {
      int nbLinesRead = 0;
      bool revisionTagFound = false;
      bool revisionNameMatches = false;

      log.DebugFormat ("CheckRevisionNameInProgramFile: folder={0}, file={1}, revision={2}", programFolder, programFileName, revisionDate);
      string fullPath = null;
      try {
        fullPath = Path.Combine (programFolder, programFileName);
        if (null != fullPath) {
          using (StreamReader reader = new StreamReader (fullPath)) {
            while (!reader.EndOfStream && (maxLinesToRead == 0 || (maxLinesToRead > 0 && nbLinesRead < maxLinesToRead))) {
              string line = reader.ReadLine ().Trim ();
              nbLinesRead++;
              // skip non-comment lines
              if (line.StartsWith ("(")) {
                if (revisionTagFound) {
                  // revision tag found on previous line, current line must contain revision name
                  var match = revisionDateFormatRegex.Match (line.Trim ());
                  string dateInFile = null;
                  if (!match.Success) {
                    log.WarnFormat ("CheckRevisionNameInProgramFile: no match with regex, invalid revision date format {0}", line);
                  }
                  else {
                    if (match.Groups["date"].Success) {
                      dateInFile = match.Groups["date"].Value.Trim ();
                      // then compare revisions date as date
                      DateTime runningRevisionDate = Convert.ToDateTime (revisionDate);
                      DateTime fileRevisionDate = Convert.ToDateTime (dateInFile);
                      if (0 == DateTime.Compare (runningRevisionDate, fileRevisionDate)) {
                        log.DebugFormat ("CheckRevisionNameInProgramFile: found revision={0} as {1} in line {2}",
                          revisionDate, dateInFile, line);
                        revisionNameMatches = true;
                      }
                    }
                    else {
                      log.WarnFormat ("CheckRevisionNameInProgramFile: no match with regex, invalid revision date {0}", line);
                    }
                  }
                  break;
                }
                else {
                  if (line.Contains (revisionTag)) {
                    revisionTagFound = true;
                  }
                }
              }
            }
          }
        }
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Lemoine.Threading.AbortException) {
        throw;
      }
      catch (Exception e) {
        log.ErrorFormat ("CheckRevisionNameInProgramFile: failed to read file: {0}, {1}", fullPath, e);
      }
      if (!revisionTagFound) {
        log.DebugFormat ("CheckRevisionNameInProgramFile: no revision tag found");
      }
      return revisionNameMatches;
    }

    /// <summary>
    /// initialize operation items
    /// <param name="operation"></param>
    /// </summary>
    public void InitializeOperationItems (IOperation operation)
    {
      log.Debug ("InitializeOperationItems:");
      // initialize operation items
      ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
      ModelDAOHelper.DAOFactory.Initialize (operation.Paths);
      ModelDAOHelper.DAOFactory.Initialize (operation.IntermediateWorkPieces);
      List<IIntermediateWorkPiece> intermediateWorkPieceItems = operation.IntermediateWorkPieces.ToList ();
      foreach (IIntermediateWorkPiece intermediateWorkPieceItem in intermediateWorkPieceItems) {
        if (null != intermediateWorkPieceItem) {
          ModelDAOHelper.DAOFactory.Initialize (intermediateWorkPieceItem.ComponentIntermediateWorkPieces);
          foreach (IComponentIntermediateWorkPiece componentiwp in intermediateWorkPieceItem.ComponentIntermediateWorkPieces) {
            ModelDAOHelper.DAOFactory.Initialize (componentiwp.Component);
            ModelDAOHelper.DAOFactory.Initialize (componentiwp.Component.Project);
            foreach (IWorkOrder workOrder in componentiwp.Component.Project.WorkOrders) {
              ModelDAOHelper.DAOFactory.Initialize (workOrder);
            }
          }
        }
      }
    }

    /// <summary>
    /// Cycle detection datetime
    /// </summary>
    public DateTime? GetCycleDetectionDateTime ()
    {
      return m_cycleDetectionStatus.GetCycleDetectionDateTime ();
    }

    /// <summary>
    /// Unlock operation
    /// <param name="operation"></param>
    /// </summary>
    public void UnlockOperation (IOperation operation)
    {
      if (null != operation && operation.Lock) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.unlockOperation")) {
            operation.Lock = false;
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
            transaction.Commit ();
          }
        }
      }
    }

    /// <summary>
    /// Lock operation
    /// <param name="operation"></param>
    /// </summary>
    public void LockOperation (IOperation operation)
    {
      if (null != operation && !operation.Lock) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("CncVariablesDetectionAnalysis.lockOperation")) {
            operation.Lock = true;
            ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
            transaction.Commit ();
          }
        }
      }
    }
  }
}
