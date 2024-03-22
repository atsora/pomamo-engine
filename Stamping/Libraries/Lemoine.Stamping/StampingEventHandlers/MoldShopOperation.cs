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
  /// Create the right operation from ProjectName, ComponentName and optionally OperationName and FileName
  /// </summary>
  public class MoldShopOperation
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (MoldShopOperation).FullName);

    readonly StampingData m_stampingData;
    IJob? m_job = null;
    IComponent? m_component = null;
    ISimpleOperation? m_simpleOperation = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public MoldShopOperation (IStamper stamper, StampingData stampingData)
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
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetData (string key, object v)
    {
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="duration"></param>
    public void SetMachiningTime (TimeSpan duration)
    {
      this.Next?.SetMachiningTime (duration);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="toolNumber"></param>
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
            if (m_job is null) {
              if (!m_stampingData.TryGet ("ProjectName", out string? projectName)) {
                log.Error ("StartSequence: ProjectName is unknown");
              }
              else if (projectName is null || string.IsNullOrEmpty (projectName)) {
                log.Error ("StartSequence: projectName is null or empty");
              }
              else {
                var workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindById (1); // Undefined
                if (workOrderStatus is null) {
                  log.Error ($"StartSequence: no work order status with Id=1 (Undefined)");
                  return;
                }
                var project = ModelDAOHelper.DAOFactory.ProjectDAO
                  .FindByName (projectName);
                if (project is null) {
                  m_job = ModelDAOHelper.ModelFactory.CreateJobFromName (workOrderStatus, projectName);
                  ModelDAOHelper.DAOFactory.JobDAO.MakePersistent (m_job);
                  project = m_job.Project;
                }
                else {
                  m_job = project.Job;
                }
              }
            }
            if ((m_job is not null) && (m_component is null)) {
              if (!m_stampingData.TryGet ("ComponentName", out string? componentName)) {
                log.Error ("StartSequence: ComponentName is unknown");
              }
              else if (componentName is null || string.IsNullOrEmpty (componentName)) {
                log.Error ("StartSequence: ComponentName is null or empty");
              }
              else {
                var componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);
                if (componentType is null) {
                  log.Error ("StartSequence: component type is null for Id=1");
                }
                else {
                  m_component = ModelDAOHelper.DAOFactory.ComponentDAO
                    .FindByNameAndProject (componentName, m_job.Project);
                  if (m_component is null) {
                    m_component = ModelDAOHelper.ModelFactory.CreateComponentFromName (m_job.Project, componentName, componentType);
                    ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (m_component);
                  }
                }
              }
            }
            if ((m_component is not null) && (m_simpleOperation is null)) {
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
                m_simpleOperation.Operation.Lock = false;
                ModelDAOHelper.DAOFactory.SimpleOperationDAO.MakePersistent (m_simpleOperation);
                var componentIntermediateWorkPiece = m_component.AddIntermediateWorkPiece (m_simpleOperation.IntermediateWorkPiece);
                ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (componentIntermediateWorkPiece);
                m_stampingData.Operation = m_simpleOperation.Operation;
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
    /// <param name="toolNumber"></param>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber);
    }

  }
}
