// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// IsoFile slot
  /// </summary>
  public class IsoFileSlot : GenericMachineModuleSlot, IIsoFileSlot, IComparable<IIsoFileSlot>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IsoFileSlot).FullName);

    #region Members
    IIsoFile m_isoFile;
    #endregion // Members

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected IsoFileSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    public IsoFileSlot (IMachineModule machineModule,
                        UtcDateTimeRange range)
      : base (machineModule, range)
    {
    }
    #endregion // Constructors and factory methods

    #region getters/setters
    /// <summary>
    /// ID of the iso file
    /// </summary>
    public virtual IIsoFile IsoFile
    {
      get { return m_isoFile; }
      set {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("IsoFile.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_isoFile = value;
      }
    }
    #endregion // getters/setters

    #region Slot implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is IIsoFileSlot) {
        IIsoFileSlot other = (IIsoFileSlot)obj;
        if (other.MachineModule.Equals (this.MachineModule)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare isofile slots " +
                           "for different machine modules {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of IsoFileSlot from different machine modules");
        }
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not an IsoFileSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IIsoFileSlot other)
    {
      if (other.MachineModule.Equals (this.MachineModule)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare isofile slots " +
                       "for different machine modules {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of IsoFileSlot from different machine modules");
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[IsoFileSlot {this.Id} {this.MachineModule?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[IsoFileSlot {this.Id}]";
      }
    }

    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      IIsoFileSlot other = obj as IIsoFileSlot;
      if (other == null) {
        return false;
      }

      Debug.Assert (null != this.MachineModule);
      Debug.Assert (null != other.MachineModule);
      return NHibernateHelper.EqualsNullable (this.IsoFile, other.IsoFile, (a, b) => a.Id == b.Id)
        && (this.MachineModule.Id == other.MachineModule.Id);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      if (null == this.IsoFile) {
        return true;
      }

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
