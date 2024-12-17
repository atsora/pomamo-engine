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
using System.Threading.Tasks;

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
    public async Task TestMachine ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("Jwt.Secret", System.Guid.NewGuid ().ToString ().Replace ("-", ""));

          var postDto = new UserPermissionsPostDto ();
          postDto.Login = "cyrille";
          postDto.Password = "abcdef";
//          postDto.Login = "lionel";
//          postDto.Password = "DatabasePassword";

          var response = (await m_service.PostAsync (postDto)) as UserPermissionsResponseDTO;

          Assert.That (response, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (response.Login, Is.EqualTo ("cyrille"));
            Assert.That (response.UserName, Is.EqualTo ("Cyrille Chepelov"));
            Assert.That (response.UserDisplay, Is.EqualTo ("cyrille (Cyrille Chepelov)"));
            Assert.That (response.Role, Is.EqualTo (""));
            Assert.That (response.CompanyId, Is.EqualTo (-1));
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
