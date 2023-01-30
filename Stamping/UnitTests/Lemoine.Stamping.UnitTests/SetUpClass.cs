// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.I18N;
using System.Reflection;
using Lemoine.ModelDAO.Interfaces;
using Pulse.Extensions;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions;
using Lemoine.Extensions.Plugin;
using Pulse.Database.ConnectionInitializer;
using Lemoine.Extensions.PluginFilter;
using Lemoine.GDBMigration;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lemoine.Stamping.UnitTests
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

      // Set the right DSNName
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");

      // Select the right implementation of ModelDAO
      // Use a direct connection to the database
      var migrationHelper = new MigrationHelper ();
      var connectionInitializer = new ConnectionInitializer ("Lemoine.Stamping.UnitTests", migrationHelper);
      connectionInitializer.Initialize ();

      Lemoine.Info.ConfigSet.ResetConfigReader ();
      Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader ());
    }

    [OneTimeTearDown]
    public void SetUpTearDown ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }

      Lemoine.Info.ConfigSet.ResetConfigReader ();
    }
  }

}
