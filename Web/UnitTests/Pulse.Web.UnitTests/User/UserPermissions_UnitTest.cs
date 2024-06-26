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
using Pulse.Web.User;

namespace Pulse.Web.UnitTests.User
{
  /// <summary>
  /// 
  /// </summary>
  public class UserPermissions_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (UserPermissions_UnitTest).FullName);

    Pulse.Web.User.UserPermissionsService m_service;

    /// <summary>
    /// Constructor
    /// </summary>
    public UserPermissions_UnitTest ()
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
          Lemoine.Info.ConfigSet.ForceValue ("Jwt.Secret", System.Guid.NewGuid ().ToString ().Replace ("-", ""));

          var postDto = new UserPermissionsPostDto ();
          postDto.Login = "lionel";
          postDto.Password = "DatabasePassword";

          var response = System.Threading.Tasks.Task.Run ( () => m_service.PostAsync (postDto)).Result as UserPermissionsResponseDTO;

          Assert.That (response, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (response.Login, Is.EqualTo ("lionel"));
            Assert.That (response.UserName, Is.EqualTo ("Lionel"));
            Assert.That (response.UserDisplay, Is.EqualTo ("lionel (Lionel)"));
            Assert.That (response.Role, Is.EqualTo ("Operator"));
            Assert.That (response.CompanyId, Is.EqualTo (2));
          });
        }
        finally {
          transaction.Rollback ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_service = new Pulse.Web.User.UserPermissionsService ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
    }
  }
}
