// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class IntermediateWorkPiece
  /// </summary>
  [TestFixture]
  public class IntermediateWorkPiece_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (IntermediateWorkPiece_UnitTest).FullName);

    /// <summary>
    /// Test the insertion of a new intermediate work piece
    /// </summary>
    [Test]
    public void TestInsert()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        IIntermediateWorkPiece intermediateWorkPiece = new IntermediateWorkPiece (session.Get<Operation> (1));
        intermediateWorkPiece.Name = "TestInsertIntermediateWorkPiece";
        session.Save (intermediateWorkPiece);
        session.Flush ();
        session.CreateQuery ("delete from IntermediateWorkPiece foo " +
                             "where foo.Name=?")
          .SetParameter (0, "TestInsertIntermediateWorkPiece")
          .ExecuteUpdate ();
        session.Flush ();
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
