// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Linq;
using System.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IFactDAO">IFactDAO</see>
  /// </summary>
  public class FactDAO
    : VersionableByMonitoredMachineNHibernateDAO<Fact, IFact, int>
    , IFactDAO
    , IOverlapsRangeByStepDefaultStrategy<IFact, IMachine>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FactDAO).FullName);

    /// <summary>
    /// Get the last fact for the specified machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IFact GetLast (IMachine machine)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .AddOrder (Order.Desc ("Begin"))
        .SetMaxResults (1)
        .UniqueResult<IFact> ();
    }
    
    /// <summary>
    /// Find the first fact after a specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    public IFact FindFirstFactAfter (IMachine machine, LowerBound<DateTime> utcDateTime)
    {
      Debug.Assert (null != machine);

      if (!utcDateTime.HasValue) {
        Debug.Assert (BoundType.Lower == utcDateTime.BoundType);
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Fact> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          .AddOrder (Order.Asc ("Begin"))
          .SetMaxResults (1)
          .UniqueResult<IFact> ();
      }
      else {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Fact> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          .Add (Restrictions.Ge ("Begin", utcDateTime.Value))
          .AddOrder (Order.Asc ("Begin"))
          .SetMaxResults (1)
          .UniqueResult<IFact> ();
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IFact> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      return FindAllInUtcRange (machine, range);
    }

    /// <summary>
    /// Find all the facts in a specified UTC date/time range
    /// Ordered by ascending "Begin"
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IFact> FindAllInUtcRange (IMachine machine,
                                           UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        .List<IFact> ();
    }
    
    /// <summary>
    /// Get all the facts either after a specified date/time
    /// with an eager fetch of MachineMode
    /// 
    /// Order the result
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="after">UTC minimum date/time</param>
    /// <param name="maxNumber">Max number of facts to retrieve</param>
    /// <returns></returns>
    public IList<IFact> FindAllAfter (IMachine machine,
                                      DateTime after,
                                      int maxNumber)
    {
      Debug.Assert (null != machine);

      log.DebugFormat ("FindAllAfter: " +
                       "machine={0} after={1} maxNumber={2}",
                       machine, after, maxNumber);
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Gt ("End", after))
        .AddOrder (Order.Asc ("Begin"))
        .Fetch (SelectMode.Fetch, "CncMachineMode")
        .SetMaxResults (maxNumber)
        .List<IFact> ();
    }

    /// <summary>
    /// Get all the facts in a specified date/time range
    /// 
    /// Order the result
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="maxNumber">Max number of facts to retrieve</param>
    /// <returns></returns>
    public IList<IAutoSequencePeriod> FindAllAutoSequencePeriodsBetween (IMachine machine,
                                                                         UtcDateTimeRange range,
                                                                         int maxNumber)
    {
      Debug.Assert (null != machine);

      log.DebugFormat ("FindAllAutoSequencePeriodsBetween: " +
                       "machine={0} range={1} maxNumber={2}",
                       machine, range, maxNumber);
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (maxNumber)
        .List<IAutoSequencePeriod> ();
    }

    /// <summary>
    /// Get all the facts that take part of the auto-sequence process
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="autoSequenceAfter">UTC minumem date/time for auto-sequence facts</param>
    /// <param name="maxNumber">Max number of facts to retrieve</param>
    /// <returns></returns>
    public IList<IFact> FindAllAutoSequence (IMachine machine,
                                             DateTime autoSequenceAfter,
                                             int maxNumber)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .CreateAlias ("CncMachineMode", "machineMode")
        .Add (Restrictions.Conjunction ()
              .Add (Restrictions.Gt ("End", autoSequenceAfter))
              .Add (Restrictions.Eq ("machineMode.AutoSequence", true)))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (maxNumber)
        .List<IFact> ();
    }
    
    AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      LowerBound<DateTime> lower;
      if (!range.LowerInclusive && range.Lower.HasValue) {
        lower = range.Lower.Value.AddSeconds (1);
      }
      else {
        lower = range.Lower;
      }
      UpperBound<DateTime> upper;
      if (range.UpperInclusive && range.Upper.HasValue) {
        upper = range.Upper.Value.AddSeconds (1);
      }
      else {
        upper = range.Upper;
      }
      return InUtcRange (lower, upper);
    }
    
    /// <summary>
    /// Range criterion with UTC date/times
    /// </summary>
    /// <param name="utcFrom">UTC from nullable date/time</param>
    /// <param name="utcTo">UTC to nullable date/time</param>
    /// <returns></returns>
    AbstractCriterion InUtcRange (LowerBound<DateTime> utcFrom,
                                  UpperBound<DateTime> utcTo)
    {
      if (!utcFrom.HasValue && !utcTo.HasValue) {
        return Expression.Sql ("TRUE");
      }
      
      DateTime minDate = new DateTime (1970, 1, 3, 0, 0, 0,
                                       DateTimeKind.Utc);
      if (utcFrom.HasValue
          && (utcFrom.Value < minDate)) {
        utcFrom = minDate;
      }
      if (utcTo.HasValue
          && (utcTo.Value < minDate)) {
        utcTo = minDate;
      }
      
      if (utcFrom.HasValue
          && NullableDateTime.Compare (utcTo, utcFrom) < 0) {
        return Expression.Sql ("FALSE");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (utcFrom.HasValue) {
        result = result
          .Add (Restrictions.Gt ("End", utcFrom.Value));
      }
      
      // To constraint
      if (utcTo.HasValue) {
        result = result
          .Add (Restrictions.Lt ("Begin", utcTo.Value));
      }
      
      return result;
    }
    
    IFact FindOldest (IMachine machine)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (1)
        .UniqueResult<IFact> ();
    }
    
    /// <summary>
    /// Find the oldest fact
    /// </summary>
    /// <returns></returns>
    public IFact FindOldest ()
    {
      // Note: This is much more efficient to visit the data for each machine separately
      //       because you can use the index
      //       Else a sequential scan is used, which is very inefficient
      IFact oldest = null;
      IList<IMonitoredMachine> machines = ModelDAOHelper.DAOFactory
        .MonitoredMachineDAO.FindAll ();
      foreach (IMonitoredMachine machine in machines) {
        IFact fact = FindOldest (machine);
        if (null != fact) {
          if ( (null == oldest)
              || (Bound.Compare<DateTime> (fact.Begin, oldest.Begin) < 0)) {
            oldest = fact;
          }
        }
      }
      
      return oldest;
    }

    /// <summary>
    /// Find a fact containing the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="datetime"></param>
    /// <returns></returns>
    public IFact FindAt(IMachine machine, DateTime datetime)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<Fact>()
        .Add(Restrictions.Eq("Machine.Id", machine.Id))
        .Add(Restrictions.Le("Begin", datetime))
        .Add(Restrictions.Gt("End", datetime))
        .SetMaxResults(1)
        .UniqueResult<IFact>();
    }

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    public virtual IEnumerable<IFact> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Fact> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRange (range))
        .AddOrder (descending
                   ? Order.Desc ("Begin")
                   : Order.Asc ("Begin"))
        .SetMaxResults (n)
        .List<IFact> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in an ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<IFact> FindOverlapsRangeAscending (IMachine machine,
                                                                  UtcDateTimeRange range,
                                                                  TimeSpan step)
    {
      Debug.Assert (null != machine);

      var logger = LogManager.GetLogger (typeof (FactDAO).FullName + "." + machine.Id);
      var strategy = new DefaultOverlapsRangeByStepStrategy<IFact, IMachine> (this, step, false);
      return new FindOverlapsRangeEnumerable<IFact, IMachine> (strategy, machine, range, logger);
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<IFact> FindOverlapsRangeDescending (IMachine machine,
                                                                   UtcDateTimeRange range,
                                                                   TimeSpan step)
    {
      Debug.Assert (null != machine);

      var logger = LogManager.GetLogger (typeof (FactDAO).FullName + "." + machine.Id);
      var strategy = new DefaultOverlapsRangeByStepStrategy<IFact, IMachine> (this, step, true);
      return new FindOverlapsRangeEnumerable<IFact, IMachine> (strategy, machine, range, logger);
    }
  }
}
