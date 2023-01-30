// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the persistent class WorkOrderLine.
  /// </summary>
  [TestFixture]
  public class WorkOrderLine_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLine_UnitTest).FullName);
    
    #region Setup and dispose
    string previousDSNName;
    
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
    #endregion // Setup and dispose
    
    /// <summary>
    /// Test for insertion and deletion
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ILineDAO lineDAO = daoFactory.LineDAO;
        IWorkOrderDAO workOrderDAO = daoFactory.WorkOrderDAO;
        IWorkOrderLineDAO workOrderLineDAO = daoFactory.WorkOrderLineDAO;
        
        // Creation of 2 Lines
        ILine line1 = ModelDAOHelper.ModelFactory.CreateLine();
        ILine line2 = ModelDAOHelper.ModelFactory.CreateLine();
        lineDAO.MakePersistent(line1);
        lineDAO.MakePersistent(line2);
        
        // Reference of 4 WorkOrders
        IList<IWorkOrder> workOrders = workOrderDAO.FindAll();
        Assert.GreaterOrEqual(workOrders.Count, 4, "not enough workOrders in the database (at least 4)");
        IWorkOrder workOrder1 = workOrders[0];
        IWorkOrder workOrder2 = workOrders[1];
        IWorkOrder workOrder3 = workOrders[2];
        IWorkOrder workOrder4 = workOrders[3];
        
        // Creation de 5 WorkOrderLines
        IWorkOrderLine workOrderLine1 = ModelDAOHelper.ModelFactory.CreateWorkOrderLine(line1, new UtcDateTimeRange (DateTime.Now), workOrder1);
        IWorkOrderLine workOrderLine2 = ModelDAOHelper.ModelFactory.CreateWorkOrderLine(line1, new UtcDateTimeRange (DateTime.Now), workOrder2);
        IWorkOrderLine workOrderLine3 = ModelDAOHelper.ModelFactory.CreateWorkOrderLine(line1, new UtcDateTimeRange (DateTime.Now), workOrder3);
        IWorkOrderLine workOrderLine4 = ModelDAOHelper.ModelFactory.CreateWorkOrderLine(line2, new UtcDateTimeRange (DateTime.Now), workOrder3);
        IWorkOrderLine workOrderLine5 = ModelDAOHelper.ModelFactory.CreateWorkOrderLine(line2, new UtcDateTimeRange (DateTime.Now), workOrder4);
        workOrderLineDAO.MakePersistent(workOrderLine1);
        workOrderLineDAO.MakePersistent(workOrderLine2);
        workOrderLineDAO.MakePersistent(workOrderLine3);
        workOrderLineDAO.MakePersistent(workOrderLine4);
        workOrderLineDAO.MakePersistent(workOrderLine5);

        // Check the different numbers of WorkOrderLines
        Assert.AreEqual(3, workOrderLineDAO.FindAllByLine(line1).Count, "wrong count of lineMachines for line 1");
        Assert.AreEqual(2, workOrderLineDAO.FindAllByLine(line2).Count, "wrong count of lineMachines for line 2");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByWorkOrder(workOrder1).Count, "wrong count of lineMachines for machine 1");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByWorkOrder(workOrder2).Count, "wrong count of lineMachines for machine 2");
        Assert.AreEqual(2, workOrderLineDAO.FindAllByWorkOrder(workOrder3).Count, "wrong count of lineMachines for machine 3");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByWorkOrder(workOrder4).Count, "wrong count of lineMachines for machine 4");
        
        // Remove 1 WorkOrderLines and check the new count
        workOrderLineDAO.MakeTransient(workOrderLine5);
        Assert.AreEqual(3, workOrderLineDAO.FindAllByLine(line1).Count, "wrong count of lineMachines for line 1");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByLine(line2).Count, "wrong count of lineMachines for line 2");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByWorkOrder(workOrder1).Count, "wrong count of lineMachines for machine 1");
        Assert.AreEqual(1, workOrderLineDAO.FindAllByWorkOrder(workOrder2).Count, "wrong count of lineMachines for machine 2");
        Assert.AreEqual(2, workOrderLineDAO.FindAllByWorkOrder(workOrder3).Count, "wrong count of lineMachines for machine 3");
        Assert.AreEqual(0, workOrderLineDAO.FindAllByWorkOrder(workOrder4).Count, "wrong count of lineMachines for machine 4");
        
        transaction.Rollback ();
      }
    }
  }
}
