// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.GDBPersistentClasses;
using NUnit.Framework;
using Lemoine.Core.Log;
using System.Reflection;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// SetUp:
  /// <item>make log4net work with nunit</item>
  /// <item>Active the DaySlot cache</item>
  /// <item>Set the right DSNName</item>
  /// </summary>
  [SetUpFixture]
  public class SetUp
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

      // Select the right implementation of ModelDAO
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void SetUpTearDown ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }

}
