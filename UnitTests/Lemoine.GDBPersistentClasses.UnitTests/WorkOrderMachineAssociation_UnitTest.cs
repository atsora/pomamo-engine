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
          Assert.NotNull (machine);
          WorkOrder workOrder1 = session.Get<WorkOrder> (1);
          Assert.NotNull (workOrder1);
          WorkOrder workOrder2 = session.Get<WorkOrder> (2);
          Assert.NotNull (workOrder2);
          Component component1 = session.Get<Component> (1);
          Assert.NotNull (component1);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          
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
          Assert.AreEqual (5, operationSlots.Count, "Number of operation slots");
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [0].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [0].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [0].Machine.Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [0].WorkOrder).Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [0].Component).Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [0].Operation).Id);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [1].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [1].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [1].Machine.Id);
          Assert.AreEqual (2, ((Lemoine.Collections.IDataWithId)operationSlots [1].WorkOrder).Id);
          Assert.Null (operationSlots [1].Component);
          Assert.Null (operationSlots [1].Operation);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [2].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [2].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [2].Machine.Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [2].WorkOrder).Id);
          Assert.Null (operationSlots [2].Component);
          Assert.Null (operationSlots [2].Operation);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [3].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [3].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [3].Machine.Id);
          Assert.AreEqual (2, ((Lemoine.Collections.IDataWithId)operationSlots [3].WorkOrder).Id);
          Assert.Null (operationSlots [3].Component);
          Assert.Null (operationSlots [3].Operation);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 12), operationSlots [4].BeginDateTime.Value);
          Assert.IsFalse (operationSlots [4].EndDateTime.HasValue);
          Assert.AreEqual (1, operationSlots [4].Machine.Id);
          Assert.AreEqual (2, ((Lemoine.Collections.IDataWithId)operationSlots [4].WorkOrder).Id);
          Assert.Null (operationSlots [4].Component);
          Assert.Null (operationSlots [4].Operation);
          // - Modifications
          IList<WorkOrderMachineAssociation> modifications =
            session.CreateCriteria<WorkOrderMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<WorkOrderMachineAssociation> ();
          Assert.AreEqual (5, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[1].AnalysisStatus, "2nd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[2].AnalysisStatus, "3rd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[3].AnalysisStatus, "4th modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[4].AnalysisStatus, "5th modification status");
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
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.NotNull (workOrder3);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.NotNull (component4);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

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
        
        Assert.AreEqual (3, operationSlots0.Count, "Number of operation slots (1)");
        
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
        Assert.AreEqual (3, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (workOrder3, operationSlots[0].WorkOrder);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[1].EndDateTime.Value);
        Assert.AreEqual (workOrder2, operationSlots[1].WorkOrder);
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
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.NotNull (workOrder3);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.NotNull (component4);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

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
        
        Assert.AreEqual (3, operationSlots0.Count, "Number of operation slots (1)");
        
        
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
        Assert.AreEqual (2, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (workOrder1, operationSlots[0].WorkOrder);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [1].EndDateTime.HasValue);
        Assert.AreEqual (workOrder3, operationSlots[1].WorkOrder);
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
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IWorkOrder workOrder3 = daoFactory.WorkOrderDAO.FindById(4);
        Assert.NotNull (workOrder3);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.NotNull (component4);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

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
        
        Assert.AreEqual (3, operationSlots0.Count, "Number of operation slots (1)");
        
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
        
        Assert.AreEqual (4, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (workOrder1, operationSlots[0].WorkOrder);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.AreEqual ((UtcDateTime.From (2011, 08, 02)).AddHours(1), operationSlots[1].EndDateTime.Value);
        Assert.AreEqual (workOrder3, operationSlots[1].WorkOrder);
        Assert.AreEqual ((UtcDateTime.From (2011, 08, 02)).AddHours(1), operationSlots[2].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[2].EndDateTime.Value);
        Assert.AreEqual (workOrder2, operationSlots[2].WorkOrder);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[3].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots[3].EndDateTime.Value);
        Assert.AreEqual (workOrder1, operationSlots[3].WorkOrder);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.NotNull (workOrder1);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.NotNull (workOrder5);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.NotNull (component4);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.NotNull (operation1);

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
          Assert.AreEqual (1, operationSlots.Count, "Number of operation slots");
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [0].BeginDateTime.Value);
          Assert.IsFalse (operationSlots [0].EndDateTime.HasValue);
          Assert.AreEqual (1, operationSlots [0].Machine.Id);
          Assert.AreEqual (workOrder5, operationSlots [0].WorkOrder);
          Assert.AreEqual (component4, operationSlots [0].Component);
          Assert.AreEqual (null, operationSlots [0].Operation);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.NotNull (workOrder1);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.NotNull (workOrder5);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.NotNull (component4);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.NotNull (operation1);

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
            Assert.AreEqual (2, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder1, operationSlots [i].WorkOrder);
            Assert.AreEqual (null, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder5, operationSlots [i].WorkOrder);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.NotNull (workOrder1);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.NotNull (workOrder5);
          IComponent component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
          Assert.NotNull (component1);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.NotNull (component4);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.NotNull (operation1);

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
            Assert.AreEqual (2, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder1, operationSlots [i].WorkOrder);
            Assert.AreEqual (component1, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder5, operationSlots [i].WorkOrder);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (1);
          Assert.NotNull (workOrder1);
          IWorkOrder workOrder5 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (5);
          Assert.NotNull (workOrder5);
          IComponent component4 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (4);
          Assert.NotNull (component4);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
          Assert.NotNull (operation1);
          
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
            Assert.AreEqual (3, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder5, operationSlots [i].WorkOrder);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (null, operationSlots [i].WorkOrder);
            Assert.AreEqual (null, operationSlots [i].Component);
            Assert.AreEqual (operation1, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder5, operationSlots [i].WorkOrder);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (2);
          Assert.NotNull (workOrder2);
          IComponent component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
          Assert.NotNull (component2);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (12691);
          Assert.NotNull (operation);
          
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
            Assert.AreEqual (3, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder2, operationSlots [i].WorkOrder);
            Assert.AreEqual (component2, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder2, operationSlots [i].WorkOrder);
            Assert.AreEqual (component2, operationSlots [i].Component);
            Assert.AreEqual (operation, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder2, operationSlots [i].WorkOrder);
            Assert.AreEqual (component2, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
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
          Assert.NotNull (machine);
          IWorkOrder workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (2);
          Assert.NotNull (workOrder2);
          IComponent component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
          Assert.NotNull (component2);
          IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (12691);
          Assert.NotNull (operation);
          
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
            Assert.AreEqual (2, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder2, operationSlots [i].WorkOrder);
            Assert.AreEqual (component2, operationSlots [i].Component);
            Assert.AreEqual (operation, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (machine, operationSlots [i].Machine);
            Assert.AreEqual (workOrder2, operationSlots [i].WorkOrder);
            Assert.AreEqual (component2, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
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
