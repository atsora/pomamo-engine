// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table SequenceSlot
  /// </summary>
  [Serializable]
  public class SequenceSlot: GenericMachineModuleSlot, ISequenceSlot, IVersionable
  {
    #region Members
    ISequence m_sequence;
    DateTime? m_nextBegin = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SequenceSlot).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected SequenceSlot ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="sequence"></param>
    /// <param name="range"></param>
    public SequenceSlot (IMachineModule machineModule,
                         ISequence sequence,
                         UtcDateTimeRange range)
      : base (machineModule, range)
    {
      m_sequence = sequence;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    public virtual string Display
    {
      get;
      set;
    }

    /// <summary>
    /// Reference to the sequence
    /// </summary>
    public virtual ISequence Sequence {
      get { return m_sequence; }
    }
    
    /// <summary>
    /// Optionally begin date/time of the next slot
    /// </summary>
    public virtual DateTime? NextBegin {
      get { return m_nextBegin; }
      set { m_nextBegin = value; }
    }
    #endregion // Getters / Setters
    
    #region Slot implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is ISequenceSlot) {
        ISequenceSlot other = (ISequenceSlot) obj;
        if (other.MachineModule.Equals (this.MachineModule)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare sequence slots " +
                           "for different machine modules {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of sequence slots from different machine modules");
        }
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not an SequenceSlot");
    }
    
    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(ISequenceSlot other)
    {
      if (other.MachineModule.Equals (this.MachineModule)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare sequence slots " +
                       "for different machine modules {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of SequenceSlot from different machine modules");
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[SequenceSlot {this.Id} {this.MachineModule?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[SequenceSlot {this.Id}]";
      }
    }
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      ISequenceSlot other = obj as ISequenceSlot;
      if (other == null) {
        return false;
      }

      Debug.Assert (null != this.MachineModule);
      Debug.Assert (null != other.MachineModule);
      return NHibernateHelper.EqualsNullable (this.Sequence, other.Sequence, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id)
        && (this.MachineModule.Id == other.MachineModule.Id);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      return false;
    }
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// Handle here the specific tasks for a removed slot
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// Handle here the specific tasks for a modified slot
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }
    #endregion // Slot implementation
  }
}
