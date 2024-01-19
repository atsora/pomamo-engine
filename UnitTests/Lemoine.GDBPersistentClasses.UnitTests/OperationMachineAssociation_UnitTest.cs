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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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

        Assert.That (operationSlots, Has.Count.EqualTo (3), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].Operation, Is.EqualTo (operation3));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[1].Operation, Is.EqualTo (operation2));
        });
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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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

        Assert.That (operationSlots0, Has.Count.EqualTo (3), "Number of operation slots (1)");
        
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

        Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].Operation, Is.EqualTo (operation1));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.HasValue, Is.False); // current slot extended to infinity
          Assert.That (operationSlots[1].Operation, Is.EqualTo (operation3));
        });
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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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

        Assert.That (operationSlots0, Has.Count.EqualTo (3), "Number of operation slots (1)");
        
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

        Assert.That (operationSlots, Has.Count.EqualTo (4), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].Operation, Is.EqualTo (operation1));
          Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo ((UtcDateTime.From (2011, 08, 02)).AddHours (1)));
          Assert.That (operationSlots[1].Operation, Is.EqualTo (operation3));
          Assert.That (operationSlots[2].BeginDateTime.Value, Is.EqualTo ((UtcDateTime.From (2011, 08, 02)).AddHours (1)));
          Assert.That (operationSlots[2].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[2].Operation, Is.EqualTo (operation2));
          Assert.That (operationSlots[3].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[3].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          Assert.That (operationSlots[3].Operation, Is.EqualTo (null));
        });
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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots0, Has.Count.EqualTo (1), "Number of operation slots (1)");
        
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

        Assert.That (operationSlots, Has.Count.EqualTo (1), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.HasValue, Is.False);
          Assert.That (operationSlots[0].Operation, Is.EqualTo (operation1));
        });
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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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
                                                           null,
                                                           null,
                                                           workOrder1,
                                                           null, null, null, null,
                                                           R(1, 2));
        opSlot1.RunTime = TimeSpan.FromHours (12);
        
        daoFactory.OperationSlotDAO.MakePersistent(opSlot1);
        
        IList<IOperationSlot> operationSlots0 =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindAll(machine);

        Assert.That (operationSlots0, Has.Count.EqualTo (1), "Number of operation slots (1)");
        
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

        Assert.That (operationSlots, Has.Count.EqualTo (1), "Number of operation slots (2)");
        Assert.Multiple (() => {
          Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
          Assert.That (operationSlots[0].Operation, Is.EqualTo (null));
        });

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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IComponent component4 = daoFactory.ComponentDAO.FindById(4);
        Assert.That (component4, Is.Not.Null);
        IIntermediateWorkPiece intermediateWorkPiece1 = daoFactory.IntermediateWorkPieceDAO.FindById (13506);
        Assert.That (intermediateWorkPiece1, Is.Not.Null); // associated to operation1
        IIntermediateWorkPiece intermediateWorkPiece2 = daoFactory.IntermediateWorkPieceDAO.FindById (2);
        Assert.That (intermediateWorkPiece2, Is.Not.Null); // associated to operation2
        IIntermediateWorkPiece intermediateWorkPiece3 = daoFactory.IntermediateWorkPieceDAO.FindById (11003);
        Assert.That (intermediateWorkPiece3, Is.Not.Null); // associated to operation3
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null);
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
        Assert.That (operationSlots, Has.Count.EqualTo (6), "Number of operation slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].Component).Id, Is.EqualTo (1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (2)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromHours (24)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (null));
          Assert.That (operationSlots[i].Component, Is.EqualTo (null));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (1)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 08)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (null));
          Assert.That (operationSlots[i].Component, Is.EqualTo (null));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (4)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromDays (2)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 08)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
          Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (2)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 12)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
          Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (2)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 12)));
          Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
          Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
        });
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.That (modifications, Has.Count.EqualTo (12), "Number of modifications");
        Assert.Multiple (() => {
          Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
          Assert.That (modifications[1].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "2nd modification status");
          Assert.That (modifications[2].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "3rd modification status");
          Assert.That (modifications[3].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "4th modification status");
          Assert.That (modifications[4].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "5th modification status");
        });

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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null); // => workOrder1
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null); // => workOrder1
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null); // => workOrder3
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
          Assert.That (operationSlots, Has.Count.EqualTo (5), "Number of operation slots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].Component).Id, Is.EqualTo (1));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
            Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (2)));
            Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromHours (24)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (3));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].Component).Id, Is.EqualTo (3));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation3));
            Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (1)));
            Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
            Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (6)));
            Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromDays (2)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
            Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 12)));
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (null));
            Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (2)));
            Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 12)));
            Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
            Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (operationSlots[i].Component, Is.EqualTo (component1));
            Assert.That (operationSlots[i].Operation, Is.EqualTo (operation2));
            Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromSeconds (0)));
          });
        }
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.That (modifications, Has.Count.EqualTo (12), "Number of modifications");
        Assert.Multiple (() => {
          Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
          Assert.That (modifications[1].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "2nd modification status");
          Assert.That (modifications[2].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "3rd modification status");
          Assert.That (modifications[3].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "4th modification status");
          Assert.That (modifications[4].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "5th modification status");
        });

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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
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
        IIntermediateWorkPiece intermediateWorkPiece1 = daoFactory.IntermediateWorkPieceDAO.FindById (13506);
        Assert.That (intermediateWorkPiece1, Is.Not.Null); // associated to operation1
        IIntermediateWorkPiece intermediateWorkPiece2 = daoFactory.IntermediateWorkPieceDAO.FindById (2);
        Assert.That (intermediateWorkPiece2, Is.Not.Null); // associated to operation2
        IIntermediateWorkPiece intermediateWorkPiece3 = daoFactory.IntermediateWorkPieceDAO.FindById (11003);
        Assert.That (intermediateWorkPiece3, Is.Not.Null); // associated to operation3
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
        Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (null));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          Assert.That (operationSlots[i].Duration, Is.EqualTo (TimeSpan.FromDays (3)));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromDays (1)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
          Assert.That (operationSlots[i].EndDateTime.HasValue, Is.False);
          Assert.That (operationSlots[i].Machine, Is.EqualTo (machine));
          Assert.That (operationSlots[i].WorkOrder, Is.EqualTo (workOrder1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
          Assert.That (operationSlots[i].RunTime, Is.EqualTo (TimeSpan.FromDays (1)));
        });

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
        Assert.That (machine, Is.Not.Null);
        IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
        Assert.That (workOrder1, Is.Not.Null);
        IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
        Assert.That (workOrder2, Is.Not.Null);
        IComponent component1 = daoFactory.ComponentDAO.FindById(1);
        Assert.That (component1, Is.Not.Null);
        IOperation operation1 = daoFactory.OperationDAO.FindById(13157);
        Assert.That (operation1, Is.Not.Null);
        IOperation operation2 = daoFactory.OperationDAO.FindById(2);
        Assert.That (operation2, Is.Not.Null);
        IOperation operation3 = daoFactory.OperationDAO.FindById(11003);
        Assert.That (operation3, Is.Not.Null);

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
        Assert.That (operationSlots, Has.Count.EqualTo (1), "Number of operation slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (operationSlots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (operationSlots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 10)));
          Assert.That (operationSlots[i].Machine.Id, Is.EqualTo (1));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].WorkOrder).Id, Is.EqualTo (1));
          Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[i].Component).Id, Is.EqualTo (1));
          Assert.That (operationSlots[i].Operation, Is.EqualTo (operation1));
        });
        // - Modifications
        IList<OperationMachineAssociation> modifications =
          session.CreateCriteria<OperationMachineAssociation> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<OperationMachineAssociation> ();
        Assert.That (modifications, Has.Count.EqualTo (2), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        
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
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = daoFactory.WorkOrderDAO.FindById(1);
          Assert.That (workOrder1, Is.Not.Null);
          IWorkOrder workOrder2 = daoFactory.WorkOrderDAO.FindById(2);
          Assert.That (workOrder2, Is.Not.Null);
          IComponent component1 = daoFactory.ComponentDAO.FindById (1);
          Assert.That (component1, Is.Not.Null);
          IOperation operation1 = daoFactory.OperationDAO.FindById (13157);
          Assert.That (operation1, Is.Not.Null);
          IOperation operation2 = daoFactory.OperationDAO.FindById (2);
          Assert.That (operation2, Is.Not.Null);
          IOperation operation3 = daoFactory.OperationDAO.FindById (11003);
          Assert.That (operation3, Is.Not.Null);
          
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
          Assert.That (operationSlots, Has.Count.EqualTo (2), "Number of operation slots");
          Assert.Multiple (() => {
            Assert.That (operationSlots[0].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (operationSlots[0].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (operationSlots[0].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[0].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[0].Component).Id, Is.EqualTo (1));
            Assert.That (operationSlots[0].Operation, Is.EqualTo (operation1));
            Assert.That (operationSlots[0].TotalCycles, Is.EqualTo (12));
            Assert.That (operationSlots[0].PartialCycles, Is.EqualTo (2));
            Assert.That (operationSlots[1].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (operationSlots[1].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (operationSlots[1].Machine.Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[1].WorkOrder).Id, Is.EqualTo (1));
            Assert.That (((Lemoine.Collections.IDataWithId)operationSlots[1].Component).Id, Is.EqualTo (1));
            Assert.That (operationSlots[1].Operation, Is.EqualTo (operation2));
          });
          CheckSummaries (operationSlots[0]);
          CheckSummaries (operationSlots[1]);
          // - Modifications
          IList<OperationMachineAssociation> modifications =
            session.CreateCriteria<OperationMachineAssociation> ()
            .AddOrder (Order.Asc ("DateTime"))
            .List<OperationMachineAssociation> ();
          Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
          Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
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
            Assert.Multiple (() => {
              Assert.That (total, Is.EqualTo (0));
              Assert.That (partial, Is.EqualTo (0));
            });
          }
          else {
            Assert.Multiple (() => {
              Assert.That (total, Is.EqualTo (summary.Full));
              Assert.That (partial, Is.EqualTo (summary.Partial));
            });
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
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (counted1));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (corrected1));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (counted2));
        Assert.That (operationSlot.TotalCycles, Is.LessThanOrEqualTo (corrected2));
      }
    }
  }
}
