// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.Reason;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulse.Web.UnitTests.Reason
{
  /// <summary>
  /// Unit tests for the class ReasonSelectionService
  /// </summary>
  [TestFixture]
  public class ReasonSelection_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelection_UnitTest).FullName);

    Pulse.Web.Reason.ReasonSelectionService m_service;

    [Test]
    public async Task TestReasonSelectionAsync ()
    {
      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new ReasonSelectionRequestDTO ();
          request.MachineId = 1;
          request.Range = "[2011-11-22T08:30:00Z,2011-11-22T08:30:00Z]";

          var response = await m_service.Get (request) as IList<ReasonSelectionResponseDTO>;

          Assert.That (response, Is.Not.Null);
          Assert.That (response, Has.Count.EqualTo (9));

          var reason1 = response.First (item => (16 == item.Id));
          Assert.Multiple (() => {
            Assert.That (reason1.Id, Is.EqualTo (16));
            Assert.That (reason1.ReasonGroupId, Is.EqualTo (17));
            Assert.That (reason1.ReasonGroupDisplay, Is.EqualTo ("Mounted new workpiece"));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_service = new ReasonSelectionService ();

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
