// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;

namespace Lemoine.Analysis.Detection
{
  public class SequenceMilestoneDetection
    : IChecked, ISequenceMilestoneDetection
  {
    ILog log;

    readonly IMachineModule m_machineModule;
    readonly Lemoine.Threading.IChecked m_caller;

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel { get; set; }

    /// <summary>
    /// Constructor to set the machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="caller"></param>
    public SequenceMilestoneDetection (IMachineModule machineModule, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      m_caller = caller;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");
    }

    #region IChecked implementation
    /// <summary>
    /// Lemoine.Threading.IChecked implementation
    /// </summary>
    public void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // IChecked implementation

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence"></param>
    public void StartSequence (DateTime dateTime, ISequence sequence = null)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.SetSequenceMilestone", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        if (sequenceMilestone is null) {
          sequenceMilestone = ModelDAOHelper.ModelFactory
            .CreateSequenceMilestone (m_machineModule);
        }
        else { // Existing milestone
          if ((null != sequenceMilestone.Sequence) && (null != sequence) && (sequenceMilestone.Sequence.Id == sequence.Id)) {
            // Same sequence => no change => commit
            transaction.Commit ();
            return;
          }
        }
        sequenceMilestone.DateTime = dateTime;
        sequenceMilestone.Sequence = sequence;
        sequenceMilestone.Completed = false;
        ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .MakePersistent (sequenceMilestone);

        transaction.Commit ();
      }
    }

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="milestone"></param>
    /// <param name="sequence"></param>
    public void SetSequenceMilestone (DateTime dateTime, TimeSpan milestone, ISequence sequence = null)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.SetSequenceMilestone", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        if (sequenceMilestone is null) {
          sequenceMilestone = ModelDAOHelper.ModelFactory
            .CreateSequenceMilestone (m_machineModule);
        }
        sequenceMilestone.DateTime = dateTime;
        sequenceMilestone.Milestone = milestone;
        if (null != sequence) {
          sequenceMilestone.Sequence = sequence;
        }
        sequenceMilestone.Completed = false;
        ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .MakePersistent (sequenceMilestone);

        transaction.Commit ();
      }
    }

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    public void RemoveSequenceMilestone ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.RemoveSequenceMilestone", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        ModelDAOHelper.DAOFactory.SequenceMilestoneDAO.MakeTransient (sequenceMilestone);

        transaction.Commit ();
      }
    }

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence"></param>
    public void TagSequenceCompleted (DateTime dateTime, ISequence sequence = null)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.TagSequenceCompleted", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        if (sequenceMilestone is null) {
          sequenceMilestone = ModelDAOHelper.ModelFactory
            .CreateSequenceMilestone (m_machineModule);
        }
        sequenceMilestone.DateTime = dateTime;
        if (null != sequence) {
          sequenceMilestone.Sequence = sequence;
        }
        sequenceMilestone.Completed = true;
        ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .MakePersistent (sequenceMilestone);

        transaction.Commit ();
      }
    }

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence"></param>
    public void CheckSequence (DateTime dateTime, ISequence sequence)
    {
      Debug.Assert (null != sequence);

      if (sequence is null) {
        log.Error ("CheckSequence: sequence is null => return at once");
        return;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.CheckSequence", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        if (null != sequenceMilestone?.Sequence) {
          if (sequence.Id != sequenceMilestone.Sequence.Id) {
            log.Info ($"CheckSequence: sequence change, {sequence.Id} VS {sequenceMilestone.Sequence.Id} => delete sequencemilestone");
            ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
              .MakeTransient (sequenceMilestone);
            transaction.Commit ();
            return;
          }
        }

        transaction.Commit ();
      }
    }

    /// <summary>
    /// <see cref="ISequenceMilestoneDetection"/>
    /// </summary>
    /// <param name="dateTime"></param>
    public void CancelSequenceMilestone (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.CancelSequenceMilestone", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
          .FindById (m_machineModule.Id);
        if (!(sequenceMilestone is null)) {
          ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
            .MakeTransient (sequenceMilestone);
          transaction.Commit ();
          return;
        }

        transaction.Commit ();
      }
    }
  }
}
