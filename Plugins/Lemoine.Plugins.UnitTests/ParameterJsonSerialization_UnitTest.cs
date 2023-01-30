// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class ParameterJsonSerialization_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (ParameterJsonSerialization_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ParameterJsonSerialization_UnitTest ()
    { }

    /// <summary>
    /// Deserialize
    /// </summary>
    [Test]
    public void TestDeserialize ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);

          {
            var extension = new Lemoine.Plugin.ShortPeriodRemoval
              .ActivityAnalysisExtension ();
            extension.SetTestConfiguration ($@"
{{
  ""MaxDuration"": ""0:00:10"",
  ""OldMachineModeId"": 1
}}
");
            extension.Initialize (machine);
            Assert.AreEqual (3, extension.NewMachineModeFilter.Id);
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

  }
}
