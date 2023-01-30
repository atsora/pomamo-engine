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

      Assert.IsNotNull(machine1);
      Assert.IsNotNull(machine2);
      Assert.IsNotNull(machineModule1);
      Assert.AreEqual(machineModule1.MonitoredMachine, machine1, "Machine module not OK");
      Assert.IsNotNull(component1);
      Assert.IsNotNull(component2);
      Assert.IsNotNull(operation1);
      Assert.IsNotNull(operation2);
      Assert.IsNotNull(operation3);
      Assert.IsNotNull(part1);
      Assert.IsNotNull(workOrder1);
      Assert.IsNotNull(workOrder2);
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
          
          Assert.IsNotNull(lastCycleWithSerialNumberDTO, "GetLastCycleWithSerialNumber is null");
          Assert.AreEqual(lastCycle.Id, lastCycleWithSerialNumberDTO.CycleId, "Bad cycle id");
          Assert.AreEqual(false, lastCycleWithSerialNumberDTO.EstimatedBegin, "Bad cycle estimated begin status");
          Assert.AreEqual(false, lastCycleWithSerialNumberDTO.EstimatedEnd, "Bad cycle estimated end status");
          Assert.AreEqual(serialNumber, lastCycleWithSerialNumberDTO.SerialNumber, "Bad serial number arg");
          Assert.AreEqual(true, lastCycleWithSerialNumberDTO.DataMissing, "Bad data missing arg");
          
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
          
          Assert.IsNotNull(lastCycleWithSerialNumberDTO2, "GetLastCycleWithSerialNumber is null (2)");
          Assert.AreEqual(lastCycle.Id, lastCycleWithSerialNumberDTO2.CycleId, "Bad cycle id");
          Assert.AreEqual(serialNumber, lastCycleWithSerialNumberDTO2.SerialNumber, "Bad serial number arg (2)");
          Assert.AreEqual(false, lastCycleWithSerialNumberDTO2.DataMissing, "Bad data missing arg (2)");
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
          
          Assert.IsNotNull(lastCycleWithSerialNumberDTO, "GetLastCycleWithSerialNumber is null");
          Assert.AreEqual(lastCycle.Id, lastCycleWithSerialNumberDTO.CycleId, "Bad cycle id");
          Assert.AreEqual(serialNumber, lastCycleWithSerialNumberDTO.SerialNumber, "Bad serial number arg");
          Assert.AreEqual(false, lastCycleWithSerialNumberDTO.DataMissing, "Bad data missing arg (1)");
          
          
          // go back in time to fetch missing serial on firstCycle
          getLastCycleWithSerialNumber.Begin = ConvertDTO.DateTimeUtcToIsoString(currentDate.Subtract(TimeSpan.FromHours(40)));
          lastCycleWithSerialNumberDTO =
            new GetLastCycleWithSerialNumberV2Service ().GetWithoutCache (getLastCycleWithSerialNumber)
            as LastCycleWithSerialNumberV2DTO;
          
          Assert.IsNotNull(lastCycleWithSerialNumberDTO, "GetLastCycleWithSerialNumber is null");
          Assert.AreEqual(lastCycle.Id, lastCycleWithSerialNumberDTO.CycleId, "Bad cycle id");
          Assert.AreEqual(serialNumber, lastCycleWithSerialNumberDTO.SerialNumber, "Bad serial number arg");
          Assert.AreEqual(true, lastCycleWithSerialNumberDTO.DataMissing, "Bad data missing arg (2)");
          
          
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
          
          Assert.IsNotNull(lastCycleWithSerialNumberDTO, "GetLastCycleWithSerialNumber is null");
          Assert.AreEqual("0", lastCycleWithSerialNumberDTO.SerialNumber, "Bad serial number arg");
          Assert.AreEqual(true, lastCycleWithSerialNumberDTO.DataMissing, "Bad data missing arg");
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
        
        Assert.IsNotNull(lastCycleWithSerialNumberDTO, "GetLastCycleWithSerialNumber is null");
        Assert.AreEqual("-1", lastCycleWithSerialNumberDTO.SerialNumber, "Bad serial number arg");
        Assert.AreEqual(false, lastCycleWithSerialNumberDTO.DataMissing, "Bad data missing arg");
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

          Assert.IsNotNull(response, "No error dto");
          Assert.IsTrue(response.ErrorMessage.StartsWith("No monitored machine with id"), "Bad error msg");
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
          
          Assert.IsNotNull(cyclesInPeriodDTO, "GetCyclesInPeriod returns null");
          Assert.IsNotNull(cyclesInPeriodDTO.List, "GetCyclesInPeriod returns null list");
          Assert.AreEqual(2, cyclesInPeriodDTO.List.Count, "GetCyclesInPeriod returns bad number of cycle DTOs");
          CycleWithWorkInformationsV2DTO cycleDTO1 = cyclesInPeriodDTO.List[0];
          CycleWithWorkInformationsV2DTO cycleDTO2 = cyclesInPeriodDTO.List[1];
          
          // reverse order
          Assert.AreEqual(lastCycle.Id, cycleDTO1.CycleId, "bad cycle id (1)");
          Assert.AreEqual(firstCycle.Id, cycleDTO2.CycleId, "bad cycle id (2)");

          Assert.AreEqual(false, cycleDTO1.EstimatedBegin, "bad 2nd cycle estimated begin");
          Assert.AreEqual(false, cycleDTO1.EstimatedEnd, "bad 2nd cycle estimated end");
          
          Assert.AreEqual(false, cycleDTO2.EstimatedBegin, "bad first cycle estimated begin");
          Assert.AreEqual(true, cycleDTO2.EstimatedEnd, "bad first cycle estimated end");
          
          Assert.AreEqual(serialNumber, cycleDTO1.SerialNumber, "Bad Serial Number (1)");
          Assert.AreEqual(null, cycleDTO2.SerialNumber, "Bad Serial Number (2)");

          Assert.IsNotNull(cyclesInPeriodDTO.Begin, "null begin"); // exact values are difficult to tell
          Assert.IsNotNull(cyclesInPeriodDTO.End, "null end");

          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin), cycleDTO2.Begin, "Bad begin for cycle (1)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd), cycleDTO2.End, "Bad end for cycle (1)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(lastCycleBegin), cycleDTO1.Begin, "Bad begin for cycle (2)");
          Assert.AreEqual("", cycleDTO1.End, "Bad end for cycle (2)");
          
          getCyclesInCurrentPeriod.Begin = ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin);
          getCyclesInCurrentPeriod.End = ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd);
          CyclesWithWorkInformationsInPeriodV2DTO cyclesInPeriodDTO2 = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;
          Assert.IsNotNull(cyclesInPeriodDTO2, "GetCyclesInPeriod returns null (2)");
          Assert.IsNotNull(cyclesInPeriodDTO2.List, "GetCyclesInPeriod returns null list (2)");
          Assert.AreEqual(1, cyclesInPeriodDTO2.List.Count, "GetCyclesInPeriod returns bad number of cycle DTOs (2)");
          CycleWithWorkInformationsV2DTO cycleDTO3 = cyclesInPeriodDTO2.List[0];
          Assert.AreEqual(null, cycleDTO3.SerialNumber, "Bad Serial Number (3)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin), cyclesInPeriodDTO2.Begin, "Bad begin for cycle (3)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd), cyclesInPeriodDTO2.End, "Bad begin for cycle (3)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin), cycleDTO3.Begin, "Bad begin for cycle (3)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd), cycleDTO3.End, "Bad begin for cycle (3)");
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
          
          Assert.IsNotNull(cyclesInPeriodDTO, "GetCyclesInPeriod returns null");
          Assert.IsNotNull(cyclesInPeriodDTO.List, "GetCyclesInPeriod returns null list");
          Assert.AreEqual(1, cyclesInPeriodDTO.List.Count, "GetCyclesInPeriod returns bad number of cycle DTOs");
          CycleWithWorkInformationsV2DTO cycleDTO2 = cyclesInPeriodDTO.List[0];
          
          Assert.AreEqual(serialNumber, cycleDTO2.SerialNumber, "Bad Serial Number");

          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(lastCycleBegin), cycleDTO2.Begin, "Bad begin for cycle");
          Assert.AreEqual("", cycleDTO2.End, "Bad end for cycle");
          
          getCyclesInCurrentPeriod.Begin = ConvertDTO.DateTimeUtcToIsoString(currentDate.AddSeconds(-10));
          getCyclesInCurrentPeriod.End = ConvertDTO.DateTimeUtcToIsoString(currentDate.AddSeconds(-9));
          
          cyclesInPeriodDTO = new GetCyclesWithWorkInformationsInPeriodV2Service ().GetWithoutCache (getCyclesInCurrentPeriod) as CyclesWithWorkInformationsInPeriodV2DTO;
          
          Assert.IsNotNull(cyclesInPeriodDTO, "GetCyclesInPeriod returns null (2)");
          Assert.IsNotNull(cyclesInPeriodDTO.List, "GetCyclesInPeriod returns null list");
          Assert.AreEqual(1, cyclesInPeriodDTO.List.Count, "GetCyclesInPeriod returns bad number of cycle DTOs (2)");
          CycleWithWorkInformationsV2DTO cycleDTO1 = cyclesInPeriodDTO.List[0];
          
          Assert.AreEqual(null, cycleDTO1.SerialNumber, "Bad Serial Number (2)");

          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleBegin), cycleDTO1.Begin, "Bad begin for cycle (2)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstCycleEnd), cycleDTO1.End, "Bad end for cycle (2)");
          
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

          Assert.IsNotNull(response, "No error dto");
          Assert.IsTrue(response.ErrorMessage.StartsWith("No monitored machine with id"), "Bad error msg");
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
          
          Assert.IsNotNull(saveSerialNumberResponse as OkDTO, "Non-OK saveSerialNumberResponse");
          
          ModelDAOHelper.DAOFactory.Flush ();
          
          Lemoine.Analysis.PendingGlobalMachineModificationAnalysis pendingModificationAnalysis =
            new Lemoine.Analysis.PendingGlobalMachineModificationAnalysis (false);

          pendingModificationAnalysis.MakeAnalysis(System.Threading.CancellationToken.None, currentDate.AddSeconds(10), 0, 0);
          
          IList<IOperationCycleDeliverablePiece> ocdpList = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(firstCycle);
          Assert.IsNotNull(ocdpList, "No associated serial number");
          Assert.AreEqual(1, ocdpList.Count, "No single associated serial number");
          IDeliverablePiece deliverablePiece = ocdpList[0].DeliverablePiece;
          Assert.IsNotNull(deliverablePiece, "Null serial number");
          Assert.AreEqual(serialNumber, deliverablePiece.Code, "Bad serial number");
          Assert.AreEqual(component1, deliverablePiece.Component, "Bad component");
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

          Assert.IsTrue(firstCycle.HasRealBegin(), "firstCycle should have real begin date time");
          
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent(lastSlot);
          ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent(firstCycle);
          
          SaveSerialNumberV5 saveSerialNumber = new SaveSerialNumberV5();
          saveSerialNumber.MachineId = machine1.Id;
          saveSerialNumber.DateTime = ConvertDTO.DateTimeUtcToIsoString(firstCycle.Begin.Value);
          saveSerialNumber.IsBegin = true;
          saveSerialNumber.SerialNumber = serialNumber;
          
          object saveSerialNumberResponse = new SaveSerialNumberV5Service ().GetSync (saveSerialNumber);
          
          Assert.IsNotNull(saveSerialNumberResponse as OkDTO, "Non-OK saveSerialNumberResponse");
          
          ModelDAOHelper.DAOFactory.Flush ();
          
          Lemoine.Analysis.PendingGlobalMachineModificationAnalysis pendingModificationAnalysis =
            new Lemoine.Analysis.PendingGlobalMachineModificationAnalysis (false);

          pendingModificationAnalysis.MakeAnalysis(System.Threading.CancellationToken.None, currentDate.AddSeconds(10), 0, 0);
          
          IList<IOperationCycleDeliverablePiece> ocdpList = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(firstCycle);
          Assert.IsNotNull(ocdpList, "No associated serial number");
          Assert.AreEqual(1, ocdpList.Count, "No single associated serial number");
          IDeliverablePiece deliverablePiece = ocdpList[0].DeliverablePiece;
          Assert.IsNotNull(deliverablePiece, "Null serial number");
          Assert.AreEqual(serialNumber, deliverablePiece.Code, "Bad serial number");
          Assert.AreEqual(component1, deliverablePiece.Component, "Bad component");
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response, "No last workpiece information");
          Assert.AreEqual(false, getLastWorkInformationV2Response.SlotMissing, "Bad slot misssing information");
          
          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;
          
          Assert.IsNotNull(lastWorkInformationList, "WorkInformations null");
          Assert.AreEqual(4, lastWorkInformationList.Count, "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.AreEqual(wpInfoDTO.Value, workOrder1.Display);
                break;
              case WorkInformationKind.Operation:
                Assert.AreEqual(wpInfoDTO.Value, operation1.Display);
                break;
              case WorkInformationKind.Part:
                Assert.AreEqual(wpInfoDTO.Value, part1.Display);
                break;
              case WorkInformationKind.Component:
                Assert.AreEqual(wpInfoDTO.Value, component1.Display);
                break;
              case WorkInformationKind.Project:
                Assert.AreEqual(wpInfoDTO.Value, component1.Project.Display);
                break;
              default:
                Assert.IsTrue(false, String.Format("Bad kind {0}", wpInfoDTO.Kind.ToString()));
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response2, "No last workpiece information (2)");
          // TODO: the unit tests data is wrong. A component is returned and the following test failed
          /*
          Assert.AreEqual(true, getLastWorkInformationV2Response2.DataMissing, "Bad data misssing information (2)");
           */

          IList<WorkInformationDTO> lastWorkInformationList2 = getLastWorkInformationV2Response2.WorkInformations;
          
          Assert.IsNotNull(lastWorkInformationList2, "WorkInformations null (2)");
          Assert.AreEqual(4, lastWorkInformationList2.Count, "WorkInformations.Count != 4 (2)");
          
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response, "No last workpiece information");
          Assert.AreEqual(false, getLastWorkInformationV2Response.SlotMissing, "Bad slot misssing information");
          
          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;
          
          Assert.IsNotNull(lastWorkInformationList, "WorkInformations null");
          Assert.AreEqual(4, lastWorkInformationList.Count, "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.AreEqual(wpInfoDTO.Value, workOrder1.Display);
                break;
              case WorkInformationKind.Operation:
                Assert.AreEqual(wpInfoDTO.Value, operation1.Display);
                break;
              case WorkInformationKind.Part:
                Assert.AreEqual(wpInfoDTO.Value, part1.Display);
                break;
              case WorkInformationKind.Component:
                Assert.AreEqual(wpInfoDTO.Value, component2.Display);
                break;
              case WorkInformationKind.Project:
                Assert.AreEqual(wpInfoDTO.Value, component2.Project.Display);
                break;
              default:
                Assert.IsTrue(false, String.Format("Bad kind {0}", wpInfoDTO.Kind.ToString()));
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response, "No last workpiece information");
          Assert.AreEqual(false, getLastWorkInformationV2Response.SlotMissing, "Bad slot missing information");
          // operationfromCnc => only operation missing implies no data missing
          Assert.AreEqual(false, getLastWorkInformationV2Response.DataMissing, "Bad data missing information");
          
          IList<WorkInformationDTO> lastWorkInformationList = getLastWorkInformationV2Response.WorkInformations;
          
          Assert.IsNotNull(lastWorkInformationList, "WorkInformations null");
          Assert.AreEqual(4, lastWorkInformationList.Count, "WorkInformations.Count != 4");
          for(int i = 0 ; i < 4 ; i++) {
            WorkInformationDTO wpInfoDTO = lastWorkInformationList[i];
            switch(wpInfoDTO.Kind) {
              case WorkInformationKind.WorkOrder:
                Assert.AreEqual(workOrder1.Display, wpInfoDTO.Value);
                break;
              case WorkInformationKind.Operation:
                Assert.AreEqual(operation1.Display, wpInfoDTO.Value);
                break;
              case WorkInformationKind.Part:
                Assert.AreEqual(part1.Display, wpInfoDTO.Value);
                break;
              case WorkInformationKind.Component:
                Assert.AreEqual(component2.Display, wpInfoDTO.Value);
                break;
              case WorkInformationKind.Project:
                Assert.AreEqual(component2.Project.Display, wpInfoDTO.Value);
                break;
              default:
                Assert.IsTrue(false, String.Format("Bad kind {0}", wpInfoDTO.Kind.ToString()));
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response, "No error dto");
          Assert.IsTrue(getLastWorkInformationV2Response.ErrorMessage.StartsWith("No monitored machine with id"), "bad error msg");

          // good id but no operation slot
          getLastWorkInformationV2.Id = 1;
          
          LastWorkInformationV3DTO getLastWorkInformationV2Response2 =
            new GetLastWorkInformationV3Service ()
            .GetWithoutCache (getLastWorkInformationV2) as LastWorkInformationV3DTO;
          
          Assert.IsNotNull(getLastWorkInformationV2Response2, "No last work information dto");
          Assert.IsTrue(getLastWorkInformationV2Response2.SlotMissing, "bad slot missing info");
          Assert.IsFalse(getLastWorkInformationV2Response2.DataMissing, "bad data missing info (2)");
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
          
          Assert.IsNotNull(getLastWorkInformationV2Response, "No last workpiece information");
          Assert.AreEqual(false, getLastWorkInformationV2Response.SlotMissing, "Bad slot misssing information");
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

          Assert.IsNotNull(getListOperationSlotResponse0, "No last operation slots (0)");
          Assert.IsNotNull(getListOperationSlotResponse0.List, "No last operation slots list (0)");
          Assert.AreEqual(0, getListOperationSlotResponse0.List.Count, "Not 0 operation slots");

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

          Assert.IsNotNull(getListOperationSlotResponse, "No last operation slots");
          Assert.IsNotNull(getListOperationSlotResponse.List, "No last operation slots list");
          Assert.AreEqual(2, getListOperationSlotResponse.List.Count, "Not 2 operation slots");
          
          OperationSlotV2DTO firstResponseSlot = getListOperationSlotResponse.List[0];
          OperationSlotV2DTO secondResponseSlot = getListOperationSlotResponse.List[1];
          
          // reverse chronological order
          Assert.AreEqual(firstSlot.Id, secondResponseSlot.OperationSlotId, "Last slot does not come first");
          Assert.AreEqual(secondSlot.Id, firstResponseSlot.OperationSlotId, "More ancient slot does not come last");
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

          Assert.IsNotNull(getListOperationSlotResponse0, "No last operation slots (0)");
          Assert.IsNotNull(getListOperationSlotResponse0.List, "No last operation slots list (0)");
          Assert.AreEqual(0, getListOperationSlotResponse0.List.Count, "Not 0 operation slots");

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

          Assert.IsNotNull(getListOperationSlotResponse, "No last operation slots");
          Assert.IsNotNull(getListOperationSlotResponse.List, "No last operation slots list");
          Assert.AreEqual(1, getListOperationSlotResponse.List.Count, "Not 1 operation slot");
          
          OperationSlotV2DTO responseSlot = getListOperationSlotResponse.List[0];
          
          Assert.AreEqual(secondSlot.Id, responseSlot.OperationSlotId, "bad slot");
          
          // now only first slot
          getListOperationSlot.Begin = ConvertDTO.DateTimeUtcToIsoString(firstSlot.BeginDateTime);
          getListOperationSlot.End = ConvertDTO.DateTimeUtcToIsoString(firstSlot.EndDateTime);
          
          getListOperationSlotResponse =
            new GetListOfOperationSlotV2Service ()
            .GetWithoutCache (getListOperationSlot) as ListOfOperationSlotV2DTO;

          Assert.IsNotNull(getListOperationSlotResponse, "No last operation slots (2)");
          Assert.IsNotNull(getListOperationSlotResponse.List, "No last operation slots list (2)");
          Assert.AreEqual(1, getListOperationSlotResponse.List.Count, "Not 1 operation slot (2)");
          
          responseSlot = getListOperationSlotResponse.List[0];
          
          Assert.AreEqual(firstSlot.Id, responseSlot.OperationSlotId, "bad slot (2)");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstSlot.BeginDateTime), responseSlot.Begin, "bad begin");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstSlot.EndDateTime), responseSlot.End, "bad begin");
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

          Assert.IsNotNull(getListOperationSlotResponse, "No error dto");
          Assert.IsTrue(getListOperationSlotResponse.ErrorMessage.StartsWith("No monitored machine with id"), "Bad error msg");
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
          
          Assert.IsNotNull(getLastMachineStatusResponse, "No last machine extended status information");
          Assert.IsNotNull(getLastMachineStatusResponse.MachineStatus, "Null Machine Status");
          Assert.AreEqual(machineMode1.Id, getLastMachineStatusResponse.MachineStatus.MachineMode.Id, "Bad MachineMode");
          Assert.AreEqual(machineObsState1.Id, getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, "Bad MachineObservationState");
          Assert.AreEqual(lastSlot.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, "Bad ReasonSlot");
          Assert.AreEqual(reason1.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, "Bad Reason");
          Assert.IsFalse(getLastMachineStatusResponse.RequiredReason);
          // ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakeTransient(lastSlot);

          // change overwrite
          ((ReasonSlot)lastSlot).SetMainAutoReason (reason1, 100.0, true);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent(lastSlot);
          
          // refetch
          getLastMachineStatusResponse =
            new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;
          
          Assert.IsTrue(getLastMachineStatusResponse.RequiredReason); // this changed from false to true


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
          
          Assert.IsNotNull(getLastMachineStatusResponse2, "Last machine extended status information is not null");
          Assert.IsNotNull(getLastMachineStatusResponse2.MachineStatus, "Machine status information is not null");
          Assert.IsTrue(getLastMachineStatusResponse2.ReasonTooOld,"ReasonSlot is too old");
          Assert.IsFalse(getLastMachineStatusResponse2.RequiredReason, "Reason is not required");
          
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
          
          Assert.IsNotNull(getLastMachineStatusResponse, "No last machine extended status information");
          Assert.IsNotNull(getLastMachineStatusResponse.MachineStatus, "Null Machine Status");
          Assert.AreEqual(machineMode1.Id, getLastMachineStatusResponse.MachineStatus.MachineMode.Id, "Bad MachineMode");
          Assert.AreEqual(machineObsState1.Id, getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, "Bad MachineObservationState");
          Assert.AreEqual(veryLastSlot.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, "Bad ReasonSlot");
          Assert.AreEqual(reason1.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, "Bad Reason");
          // only veryLastSlot fetched => no overwrite required
          Assert.IsFalse(getLastMachineStatusResponse.RequiredReason, "False required reason");

          getLastMachineStatus.Begin = ConvertDTO.DateTimeUtcToIsoString(lastSlot.BeginDateTime.Value.AddHours(1));
          getLastMachineStatusResponse = new GetLastMachineStatusV2Service ()
            .GetWithoutCache (getLastMachineStatus) as LastMachineExtendedStatusV2DTO;
          
          Assert.IsNotNull(getLastMachineStatusResponse, "No last machine extended status information");
          Assert.IsNotNull(getLastMachineStatusResponse.MachineStatus, "Null Machine Status");
          Assert.AreEqual(machineMode1.Id, getLastMachineStatusResponse.MachineStatus.MachineMode.Id, "Bad MachineMode");
          Assert.AreEqual(machineObsState1.Id, getLastMachineStatusResponse.MachineStatus.MachineObservationState.Id, "Bad MachineObservationState");
          Assert.AreEqual(veryLastSlot.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Id, "Bad ReasonSlot");
          Assert.AreEqual(reason1.Id, getLastMachineStatusResponse.MachineStatus.ReasonSlot.Reason.Id, "Bad Reason");
          // lastSlot also fetched => overwrite required
          Assert.IsTrue(getLastMachineStatusResponse.RequiredReason, "True required reason");

          
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
          
          Assert.IsNotNull(GetMachineObservationStateListV2Response, "No machine observation state list");
          Assert.IsNotNull(GetMachineObservationStateListV2Response.List, "No machine observation state list's list");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.AreEqual(3, GetMachineObservationStateListV2Response.List.Count, "Not 3 machine observation states");
          MachineObservationStateSlotV2DTO mobs1 = GetMachineObservationStateListV2Response.List[0];
          MachineObservationStateSlotV2DTO mobs2 = GetMachineObservationStateListV2Response.List[1];
          
          // reverse chronological order
          Assert.AreEqual(secondSlot.Id, mobs1.Id, "Bad id for first machine observation state");
          Assert.AreEqual(firstSlot.Id,  mobs2.Id, "Bad id for second machine observation state");
          
          Assert.AreEqual(secondSlot.MachineObservationState.Display, mobs1.MachineObservationState.Text, "Bad text for first machine observation state");
          Assert.AreEqual(firstSlot.MachineObservationState.Display, mobs2.MachineObservationState.Text, "Bad text for second machine observation state");

          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(secondSlot.BeginDateTime), mobs1.Begin, "Bad begin for first machine observation state");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstSlot.BeginDateTime), mobs2.Begin, "Bad begin for second machine observation state");
          
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(secondSlot.EndDateTime), mobs1.End, "Bad end for first machine observation state");
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(firstSlot.EndDateTime), mobs2.End, "Bad end for second machine observation state");
          
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
          
          Assert.IsNotNull(GetMachineObservationStateListV2Response, "No machine observation state list");
          Assert.IsNotNull(GetMachineObservationStateListV2Response.List, "No machine observation state list's list");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.AreEqual(2, GetMachineObservationStateListV2Response.List.Count, "Not 2 machine observation states");
          MachineObservationStateSlotV2DTO mobs1 = GetMachineObservationStateListV2Response.List[0];
          
          // reverse chronological order
          Assert.AreEqual(secondSlot.Id, mobs1.Id, "Bad id for first machine observation state");
          
          Assert.AreEqual(secondSlot.MachineObservationState.Display, mobs1.MachineObservationState.Text, "Bad text for first machine observation state");

          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(secondSlot.BeginDateTime), mobs1.Begin, "Bad begin for first machine observation state");
          
          Assert.AreEqual(ConvertDTO.DateTimeUtcToIsoString(secondSlot.EndDateTime), mobs1.End, "Bad end for first machine observation state");
          
          GetMachineObservationStateListV2.Begin =
            ConvertDTO.DateTimeUtcToIsoString(beginDateTime1);

          GetMachineObservationStateListV2.End =
            ConvertDTO.DateTimeUtcToIsoString(endDateTime1);
          
          GetMachineObservationStateListV2Response =
            new GetMachineObservationStateListV2Service ().GetWithoutCache (GetMachineObservationStateListV2) as MachineObservationStateSlotListV2DTO;
          
          Assert.IsNotNull(GetMachineObservationStateListV2Response, "No machine observation state list (2)");
          Assert.IsNotNull(GetMachineObservationStateListV2Response.List, "No machine observation state list's list (2)");
          // there is already a slot with no end in LemoineUnitTests (the last in response) that we don't care about
          Assert.AreEqual(2, GetMachineObservationStateListV2Response.List.Count, "Not 2 machine observation states (2)");
          mobs1 = GetMachineObservationStateListV2Response.List[0];
          
          // reverse chronological order
          Assert.AreEqual(firstSlot.Id, mobs1.Id, "Bad id for first machine observation state (2)");
          
          Assert.AreEqual(firstSlot.MachineObservationState.Display, mobs1.MachineObservationState.Text, "Bad text for first machine observation state (2)");

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
          
          Assert.IsNotNull(getMachineObservationStateSelectionResponse, "No GetMachineObservationStateSelection response");
          Assert.AreEqual(14, getMachineObservationStateSelectionResponse.Count, "Bad number of machine observation states");

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
          
          Assert.IsNotNull(saveMachineObservationStateResponse as OkDTO, "Non-OK saveMachineObservationState");
          
          // just test there is a pending modification of the right type
          
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.IsNotNull(modifications, "no modification");
          Assert.AreEqual(1, modifications.Count, "not 1 modification");
          IModification modification = modifications[0];
          IMachineObservationStateAssociation machineObservationStateAssociation = modification as IMachineObservationStateAssociation;
          Assert.IsNotNull(machineObservationStateAssociation, "not a machine observation state association");
          Assert.AreEqual(begin.ToUniversalTime(), machineObservationStateAssociation.Begin.Value, "bad begin for machine observation state association");
          Assert.AreEqual(end.ToUniversalTime(), machineObservationStateAssociation.End.Value, "bad end for machine observation state association");
          Assert.AreEqual(machObs1.Id, machineObservationStateAssociation.MachineObservationState.Id);

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
          
          Assert.IsNotNull(SaveMachineObservationStateV2Response as OkDTO, "Non-OK SaveMachineObservationStateV2");
          
          // just test there is a pending modification of the right type
          
          IList<IModification> modifications = ModelDAOHelper.DAOFactory.ModificationDAO.FindAll().ToList ();
          Assert.IsNotNull(modifications, "no modification");
          Assert.AreEqual(1, modifications.Count, "not 1 modification");
          IModification modification = modifications[0];
          IMachineObservationStateAssociation machineObservationStateAssociation = modification as IMachineObservationStateAssociation;
          Assert.IsNotNull(machineObservationStateAssociation, "not a machine observation state association");
          Assert.AreEqual(begin.ToUniversalTime(), machineObservationStateAssociation.Begin.Value, "bad begin for machine observation state association");
          Assert.AreEqual(end.ToUniversalTime(), machineObservationStateAssociation.End.Value, "bad end for machine observation state association");
          Assert.AreEqual(machObs1.Id, machineObservationStateAssociation.MachineObservationState.Id);

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
      Assert.AreEqual(ticks, 0);
      ticks = 1000;
      DateTime originPlus1s = origin.AddSeconds(1);
      DateTime ticksConvertedDt = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticks);
      Assert.AreEqual(DateTimeKind.Utc, ticksConvertedDt.Kind);
      Assert.AreEqual(originPlus1s, ticksConvertedDt);
      DateTime current = DateTime.UtcNow; // new DateTime(2000, 1, 1, 12, 15, 35, DateTimeKind.Utc);
      Assert.AreEqual(DateTimeKind.Utc, current.Kind);
      ticks = ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(current);
      DateTime shouldBeCurrent = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticks);
      Assert.AreEqual(shouldBeCurrent.Millisecond, current.Millisecond);
      // Assert.AreEqual(shouldBeCurrent, current); // no cf. ms resolution only
      long ticks2 =  ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(shouldBeCurrent);
      Assert.AreEqual(ticks2, ticks);
      
      DateTime preOrigin = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      long ticksPre = ConvertDTO.ConvertDateTimeToJavaTotalTicksMilliseconds(preOrigin);
      DateTime convertedPreOrigin = ConvertDTO.ConvertJavaTotalTicksMillisecondsToDateTime(ticksPre);
      Assert.AreEqual(preOrigin, convertedPreOrigin, "Bad convert for 1950");
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
