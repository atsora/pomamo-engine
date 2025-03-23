// Copyright (C) 2025 Atsora Solutions

using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Business.Reason;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Business.UnitTests.Reason
{
  [TestFixture]
  class ReasonData_UnitTest
  {
    /// <summary>
    /// Test deserialization
    /// </summary>
    [Test]
    public void TestDeserialization ()
    {
      var json = """
        {
          "mwo": {
            "Number": 36095,
            "Description": "Laser Calibration.- Mon. Num. 236"
          }
        }
        """;
      Assert.That (json, Is.Not.Null);
      // TODO: Test deserialization
    }

    [Test]
    public void TestDisplay ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<Lemoine.Plugin.TestReasonData.ReasonDataExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var d = ReasonData.OverwriteDisplay ("Original", """{"Test": 123.4}""", false);
          Assert.That (d, Is.EqualTo ("Float:123.4"));
          var l = ReasonData.OverwriteDisplay ("Original", """{"Test": 123.4}""", true);
          Assert.That (l, Is.EqualTo ("Original Float:123.4"));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

  }
}
