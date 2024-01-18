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

      Assert.That (response, Is.Not.Null);
      Assert.That (response.Items, Has.Count.EqualTo (1));
      Assert.Multiple (() => {
        Assert.That (response.Items[0].Field.Id, Is.EqualTo (100));
        Assert.That (response.Items[0].Legends, Has.Count.EqualTo (4));
      });
      var fl1 = response.Items[0].Legends[0];
      var fl4 = response.Items[0].Legends[3];
      Assert.Multiple (() => {
        Assert.That (fl1.Display, Is.EqualTo ("< 1000"), "First field legend label");
        Assert.That (fl1.Color, Is.EqualTo ("#0080FF"), "First field legend color");
        Assert.That (fl4.Display, Is.EqualTo ("> 6000"), "Fourth field legend label");
        Assert.That (fl4.Color, Is.EqualTo ("#800080"), "Fourth field legend color");
      });
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

      Assert.That (response, Is.Not.Null);
      Assert.That (response.Items, Is.Empty);
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
