// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification of an operation's properties (such as estimated machining hours)
  /// </summary>
  [Serializable]
  public class OperationInformation: GlobalModification, IOperationInformation
  {
    #region Members
    IOperation m_operation;
    TimeSpan? m_oldMachiningDuration;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationInformation).FullName);

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "OperationInformation"; }
    }

    /// <summary>
    /// Operation
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation {
      get { return m_operation; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("Operation.set: " +
                           "null value");
          throw new ArgumentNullException ("OperationInformation.Operation");
        }
        m_operation = value;
      }
    }
    
    /// <summary>
    /// Reference to the Operation for Xml Serialization
    /// </summary>
    [XmlElement("Operation")]
    public virtual Operation XmlSerializationOperation {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }
    
    /// <summary>
    /// Old estimated machining duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? OldMachiningDuration {
      get { return m_oldMachiningDuration; }
      set { m_oldMachiningDuration = value; }
    }
    
    /// <summary>
    /// Old estimated machining duration as string
    /// </summary>
    [XmlAttribute("OldMachiningDuration")]
    public virtual string OldEstimatedMachiningDurationAsString {
      get
      {
        return (this.OldMachiningDuration.HasValue)
          ? this.OldMachiningDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.OldMachiningDuration =
          string.IsNullOrEmpty (value)
          ? default (TimeSpan?)
          : TimeSpan.Parse (value);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected OperationInformation ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected OperationInformation (IOperation operation, DateTime dateTime)
      : base ()
    {
      this.DateTime = dateTime;
      this.Operation = operation;
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      // TODO: recompute the cycle duration summary, if the plugin is activated
      /*
      if (AnalysisConfigHelper.TrackNonStandardCycles) {
        // UNDONE: Note: this should be reworked, this blocks the whole service when there are too many cycles
        // - This should be split by machine first (so the activity analysis is stopped as well)
        // - And for each machine, it should be made by step
        
        IList<IOperationCycle> allOperationCycles =
          ModelDAO.ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperation(this.Operation);
        
        // takes a long time (around 45 sec for 100k cycles, 67 sec for 150k)
        // so beware of modification analysis timeout
        // to be improved
        foreach (IOperationCycle operationCycle in allOperationCycles) {
          ((OperationCycle) operationCycle).RecomputeOffsetDuration ();
          ((OperationCycle) operationCycle).RecomputeBetweenCyclesOffsetDuration ();
        }
        
        // using a bulk update is not such a great idea,
        // since testing shows it has a big variance
        // (between 20 to 110 seconds for 150k cycles,
        // instead of 67 seconds more or less 5 seconds).
        // Also has issues w.r.t. command timeouts (around 20 seconds),
        // and more difficult to maintain
        //
      }
    */

      // Analysis is done
      MarkAsCompleted ("");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient OperationInformation to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }
    #endregion // Methods
  }
}
