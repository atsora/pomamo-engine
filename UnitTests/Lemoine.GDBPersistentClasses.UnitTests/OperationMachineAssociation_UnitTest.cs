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
using System.Linq;
using Lemoine.Plugin.CycleCountSummary;
using Lemoine.Plugin.IntermediateWorkPieceSummary;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationMachineAssociation
  /// </summary>
  [TestFixture]
  public class OperationMachineAssociation_UnitTest: WithDayTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationMachineAssociation_UnitTest).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationMachineAssociation_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }
    
    /// <summary>
    /// Test the method MakeAnalysis with an OperationMachineAssociation having
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
        
        IOperationMachineAssociation operationMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                         opSlot1.BeginDateTime.Value);
        operationMachineAssociation.Operation = operation3;
        operationMachineAssociation.End = opSlot1.EndDateTime;
        operationMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
        
        daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (3, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (operation3, operationSlots[0].Operation);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[1].EndDateTime.Value);
        Assert.AreEqual (operation2, operationSlots[1].Operation);
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with an OperationMachineAssociation having
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
                                                           null,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
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
        
        IOperationMachineAssociation operationMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                         opSlot2.BeginDateTime.Value);
        operationMachineAssociation.Operation = operation3;
        operationMachineAssociation.End = opSlot2.BeginDateTime.Value.AddHours(1); // too short
        operationMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // but extend to slot
        
        daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (2, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (operation1, operationSlots[0].Operation);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [1].EndDateTime.HasValue); // current slot extended to infinity
        Assert.AreEqual (operation3, operationSlots[1].Operation);
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with an OperationMachineAssociation having
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
                                                           null,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        IOperationSlot opSlot2 =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                           operation2,
                                                           null,
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
        
        IOperationMachineAssociation operationMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                         opSlot2.BeginDateTime.Value);
        operationMachineAssociation.Operation = operation3;
        operationMachineAssociation.End = opSlot2.BeginDateTime.Value.AddHours(1); // too short
        // operationMachineAssociation.Option = AssociationOption.AssociateToSlotOption; // NO
        
        daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        
        ((OperationMachineAssociation) operationMachineAssociation).MakeAnalysis ();
        
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (4, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (operation1, operationSlots[0].Operation);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[1].BeginDateTime.Value);
        Assert.AreEqual ((UtcDateTime.From (2011, 08, 02)).AddHours(1), operationSlots[1].EndDateTime.Value);
        Assert.AreEqual (operation3, operationSlots[1].Operation);
        Assert.AreEqual ((UtcDateTime.From (2011, 08, 02)).AddHours(1), operationSlots[2].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[2].EndDateTime.Value);
        Assert.AreEqual (operation2, operationSlots[2].Operation);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots[3].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots[3].EndDateTime.Value);
        Assert.AreEqual (null, operationSlots[3].Operation);
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis with an OperationMachineAssociation having
    /// a AssociateToSlot Option (case set operation on a machine with a unique
    /// operation slot with no operation)
    /// </summary>
    [Test]
    public void TestMakeAnalysisSlotAssociationCase4()
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
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (1, operationSlots0.Count, "Number of operation slots (1)");
        
        IOperationMachineAssociation operationMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                         opSlot1.BeginDateTime.Value);
        operationMachineAssociation.Operation = operation1;
        operationMachineAssociation.End = opSlot1.EndDateTime;
        operationMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
        
        daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (1, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [0].EndDateTime.HasValue);
        Assert.AreEqual (operation1, operationSlots[0].Operation);
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with an OperationMachineAssociation having
    /// a AssociateToSlot Option (case set operation at a date where there is not slot:
    /// should fail)
    /// </summary>
    [Test]
    public void TestMakeAnalysisSlotAssociationCase5()
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
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (1, operationSlots0.Count, "Number of operation slots (1)");
        
        IOperationMachineAssociation operationMachineAssociation =
          ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                         new UtcDateTimeRange (opSlot1.EndDateTime.Value.AddHours(1)));
        operationMachineAssociation.Operation = operation1;
        operationMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
        
        daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
        ((OperationMachineAssociation) operationMachineAssociation).MakeAnalysis ();
        
        
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);
        
        Assert.AreEqual (1, operationSlots.Count, "Number of operation slots (2)");
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots[0].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots[0].EndDateTime.Value);
        Assert.AreEqual (null, operationSlots[0].Operation);
        
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (1);
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Config
        Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                           (int) OperationSlotSplitOption.None);
        
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.NotNull (component4);
        IIntermediateWorkPiece intermediateWorkPiece1 = daoFactory.IntermediateWorkPieceDAO.FindById (13506);
        Assert.NotNull (intermediateWorkPiece1); // associated to operation1
        IIntermediateWorkPiece intermediateWorkPiece2 = daoFactory.IntermediateWorkPieceDAO.FindById (2);
        Assert.NotNull (intermediateWorkPiece2); // associated to operation2
        IIntermediateWorkPiece intermediateWorkPiece3 = daoFactory.IntermediateWorkPieceDAO.FindById (11003);
        Assert.NotNull (intermediateWorkPiece3); // associated to operation3
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);
        MachineObservationState attended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Attended);
        MachineObservationState unattended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = daoFactory.ReasonDAO.FindById(2);
        IReason reasonShort = daoFactory.ReasonDAO.FindById(3);
        IReason reasonUnanswered = daoFactory.ReasonDAO.FindById(4);
        IReason reasonUnattended = daoFactory.ReasonDAO.FindById(5);
        IMachineMode inactive = daoFactory.MachineModeDAO.FindById(1);
        IMachineMode active = daoFactory.MachineModeDAO.FindById(2);
        IMachineMode auto = daoFactory.MachineModeDAO.FindById(3);
        
        // Associate operations to another components
        // to avoid any automatic determination of component and work order
        component4.AddIntermediateWorkPiece (intermediateWorkPiece1);
        component4.AddIntermediateWorkPiece (intermediateWorkPiece2);
        component4.AddIntermediateWorkPiece (intermediateWorkPiece3);

        // Existing reason slots (motion only)
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 00, 00),
                                                  UtcDateTime.From (2011, 08, 02, 12, 00, 00)));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04),
                                                  UtcDateTime.From (2011, 08, 06)));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }
        
        // Existing operation slots
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation1,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(1, 2));
          existingOperationSlot.RunTime = TimeSpan.FromHours (12);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation2,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(2, 3));
          existingOperationSlot.RunTime = TimeSpan.FromHours (12);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             null,
                                                             null,
                                                             workOrder2,
                                                             null, null, null, null,
                                                             R(3, 5));
          existingOperationSlot.RunTime = TimeSpan.FromHours (24);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation1,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(5, null));
          existingOperationSlot.RunTime = TimeSpan.FromHours (24);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        
        // New association 2 -> oo
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 3 -> 8
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 03));
          operationMachineAssociation.Operation = operation3;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 05);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 08);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 5 -> 6
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 05));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 06);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 06);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 4 -> oo
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
          operationMachineAssociation.Operation = operation2;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 07);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 10 -> 12
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 10));
          operationMachineAssociation.Operation = null;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 08);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 12);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 2 -> 3 (again, previously 2 -> 4)
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 02));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 09);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 03);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 12 -> oo (again, previously 4 -> oo)
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 12)));
          operationMachineAssociation.Operation = operation2;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 10);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        
        // Run MakeAnalysis
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        // Check the values
        // - OperationSlots
        IList<OperationSlot> operationSlots =
          session.CreateCriteria<OperationSlot> ()
          .AddOrder (Order.Asc ("DateTimeRange"))
          .List<OperationSlot> ();
        Assert.AreEqual (6, operationSlots.Count, "Number of operation slots");
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
        Assert.AreEqual (operation1, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromHours (24), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (null, operationSlots [i].WorkOrder);
        Assert.AreEqual (null, operationSlots [i].Component);
        Assert.AreEqual (operation3, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (1), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 08), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (null, operationSlots [i].WorkOrder);
        Assert.AreEqual (null, operationSlots [i].Component);
        Assert.AreEqual (operation2, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (4), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 08), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
        Assert.AreEqual (component1, operationSlots [i].Component);
        Assert.AreEqual (operation2, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 12), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
        Assert.AreEqual (component1, operationSlots [i].Component);
        Assert.AreEqual (null, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 12), operationSlots [i].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
        Assert.AreEqual (component1, operationSlots [i].Component);
        Assert.AreEqual (operation2, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.AreEqual (12, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[1].AnalysisStatus, "2nd modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[2].AnalysisStatus, "3rd modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[3].AnalysisStatus, "4th modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[4].AnalysisStatus, "5th modification status");
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysisAutoWorkOrder()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Config
        Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                           (int) OperationSlotSplitOption.None);
        
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1); // => workOrder1
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2); // => workOrder1
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3); // => workOrder3
        MachineObservationState attended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Attended);
        MachineObservationState unattended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = daoFactory.ReasonDAO.FindById(2);
        IReason reasonShort = daoFactory.ReasonDAO.FindById(3);
        IReason reasonUnanswered = daoFactory.ReasonDAO.FindById(4);
        IReason reasonUnattended = daoFactory.ReasonDAO.FindById(5);
        IMachineMode inactive = daoFactory.MachineModeDAO.FindById(1);
        IMachineMode active = daoFactory.MachineModeDAO.FindById(2);
        IMachineMode auto = daoFactory.MachineModeDAO.FindById(3);
        
        // Existing reason slots (motion only)
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 00, 00),
                                                  UtcDateTime.From (2011, 08, 02, 12, 00, 00)));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            R(4, 6));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }
        
        // Existing operation slots
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation1,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(1, 2));
          existingOperationSlot.RunTime = TimeSpan.FromHours (12);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation2,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(2, 3));
          existingOperationSlot.RunTime = TimeSpan.FromHours (12);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             null,
                                                             null,
                                                             workOrder2,
                                                             null, null, null, null,
                                                             R(3, 5));
          existingOperationSlot.RunTime = TimeSpan.FromHours (24);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation1,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(5, null));
          existingOperationSlot.RunTime = TimeSpan.FromHours (24);
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        
        // New association 2 -> oo
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 02)));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 3 -> 8
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 03));
          operationMachineAssociation.Operation = operation3;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 05);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 08);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 5 -> 6
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 05));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 06);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 06);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 4 -> oo
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
          operationMachineAssociation.Operation = operation2;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 07);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 10 -> 12
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 10));
          operationMachineAssociation.Operation = null;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 08);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 12);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 2 -> 3 (again, previously 2 -> 4)
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           UtcDateTime.From (2011, 08, 02));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 09);
          operationMachineAssociation.End = UtcDateTime.From (2011, 08, 03);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 12 -> oo (again, previously 4 -> oo)
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 12)));
          operationMachineAssociation.Operation = operation2;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 10);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
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
          Assert.AreEqual (5, operationSlots.Count, "Number of operation slots");
          int i = 0;
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].EndDateTime.Value);
          Assert.AreEqual (machine, operationSlots [i].Machine);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
          Assert.AreEqual (operation1, operationSlots [i].Operation);
          Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].Duration);
          Assert.AreEqual (TimeSpan.FromHours (24), operationSlots [i].RunTime);
          ++i;
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [i].EndDateTime.Value);
          Assert.AreEqual (machine, operationSlots [i].Machine);
          Assert.AreEqual (3, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
          Assert.AreEqual (3, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
          Assert.AreEqual (operation3, operationSlots [i].Operation);
          Assert.AreEqual (TimeSpan.FromDays (1), operationSlots [i].Duration);
          Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
          ++i;
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), operationSlots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [i].EndDateTime.Value);
          Assert.AreEqual (machine, operationSlots [i].Machine);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
          Assert.AreEqual (component1, operationSlots [i].Component);
          Assert.AreEqual (operation2, operationSlots [i].Operation);
          Assert.AreEqual (TimeSpan.FromDays (6), operationSlots [i].Duration);
          Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].RunTime);
          ++i;
          Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 12), operationSlots [i].EndDateTime.Value);
          Assert.AreEqual (machine, operationSlots [i].Machine);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
          Assert.AreEqual (component1, operationSlots [i].Component);
          Assert.AreEqual (null, operationSlots [i].Operation);
          Assert.AreEqual (TimeSpan.FromDays (2), operationSlots [i].Duration);
          Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
          ++i;
          Assert.AreEqual (UtcDateTime.From (2011, 08, 12), operationSlots [i].BeginDateTime.Value);
          Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
          Assert.AreEqual (machine, operationSlots [i].Machine);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
          Assert.AreEqual (component1, operationSlots [i].Component);
          Assert.AreEqual (operation2, operationSlots [i].Operation);
          Assert.AreEqual (TimeSpan.FromSeconds (0), operationSlots [i].RunTime);
        }
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.AreEqual (12, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[1].AnalysisStatus, "2nd modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[2].AnalysisStatus, "3rd modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[3].AnalysisStatus, "4th modification status");
        Assert.AreEqual (AnalysisStatus.Done, modifications[4].AnalysisStatus, "5th modification status");
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with first an operation alone then the work order
    /// </summary>
    [Test]
    public void TestOperationThenWorkOrder()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IMachine machine = daoFactory.MachineDAO.FindById(4);
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
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
        IIntermediateWorkPiece intermediateWorkPiece1 = daoFactory.IntermediateWorkPieceDAO.FindById (13506);
        Assert.NotNull (intermediateWorkPiece1); // associated to operation1
        IIntermediateWorkPiece intermediateWorkPiece2 = daoFactory.IntermediateWorkPieceDAO.FindById (2);
        Assert.NotNull (intermediateWorkPiece2); // associated to operation2
        IIntermediateWorkPiece intermediateWorkPiece3 = daoFactory.IntermediateWorkPieceDAO.FindById (11003);
        Assert.NotNull (intermediateWorkPiece3); // associated to operation3
        IMachineObservationState attended =
          daoFactory.MachineObservationStateDAO.FindById((int) MachineObservationStateId.Attended);
        IMachineObservationState unattended =
          daoFactory.MachineObservationStateDAO.FindById((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = daoFactory.ReasonDAO.FindById(2);
        IReason reasonShort = daoFactory.ReasonDAO.FindById(3);
        IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
        IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
        IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
        IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
        IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);
        
        // Associate operations to another components
        // to avoid any automatic determination of component and work order
        component4.AddIntermediateWorkPiece (intermediateWorkPiece1);
        component4.AddIntermediateWorkPiece (intermediateWorkPiece2);
        component4.AddIntermediateWorkPiece (intermediateWorkPiece3);

        // Existing reason slots (motion only)
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 00, 00),
                                                  UtcDateTime.From (2011, 08, 02, 12, 00, 00)));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05), UtcDateTime.From (2011, 08, 07)));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attended;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          daoFactory.ReasonSlotDAO.MakePersistent(existingSlot);
        }

        // New association 3 -> oo: operation only
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03)));
          operationMachineAssociation.Operation = operation1;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        // New association 6 -> oo: work order only
        {
          IWorkOrderMachineAssociation association = ModelDAOHelper.ModelFactory
            .CreateWorkOrderMachineAssociation (machine, workOrder1, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 06)));
          association.WorkOrder = workOrder1;
          association.DateTime = UtcDateTime.From (2011, 08, 06);
          daoFactory.WorkOrderMachineAssociationDAO.MakePersistent(association);
        }
        
        // Run MakeAnalysis
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        // Check the values
        // - OperationSlots
        IList<OperationSlot> operationSlots =
          session.CreateCriteria<OperationSlot> ()
          .AddOrder (Order.Asc ("DateTimeRange"))
          .List<OperationSlot> ();
        Assert.AreEqual (2, operationSlots.Count, "Number of operation slots");
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 06), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (null, operationSlots [i].WorkOrder);
        Assert.AreEqual (operation1, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (3), operationSlots [i].Duration);
        Assert.AreEqual (TimeSpan.FromDays (1), operationSlots [i].RunTime);
        ++i;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 06), operationSlots [i].BeginDateTime.Value);
        Assert.IsFalse (operationSlots [i].EndDateTime.HasValue);
        Assert.AreEqual (machine, operationSlots [i].Machine);
        Assert.AreEqual (workOrder1, operationSlots [i].WorkOrder);
        Assert.AreEqual (operation1, operationSlots [i].Operation);
        Assert.AreEqual (TimeSpan.FromDays (1), operationSlots [i].RunTime);
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with a null operation
    /// to see if the work order and component are reseted
    /// in case they depend exclusively on the operation
    /// </summary>
    [Test]
    public void TestMakeAnalysisWithNullOperation()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        
        // Reference data
        IMachine machine = daoFactory.MachineDAO.FindById(1);
        Assert.NotNull (machine);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.NotNull (workOrder1);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.NotNull (workOrder2);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.NotNull (component1);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.NotNull (operation1);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.NotNull (operation2);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.NotNull (operation3);

        // Configuration
        Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly), true);
        Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly), true);
        
        // Existing operation slots
        {
          IOperationSlot existingOperationSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                             operation1,
                                                             component1,
                                                             workOrder1,
                                                             null, null, null, null,
                                                             R(1, null));
          daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
        }
        
        // New association 10 -> oo
        {
          IOperationMachineAssociation operationMachineAssociation =
            ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                           new UtcDateTimeRange (UtcDateTime.From (2011, 08, 10)));
          operationMachineAssociation.Operation = null;
          operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 08);
          daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
        }
        
        // Run MakeAnalysis
        AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
        
        // Check the values
        // - OperationSlots
        IList<OperationSlot> operationSlots =
          session.CreateCriteria<OperationSlot> ()
          .AddOrder (Order.Asc ("DateTimeRange"))
          .List<OperationSlot> ();
        Assert.AreEqual (1, operationSlots.Count, "Number of operation slots");
        int i = 0;
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 10), operationSlots [i].EndDateTime.Value);
        Assert.AreEqual (1, operationSlots [i].Machine.Id);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].WorkOrder).Id);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [i].Component).Id);
        Assert.AreEqual (operation1, operationSlots [i].Operation);
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.AreEqual (2, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
        
        transaction.Rollback ();
      }

      Lemoine.Info.ConfigSet.ResetForceValues ();
      Lemoine.Info.ConfigSet.ResetCache ();
    }

    /// <summary>
    /// Test the method MakeAnalysis with an association
    /// that corresponds to the existing slot
    /// 
    /// In this case, you should keep the operation cycle information
    /// without catching them again
    /// </summary>
    [Test]
    public void TestMakeAnalysisWithSameAssociation()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();

        try {
          Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.IntermediateWorkPieceSummary.IntermediateWorkPieceByMachineSummaryAccumulatorExtension));

          // Config
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int) OperationSlotSplitOption.None);
          
          // Reference data
          IMachine machine = daoFactory.MachineDAO.FindById(1);
          Assert.NotNull (machine);
          IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
          Assert.NotNull (workOrder1);
          IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
          Assert.NotNull (workOrder2);
          IComponent component1 = daoFactory.ComponentDAO.FindById (1);
          Assert.NotNull (component1);
          IOperation operation1 = daoFactory.OperationDAO.FindById (13157);
          Assert.NotNull (operation1);
          IOperation operation2 = daoFactory.OperationDAO.FindById (2);
          Assert.NotNull (operation2);
          IOperation operation3 = daoFactory.OperationDAO.FindById (11003);
          Assert.NotNull (operation3);
          
          // Existing operation slots
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation1,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(1, 2));
            ((OperationSlot)existingOperationSlot).TotalCycles = 12;
            ((OperationSlot)existingOperationSlot).PartialCycles = 2;
            daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
          }
          {
            IOperationSlot existingOperationSlot =
              ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                               operation2,
                                                               component1,
                                                               workOrder1,
                                                               null, null, null, null,
                                                               R(2, 3));
            daoFactory.OperationSlotDAO.MakePersistent(existingOperationSlot);
          }
          
          // New association 1 -> 2
          {
            IOperationMachineAssociation operationMachineAssociation =
              ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                             UtcDateTime.From (2011, 08, 01));
            operationMachineAssociation.Operation = operation1;
            operationMachineAssociation.DateTime = UtcDateTime.From (2011, 08, 04);
            operationMachineAssociation.End = UtcDateTime.From (2011, 08, 02);
            daoFactory.OperationMachineAssociationDAO.MakePersistent(operationMachineAssociation);
          }
          
          // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<OperationMachineAssociation> ();
          DAOFactory.EmptyAccumulators ();

          // Check the values
          // - OperationSlots
          IList<OperationSlot> operationSlots =
            session.CreateCriteria<OperationSlot> ()
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<OperationSlot> ();
          Assert.AreEqual (2, operationSlots.Count, "Number of operation slots");
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), operationSlots [0].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [0].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [0].Machine.Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [0].WorkOrder).Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [0].Component).Id);
          Assert.AreEqual (operation1, operationSlots [0].Operation);
          Assert.AreEqual (12, operationSlots [0].TotalCycles);
          Assert.AreEqual (2, operationSlots [0].PartialCycles);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), operationSlots [1].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), operationSlots [1].EndDateTime.Value);
          Assert.AreEqual (1, operationSlots [1].Machine.Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [1].WorkOrder).Id);
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)operationSlots [1].Component).Id);
          Assert.AreEqual (operation2, operationSlots [1].Operation);
          CheckSummaries (operationSlots[0]);
          CheckSummaries (operationSlots[1]);
          // - Modifications
          IList<OperationMachineAssociation> modifications =
            session.CreateCriteria<OperationMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<OperationMachineAssociation> ();
          Assert.AreEqual (1, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
        }
        finally {
          transaction.Rollback ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        }
      }
    }

    void CheckSummaries (IOperationSlot operationSlot)
    {
      {
        if (operationSlot.Day.HasValue) {
          var summary = new CycleCountSummaryDAO ()
            .FindByKey (operationSlot.Machine,
            operationSlot.Day.Value,
            operationSlot.Shift,
            operationSlot.WorkOrder,
            operationSlot.Line,
            operationSlot.Task,
            operationSlot.Component,
            operationSlot.Operation);
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindByDayShift (operationSlot.Machine, operationSlot.Day.Value, operationSlot.Shift)
            .Where (s => WorkOrder.Equals (operationSlot.WorkOrder, s.WorkOrder))
            .Where (s => Line.Equals (operationSlot.Line, s.Line))
            .Where (s => Task.Equals (operationSlot.Task, s.Task))
            .Where (s => Component.Equals (operationSlot.Component, s.Component))
            .Where (s => Operation.Equals (operationSlot.Operation, s.Operation));
          var total = operationSlots
            .Sum (s => s.TotalCycles);
          var partial = operationSlots
            .Sum (s => s.PartialCycles);
          if (null == summary) {
            Assert.AreEqual (0, total);
            Assert.AreEqual (0, partial);
          }
          else {
            Assert.AreEqual (summary.Full, total);
            Assert.AreEqual (summary.Partial, partial);
          }
        }
      }

      {
        int counted1 = 0;
        int corrected1 = 0;
        int counted2 = 0;
        int corrected2 = 0;
        foreach (IIntermediateWorkPiece intermediateWorkPiece in operationSlot.Operation.IntermediateWorkPieces) {
          {
            var summary =
              new IntermediateWorkPieceByMachineSummaryDAO ()
              .FindByKey (operationSlot.Machine,
                          intermediateWorkPiece,
                          operationSlot.Component,
                          operationSlot.WorkOrder,
                          operationSlot.Line,
                          operationSlot.Task,
                          operationSlot.Day,
                          operationSlot.Shift);
            if (null != summary) {
              counted1 += summary.Counted / intermediateWorkPiece.OperationQuantity;
              corrected1 += summary.Corrected / intermediateWorkPiece.OperationQuantity;
            }
          }
          {
            IIntermediateWorkPieceSummary summary =
              new IntermediateWorkPieceSummaryDAO ()
              .FindByKey (intermediateWorkPiece,
                          operationSlot.Component,
                          operationSlot.WorkOrder,
                          operationSlot.Line,
                          operationSlot.Day,
                          operationSlot.Shift);
            if (null != summary) {
              counted2 += summary.Counted / intermediateWorkPiece.OperationQuantity;
              corrected2 += summary.Corrected / intermediateWorkPiece.OperationQuantity;
            }
          }
        }
        // Because there may be other operaiton slots that target the same summary,
        // there is a LessOrEqual
        Assert.LessOrEqual (operationSlot.TotalCycles, counted1);
        Assert.LessOrEqual (operationSlot.TotalCycles, corrected1);
        Assert.LessOrEqual (operationSlot.TotalCycles, counted2);
        Assert.LessOrEqual (operationSlot.TotalCycles, corrected2);
      }
    }
  }
}
