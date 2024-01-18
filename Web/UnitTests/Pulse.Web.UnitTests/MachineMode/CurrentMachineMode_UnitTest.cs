// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.ModelDAO;
using Pulse.Web.MachineMode;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.UnitTests.MachineMode
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class CurrentMachineMode_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentMachineMode_UnitTest).FullName);

    Pulse.Web.MachineMode.CurrentMachineModeService m_service;
    
    /* This is not possible to target a current machine mode for the moment in the unit tests
    [Test]
    public void TestCurrentMachineModeSuccess()
    {
      var request = new CurrentMachineModeRequestDTO ();
      request.MachineId = 1;
      var answer = m_service.GetWithoutCache (request) as CurrentMachineModeResponseDTO;
      
      Assert.IsNotNull(answer);
      Assert.AreEqual("#FFFF00", answer.MachineMode.Color);
    }
    */

    [Test]
    public void TestCurrentMachineModeNoDataSuccess()
    {
      var request = new CurrentMachineModeRequestDTO ();
      request.MachineId = 2;
      var answer = m_service.GetWithoutCache (request) as ErrorDTO;

      Assert.That (answer, Is.Not.Null);
    }

    [Test]
    public void TestCurrentMachineModeBadRequest()
    {
      var request = new CurrentMachineModeRequestDTO ();
      request.MachineId = 1000;
      var answer = m_service.GetWithoutCache (request) as ErrorDTO;

      Assert.That (answer, Is.Not.Null);
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new CurrentMachineModeService ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
