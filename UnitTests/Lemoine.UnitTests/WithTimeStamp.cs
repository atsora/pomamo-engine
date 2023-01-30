// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Description of UnitTestWithTimeStamps.
  /// </summary>
  public abstract class WithTimeStamp
  {
    protected DateTime m_baseDateTime;
    
    static readonly ILog log = LogManager.GetLogger(typeof (WithTimeStamp).FullName);
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="baseDateTime">in UTC !</param>
    protected WithTimeStamp (DateTime baseDateTime)
    {
      m_baseDateTime = baseDateTime.ToUniversalTime ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get a time stamp
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    protected abstract DateTime T(int n);
    
    /// <summary>
    /// Convenient function to create a range, relative to the origin
    /// </summary>
    /// <param name="l">number of seconds to add, defining the start of the range</param>
    /// <param name="u">number of seconds to add, defining the end of the range. Can be null</param>
    protected virtual UtcDateTimeRange R(int l, int? u)
    {
      return u.HasValue
        ? new UtcDateTimeRange (T(l), T(u.Value))
        : new UtcDateTimeRange (T(l));
    }

    /// <summary>
    /// Convenient function to create a range, relative to the origin
    /// </summary>
    /// <param name="l">number of seconds to add, defining the start of the range</param>
    /// <param name="u">number of seconds to add, defining the end of the range. Can be null</param>
    /// <param name="inclusivity"></param>
    protected virtual UtcDateTimeRange R (int l, int? u, string inclusivity)
    {
      return u.HasValue
        ? new UtcDateTimeRange (T (l), T (u.Value), inclusivity)
        : new UtcDateTimeRange (T (l), new UpperBound<DateTime> (null), inclusivity);
    }

    /// <summary>
    /// Convenient function to create a range, relative to the origin
    /// </summary>
    /// <param name="l"></param>
    /// <returns></returns>
    protected virtual UtcDateTimeRange R (int l)
    {
      return R (l, null);
    }
    #endregion // Methods
  }
}
