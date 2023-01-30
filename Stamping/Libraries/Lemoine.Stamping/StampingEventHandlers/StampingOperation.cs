// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Stamping.SequenceNamers;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// StampingData
  /// </summary>
  public class StampingOperation
    : IStampingEventHandler
    , IAsyncDisposable, IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingOperation).FullName);

    bool m_disposed = false;
    readonly StampingData m_stampingData;
    readonly ISequenceNamer m_sequenceNamer;
    readonly IDAOSession m_session = ModelDAOHelper.DAOFactory.OpenSession ();
    IOperation? m_operation;
    IPath? m_path;
    IIsoFile? m_isoFile;
    ISequence? m_currentSequence;
    int m_sequenceOrder = 1;

    #region Getters / Setters
    /// <summary>
    /// Operation (lazy)
    /// </summary>
    IOperation Operation => GetOperation ().Item1;

    /// <summary>
    /// Iso file (lazy)
    /// </summary>
    IIsoFile IsoFile => GetIsoFile ();
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public StampingOperation (IStamper stamper, StampingData stampingData, ISequenceNamer sequenceNamer)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
      m_sequenceNamer = sequenceNamer;
      m_operation = m_stampingData.Operation; // nullable
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingOperation (IStamper stamper, StampingData stampingData)
      : this (stamper, stampingData, new OrderIsSequenceName ())
    { }
    #endregion // Constructors

    (IOperation, IPath) GetOperation ()
    {
      if (m_operation is null) {
        var operation = m_stampingData.Operation;
        if (operation is null) {
          log.Info ($"GetOperation: no operation was set");
        }
        else {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
          m_operation = operation;
        }
      }
      if (m_path is null) {
        var path = m_stampingData.OperationPath;
        if (path is null) {
          log.Info ($"GetOperation: no path was set");
        }
        else {
          ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
          m_path = path;
        }
      }

      if (m_operation is null) {
        (m_operation, m_path) = CreateOperation ();
      }
      else if (m_path is null) { // m_operation is not null
        m_path = CreatePath (m_operation);
      }
      return (m_operation, m_path);
    }

    IIsoFile GetIsoFile ()
    {
      if (m_isoFile is null) {
        m_isoFile = CreateIsoFile ();
      }
      return m_isoFile;
    }

    /// <summary>
    /// Create an operation/path
    /// </summary>
    /// <returns></returns>
    protected virtual (IOperation, IPath) CreateOperation ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (m_operation is null) {
          var operationType = ModelDAO.ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
          var operation = ModelDAO.ModelDAOHelper.ModelFactory.CreateOperation (operationType);
          ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
          m_operation = operation;
        }
        var path = ModelDAO.ModelDAOHelper.ModelFactory.CreatePath (m_operation);
        ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);
        return (m_operation, path);
      }
    }

    /// <summary>
    /// Create an operation/path
    /// </summary>
    /// <returns></returns>
    protected virtual IPath CreatePath (IOperation operation)
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var path = ModelDAO.ModelDAOHelper.ModelFactory.CreatePath (operation);
        ModelDAOHelper.DAOFactory.PathDAO.MakePersistent (path);
        return path;
      }
    }

    /// <summary>
    /// Create an iso file
    /// </summary>
    /// <returns></returns>
    protected virtual IIsoFile CreateIsoFile ()
    {
      string fileName = string.IsNullOrEmpty (m_stampingData.Source)
        ? ""
        : System.IO.Path.GetFileName (m_stampingData.Source);
      string? directoryName = string.IsNullOrEmpty (m_stampingData.Source)
        ? null
        : System.IO.Path.GetDirectoryName (m_stampingData.Source);

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var isoFile = ModelDAO.ModelDAOHelper.ModelFactory.CreateIsoFile (fileName);
        isoFile.SourceDirectory = directoryName ?? ""; // SourceDirectory is mandatory
        if (m_stampingData.Destination is null) {
          log.Warn ($"CreateIsoFile: stamping directory is not set");
          isoFile.StampingDirectory = "";
        }
        else { // m_stampingData.Destination is not null
          var destinationDirectoryName = System.IO.Path.GetDirectoryName (m_stampingData.Destination);
          isoFile.StampingDirectory = destinationDirectoryName;
        }
        isoFile.StampingDateTime = DateTime.UtcNow;
        var computer = ModelDAO.ModelDAOHelper.DAOFactory.ComputerDAO.GetOrCreateLocal ();
        if (computer is not null) {
          isoFile.Computer = computer;
          ModelDAO.ModelDAOHelper.DAOFactory.ComputerDAO.MakePersistent (computer);
        }
        ModelDAO.ModelDAOHelper.DAOFactory.IsoFileDAO.MakePersistent (isoFile);
        return isoFile;
      }
    }

    /// <summary>
    /// Create a sequence asynchronously
    /// </summary>
    /// <returns></returns>
    protected virtual ISequence CreateSequence (SequenceKind sequenceKind = SequenceKind.Machining)
    {
      var (operation, path) = GetOperation ();

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var sequenceName = m_sequenceNamer.GetSequenceName (m_sequenceOrder);
        var sequence = ModelDAO.ModelDAOHelper.ModelFactory.CreateSequence (sequenceName, operation, path);
        sequence.Kind = sequenceKind;
        sequence.Order = m_sequenceOrder++;
        // TODO: Cad name: probably not necessary
        var sequenceDuration = m_stampingData.SequenceDuration;
        if (sequenceDuration is not null) {
          sequence.EstimatedTime = sequenceDuration;
        }
        var toolNumber = m_stampingData.ToolNumber;
        if (toolNumber is not null) {
          sequence.ToolNumber = toolNumber;
        }
        ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);
        m_stampingData.Sequence = sequence;
        return sequence;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>stamp id</returns>
    public int CreateStartCycleStamp ()
    {
      var (operation, path) = GetOperation ();

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var stamp = ModelDAO.ModelDAOHelper.ModelFactory.CreateStamp ();
        stamp.IsoFile = GetIsoFile ();
        stamp.Position = m_stampingData.ReadPosition;
        stamp.Operation = operation;
        stamp.OperationCycleBegin = true;
        ModelDAOHelper.DAOFactory.StampDAO.MakePersistent (stamp);
        m_stampingData.Add ("StartCycleStamp", stamp);
        return stamp.Id;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>stamp id</returns>
    public int CreateStopCycleStamp ()
    {
      var (operation, path) = GetOperation ();

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var stamp = ModelDAO.ModelDAOHelper.ModelFactory.CreateStamp ();
        stamp.IsoFile = GetIsoFile ();
        stamp.Position = m_stampingData.ReadPosition;
        stamp.Operation = operation;
        stamp.OperationCycleEnd = true;
        ModelDAO.ModelDAOHelper.DAOFactory.StampDAO.MakePersistent (stamp);
        m_stampingData.Add ("StopCycleStamp", stamp);
        return stamp.Id;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>stamp id</returns>
    public int CreateSequenceStamp (SequenceKind sequenceKind = SequenceKind.Machining)
    {
      if (m_currentSequence is not null) {
        CompleteSequencePropertiesAtEnd ();
      }

      m_currentSequence = CreateSequence (sequenceKind);

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var stamp = ModelDAO.ModelDAOHelper.ModelFactory.CreateStamp ();
        stamp.IsoFile = GetIsoFile ();
        stamp.Position = m_stampingData.ReadPosition;
        stamp.Sequence = m_currentSequence;
        ModelDAOHelper.DAOFactory.StampDAO.MakePersistent (stamp);
        m_stampingData.Add ("SequenceStamp", stamp);
        return stamp.Id;
      }
    }

    /// <summary>
    /// Complete the sequence properties before the machining starts
    /// </summary>
    /// <returns></returns>
    protected virtual Task CompleteSequencePropertiesBeforeMachining ()
    {
      if (m_currentSequence is not null) {
        // TODO: cad model: probably not necessary
        var toolNumber = m_stampingData.ToolNumber;
        if (toolNumber is not null) {
          m_currentSequence.ToolNumber = toolNumber;
        }
      }
      return Task.CompletedTask;
    }

    /// <summary>
    /// Complete the sequence properties at the end of the sequence
    /// </summary>
    /// <returns></returns>
    protected virtual Task CompleteSequencePropertiesAtEnd ()
    {
      if (m_currentSequence is not null) {
        // CAD Model...
      }
      return Task.CompletedTask;
    }

    /// <summary>
    /// Trigger the sequence machining was started
    /// </summary>
    public async Task TriggerSequenceStarted ()
    {
      await CompleteSequencePropertiesBeforeMachining ();
    }

    /// <summary>
    /// Trigger the end of file actions
    /// </summary>
    /// <returns></returns>
    public async Task TriggerEndOfFileActions ()
    {
      if (m_currentSequence is not null) {
        await CompleteSequencePropertiesAtEnd ();
      }
      // TODO: place where to update job/component from operation

    }

    #region IStampingEventHandler
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
    public void StartCycle ()
    {
      var stampId = CreateStartCycleStamp ();
      m_stampingData.Add ("StartCycleStampId", stampId);
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      var stampId = CreateStopCycleStamp ();
      m_stampingData.Add ("StopCycleStampId", stampId);
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      var stampId = CreateSequenceStamp (sequenceKind);
      m_stampingData.Add ("SequenceStampId", stampId);
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="comment"></param>
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
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
    /// <param name="duration"></param>
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
    public void TriggerMachining ()
    {
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber);
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
    #endregion // IStampingEventHandler

    #region IDisposable
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Dispose (disposing: true);
      GC.SuppressFinalize (this);
    }


    /// <summary>
    /// <see cref="IAsyncDisposable"/>
    /// </summary>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
    public async ValueTask DisposeAsync ()
    {
      await DisposeAsyncCore ();

      Dispose (disposing: false);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        m_session.Dispose ();
      }

      m_disposed = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore ()
    {
      if (m_disposed) {
        return;
      }

      if (!m_disposed) {
        await Task.Run (() => m_session.Dispose ());
      }

      m_disposed = true;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void NotifyNewBlock (bool edit, int level)
    {
      this.Next?.NotifyNewBlock (edit, level);
    }
    #endregion // IDisposable

  }
}
