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
    /// Cnc data parameters (column fieldcncdataparameters, migration 2006): stored, read with a type (case insensitive),
    /// an invalid json or a json that is not an object is rejected, JsonType to deserialize the json values
    /// </summary>
    [Test]
    public void TestCncDataParameters ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var session = daoFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var field = ModelDAOHelper.ModelFactory.CreateFieldFromName ("TestCncDataParameters", "Test cnc data parameters");
          field.Type = FieldType.String;
          Assert.Throws<ArgumentException> (() => field.CncDataParameters = "{ not json");
          Assert.Throws<ArgumentException> (() => field.CncDataParameters = "[1, 2]", "not an object");
          field.CncDataParameters = @"{ ""MaxGap"": ""00:05:00"", ""Factor"": 1.5,
  ""JsonType"": ""System.Collections.Generic.Dictionary`2[[System.String],[System.Double]]"" }";
          daoFactory.FieldDAO.MakePersistent (field);
          daoFactory.FlushData ();
          NHibernateHelper.GetCurrentSession ().Evict (field);

          var reloaded = daoFactory.FieldDAO.FindById (field.Id);
          Assert.Multiple (() => {
            Assert.That (reloaded.CncDataParameters, Is.Not.Null);
            Assert.That (reloaded.GetCncDataParameter<TimeSpan?> (CncDataParameterNames.MaxGap), Is.EqualTo (TimeSpan.FromMinutes (5)));
            Assert.That (reloaded.GetCncDataParameter<double> ("factor"), Is.EqualTo (1.5), "case insensitive");
            Assert.That (reloaded.GetCncDataParameter ("Missing", 7), Is.EqualTo (7), "default value");
            Assert.That (reloaded.GetCncDataParameter ("Factor", "x"), Is.EqualTo ("x"), "invalid type: default value");
            Assert.That (reloaded.GetJsonType (), Is.EqualTo (typeof (System.Collections.Generic.Dictionary<string, double>)));
            var value = reloaded.DeserializeJsonValue (@"{ ""a"": 1.5 }");
            Assert.That (value, Is.InstanceOf<System.Collections.Generic.Dictionary<string, double>> ());
            Assert.That (((System.Collections.Generic.Dictionary<string, double>)value)["a"], Is.EqualTo (1.5));
          });

          reloaded.CncDataParameters = null;
          Assert.Multiple (() => {
            Assert.That (reloaded.GetJsonType (), Is.Null);
            Assert.That (reloaded.DeserializeJsonValue (@"{ ""a"": 1 }"), Is.InstanceOf<System.Text.Json.JsonElement> (), "no JsonType");
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
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
        field.CncDataStorage = CncDataStorage.CncDataPeriod;
        field.PeriodMaxDuration = TimeSpan.FromMinutes (1);
        Assert.Throws<ArgumentOutOfRangeException> (() => field.PeriodMaxDuration = TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException> (() => field.CncDataStorage = (CncDataStorage)3, "no double storage");
        Assert.That (field.CncDataStorage, Is.EqualTo (CncDataStorage.CncDataPeriod));
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
          Assert.That (reloaded.CncDataStorage, Is.EqualTo (CncDataStorage.CncDataPeriod));
          Assert.That (reloaded.GetCncDataStorage (), Is.EqualTo (CncDataStorage.CncDataPeriod));
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
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetCncConfigKey (CncConfigKey.DefaultDataStorage), (CncDataStorage)3);
          Assert.That (reloadedCustom.GetCncDataStorage (), Is.EqualTo (CncDataStorage.CncValue), "invalid config");
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }
  }
}
