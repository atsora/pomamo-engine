// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.ModelDAO;
using Pulse.Web.Reason;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Pulse.Web.UnitTests.Reason
{
  /// <summary>
  /// Unit tests for the class ReasonSelectionService
  /// </summary>
  [TestFixture]
  public class ReasonSelection_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSelection_UnitTest).FullName);

    Pulse.Web.Reason.ReasonSelectionService m_service;
    
    [Test]
    public void TestReasonSelection()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";
          
          var response = m_service.GetWithoutCache (request) as IList<ReasonSelectionResponseDTO>;
          
          Assert.IsNotNull(response);
          Assert.AreEqual (9, response.Count);
          
          var reason1 = response.First (item => (16 == item.Id));
          Assert.AreEqual (16, reason1.Id);
          Assert.AreEqual (17, reason1.ReasonGroupId);
          Assert.AreEqual ("Mounted new workpiece", reason1.ReasonGroupDisplay);
        }
        finally {
          transaction.Rollback();
        }
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new ReasonSelectionService ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
