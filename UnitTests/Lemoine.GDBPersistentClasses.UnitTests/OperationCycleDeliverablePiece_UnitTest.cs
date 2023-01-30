// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;
using NUnit.Framework;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class OperationCycleDeliverablePiece_UnitTest
  {
    string previousDSNName;
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleDeliverablePiece_UnitTest).FullName);

    /// <summary>
    /// TODO: Documentation of the test TestMethod
    /// </summary>
    [Test]
    public void TestOperationCycleDeliverablePiece()
    {
      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = daoSession.BeginTransaction ())
        {
          ISession session = NHibernateHelper.GetCurrentSession();
          // create deliverable piece 1
          IComponent component1 = daoFactory.ComponentDAO.FindById (1);
          IDeliverablePiece deliverablePiece1 = new DeliverablePiece();
          deliverablePiece1.Code = "UX738";
          deliverablePiece1.Component = component1;
          
          daoFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece1);
          
          // create deliverable piece 2
          IDeliverablePiece deliverablePiece2 = new DeliverablePiece();
          deliverablePiece2.Code = "UX739";
          deliverablePiece2.Component = component1;
          
          daoFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece2);
          
          // create operation cycle
          IOperation operation1 = daoFactory.OperationDAO.FindById (1);
          IMonitoredMachine monitoredMachine1 = daoFactory.MonitoredMachineDAO.FindById(1);
          
          IOperationCycle operationCycle1 =
            ModelDAOHelper.ModelFactory
            .CreateOperationCycle (monitoredMachine1);
          operationCycle1.SetRealEnd (new DateTime (2013, 01, 01));
          // operationCycle1.OperationSlot = operationSlot;
          
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle1);
          
          IOperationCycleDeliverablePiece operationCycleDeliverablePiece1 =
            ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece1, operationCycle1);

          IOperationCycleDeliverablePiece operationCycleDeliverablePiece2 =
            ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece2, operationCycle1);
          
          daoFactory.OperationCycleDeliverablePieceDAO.MakePersistent(operationCycleDeliverablePiece1);
          daoFactory.OperationCycleDeliverablePieceDAO.MakePersistent(operationCycleDeliverablePiece2);
          
          
          // fetch deliverable pieces associated with operationCycle1
          // and check both deliverablepiece1 and deliverablepiece2 are returned
          // and none other
          IList<OperationCycleDeliverablePiece> opCycledeliverablePieceList1 =
            session.CreateCriteria<OperationCycleDeliverablePiece>()
            .Add (Restrictions.Eq ("OperationCycle", operationCycle1))
            .List<OperationCycleDeliverablePiece> ();

          bool foundDeliverablePiece1 = false;
          bool foundDeliverablePiece2 = false;
          bool foundOther = false;
          
          foreach(IOperationCycleDeliverablePiece opCyclePiece in opCycledeliverablePieceList1) {
            if (opCyclePiece.DeliverablePiece.Equals(deliverablePiece1)) {
              foundDeliverablePiece1 = true;
            }
            else if (opCyclePiece.DeliverablePiece.Equals(deliverablePiece2)) {
              foundDeliverablePiece2 = true;
            }
            else {
              foundOther = true;
            }
          }
          
          Assert.IsTrue(foundDeliverablePiece1 && foundDeliverablePiece2 && !foundOther);
          
          transaction.Rollback();

        }
      } catch(Exception /*ex*/) {
        Assert.IsTrue(false);
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
