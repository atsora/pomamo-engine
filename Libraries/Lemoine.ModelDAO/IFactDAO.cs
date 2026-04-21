// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IFact.
  /// </summary>
  public interface IFactDAO
    : IGenericByMonitoredMachineUpdateDAO<IFact, int>
  {
    /// <summary>
    /// Get the last fact for the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IFact GetLast (IMachine machine);

    /// <summary>
    /// Find the first fact after a specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime">UTC date/time</param>
    /// <returns></returns>
    IFact FindFirstFactAfter (IMachine machine,
                              LowerBound<DateTime> dateTime);

    /// <summary>
    /// Find all the facts in a specified UTC date/time range
    /// Ordered by ascending "Begin"
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IFact> FindAllInUtcRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find a fact containing the specified UTC date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="datetime"></param>
    /// <returns></returns>
    IFact FindAt (IMachine machine, DateTime datetime);

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
    IList<IFact> FindAllAfter (IMachine machine,
                               DateTime after,
                               int maxNumber);

    /// <summary>
    /// Get all the facts in a specified date/time range
    /// 
    /// Order the result
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="maxNumber">Max number of facts to retrieve</param>
    /// <returns></returns>
    IList<IAutoSequencePeriod> FindAllAutoSequencePeriodsBetween (IMachine machine,
                                                                  UtcDateTimeRange range,
                                                                  int maxNumber);

    /// <summary>
    /// Get all the facts that take part of the auto-sequence process
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="autoSequenceAfter">UTC minumem date/time for auto-sequence facts</param>
    /// <param name="maxNumber">Max number of facts to retrieve</param>
    /// <returns></returns>
    IList<IFact> FindAllAutoSequence (IMachine machine,
                                      DateTime autoSequenceAfter,
                                      int maxNumber);

    /// <summary>
    /// Return the oldest fact
    /// </summary>
    /// <returns></returns>
    IFact FindOldest ();

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    IEnumerable<IFact> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<IFact> FindOverlapsRangeAscending (IMachine machine,
                                                   UtcDateTimeRange range,
                                                   TimeSpan step);

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
    IEnumerable<IFact> FindOverlapsRangeDescending (IMachine machine,
                                                    UtcDateTimeRange range,
                                                    TimeSpan step);
  }

  /// <summary>
  /// Extensions class to <see cref="IFactDAO"/>
  /// </summary>
  public static class FactDAOExtensions
  {
    static Lemoine.Core.Log.ILog log = Lemoine.Core.Log.LogManager.GetLogger (typeof (FactDAOExtensions));

    /// <summary>
    /// Get an enumerator on facts with no gap between them.
    /// 
    /// In case of a gap, return a fictive fact with a machine mode NoData
    /// </summary>
    /// <param name="factDao"></param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="facts"></param>
    /// <returns></returns>
    public static IEnumerable<IFact> GetNoGap (this IFactDAO factDao, IMonitoredMachine machine, UtcDateTimeRange range, IEnumerable<IFact> facts)
    {
      Debug.Assert (null != machine);

      IMachineMode noDataMachineMode = null;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lastFactEnd = range.Lower;
        foreach (var fact in facts) {
          if (Bound.Compare<DateTime> (lastFactEnd, fact.DateTimeRange.Lower) < 0) {
            if (!lastFactEnd.HasValue) {
              log.Warn ($"GetNoGap: gap {lastFactEnd}-{fact.DateTimeRange.Lower} with no lower bound, skip it");
            }
            else {
              noDataMachineMode = GetNoDataMachineMode (noDataMachineMode);
              var noDataFact = ModelDAOHelper.ModelFactory.CreateFact (machine, lastFactEnd.Value, fact.DateTimeRange.Lower.Value, noDataMachineMode);
              yield return noDataFact;
            }
          }
          lastFactEnd = fact.DateTimeRange.Upper.Value;
          yield return fact;
        }
        if ( (Bound.Compare<DateTime> (lastFactEnd, range.Upper) < 0)
          || (range.UpperInclusive && (Bound.Compare<DateTime> (lastFactEnd, range.Upper) <= 0))) {
          if (!range.Upper.HasValue) { // No fact after range
            yield break;
          }
          else if (!lastFactEnd.HasValue) {
            log.Warn ($"GetNoGap: final gap {lastFactEnd}-{range.Upper} with no lower bound, skip it");
          }
          else {
            var nextFact = factDao
              .FindFirstFactAfter (machine, range.Upper.Value);
            if (null == nextFact) { // No fact after range
              yield break;
            }
            else if (Bound.Compare<DateTime> (lastFactEnd.Value, nextFact.DateTimeRange.Lower) < 0) {
              noDataMachineMode = GetNoDataMachineMode (noDataMachineMode);
              var noDataFact = ModelDAOHelper.ModelFactory.CreateFact (machine, lastFactEnd.Value, nextFact.DateTimeRange.Lower.Value, noDataMachineMode);
              yield return noDataFact;
            }
            else { // No gap at the end
              yield break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Get a descending enumerator on facts with no gap between them.
    /// 
    /// In case of a gap, return a fictive fact with a machine mode NoData
    /// </summary>
    /// <param name="factDao"></param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="facts"></param>
    /// <returns></returns>
    public static IEnumerable<IFact> GetNoGapDescending (this IFactDAO factDao, IMonitoredMachine machine, UtcDateTimeRange range, IEnumerable<IFact> facts)
    {
      Debug.Assert (null != machine);

      IMachineMode noDataMachineMode = null;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lastFactStart = range.Upper;
        foreach (var fact in facts) {
          if (Bound.Compare<DateTime> (fact.DateTimeRange.Upper, lastFactStart) < 0) {
            if (!lastFactStart.HasValue) {
              log.Warn ($"GetNoGapDescending: gap {lastFactStart}-{fact.DateTimeRange.Lower} with no lower bound, skip it");
            }
            else {
              noDataMachineMode = GetNoDataMachineMode (noDataMachineMode);
              var noDataFact = ModelDAOHelper.ModelFactory.CreateFact (machine, fact.DateTimeRange.Upper.Value, lastFactStart.Value, noDataMachineMode);
              yield return noDataFact;
            }
          }
          lastFactStart = fact.DateTimeRange.Lower.Value;
          yield return fact;
        }
        if ((Bound.Compare<DateTime> (range.Lower, lastFactStart) < 0)
          || (range.LowerInclusive && (Bound.Compare<DateTime> (range.Lower, lastFactStart) <= 0))) {
          if (!range.Lower.HasValue) { // No fact after range
            yield break;
          }
          else if (!lastFactStart.HasValue) {
            log.Warn ($"GetNoGapDescending: final gap {range.Lower}-{lastFactStart} with no lower bound, skip it");
          }
          else {
            var beforeFacts = factDao
              .FindFirstOverlapsRange (machine, new UtcDateTimeRange (new LowerBound<DateTime> (null), lastFactStart), 1, true);
            if (!beforeFacts.Any ()) { // No fact before range
              yield break;
            }
            else {
              var beforeFact = beforeFacts.Single ();
              if (Bound.Compare<DateTime> (beforeFact.DateTimeRange.Upper, lastFactStart.Value) < 0) {
                noDataMachineMode = GetNoDataMachineMode (noDataMachineMode);
                var noDataFact = ModelDAOHelper.ModelFactory.CreateFact (machine, beforeFact.DateTimeRange.Upper.Value, lastFactStart.Value, noDataMachineMode);
                yield return noDataFact;
              }
              else { // No gap at the end
                yield break;
              }
            }
          }
        }
      }
    }

    static IMachineMode GetNoDataMachineMode (IMachineMode noDataMachineModeCache)
    {
      if (null != noDataMachineModeCache) {
        return noDataMachineModeCache;
      }
      else {
        var noDataMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById ((int)MachineModeId.NoData);
        Debug.Assert (null != noDataMachineMode);
        return noDataMachineMode;
      }
    }
  }
}
