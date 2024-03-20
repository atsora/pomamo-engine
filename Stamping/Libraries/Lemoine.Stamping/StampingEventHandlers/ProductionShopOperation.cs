// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Create the operation from PartCode and OperationName
  /// </summary>
  public class ProductionShopOperation
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionShopOperation).FullName);

    readonly StampingData m_stampingData;
    IPart? m_part = null;
    ISimpleOperation? m_simpleOperation = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionShopOperation (IStamper stamper, StampingData stampingData)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStampingEventHandler? Next { get; set; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStamper Stamper { get; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void NotifyNewBlock (bool edit, int level)
    {
      this.Next?.NotifyNewBlock (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetComment (string message)
    {
      this.Next?.SetComment (message);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetMachiningTime (TimeSpan duration)
    {
      this.Next?.SetMachiningTime (duration);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetNextToolNumber (string toolNumber)
    {
      this.Next?.SetNextToolNumber (toolNumber);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      if (m_simpleOperation is null) {
        try {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            if (!m_stampingData.TryGet ("PartCode", out string? partCode)) {
              log.Warn ("StartSequence: PartCode is unknown");
            }
            else if (partCode is null || string.IsNullOrEmpty (partCode)) {
              log.Error ("StartSequence: PartCode is null or empty");
              partCode = null;
            }
            if (!m_stampingData.TryGet ("PartName", out string? partName)) {
              log.Debug ($"StartSequence: PartName is unknown");
            }
            else if (partName is null || string.IsNullOrEmpty (partName)) {
              log.Error ($"StartSequence: PartName is null or empty");
              partName = null;
            }
            if (!string.IsNullOrEmpty (partCode) || !string.IsNullOrEmpty (partName)) {
              var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);
              if (componentType is null) {
                log.Error ("StartSequence: component type is null for Id=1");
              }
              else {
                if (m_part is null) {
                  if (partCode is not null) {
                    m_part = ModelDAOHelper.DAOFactory.PartDAO
                      .FindByCode (partCode);
                    if (partName is not null && (!string.Equals (m_part.Name, partName))) {
                      if (m_part.Name is null) {
                        m_part.Name = partName;
                      }
                      else if (log.IsErrorEnabled) {
                        log.Error ($"StartSequence: part name stored={m_part.Name} VS {partName} for code={m_part.Code}");
                      }
                    }
                  }
                  else if (partName is not null) {
                    m_part = ModelDAOHelper.DAOFactory.PartDAO
                      .FindByName (partName);
                  }
                }
                if (m_part is null) {
                  m_part = ModelDAOHelper.ModelFactory.CreatePart (componentType);
                  m_part.Code = partCode;
                  m_part.Name = partName;
                  ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (m_part);
                }
                var operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
                if (operationType is null) {
                  log.Error ("StartSequence: operation type is null for Id=1");
                }
                else {
                  m_simpleOperation = ModelDAOHelper.ModelFactory.CreateSimpleOperation (operationType);
                  if (m_stampingData.TryGet ("OperationName", out string? operationName)) {
                    m_simpleOperation.Name = operationName;
                  }
                  else if (m_stampingData.TryGet ("FileName", out string? fileName)) {
                    m_simpleOperation.Name = fileName;
                  }
                  ModelDAOHelper.DAOFactory.SimpleOperationDAO.MakePersistent (m_simpleOperation);
                  var componentIntermediateWorkPiece = m_part.AddIntermediateWorkPiece (m_simpleOperation.IntermediateWorkPiece);
                  ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (componentIntermediateWorkPiece);
                  m_stampingData.Operation = m_simpleOperation.Operation;
                }
              }
            }
            this.Next?.StartSequence (sequenceKind);
          } // session
        }
        catch (Exception ex) {
          log.Error ($"StartSequence: exception", ex);
          throw;
        }
      }
      else {
        this.Next?.StartSequence (sequenceKind);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      this.Next?.StartProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      this.Next?.EndProgram (edit, level, endOfFile);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
      this.Next?.ResumeProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SuspendProgram (bool optional = false, string details = "")
    {
      this.Next?.SuspendProgram (optional, details);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerMachining ()
    {
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber: toolNumber);
    }
  }
}
