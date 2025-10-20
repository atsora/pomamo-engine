// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Pulse.PluginImplementation.Analysis
{
  /// <summary>
  /// Extract informations from a program comment
  /// 
  /// The supported group names are:
  /// <item>jobName</item>
  /// <item>jobComponent</item>
  /// <item>projectName</item>
  /// <item>projectComponent</item>
  /// <item>componentName</item>
  /// <item>componentCode</item>
  /// <item>partName</item>
  /// <item>partCode</item>
  /// <item>opName</item>
  /// <item>opCode</item>
  /// <item>op1Name / op2Name</item>
  /// <item>op1Code / op2Code</item>
  /// <item>qty</item>
  /// <item>qty1</item>
  /// <item>qty2</item>
  /// </summary>
  public class ProgramCommentParser
  {
    static readonly ILog log = LogManager.GetLogger<ProgramCommentParser> ();

    readonly Regex m_regex;
    readonly string m_programComment;
    readonly IEnumerable<IRegexMatchToFileOpDataExtension> m_regexMatchToOpDatas;
    readonly ISequenceCreatorExtension m_sequenceCreator;
    readonly string m_defaultOpName = "";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="programComment"></param>
    public ProgramCommentParser (Regex regex, string programComment, IEnumerable<IRegexMatchToFileOpDataExtension> regexMatchToOpDatas = null, ISequenceCreatorExtension sequenceCreator = null, string defaultOpName = "")
    {
      m_regex = regex;
      m_programComment = programComment;
      m_regexMatchToOpDatas = regexMatchToOpDatas;
      m_sequenceCreator = sequenceCreator;
      m_defaultOpName = defaultOpName;
    }

    bool TryGetOpData (Match match, string dataName, out string opData)
    {
      if (null != m_regexMatchToOpDatas) {
        foreach (var regexMatchToOpData in m_regexMatchToOpDatas) {
          try {
            opData = regexMatchToOpData.GetOpData (match, dataName);
            return true;
          }
          catch { }
        }
      }
      opData = null;
      return false;
    }

    /// <summary>
    /// Create the sequence(s):
    /// one by pallet
    /// 
    /// Deprecated: use ISequenceCreateExtension now
    /// </summary>
    public bool CreateSequences { get; set; } = false;

    IProject GetProject (Match match)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        // Check first projectName, then projectCode
        if (!TryGetOpData (match, "projectName", out var projectName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetProject: no valid regexMatchToOpData for projectName");
          }
          var projectNameGroup = match.Groups["projectName"];
          if (projectNameGroup.Success) {
            projectName = projectNameGroup.Value.Trim ();
            if (string.IsNullOrEmpty (projectName)) {
              log.Warn ($"GetProject: projectName is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetProject: projectName={projectName} in {m_programComment} for {m_regex}");
              }
            }
          }
        }

        if (!TryGetOpData (match, "projectCode", out var projectCode)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetProject: no valid regexMatchToOpData for projectCode");
          }
          var projectCodeGroup = match.Groups["projectCode"];
          if (projectCodeGroup.Success) {
            projectCode = projectCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (projectCode)) {
              log.Warn ($"GetProject: projectCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetProject: projectCode={projectCode} in {m_programComment} for regex {m_regex}");
              }
            }
          }
        }

        projectName = projectName?.Trim ();
        projectCode = projectCode?.Trim ();
        if (string.IsNullOrEmpty (projectName) && string.IsNullOrEmpty (projectCode)) {
          log.Error ($"GetProject: no project information in {m_programComment}, regex={m_regex}");
          return null;
        }

        IProject project = null;
        if (!string.IsNullOrEmpty (projectName)) {
          project = ModelDAOHelper.DAOFactory.ProjectDAO.FindByName (projectName);
          if ((null != project)
            && !string.IsNullOrEmpty (projectCode)
            && (!project.Code.Equals (projectCode, StringComparison.InvariantCultureIgnoreCase))) {
            log.Warn ($"GetProject: project with name {projectName} has a code {project.Code} that does not match {projectCode}");
          }
        }
        else if (!string.IsNullOrEmpty (projectCode)) {
          project = ModelDAOHelper.DAOFactory.ProjectDAO.FindByCode (projectCode);
        }
        if (project is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetProject: projectName={projectName} does not exist, create it");
          }
          var workorderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
            .FindById (1); // 1: undefined
          project = ModelDAOHelper.ModelFactory.CreateProjectFromName (projectName);
          project.Code = projectCode;
          ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
        }

        return project;
      }
    }

    IJob GetJob (Match match)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        // Check first jobName, then jobCode
        if (!TryGetOpData (match, "jobName", out var jobName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetJob: no valid regexMatchToOpData for jobName");
          }
          var jobNameGroup = match.Groups["jobName"];
          if (jobNameGroup.Success) {
            jobName = jobNameGroup.Value.Trim ();
            if (string.IsNullOrEmpty (jobName)) {
              log.Warn ($"GetJob: jobName is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetJob: jobName={jobName} in {m_programComment} for {m_regex}");
              }
            }
          }
        }

        if (!TryGetOpData (match, "jobCode", out var jobCode)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetJob: no valid regexMatchToOpData for jobCode");
          }
          var jobCodeGroup = match.Groups["jobCode"];
          if (jobCodeGroup.Success) {
            jobCode = jobCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (jobCode)) {
              log.Warn ($"GetJob: jobCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetJob: jobCode={jobCode} in {m_programComment} for regex {m_regex}");
              }
            }
          }
        }

        jobName = jobName?.Trim ();
        jobCode = jobCode?.Trim ();
        if (string.IsNullOrEmpty (jobName) && string.IsNullOrEmpty (jobCode)) {
          log.Error ($"GetJob: no job information in {m_programComment}, regex={m_regex}");
          return null;
        }

        IJob job = null;
        if (!string.IsNullOrEmpty (jobName)) {
          job = ModelDAOHelper.DAOFactory.ProjectDAO.FindByName (jobName)?.Job;
          if ((null != job)
            && !string.IsNullOrEmpty (jobCode)
            && (!job.Code.Equals (jobCode, StringComparison.InvariantCultureIgnoreCase))) {
            log.Warn ($"GetJob: job with name {jobName} has a code {job.Code} that does not match {jobCode}");
          }
        }
        else if (!string.IsNullOrEmpty (jobCode)) {
          job = ModelDAOHelper.DAOFactory.ProjectDAO.FindByCode (jobCode)?.Job;
        }
        if (job is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetJob: jobName={jobName} does not exist, create it");
          }
          var workorderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
            .FindById (1); // 1: undefined
          job = ModelDAOHelper.ModelFactory.CreateJobFromName (workorderStatus, jobName);
          job.Code = jobCode;
          ModelDAOHelper.DAOFactory.JobDAO.MakePersistent (job);
        }

        return job;
      }
    }

    IPart GetPart (Match match)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        // Check first partName, then partCode
        if (!TryGetOpData (match, "partName", out var partName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPart: no valid regexMatchToOpData for partName");
          }
          var partNameGroup = match.Groups["partName"];
          if (partNameGroup.Success) {
            partName = partNameGroup.Value.Trim ();
            if (string.IsNullOrEmpty (partName)) {
              log.Warn ($"GetPart: partName is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPart: partName={partName} in {m_programComment} for {m_regex}");
              }
            }
          }
        }

        if (!TryGetOpData (match, "partCode", out var partCode)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPart: no valid regexMatchToOpData for partCode");
          }
          var partCodeGroup = match.Groups["partCode"];
          if (partCodeGroup.Success) {
            partCode = partCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (partCode)) {
              log.Warn ($"GetPart: partCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPart: partCode={partCode} in {m_programComment} for regex {m_regex}");
              }
            }
          }
        }

        partName = partName?.Trim ();
        partCode = partCode?.Trim ();
        if (string.IsNullOrEmpty (partName) && string.IsNullOrEmpty (partCode)) {
          log.Error ($"GetPart: no part information in {m_programComment}, regex={m_regex}");
          return null;
        }

        IPart part = null;
        if (!string.IsNullOrEmpty (partName)) {
          part = ModelDAOHelper.DAOFactory.PartDAO.FindByName (partName);
          if ((null != part)
            && !string.IsNullOrEmpty (partCode)
            && (!part.Code.Equals (partCode, StringComparison.InvariantCultureIgnoreCase))) {
            log.Warn ($"GetPart: part with name {partName} has a code {part.Code} that does not match {partCode}");
          }
        }
        else if (!string.IsNullOrEmpty (partCode)) {
          part = ModelDAOHelper.DAOFactory.PartDAO.FindByCode (partCode);
        }
        if (part is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPart: partName={partName} does not exist, create it");
          }
          var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO
            .FindById (1); // 1: undefined
          part = ModelDAOHelper.ModelFactory.CreatePart (componentType);
          part.Name = partName;
          part.Code = partCode;
          ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
        }

        return part;
      }
    }

    IComponent GetComponent (IProject project, Match match)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        // Check first componentName, then componentCode
        if (!TryGetOpData (match, "componentName", out var componentName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetComponent: no valid regexMatchToOpData for componentName");
          }
          var componentNameGroup = match.Groups["componentName"];
          if (componentNameGroup.Success) {
            componentName = componentNameGroup.Value.Trim ();
            if (string.IsNullOrEmpty (componentName)) {
              log.Warn ($"GetComponent: componentName is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetComponent: componentName={componentName} in {m_programComment} for {m_regex}");
              }
            }
          }
        }

        if (!TryGetOpData (match, "componentCode", out var componentCode)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetComponent: no valid regexMatchToOpData for componentCode");
          }
          var componentCodeGroup = match.Groups["componentCode"];
          if (componentCodeGroup.Success) {
            componentCode = componentCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (componentCode)) {
              log.Warn ($"GetComponent: componentCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetComponent: componentCode={componentCode} in {m_programComment} for regex {m_regex}");
              }
            }
          }
        }

        componentName = componentName?.Trim ();
        componentCode = componentCode?.Trim ();
        if (string.IsNullOrEmpty (componentName) && string.IsNullOrEmpty (componentCode)) {
          log.Error ($"GetComponent: no component information in {m_programComment}, regex={m_regex}");
          return null;
        }

        IComponent component = null;
        if (!string.IsNullOrEmpty (componentName)) {
          component = ModelDAOHelper.DAOFactory.ComponentDAO.FindByNameAndProject (componentName, project);
          if ((null != component)
            && !string.IsNullOrEmpty (componentCode)
            && (!component.Code.Equals (componentCode, StringComparison.InvariantCultureIgnoreCase))) {
            log.Warn ($"GetComponent: component with name {componentName} has a code {component.Code} that does not match {componentCode}");
          }
        }
        else if (!string.IsNullOrEmpty (componentCode)) {
          component = ModelDAOHelper.DAOFactory.ComponentDAO.FindByCodeAndProject (componentCode, project);
        }
        if (component is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetComponent: componentName={componentName} does not exist, create it");
          }
          var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO
            .FindById (1); // 1: undefined
          component = ModelDAOHelper.ModelFactory.CreateComponentFromType (project, componentType);
          component.Name = componentName;
          component.Code = componentCode;
          ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
        }

        return component;
      }
    }

    /// <summary>
    /// Get the operation
    /// </summary>
    /// <param name="component">not null</param>
    /// <param name="match"></param>
    /// <returns></returns>
    IOperation GetOperation (IComponent component, Match match)
    {
      Debug.Assert (null != component);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        var operations = component
          .ComponentIntermediateWorkPieces
          .Select (ciwp => ciwp.IntermediateWorkPiece.Operation)
          .Distinct ();
        IOperation operation = null;
        IIntermediateWorkPiece intermediateWorkPiece = null;

        bool op1op2 = false;

        // opName / op1Name / op2Name
        string op1Name = null;
        string op2Name = null;
        if (!TryGetOpData (match, "opName", out var opName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: no valid regexMatchToOperationName for opName");
          }
          var opNameGroup = match.Groups["opName"];
          if (opNameGroup.Success) {
            opName = opNameGroup.Value.Trim ();
          }
          if (!TryGetOpData (match, "op1Name", out op1Name)) {
            var op1NameGroup = match.Groups["op1Name"];
            if (op1NameGroup.Success) {
              op1Name = op1NameGroup.Value.Trim ();
            }
          }
          if (!TryGetOpData (match, "op2Name", out op2Name)) {
            var op2NameGroup = match.Groups["op2Name"];
            if (op2NameGroup.Success) {
              op2Name = op2NameGroup.Value.Trim ();
            }
          }
          if (string.IsNullOrEmpty (opName)) {
            if (string.IsNullOrEmpty (op1Name)) {
              log.Debug ($"GetOperation: nor opName, nor op1Name is defined");
            }
            else if (string.IsNullOrEmpty (op2Name)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op op2Name => opName=op1Name={op1Name}");
              }
              opName = op1Name;
            }
            else if (string.Equals (op1Name, op2Name, StringComparison.InvariantCultureIgnoreCase)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op1Name=op2Name=opName={op1Name}");
              }
              opName = op1Name;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op1Name={op1Name} op2Name={op2Name}");
              }
              op1op2 = true;
              opName = $"{op1Name}/{op2Name}";
            }
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: opName={opName} from regex");
          }
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"GetOperation: opName={opName} from plugin");
        }

        // opCode / op1Code / op2Code
        string op1Code = null;
        string op2Code = null;
        if (!TryGetOpData (match, "opCode", out var opCode)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: no valid regexMatchToOperationName for opCode");
          }
          var opCodeGroup = match.Groups["opCode"];
          if (opCodeGroup.Success) {
            opCode = opCodeGroup.Value.Trim ();
          }
          if (!TryGetOpData (match, "op1Code", out op1Code)) {
            var op1CodeGroup = match.Groups["op1Code"];
            if (op1CodeGroup.Success) {
              op1Code = op1CodeGroup.Value.Trim ();
            }
          }
          if (!TryGetOpData (match, "op2Code", out op2Code)) {
            var op2CodeGroup = match.Groups["op2Code"];
            if (op2CodeGroup.Success) {
              op2Code = op2CodeGroup.Value.Trim ();
            }
          }
          if (string.IsNullOrEmpty (opCode)) {
            if (string.IsNullOrEmpty (op1Code)) {
              log.Debug ($"GetOperation: nor opCode, nor op1Code is defined");
            }
            else if (string.IsNullOrEmpty (op2Code)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op op2Code => opCode=op1Code={op1Code}");
              }
              opCode = op1Code;
            }
            else if (string.Equals (op1Code, op2Code, StringComparison.InvariantCultureIgnoreCase)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op1Code=op2Code=opCode={op1Code}");
              }
              opCode = op1Code;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: op1Code={op1Code} op2Code={op2Code}");
              }
              op1op2 = true;
              opCode = $"{op1Code}/{op2Code}";
            }
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: opCode={opCode} from regex");
          }
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"GetOperation: opCode={opCode} from plugin");
        }

        if (string.IsNullOrEmpty (opName) && string.IsNullOrEmpty (opCode)) {
          if (string.IsNullOrEmpty (m_defaultOpName)) {
            log.Warn ($"GetOperation: opName and opCode are empty in {m_programComment} for regex {m_regex}");
            return null;
          }
          else {
            log.Info ($"GetOperation: use default op name={m_defaultOpName} since there is no opCode or opName in regex");
            opName = m_defaultOpName;
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"GetOperation: opName={opName}, opCode={opCode} in {m_programComment} for {m_regex}");
        }
        var matchingOperations = operations
          .Where (x => string.IsNullOrEmpty (opName) || string.Equals (x.Name, opName, StringComparison.InvariantCultureIgnoreCase))
          .Where (x => string.IsNullOrEmpty (opCode) || string.Equals (x.Code, opCode, StringComparison.InvariantCultureIgnoreCase))
          .Where (x => !x.ArchiveDateTime.HasValue);
        if (2 <= matchingOperations.Count ()) {
          log.Warn ($"GetOperation: more than one operation with opName={opName} opCode={opCode} in {m_programComment} for {m_regex} => take the first one");
          operation = matchingOperations.FirstOrDefault ();
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: single operation for opName={opName} opCode={opCode} in {m_programComment} for {m_regex}");
          }
          operation = matchingOperations.SingleOrDefault ();
        }
        if (null != operation) {
          ModelDAOHelper.DAOFactory.Initialize (operation.IntermediateWorkPieces);
          ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
          if ((null != m_sequenceCreator) && !operation.Sequences.Any ()) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetOperation: create sequences");
            }
            try {
              m_sequenceCreator.CreateSequences (operation, match);
            }
            catch (Exception ex) {
              log.Error ($"GetOperation: exception in CreateSequences", ex);
            }
          }
        }
        else { // operation is null
          if (log.IsDebugEnabled) {
            log.Debug ($"GetOperation: opName={opName} opCode={opCode} does not exist, create it");
          }

          // qty / qty1 / qty2
          int qty = 0;
          int qty1 = 0;
          int qty2 = 0;
          var qtyGroup = match.Groups["qty"];
          if (qtyGroup.Success && int.TryParse (qtyGroup.Value.Trim (), out qty) && log.IsDebugEnabled) {
            log.Debug ($"GetOperation: qty={qty}");
          }
          var qty1Group = match.Groups["qty1"];
          if (qty1Group.Success && int.TryParse (qty1Group.Value.Trim (), out qty1) && log.IsDebugEnabled) {
            log.Debug ($"GetOperation: qty1={qty1}");
          }
          var qty2Group = match.Groups["qty2"];
          if (qty2Group.Success && int.TryParse (qty2Group.Value.Trim (), out qty2) && log.IsDebugEnabled) {
            log.Debug ($"GetOperation: qty2={qty2}");
          }
          if ((0 == qty) && (0 < qty1)) {
            qty = qty1 + qty2;
            if (log.IsDebugEnabled) {
              log.Debug ($"GetOperation: consider qty=qty1+qty2={qty1}+{qty2}={qty}");
            }
          }

          var operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO
            .FindById (1);
          operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
          operation.Name = opName;
          operation.Code = opCode;
          operation.Lock = false;
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
          if (op1op2) {
            var iwp1 = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece (operation);
            iwp1.Name = op1Name;
            iwp1.Code = op1Code;
            if (0 < qty1) {
              iwp1.OperationQuantity = qty1;
            }
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (iwp1);
            var l1 = component.AddIntermediateWorkPiece (iwp1);
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (l1);
            var iwp2 = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece (operation);
            iwp2.Name = op2Name;
            iwp2.Code = op2Code;
            if (0 < qty2) {
              iwp2.OperationQuantity = qty2;
            }
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (iwp2);
            var l2 = component.AddIntermediateWorkPiece (iwp2);
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (l2);
            ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
          }
          else { // !op1op2
            intermediateWorkPiece = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece (operation);
            intermediateWorkPiece.Name = opName;
            intermediateWorkPiece.Code = opCode;
            if (0 < qty) {
              intermediateWorkPiece.OperationQuantity = qty;
            }
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
            var l = component.AddIntermediateWorkPiece (intermediateWorkPiece);
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (l);
            ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
          }
          if (null != m_sequenceCreator) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetOperation: create sequences");
            }
            try {
              m_sequenceCreator.CreateSequences (operation, match);
            }
            catch (Exception ex) {
              log.Error ($"GetOperation: exception in CreateSequences", ex);
            }
          }
          else if (this.CreateSequences) {
            var path = ModelDAOHelper.ModelFactory.CreatePath (operation);
            path.Number = 1;
            ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);
            if (op1op2) {
              var sequence1 = ModelDAOHelper.ModelFactory.CreateSequence (op1Name ?? op1Code);
              sequence1.Operation = operation;
              sequence1.Path = path;
              sequence1.Order = 1;
              sequence1.OperationStep = 1;
              sequence1.Kind = SequenceKind.Machining;
              ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence1);
              var sequence2 = ModelDAOHelper.ModelFactory.CreateSequence (op2Name ?? op2Code);
              sequence2.Operation = operation;
              sequence2.Path = path;
              sequence2.Order = 2;
              sequence2.OperationStep = 2;
              sequence2.Kind = SequenceKind.Machining;
              ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence2);
            }
            else { // !op1op2
              var sequence1 = ModelDAOHelper.ModelFactory.CreateSequence (op1Name ?? op1Code);
              sequence1.Operation = operation;
              sequence1.Path = path;
              sequence1.Order = 1;
              sequence1.OperationStep = 1;
              sequence1.Kind = SequenceKind.Machining;
              ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence1);
            }
          }
        }

        return operation;
      }
    }

    public IOperation GetOperation ()
    {
      if (string.IsNullOrEmpty (m_programComment)) {
        log.Debug ($"GetOperation: empty program comment => return null");
        return null;
      }

      var match = m_regex.Match (m_programComment);
      if (!match.Success) {
        log.Warn ($"GetOperation: program commnet {m_programComment} does not match regex {m_regex}");
        return null;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IProject project = null;
        IComponent component = null;
        bool projectComponentIsPart = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart));
        bool workOrderProjectIsJob = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob));
        if (workOrderProjectIsJob) {
          var job = GetJob (match);
          project = job.Project;
          component = GetComponent (project, match);
        }
        else if (projectComponentIsPart) {
          var part = GetPart (match);
          project = part.Project;
          component = part.Component;
        }
        else { // WorkOrder > Project > Component
          project = GetProject (match);
          component = GetComponent (project, match);
        }
        if (component is null) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetOperation: component/part is null in {m_programComment} for regex {m_regex} => return null");
          }
          return null;
        }
        var operation = GetOperation (component, match);
        if (operation is null) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetOperation: operation is null in {m_programComment} for regex {m_regex} => return null");
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetOperation: got operation id={operation.Id}");
        }
        return operation;
      }
    }
  }
}
