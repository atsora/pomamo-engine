// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class IsoFileMachineModuleAssociation
  /// </summary>
  [TestFixture]
  public class IsoFileMachineModuleAssociation_UnitTest: WithMinuteTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (IsoFileMachineModuleAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public IsoFileMachineModuleAssociation_UnitTest ()
      : base (UtcDateTime.From (2011, 08, 01, 12, 00, 00))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis (via Apply)
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
        
        // Reference data
        IMachineModule machineModule = daoFactory.MachineModuleDAO.FindById(3);
        IIsoFile isoFile1 = daoFactory.IsoFileDAO.FindById(1);
        IIsoFile isoFile2 = daoFactory.IsoFileDAO.FindById(2);
        IIsoFile isoFile3 = daoFactory.IsoFileDAO.FindById(3);
        IIsoFile isoFile4 = daoFactory.IsoFileDAO.FindById(4);
        IIsoFile isoFile5 = daoFactory.IsoFileDAO.FindById(5);
        
        // Existing IsoFileSlots
        {
          IIsoFileSlot existingSlot = modelFactory.CreateIsoFileSlot(machineModule, isoFile1,
                                                                     R(0, 10));
          daoFactory.IsoFileSlotDAO.MakePersistent(existingSlot);
        }
        {
          IIsoFileSlot existingSlot = modelFactory.CreateIsoFileSlot(machineModule, isoFile2,
                                                                     R(10, 20));
          daoFactory.IsoFileSlotDAO.MakePersistent(existingSlot);
        }
        {
          IIsoFileSlot existingSlot = modelFactory.CreateIsoFileSlot(machineModule, isoFile3,
                                                                     R(20, 30));
          daoFactory.IsoFileSlotDAO.MakePersistent(existingSlot);
        }
        
        // New association 12:25:00 -> oo
        {
          IIsoFileMachineModuleAssociation association =
            new IsoFileMachineModuleAssociation(machineModule, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 25, 00)));
          association.IsoFile = isoFile4;
          association.DateTime = UtcDateTime.From (2011, 08, 01, 12, 40, 00);
          association.IsoFile = isoFile4;
          association.Apply();
        }
        
        // New association 12:32:00 -> oo
        {
          IIsoFileMachineModuleAssociation association =
            new IsoFileMachineModuleAssociation(machineModule, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 32, 00)));
          association.IsoFile = isoFile5;
          association.DateTime = UtcDateTime.From (2011, 08, 01, 12, 40, 00);
          association.Apply();
        }
        
        // Check the values
        {
          IList<IsoFileSlot> slots =
            session.CreateCriteria<IsoFileSlot> ()
            .Add (Expression.Eq ("MachineModule", machineModule))
            .AddOrder (Order.Asc ("BeginDateTime"))
            .List<IsoFileSlot> ();
          
          Assert.AreEqual (5, slots.Count, "Number of isofile slots");
          int i = 0;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile1, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 00, 00), slots[i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 10, 00), slots[i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile2, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 10, 00), slots[i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 20, 00), slots[i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile3, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 20, 00), slots[i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 25, 00), slots[i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile4, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 25, 00), slots[i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 32, 00), slots[i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile5, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 32, 00), slots[i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        
        // New association: isofile null 12:36:00 -> oo
        {
          IIsoFileMachineModuleAssociation association =
            new IsoFileMachineModuleAssociation(machineModule, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 01, 12, 36, 00)));
          association.IsoFile = null;
          association.DateTime = UtcDateTime.From (2011, 08, 01, 12, 40, 00);
          association.Apply();
        }
        
        // Check the values of the last slot
        {
          IList<IsoFileSlot> slots =
            session.CreateCriteria<IsoFileSlot> ()
            .Add (Expression.Eq ("MachineModule", machineModule))
            .AddOrder (Order.Asc ("BeginDateTime"))
            .List<IsoFileSlot> ();
          
          Assert.AreEqual (5, slots.Count, "Number of isofile slots");
          int i = 4;
          Assert.AreEqual (machineModule, slots[i].MachineModule);
          Assert.AreEqual (isoFile5, slots[i].IsoFile);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 32, 00), slots[i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 12, 36, 00), slots[i].EndDateTime.Value);
        }
        
        transaction.Rollback ();
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
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
