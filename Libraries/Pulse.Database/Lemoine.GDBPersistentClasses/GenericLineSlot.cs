// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of UserSlot.
  /// </summary>
  public abstract class GenericLineSlot: BeginEndSlot, IPartitionedByLine
  {
    ILog log = LogManager.GetLogger(typeof (GenericLineSlot).FullName);
    
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }
    
    #region Members
    /// <summary>
    /// Associated line
    /// </summary>
    protected ILine m_line;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the Line
    /// </summary>
    public virtual ILine Line {
      get { return m_line; }
      protected set
      {
        m_line = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor should not be used outside the assembly
    /// 
    /// This constructor must be followed by the Initialize method.
    /// </summary>
    protected GenericLineSlot ()
    {
    }
    
    /// <summary>
    /// Create a new LineSlot (factory method)
    /// </summary>
    /// <param name="type">Type of the Line slot to create</param>
    /// <param name="line">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static GenericLineSlot Create (Type type,
                                          ILine line,
                                          UtcDateTimeRange range)
    {
      Debug.Assert (null != line);
      System.Reflection.ConstructorInfo constructorInfo = type.GetConstructor (new Type[] { typeof (ILine), typeof (UtcDateTimeRange)});
      return (GenericLineSlot)
        constructorInfo.Invoke (new Object[] {line, range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    protected GenericLineSlot (ILine line,
                               UtcDateTimeRange range)
      : base (range)
    {
      Debug.Assert (null != line);
      if (null == line) {
        log.FatalFormat ("GenericLineSlot: " +
                         "line can't be null");
        throw new ArgumentNullException ("line");
      }
      this.m_line = line;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * BeginDateTime.GetHashCode();
        if (this.Line != null) {
          hashCode += 1000000009 * this.Line.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      GenericLineSlot other = obj as GenericLineSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.Line, other.Line);
    }
    #endregion // Methods
  }
}
