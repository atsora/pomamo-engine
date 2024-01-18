// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class DeliverablePiece_UnitTest
  {
    string previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (DeliverablePiece_UnitTest).FullName);

    /// <summary>
    /// TODO: Documentation of the test TestMethod
    /// </summary>
    [Test]
    public void TestDeliverablePiece ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        // create deliverable piece 1
        IComponent component1 = session.Get<Component> (1);
        IComponent component2 = session.Get<Component> (2);
        IDeliverablePiece deliverablePiece1 = new DeliverablePiece ();
        deliverablePiece1.Code = "UX738";
        deliverablePiece1.Component = component1;

        session.Save (deliverablePiece1);

        // get saved deliverable piece by ID and check equality
        IDeliverablePiece deliverablePiece2 =
          session.Get<DeliverablePiece> (deliverablePiece1.Id);

        Assert.That (deliverablePiece2, Is.EqualTo (deliverablePiece1));

        // change component of deliverable piece 1 to component2
        deliverablePiece1.Component = component2;

        session.SaveOrUpdate (deliverablePiece1);

        // fetch deliverable pieces with component equal to component2
        IList<IDeliverablePiece> deliverablePieceList1 =
          session.CreateCriteria<DeliverablePiece> ()
          .Add (Restrictions.Eq ("Component", component2))
          .List<IDeliverablePiece> ();

        // check deliverable piece 1 is in fetched list
        Assert.That (deliverablePieceList1.Contains (deliverablePiece1), Is.True);

        // fetch deliverable pieces with component equal to component2
        IList<IDeliverablePiece> deliverablePieceList2 =
          session.CreateCriteria<DeliverablePiece> ()
          .Add (Restrictions.Eq ("Component", component1))
          .List<IDeliverablePiece> ();

        // check deliverable piece 1 is not in fetched list
        Assert.That (deliverablePieceList2.Contains (deliverablePiece1), Is.False);

        transaction.Rollback ();

      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }

  }
}
