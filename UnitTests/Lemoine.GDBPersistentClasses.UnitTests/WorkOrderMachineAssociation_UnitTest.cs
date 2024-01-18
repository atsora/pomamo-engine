// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class WorkOrderMachineAssociation
  /// </summary>
  [TestFixture]
  public class WorkOrderMachineAssociation_UnitTest: WithDayTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderMachineAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkOrderMachineAssociation_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          Machine machine = session.Get<Machine> (1);
          Assert.That (machine, Is.Not.Null);
          WorkOrder workOrder1 = session.Get<WorkOrder> (1);
          Assert.That (workOrder1, Is.Not.Null);
          WorkOrder workOrder2 = session.Get<WorkOrder> (2);
          Assert.That (workOrder2, Is.Not.Null);
          Component component1 = session.Get<Component> (1);
          Assert.That (component1, Is.Not.Null);
          Operation operation1 = session.Get<Operation> (1);
          Assert.That (operation1, Is.Not.Null);
          Operation operation2 = session.Get<Operation> (2);
          Assert.That (operation2, Is.Not.Null);
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation1,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(1, 2));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation2,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(2, 3));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               null,
                                                               workOrder2,
                                                               null, null, null, null,
                                                               R(3, 5));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation1,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(5, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          
          // New association 2 -> oo
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder2, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          // New association 3 -> 8
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder1, UtcDateTime.From (2011, 08, 03));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 05);
            workOrderMachineAssociation.End = UtcDateTime.From (2011, 08, 08);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          // New association 5 -> 6
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder2, UtcDateTime.From (2011, 08, 05));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 06);
            workOrderMachineAssociation.End = UtcDateTime.From (2011, 08, 06);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          // New association 4 -> oo
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder2, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 07);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          // New association 10 -> 12
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, null, UtcDateTime.From (2011, 08, 10));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 08);
            workOrderMachineAssociation.End = UtcDateTime.From (2011, 08, 12);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          
          // Run MakeAnalysis
          IList<WorkOrderMachineAssociation> workOrderMachineAssociations =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          foreach (WorkOrderMachineAssociation workOrderMachineAssociation
                   in workOrderMachineAssociations ) {
            workOrderMachineAssociation.MakeAnalysis ();
          }
          
          // Check the values
          // - OperationSlots
          IList<OperationSlot> operationSlots =
            session.CreateCriteria<OperationSlot> ()
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<OperationSlot> ();
          Assert.That (operationSlots, Has.Count.EqualTo (5), "Number of operation slots");
          Assert.Multiple (() => {
            Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (operationSlots[0].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[0].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[0].Component).Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[0].Operation).Id, Is.EqualTo (1));
            Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (operationSlots[1].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[1].WorkOrder).Id, Is.EqualTo (2));
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[1].Component, Is.Null);
            Assert.That (operationSlots[1].Operation, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[2].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (operationSlots[2].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (operationSlots[2].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[2].WorkOrder).Id, Is.EqualTo (1));
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[2].Component, Is.Null);
            Assert.That (operationSlots[2].Operation, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[3].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (operationSlots[3].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
            Assert.That (operationSlots[3].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[3].WorkOrder).Id, Is.EqualTo (2));
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[3].Component, Is.Null);
            Assert.That (operationSlots[3].Operation, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[4].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 12)));
            Assert.That (operationSlots[4].EndDateTime.HasValue, Is.False);
            Assert.That (operationSlots[4].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[4].WorkOrder).Id, Is.EqualTo (2));
          });
          Assert.Multiple (() => {
            Assert.That (operationSlots[4].Component, Is.Null);
            Assert.That (operationSlots[4].Operation, Is.Null);
          });
          // - Modifications
          IList<WorkOrderMachineAssociation> modifications =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          Assert.That (modifications, Has.Count.EqualTo (5), "Number of modifications");
          Assert.Multiple (() => {
            Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
            Assert.That (modifications[1].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "2nd modification status");
            Assert.That (modifications[2].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "3rd modification status");
            Assert.That (modifications[3].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "4th modification status");
            Assert.That (modifications[4].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "5th modification status");
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis with a WorkOrderMachineAssociation having
    /// a AssociateToSlot Option (case set operation on non-current operation slot:
    /// only this slot should be impacted)
    /// </summary>
    [Test]
    public void TestMakeAnalysisSlotAssociationCase1()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.That (workOrder3, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.That (component4, Is.Not.Null);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null);

        IOperationSlot opSlot1 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
                                                           workOrder2,
                                                           null, null, null, null,
                                                           R(2, 3));
        opSlot2.RunTime = TimeSpan.FromHours (12);

        IOperationSlot opSlot3 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           null,
                                                           null,
                                                           null,
                                                           null, null, null, null,
                                                           R(3, 4));
        opSlot3.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot2);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot3);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots0, Has.Count.EqualTo (3), "Number of operation slots (1)");
        
        IWorkOrderMachineAssociation workOrderMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine,
                                                                         workOrder3,
                                                                         opSlot1.BeginDateTime.Value);
        workOrderMachineAssociation.End = opSlot1.EndDateTime;
        workOrderMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
        
        daoFactory.WorkOrderMachineAssociationDAO.MakePersistent(workOrderMachineAssociation);
        
        ((WorkOrderMachineAssociation) workOrderMachineAssociation).MakeAnalysis ();
        foreach (IModification subModification in workOrderMachineAssociation.SubModifications) {
          ((Modification)subModification).MakeAnalysis ();
        }
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        Assert.That (operationSlots, Has.Count.EqualTo (3), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].WorkOrder, Is.EqualTo (workOrder3));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[1].WorkOrder, Is.EqualTo (workOrder2));
        });
      }
    }

    
    /// <summary>
    /// Test the method MakeAnalysis with a WorkOrderMachineAssociation having
    /// a AssociateToSlot Option (case set operation on current operation slot
    /// with end lesser than slot's end: the slot is impacted and merged with the next)
    /// </summary>
    [Test]
    public void TestMakeAnalysisSlotAssociationCase2()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.That (workOrder3, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.That (component4, Is.Not.Null);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null);

        IOperationSlot opSlot1 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
                                                           workOrder2,
                                                           null, null, null, null,
                                                           R(2, 3));
        opSlot2.RunTime = TimeSpan.FromHours (12);

        IOperationSlot opSlot3 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(3, 4));
        opSlot3.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot2);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot3);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots0, Has.Count.EqualTo (3), "Number of operation slots (1)");
        
        
        IWorkOrderMachineAssociation workOrderMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine,
                                                                         workOrder3,
                                                                         opSlot2.BeginDateTime.Value);
        workOrderMachineAssociation.End = opSlot2.BeginDateTime.Value.AddHours(1); // too short
        workOrderMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // but extend to slot
        
        daoFactory.WorkOrderMachineAssociationDAO.MakePersistent(workOrderMachineAssociation);
        
        ((WorkOrderMachineAssociation) workOrderMachineAssociation).MakeAnalysis ();
        foreach (IModification subModification in workOrderMachineAssociation.SubModifications) {
          ((Modification)subModification).MakeAnalysis ();
        }
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].WorkOrder, Is.EqualTo (workOrder1));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.HasValue, Is.False);
          Assert.That (operationSlots[1].WorkOrder, Is.EqualTo (workOrder3));
        });
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with a WorkOrderMachineAssociation having
    /// no AssociateToSlot Option: otherwise similar to TestMakeAnalysisSlotAssociationCase2.
    /// Since option not set, operation slot is cut.
    /// </summary>
    [Test]
    public void TestMakeAnalysisSlotAssociationCase3()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.That (workOrder3, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.That (component4, Is.Not.Null);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null);

        IOperationSlot opSlot1 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
                                                           workOrder2,
                                                           null, null, null, null,
                                                           R(2, 3));
        opSlot2.RunTime = TimeSpan.FromHours (12);

        IOperationSlot opSlot3 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(3, 4));
        opSlot3.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot2);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot3);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots0, Has.Count.EqualTo (3), "Number of operation slots (1)");
        
        IWorkOrderMachineAssociation workOrderMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine,
                                                                         workOrder3,
                                                                         opSlot2.BeginDateTime.Value);
        workOrderMachineAssociation.End = opSlot2.BeginDateTime.Value.AddHours(1); // too short
        // workOrderMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // NO
        
        daoFactory.WorkOrderMachineAssociationDAO.MakePersistent(workOrderMachineAssociation);
        
        ((WorkOrderMachineAssociation) workOrderMachineAssociation).MakeAnalysis ();
        
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots, Has.Count.EqualTo (4), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].WorkOrder, Is.EqualTo (workOrder1));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo ((UtcDateTime.From (2011, 08, 02)).AddHours (1)));
          Assert.That (operationSlots[1].WorkOrder, Is.EqualTo (workOrder3));
          Assert.That (operationSlots[2].BeginDateTime.Value, Is.EqualTo ((UtcDateTime.From (2011, 08, 02)).AddHours (1)));
          Assert.That (operationSlots[2].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[2].WorkOrder, Is.EqualTo (workOrder2));
          Assert.That (operationSlots[3].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[3].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          Assert.That (operationSlots[3].WorkOrder, Is.EqualTo (workOrder1));
        });
      }
    }

    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Part ConvertToSlot
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder1 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.That (workOrder1, Is.Not.Null);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.That (workOrder5, Is.Not.Null);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.That (component4, Is.Not.Null);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.That (operation1, Is.Not.Null);

          // Config
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder), true);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly), false);
          
          // New association 2 -> oo
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder5, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          
          // Run MakeAnalysis
          IList<WorkOrderMachineAssociation> workOrderMachineAssociations =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          foreach (WorkOrderMachineAssociation workOrderMachineAssociation
                   in workOrderMachineAssociations ) {
            workOrderMachineAssociation.MakeAnalysis ();
          }
          
          // Check the values
          // - OperationSlots
          IList<OperationSlot> operationSlots =
            session.CreateCriteria<OperationSlot> ()
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<OperationSlot> ();
          Assert.That (operationSlots, Has.Count.EqualTo (1), "Number of operation slots");
          Assert.Multiple (() => {
            Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (operationSlots[0].EndDateTime.HasValue, Is.False);
            Assert.That (operationSlots[0].Machine.Id, Is.EqualTo (1));
            Assert.That (operationSlots[0].WorkOrder, Is.EqualTo (workOrder5));
            Assert.That (operationSlots[0].Component, Is.EqualTo (component4));
            Assert.That (operationSlots[0].Operation, Is.EqualTo (null));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Part MergeDataWithOldSlot
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder2 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.That (workOrder1, Is.Not.Null);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.That (workOrder5, Is.Not.Null);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.That (component4, Is.Not.Null);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.That (operation1, Is.Not.Null);

          // Config
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder), true);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly), false);

          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               null,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(1, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }

          // New association 2 -> oo
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder5, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          
          // Run MakeAnalysis
          IList<WorkOrderMachineAssociation> workOrderMachineAssociations =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          foreach (WorkOrderMachineAssociation workOrderMachineAssociation
                   in workOrderMachineAssociations ) {
            workOrderMachineAssociation.MakeAnalysis ();
          }
          
          // Check the values
          // - OperationSlots
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder1));
              Assert.That (operationSlots[i].Component, Is.EqualTo (null));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder5));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component4));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Part MergeDataWithOldSlot when there is an existing component in the slot
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder2b ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.That (workOrder1, Is.Not.Null);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.That (workOrder5, Is.Not.Null);
          IComponent component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
          Assert.That (component1, Is.Not.Null);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.That (component4, Is.Not.Null);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.That (operation1, Is.Not.Null);

          // Config
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder), true);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly), false);
          Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly), false);

          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(1, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }

          // New association 2 -> oo
          {
            IWorkOrderMachineAssociation workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, workOrder5, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
            workOrderMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderMachineAssociation);
          }
          
          // Run MakeAnalysis
          IList<WorkOrderMachineAssociation> workOrderMachineAssociations =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          foreach (WorkOrderMachineAssociation workOrderMachineAssociation
                   in workOrderMachineAssociations ) {
            workOrderMachineAssociation.MakeAnalysis ();
          }
          
          // Check the values
          // - OperationSlots
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder1));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder5));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component4));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Insert another incompatible operation
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder3 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.That (workOrder1, Is.Not.Null);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.That (workOrder5, Is.Not.Null);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.That (component4, Is.Not.Null);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.That (operation1, Is.Not.Null);
          
          // Config
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder));
            config.Value = true;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          Lemoine.Info.ConfigSet.ResetCache ();

          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               component4,
                                                               workOrder5,
                                                               null, null, null, null,
                                                               R(1, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }

          // New association 2 -> 3
          {
            IOperationMachineAssociation operationMachineAssociation =
              ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, UtcDateTime.From (2011, 08, 02));
            operationMachineAssociation.Operation = operation1;
            operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            operationMachineAssociation.End = UtcDateTime.From (2011, 08, 03);
            ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (operationMachineAssociation);
          }
          
          // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
          
          // Check the values
          // - OperationSlots
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots, Has.Count.EqualTo (3), "Number of operation slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder5));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component4));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (null));
              Assert.That (operationSlots[i].Component, Is.EqualTo (null));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder5));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component4));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Insert another compatible operation
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder4 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (2);
          Assert.That (workOrder2, Is.Not.Null);
          IComponent component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
          Assert.That (component2, Is.Not.Null);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (12691);
          Assert.That (operation, Is.Not.Null);
          
          // Config
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder));
            config.Value = true;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          Lemoine.Info.ConfigSet.ResetCache ();

          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               component2,
                                                               workOrder2,
                                                               null, null, null, null,
                                                               R(1, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }

          // New association 2 -> 3
          {
            IOperationMachineAssociation operationMachineAssociation =
              ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, UtcDateTime.From (2011, 08, 02));
            operationMachineAssociation.Operation = operation;
            operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            operationMachineAssociation.End = UtcDateTime.From (2011, 08, 03);
            ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (operationMachineAssociation);
          }
          
          // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
          
          // Check the values
          // - OperationSlots
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots, Has.Count.EqualTo (3), "Number of operation slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder2));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component2));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder2));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component2));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder2));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component2));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test with the UniqueProjectOrPartFromWorkOrder option
    /// 
    /// Insert another compatible operation
    /// </summary>
    [Test]
    public void TestUniqueProjectOrPartFromWorkOrder5 ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        try {
          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int) OperationSlotSplitOption.None);
          
          // Reference data
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (2);
          Assert.That (workOrder2, Is.Not.Null);
          IComponent component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
          Assert.That (component2, Is.Not.Null);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (12691);
          Assert.That (operation, Is.Not.Null);
          
          // Config
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder));
            config.Value = true;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueComponentFromOperation));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          {
            IConfig config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly));
            config.Value = false;
            ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
          }
          Lemoine.Info.ConfigSet.ResetCache ();

          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation,
                                                               component2,
                                                               workOrder2,
                                                               null, null, null, null,
                                                               R(1, 2));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               null,
                                                               component2,
                                                               workOrder2,
                                                               null, null, null, null,
                                                               R(2, null));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }

          // New association 2 -> 3
          {
            IOperationMachineAssociation operationMachineAssociation =
              ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (machine, UtcDateTime.From (2011, 08, 02));
            operationMachineAssociation.Operation = operation;
            operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            operationMachineAssociation.End = UtcDateTime.From (2011, 08, 03);
            ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (operationMachineAssociation);
          }
          
          // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
          
          // Check the values
          // - OperationSlots
          {
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots");
            int i = 0;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
              Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder2));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component2));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (operation));
            });
            ++i;
            Assert.Multiple (() => {
              Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
              Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
              Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
              Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder2));
              Assert.That (operationSlots[i].Component, Is.EqualTo (component2));
              Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            });
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      Lemoine.Info.ConfigSet.ResetForceValues ();
    }
  }
}
