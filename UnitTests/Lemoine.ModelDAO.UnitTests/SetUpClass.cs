// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using NUnit.Framework;
using Lemoine.Core.Log;
using System.Reflection;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// make log4net work with nunit
  /// </summary>
  [SetUpFixture]
  public class SetUpClass {
    [OneTimeSetUp]
    public void SetUpInitialize()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());

      // Take care the log4net.config file is added to the deployment files of the testconfig
      var log4netConfigurationPath = System.IO.Path.Combine (TestContext.CurrentContext.TestDirectory, "pulselog.log4net");
      LogManager.ClearLoggerFactories ();
      LogManager.AddLog4net (log4netConfigurationPath);
    }

    [OneTimeTearDown]
    public void SetUpTearDown() {}
  }

}
