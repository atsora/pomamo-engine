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
    public void TestReasonSave()
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
          
          var response = new ReasonSaveService ().GetSync (reasonSave) as ReasonSaveResponseDTO;
              
          Assert.IsNotNull(response);
          
          // just test there is a pending modification of the right type
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.IsNotNull(modifications, "no modification");
          Assert.AreEqual(1, modifications.Count, "not 1 modification");
          IModification modification = modifications[0];
          IReasonMachineAssociation reasonMachineAssociation = modification as IReasonMachineAssociation;
          Assert.IsNotNull(reasonMachineAssociation, "not a reason machine association");
          Assert.AreEqual(details, reasonMachineAssociation.ReasonDetails, "bad details");
          Assert.AreEqual(reason1.Id, reasonMachineAssociation.Reason.Id);
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
