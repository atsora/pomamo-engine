// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.CncValue;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.UnitTests.CncValue
{
  /// <summary>
  /// Unit tests for the class CncValueCurrentService
  /// </summary>
  [TestFixture]
  public class CncValueCurrent_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueCurrent_UnitTest).FullName);

    Pulse.Web.CncValue.CncValueCurrentService m_service;

    [Test]
    public void TestWrongMachineIdRequest ()
    {
      var request = new CncValueCurrentRequestDTO ();
      request.MachineId = 1000; // non existing
      ErrorDTO response = m_service.GetWithoutCache (request) as ErrorDTO;
      Assert.That (response, Is.Not.Null);
    }


    /// <summary>
    /// Test with the main performance field only
    /// </summary>
    [Test]
    public void TestMainPerformanceFieldSuccess ()
    {
      var request = new CncValueCurrentRequestDTO ();
      request.MachineId = 1;

      var response = m_service.GetWithoutCache (request) as CncValueCurrentResponseDTO;

      Assert.That (response, Is.Not.Null, "/CncValue/Current MachineId=1 returns null");
      Assert.Multiple (() => {
        Assert.That (response.ByMachineModule[0].ByField[0].Field.Display, Is.EqualTo ("Feedrate"));
        // TODO: test unit
        /* Assert.AreEqual ("mm/min", ...); */
        Assert.That (response.ByMachineModule[0].ByField[0].Value, Is.EqualTo (95.5));
        Assert.That (response.ByMachineModule[0].ByField[0].DateTime, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (new DateTime (2008, 1, 16, 10, 0, 0, DateTimeKind.Utc))));
      });
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_service = new CncValueCurrentService ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
    }
  }
}
