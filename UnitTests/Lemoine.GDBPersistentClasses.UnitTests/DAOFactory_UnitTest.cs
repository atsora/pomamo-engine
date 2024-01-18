// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DAOFactory
  /// </summary>
  [TestFixture]
  public class DAOFactory_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DAOFactory_UnitTest).FullName);

    /// <summary>
    /// Test the IsReadOnlyTransaction method
    /// </summary>
    [Test]
    public void TestIsReadOnlyTransaction()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ())
        {
          Assert.That (ModelDAOHelper.DAOFactory.IsTransactionReadOnly (), Is.True);
        }

        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          Assert.That (ModelDAOHelper.DAOFactory.IsTransactionReadOnly (), Is.False);
          transaction.Commit ();
        }
      }
    }

    [Test]
    public void TestConnectionString()
    {
      string connectionString = Lemoine.Info.GDBConnectionParameters.GetGDBConnectionString ("App");
      Assert.That (string.IsNullOrEmpty (connectionString), Is.False);
    }
    
    [Test]
    public void TestGetPostgreSQLVersionDirect ()
    {
      var  connectionString = Lemoine.Info.GDBConnectionParameters.GetGDBConnectionString ("Test");

      string serverVersion;
      using (var connection = new Npgsql.NpgsqlConnection (connectionString)) {
        connection.Open ();
        using (var command = connection.CreateCommand ()) {
          command.CommandText = "SHOW server_version;";
          serverVersion = (string)command.ExecuteScalar ();
        }
      }
      Assert.That (string.IsNullOrEmpty (serverVersion), Is.False);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
