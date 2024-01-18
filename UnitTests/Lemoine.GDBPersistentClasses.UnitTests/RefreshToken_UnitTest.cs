// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class RefreshToken_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (RefreshToken_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public RefreshToken_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestAsyncMethods ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var user = ModelDAOHelper.ModelFactory.CreateUser ("test", "test");
          user.Name = "test";
          System.Threading.Tasks.Task.Run (() => ModelDAOHelper.DAOFactory.UserDAO.MakePersistentAsync (user)).Wait ();
          var refreshToken = ModelDAOHelper.ModelFactory.CreateRefreshToken (user, TimeSpan.FromSeconds (30));
          System.Threading.Tasks.Task.Run (() => ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakePersistentAsync (refreshToken)).Wait ();
          Assert.That (0 != refreshToken.Id, Is.True);
          ModelDAOHelper.DAOFactory.Flush ();
          System.Threading.Tasks.Task.Run (() => ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakeTransientAsync (refreshToken)).Wait ();
          ModelDAOHelper.DAOFactory.Flush ();
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
    public void TestGetRefreshToken ()
    {
      System.Threading.Tasks.Task.Run (TestGetRefreshTokenAsync).Wait ();
    }

    async System.Threading.Tasks.Task TestGetRefreshTokenAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue ("RefreshToken.CleanAge", TimeSpan.FromSeconds (0));

          var user = ModelDAOHelper.ModelFactory.CreateUser ("test", "test");
          user.Name = "test";
          await ModelDAOHelper.DAOFactory.UserDAO.MakePersistentAsync (user);
          var refreshToken = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, true);
          ModelDAOHelper.DAOFactory.Flush ();
          Assert.IsNotNull (refreshToken);
          Assert.That (refreshToken.Id, Is.Not.EqualTo (0));
          var refreshToken2 = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, true);
          ModelDAOHelper.DAOFactory.Flush ();
          Assert.IsNotNull (refreshToken2);
          Assert.That (refreshToken2, Is.EqualTo (refreshToken));
          refreshToken.Revoked = DateTime.UtcNow;
          await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakePersistentAsync (refreshToken);
          ModelDAOHelper.DAOFactory.Flush ();
          var refreshToken3 = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, false);
          ModelDAOHelper.DAOFactory.Flush ();
          var allTokens = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.FindAllAsync ();
          Assert.That (allTokens, Has.Count.EqualTo (2));
          refreshToken3.Revoked = DateTime.UtcNow;
          var refreshToken4 = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, false);
          ModelDAOHelper.DAOFactory.Flush ();
          allTokens = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.FindAllAsync ();
          Assert.That (allTokens, Has.Count.EqualTo (3));
          var refreshToken5 = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, true);
          ModelDAOHelper.DAOFactory.Flush ();
          Assert.That (refreshToken5, Is.EqualTo (refreshToken4));
          allTokens = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.FindAllAsync ();
          Assert.That (allTokens, Has.Count.EqualTo (1), "Last number of tokens");
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }
  }
}
