// Copyright (C) 2025 Atsora Solutions

using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
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
      // TODO: ...
    }
  }
}
