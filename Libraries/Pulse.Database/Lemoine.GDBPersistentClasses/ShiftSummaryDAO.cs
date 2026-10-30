// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftSummaryDAO">IShiftSummaryDAO</see>
  /// </summary>
  public class ShiftSummaryDAO
    : VersionableNHibernateDAO<ShiftSummary, IShiftSummary, int>
    , IShiftSummaryDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummaryDAO).FullName);

    /// <summary>
    /// <see cref="IShiftSummaryDAO"/>
    /// </summary>
    public async Task<IShiftSummary> FindAsync (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (machine is not null);

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day.Date));
      criteria = (shift is null)
        ? criteria.Add (Restrictions.IsNull ("Shift"))
        : criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      return await criteria.UniqueResultAsync<IShiftSummary> ();
    }

    /// <summary>
    /// <see cref="IShiftSummaryDAO"/>
    /// </summary>
    public async Task<IList<IShiftSummary>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (machine is not null);
      Debug.Assert (range is not null);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<IShiftSummary> ();
    }

    /// <summary>
    /// <see cref="IShiftSummaryDAO"/>
    /// </summary>
    public async Task<IList<IShiftSummary>> FindOverlapsRangeAsync (UtcDateTimeRange range)
    {
      Debug.Assert (range is not null);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummary> ()
        .Add (new SimpleExpression ("DateTimeRange", range, "&&"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<IShiftSummary> ();
    }
  }

  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftSummarySectionDAO">IShiftSummarySectionDAO</see>
  /// </summary>
  public class ShiftSummarySectionDAO
    : VersionableNHibernateDAO<ShiftSummarySection, IShiftSummarySection, int>
    , IShiftSummarySectionDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummarySectionDAO).FullName);

    /// <summary>
    /// <see cref="IShiftSummarySectionDAO"/>
    /// </summary>
    public async Task<IList<IShiftSummarySection>> FindBySummaryAsync (IShiftSummary shiftSummary)
    {
      Debug.Assert (shiftSummary is not null);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummarySection> ()
        .Add (Restrictions.Eq ("ShiftSummary.Id", shiftSummary.Id))
        .AddOrder (Order.Asc ("SectionKey"))
        .ListAsync<IShiftSummarySection> ();
    }

    /// <summary>
    /// <see cref="IShiftSummarySectionDAO"/>
    /// </summary>
    public async Task<IList<IShiftSummarySection>> FindToComputeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (machine is not null);
      Debug.Assert (range is not null);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummarySection> ()
        .CreateAlias ("ShiftSummary", "summary")
        .Add (Restrictions.Eq ("UpdateRequired", true))
        .Add (Restrictions.Eq ("summary.Machine.Id", machine.Id))
        .Add (new SimpleExpression ("summary.DateTimeRange", range, "&&"))
        .AddOrder (Order.Desc ("summary.DateTimeRange"))
        .AddOrder (Order.Asc ("SectionKey"))
        .ListAsync<IShiftSummarySection> ();
    }

    /// <summary>
    /// <see cref="IShiftSummarySectionDAO"/>
    /// </summary>
    public void SetUpdateRequired (int machineId, DateTime day, int? shiftId, string sectionKey)
    {
      Debug.Assert (!string.IsNullOrEmpty (sectionKey));

      var shiftCondition = shiftId.HasValue ? "m.shiftid = :shiftid" : "m.shiftid IS NULL";
      var query = NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ($"""
          UPDATE shiftsummarysection s SET shiftsummarysectionupdaterequired = TRUE
          FROM shiftsummary m
          WHERE s.shiftsummaryid = m.shiftsummaryid
            AND m.machineid = :machineid AND m.shiftsummaryday = :day AND {shiftCondition}
            AND s.shiftsummarysectionkey = :sectionkey
            AND NOT s.shiftsummarysectionupdaterequired
          """)
        .SetInt32 ("machineid", machineId)
        .SetDateTime ("day", day.Date)
        .SetString ("sectionkey", sectionKey);
      if (shiftId.HasValue) {
        query.SetInt32 ("shiftid", shiftId.Value);
      }
      query.ExecuteUpdate ();
    }

    /// <summary>
    /// <see cref="IShiftSummarySectionDAO"/>
    /// </summary>
    public void SetUpdateRequiredAt (int machineId, DateTime dateTime, string sectionKey)
    {
      Debug.Assert (!string.IsNullOrEmpty (sectionKey));

      NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ("""
          UPDATE shiftsummarysection s SET shiftsummarysectionupdaterequired = TRUE
          FROM shiftsummary m
          WHERE s.shiftsummaryid = m.shiftsummaryid
            AND m.machineid = :machineid AND m.shiftsummarydatetimerange @> CAST(:at AS timestamp)
            AND s.shiftsummarysectionkey = :sectionkey
            AND NOT s.shiftsummarysectionupdaterequired
          """)
        .SetInt32 ("machineid", machineId)
        .SetParameter ("at", dateTime.ToUniversalTime (), new Lemoine.NHibernateTypes.UTCDateTimeFullType ())
        .SetString ("sectionkey", sectionKey)
        .ExecuteUpdate ();
    }
  }

  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftSummaryContentDAO">IShiftSummaryContentDAO</see>
  /// </summary>
  public class ShiftSummaryContentDAO
    : VersionableNHibernateDAO<ShiftSummaryContent, IShiftSummaryContent, int>
    , IShiftSummaryContentDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummaryContentDAO).FullName);

    /// <summary>
    /// <see cref="IShiftSummaryContentDAO"/>
    /// </summary>
    public async Task<IShiftSummaryContent> FindAsync (IShiftSummary shiftSummary, string generator)
    {
      Debug.Assert (shiftSummary is not null);
      Debug.Assert (!string.IsNullOrEmpty (generator));

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummaryContent> ()
        .Add (Restrictions.Eq ("ShiftSummary.Id", shiftSummary.Id))
        .Add (Restrictions.Eq ("Generator", generator))
        .UniqueResultAsync<IShiftSummaryContent> ();
    }

    /// <summary>
    /// <see cref="IShiftSummaryContentDAO"/>
    /// </summary>
    public async Task<IList<IShiftSummaryContent>> FindBySummaryAsync (IShiftSummary shiftSummary)
    {
      Debug.Assert (shiftSummary is not null);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSummaryContent> ()
        .Add (Restrictions.Eq ("ShiftSummary.Id", shiftSummary.Id))
        .ListAsync<IShiftSummaryContent> ();
    }
  }
}
