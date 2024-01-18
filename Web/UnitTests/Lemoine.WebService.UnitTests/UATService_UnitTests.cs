// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.GDBPersistentClasses;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.DTO;
using Lemoine.UnitTests;
using Lemoine.WebService;

using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Business.Config;

namespace Lemoine.WebService.UnitTests
{
  /// <summary>
  /// Unit tests for Lemoine.WebService.UATService
  /// </summary>
  [TestFixture]
  public class UATService_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UATService_UnitTests).FullName);
    private IMonitoredMachine machine1;
    private IMonitoredMachine machine2;
    private IMachineModule machineModule1;
    private IComponent component1;
    private IComponent component2;
    private IOperation operation1;
    private IOperation operation2;
    private IOperation operation3;
    private IPart part1;
    private IWorkOrder workOrder1;
    private IWorkOrder workOrder2;
    
    private IProject project1;
    private IJob job1;
    
    // compute timespan between "java" origin of time and datetime
    private TimeSpan ComputeJavaOffsetFromDateTime(DateTime dt) {
      return dt - new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
    }
    
    private void TestInit() {
      machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(1);
      machine2 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(2);
      machineModule1 = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById(1);
      component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById(1);
      component2 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById(2);
      operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById(1);
      operation2 = ModelDAOHelper.DAOFactory.OperationDAO.FindById(2);
      operation3 = ModelDAOHelper.DAOFactory.OperationDAO.FindById(11003);
      workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById(1);
      workOrder2 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById(2);
      part1 = component1.Part;
      
      project1 = ModelDAOHelper.DAOFactory.ProjectDAO.FindById(1);
      job1 = project1.Job;

      Assert.Multiple (() => {
        Assert.That (machine1, Is.Not.Null);
        Assert.That (machine2, Is.Not.Null);
        Assert.That (machineModule1, Is.Not.Null);
      });
      Assert.Multiple (() => {
        Assert.That (machine1, Is.EqualTo (machineModule1.MonitoredMachine), "Machine module not OK");
        Assert.That (component1, Is.Not.Null);
        Assert.That (component2, Is.Not.Null);
        Assert.That (operation1, Is.Not.Null);
        Assert.That (operation2, Is.Not.Null);
        Assert.That (operation3, Is.Not.Null);
        Assert.That (part1, Is.Not.Null);
        Assert.That (workOrder1, Is.Not.Null);
        Assert.That (workOrder2, Is.Not.Null);
      });
    }
    
    #region TestGetLastCycleWithSerialNumberSuccess
    /// <summary>
    /// Test GetLastCycleWithSerialNumber service of UATService: successful case
    /// </summary>
    [Test]
    public void TestGetLastCycleWithSerialNumberSuccess()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow.AddSeconds(-500);
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, null, null, null, null,  null, null,
                                                                                    new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          firstCycle.Begin = currentDate.AddSeconds(1);
          IOperationCycle lastCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          lastCycle.OperationSlot = lastSlot;
          lastCycle.Begin = currentDate.AddSeconds(2); // changed to true begin
          
          IDeliverablePiece deliverablePiece = ModelDAOHelper.ModelFactory.CreateDeliverablePiece(serialNumber);
          deliverablePiece.Component = component1;
          IOperationCycleDeliverablePiece ocdp = ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece, lastCycle);
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(lastCycle);
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
          
          GetLastCycleWithSerialNumberV2 getLastCycleWithSerialNumber = new GetLastCycleWithSerialNumberV2();
          getLastCycleWithSerialNumber.Id = machine1.Id; // monitored machine
          
          LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberDTO = new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
            as LastCycleWithSerialNumberV2DTO;

          Assert.That (lastCycleWithSerialNumberDTO, Is.Not.Null, "GetLastCycleWithSerialNumber is null");
          Assert.Multiple (() => {
            Assert.That (lastCycleWithSerialNumberDTO.CycleId, Is.EqualTo (lastCycle.Id), "Bad cycle id");
            Assert.That (lastCycleWithSerialNumberDTO.EstimatedBegin, Is.EqualTo (false), "Bad cycle estimated begin status");
            Assert.That (lastCycleWithSerialNumberDTO.EstimatedEnd, Is.EqualTo (false), "Bad cycle estimated end status");
            Assert.That (lastCycleWithSerialNumberDTO.SerialNumber, Is.EqualTo (serialNumber), "Bad serial number arg");
            Assert.That (lastCycleWithSerialNumberDTO.DataMissing, Is.EqualTo (true), "Bad data missing arg");
          });

          string serialNumber2 = "S2345";
          IDeliverablePiece deliverablePiece2 = ModelDAOHelper.ModelFactory.CreateDeliverablePiece(serialNumber2);
          deliverablePiece2.Component = component1;
          IOperationCycleDeliverablePiece ocdp2 = ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece2, firstCycle);
          
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece2);
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent(ocdp2);
          
          LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberDTO2 =
            new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
            as LastCycleWithSerialNumberV2DTO;

          Assert.That (lastCycleWithSerialNumberDTO2, Is.Not.Null, "GetLastCycleWithSerialNumber is null (2)");
          Assert.Multiple (() => {
            Assert.That (lastCycleWithSerialNumberDTO2.CycleId, Is.EqualTo (lastCycle.Id), "Bad cycle id");
            Assert.That (lastCycleWithSerialNumberDTO2.SerialNumber, Is.EqualTo (serialNumber), "Bad serial number arg (2)");
            Assert.That (lastCycleWithSerialNumberDTO2.DataMissing, Is.EqualTo (false), "Bad data missing arg (2)");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastCycleWithSerialNumberSuccessWithBegin
    /// <summary>
    /// Test GetLastCycleWithSerialNumber service of UATService: explicit begin
    /// </summary>
    [Test]
    public void TestGetLastCycleWithSerialNumberSuccessWithBegin()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, null, null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          firstCycle.Begin = currentDate.Subtract(TimeSpan.FromHours(40));
          IOperationCycle lastCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          lastCycle.OperationSlot = lastSlot;
          lastCycle.Begin = currentDate.Subtract(TimeSpan.FromMinutes(20));
          IDeliverablePiece deliverablePiece = ModelDAOHelper.ModelFactory.CreateDeliverablePiece(serialNumber);
          deliverablePiece.Component = component1;
          IOperationCycleDeliverablePiece ocdp = ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece, lastCycle);
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(lastCycle);
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
          
          GetLastCycleWithSerialNumberV2 getLastCycleWithSerialNumber = new GetLastCycleWithSerialNumberV2();
          getLastCycleWithSerialNumber.Id = machine1.Id; // monitored machine
          getLastCycleWithSerialNumber.Begin = ConvertDTO.DateTimeUtcToIsoString (currentDate.Subtract(TimeSpan.FromHours(15)));
          
          LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberDTO =
            new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
            as LastCycleWithSerialNumberV2DTO;

          Assert.That (lastCycleWithSerialNumberDTO, Is.Not.Null, "GetLastCycleWithSerialNumber is null");
          Assert.Multiple (() => {
            Assert.That (lastCycleWithSerialNumberDTO.CycleId, Is.EqualTo (lastCycle.Id), "Bad cycle id");
            Assert.That (lastCycleWithSerialNumberDTO.SerialNumber, Is.EqualTo (serialNumber), "Bad serial number arg");
            Assert.That (lastCycleWithSerialNumberDTO.DataMissing, Is.EqualTo (false), "Bad data missing arg (1)");
          });


          // go back in time to fetch missing serial on firstCycle
          getLastCycleWithSerialNumber.Begin = ConvertDTO.DateTimeUtcToIsoString(currentDate.Subtract(TimeSpan.FromHours(40)));
          lastCycleWithSerialNumberDTO =
            new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
            as LastCycleWithSerialNumberV2DTO;

          Assert.That (lastCycleWithSerialNumberDTO, Is.Not.Null, "GetLastCycleWithSerialNumber is null");
          Assert.Multiple (() => {
            Assert.That (lastCycleWithSerialNumberDTO.CycleId, Is.EqualTo (lastCycle.Id), "Bad cycle id");
            Assert.That (lastCycleWithSerialNumberDTO.SerialNumber, Is.EqualTo (serialNumber), "Bad serial number arg");
            Assert.That (lastCycleWithSerialNumberDTO.DataMissing, Is.EqualTo (true), "Bad data missing arg (2)");
          });


        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastCycleWithSerialNumberNoSerialNumber
    /// <summary>
    /// Test GetLastCycleWithSerialNumber service of UATService: no serial number case
    /// </summary>
    [Test]
    public void TestGetLastCycleWithSerialNumberNoSerialNumber()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, null, null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          firstCycle.Begin = currentDate.AddSeconds(1);
          IOperationCycle lastCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          lastCycle.OperationSlot = lastSlot;
          lastCycle.Begin = currentDate.AddSeconds(2);

          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(lastCycle);
          
          GetLastCycleWithSerialNumberV2 getLastSerialNumber = new GetLastCycleWithSerialNumberV2();
          getLastSerialNumber.Id = machine1.Id; // monitored machine
          
          LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberDTO =
            new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastSerialNumber)
            as LastCycleWithSerialNumberV2DTO;

          Assert.That (lastCycleWithSerialNumberDTO, Is.Not.Null, "GetLastCycleWithSerialNumber is null");
          Assert.Multiple (() => {
            Assert.That (lastCycleWithSerialNumberDTO.SerialNumber, Is.EqualTo ("0"), "Bad serial number arg");
            Assert.That (lastCycleWithSerialNumberDTO.DataMissing, Is.EqualTo (true), "Bad data missing arg");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastCycleWithSerialNumberNoCycle
    /// <summary>
    /// Test GetLastCycleWithSerialNumber service of UATService: no cycle case
    /// </summary>
    [Test]
    public void TestGetLastCycleWithSerialNumberNoCycle()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        TestInit();
        GetLastCycleWithSerialNumberV2 getLastCycleWithSerialNumber = new GetLastCycleWithSerialNumberV2();
        getLastCycleWithSerialNumber.Id = machine1.Id; // monitored machine
        
        LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberDTO =
          new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
          as LastCycleWithSerialNumberV2DTO;

        Assert.That (lastCycleWithSerialNumberDTO, Is.Not.Null, "GetLastCycleWithSerialNumber is null");
        Assert.Multiple (() => {
          Assert.That (lastCycleWithSerialNumberDTO.SerialNumber, Is.EqualTo ("-1"), "Bad serial number arg");
          Assert.That (lastCycleWithSerialNumberDTO.DataMissing, Is.EqualTo (false), "Bad data missing arg");
        });
      }
    }
    #endregion

    #region TestGetLastCycleWithSerialNumberFailure
    /// <summary>
    /// Test GetLastCycleWithSerialNumber service of UATService: failure case
    /// </summary>
    [Test]
    public void TestGetLastCycleWithSerialNumberFailure()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetLastCycleWithSerialNumberV2 getLastSerialNumber = new GetLastCycleWithSerialNumberV2();
          getLastSerialNumber.Id = 9999999;
          
          ErrorDTO response = new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastSerialNumber) as ErrorDTO;

          Assert.That (response, Is.Not.Null, "No error dto");
          Assert.That (response.ErrorMessage, Does.StartWith ("No monitored machine with id"), "Bad error msg");
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetCyclesInCurrentPeriod
    /// <summary>
    /// Test GetCyclesInCurrentPeriod service of UATService : success case
    /// </summary>
    [Test]
    public void TestGetCyclesInCurrentPeriodSuccess()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, null, null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          DateTime firstCycleBegin = currentDate.AddSeconds(-10);
          DateTime firstCycleEnd = currentDate.AddSeconds(-8);
          DateTime lastCycleBegin = currentDate.AddSeconds(-5);
          firstCycle.Begin = firstCycleBegin;
          firstCycle.SetEstimatedEnd(firstCycleEnd);
          IOperationCycle lastCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          lastCycle.OperationSlot = lastSlot;
          lastCycle.Begin = lastCycleBegin;
          
          IDeliverablePiece deliverablePiece = ModelDAOHelper.ModelFactory.CreateDeliverablePiece(serialNumber);
          deliverablePiece.Component = component1;
          IOperationCycleDeliverablePiece ocdp = ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece, lastCycle);
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(lastCycle);
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
          
          GetCyclesWithWorkInformationsInPeriodV2 getCyclesInCurrentPeriod = new GetCyclesWithWorkInformationsInPeriodV2();
          getCyclesInCurrentPeriod.Id = machine1.Id; // monitored machine
          
          CyclesWithWorkInformationsInPeriodV2DTO cyclesInPeriodDTO = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;

          Assert.That (cyclesInPeriodDTO, Is.Not.Null, "GetCyclesInPeriod returns null");
          Assert.That (cyclesInPeriodDTO.List, Is.Not.Null, "GetCyclesInPeriod returns null list");
          Assert.That (cyclesInPeriodDTO.List, Has.Count.EqualTo (2), "GetCyclesInPeriod returns bad number of cycle DTOs");
          CycleWithWorkInformationsV2DTO cycleDTO1 = cyclesInPeriodDTO.List[0];
          CycleWithWorkInformationsV2DTO cycleDTO2 = cyclesInPeriodDTO.List[1];

          Assert.Multiple (() => {
            // reverse order
            Assert.That (cycleDTO1.CycleId, Is.EqualTo (lastCycle.Id), "bad cycle id (1)");
            Assert.That (cycleDTO2.CycleId, Is.EqualTo (firstCycle.Id), "bad cycle id (2)");

            Assert.That (cycleDTO1.EstimatedBegin, Is.EqualTo (false), "bad 2nd cycle estimated begin");
            Assert.That (cycleDTO1.EstimatedEnd, Is.EqualTo (false), "bad 2nd cycle estimated end");

            Assert.That (cycleDTO2.EstimatedBegin, Is.EqualTo (false), "bad first cycle estimated begin");
            Assert.That (cycleDTO2.EstimatedEnd, Is.EqualTo (true), "bad first cycle estimated end");

            Assert.That (cycleDTO1.SerialNumber, Is.EqualTo (serialNumber), "Bad Serial Number (1)");
            Assert.That (cycleDTO2.SerialNumber, Is.EqualTo (null), "Bad Serial Number (2)");

            Assert.That (cyclesInPeriodDTO.Begin, Is.Not.Null, "null begin"); // exact values are difficult to tell
            Assert.That (cyclesInPeriodDTO.End, Is.Not.Null, "null end");

            Assert.That (cycleDTO2.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleBegin)), "Bad begin for cycle (1)");
            Assert.That (cycleDTO2.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleEnd)), "Bad end for cycle (1)");
            Assert.That (cycleDTO1.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (lastCycleBegin)), "Bad begin for cycle (2)");
            Assert.That (cycleDTO1.End, Is.EqualTo (""), "Bad end for cycle (2)");
          });

          getCyclesInCurrentPeriod.Begin = ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin);
          getCyclesInCurrentPeriod.End = ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd);
          CyclesWithWorkInformationsInPeriodV2DTO cyclesInPeriodDTO2 = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;
          Assert.That (cyclesInPeriodDTO2, Is.Not.Null, "GetCyclesInPeriod returns null (2)");
          Assert.That (cyclesInPeriodDTO2.List, Is.Not.Null, "GetCyclesInPeriod returns null list (2)");
          Assert.That (cyclesInPeriodDTO2.List, Has.Count.EqualTo (1), "GetCyclesInPeriod returns bad number of cycle DTOs (2)");
          CycleWithWorkInformationsV2DTO cycleDTO3 = cyclesInPeriodDTO2.List[0];
          Assert.Multiple (() => {
            Assert.That (cycleDTO3.SerialNumber, Is.EqualTo (null), "Bad Serial Number (3)");
            Assert.That (cyclesInPeriodDTO2.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleBegin)), "Bad begin for cycle (3)");
            Assert.That (cyclesInPeriodDTO2.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleEnd)), "Bad begin for cycle (3)");
            Assert.That (cycleDTO3.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleBegin)), "Bad begin for cycle (3)");
            Assert.That (cycleDTO3.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleEnd)), "Bad begin for cycle (3)");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    
    /// <summary>
    /// Test GetCyclesInCurrentPeriod service of UATService: explicit begin/end
    /// </summary>
    [Test]
    public void TestGetCyclesInCurrentPeriodWithBegin()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, null, null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          DateTime firstCycleBegin = currentDate.AddSeconds(-10);
          DateTime firstCycleEnd = currentDate.AddSeconds(-8);
          DateTime lastCycleBegin = currentDate.AddSeconds(-5);
          firstCycle.Begin = firstCycleBegin;
          firstCycle.SetRealEnd(firstCycleEnd);
          IOperationCycle lastCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          lastCycle.OperationSlot = lastSlot;
          lastCycle.Begin = lastCycleBegin;
          IDeliverablePiece deliverablePiece = ModelDAOHelper.ModelFactory.CreateDeliverablePiece(serialNumber);
          deliverablePiece.Component = component1;
          IOperationCycleDeliverablePiece ocdp = ModelDAOHelper.ModelFactory.CreateOperationCycleDeliverablePiece(deliverablePiece, lastCycle);
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(lastCycle);
          ModelDAOHelper.DAOFactory.DeliverablePieceDAO.MakePersistent(deliverablePiece);
          ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent(ocdp);
          
          GetCyclesWithWorkInformationsInPeriodV2 getCyclesInCurrentPeriod = new GetCyclesWithWorkInformationsInPeriodV2();
          getCyclesInCurrentPeriod.Id = machine1.Id; // monitored machine
          getCyclesInCurrentPeriod.Begin = ConvertDTO.DateTimeUtcToIsoString(currentDate.AddSeconds(-5));
          
          CyclesWithWorkInformationsInPeriodV2DTO cyclesInPeriodDTO = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;

          Assert.That (cyclesInPeriodDTO, Is.Not.Null, "GetCyclesInPeriod returns null");
          Assert.That (cyclesInPeriodDTO.List, Is.Not.Null, "GetCyclesInPeriod returns null list");
          Assert.That (cyclesInPeriodDTO.List, Has.Count.EqualTo (1), "GetCyclesInPeriod returns bad number of cycle DTOs");
          CycleWithWorkInformationsV2DTO cycleDTO2 = cyclesInPeriodDTO.List[0];

          Assert.Multiple (() => {
            Assert.That (cycleDTO2.SerialNumber, Is.EqualTo (serialNumber), "Bad Serial Number");

            Assert.That (cycleDTO2.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (lastCycleBegin)), "Bad begin for cycle");
            Assert.That (cycleDTO2.End, Is.EqualTo (""), "Bad end for cycle");
          });

          getCyclesInCurrentPeriod.Begin = ConvertDTO.DateTimeUtcToIsoString(currentDate.AddSeconds(-10));
          getCyclesInCurrentPeriod.End = ConvertDTO.DateTimeUtcToIsoString(currentDate.AddSeconds(-9));
          
          cyclesInPeriodDTO = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;

          Assert.That (cyclesInPeriodDTO, Is.Not.Null, "GetCyclesInPeriod returns null (2)");
          Assert.That (cyclesInPeriodDTO.List, Is.Not.Null, "GetCyclesInPeriod returns null list");
          Assert.That (cyclesInPeriodDTO.List, Has.Count.EqualTo (1), "GetCyclesInPeriod returns bad number of cycle DTOs (2)");
          CycleWithWorkInformationsV2DTO cycleDTO1 = cyclesInPeriodDTO.List[0];

          Assert.Multiple (() => {
            Assert.That (cycleDTO1.SerialNumber, Is.EqualTo (null), "Bad Serial Number (2)");

            Assert.That (cycleDTO1.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleBegin)), "Bad begin for cycle (2)");
            Assert.That (cycleDTO1.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstCycleEnd)), "Bad end for cycle (2)");
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    
    /// <summary>
    /// Test GetCyclesInCurrentPeriod service of UATService : failure case
    /// </summary>
    [Test]
    public void TestGetCyclesInCurrentPeriodFailure()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetCyclesWithWorkInformationsInPeriodV2 getCyclesInCurrentPeriod = new GetCyclesWithWorkInformationsInPeriodV2();
          getCyclesInCurrentPeriod.Id = 9999999;
          
          ErrorDTO response = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache(getCyclesInCurrentPeriod) as ErrorDTO;

          Assert.That (response, Is.Not.Null, "No error dto");
          Assert.That (response.ErrorMessage, Does.StartWith ("No monitored machine with id"), "Bad error msg");
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestSaveSerialNumberV4Success1
    /// <summary>
    /// Test SaveSerialNumberV4 service of UATService : success case with datetime set to end of operation
    /// </summary>
    [Test]
    public void TestSaveSerialNumberV4Success1()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, component1,null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          firstCycle.Begin = currentDate.AddSeconds(1);
          firstCycle.SetRealEnd(currentDate.AddSeconds(2));
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          
          SaveSerialNumberV5 saveSerialNumber = new SaveSerialNumberV5();
          saveSerialNumber.MachineId = machine1.Id;
          saveSerialNumber.DateTime = ConvertDTO.DateTimeUtcToIsoString(firstCycle.End.Value);
          saveSerialNumber.IsBegin = false;
          saveSerialNumber.SerialNumber = serialNumber;
          
          object saveSerialNumberResponse = new SaveSerialNumberV5Service ().GetSync (saveSerialNumber);

          Assert.That (saveSerialNumberResponse as OkDTO, Is.Not.Null, "Non-OK saveSerialNumberResponse");
          
          ModelDAOHelper.DAOFactory.Flush ();
          
          Lemoine.Analysis.PendingGlobalMachineModificationAnalysis pendingModificationAnalysis =
            new Lemoine.Analysis.PendingGlobalMachineModificationAnalysis (false);

          pendingModificationAnalysis.MakeAnalysis(System.Threading.CancellationToken.None, currentDate.AddSeconds(10), 0, 0);
          
          IList<IOperationCycleDeliverablePiece> ocdpList = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(firstCycle);
          Assert.That (ocdpList, Is.Not.Null, "No associated serial number");
          Assert.That (ocdpList, Has.Count.EqualTo (1), "No single associated serial number");
          IDeliverablePiece deliverablePiece = ocdpList[0].DeliverablePiece;
          Assert.That (deliverablePiece, Is.Not.Null, "Null serial number");
          Assert.Multiple (() => {
            Assert.That (deliverablePiece.Code, Is.EqualTo (serialNumber), "Bad serial number");
            Assert.That (deliverablePiece.Component, Is.EqualTo (component1), "Bad component");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion
    
    #region TestSaveSerialNumberV4Success1
    /// <summary>
    /// Test SaveSerialNumberV4 service of UATService : success case with datetime set to begin of operation
    /// </summary>
    [Test]
    public void TestSaveSerialNumberV4Success2()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation cycle information
          string serialNumber = "S1234";
          DateTime currentDate = DateTime.UtcNow;
          IOperationSlot lastSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null, component1,null, null, null,  null, null,new UtcDateTimeRange (currentDate));
          IOperationCycle firstCycle = ModelDAOHelper.ModelFactory.CreateOperationCycle(machine1);
          firstCycle.OperationSlot = lastSlot;
          firstCycle.Begin = currentDate.AddSeconds(1);

          Assert.That (firstCycle.HasRealBegin(), Is.True, "firstCycle should have real begin date time");
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          
          SaveSerialNumberV5 saveSerialNumber = new SaveSerialNumberV5();
          saveSerialNumber.MachineId = machine1.Id;
          saveSerialNumber.DateTime = ConvertDTO.DateTimeUtcToIsoString(firstCycle.Begin.Value);
          saveSerialNumber.IsBegin = true;
          saveSerialNumber.SerialNumber = serialNumber;
          
          object saveSerialNumberResponse = new SaveSerialNumberV5Service ().GetSync (saveSerialNumber);

          Assert.That (saveSerialNumberResponse as OkDTO, Is.Not.Null, "Non-OK saveSerialNumberResponse");
          
          ModelDAOHelper.DAOFactory.Flush ();
          
          Lemoine.Analysis.PendingGlobalMachineModificationAnalysis pendingModificationAnalysis =
            new Lemoine.Analysis.PendingGlobalMachineModificationAnalysis (false);

          pendingModificationAnalysis.MakeAnalysis(System.Threading.CancellationToken.None, currentDate.AddSeconds(10), 0, 0);
          
          IList<IOperationCycleDeliverablePiece> ocdpList = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(firstCycle);
          Assert.That (ocdpList, Is.Not.Null, "No associated serial number");
          Assert.That (ocdpList, Has.Count.EqualTo (1), "No single associated serial number");
          IDeliverablePiece deliverablePiece = ocdpList[0].DeliverablePiece;
          Assert.That (deliverablePiece, Is.Not.Null, "Null serial number");
          Assert.Multiple (() => {
            Assert.That (deliverablePiece.Code, Is.EqualTo (serialNumber), "Bad serial number");
            Assert.That (deliverablePiece.Component, Is.EqualTo (component1), "Bad component");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastWorkInformationV2Success1
    /// <summary>
    /// Test GetLastWorkInformationV2 service of UATService : success case (1)
    /// </summary>
    [Test]
    public void TestGetLastWorkInformationV3Success1()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime operationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(1));
          IOperationSlot lastSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, component1, workOrder1,
                                                            null, null,  null, null, new UtcDateTimeRange (operationSlotBeginDate));
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          
          GetLastWorkInformationV3 getLastWorkInformationV2 = new GetLastWorkInformationV3();
          getLastWorkInformationV2.Id = machine1.Id;
          
          ModelDAOHelper.DAOFactory.Flush ();
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response = new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response, Is.Not.Null, "No last workpiece information");
          Assert.That (getLastWorkInformationV2Response.SlotMissing, Is.EqualTo (false), "Bad slot misssing information");
          
          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;

          Assert.That (lastWorkInformationList, Is.Not.Null, "WorkInformations null");
          Assert.That (lastWorkInformationList, Has.Count.EqualTo (4), "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.That (workOrder1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Operation:
                Assert.That (operation1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Part:
                Assert.That (part1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Component:
                Assert.That (component1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Project:
                Assert.That (component1.Project.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              default:
                Assert.Fail ($"Bad kind {wpInfoDTO.Kind}");
                break;
            }
          }
          
          DateTime operationSlotBeginDate2 = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IOperationSlot prevSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, null, workOrder1,
                                                            null, null, null, null, new UtcDateTimeRange (operationSlotBeginDate2)); /* missing component */
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(prevSlot);
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response2 =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response2, Is.Not.Null, "No last workpiece information (2)");
          // TODO: the unit tests data is wrong. A component is returned and the following test failed
          /*
          Assert.AreEqual(true, getLastWorkInformationV2Response2.DataMissing, "Bad data misssing information (2)");
           */

          IList<WorkInformationDTO> lastWorkInformationList2 = getLastWorkInformationV2Response2.WorkInformations;

          Assert.That (lastWorkInformationList2, Is.Not.Null, "WorkInformations null (2)");
          Assert.That (lastWorkInformationList2, Has.Count.EqualTo (4), "WorkInformations.Count != 4 (2)");
          
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastWorkInformationV2Success3
    /// <summary>
    /// Test GetLastWorkInformationV2 service of UATService : success case (3)
    /// => last operation slot has no associated operation, but there is an
    /// operation slot which is close enough to take it into account.
    /// </summary>
    [Test]
    public void TestGetLastWorkInformationV3Success3()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime oldOperationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(100));
          DateTime oldOperationSlotEndDate = currentDate.Subtract(TimeSpan.FromSeconds(50));
          DateTime lastOperationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IOperationSlot oldSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, component2, workOrder1,
                                                            null,  null, null, null, new UtcDateTimeRange (oldOperationSlotBeginDate,
                                                                                                     oldOperationSlotEndDate));
          
          IOperationSlot lastSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null /* operation1*/ , component1, workOrder1,
                                                            null,  null, null, null,
                                                            new UtcDateTimeRange (lastOperationSlotBeginDate));
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(oldSlot);
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          
          GetLastWorkInformationV3 getLastWorkInformationV2 = new GetLastWorkInformationV3();
          getLastWorkInformationV2.Id = machine1.Id;
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response, Is.Not.Null, "No last workpiece information");
          Assert.That (getLastWorkInformationV2Response.SlotMissing, Is.EqualTo (false), "Bad slot misssing information");
          
          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;

          Assert.That (lastWorkInformationList, Is.Not.Null, "WorkInformations null");
          Assert.That (lastWorkInformationList, Has.Count.EqualTo (4), "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.That (workOrder1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Operation:
                Assert.That (operation1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Part:
                Assert.That (part1.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Component:
                Assert.That (component2.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              case WorkInformationKind.Project:
                Assert.That (component2.Project.Display, Is.EqualTo (wpInfoDTO.Value));
                break;
              default:
                Assert.Fail ($"Bad kind {wpInfoDTO.Kind}");
                break;
            }
          }
          
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastWorkInformationV2Success4
    /// <summary>
    /// Test GetLastWorkInformationV2 service of UATService : success case (4)
    /// => last operation slot has no associated operation, there is an
    /// operation slot before with an operation, but not close enough to take it into account.
    /// </summary>
    [Test]
    public void TestGetLastWorkInformationV3Success4()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime oldOperationSlotBeginDate = currentDate.Subtract(TimeSpan.FromMinutes(100));
          DateTime oldOperationSlotEndDate = currentDate.Subtract(TimeSpan.FromMinutes(50));
          DateTime lastOperationSlotBeginDate = currentDate.Subtract(TimeSpan.FromMinutes(10));
          IOperationSlot oldSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, component2, workOrder1,
                                                            null,  null, null, null,new UtcDateTimeRange (oldOperationSlotBeginDate,
                                                                                                    oldOperationSlotEndDate));
          
          IOperationSlot lastSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null /* operation1*/ , component1, workOrder1,
                                                            null, null, null, null, new UtcDateTimeRange (lastOperationSlotBeginDate));
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(oldSlot);
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          
          GetLastWorkInformationV3 getLastWorkInformationV2 = new GetLastWorkInformationV3();
          getLastWorkInformationV2.Id = machine1.Id;
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response, Is.Not.Null, "No last workpiece information");
          Assert.Multiple (() => {
            Assert.That (getLastWorkInformationV2Response.SlotMissing, Is.EqualTo (false), "Bad slot missing information");
            // operationfromCnc => only operation missing implies no data missing
            Assert.That (getLastWorkInformationV2Response.DataMissing, Is.EqualTo (false), "Bad data missing information");
          });

          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;

          Assert.That (lastWorkInformationList, Is.Not.Null, "WorkInformations null");
          Assert.That (lastWorkInformationList, Has.Count.EqualTo (4), "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.That (wpInfoDTO.Value, Is.EqualTo (workOrder1.Display));
                break;
              case WorkInformationKind.Operation:
                Assert.That (wpInfoDTO.Value, Is.EqualTo (operation1.Display));
                break;
              case WorkInformationKind.Part:
                Assert.That (wpInfoDTO.Value, Is.EqualTo (part1.Display));
                break;
              case WorkInformationKind.Component:
                Assert.That (wpInfoDTO.Value, Is.EqualTo (component2.Display));
                break;
              case WorkInformationKind.Project:
                Assert.That (wpInfoDTO.Value, Is.EqualTo (component2.Project.Display));
                break;
              default:
                Assert.Fail ($"Bad kind {wpInfoDTO.Kind}");
                break;
            }
          }

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastWorkInformationV2Failure
    /// <summary>
    /// Test GetLastWorkInformationV2 service of UATService : no last operation slot
    /// </summary>
    [Test]
    public void TestGetLastWorkInformationV3Failure()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetLastWorkInformationV3 getLastWorkInformationV2 = new GetLastWorkInformationV3();
          getLastWorkInformationV2.Id = 99999999;
          
          ErrorDTO getLastWorkInformationV2Response =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as ErrorDTO;

          Assert.That (getLastWorkInformationV2Response, Is.Not.Null, "No error dto");
          Assert.That (getLastWorkInformationV2Response.ErrorMessage, Does.StartWith ("No monitored machine with id"), "bad error msg");

          // good id but no operation slot
          getLastWorkInformationV2.Id = 1;
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response2 =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response2, Is.Not.Null, "No last work information dto");
          Assert.Multiple (() => {
            Assert.That (getLastWorkInformationV2Response2.SlotMissing, Is.True, "bad slot missing info");
            Assert.That (getLastWorkInformationV2Response2.DataMissing, Is.False, "bad data missing info (2)");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastWorkInformationV2Failure2
    /// <summary>
    /// Test GetLastWorkInformationV2 service of UATService : failure case (2)
    /// no last operation slot with an operation set which is close enough, return last one
    /// </summary>
    [Test]
    public void TestGetLastWorkInformationV3Failure2()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          // First create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime lastOperationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IOperationSlot lastSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, null /* operation */,
                                                            component1, workOrder1,
                                                            null, null, null, null, new UtcDateTimeRange (lastOperationSlotBeginDate));
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          
          GetLastWorkInformationV3 getLastWorkInformationV2 = new GetLastWorkInformationV3();
          getLastWorkInformationV2.Id = machine1.Id;
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;

          Assert.That (getLastWorkInformationV2Response, Is.Not.Null, "No last workpiece information");
          Assert.That (getLastWorkInformationV2Response.SlotMissing, Is.EqualTo (false), "Bad slot misssing information");
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetListOfOperationSlot
    /// <summary>
    /// Test GetListOfOperationSlot service of UATService : success case
    /// </summary>
    [Test]
    public void TestGetListOfOperationSlot()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetListOfOperationSlotV2 getListOperationSlot = new GetListOfOperationSlotV2();
          getListOperationSlot.Id = machine1.Id;
          
          ListOfOperationSlotV2DTO getListOperationSlotResponse0 = new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.That (getListOperationSlotResponse0, Is.Not.Null, "No last operation slots (0)");
          Assert.That (getListOperationSlotResponse0.List, Is.Not.Null, "No last operation slots list (0)");
          Assert.That (getListOperationSlotResponse0.List, Is.Empty, "Not 0 operation slots");

          // Now create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime operationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IOperationSlot firstSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, component1, workOrder1,
                                                            null, null, null, null, new UtcDateTimeRange (operationSlotBeginDate));
          
          DateTime operationSlotBeginDate2 = currentDate.Subtract(TimeSpan.FromSeconds(5));
          IOperationSlot secondSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, null, null,
                                                            null, null, null, null, new UtcDateTimeRange (operationSlotBeginDate2));
          
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(firstSlot);
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(secondSlot);
          
          ListOfOperationSlotV2DTO getListOperationSlotResponse =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.That (getListOperationSlotResponse, Is.Not.Null, "No last operation slots");
          Assert.That (getListOperationSlotResponse.List, Is.Not.Null, "No last operation slots list");
          Assert.That (getListOperationSlotResponse.List, Has.Count.EqualTo (2), "Not 2 operation slots");
          
          OperationSlotV2DTO firstResponseSlot = getListOperationSlotResponse.List[0];
          OperationSlotV2DTO secondResponseSlot = getListOperationSlotResponse.List[1];

          Assert.Multiple (() => {
            // reverse chronological order
            Assert.That (secondResponseSlot.OperationSlotId, Is.EqualTo (firstSlot.Id), "Last slot does not come first");
            Assert.That (firstResponseSlot.OperationSlotId, Is.EqualTo (secondSlot.Id), "More ancient slot does not come last");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetListOfOperationSlotWithBegin
    /// <summary>
    /// Test GetListOfOperationSlot service of UATService: explicit begin
    /// </summary>
    [Test]
    public void TestGetListOfOperationSlotWithBegin()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetListOfOperationSlotV2 getListOperationSlot = new GetListOfOperationSlotV2();
          getListOperationSlot.Id = machine1.Id;
          
          ListOfOperationSlotV2DTO getListOperationSlotResponse0 =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.That (getListOperationSlotResponse0, Is.Not.Null, "No last operation slots (0)");
          Assert.That (getListOperationSlotResponse0.List, Is.Not.Null, "No last operation slots list (0)");
          Assert.That (getListOperationSlotResponse0.List, Is.Empty, "Not 0 operation slots");

          // Now create some operation slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime operationSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IOperationSlot firstSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, component1, workOrder1,
                                                            null, null, null, null,
                                                            new UtcDateTimeRange (operationSlotBeginDate,
                                                                                  currentDate.Subtract(TimeSpan.FromSeconds(7))));
          
          DateTime operationSlotBeginDate2 = currentDate.Subtract(TimeSpan.FromSeconds(5));
          IOperationSlot secondSlot =
            ModelDAOHelper.ModelFactory.CreateOperationSlot(machine1, operation1, null, null,
                                                            null, null, null, null, new UtcDateTimeRange (operationSlotBeginDate2));
          
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(firstSlot);
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(secondSlot);
          
          getListOperationSlot.Begin = ConvertDTO.DateTimeUtcToIsoString(secondSlot.BeginDateTime);
          
          ListOfOperationSlotV2DTO getListOperationSlotResponse =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.That (getListOperationSlotResponse, Is.Not.Null, "No last operation slots");
          Assert.That (getListOperationSlotResponse.List, Is.Not.Null, "No last operation slots list");
          Assert.That (getListOperationSlotResponse.List, Has.Count.EqualTo (1), "Not 1 operation slot");
          
          OperationSlotV2DTO responseSlot = getListOperationSlotResponse.List[0];

          Assert.That (responseSlot.OperationSlotId, Is.EqualTo (secondSlot.Id), "bad slot");
          
          // now only first slot
          getListOperationSlot.Begin = ConvertDTO.DateTimeUtcToIsoString(firstSlot.BeginDateTime);
          getListOperationSlot.End = ConvertDTO.DateTimeUtcToIsoString(firstSlot.EndDateTime);
          
          getListOperationSlotResponse =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.That (getListOperationSlotResponse, Is.Not.Null, "No last operation slots (2)");
          Assert.That (getListOperationSlotResponse.List, Is.Not.Null, "No last operation slots list (2)");
          Assert.That (getListOperationSlotResponse.List, Has.Count.EqualTo (1), "Not 1 operation slot (2)");
          
          responseSlot = getListOperationSlotResponse.List[0];

          Assert.Multiple (() => {
            Assert.That (responseSlot.OperationSlotId, Is.EqualTo (firstSlot.Id), "bad slot (2)");
            Assert.That (responseSlot.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstSlot.BeginDateTime)), "bad begin");
            Assert.That (responseSlot.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstSlot.EndDateTime)), "bad begin");
          });
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetListOfOperationSlotFailure
    /// <summary>
    /// Test GetListOfOperationSlot service of UATService : failure case
    /// </summary>
    [Test]
    public void TestGetListOfOperationSlotFailure()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetListOfOperationSlotV2 getListOperationSlot = new GetListOfOperationSlotV2();
          getListOperationSlot.Id = 9999999;
          
          ErrorDTO getListOperationSlotResponse =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ErrorDTO;

          Assert.That (getListOperationSlotResponse, Is.Not.Null, "No error dto");
          Assert.That (getListOperationSlotResponse.ErrorMessage, Does.StartWith ("No monitored machine with id"), "Bad error msg");
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastMachineStatus
    /// <summary>
    /// Test GetLastMachineStatus service of UATService : success case
    /// </summary>
    [Test]
    public void TestGetLastMachineStatus()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          IMachineMode machineMode1 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById(1);
          IMachineObservationState machineObsState1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          IReason reason1 = ModelDAOHelper.DAOFactory.ReasonDAO.FindById(1);
          
          // First create some reason slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime reasonSlotBeginDate = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IReasonSlot lastSlot =
            ModelDAOHelper.ModelFactory.CreateReasonSlot(machine1,
                                                         new UtcDateTimeRange (reasonSlotBeginDate,
                                                                               currentDate.Subtract(TimeSpan.FromSeconds(5))));
          lastSlot.MachineMode = machineMode1;
          lastSlot.MachineObservationState = machineObsState1;
          ((ReasonSlot)lastSlot).SetManualReason (reason1, 100.0);
          
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(lastSlot);
          
          GetLastMachineStatusV2 getLastMachineStatus = new GetLastMachineStatusV2();
          getLastMachineStatus.Id = machine1.Id;
          //getLastMachineStatus.Begin = null;
          
          LastMachineExtendedStatusV2DTO getLastMachineStatusResponse =
            new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;

          Assert.That (getLastMachineStatusResponse, Is.Not.Null, "No last machine extended status information");
          Assert.That (getLastMachineStatusResponse.MachineStatus, Is.Not.Null, "Null Machine Status");
          Assert.Multiple (() => {
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineMode.Id, Is.EqualTo (machineMode1.Id), "Bad MachineMode");
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, Is.EqualTo (machineObsState1.Id), "Bad MachineObservationState");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, Is.EqualTo (lastSlot.Id), "Bad ReasonSlot");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, Is.EqualTo (reason1.Id), "Bad Reason");
            Assert.That (getLastMachineStatusResponse.RequiredReason, Is.False);
          });
          // ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakeTransient(lastSlot);

          // change overwrite
          ((ReasonSlot)lastSlot).SetMainAutoReason (reason1, 100.0, true);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(lastSlot);
          
          // refetch
          getLastMachineStatusResponse =
            new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;

          Assert.That (getLastMachineStatusResponse.RequiredReason, Is.True); // this changed from false to true


          // make slot older than 1 minute => obsolete
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakeTransient(lastSlot);
          lastSlot =
            ModelDAOHelper.ModelFactory
            .CreateReasonSlot(machine1, new UtcDateTimeRange (currentDate.Subtract(TimeSpan.FromMinutes(10)),
                                                              currentDate.Subtract(TimeSpan.FromMinutes(5))));
          lastSlot.MachineMode = machineMode1;
          lastSlot.MachineObservationState = machineObsState1;
          ((ReasonSlot)lastSlot).SetManualReason (reason1, 100.0);
          
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(lastSlot);

          // refetch: should return information with reasonslot too old
          LastMachineExtendedStatusV2DTO getLastMachineStatusResponse2 =
            new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;

          Assert.That (getLastMachineStatusResponse2, Is.Not.Null, "Last machine extended status information is not null");
          Assert.Multiple (() => {
            Assert.That (getLastMachineStatusResponse2.MachineStatus, Is.Not.Null, "Machine status information is not null");
            Assert.That (getLastMachineStatusResponse2.ReasonTooOld, Is.True, "ReasonSlot is too old");
            Assert.That (getLastMachineStatusResponse2.RequiredReason, Is.False, "Reason is not required");
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetLastMachineStatusWithBegin
    /// <summary>
    /// Test GetLastMachineStatus service of UATService : begin in input
    /// </summary>
    [Test]
    public void TestGetLastMachineStatusWithBegin()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          IMachineMode machineMode1 = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById(1);
          IMachineObservationState machineObsState1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          IReason reason1 = ModelDAOHelper.DAOFactory.ReasonDAO.FindById(1);
          
          // First create some reason slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime reasonSlotBeginDate = currentDate.Subtract(TimeSpan.FromHours(40));
          IReasonSlot lastSlot =
            ModelDAOHelper.ModelFactory
            .CreateReasonSlot(machine1,
                              new UtcDateTimeRange (reasonSlotBeginDate,
                                                    currentDate.Subtract(TimeSpan.FromHours(5))));
          lastSlot.MachineMode = machineMode1;
          lastSlot.MachineObservationState = machineObsState1;
          ((ReasonSlot)lastSlot).SetMainAutoReason (reason1, 100.0, true);
          
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(lastSlot);

          DateTime veryLastSlotBeginDateTime = currentDate.Subtract(TimeSpan.FromSeconds(30));
          IReasonSlot veryLastSlot =
            ModelDAOHelper.ModelFactory
            .CreateReasonSlot(machine1,
                              new UtcDateTimeRange (veryLastSlotBeginDateTime,
                                                    veryLastSlotBeginDateTime.AddSeconds(10)));
          veryLastSlot.MachineMode = machineMode1;
          veryLastSlot.MachineObservationState = machineObsState1;
          ((ReasonSlot)veryLastSlot).SetManualReason (reason1, 100.0);
          
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(veryLastSlot);
          
          GetLastMachineStatusV2 getLastMachineStatus = new GetLastMachineStatusV2();
          getLastMachineStatus.Id = machine1.Id;
          getLastMachineStatus.Begin = ConvertDTO.DateTimeUtcToIsoString(lastSlot.EndDateTime.Value.AddHours(1));
          
          LastMachineExtendedStatusV2DTO getLastMachineStatusResponse =
            new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;

          Assert.That (getLastMachineStatusResponse, Is.Not.Null, "No last machine extended status information");
          Assert.That (getLastMachineStatusResponse.MachineStatus, Is.Not.Null, "Null Machine Status");
          Assert.Multiple (() => {
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineMode.Id, Is.EqualTo (machineMode1.Id), "Bad MachineMode");
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, Is.EqualTo (machineObsState1.Id), "Bad MachineObservationState");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, Is.EqualTo (veryLastSlot.Id), "Bad ReasonSlot");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, Is.EqualTo (reason1.Id), "Bad Reason");
            // only veryLastSlot fetched => no overwrite required
            Assert.That (getLastMachineStatusResponse.RequiredReason, Is.False, "False required reason");
          });

          getLastMachineStatus.Begin = ConvertDTO.DateTimeUtcToIsoString(lastSlot.BeginDateTime.Value.AddHours(1));
          getLastMachineStatusResponse = new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;

          Assert.That (getLastMachineStatusResponse, Is.Not.Null, "No last machine extended status information");
          Assert.That (getLastMachineStatusResponse.MachineStatus, Is.Not.Null, "Null Machine Status");
          Assert.Multiple (() => {
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineMode.Id, Is.EqualTo (machineMode1.Id), "Bad MachineMode");
            Assert.That (getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, Is.EqualTo (machineObsState1.Id), "Bad MachineObservationState");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, Is.EqualTo (veryLastSlot.Id), "Bad ReasonSlot");
            Assert.That (getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, Is.EqualTo (reason1.Id), "Bad Reason");
            // lastSlot also fetched => overwrite required
            Assert.That (getLastMachineStatusResponse.RequiredReason, Is.True, "True required reason");
          });


        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetMachineObservationStateListV2
    /// <summary>
    /// Test GetMachineObservationStateListV2 service of UATService : success case
    /// </summary>
    [Test]
    public void TestGetMachineObservationStateListV2()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          IMachineObservationState machineObsState1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          IMachineObservationState machineObsState2 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(2);
          
          // First create some machine observation state slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime beginDateTime1 = currentDate.Subtract(TimeSpan.FromSeconds(100));
          DateTime endDateTime1 = currentDate.Subtract(TimeSpan.FromSeconds(50));
          IObservationStateSlot firstSlot =
            new ObservationStateSlot(machine1, new UtcDateTimeRange (beginDateTime1,
                                                                     endDateTime1));
          firstSlot.MachineObservationState = machineObsState1;
          DateTime beginDateTime2 = currentDate.Subtract(TimeSpan.FromSeconds(20));
          DateTime endDateTime2 = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IObservationStateSlot secondSlot =
            new ObservationStateSlot(machine1, new UtcDateTimeRange (beginDateTime2,
                                                                     endDateTime2));
          secondSlot.MachineObservationState = machineObsState2;
          
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent(firstSlot);
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent(secondSlot);
          
          GetMachineObservationStateListV2 GetMachineObservationStateListV2 = new GetMachineObservationStateListV2();
          GetMachineObservationStateListV2.Id = machine1.Id;
          
          MachineObservationStateSlotListV2DTO GetMachineObservationStateListV2Response =
            new GetMachineObservationStateListV2Service ().GetWithoutCache (GetMachineObservationStateListV2) as MachineObservationStateSlotListV2DTO;

          Assert.That (GetMachineObservationStateListV2Response, Is.Not.Null, "No machine observation state list");
          Assert.That (GetMachineObservationStateListV2Response.List, Is.Not.Null, "No machine observation state list's list");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.That (GetMachineObservationStateListV2Response.List, Has.Count.EqualTo (3), "Not 3 machine observation states");
          MachineObservationStateSlotV2DTO mobs1 = GetMachineObservationStateListV2Response.List[0];
          MachineObservationStateSlotV2DTO mobs2 = GetMachineObservationStateListV2Response.List[1];

          Assert.Multiple (() => {
            // reverse chronological order
            Assert.That (mobs1.Id, Is.EqualTo (secondSlot.Id), "Bad id for first machine observation state");
            Assert.That (mobs2.Id, Is.EqualTo (firstSlot.Id), "Bad id for second machine observation state");

            Assert.That (mobs1.MachineObservationState.Text, Is.EqualTo (secondSlot.MachineObservationState.Display), "Bad text for first machine observation state");
            Assert.That (mobs2.MachineObservationState.Text, Is.EqualTo (firstSlot.MachineObservationState.Display), "Bad text for second machine observation state");

            Assert.That (mobs1.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (secondSlot.BeginDateTime)), "Bad begin for first machine observation state");
            Assert.That (mobs2.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstSlot.BeginDateTime)), "Bad begin for second machine observation state");

            Assert.That (mobs1.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (secondSlot.EndDateTime)), "Bad end for first machine observation state");
            Assert.That (mobs2.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (firstSlot.EndDateTime)), "Bad end for second machine observation state");
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetMachineObservationStateListV2WithBegin
    /// <summary>
    /// Test GetMachineObservationStateListV2 service of UATService: explicit begin/end
    /// </summary>
    [Test]
    public void TestGetMachineObservationStateListV2WithBegin()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          IMachineObservationState machineObsState1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          IMachineObservationState machineObsState2 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(2);
          
          // First create some machine observation state slot information
          DateTime currentDate = DateTime.UtcNow;
          DateTime beginDateTime1 = currentDate.Subtract(TimeSpan.FromSeconds(100));
          DateTime endDateTime1 = currentDate.Subtract(TimeSpan.FromSeconds(50));
          IObservationStateSlot firstSlot =
            new ObservationStateSlot(machine1, new UtcDateTimeRange (beginDateTime1,
                                                                     endDateTime1));
          firstSlot.MachineObservationState = machineObsState1;
          DateTime beginDateTime2 = currentDate.Subtract(TimeSpan.FromSeconds(20));
          DateTime endDateTime2 = currentDate.Subtract(TimeSpan.FromSeconds(10));
          IObservationStateSlot secondSlot =
            new ObservationStateSlot(machine1, new UtcDateTimeRange (beginDateTime2,
                                                                     endDateTime2));
          secondSlot.MachineObservationState = machineObsState2;
          
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent(firstSlot);
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.MakePersistent(secondSlot);
          
          GetMachineObservationStateListV2 GetMachineObservationStateListV2 = new GetMachineObservationStateListV2();
          GetMachineObservationStateListV2.Id = machine1.Id;
          GetMachineObservationStateListV2.Begin =
            ConvertDTO.DateTimeUtcToIsoString(currentDate.Subtract(TimeSpan.FromSeconds(15)));
          
          MachineObservationStateSlotListV2DTO GetMachineObservationStateListV2Response =
            new GetMachineObservationStateListV2Service ().GetWithoutCache (GetMachineObservationStateListV2) as MachineObservationStateSlotListV2DTO;

          Assert.That (GetMachineObservationStateListV2Response, Is.Not.Null, "No machine observation state list");
          Assert.That (GetMachineObservationStateListV2Response.List, Is.Not.Null, "No machine observation state list's list");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.That (GetMachineObservationStateListV2Response.List, Has.Count.EqualTo (2), "Not 2 machine observation states");
          MachineObservationStateSlotV2DTO mobs1 = GetMachineObservationStateListV2Response.List[0];

          Assert.Multiple (() => {
            // reverse chronological order
            Assert.That (mobs1.Id, Is.EqualTo (secondSlot.Id), "Bad id for first machine observation state");

            Assert.That (mobs1.MachineObservationState.Text, Is.EqualTo (secondSlot.MachineObservationState.Display), "Bad text for first machine observation state");

            Assert.That (mobs1.Begin, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (secondSlot.BeginDateTime)), "Bad begin for first machine observation state");

            Assert.That (mobs1.End, Is.EqualTo (ConvertDTO.DateTimeUtcToIsoString (secondSlot.EndDateTime)), "Bad end for first machine observation state");
          });

          GetMachineObservationStateListV2.Begin =
            ConvertDTO.DateTimeUtcToIsoString(beginDateTime1);

          GetMachineObservationStateListV2.End =
            ConvertDTO.DateTimeUtcToIsoString(endDateTime1);
          
          GetMachineObservationStateListV2Response =
            new GetMachineObservationStateListV2Service ().GetWithoutCache (GetMachineObservationStateListV2) as MachineObservationStateSlotListV2DTO;

          Assert.That (GetMachineObservationStateListV2Response, Is.Not.Null, "No machine observation state list (2)");
          Assert.That (GetMachineObservationStateListV2Response.List, Is.Not.Null, "No machine observation state list's list (2)");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.That (GetMachineObservationStateListV2Response.List, Has.Count.EqualTo (2), "Not 2 machine observation states (2)");
          mobs1 = GetMachineObservationStateListV2Response.List[0];

          Assert.Multiple (() => {
            // reverse chronological order
            Assert.That (mobs1.Id, Is.EqualTo (firstSlot.Id), "Bad id for first machine observation state (2)");

            Assert.That (mobs1.MachineObservationState.Text, Is.EqualTo (firstSlot.MachineObservationState.Display), "Bad text for first machine observation state (2)");
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion

    #region TestGetMachineObservationStateSelection
    /// <summary>
    /// Test GetMachineObservationStateSelection service of UATService : success case
    /// </summary>
    [Test]
    public void TestGetMachineObservationStateSelection()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          GetMachineObservationStateSelection getMachineObservationStateSelection = new GetMachineObservationStateSelection();
          
          IList<MachineObservationStateDTO> getMachineObservationStateSelectionResponse =
            new GetMachineObservationStateSelectionService ().GetWithoutCache (getMachineObservationStateSelection) as IList<MachineObservationStateDTO>;

          Assert.That (getMachineObservationStateSelectionResponse, Is.Not.Null, "No GetMachineObservationStateSelection response");
          Assert.That (getMachineObservationStateSelectionResponse, Has.Count.EqualTo (14), "Bad number of machine observation states");

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion
          
    #region TestSaveMachineObservationState
    /// <summary>
    /// Test SaveMachineObservationState service of UATService : success case
    /// </summary>
    [Test]
    public void TestSaveMachineObservationState() {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          DateTime begin = new DateTime(2013, 9, 6, 14, 59, 0, DateTimeKind.Utc);
          DateTime end = new DateTime(2013, 9, 6, 15, 0, 0, DateTimeKind.Utc);

          IMachineObservationState machObs1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          
          SaveMachineObservationStateV2 saveMachineObservationState = new SaveMachineObservationStateV2();
          saveMachineObservationState.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
          saveMachineObservationState.End = ConvertDTO.DateTimeUtcToIsoString(end);
          saveMachineObservationState.Id = machine1.Id;
          saveMachineObservationState.MachineObservationStateId = machObs1.Id;
          
          object saveMachineObservationStateResponse = new SaveMachineObservationStateV2Service ().GetSync (saveMachineObservationState);

          Assert.That (saveMachineObservationStateResponse as OkDTO, Is.Not.Null, "Non-OK saveMachineObservationState");
          
          // just test there is a pending modification of the right type
          
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.That (modifications, Is.Not.Null, "no modification");
          Assert.That (modifications, Has.Count.EqualTo (1), "not 1 modification");
          IModification modification = modifications[0];
          IMachineObservationStateAssociation machineObservationStateAssociation = modification as IMachineObservationStateAssociation;
          Assert.That (machineObservationStateAssociation, Is.Not.Null, "not a machine observation state association");
          Assert.Multiple (() => {
            Assert.That (machineObservationStateAssociation.Begin.Value, Is.EqualTo (begin.ToUniversalTime ()), "bad begin for machine observation state association");
            Assert.That (machineObservationStateAssociation.End.Value, Is.EqualTo (end.ToUniversalTime ()), "bad end for machine observation state association");
            Assert.That (machineObservationStateAssociation.MachineObservationState.Id, Is.EqualTo (machObs1.Id));
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion
    
    #region TestSaveMachineObservationStateV2
    /// <summary>
    /// Test SaveMachineObservationStateV2 service of UATService : success case
    /// </summary>
    [Test]
    public void TestSaveMachineObservationStateV2() {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          TestInit();
          DateTime begin = new DateTime(2013, 9, 6, 14, 59, 0, DateTimeKind.Utc);
          DateTime end = new DateTime(2013, 9, 6, 15, 0, 0, DateTimeKind.Utc);

          IMachineObservationState machObs1 = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById(1);
          
          SaveMachineObservationStateV2 saveMachineObservationStateV2 = new SaveMachineObservationStateV2();
          saveMachineObservationStateV2.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
          saveMachineObservationStateV2.End = ConvertDTO.DateTimeUtcToIsoString(end);
          saveMachineObservationStateV2.Id = machine1.Id;
          saveMachineObservationStateV2.MachineObservationStateId = machObs1.Id;
          
          object SaveMachineObservationStateV2Response = new SaveMachineObservationStateV2Service ().GetSync (saveMachineObservationStateV2);

          Assert.That (SaveMachineObservationStateV2Response as OkDTO, Is.Not.Null, "Non-OK SaveMachineObservationStateV2");
          
          // just test there is a pending modification of the right type
          
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.That (modifications, Is.Not.Null, "no modification");
          Assert.That (modifications, Has.Count.EqualTo (1), "not 1 modification");
          IModification modification = modifications[0];
          IMachineObservationStateAssociation machineObservationStateAssociation = modification as IMachineObservationStateAssociation;
          Assert.That (machineObservationStateAssociation, Is.Not.Null, "not a machine observation state association");
          Assert.Multiple (() => {
            Assert.That (machineObservationStateAssociation.Begin.Value, Is.EqualTo (begin.ToUniversalTime ()), "bad begin for machine observation state association");
            Assert.That (machineObservationStateAssociation.End.Value, Is.EqualTo (end.ToUniversalTime ()), "bad end for machine observation state association");
            Assert.That (machineObservationStateAssociation.MachineObservationState.Id, Is.EqualTo (machObs1.Id));
          });

        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion
    
    /// <summary>
    /// Test DateTime conversion
    /// </summary>
    [Test]
    public void TestDateTimeConverter()
    {
      DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      long ticks = ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(origin);
      Assert.That (ticks, Is.EqualTo (0));
      ticks = 1000;
      DateTime originPlus1s = origin.AddSeconds(1);
      DateTime ticksConvertedDt = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticks);
      Assert.Multiple (() => {
        Assert.That (ticksConvertedDt.Kind, Is.EqualTo (DateTimeKind.Utc));
        Assert.That (ticksConvertedDt, Is.EqualTo (originPlus1s));
      });
      DateTime current = DateTime.UtcNow; // new DateTime(2000, 1, 1, 12, 15, 35, DateTimeKind.Utc);
      Assert.That (current.Kind, Is.EqualTo (DateTimeKind.Utc));
      ticks = ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(current);
      DateTime shouldBeCurrent = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticks);
      Assert.That (current.Millisecond, Is.EqualTo (shouldBeCurrent.Millisecond));
      // Assert.AreEqual(shouldBeCurrent, current); // no cf. ms resolution only
      long ticks2 =  ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(shouldBeCurrent);
      Assert.That (ticks, Is.EqualTo (ticks2));
      
      DateTime preOrigin = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      long ticksPre = ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(preOrigin);
      DateTime convertedPreOrigin = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticksPre);
      Assert.That (convertedPreOrigin, Is.EqualTo (preOrigin), "Bad convert for 1950");
    }
    
    [OneTimeSetUp]
    public void Init()
    {
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
