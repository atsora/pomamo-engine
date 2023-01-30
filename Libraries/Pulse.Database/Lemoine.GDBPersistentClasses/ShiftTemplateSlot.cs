// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ShiftTemplateSlot
  /// </summary>
  [Serializable]
  public class ShiftTemplateSlot: GenericRangeSlot, ISlot, IShiftTemplateSlot
  {
    #region Members
    IShiftTemplate m_shiftTemplate;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateSlot).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected ShiftTemplateSlot ()
      : base (false)
    {
    }
    
    /// <summary>
    /// Constructor for GenericRangeSlot.Create
    /// </summary>
    /// <param name="range"></param>
    internal ShiftTemplateSlot (UtcDateTimeRange range)
      : base (false, range)
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="range"></param>
    public ShiftTemplateSlot  (IShiftTemplate shiftTemplate,
                               UtcDateTimeRange range)
      : base (false, range)
    {
      Debug.Assert (null != shiftTemplate);
      m_shiftTemplate = shiftTemplate;
    }
    #endregion // Constructors and factory methods
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the ShiftTemplate
    /// </summary>
    public virtual IShiftTemplate ShiftTemplate {
      get { return m_shiftTemplate; }
      set { m_shiftTemplate = value; }
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
      if (obj is IShiftTemplateSlot) {
        var other = (IShiftTemplateSlot) obj;
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not a ShiftTemplateSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IShiftTemplateSlot other)
    {
      return this.BeginDateTime.CompareTo (other.BeginDateTime);
    }
    
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      var other = obj as IShiftTemplateSlot;
      return other != null
        && NHibernateHelper.EqualsNullable(this.ShiftTemplate, other.ShiftTemplate, (a,b) => a.Id == b.Id);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      return null == this.ShiftTemplate;
    }
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }
    #endregion // Slot implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ShiftTemplateSlot {this.Id} Range={this.DateTimeRange} {this.ShiftTemplate?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[ShiftTemplateSlot {this.Id}]";
      }
    }
  }
}
