// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the persistent class LineMachine.
  /// </summary>
  [TestFixture]
  public class LineMachine_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LineMachine_UnitTest).FullName);
    
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
        IMachineDAO machineDAO = daoFactory.MachineDAO;
        ILineMachineDAO lineMachineDAO = daoFactory.LineMachineDAO;
        
        // Creation of 2 Lines
        ILine line1 = ModelDAOHelper.ModelFactory.CreateLine();
        ILine line2 = ModelDAOHelper.ModelFactory.CreateLine();
        lineDAO.MakePersistent(line1);
        lineDAO.MakePersistent(line2);
        
        // Reference of 4 Machines
        IList<IMachine> machines = machineDAO.FindAll();
        Assert.That (machines, Has.Count.GreaterThanOrEqualTo (4), "not enough machines in the database (at least 4)");
        IMachine machine1 = machines[0];
        IMachine machine2 = machines[1];
        IMachine machine3 = machines[2];
        IMachine machine4 = machines[3];
        
        // Reference to 1 Operation
        IList<IOperation> operations = daoFactory.OperationDAO.FindAll();
        Assert.That (operations, Is.Not.Empty, "not enough machines in the database (at least 1)");
        IOperation operation1 = operations[0];
                
        // Creation de 5 LineMachines
        ILineMachine lineMachine1 = ModelDAOHelper.ModelFactory.CreateLineMachine(line1, machine1, operation1);
        ILineMachine lineMachine2 = ModelDAOHelper.ModelFactory.CreateLineMachine(line1, machine2, operation1);
        ILineMachine lineMachine3 = ModelDAOHelper.ModelFactory.CreateLineMachine(line1, machine3, operation1);
        ILineMachine lineMachine4 = ModelDAOHelper.ModelFactory.CreateLineMachine(line2, machine3, operation1);
        ILineMachine lineMachine5 = ModelDAOHelper.ModelFactory.CreateLineMachine(line2, machine4, operation1);
        lineMachineDAO.MakePersistent(lineMachine1);
        lineMachineDAO.MakePersistent(lineMachine2);
        lineMachineDAO.MakePersistent(lineMachine3);
        lineMachineDAO.MakePersistent(lineMachine4);
        lineMachineDAO.MakePersistent(lineMachine5);

        Assert.Multiple (() => {
          // Check the different numbers of LineMachines
          Assert.That (lineMachineDAO.FindAllByLine (line1), Has.Count.EqualTo (3), "wrong count of lineMachines for line 1");
          Assert.That (lineMachineDAO.FindAllByLine (line2), Has.Count.EqualTo (2), "wrong count of lineMachines for line 2");
          Assert.That (lineMachineDAO.FindAllByMachine (machine1), Has.Count.EqualTo (1), "wrong count of lineMachines for machine 1");
          Assert.That (lineMachineDAO.FindAllByMachine (machine2), Has.Count.EqualTo (1), "wrong count of lineMachines for machine 2");
          Assert.That (lineMachineDAO.FindAllByMachine (machine3), Has.Count.EqualTo (2), "wrong count of lineMachines for machine 3");
          Assert.That (lineMachineDAO.FindAllByMachine (machine4), Has.Count.EqualTo (1), "wrong count of lineMachines for machine 4");
        });

        // Remove 1 LineMachine and check the new count
        lineMachineDAO.MakeTransient(lineMachine5);
        Assert.Multiple (() => {
          Assert.That (lineMachineDAO.FindAllByLine (line1), Has.Count.EqualTo (3), "wrong count of lineMachines for line 1");
          Assert.That (lineMachineDAO.FindAllByLine (line2), Has.Count.EqualTo (1), "wrong count of lineMachines for line 2");
          Assert.That (lineMachineDAO.FindAllByMachine (machine1), Has.Count.EqualTo (1), "wrong count of lineMachines for machine 1");
          Assert.That (lineMachineDAO.FindAllByMachine (machine2), Has.Count.EqualTo (1), "wrong count of lineMachines for machine 2");
          Assert.That (lineMachineDAO.FindAllByMachine (machine3), Has.Count.EqualTo (2), "wrong count of lineMachines for machine 3");
          Assert.That (lineMachineDAO.FindAllByMachine (machine4), Is.Empty, "wrong count of lineMachines for machine 4");
        });

        transaction.Rollback ();
      }
    }
  }
}
