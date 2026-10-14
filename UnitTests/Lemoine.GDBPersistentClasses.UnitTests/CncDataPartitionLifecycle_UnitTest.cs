// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Life cycle of the cncdata partitions with the machine modules and the fields (migration 2005)
  ///
  /// Everything is made in a transaction that is rolled back
  /// </summary>
  [TestFixture]
  public class CncDataPartitionLifecycle_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPartitionLifecycle_UnitTest).FullName);

    static readonly DateTime T0 = new DateTime (2091, 05, 07, 10, 00, 00, DateTimeKind.Utc);

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

    /// <summary>
    /// A new machine module gets its partitions without locking the parent table in ACCESS EXCLUSIVE mode
    /// (the reads and the writes of the other machine modules are not blocked until the end of the transaction)
    /// </summary>
    [Test]
    public void TestNewMachineModuleLock ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var machineModule = CreateMachineModule ("TestCncDataPartitionNew");
          Assert.That (Exists ($"cncdataperiod_mm{machineModule.Id}"), Is.False);

          daoFactory.CncDataPartitionDAO.EnsurePartition (CncDataTable.Period, machineModule.Id, T0);

          Assert.Multiple (() => {
            Assert.That (Exists ($"cncdataperiod_mm{machineModule.Id}"), Is.True);
            Assert.That (ScalarLong ($@"
SELECT count (*) FROM public.cncdatapartitions
 WHERE tablename = 'cncdataperiod' AND machinemoduleid = {machineModule.Id}"), Is.EqualTo (1));
            Assert.That (ScalarString (@"
SELECT string_agg (l.mode, ',') FROM pg_catalog.pg_locks l
 WHERE l.pid = pg_backend_pid () AND l.relation = 'public.cncdataperiod'::regclass"),
              Does.Not.Contain ("AccessExclusiveLock"));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Reason of the migration 2005: CREATE TABLE ... PARTITION OF locks the parent table in ACCESS EXCLUSIVE mode,
    /// while a table prepared on its own then attached (ATTACH PARTITION) locks it only in SHARE UPDATE EXCLUSIVE mode
    /// </summary>
    [Test]
    public void TestPartitionLockModes ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          // Two unused machine module ids, on the existing tables (created before the transaction)
          var id = ScalarLong ("SELECT coalesce (max (machinemoduleid), 0) + 2000 FROM public.machinemodule");
          Execute ($"CREATE TABLE public.cncdatasample_testlock (LIKE public.cncdatasample INCLUDING ALL)");
          Execute ($"ALTER TABLE public.cncdatasample ATTACH PARTITION public.cncdatasample_testlock FOR VALUES IN ({id})");
          var attachModes = LockModes ("public.cncdatasample");
          Execute ($"CREATE TABLE public.cncdataperiod_testlock PARTITION OF public.cncdataperiod FOR VALUES IN ({id + 1})");
          var partitionOfModes = LockModes ("public.cncdataperiod");
          Assert.Multiple (() => {
            Assert.That (attachModes, Does.Contain ("ShareUpdateExclusiveLock"));
            Assert.That (attachModes, Does.Not.Contain ("AccessExclusiveLock"));
            Assert.That (partitionOfModes, Does.Contain ("AccessExclusiveLock"));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    static string LockModes (string relation) => ScalarString ($@"
SELECT string_agg (l.mode, ',') FROM pg_catalog.pg_locks l
 WHERE l.pid = pg_backend_pid () AND l.relation = '{relation}'::regclass") ?? "";

    /// <summary>
    /// Deleting a machine module drops its partitions (no row by row delete)
    /// </summary>
    [Test]
    public void TestDeleteMachineModule ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var machineModule = CreateMachineModule ("TestCncDataPartitionDelete");
          var field = CreateField ("TestCncDataPartitionDelete");
          var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, field, T0);
          period.Extend (T0.AddSeconds (10), CncDataStatistics.FromValue (1.0));
          daoFactory.CncDataPeriodDAO.MakePersistent (period);
          daoFactory.FlushData ();
          Assert.That (Exists ($"cncdataperiod_mm{machineModule.Id}"), Is.True);
          var machineModuleId = machineModule.Id;
          NHibernateHelper.GetCurrentSession ().Evict (period);

          Execute ($"DELETE FROM public.machinemodule WHERE machinemoduleid = {machineModuleId}");

          Assert.Multiple (() => {
            Assert.That (Exists ($"cncdataperiod_mm{machineModuleId}"), Is.False);
            Assert.That (ScalarLong ($"SELECT count (*) FROM public.cncdataperiod WHERE machinemoduleid = {machineModuleId}"), Is.EqualTo (0));
            Assert.That (ScalarLong ($"SELECT count (*) FROM public.cncdatapartitions WHERE machinemoduleid = {machineModuleId}"), Is.EqualTo (0));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A field without any cnc data can be deleted, a field with some cnc data can't (deactivate it instead)
    /// </summary>
    [Test]
    public void TestDeleteField ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var machineModule = CreateMachineModule ("TestCncDataPartitionField");
          var unused = CreateField ("TestCncDataPartitionUnused");
          var used = CreateField ("TestCncDataPartitionUsed");
          var period = ModelDAOHelper.ModelFactory.CreateCncDataPeriod (machineModule, used, T0);
          period.Extend (T0.AddSeconds (10), CncDataStatistics.FromValue (1.0));
          daoFactory.CncDataPeriodDAO.MakePersistent (period);
          daoFactory.FlushData ();
          var unusedId = unused.Id;
          var usedId = used.Id;

          Execute ($"DELETE FROM public.field WHERE fieldid = {unusedId}");
          Assert.That (ScalarLong ($"SELECT count (*) FROM public.field WHERE fieldid = {unusedId}"), Is.EqualTo (0));

          var ex = Assert.Catch (() => Execute ($"DELETE FROM public.field WHERE fieldid = {usedId}"));
          Assert.That (ex.ToString (), Does.Contain ("23503").Or.Contain ("foreign key"), "foreign key violation");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// The partitions of a machine module that does not exist any more are dropped by the maintenance
    /// </summary>
    [Test]
    public void TestDropOrphanPartitions ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var orphanId = ScalarLong ("SELECT coalesce (max (machinemoduleid), 0) + 1000 FROM public.machinemodule");
          Execute ($"SELECT public.cncdatacreatepartition ('cncdataperiod', {orphanId}, '{T0.ToString ("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}'::timestamp, 0)");
          Assert.That (Exists ($"cncdataperiod_mm{orphanId}"), Is.True);

          var count = daoFactory.CncDataPartitionDAO.DropOrphanPartitions (TimeSpan.FromMinutes (1));

          Assert.Multiple (() => {
            Assert.That (count, Is.GreaterThanOrEqualTo (1));
            Assert.That (Exists ($"cncdataperiod_mm{orphanId}"), Is.False);
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    static IMachineModule CreateMachineModule (string name)
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      machine.Name = name;
      machine.MonitoringType = daoFactory.MachineMonitoringTypeDAO.FindById (2); // Monitored
      daoFactory.MonitoredMachineDAO.MakePersistent (machine);
      var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, name);
      daoFactory.MachineModuleDAO.MakePersistent (machineModule);
      daoFactory.FlushData ();
      return machineModule;
    }

    static IField CreateField (string name)
    {
      var field = ModelDAOHelper.ModelFactory.CreateFieldFromName (name, name);
      field.Type = FieldType.Double;
      field.CncDataAggregationType = CncDataAggregationType.Average;
      field.CncDataStorage = CncDataStorage.CncDataPeriod;
      ModelDAOHelper.DAOFactory.FieldDAO.MakePersistent (field);
      ModelDAOHelper.DAOFactory.FlushData ();
      return field;
    }

    static bool Exists (string tableName) =>
      !string.IsNullOrEmpty (ScalarString ($"SELECT to_regclass ('public.{tableName}')::text"));

    static void Execute (string sql)
    {
      using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
        command.CommandText = sql;
        command.ExecuteNonQuery ();
      }
    }

    static long ScalarLong (string sql)
    {
      using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
        command.CommandText = sql;
        return Convert.ToInt64 (command.ExecuteScalar (), CultureInfo.InvariantCulture);
      }
    }

    static string ScalarString (string sql)
    {
      using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
        command.CommandText = sql;
        var result = command.ExecuteScalar ();
        return (result is null || result is DBNull) ? null : Convert.ToString (result, CultureInfo.InvariantCulture);
      }
    }
  }
}
