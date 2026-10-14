// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests on the date/time parameters sent by Npgsql (Npgsql 6+ breaking changes)
  /// to the timestamp without time zone columns, in which the UTC date/times are stored
  /// </summary>
  [TestFixture]
  public class NpgsqlDateTime_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NpgsqlDateTime_UnitTest).FullName);

    /// <summary>
    /// A UTC DateTime parameter with no specific NHibernate type must be sent as a timestamp without time zone
    /// </summary>
    [Test]
    public void TestUtcDateTimeParameterType ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var type = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(pg_typeof(:p) AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("p", new DateTime (2024, 01, 01, 12, 00, 00, DateTimeKind.Utc))
          .UniqueResult<string> ();
        Assert.That (type, Is.EqualTo ("timestamp without time zone"));

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A null DateTime parameter must be sent as a timestamp without time zone
    /// </summary>
    [Test]
    public void TestNullDateTimeParameterType ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var type = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(pg_typeof(:p) AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("p", null, NHibernate.NHibernateUtil.DateTime)
          .UniqueResult<string> ();
        Assert.That (type, Is.EqualTo ("timestamp without time zone"));

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A UTC DateTime parameter must be compared to a timestamp without time zone without any conversion,
    /// whatever the time zone of the session is
    /// </summary>
    [Test]
    public void TestUtcDateTimeParameterComparison ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SET LOCAL TIME ZONE 'America/New_York'")
          .ExecuteUpdate ();

        var equal = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT (:p = CAST('2024-01-01 12:00:00' AS timestamp)) AS e")
          .AddScalar ("e", NHibernate.NHibernateUtil.Boolean)
          .SetParameter ("p", new DateTime (2024, 01, 01, 12, 00, 00, DateTimeKind.Utc))
          .UniqueResult<bool> ();
        Assert.That (equal, Is.True);

        var equalWithType = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT (:p = CAST('2024-01-01 12:00:00' AS timestamp)) AS e")
          .AddScalar ("e", NHibernate.NHibernateUtil.Boolean)
          .SetParameter ("p", new DateTime (2024, 01, 01, 12, 00, 00, DateTimeKind.Utc), NHibernate.NHibernateUtil.DateTime)
          .UniqueResult<bool> ();
        Assert.That (equalWithType, Is.True);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A UTC DateTime is read back without any conversion
    /// </summary>
    [Test]
    public void TestUtcDateTimeRoundTrip ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var dateTime = new DateTime (2024, 01, 01, 12, 34, 56, DateTimeKind.Utc);
        var result = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT :p AS d")
          .AddScalar ("d", new Lemoine.NHibernateTypes.UTCDateTimeFullType ())
          .SetParameter ("p", dateTime)
          .UniqueResult<DateTime> ();
        Assert.Multiple (() => {
          Assert.That (result, Is.EqualTo (dateTime));
          Assert.That (result.Kind, Is.EqualTo (DateTimeKind.Utc));
        });

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// DateTime.MaxValue must not be converted to infinity
    /// </summary>
    [Test]
    public void TestMaxValueNoInfinity ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var isInfinite = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT (NOT isfinite(:p)) AS i")
          .AddScalar ("i", NHibernate.NHibernateUtil.Boolean)
          .SetParameter ("p", DateTime.MaxValue, new Lemoine.NHibernateTypes.UTCDateTimeFullType ())
          .UniqueResult<bool> ();
        Assert.That (isInfinite, Is.False);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A UTC date/time range parameter is sent as a tsrange without any conversion
    /// </summary>
    [Test]
    public void TestUtcDateTimeRangeParameter ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var range = new UtcDateTimeRange (new DateTime (2024, 01, 01, 12, 00, 00, DateTimeKind.Utc),
          new DateTime (2024, 01, 01, 13, 00, 00, DateTimeKind.Utc));
        var text = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(:r AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("r", range, new Lemoine.NHibernateTypes.UTCDateTimeRangeType ())
          .UniqueResult<string> ();
        Assert.That (text, Is.EqualTo ("[\"2024-01-01 12:00:00\",\"2024-01-01 13:00:00\")"));

        var infiniteRange = new UtcDateTimeRange (new DateTime (2024, 01, 01, 12, 00, 00, DateTimeKind.Utc));
        var infiniteText = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(:r AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("r", infiniteRange, new Lemoine.NHibernateTypes.UTCDateTimeRangeType ())
          .UniqueResult<string> ();
        Assert.That (infiniteText, Is.EqualTo ("[\"2024-01-01 12:00:00\",)"));

        var type = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(pg_typeof(:r) AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("r", range, new Lemoine.NHibernateTypes.UTCDateTimeRangeType ())
          .UniqueResult<string> ();
        Assert.That (type, Is.EqualTo ("tsrange"));

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A day range parameter is sent as a daterange and read back
    /// </summary>
    [Test]
    public void TestDayRangeParameter ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        var range = new DayRange (new LowerBound<DateTime> (new DateTime (2024, 01, 01)),
          new UpperBound<DateTime> (new DateTime (2024, 01, 31)), true, true);

        var type = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(pg_typeof(:r) AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("r", range, new Lemoine.NHibernateTypes.DayRangeType ())
          .UniqueResult<string> ();
        Assert.That (type, Is.EqualTo ("daterange"));

        var text = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT CAST(:r AS text) AS t")
          .AddScalar ("t", NHibernate.NHibernateUtil.String)
          .SetParameter ("r", range, new Lemoine.NHibernateTypes.DayRangeType ())
          .UniqueResult<string> ();
        Assert.That (text, Is.EqualTo ("[2024-01-01,2024-02-01)"));

        var infiniteRange = new DayRange (new LowerBound<DateTime> (new DateTime (2024, 01, 01)),
          new UpperBound<DateTime> (null), true, false);
        var readRange = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery ("SELECT :r AS r")
          .AddScalar ("r", new Lemoine.NHibernateTypes.DayRangeType ())
          .SetParameter ("r", infiniteRange, new Lemoine.NHibernateTypes.DayRangeType ())
          .UniqueResult<DayRange> ();
        Assert.Multiple (() => {
          Assert.That (readRange.Lower.Value, Is.EqualTo (new DateTime (2024, 01, 01)));
          Assert.That (readRange.Upper.HasValue, Is.False);
        });

        transaction.Rollback ();
      }
    }
  }
}
