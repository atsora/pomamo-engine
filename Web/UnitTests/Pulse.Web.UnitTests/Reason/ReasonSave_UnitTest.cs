// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.Reason;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.Log;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Pulse.Web.UnitTests.Reason
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class ReasonSave_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSave_UnitTest).FullName);

    Pulse.Web.Reason.ReasonSaveService m_service;
    
    [Test]
    public async Task TestReasonSave()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          const string details = "my details";
          IReason reason1 = ModelDAOHelper.DAOFactory.ReasonDAO.FindById(1);

          ReasonSaveRequestDTO reasonSave = new ReasonSaveRequestDTO ();
          reasonSave.MachineId = 1;
          reasonSave.ReasonId = reason1.Id;
          reasonSave.Range = "[2013-09-06T14:59:00Z,2013-09-06T15:00:00Z)";
          reasonSave.ReasonDetails = details;
          
          var response = await new ReasonSaveService ().Get (reasonSave) as ReasonSaveResponseDTO;

          Assert.That (response, Is.Not.Null);
          
          // just test there is a pending modification of the right type
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.That (modifications, Is.Not.Null, "no modification");
          Assert.That (modifications, Has.Count.EqualTo (1), "not 1 modification");
          IModification modification = modifications[0];
          IReasonMachineAssociation reasonMachineAssociation = modification as IReasonMachineAssociation;
          Assert.That (reasonMachineAssociation, Is.Not.Null, "not a reason machine association");
          Assert.Multiple (() => {
            Assert.That (reasonMachineAssociation.ReasonDetails, Is.EqualTo (details), "bad details");
            Assert.That (reasonMachineAssociation.Reason.Id, Is.EqualTo (reason1.Id));
          });
        }
        finally {
          transaction.Rollback();
        }
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_service = new ReasonSaveService ();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
