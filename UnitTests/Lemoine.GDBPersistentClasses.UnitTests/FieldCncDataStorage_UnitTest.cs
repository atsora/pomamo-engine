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
  /// Unit tests for the storage of the cnc data of a field (columns fieldcncdatastorage and fieldperiodmaxduration)
  /// and for <see cref="CncDataAggregationTypeExtensions"/>
  /// </summary>
  [TestFixture]
  public class FieldCncDataStorage_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FieldCncDataStorage_UnitTest).FullName);

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
    /// Kind of the periods for each aggregation type, a new type is custom
    /// </summary>
    [Test]
    public void TestPeriodAggregationKind ()
    {
      Assert.Multiple (() => {
        Assert.That (((CncDataAggregationType?)null).GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.None));
        Assert.That (CncDataAggregationType.None.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.None));
        Assert.That (CncDataAggregationType.NewValue.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Change));
        Assert.That (CncDataAggregationType.Average.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Statistics));
        Assert.That (CncDataAggregationType.Sum.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Statistics));
        Assert.That (CncDataAggregationType.Max.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Statistics));
        Assert.That (((CncDataAggregationType)100).GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Custom));
      });
    }

    /// <summary>
    /// The new columns are stored, the default storage comes from the config
    /// </summary>
    [Test]
    public void TestStorage ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var field = ModelDAOHelper.ModelFactory.CreateFieldFromName ("TestCncDataStorage", "Test cnc data storage");
        field.Type = FieldType.Double;
        field.CncDataAggregationType = CncDataAggregationType.Average;
        field.CncDataStorage = CncDataStorage.CncValue | CncDataStorage.CncDataPeriod;
        field.PeriodMaxDuration = TimeSpan.FromMinutes (1);
        Assert.Throws<ArgumentOutOfRangeException> (() => field.PeriodMaxDuration = TimeSpan.Zero);
        daoFactory.FieldDAO.MakePersistent (field);
        var custom = ModelDAOHelper.ModelFactory.CreateFieldFromName ("TestCncDataStorageCustom", "Test cnc data storage custom");
        custom.Type = FieldType.Double;
        custom.CncDataAggregationType = (CncDataAggregationType)100;
        daoFactory.FieldDAO.MakePersistent (custom);
        daoFactory.FlushData ();
        NHibernateHelper.GetCurrentSession ().Evict (field);
        NHibernateHelper.GetCurrentSession ().Evict (custom);

        var reloaded = daoFactory.FieldDAO.FindById (field.Id);
        var reloadedCustom = daoFactory.FieldDAO.FindById (custom.Id);
        Assert.Multiple (() => {
          Assert.That (reloaded.CncDataStorage, Is.EqualTo (CncDataStorage.CncValue | CncDataStorage.CncDataPeriod));
          Assert.That (reloaded.GetCncDataStorage (), Is.EqualTo (CncDataStorage.CncValue | CncDataStorage.CncDataPeriod));
          Assert.That (reloaded.PeriodMaxDuration, Is.EqualTo (TimeSpan.FromMinutes (1)));
          Assert.That (reloaded.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Statistics));
          Assert.That (reloadedCustom.CncDataAggregationType, Is.EqualTo ((CncDataAggregationType)100), "new aggregation type without any migration");
          Assert.That (reloadedCustom.GetPeriodAggregationKind (), Is.EqualTo (CncDataPeriodAggregationKind.Custom));
          Assert.That (reloadedCustom.CncDataStorage, Is.Null);
          Assert.That (reloadedCustom.PeriodMaxDuration, Is.Null);
          Assert.That (reloadedCustom.GetCncDataStorage (), Is.EqualTo (CncDataStorage.CncValue), "default");
        });

        Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetCncConfigKey (CncConfigKey.DefaultDataStorage), CncDataStorage.CncDataPeriod);
        try {
          Assert.That (reloadedCustom.GetCncDataStorage (), Is.EqualTo (CncDataStorage.CncDataPeriod), "config");
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }
  }
}
