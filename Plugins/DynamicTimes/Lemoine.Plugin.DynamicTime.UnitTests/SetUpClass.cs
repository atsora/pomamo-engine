// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.GDBPersistentClasses;
using NUnit.Framework;
using Lemoine.Core.Log;
using System.Reflection;
using Lemoine.GDBMigration;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// SetUpClass:
  /// <item>make log4net work with nunit</item>
  /// <item>Active the DaySlot cache</item>
  /// <item>Set the right DSNName</item>
  /// </summary>
  [SetUpFixture]
  public class SetUpClass
  {
    string m_previousDSNName;

    [OneTimeSetUp]
    public void SetUpInitialize ()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      // Take care the log4net.config file is added to the deployment files of the testconfig
      var log4netConfigurationPath = System.IO.Path.Combine (TestContext.CurrentContext.TestDirectory, "pulselog.log4net");
      LogManager.ClearLoggerFactories ();
      LogManager.AddLog4net (log4netConfigurationPath);

      // Activate DaySlotCache
      DaySlotCache.Activate ();

      // Set the right DSNName
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");

      Lemoine.Info.ConfigSet.ForceValue ("Extensions.AlternativePluginsDirectories", TestContext.CurrentContext.TestDirectory);

      // Select the right implementation of ModelDAO
      var migrationHelper = new MigrationHelper ();
      var connectionInitializer = new Pulse.Database.ConnectionInitializer.ConnectionInitializer ("Lemoine.Plugin.DynamicTime.UnitTests", migrationHelper);
      connectionInitializer.CreateAndInitializeConnection ();

      Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));
    }

    [OneTimeTearDown]
    public void SetUpTearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      Lemoine.Info.ConfigSet.ResetForceValues ();

      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }

}
