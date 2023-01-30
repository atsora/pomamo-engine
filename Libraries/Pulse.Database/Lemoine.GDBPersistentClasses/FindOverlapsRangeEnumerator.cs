// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.Core.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Interface a DAO must implement to use a FindOverlapsRangeEnumerator with the default strategy
  /// </summary>
  /// <typeparam name="I"></typeparam>
  /// <typeparam name="IPartitionKey"></typeparam>
  public interface IOverlapsRangeByStepDefaultStrategy<I, IPartitionKey>
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="partitionKey">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<I> FindOverlapsRange (IPartitionKey partitionKey, UtcDateTimeRange range);

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    IEnumerable<I> FindFirstOverlapsRange (IPartitionKey partitionKey, UtcDateTimeRange range, int n, bool descending);
  }

  /// <summary>
  /// Default OverlapsRangeByStep strategy implementation
  /// </summary>
  /// <typeparam name="I"></typeparam>
  /// <typeparam name="IPartitionKey"></typeparam>
  public class DefaultOverlapsRangeByStepStrategy<I, IPartitionKey>
    : IFindOverlapsRangeByStepStrategy<I, IPartitionKey>
  {
    readonly string INITIAL_STEP_NUMBER_KEY = "Database.SlotDAO.FindOverlapsRangeStep.InitialStepNumber";
    readonly int INITIAL_STEP_NUMBER_DEFAULT = 4;

    readonly string LOWER_LIMIT_KEY = "Database.SlotDAO.FindOverlapsRangeStep.LowerLimit";
    readonly LowerBound<DateTime> LOWER_LIMIT_DEFAULT = new DateTime (2014, 01, 01, 00, 00, 00, DateTimeKind.Utc);

    readonly string MAX_GAP_KEY = "Database.SlotDAO.FindOverlapsRangeStep.MaxGap";
    readonly TimeSpan? MAX_GAP_DEFAULT = TimeSpan.FromDays (90); // 90 days

    readonly IOverlapsRangeByStepDefaultStrategy<I, IPartitionKey> m_dao;
    readonly TimeSpan m_step;
    readonly bool m_descending;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dao"></param>
    /// <param name="step"></param>
    /// <param name="descending"></param>
    public DefaultOverlapsRangeByStepStrategy (IOverlapsRangeByStepDefaultStrategy<I, IPartitionKey> dao, TimeSpan step, bool descending)
    {
      m_dao = dao;
      m_step = step;
      m_descending = descending;
    }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool Descending { get { return m_descending; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public UtcDateTimeRange LimitRange
    {
      get
      {
        var lower = Lemoine.Info.ConfigSet
          .LoadAndGet (LOWER_LIMIT_KEY, LOWER_LIMIT_DEFAULT);
        return new UtcDateTimeRange (lower, DateTime.UtcNow);
      }
    }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public TimeSpan? MaxGap
    {
      get
      {
        return Lemoine.Info.ConfigSet
          .LoadAndGet (MAX_GAP_KEY, MAX_GAP_DEFAULT);
      }
    }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public TimeSpan Step { get { return m_step; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<I> InitialRequest (IPartitionKey partitionKey, UtcDateTimeRange range)
    {
      var initialStepNumber = Lemoine.Info.ConfigSet
        .LoadAndGet (INITIAL_STEP_NUMBER_KEY, INITIAL_STEP_NUMBER_DEFAULT);
      return m_dao.FindFirstOverlapsRange (partitionKey, range, initialStepNumber, m_descending);
    }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool MaxResultsInitial { get { return true; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool LimitRangeInitial { get { return false; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<I> NextRequest (IPartitionKey partitionKey, UtcDateTimeRange range)
    {
      var requestResult = m_dao.FindOverlapsRange (partitionKey, range);
      if (m_descending) {
        return requestResult.Reverse ();
      }
      else {
        return requestResult;
      }
    }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool MaxResultsNext { get { return false; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool LimitRangeNext { get { return true; } }

    /// <summary>
    /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
    /// </summary>
    public bool ReadWrite { get { return false; } }
  }

  /// <summary>
  /// Strategy interface to set in FindOverlapsRangeEnumerable
  /// </summary>
  /// <typeparam name="I"></typeparam>
  /// <typeparam name="IPartitionKey"></typeparam>
  public interface IFindOverlapsRangeByStepStrategy<I, IPartitionKey>
  {
    /// <summary>
    /// Initial request
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<I> InitialRequest (IPartitionKey partitionKey, UtcDateTimeRange range);

    /// <summary>
    /// Does the initial request limit the number of results ?
    /// </summary>
    bool MaxResultsInitial { get; }

    /// <summary>
    /// Does the initial request should consider a limited range that is computed from the step parameter ?
    /// </summary>
    bool LimitRangeInitial { get; }

    /// <summary>
    /// Next request
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<I> NextRequest (IPartitionKey partitionKey, UtcDateTimeRange range);

    /// <summary>
    /// Does the next request limit the number of results ?
    /// </summary>
    bool MaxResultsNext { get; }

    /// <summary>
    /// Does the next request should consider a limited range that is computed from the step parameter ?
    /// </summary>
    bool LimitRangeNext { get; }

    /// <summary>
    /// Step
    /// </summary>
    TimeSpan Step { get; }

    /// <summary>
    /// [Optional] Max gap
    /// </summary>
    TimeSpan? MaxGap { get; }

    /// <summary>
    /// Limit range
    /// </summary>
    UtcDateTimeRange LimitRange { get; }

    /// <summary>
    /// Descending order ?
    /// </summary>
    bool Descending { get; }

    /// <summary>
    /// Force the internal transactions to be read-write
    /// </summary>
    bool ReadWrite { get; }
  }

  internal class EnumeratorRequest<I, IPartitionKey> // struct is ok in .NET 4, not in .NET 3.5
    where I : IWithDateTimeRange
  {
    public Func<IPartitionKey, UtcDateTimeRange, IEnumerable<I>> Request { get; private set; }
    public bool MaxResults { get; private set; }
    public bool LimitRange { get; private set; }
    public EnumeratorRequest (Func<IPartitionKey, UtcDateTimeRange, IEnumerable<I>> request, bool maxResults, bool limitRange)
    {
      if (!limitRange && !maxResults) {
        throw new ArgumentException ("At least limitRange or maxResults must be true");
      }

      Debug.Assert (limitRange || maxResults);

      this.Request = request;
      this.MaxResults = maxResults;
      this.LimitRange = limitRange;
    }
  }

  /// <summary>
  /// FindOverlapsRangeEnumerable implementation
  /// </summary>
  /// <typeparam name="I"></typeparam>
  /// <typeparam name="IPartitionKey"></typeparam>
  internal class FindOverlapsRangeEnumerable<I, IPartitionKey> : IEnumerable<I>
    where I : IWithDateTimeRange
  {
    readonly ILog m_log = LogManager.GetLogger (typeof (FindOverlapsRangeEnumerable<I, IPartitionKey>).FullName);

    readonly IEnumerator<I> m_enumerator;

    public FindOverlapsRangeEnumerable (IFindOverlapsRangeByStepStrategy<I, IPartitionKey> strategy, IPartitionKey partitionKey, UtcDateTimeRange range, ILog logger)
      : this (new EnumeratorRequest<I, IPartitionKey> (strategy.InitialRequest, strategy.MaxResultsInitial, strategy.LimitRangeInitial), new EnumeratorRequest<I, IPartitionKey> (strategy.NextRequest, strategy.MaxResultsNext, strategy.LimitRangeNext), partitionKey, range, strategy.LimitRange, strategy.Step, strategy.MaxGap, strategy.ReadWrite, strategy.Descending, logger)
    {
    }

    public FindOverlapsRangeEnumerable (EnumeratorRequest<I, IPartitionKey> initialRequest,
                                        EnumeratorRequest<I, IPartitionKey> nextRequest,
                                        IPartitionKey partitionKey,
                                        UtcDateTimeRange range,
                                        UtcDateTimeRange limitRange,
                                        TimeSpan step,
                                        TimeSpan? maxGap,
                                        bool readWriteTransactions,
                                        bool descending,
                                        ILog logger)
    {
      Debug.Assert (null != partitionKey);

      m_log = logger;
      m_log.DebugFormat ("FindOverlapsRangeEnumerable: " +
                         "range={0}",
                         range);
      var correctedRange = new UtcDateTimeRange (range.Intersects (limitRange));

      if (descending) {
        m_enumerator = new FindOverlapsRangeDescendingEnumerator<I, IPartitionKey> (initialRequest,
          nextRequest,
          partitionKey,
          correctedRange,
          step,
          maxGap,
          readWriteTransactions,
          logger);
      }
      else { // ascending
        m_enumerator = new FindOverlapsRangeAscendingEnumerator<I, IPartitionKey> (initialRequest,
          nextRequest,
          partitionKey,
          correctedRange,
          step,
          maxGap,
          readWriteTransactions,
          logger);
      }
    }

    #region IEnumerable implementation
    IEnumerator<I> IEnumerable<I>.GetEnumerator ()
    {
      return m_enumerator;
    }
    #endregion
    #region IEnumerable implementation
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return m_enumerator;
    }
    #endregion
  }

  internal class FindOverlapsRangeAscendingEnumerator<I, IPartitionKey> : IEnumerator<I>
    where I : IWithDateTimeRange
  {
    readonly ILog m_log;
    readonly EnumeratorRequest<I, IPartitionKey> m_initialRequest;
    readonly EnumeratorRequest<I, IPartitionKey> m_nextRequest;
    readonly IPartitionKey m_partitionKey;
    readonly UtcDateTimeRange m_range;
    readonly TimeSpan m_step;
    readonly TimeSpan? m_maxGap;
    readonly bool m_readWriteTransactions;
    bool m_initialStep = true;
    EnumeratorRequest<I, IPartitionKey> m_currentRequest;

    IEnumerator<I> m_currentEnumerator;
    UpperBound<DateTime> m_currentMaxTime;
    I m_latestSlot;

    public FindOverlapsRangeAscendingEnumerator (EnumeratorRequest<I, IPartitionKey> initialRequest,
                                                 EnumeratorRequest<I, IPartitionKey> nextRequest,
                                                 IPartitionKey partitionKey,
                                                 UtcDateTimeRange range,
                                                 TimeSpan step,
                                                 TimeSpan? maxGap,
                                                 bool readWriteTransactions,
                                                 ILog logger)
    {
      Debug.Assert (null != partitionKey);

      m_log = logger;
      m_initialRequest = initialRequest;
      m_nextRequest = nextRequest;
      m_currentRequest = m_initialRequest;
      m_partitionKey = partitionKey;
      m_range = range;
      m_step = step;
      m_maxGap = maxGap;
      m_readWriteTransactions = readWriteTransactions;

      m_log.DebugFormat ("FindOverlapsRangeAscendingEnumerator: " +
                         "range={0}",
                         m_range);
    }

    UtcDateTimeRange GetNextRange ()
    {
      if (m_initialStep) {
        if (m_currentRequest.LimitRange) {
          UpperBound<DateTime> upper;
          if (!m_range.Lower.HasValue) {
            upper = new DateTime (2018, 01, 01, 0, 0, 0, DateTimeKind.Utc);
          }
          else { // m_range.Upper.HasValue
            upper = m_range.Lower.Value.Add (m_step);
          }
          var upperInclusive = m_range.UpperInclusive && Bound.Equals (upper, m_range.Upper);
          return new UtcDateTimeRange (m_range.Lower, upper, m_range.LowerInclusive, upperInclusive);
        }
        else {
          return m_range;
        }
      }
      else { // !m_initialStep: m_currentMinTime is known
        Debug.Assert (m_currentMaxTime.HasValue);
        if (m_currentRequest.LimitRange) {
          return new UtcDateTimeRange (m_currentMaxTime.Value, m_range.Upper, true, m_range.UpperInclusive);
        }
        else {
          var newMaxDateTime = m_currentMaxTime.Value.Add (m_step);
          var upperInclusive = m_range.UpperInclusive && Bound.Equals (newMaxDateTime, m_range.Upper);
          return new UtcDateTimeRange (new UtcDateTimeRange (m_currentMaxTime.Value, newMaxDateTime, true, upperInclusive));
        }
      }
    }

    #region IEnumerator implementation
    bool IEnumerator.MoveNext ()
    {
      if (m_range.IsEmpty ()) {
        if (m_log.IsDebugEnabled) {
          m_log.DebugFormat ("MoveNext: " +
                             "empty range => return false");
        }
        return false;
      }

      if (null != m_currentEnumerator) {
        if (m_currentEnumerator.MoveNext ()) {
          return true;
        }
      }

      if ((null != m_latestSlot) && !m_latestSlot.DateTimeRange.Upper.HasValue) {
        if (m_log.IsDebugEnabled) {
          m_log.DebugFormat ("MoveNext: the newest possible slot was already found => return false");
        }
        return false;
      }

      if (!m_initialStep) {
        if (!m_currentMaxTime.HasValue) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: m_currentMaxTime {0} reached +oo => return false", m_currentMaxTime);
          }
          return false;
        }
        if (!m_range.ContainsElement (m_currentMaxTime)) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: curent max time {0} not in range {1} => return false", m_currentMaxTime, m_range);
          }
          return false;
        }
        if (UpperBound.Equals<DateTime> (m_range.Upper, m_currentMaxTime)) {
          if (m_log.IsDebugEnabled) {
            m_log.Debug ($"MoveNext: current max time {m_currentMaxTime} reached the upper bound of {m_range} => return false");
          }
          return false;
        }
        if (m_maxGap.HasValue
            && (null != m_latestSlot)
            && m_latestSlot.DateTimeRange.Upper.HasValue
            && (m_maxGap.Value < m_currentMaxTime.Value.Subtract (m_latestSlot.DateTimeRange.Upper.Value))) {
          if (m_log.IsInfoEnabled) {
            m_log.InfoFormat ("MoveNext: " +
                              "max gap {0} detected, newestRetrieved={1} VS currentMaxTime={2} => return false",
                              m_maxGap.Value, m_latestSlot.DateTimeRange.Upper.Value, m_currentMaxTime.Value);
          }
          return false;
        }
      }

      // Else switch to the next enumerator
      var range = GetNextRange ();
      if (range.IsEmpty ()) {
        if (m_log.IsDebugEnabled) {
          m_log.Debug ($"MoveNext: next range {range} is empty (last one ?) => return false");
        }
        return false;
      }
      if (!range.Overlaps (m_range)) {
        if (m_log.IsWarnEnabled) {
          m_log.Warn ($"MoveNext: next range {range} does not overlap {m_range} => return false");
        }
        return false;
      }
      range = new UtcDateTimeRange (range.Intersects (m_range));
      if (range.IsEmpty ()) {
        if (m_log.IsWarnEnabled) {
          m_log.WarnFormat ("MoveNext: new returned range is empty => return false");
        }
        return false;
      }
      IEnumerable<I> slots;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (m_readWriteTransactions) {
          using (var transaction = session.BeginTransaction ("FindOverlapsRangeEnumerator")) {
            slots = m_currentRequest.Request (m_partitionKey, range);
            transaction.Commit ();
          }
        }
        else {
          using (var transaction = session.BeginReadOnlyTransaction ("FindOverlapsRangeEnumerator")) {
            slots = m_currentRequest.Request (m_partitionKey, range);
          }
        }
      }
      try {
        if (!slots.Any ()) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: no slot in range {0}", range);
          }
          if (!m_currentRequest.LimitRange) {
            return false;
          }
          else { // Try the next enumerator
            m_currentMaxTime = range.Upper;
            m_currentEnumerator = null;
            return ((IEnumerator)this).MoveNext ();
          }
        }
        else { // slots.Any ()
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: {0} slots in range {1}", slots.Count (), range);
          }
          m_latestSlot = slots.Last ();
          if (m_currentRequest.MaxResults) {
            m_currentMaxTime = m_latestSlot.DateTimeRange.Upper;
          }
          else { // !m_currentRequest.MaxResults
            if (Bound.Compare<DateTime> (range.Upper, m_latestSlot.DateTimeRange.Upper) < 0) {
              m_currentMaxTime = m_latestSlot.DateTimeRange.Upper;
            }
            else {
              m_currentMaxTime = range.Upper;
            }
          }
          m_currentEnumerator = slots.GetEnumerator ();
          var moveNextResult = m_currentEnumerator.MoveNext ();
          Debug.Assert (true == moveNextResult); // Because slots.Any ()
          return true;
        }
      }
      finally {
        m_initialStep = false;
        m_currentRequest = m_nextRequest;
      }
    }
    void IEnumerator.Reset ()
    {
      m_initialStep = true;
      m_currentRequest = m_initialRequest;
      m_currentEnumerator = null;
    }
    object System.Collections.IEnumerator.Current
    {
      get
      {
        if (null == m_currentEnumerator) {
          m_log.ErrorFormat ("Current: current enumerator is null, MoveNext must be called first");
          throw new InvalidOperationException ();
        }
        return m_currentEnumerator.Current;
      }
    }
    #endregion
    #region IDisposable implementation
    void IDisposable.Dispose ()
    {
      if (null != m_currentEnumerator) {
        m_currentEnumerator.Dispose ();
        m_currentEnumerator = null;
      }
    }
    #endregion
    #region IEnumerator implementation
    I IEnumerator<I>.Current
    {
      get
      {
        return m_currentEnumerator.Current;
      }
    }
    #endregion
  }

  internal class FindOverlapsRangeDescendingEnumerator<I, IPartitionKey> : IEnumerator<I>
    where I : IWithDateTimeRange
  {
    readonly ILog m_log;
    readonly EnumeratorRequest<I, IPartitionKey> m_initialRequest;
    readonly EnumeratorRequest<I, IPartitionKey> m_nextRequest;
    readonly IPartitionKey m_partitionKey;
    readonly UtcDateTimeRange m_range;
    readonly TimeSpan m_step;
    readonly TimeSpan? m_maxGap;
    readonly bool m_readWriteTransactions;
    bool m_initialStep = true;
    EnumeratorRequest<I, IPartitionKey> m_currentRequest;

    IEnumerator<I> m_currentEnumerator;
    LowerBound<DateTime> m_currentMinTime;
    I m_latestSlot;

    public FindOverlapsRangeDescendingEnumerator (EnumeratorRequest<I, IPartitionKey> initialRequest,
                                                  EnumeratorRequest<I, IPartitionKey> nextRequest,
                                                  IPartitionKey partitionKey,
                                                  UtcDateTimeRange range,
                                                  TimeSpan step,
                                                  TimeSpan? maxGap,
                                                  bool readWriteTransactions,
                                                  ILog logger)
    {
      Debug.Assert (null != partitionKey);

      m_log = logger;

      m_initialRequest = initialRequest;
      m_nextRequest = nextRequest;
      m_currentRequest = m_initialRequest;
      m_partitionKey = partitionKey;
      m_range = range;
      m_step = step;
      m_maxGap = maxGap;
      m_readWriteTransactions = readWriteTransactions;

      if (m_log.IsDebugEnabled) {
        m_log.DebugFormat ("FindOverlapsRangeDescendingEnumerator: " +
                           "range={0}",
                           m_range);
      }
    }

    UtcDateTimeRange GetNextRange ()
    {
      if (m_initialStep) {
        if (m_currentRequest.LimitRange) {
          LowerBound<DateTime> lower;
          var now = DateTime.UtcNow;
          if (Bound.Compare<DateTime> (now, m_range.Upper) < 0) {
            lower = now.Subtract (m_step);
          }
          else { // m_range.Upper <= now
            Debug.Assert (m_range.Upper.HasValue);
            lower = m_range.Upper.Value.Subtract (m_step);
          }
          return new UtcDateTimeRange (lower, m_range.Upper, true, m_range.UpperInclusive);
        }
        else {
          return m_range;
        }
      }
      else { // !m_initialStep: m_currentMinTime is known
        Debug.Assert (m_currentMinTime.HasValue);
        if (m_currentRequest.LimitRange) {
          return new UtcDateTimeRange (m_range.Lower, m_currentMinTime.Value);
        }
        else {
          var newMinDateTime = m_currentMinTime.Value.Subtract (m_step);
          return new UtcDateTimeRange (new UtcDateTimeRange (newMinDateTime, m_currentMinTime.Value));
        }
      }
    }

    #region IEnumerator implementation
    bool IEnumerator.MoveNext ()
    {
      if (m_range.IsEmpty ()) {
        if (m_log.IsDebugEnabled) {
          m_log.DebugFormat ("MoveNext: " +
                             "empty range => return false");
        }
        return false;
      }

      if (null != m_currentEnumerator) {
        if (m_currentEnumerator.MoveNext ()) {
          return true;
        }
      }

      if ((null != m_latestSlot) && !m_latestSlot.DateTimeRange.Lower.HasValue) {
        if (m_log.IsDebugEnabled) {
          m_log.DebugFormat ("MoveNext: the oldest possible slot was already found => return false");
        }
        return false;
      }

      if (!m_initialStep) {
        if (!m_currentMinTime.HasValue) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: m_currentMinTime {0} reached -oo => return false", m_currentMinTime);
          }
          return false;
        }
        if (!m_range.ContainsElement (m_currentMinTime)) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: curent min time {0} not in range {1} => return false", m_currentMinTime, m_range);
          }
          return false;
        }
        if (LowerBound.Equals<DateTime> (m_range.Lower, m_currentMinTime)) {
          if (m_log.IsDebugEnabled) {
            m_log.Debug ($"MoveNext: current min time {m_currentMinTime} reached the lower bound of {m_range} => return false");
          }
          return false;
        }
        if (m_maxGap.HasValue
            && (null != m_latestSlot)
            && m_latestSlot.DateTimeRange.Lower.HasValue
            && (m_maxGap.Value < m_latestSlot.DateTimeRange.Lower.Value.Subtract (m_currentMinTime.Value))) {
          if (m_log.IsInfoEnabled) {
            m_log.InfoFormat ("MoveNext: " +
                              "max gap {0} detected, oldestRetrieved={1} VS currentMinTime={2} => return false",
                              m_maxGap.Value, m_latestSlot.DateTimeRange.Lower.Value, m_currentMinTime.Value);
          }
          return false;
        }
      }

      // Else switch to the next enumerator
      var range = GetNextRange ();
      if (range.IsEmpty ()) {
        if (m_log.IsDebugEnabled) {
          m_log.Debug ($"MoveNext: next range {range} is empty (last one ?) => return false");
        }
        return false;
      }
      if (!range.Overlaps (m_range)) {
        if (m_log.IsWarnEnabled) {
          m_log.Warn ($"MoveNext: next range {range} does not overlap {m_range} => return false");
        }
        return false;
      }
      range = new UtcDateTimeRange (range.Intersects (m_range));
      if (range.IsEmpty ()) {
        if (m_log.IsWarnEnabled) {
          m_log.WarnFormat ("MoveNext: new returned range is empty => return false");
        }
        return false;
      }
      IEnumerable<I> slots;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (m_readWriteTransactions) {
          using (var transaction = session.BeginTransaction ("FindOverlapsRangeEnumerator")) {
            slots = m_currentRequest.Request (m_partitionKey, range);
            transaction.Commit ();
          }
        }
        else {
          using (var transaction = session.BeginReadOnlyTransaction ("FindOverlapsRangeEnumerator")) {
            slots = m_currentRequest.Request (m_partitionKey, range);
          }
        }
      }
      try {
        if (!slots.Any ()) {
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: no slot in range {0}", range);
          }
          if (!m_currentRequest.LimitRange) {
            return false;
          }
          else { // Try the next enumerator
            m_currentMinTime = range.Lower;
            m_currentEnumerator = null;
            return ((IEnumerator)this).MoveNext ();
          }
        }
        else { // slots.Any ()
          if (m_log.IsDebugEnabled) {
            m_log.DebugFormat ("MoveNext: {0} slots in range {1}", slots.Count (), range);
          }
          m_latestSlot = slots.Last ();
          if (m_currentRequest.MaxResults) {
            m_currentMinTime = m_latestSlot.DateTimeRange.Lower;
          }
          else { // !m_currentRequest.MaxResults
            if (Bound.Compare<DateTime> (m_latestSlot.DateTimeRange.Lower, range.Lower) < 0) {
              m_currentMinTime = m_latestSlot.DateTimeRange.Lower;
            }
            else {
              m_currentMinTime = range.Lower;
            }
          }
          m_currentEnumerator = slots.GetEnumerator ();
          var moveNextResult = m_currentEnumerator.MoveNext ();
          Debug.Assert (true == moveNextResult); // Because slots.Any ()
          return true;
        }
      }
      finally {
        m_initialStep = false;
        m_currentRequest = m_nextRequest;
      }
    }
    void IEnumerator.Reset ()
    {
      m_initialStep = true;
      m_currentRequest = m_initialRequest;
      m_currentEnumerator = null;
    }
    object System.Collections.IEnumerator.Current
    {
      get
      {
        if (null == m_currentEnumerator) {
          m_log.ErrorFormat ("Current: current enumerator is null, MoveNext must be called first");
          throw new InvalidOperationException ();
        }
        return m_currentEnumerator.Current;
      }
    }
    #endregion
    #region IDisposable implementation
    void IDisposable.Dispose ()
    {
      if (null != m_currentEnumerator) {
        m_currentEnumerator.Dispose ();
        m_currentEnumerator = null;
      }
    }
    #endregion
    #region IEnumerator implementation
    I IEnumerator<I>.Current
    {
      get
      {
        return m_currentEnumerator.Current;
      }
    }
    #endregion
  }

}
