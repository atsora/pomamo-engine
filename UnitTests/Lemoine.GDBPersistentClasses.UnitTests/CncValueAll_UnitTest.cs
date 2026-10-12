// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the view cncvalueall and the function cncvaluerange (migration 2004)
  /// </summary>
  [TestFixture]
  public class CncValueAll_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueAll_UnitTest).FullName);

    const int MACHINE_ID = 1;
    static readonly DateTime T0 = new DateTime (2091, 03, 07, 10, 00, 00, DateTimeKind.Utc);
    // In the past: the reads by step are limited to [LowerLimit, now]
    static readonly DateTime P0 = new DateTime (2024, 03, 06, 10, 00, 00, DateTimeKind.Utc);

    string previousDSNName;

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName", Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", previousDSNName);
      }
    }

    record Row (long Id, DateTime Begin, DateTime End, string String, int? Int, double? Double, double? Deviation, bool Stopped,
      string Source, int? Count, double? Min, double? Max);

    /// <summary>
    /// Historical data in cncvalue and new data in cncdataperiod, translated into the columns of cncvalue
    /// </summary>
    [Test]
    public void TestCncValueAll ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var monitoredMachine = daoFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
        var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();

        var average = CreateField ("TestCncValueAllAverage", CncDataAggregationType.Average, FieldType.Double);
        var max = CreateField ("TestCncValueAllMax", CncDataAggregationType.Max, FieldType.Double);
        var sum = CreateField ("TestCncValueAllSum", CncDataAggregationType.Sum, FieldType.Double);
        var boolean = CreateField ("TestCncValueAllBoolean", CncDataAggregationType.NewValue, FieldType.Boolean);
        var text = CreateField ("TestCncValueAllString", CncDataAggregationType.NewValue, FieldType.String);

        // Historical data in cncvalue
        var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, average, T0.AddHours (-1));
        cncValue.End = T0.AddMinutes (-30);
        cncValue.Double = 10.0;
        cncValue.Deviation = 1.0;
        daoFactory.CncValueDAO.MakePersistent (cncValue);

        // New data in cncdataperiod
        AddPeriod (machineModule, average, p => p.Statistics = CncDataStatistics.FromValues (new double[] { 1.0, 3.0 }));
        AddPeriod (machineModule, max, p => p.Statistics = CncDataStatistics.FromValues (new double[] { 1.0, 5.0 }));
        AddPeriod (machineModule, sum, p => p.Statistics = CncDataStatistics.FromValues (new double[] { 1.0, 2.0, 3.0 }));
        AddPeriod (machineModule, boolean, p => p.LastValue = 1.0);
        AddPeriod (machineModule, text, p => p.StringValue = "abc");
        daoFactory.FlushData ();

        var averageRows = ReadView (machineModule, average);
        Assert.That (averageRows, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (averageRows[0].Source, Is.EqualTo ("cncvalue"));
          Assert.That (averageRows[0].Double, Is.EqualTo (10.0));
          Assert.That (averageRows[0].Deviation, Is.EqualTo (1.0));
          Assert.That (averageRows[0].Begin, Is.EqualTo (T0.AddHours (-1)));
          Assert.That (averageRows[0].Count, Is.Null);
          Assert.That (averageRows[1].Source, Is.EqualTo ("cncdataperiod"));
          Assert.That (averageRows[1].Begin, Is.EqualTo (T0));
          Assert.That (averageRows[1].End, Is.EqualTo (T0.AddMinutes (1)));
          Assert.That (averageRows[1].Double, Is.EqualTo (2.0));
          Assert.That (averageRows[1].Deviation, Is.EqualTo (1.0).Within (1e-9));
          Assert.That (averageRows[1].Int, Is.EqualTo (2));
          Assert.That (averageRows[1].String, Is.EqualTo ("2"));
          Assert.That (averageRows[1].Count, Is.EqualTo (2));
          Assert.That (averageRows[1].Min, Is.EqualTo (1.0));
          Assert.That (averageRows[1].Max, Is.EqualTo (3.0));
        });

        var maxRow = ReadView (machineModule, max).Single ();
        var sumRow = ReadView (machineModule, sum).Single ();
        var booleanRow = ReadView (machineModule, boolean).Single ();
        var textRow = ReadView (machineModule, text).Single ();
        Assert.Multiple (() => {
          Assert.That (maxRow.Double, Is.EqualTo (5.0));
          Assert.That (maxRow.Deviation, Is.EqualTo (4.0));
          Assert.That (sumRow.Double, Is.EqualTo (6.0).Within (1e-9));
          Assert.That (sumRow.Deviation, Is.Null);
          Assert.That (booleanRow.Double, Is.EqualTo (1.0));
          Assert.That (booleanRow.Int, Is.EqualTo (1));
          Assert.That (booleanRow.String, Is.EqualTo ("True"));
          Assert.That (textRow.String, Is.EqualTo ("abc"));
          Assert.That (textRow.Int, Is.Null);
          Assert.That (textRow.Double, Is.Null);
        });

        // Function: same rows, restricted to the range
        var rangeRows = ReadRange (machineModule, average, T0.AddMinutes (-45), T0.AddMinutes (10));
        var laterRows = ReadRange (machineModule, average, T0.AddMinutes (-20), T0.AddMinutes (10));
        Assert.Multiple (() => {
          Assert.That (rangeRows, Is.EqualTo (averageRows));
          Assert.That (laterRows.Select (r => r.Source), Is.EqualTo (new[] { "cncdataperiod" }));
        });
      }
    }

    /// <summary>
    /// ICncValueDataDAO: same semantics as ICncValueDAO, from cncvalue and cncdataperiod
    /// </summary>
    [Test]
    public void TestCncValueDataDAO ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var monitoredMachine = daoFactory.MonitoredMachineDAO.FindByIdWithMachineModules (MACHINE_ID);
        var machineModule = monitoredMachine.MachineModules.OrderBy (m => m.Id).First ();
        var average = CreateField ("TestCncValueDataAverage", CncDataAggregationType.Average, FieldType.Double);
        var boolean = CreateField ("TestCncValueDataBoolean", CncDataAggregationType.NewValue, FieldType.Boolean);

        // Historical data in cncvalue [P0-1h, P0), then a period [P0, P0+1min)
        var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, average, P0.AddHours (-1));
        cncValue.End = P0;
        cncValue.Double = 10.0;
        cncValue.Deviation = 1.0;
        daoFactory.CncValueDAO.MakePersistent (cncValue);
        AddPeriod (machineModule, average, P0, p => p.Statistics = CncDataStatistics.FromValues (new double[] { 1.0, 3.0 }));
        AddPeriod (machineModule, boolean, P0, p => p.LastValue = 1.0);
        daoFactory.FlushData ();

        var dao = daoFactory.CncValueDataDAO;
        var before = dao.FindAt (machineModule, average, P0.AddMinutes (-10));
        var atBound = dao.FindAt (machineModule, average, P0);
        var after = dao.FindAt (machineModule, average, P0.AddSeconds (30));
        var withEnd = dao.FindWithEnd (machineModule, average, P0);
        var overlaps = dao.FindOverlapsRange (machineModule, average, new UtcDateTimeRange (P0.AddMinutes (-30), P0.AddMinutes (30)));
        var overlapsAfter = dao.FindOverlapsRange (machineModule, average, new UtcDateTimeRange (P0));
        var all = dao.FindAt (machineModule, P0.AddSeconds (30));
        Assert.Multiple (() => {
          Assert.That (before?.Source, Is.EqualTo (CncValueSource.CncValue));
          Assert.That (before?.Value, Is.EqualTo (10.0));
          Assert.That (atBound?.Source, Is.EqualTo (CncValueSource.CncDataPeriod), "[begin, end): the period at its begin");
          Assert.That (after?.Source, Is.EqualTo (CncValueSource.CncDataPeriod));
          Assert.That (after?.Value, Is.EqualTo (2.0));
          Assert.That (after?.Count, Is.EqualTo (2));
          Assert.That (after?.MachineModule?.Id, Is.EqualTo (machineModule.Id));
          Assert.That (after?.Field?.Id, Is.EqualTo (average.Id));
          Assert.That (withEnd?.Source, Is.EqualTo (CncValueSource.CncValue), "cncvalue that ends at P0");
          Assert.That (overlaps.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncValue, CncValueSource.CncDataPeriod }));
          Assert.That (overlapsAfter.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncDataPeriod }), "the cncvalue ending at P0 is excluded");
          Assert.That (all.Where (v => v.Field.Id == average.Id).Select (v => v.Value), Is.EqualTo (new object[] { 2.0 }));
          Assert.That (all.Where (v => v.Field.Id == boolean.Id).Select (v => v.Value), Is.EqualTo (new object[] { true }));
        });

        // Methods for the plugins: across cncvalue and cncdataperiod
        var next = dao.FindNext (machineModule, average, P0.AddHours (-2), 5);
        var nextAfter = dao.FindNext (machineModule, average, P0.AddMinutes (-10), 5);
        var lastOne = dao.FindFirstOverlapsRange (machineModule, average, new UtcDateTimeRange ("(,)"), 1, true);
        var firstOne = dao.FindFirstOverlapsRange (machineModule, average, new UtcDateTimeRange ("(,)"), 1, false);
        var ascending = dao.FindOverlapsRangeAscending (machineModule, average, new UtcDateTimeRange (P0.AddHours (-2), P0.AddHours (1)), TimeSpan.FromMinutes (30));
        var descending = dao.FindOverlapsRangeDescending (machineModule, average, new UtcDateTimeRange (P0.AddHours (-2), P0.AddHours (1)), TimeSpan.FromMinutes (30));
        Assert.Multiple (() => {
          Assert.That (next.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncValue, CncValueSource.CncDataPeriod }));
          Assert.That (nextAfter.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncDataPeriod }), "begin after the date/time only");
          Assert.That (lastOne.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncDataPeriod }));
          Assert.That (firstOne.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncValue }));
          Assert.That (ascending.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncValue, CncValueSource.CncDataPeriod }));
          Assert.That (descending.Select (v => v.Source), Is.EqualTo (new[] { CncValueSource.CncDataPeriod, CncValueSource.CncValue }));
        });
      }
    }

    static IField CreateField (string code, CncDataAggregationType aggregationType, FieldType type)
    {
      var field = ModelDAOHelper.ModelFactory.CreateFieldFromName (code, code);
      field.Type = type;
      field.CncDataAggregationType = aggregationType;
      field.CncDataStorage = CncDataStorage.CncDataPeriod;
      ModelDAOHelper.DAOFactory.FieldDAO.MakePersistent (field);
      return field;
    }

    static void AddPeriod (IMachineModule machineModule, IField field, Action<ICncDataPeriod> set) =>
      AddPeriod (machineModule, field, T0, set);

    static void AddPeriod (IMachineModule machineModule, IField field, DateTime begin, Action<ICncDataPeriod> set)
    {
      var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, begin);
      period.End = begin.AddMinutes (1);
      set (period);
      ModelDAOHelper.DAOFactory.CncDataPeriodDAO.MakePersistent (period);
    }

    static IList<Row> ReadView (IMachineModule machineModule, IField field) =>
      Read ($@"
SELECT * FROM public.cncvalueall
 WHERE machinemoduleid = {machineModule.Id} AND fieldid = {field.Id}
 ORDER BY cncvaluebegindatetime");

    static IList<Row> ReadRange (IMachineModule machineModule, IField field, DateTime from, DateTime to) =>
      Read ($@"
SELECT * FROM public.cncvaluerange ({machineModule.Id}, {field.Id},
  '{from:yyyy-MM-dd HH:mm:ss}'::timestamp, '{to:yyyy-MM-dd HH:mm:ss}'::timestamp)");

    static IList<Row> Read (string sql)
    {
      var rows = new List<Row> ();
      using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
        command.CommandText = sql;
        using (var reader = command.ExecuteReader ()) {
          while (reader.Read ()) {
            rows.Add (new Row (
              reader.GetInt64 (reader.GetOrdinal ("cncvalueid")),
              DateTime.SpecifyKind (reader.GetDateTime (reader.GetOrdinal ("cncvaluebegindatetime")), DateTimeKind.Utc),
              DateTime.SpecifyKind (reader.GetDateTime (reader.GetOrdinal ("cncvalueenddatetime")), DateTimeKind.Utc),
              Get<string> (reader, "cncvaluestring"),
              Get<int?> (reader, "cncvalueint"),
              Get<double?> (reader, "cncvaluedouble"),
              Get<double?> (reader, "cncvaluedeviation"),
              reader.GetBoolean (reader.GetOrdinal ("cncvaluestopped")),
              Get<string> (reader, "cncvaluesource"),
              Get<int?> (reader, "cncvaluecount"),
              Get<double?> (reader, "cncvaluemin"),
              Get<double?> (reader, "cncvaluemax")));
          }
        }
      }
      return rows;
    }

    static T Get<T> (IDataReader reader, string column)
    {
      var i = reader.GetOrdinal (column);
      return reader.IsDBNull (i) ? default : (T)reader.GetValue (i);
    }
  }
}
