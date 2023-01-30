// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the line association modification tables
  /// </summary>
  [Serializable,
   XmlInclude(typeof(WorkOrderLineAssociation))]
  public abstract class LineAssociation : PeriodAssociation
  {
    #region Members
    ILine m_line;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the Line persistent class
    /// It can't be null
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line
    {
      get { return m_line; }
      set
      {
        if (null == value) {
          log.Fatal ("LineAssociation: Line can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ();
        }
        else {
          m_line = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                    this.GetType().FullName,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Reference to the Line persistent class
    /// for Xml Serialization
    /// 
    /// It can't be null
    /// </summary>
    [XmlElement("Line")]
    public virtual Line XmlSerializationLine {
      get { return m_line as Line; }
      set { m_line = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected LineAssociation()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="line"></param>
    /// <param name="begin"></param>
    protected LineAssociation (ILine line, DateTime begin)
      : base (begin)
    {
      this.Line = line;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="line"></param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    internal protected LineAssociation (ILine line, DateTime begin, IModification mainModification)
      : base (begin, mainModification)
    {
      this.Line = line;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slot
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    public override IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                   bool pastOnly)
    {
      bool leftMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoLeftMerge);
      bool rightMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoRightMerge);
      
      IList<I> impactedSlots = slotDAO
        .GetImpactedLineSlotsForAnalysis (this.Line,
                                          this.Range,
                                          this.DateTime,
                                          pastOnly,
                                          leftMerge,
                                          rightMerge);
      return impactedSlots;
    }
    
    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      IList<IMachine> list = new List<IMachine> ();
      // Session...
      IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAllByLine(Line);
      
      foreach (ILineMachine lineMachine in lineMachines) {
        if (lineMachine.LineMachineStatus == LineMachineStatus.Dedicated) {
          list.Add(lineMachine.Machine);
        }
      }
      return list;
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<ILine> (ref m_line);
    }
    #endregion // Methods
  }
}
