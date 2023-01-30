// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.Modification;
using Pulse.Web.Reason;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.UnitTests.Modification
{
  /// <summary>
  /// Unit tests for the class PendingModificationsService
  /// </summary>
  [TestFixture]
  public class PendingModificationsService_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PendingModificationsService_UnitTest).FullName);

    Pulse.Web.Modification.PendingModificationsService m_service;
    
    [Test]
    public void TestPendingModifications()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          // first save a reason
          DateTime begin = DateTime.Parse("2013-09-06 14:59:00");
          DateTime end = DateTime.Parse("2013-09-06 15:00:00");
          IReason reason1 = ModelDAOHelper.DAOFactory.ReasonDAO.FindById(1);
          IMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(1);
          const String details = "Details for reason";
          
          ReasonSaveRequestDTO reasonSave = new ReasonSaveRequestDTO ();
          reasonSave.MachineId = machine1.Id;
          reasonSave.Range = "[" + ConvertDTO.DateTimeUtcToIsoString (begin) + "," + ConvertDTO.DateTimeUtcToIsoString (end) + ")";
          reasonSave.ReasonDetails = details;
          ReasonSaveResponseDTO response = new ReasonSaveService ().GetSync (reasonSave) as ReasonSaveResponseDTO;   
          Assert.IsNotNull(response, "Non OK saveReasonResponse");
          
          int revisionId = (int)response.Revision.Id;
          
          // actual test (first phase)
          {
            var request = new PendingModificationsRequestDTO ();
            request.RevisionId = revisionId;
            PendingModificationsResponseDTO pendingModifications =
              m_service.GetWithoutCache (request) as PendingModificationsResponseDTO;
            Assert.IsNotNull(pendingModifications, "Null number of pending modifications");
            Assert.AreEqual(1, pendingModifications.Number, "Not 1 pending modification");
          }
          
          // save reason a second time
          ReasonSaveResponseDTO response2 = new ReasonSaveService ().GetSync (reasonSave) as ReasonSaveResponseDTO;
          Assert.IsNotNull(response2, "Non OK saveReasonResponse");
          
          int revisionId2 = (int)response2.Revision.Id;
          {
            var request = new PendingModificationsRequestDTO ();
            request.RevisionId = revisionId2;
            PendingModificationsResponseDTO pendingModifications =
              m_service.GetWithoutCache (request) as PendingModificationsResponseDTO;
            Assert.IsNotNull(pendingModifications);
            Assert.AreEqual(2, pendingModifications.Number);
          }          
        }
        finally {
          transaction.Rollback();
        }
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new PendingModificationsService ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
