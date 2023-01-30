// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the persistent class WorkOrderLineQuantity.
  /// </summary>
  [TestFixture]
  public class WorkOrderLineQuantity_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLineQuantity_UnitTest).FullName);
    
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
    public void TestQuantity()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ILineDAO lineDAO = daoFactory.LineDAO;
        IWorkOrderDAO workOrderDAO = daoFactory.WorkOrderDAO;
        IWorkOrderLineDAO workOrderLineDAO = daoFactory.WorkOrderLineDAO;
        IIntermediateWorkPieceDAO iwpDAO = daoFactory.IntermediateWorkPieceDAO;
        
        // Creation of 2 Lines
        ILine line1 = ModelDAOHelper.ModelFactory.CreateLine();
        lineDAO.MakePersistent(line1);
        
        // Reference to 1 WorkOrder
        IList<IWorkOrder> workOrders = workOrderDAO.FindAll();
        Assert.GreaterOrEqual(workOrders.Count, 1, "not enough workorders in the database (at least 1)");
        IWorkOrder workOrder1 = workOrders[0];
        
        // Creation of 2 WorkOrderLines
        IWorkOrderLine workOrderLine1 = ModelDAOHelper.ModelFactory
          .CreateWorkOrderLine(line1,
                               new UtcDateTimeRange (DateTime.Now),
                               workOrder1);
        workOrderLineDAO.MakePersistent(workOrderLine1);
        
        // Reference of 4 IntermediateWorkPiece
        IList<IIntermediateWorkPiece> iwps = iwpDAO.FindAll();
        Assert.GreaterOrEqual(iwps.Count, 1, "not enough intermediate work pieces in the database (at least 1)");
        IIntermediateWorkPiece iwp1 = iwps[0];
        
        // Creation de 5 WorkOrderLineQuantities
        workOrderLine1.SetIntermediateWorkPieceQuantity (iwp1, 10);
        workOrderLineDAO.MakePersistent(workOrderLine1);
        ModelDAOHelper.DAOFactory.FlushData ();

        // Check the different numbers of WorkOrderLineQuantities
        { // WorkOrderLine1
          IWorkOrderLine wol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
            .FindById (workOrderLine1.Id, line1);
          Assert.IsTrue (wol.IntermediateWorkPieceQuantities.ContainsKey (((Lemoine.Collections.IDataWithId)iwp1).Id));
          Assert.AreEqual (10, wol.IntermediateWorkPieceQuantities [((Lemoine.Collections.IDataWithId)iwp1).Id].Quantity);
        }
        
        transaction.Rollback ();
      }
    }
  }
}
