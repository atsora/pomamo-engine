// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Web.Machine;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.UnitTests.Machine
{
  /// <summary>
  /// 
  /// </summary>
  public class NameService_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (NameService_UnitTest).FullName);

    Pulse.Web.Machine.NameService m_service;

    /// <summary>
    /// Constructor
    /// </summary>
    public NameService_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestMachine ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new NameRequestDTO ();
          request.MachineId = 1;

          var response = m_service.GetWithoutCache (request) as NameResponseDTO;

          Assert.IsNotNull (response);
          Assert.AreEqual ("1", response.Id);
          Assert.AreEqual ("MACHINE_A17", response.Name);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestCompatibility ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new NameRequestDTO ();
          request.Id = 1;

          var response = m_service.GetWithoutCache (request) as NameResponseDTO;

          Assert.IsNotNull (response);
          Assert.AreEqual ("1", response.Id);
          Assert.AreEqual ("MACHINE_A17", response.Name);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestBadRequest ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var request = new NameRequestDTO ();
          request.Id = 1000;

          var response = m_service.GetWithoutCache (request) as ErrorDTO;

          Assert.IsNotNull (response);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_service = new Pulse.Web.Machine.NameService ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
    }
  }
}
