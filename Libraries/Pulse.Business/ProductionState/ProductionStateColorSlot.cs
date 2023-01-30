// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.ProductionState
{
  /// <summary>
  /// Reason color slot
  /// </summary>
  [Serializable]
  public class ProductionStateColorSlot
    : IProductionStateColorSlot
    , IWithRange
    , IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger (typeof (ProductionStateColorSlot).FullName);

    #region Members
    IMachine m_machine;
    string m_color;
    double? m_productionRate;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ProductionStateColorSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="color"></param>
    /// <param name="productionRate"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal ProductionStateColorSlot (IMachine machine,
                                        string color,
                                        double? productionRate,
                                        UtcDateTimeRange range,
                                        DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (machine is null) {
        log.Fatal ("ProductionStateColorSlot: machine is null");
        throw new ArgumentNullException ("machine");
      }
      m_machine = machine;
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");
      m_color = color;
      m_productionRate = productionRate;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="color"></param>
    /// <param name="productionRate"></param>
    /// <param name="range"></param>
    protected internal ProductionStateColorSlot (IMachine machine,
                                                 string color,
                                                 double? productionRate,
                                                 UtcDateTimeRange range)
      : this (machine, color, productionRate, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="reasonSlot"></param>
    protected internal ProductionStateColorSlot (IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.ProductionState?.Color ?? "",
          reasonSlot.ProductionRate,
          reasonSlot.DateTimeRange, reasonSlot.DayRange)
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Reference to the reason color
    /// </summary>
    public virtual string Color
    {
      get { return m_color; }
    }

    /// <summary>
    /// Production rate
    /// </summary>
    public virtual double? ProductionRate => m_productionRate;

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      protected set {
        m_dateTimeRange = value;
        m_dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (m_dateTimeRange));
      }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange
    {
      get { return m_dayRange; }
    }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get { return m_dateTimeRange.Duration; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IProductionStateColorSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && string.Equals (this.Color, other.Color, StringComparison.InvariantCultureIgnoreCase)
        && object.Equals (this.ProductionRate, other.ProductionRate);
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is ProductionStateColorSlot) {
        IProductionStateColorSlot other = (IProductionStateColorSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.Error ($"CompareTo: trying to compare slots for different machines {this} {other}");
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }

      log.Error ($"CompareTo: object {obj} of invalid type");
      throw new ArgumentException ("object is not the right slot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IProductionStateColorSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.Error ($"CompareTo: trying to compare ProductionStateColorSlots for different machines {this} {other}");
      throw new ArgumentException ("Comparison of ProductionStateColorSlots from different machines");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false;
    }
    #endregion // IWithRange implementation

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IProductionStateColorSlot other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ProductionStateColorSlot other = obj as ProductionStateColorSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode ();
        hashCode += 1000000009 * DateTimeRange.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return $"[ProductionStateColorSlot Machine={this.Machine} Range={this.DateTimeRange}]";
    }

    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IProductionStateColorSlot Clone (UtcDateTimeRange range)
    {
      IProductionStateColorSlot clone = new ProductionStateColorSlot (this.Machine,
        this.Color, this.ProductionRate, range);
      return clone;
    }
  }
}
