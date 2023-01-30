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
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class CncValueColor_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncValueColor_UnitTest).FullName);

    Pulse.Web.CncValue.CncValueColorService m_service;
    
    [Test]
    public void TestGetMainCncValueBadRequest()
    {
      var request = new CncValueColorRequestDTO ();
      request.MachineId = 1000; // non existing
      request.Range = "";
      ErrorDTO response = m_service.GetWithoutCache (request) as ErrorDTO;
      Assert.IsNotNull(response);
      
      request.MachineId = 1;
      request.Range = "blabla";
      Assert.Throws<FormatException> (
        new TestDelegate
        (delegate ()
         { m_service.GetWithoutCache(request); } ));
    }
    
    [Test]
    public void TestGetMainCncValueSuccess()
    {
      var request = new CncValueColorRequestDTO ();
      request.MachineId = 1;
      request.Range = "[2008-01-15T21:00:00Z,2008-01-16T21:00:00Z)";
      
      var response = m_service.GetWithoutCache (request) as CncValueColorResponseDTO;

      DateTime targetDay = new DateTime(2008,01,16);
      IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (targetDay);
      
      Assert.IsNotNull(response);
      Assert.AreEqual ("[2008-01-15T21:00:00Z,2008-01-16T21:00:00Z)", response.Range);
    }
    

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new CncValueColorService ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
