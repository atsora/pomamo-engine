// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.ModelDAO;
using Pulse.Web.CncValue;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Pulse.Web.UnitTests.CncValue
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class CncValueLegend_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncValueLegend_UnitTest).FullName);

    Pulse.Web.CncValue.CncValueLegendService m_service;
    
    /// <summary>
    /// Test GetFieldLegends service of RTDService (success)
    /// </summary>
    [Test]
    public void TestGetFieldLegendsSuccess()
    {
      var request = new CncValueLegendRequestDTO ();
      request.MachineIds = new List<int> ();
      request.MachineIds.Add (1);
      request.MachineIds.Add (2);
      
      var response = m_service.GetWithoutCache (request) as CncValueLegendResponseDTO;
      
      Assert.IsNotNull (response);
      Assert.AreEqual (1, response.Items.Count);
      Assert.AreEqual (100, response.Items[0].Field.Id);
      Assert.AreEqual (4, response.Items[0].Legends.Count);
      var fl1 = response.Items[0].Legends[0];
      var fl4 = response.Items[0].Legends[3];
      Assert.AreEqual("< 1000", fl1.Display, "First field legend label");
      Assert.AreEqual("#0080FF", fl1.Color, "First field legend color");
      Assert.AreEqual("> 6000", fl4.Display, "Fourth field legend label");
      Assert.AreEqual("#800080", fl4.Color, "Fourth field legend color");      
    }
    
    /// <summary>
    /// Test GetFieldLegends service of RTDService (failure: non existing field)
    /// </summary>
    [Test]
    public void TestGetFieldLegendsFailure()
    {
      var request = new CncValueLegendRequestDTO ();
      request.MachineIds = new List<int> ();
      request.MachineIds.Add (100);
      
      var response = m_service.GetWithoutCache (request) as CncValueLegendResponseDTO;
      
      Assert.IsNotNull (response);
      Assert.AreEqual (0, response.Items.Count);
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new CncValueLegendService ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
