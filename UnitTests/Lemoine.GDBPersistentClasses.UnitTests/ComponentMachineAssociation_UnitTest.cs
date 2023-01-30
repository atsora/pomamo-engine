// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ComponentMachineAssociation
  /// </summary>
  [TestFixture]
  public class ComponentMachineAssociation_UnitTest: Lemoine.UnitTests.WithDayTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ComponentMachineAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ComponentMachineAssociation_UnitTest ()
      : base (Lemoine.UnitTests.UtcDateTime.From  (2011, 07, 31))
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
          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int) OperationSlotSplitOption.None);
          
          // Reference data
          Machine machine = session.Get<Machine> (1);
          Assert.NotNull (machine);
          WorkOrder workOrder1 = session.Get<WorkOrder> (1);
          Assert.NotNull (workOrder1);
          WorkOrder workOrder2 = session.Get<WorkOrder> (2);
          Assert.NotNull (workOrder2);
          Component component1 = session.Get<Component> (1);
          Assert.NotNull (component1);
          Component component2 = session.Get<Component> (2);
          Assert.NotNull (component2);
          Component component3 = session.Get<Component> (18479);
          Assert.NotNull (component3);
          Component component4 = session.Get<Component> (4);
          Assert.NotNull (component2);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          
          // Associate component4 and component1 with another work order,
          // to avoid any automatic determination of work order
          component4.Project.WorkOrders.Add (workOrder2);
          component1.Project.WorkOrders.Add (workOrder2);
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1,
                                    component1,
                                    workOrder1,
                                    null, null, null, null,
                                    new UtcDateTimeRange (T(1), T(2)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    null,
                                    component3,
                                    workOrder1,
                                    null, null, null, null,
                                    new UtcDateTimeRange (T(2), T(3)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    null,
                                    component2,
                                    workOrder2,
                                    null, null, null, null,
                                    new UtcDateTimeRange (T(3), T(5)));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1,
                                    component1,
                                    workOrder1,
                                    null, null, null, null,
                                    R(5, null));
            session.Save (existingOperationSlot);
          }
          
          // New association 2 -> oo
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component1, R(2, null));
            componentMachineAssociation.DateTime = T(4);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          AnalysisUnitTests.RunMakeAnalysis<ComponentMachineAssociation> ();
          { // Check the operation slots
            IList<IOperationSlot> operationSlots =
              ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.AreEqual (4, operationSlots.Count);
          }
          
          // New association 3 -> 8
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component4, R(3, 8));
            componentMachineAssociation.DateTime = T(5);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          // New association 5 -> 6
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component1, R(5, 6));
            componentMachineAssociation.DateTime = T(6);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          // New association 4 -> oo
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component3, R(4, null));
            componentMachineAssociation.DateTime = T(7);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          
          // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<ComponentMachineAssociation> ();
          
          // Check the values
          { // - OperationSlots
            IList<OperationSlot> operationSlots =
              session.CreateCriteria<OperationSlot> ()
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<OperationSlot> ();
            Assert.AreEqual (5, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (T(1), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(2), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Operation).Id);
            ++i;
            Assert.AreEqual (T(2), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(3), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (component1, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i; // day 3
            Assert.AreEqual (T(3), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(4), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (null, operationSlots [i].WorkOrder);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i; // day 4
            Assert.AreEqual (T(4), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(8), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (null, operationSlots [i].WorkOrder);
            Assert.AreEqual (component3, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (T(8), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (component3, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
          }
          // - Modifications
          IList<ComponentMachineAssociation> modifications =
            session.CreateCriteria<ComponentMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<ComponentMachineAssociation> ();
          Assert.AreEqual (4, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[1].AnalysisStatus, "2nd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[2].AnalysisStatus, "3rd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[3].AnalysisStatus, "4th modification status");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with automatic determination of work order
    /// </summary>
    [Test]
    public void TestMakeAnalysis2()
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
          Machine machine = session.Get<Machine> (1);
          Assert.NotNull (machine);
          WorkOrder workOrder1 = session.Get<WorkOrder> (1);
          Assert.NotNull (workOrder1);
          WorkOrder workOrder2 = session.Get<WorkOrder> (2);
          Assert.NotNull (workOrder2);
          Component component1 = session.Get<Component> (1);
          Assert.NotNull (component1);
          Component component2 = session.Get<Component> (2);
          Assert.NotNull (component2);
          Component component3 = session.Get<Component> (18479);
          Assert.NotNull (component3);
          Component component4 = session.Get<Component> (4);
          Assert.NotNull (component2);
          Operation operation1 = session.Get<Operation> (1);
          Assert.NotNull (operation1);
          Operation operation2 = session.Get<Operation> (2);
          Assert.NotNull (operation2);
          
          // component4 is associated to work order 1 only
          Assert.AreEqual (1, component4.Project.WorkOrders.Count);
          foreach (IWorkOrder workOrder in component4.Project.WorkOrders) {
            Assert.AreEqual (5, ((Lemoine.Collections.IDataWithId)workOrder).Id);
          }
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1,
                                    component1,
                                    workOrder1,
                                    null, null, null, null,
                                    R(1, 2));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    null,
                                    component3,
                                    workOrder1,
                                    null, null, null, null,
                                    R(2,3));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    null,
                                    component2,
                                    workOrder2,
                                    null, null, null, null,
                                    R(3,5));
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory
              .CreateOperationSlot (machine,
                                    operation1,
                                    component1,
                                    workOrder1,
                                    null, null, null, null,
                                    R(5, null));
            session.Save (existingOperationSlot);
          }
          
          // New association 2 -> oo
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component1, R(2, null));
            componentMachineAssociation.DateTime = T(4);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          // New association 3 -> 8
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component4, R(3, 8));
            componentMachineAssociation.DateTime = T(5);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          // New association 5 -> 6
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component1, R(5, 6));
            componentMachineAssociation.DateTime = T(6);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          // New association 4 -> oo
          {
            IComponentMachineAssociation componentMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateComponentMachineAssociation (machine, component3, R(4, null));
            componentMachineAssociation.DateTime = T(7);
            ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (componentMachineAssociation);
          }
          
          AnalysisUnitTests.RunMakeAnalysis<ComponentMachineAssociation> ();
          
          // Check the values
          // - OperationSlots
          { // Check the operation slots
            IList<IOperationSlot> operationSlots =
              ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAll (machine);
            Assert.AreEqual (4, operationSlots.Count, "Number of operation slots");
            int i = 0;
            Assert.AreEqual (T(1), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(2), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Operation).Id);
            ++i;
            Assert.AreEqual (T(2), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(3), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (component1, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (T(3), operationSlots [i].BeginDateTime.Value);
            Assert.AreEqual (T(4), operationSlots [i].EndDateTime.Value);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (5, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (component4, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
            ++i;
            Assert.AreEqual (T(4), operationSlots [i].BeginDateTime.Value);
            Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
            Assert.AreEqual (1, operationSlots [i].Machine.Id);
            Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
            Assert.AreEqual (component3, operationSlots [i].Component);
            Assert.AreEqual (null, operationSlots [i].Operation);
          }
          // - Modifications
          IList<ComponentMachineAssociation> modifications =
            session.CreateCriteria<ComponentMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<ComponentMachineAssociation> ();
          Assert.AreEqual (4, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[1].AnalysisStatus, "2nd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[2].AnalysisStatus, "3rd modification status");
          Assert.AreEqual (AnalysisStatus.Done, modifications[3].AnalysisStatus, "4th modification status");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with a ComponentMachineAssociation having
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
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component2 = daoFactory.ComponentDAO.FindById(2);
        Assert.NotNull (component2);
        IComponent component3 = daoFactory.ComponentDAO.FindById(3);
        Assert.NotNull (component3);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

        IOperationSlot opSlot1 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           component1,
                                                           null,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           component2,
                                                           null,
                                                           null, null, null, null,
                                                           R(2,3));
        opSlot2.RunTime = TimeSpan.FromHours (12);

        IOperationSlot opSlot3 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           null,
                                                           component1,
                                                           null,
                                                           null, null, null, null,
                                                           R(3,4));
        opSlot3.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot2);
        daoFactory.OperationSlotDAO.MakePersistent(opSlot3);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (3, operationSlots0.Count, "Number of operation slots (1)");
        
        IComponentMachineAssociation componentMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateComponentMachineAssociation (machine,
                                                                         component3,
                                                                         new UtcDateTimeRange (opSlot1.BeginDateTime.Value,
                                                                                               opSlot1.EndDateTime));
        componentMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
        
        daoFactory.ComponentMachineAssociationDAO.MakePersistent(componentMachineAssociation);
        
        ((ComponentMachineAssociation) componentMachineAssociation).MakeAnalysis ();
        foreach (IModification subModification in componentMachineAssociation.SubModifications) {
          ((Modification)subModification).MakeAnalysis ();
        }
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (3, operationSlots.Count, "Number of operation slots (2)");
        int i = 0;
        Assert.AreEqual (T(1), operationSlots[i].BeginDateTime.Value);
        Assert.AreEqual (T(2), operationSlots[i].EndDateTime.Value);
        Assert.AreEqual (component3, operationSlots[i].Component);
        ++i;
        Assert.AreEqual (T(2), operationSlots[i].BeginDateTime.Value);
        Assert.AreEqual (T(3), operationSlots[i].EndDateTime.Value);
        Assert.AreEqual (component2, operationSlots[i].Component);
        
        transaction.Rollback ();
      }
      
      // Reset the config
      Lemoine.Info.ConfigSet.ResetForceValues ();
    }

    /// <summary>
    /// Test the method MakeAnalysis with a ComponentMachineAssociation having
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
        // Config
        Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                           (int) OperationSlotSplitOption.None);
        
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component2 = daoFactory.ComponentDAO.FindById(2);
        Assert.NotNull (component2);
        IComponent component3 = daoFactory.ComponentDAO.FindById(3);
        Assert.NotNull (component3);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

        IOperationSlot opSlot1 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation1,
                                                           component1,
                                                           null,
                                                           null, null, null, null,
                                                           R(1,2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           component2,
                                                           null,
                                                           null, null, null, null,
                                                           R(2,3));
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
        
        IComponentMachineAssociation componentMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateComponentMachineAssociation (machine,
                                                                         component3,
                                                                         new UtcDateTimeRange (
                                                                           opSlot2.BeginDateTime.Value,
                                                                           // too short
                                                                           opSlot2.BeginDateTime.Value.AddHours(1)));
        
        componentMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // but extend to slot
        
        daoFactory.ComponentMachineAssociationDAO.MakePersistent(componentMachineAssociation);
        
        ((ComponentMachineAssociation) componentMachineAssociation).MakeAnalysis ();
        foreach (IModification subModification in componentMachineAssociation.SubModifications) {
          ((Modification)subModification).MakeAnalysis ();
        }
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (2, operationSlots.Count, "Number of operation slots (2)");
        int i = 0;
        Assert.AreEqual (T(1), operationSlots[i].BeginDateTime.Value);
        Assert.AreEqual (T(2), operationSlots[i].EndDateTime.Value);
        Assert.AreEqual (component1, operationSlots[i].Component);
        ++i;
        Assert.AreEqual (T(2), operationSlots[i].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);// current slot extended to infinity
        Assert.AreEqual (component3, operationSlots[i].Component);
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with a ComponentMachineAssociation having
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
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component2 = daoFactory.ComponentDAO.FindById(2);
        Assert.NotNull (component2);
        IComponent component3 = daoFactory.ComponentDAO.FindById(3);
        Assert.NotNull (component3);        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
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
                                                           component1,
                                                           null,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           component2,
                                                           null,
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
        
        IComponentMachineAssociation componentMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateComponentMachineAssociation (machine,
                                                                         component3,
                                                                         new UtcDateTimeRange (
                                                                           opSlot2.BeginDateTime.Value,
                                                                           // too short
                                                                           opSlot2.BeginDateTime.Value.AddHours(1)));
        
        // componentMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // NO
        
        daoFactory.ComponentMachineAssociationDAO.MakePersistent(componentMachineAssociation);
        
        ((ComponentMachineAssociation) componentMachineAssociation).MakeAnalysis ();
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (4, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (T(1), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (T(2), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (component1, operationSlots[0].Component);
        Assert.AreEqual (T(2), operationSlots[1].BeginDateTime.Value);
        Assert.AreEqual ((T(2)).AddHours(1), operationSlots[1].EndDateTime.Value);
        Assert.AreEqual (component3, operationSlots[1].Component);
        Assert.AreEqual ((T(2)).AddHours(1), operationSlots[2].BeginDateTime.Value);
        Assert.AreEqual (T(3), operationSlots[2].EndDateTime.Value);
        Assert.AreEqual (component2, operationSlots[2].Component);
        Assert.AreEqual (T(3), operationSlots[3].BeginDateTime.Value);
        Assert.AreEqual (T(4), operationSlots[3].EndDateTime.Value);
        Assert.AreEqual (null, operationSlots[3].Component);
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
