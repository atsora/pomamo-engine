// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Analysis.Detection;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationDetection.
  /// </summary>
  [TestFixture]
  public class OperationDetection_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationDetection_UnitTest).FullName);

    /// <summary>
    /// Test if the auto-operation is applied with StartOperation
    /// </summary>
    [Test]
    public void TestStartOperationWithAutoOperation()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        Assert.NotNull (operation1);
        
        OperationDetection operationDetection = new OperationDetection (machine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
        operationDetection.StartOperation (operation1, T (0));
        operationDetection.StartOperation (operation1, T(2));
        
        {
          // Check there is operation1 between T(0) and T(2)+1s
          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAll (machine);
          Assert.That (operationSlots.Count, Is.EqualTo (1), "Number of operation slots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (2).AddSeconds (1)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          });
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test if the auto-component / auto-workorder is applied with StartOperation
    /// </summary>
    [Test]
    public void TestStartOperationWithAutoComponent()
    {
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (1);
        Assert.NotNull (machine);
        IComponent component =
          ModelDAOHelper.DAOFactory.ComponentDAO
          .FindById (2);
        IWorkOrder workOrder =
          ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (2);
        IOperation operation1 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (12691);
        Assert.NotNull (operation1);
        IOperation operation2 =
          ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (12692);
        Assert.NotNull (operation2);
        
        OperationDetection operationDetection = new OperationDetection (machine, new List<Lemoine.Extensions.Analysis.IOperationDetectionExtension> ());
        operationDetection.StartOperation (operation1, T(0));
        operationDetection.ExtendOperation (operation1, T(1));
        operationDetection.StartOperation (operation2, T(2));
        
        {
          // Check there is operation1 between T(0) and T(1)
          // and operation2 between T(2) and T(2)+1s
          IList<IOperationSlot> operationSlots =
            ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAll (machine);
          Assert.That (operationSlots.Count, Is.EqualTo (2), "Number of operation slots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (0)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (T (2).AddSeconds (1)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
          });
        }
        
        // Check there are two new machine associations
        {
          IList<IComponentMachineAssociation> associations =
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.FindAll ();
          Assert.That (associations.Count, Is.EqualTo (1));
          Assert.Multiple (() => {
            Assert.That (associations[0].Component, Is.EqualTo (component));
            Assert.That (associations[0].Machine, Is.EqualTo (machine));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (1)));
            Assert.That (associations[0].End.Value, Is.EqualTo (T (2)));
          });
        }
        {
          IList<IWorkOrderMachineAssociation> associations =
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.FindAll ();
          Assert.That (associations.Count, Is.EqualTo (1));
          Assert.Multiple (() => {
            Assert.That (associations[0].WorkOrder, Is.EqualTo (workOrder));
            Assert.That (associations[0].Machine, Is.EqualTo (machine));
            Assert.That (associations[0].Begin.Value, Is.EqualTo (T (1)));
            Assert.That (associations[0].End.Value, Is.EqualTo (T (2)));
          });
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Return different date/times for the tests
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    DateTime T (int i)
    {
      return UtcDateTime.From (2012, 06, 01, 12, 00, i);
    }
  }
}
