// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data.Common;
using System.Diagnostics;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.UserTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate composite type to get a DateTime object
  /// from two columns, one for UTC date and one for UTC time
  /// </summary>
  [Serializable]
  public class CompositeUTCDateTimeType: ICompositeUserType
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CompositeUTCDateTimeType).FullName);

    /// <summary>
    /// <see cref="ICompositeUserType.PropertyNames" />
    /// </summary>
    public string[] PropertyNames {
      get {
        return new string[2] {"UTCDate", "UTCTime"};
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.PropertyTypes" />
    /// </summary>
    public NHibernate.Type.IType[] PropertyTypes {
      get {
        return new NHibernate.Type.IType[2] {NHibernateUtil.Date, NHibernateUtil.Time};
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.ReturnedClass" />
    /// </summary>
    public Type ReturnedClass {
      get {
        return typeof (DateTime);
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.IsMutable" />
    /// </summary>
    public bool IsMutable {
      get {
        return false;
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.GetPropertyValue" />
    /// </summary>
    /// <param name="component"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public object GetPropertyValue(object component, int property)
    {
      DateTime dateTime = ((DateTime) component).ToUniversalTime ();
      
      switch (property) {
        case 0: // Return UTC Date from dateTime
          return dateTime.Date;
        case 1: // Return UTC Time from dateTime
          log.DebugFormat ("TimeOfDay is {0} " +
                           "and associated date is {1}",
                           dateTime.TimeOfDay,
                           new DateTime (dateTime.TimeOfDay.Ticks,
                                         DateTimeKind.Utc));
          return new DateTime (dateTime.TimeOfDay.Ticks,
                               DateTimeKind.Utc);
        default:
          log.Error ("GetPropertyValue: invalid property index " + property);
          throw new ArgumentOutOfRangeException ("property",
                                                 property,
                                                 "property must be 0 or 1");
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.SetPropertyValue" />
    /// </summary>
    /// <param name="component"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public void SetPropertyValue(object component, int property, object value)
    {
      if (component == null) {
        log.Error ("SetPropertyValue: null component");
        throw new ArgumentNullException ("component");
      }

      DateTime dateTime = ((DateTime) component).ToUniversalTime ();
      
      DateTime dbDateTime = (DateTime) value;
      if (dbDateTime.Kind != DateTimeKind.Utc) {
        log.ErrorFormat ("SetPropertyValue: dbDateTime {0} is of kind {1}, " +
                         "not UTC",
                         dbDateTime,
                         dbDateTime.Kind);
        throw new ArgumentException ("value is not UTC",
                                     "value");
      }
      
      log.DebugFormat ("SetPropertyValue: process with property={0} " +
                       "dateTime={1} dbDateTime={2}",
                       property,
                       dateTime, dbDateTime);
      switch (property) {
        case 0: // Update the date value
          dateTime = dbDateTime.Date.Add (dateTime.TimeOfDay);
          break;
        case 1: // Update the time value
          dateTime = dateTime.Date.Add (dbDateTime.TimeOfDay);
          break;
        default:
          log.Error ("SetPropertyValue: invalid property index " + property);
          throw new ArgumentOutOfRangeException ("property",
                                                 property,
                                                 "property must be 0 or 1");
      }
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.Equals" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public new bool Equals(object x, object y)
    {
      if (ReferenceEquals(x, y)) {
        return true;
      }

      if (x == null || y == null) {
        return false;
      }

      return x.Equals(y);
    }

    /// <summary>
    /// <see cref="ICompositeUserType.GetHashCode" />
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public int GetHashCode(object x)
    {
      return (x == null) ? typeof (DateTime).GetHashCode () + 321
        : x.GetHashCode ();
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.NullSafeGet" />
    /// </summary>
    /// <param name="dr"></param>
    /// <param name="names"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object NullSafeGet(DbDataReader dr, string[] names, NHibernate.Engine.ISessionImplementor session, object owner)
    {
      if (dr == null) {
        return null;
      }
      
      DateTime utcDate = DateTime.SpecifyKind (
        (DateTime) NHibernateUtil.Date.NullSafeGet (dr,
                                                    names [0],
                                                    session,
                                                    owner),
        DateTimeKind.Utc);
      TimeSpan timeSpan =
        (TimeSpan) new TimeWithoutTZType ().NullSafeGet (dr,
                                                         names [1],
                                                         session,
                                                         owner);
      log.DebugFormat ("NullSafeGet: utcDate={0} timeSpan={1}",
                       utcDate, timeSpan);
      return utcDate.Add (timeSpan);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.NullSafeSet" />
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="session"></param>
    public void NullSafeSet(DbCommand cmd, object value, int index, NHibernate.Engine.ISessionImplementor session)
    {
      bool[] settable = new bool[2] { true, true };
      NullSafeSet (cmd, value, index, settable, session);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.NullSafeSet" />
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="settable"></param>
    /// <param name="session"></param>
    public void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, NHibernate.Engine.ISessionImplementor session)
    {
      if (value == null) {
        return;
      }
      
      DateTime dateTime = ((DateTime) value).ToUniversalTime ();
      NHibernateUtil.Date.NullSafeSet (cmd, dateTime.Date, index, session);
      new TimeWithoutTZType ().NullSafeSet (cmd, dateTime.TimeOfDay, index+1, session);
      log.DebugFormat ("NullSafeSet: dateTime={0}",
                       dateTime);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.DeepCopy" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object DeepCopy(object value)
    {
      DateTime dateTime = (DateTime) value;
      Debug.Assert (dateTime.Kind == DateTimeKind.Utc);
      return new DateTime (dateTime.Ticks, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.Disassemble" />
    /// </summary>
    /// <param name="value"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public object Disassemble(object value, NHibernate.Engine.ISessionImplementor session)
    {
      return DeepCopy (value);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.Assemble" />
    /// </summary>
    /// <param name="cached"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Assemble(object cached, NHibernate.Engine.ISessionImplementor session, object owner)
    {
      return DeepCopy (cached);
    }
    
    /// <summary>
    /// <see cref="ICompositeUserType.Replace" />
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public object Replace(object original, object target, NHibernate.Engine.ISessionImplementor session, object owner)
    {
      return DeepCopy (original);
    }
  }
}
