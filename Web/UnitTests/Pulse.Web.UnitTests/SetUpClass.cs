// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Reflection;

namespace Pulse.Web.UnitTests
{
  /// <summary>
  /// make log4net work with nunit
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
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();

      Lemoine.Info.ConfigSet.AddConfigReader (new Lemoine.ModelDAO.Info.ModelDAOConfigReader (true));

      // In option: activate DaySlotCache
      //Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
    }

    [OneTimeTearDown]
    public void SetUpTearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }

}
