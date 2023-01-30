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
  /// Persistent class of table DayTemplateSlot
  /// </summary>
  [Serializable]
  public class DayTemplateSlot: GenericRangeSlot, ISlot, IDayTemplateSlot
  {
    #region Members
    IDayTemplate m_dayTemplate;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateSlot).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected DayTemplateSlot ()
      : base (false)
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayTemplate">not null</param>
    /// <param name="range"></param>
    public DayTemplateSlot  (IDayTemplate dayTemplate,
                             UtcDateTimeRange range)
      : base (false, range)
    {
      Debug.Assert (null != dayTemplate);
      m_dayTemplate = dayTemplate;
    }
    #endregion // Constructors and factory methods
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the DayTemplate
    /// </summary>
    public virtual IDayTemplate DayTemplate {
      get { return m_dayTemplate; }
      set { m_dayTemplate = value; }
    }
    #endregion // Getters / Setters
    
    #region Slot implementation
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is IDayTemplateSlot) {
        var other = (IDayTemplateSlot) obj;
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }
      
      GetLogger ().ErrorFormat ("CompareTo: " +
                                "object {0} of invalid type",
                                obj);
      throw new ArgumentException ("object is not a DayTemplateSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IDayTemplateSlot other)
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
      var other = obj as IDayTemplateSlot;
      if (other == null) {
        return false;
      }

      return NHibernateHelper.EqualsNullable (this.DayTemplate, other.DayTemplate, (a, b) => a.Id == b.Id); // Not to initialize the proxy if not required
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      // No gap is wished
      // Return false even when the DayTemplate is null
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
        return $"[DayTemplateSlot {this.Id} Range={this.DateTimeRange} DayTemplate={m_dayTemplate?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[DayTemplateSlot {this.Id}]";
      }
    }
  }
}
