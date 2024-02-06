// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pulse.PluginImplementation.Analysis
{
  /// <summary>
  /// Extract informations from a program comment
  /// 
  /// The supported group names are:
  /// <item>partName</item>
  /// <item>partCode</item>
  /// <item>opName</item>
  /// <item>opCode</item>
  /// <item>qty</item>
  /// <item>qty1</item>
  /// <item>qty2</item>
  /// </summary>
  public class ProgramCommentParser
  {
    static readonly ILog log = LogManager.GetLogger<ProgramCommentParser> ();

    readonly Regex m_regex;
    readonly string m_programComment;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="programComment"></param>
    public ProgramCommentParser (Regex regex, string programComment)
    {
      m_regex = regex;
      m_programComment = programComment;
    }

    IPart GetPart (Match match)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        {
          var partNameGroup = match.Groups["partName"];
          if (partNameGroup.Success) { // Check first partName, then partCode
            var partName = partNameGroup.Value.Trim ();
            if (string.IsNullOrEmpty (partName)) {
              log.Warn ($"GetPart: partName is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPart: partName={partName} in {m_programComment} for {m_regex}");
              }
              var part = ModelDAOHelper.DAOFactory.PartDAO.FindByName (partName);
              if (part is null) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetPart: partName={partName} does not exist, create it");
                }
                var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO
                  .FindById (1); // 1: undefined
                part = ModelDAOHelper.ModelFactory.CreatePart (componentType);
                part.Name = partName;
                ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
              }
              return part;
            }
          }
        }

        // Try with partCode
        {
          var partCodeGroup = match.Groups["partCode"];
          if (partCodeGroup.Success) {
            var partCode = partCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (partCode)) {
              log.Warn ($"GetPart: partCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPart: partCode={partCode} in {m_programComment} for regex {m_regex}");
              }
              var part = ModelDAOHelper.DAOFactory.PartDAO.FindByCode (partCode);
              if (part is null) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetPart: partCode={partCode} does not exist, create it");
                }
                var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO
                  .FindById (1); // 1: undefined
                part = ModelDAOHelper.ModelFactory.CreatePart (componentType);
                part.Code = partCode;
                ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
              }
              return part;
            }
          }
        }

        log.Error ($"GetPart: no part information in {m_programComment}, regex={m_regex}");
        return null;
      }
    }

    /// <summary>
    /// Get the operation
    /// </summary>
    /// <param name="part">not null</param>
    /// <param name="match"></param>
    /// <returns></returns>
    IOperation GetOperation (IPart part, Match match)
    {
      Debug.Assert (null != part);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        var operations = part
          .ComponentIntermediateWorkPieces
          .ToList ()
          .Select (ciwp => ciwp.IntermediateWorkPiece.Operation);
        IOperation operation = null;
        IIntermediateWorkPiece intermediateWorkPiece = null;

        var opNameGroup = match.Groups["opName"];
        if (opNameGroup.Success) { // Check first opName, then opCode
          var opName = opNameGroup.Value.Trim ();
          if (string.IsNullOrEmpty (opName)) {
            log.Warn ($"GetOperation: opName is empty in {m_programComment} for regex {m_regex}");
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetOperation: opName={opName} in {m_programComment} for {m_regex}");
            }
            operation = operations
              .FirstOrDefault (x => string.Equals (x.Name, opName, StringComparison.InvariantCultureIgnoreCase));
            if (operation is null) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: opName={opName} does not exist, create it");
              }
              var operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO
                .FindById (1);
              operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
              operation.Name = opName;
              ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
              intermediateWorkPiece = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece (operation);
              intermediateWorkPiece.Name = opName;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
              part.AddIntermediateWorkPiece (intermediateWorkPiece);
              ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
            }
          }
        }

        if (operation is null) {
          var opCodeGroup = match.Groups["opCode"];
          if (opCodeGroup.Success) {
            var opCode = opCodeGroup.Value.Trim ();
            if (string.IsNullOrEmpty (opCode)) {
              log.Warn ($"GetOperation: opCode is empty in {m_programComment} for regex {m_regex}");
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetOperation: opCode={opCode} in {m_programComment} for {m_regex}");
              }
              operation = operations
                .FirstOrDefault (x => string.Equals (x.Code, opCode, StringComparison.InvariantCultureIgnoreCase));
              if (operation is null) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetOperation: opCode={opCode} does not exist, create it");
                }
                var operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO
                  .FindById (1);
                operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
                operation.Code = opCode;
                ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
                intermediateWorkPiece = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece (operation);
                intermediateWorkPiece.Code = opCode;
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
                part.AddIntermediateWorkPiece (intermediateWorkPiece);
                ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
              }
            }
          }
        }

        if (null != operation) { // Single operation
          if (intermediateWorkPiece is null) {
            intermediateWorkPiece = operation.IntermediateWorkPieces.UniqueOrDefault ();
            if (intermediateWorkPiece is null) {
              log.Error ($"GetOperation: multiple intermediate work pieces for operation id={operation.Id}");
            }
          }
          if (null != intermediateWorkPiece) {
            var qtyGroup = match.Groups["qty"];
            if (qtyGroup.Success) {
              if (int.TryParse (qtyGroup.Value.Trim (), out var qty)) {
                intermediateWorkPiece.OperationQuantity = qty;
              }
              else {
                log.Error ($"GetOperation: qty={qtyGroup.Value} is not a quantity");
              }
            }
            else {
              var qty1Group = match.Groups["qty1"];
              if (qty1Group.Success) {
                if (!int.TryParse (qty1Group.Value.Trim (), out var qty1)) {
                  log.Error ($"GetOperation: qty1={qty1Group.Value} is not a quantity");
                }
                else {
                  var qty2Group = match.Groups["qty2"];
                  if (qty2Group.Success) {
                    if (int.TryParse (qty2Group.Value.Trim (), out var qty2)) {
                      intermediateWorkPiece.OperationQuantity = qty1 + qty2;
                    }
                    else {
                      log.Error ($"GetOperation: qty2={qty2Group.Value} is not a quantity");
                    }
                  }
                  else {
                    log.Info ($"GetOperation: no qty2 found in {m_programComment} for {m_regex}");
                    intermediateWorkPiece.OperationQuantity = qty1;
                  }
                }
              }
            }
          }
        }

        if (operation is null) {
          log.Error ($"GetOperation: no operation information in {m_programComment}, regex={m_regex}");
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
        var part = GetPart (match);
        if (part is null) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetOperation: part is null in {m_programComment} for regex {m_regex} => return null");
          }
          return null;
        }
        var operation = GetOperation (part, match);
        if (operation is null) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetOperation: operation is null in {m_programComment} for regex {m_regex} => return null");
          }
        }
        return operation;
      }
    }
  }
}
