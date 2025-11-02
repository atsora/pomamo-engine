// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IScrapReportDAO"></see>
  /// </summary>
  public class ScrapReportDAO
    : SaveOnlyByMachineNHibernateDAO<ScrapReport, IScrapReport, long>
    , IScrapReportDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapReportDAO).FullName);

    /// <summary>
    /// Get a projection of the lower bound of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetLowerProjection ()
    {
      return Projections
        .SqlFunction ("lower",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Get a projection of the upper bound of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetUpperProjection ()
    {
      return Projections
        .SqlFunction ("upper",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Range criterion with UTC date/times using directly the range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion OverlapRange (UtcDateTimeRange range)
    {
      return new SimpleExpression ("DateTimeRange", range, "&&");
    }

    /// <summary>
    /// <see cref="IScrapReportDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public async Task<IScrapReport> FindActiveAtAsync (IMachine machine, DateTime at)
    {
      DateTime utc;
      switch (at.Kind) {
        case DateTimeKind.Utc:
          utc = at;
          break;
        case DateTimeKind.Local:
          utc = at.ToUniversalTime ();
          break;
        case DateTimeKind.Unspecified:
        default:
          log.Error ($"FindActiveAtAsync: date/time {at} is of kind Unspecified");
          Debug.Assert (DateTimeKind.Unspecified != at.Kind);
          utc = at;
          break;
      }
      var scrapReports = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ScrapReport> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
        .ListAsync<IScrapReport> ();
      if (0 == scrapReports.Count) {
        return null;
      }
      else if (1 == scrapReports.Count) {
        return scrapReports.Single ();
      }
      else {
        return scrapReports.Single (x => !scrapReports.Any (y => (y.ReportUpdate?.Id ?? 0) == x.Id));
      }
    }

    public async Task<IEnumerable<IScrapReport>> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      var scrapReports = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ScrapReport> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .ListAsync<IScrapReport> ();
      return scrapReports.Where (x => !scrapReports.Any (y => (y.ReportUpdate?.Id ?? 0) == x.Id));
    }

    /// <summary>
    /// <see cref="IScrapReportDAO"/>
    /// </summary>
    public async Task<DateTime?> FindLatestReportTimeBeforeAsync (IMachine machine, IOperationSlot operationSlot, DateTime before)
    {
      DateTime utcBefore;
      switch (before.Kind) {
        case DateTimeKind.Utc:
          utcBefore = before;
          break;
        case DateTimeKind.Local:
          utcBefore = before.ToUniversalTime ();
          break;
        case DateTimeKind.Unspecified:
        default:
          log.Error ($"FindLatestReportTimeBeforeAsync: date/time {before} is of kind Unspecified");
          Debug.Assert (DateTimeKind.Unspecified != before.Kind);
          utcBefore = before;
          break;
      }
      var upperProjection = GetUpperProjection ();
      var scrapReport = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ScrapReport> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (operationSlot.DateTimeRange))
        .Add (Restrictions.Lt (upperProjection, utcBefore))
        .AddOrder (Order.Desc (upperProjection))
        .SetMaxResults (1)
        .UniqueResultAsync<IScrapReport> ();
      if (scrapReport is null) {
        return null;
      }
      else {
        return scrapReport.DateTimeRange.Lower.Value;
      }
    }

    /// <summary>
    /// <see cref="IScrapReportDAO"/>
    /// </summary>
    public async Task<DateTime?> FindNextReportTimeAfterAsync (IMachine machine, IOperationSlot operationSlot, DateTime after)
    {
      DateTime utcAfter;
      switch (after.Kind) {
        case DateTimeKind.Utc:
          utcAfter = after;
          break;
        case DateTimeKind.Local:
          utcAfter = after.ToUniversalTime ();
          break;
        case DateTimeKind.Unspecified:
        default:
          log.Error ($"FindNextReportTimeAfterAsync: date/time {after} is of kind Unspecified");
          Debug.Assert (DateTimeKind.Unspecified != after.Kind);
          utcAfter = after;
          break;
      }
      var lowerProjection = GetLowerProjection ();
      var scrapReport = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ScrapReport> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (operationSlot.DateTimeRange))
        .Add (Restrictions.Ge (lowerProjection, utcAfter))
        .AddOrder (Order.Asc (lowerProjection))
        .SetMaxResults (1)
        .UniqueResultAsync<IScrapReport> ();
      if (scrapReport is null) {
        return null;
      }
      else {
        return scrapReport.DateTimeRange.Lower.Value;
      }
    }
  }
}
