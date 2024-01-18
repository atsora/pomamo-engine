// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class TODO: MyClassName.
  /// </summary>
  [TestFixture]
  public class MachineModification_UnitTest: WithDayTimeStamp
  {
    string previousDSNName;
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModification_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineModification_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    [Test]
    public void TestMachineModificationPriority1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);

        DateTime startDateTime = DateTime.UtcNow;
        IReasonMachineAssociation association = ModelDAOHelper.ModelFactory
          .CreateReasonMachineAssociation (machine, R (1, 2));
        association.SetManualReason (reason);
        association.Priority = 1000;
        association = new ReasonMachineAssociationDAO ()
          .MakePersistent (association);

        ModelDAOHelper.DAOFactory.Flush ();

        Assert.That (association.StatusPriority, Is.EqualTo (1000));
        
        var statusPriorityRequest = $@"SELECT modificationstatuspriority 
FROM machinemodificationstatus
WHERE modificationid={((IDataWithId<long>)association).Id}";
        var statusPriority = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (statusPriorityRequest)
          .AddScalar ("modificationstatuspriority", NHibernate.NHibernateUtil.Int32)
          .UniqueResult<int> ();
        Assert.That (statusPriority, Is.EqualTo (1000));

        transaction.Rollback ();
      }
    }

    [Test]
    public void TestMachineModificationPriority2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);

        var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertManualReason (machine, R (1, 2), reason, 100, "");

        ModelDAOHelper.DAOFactory.Flush ();

        var reasonMachineAssociation = ModelDAOHelper.DAOFactory
          .ReasonMachineAssociationDAO.FindById (modificationId, machine);
        Assert.That (reasonMachineAssociation.StatusPriority, Is.EqualTo (1000));

        var machineModification = ModelDAOHelper.DAOFactory
          .MachineModificationDAO.FindById (modificationId, machine);
        Assert.That (machineModification.StatusPriority, Is.EqualTo (1000));

        // The code below works only if the table is partitioned
        /*
        {
          var statusPriorityRequest = $@"SELECT modificationstatuspriority 
FROM machinemodificationstatus
WHERE modificationid={modificationId}";
          var statusPriority = NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (statusPriorityRequest)
            .AddScalar ("modificationstatuspriority", NHibernate.NHibernateUtil.Int32)
            .UniqueResult<int?> ();
          Assert.IsFalse (statusPriority.HasValue);
        }

        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .CreateNewAnalysisStatusNoLimit (machine, true, 0);

        {
          var statusPriorityRequest = $@"SELECT modificationstatuspriority 
FROM machinemodificationstatus
WHERE modificationid={modificationId}";
          var statusPriority = NHibernateHelper.GetCurrentSession ()
            .CreateSQLQuery (statusPriorityRequest)
            .AddScalar ("modificationstatuspriority", NHibernate.NHibernateUtil.Int32)
            .UniqueResult<int?> ();
          Assert.IsTrue (statusPriority.HasValue);
          Assert.AreEqual (1000, statusPriority.Value);
        }
        */

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test an update of a machinemodificationstatus
    /// </summary>
    [Test]
    public void TestMachineModificationStatusUpdate()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);

        DateTime startDateTime = DateTime.UtcNow;
        IReasonMachineAssociation association = ModelDAOHelper.ModelFactory
          .CreateReasonMachineAssociation (machine, R(1, 2));
        association.SetManualReason (reason);
        association = new ReasonMachineAssociationDAO ()
          .MakePersistent (association);
        
        ModelDAOHelper.DAOFactory.Flush ();
        
        association.MarkAsTimeout (startDateTime);
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .MakePersistent (association);
        ModelDAOHelper.DAOFactory.Flush ();
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestFindById1 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        const string request1 = @"INSERT INTO machinemodification (machinemodificationmachineid, revisionid, parentglobalmodificationid, parentmachinemodificationid, modificationpriority, modificationdatetime, modificationauto, modificationreferencedtable, modificationid)
 VALUES (1, NULL, NULL, NULL, 100, '2016-01-27 15:10:02', FALSE, 'ReasonMachineAssociation', 10829);
";
        ExecuteRequest (request1);
        const string request2 = @"INSERT INTO ReasonMachineAssociation (machineid, reasonid, reasonmachineassociationbegin, reasonmachineassociationend, reasondetails, reasonmachineassociationoption, reasonmachineassociationreasonscore, reasonmachineassociationkind, modificationid)
 VALUES (1, 21, '2011-08-01 00:00:00', '2011-08-02 00:00:00', NULL, NULL, 100.0, 4, 10829);
";
        ExecuteRequest (request2);
        
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);

        IMachineModification modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (10829, machine);
        Assert.IsNotNull (modification);

        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestFindById2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        const string request1 = @"INSERT INTO machinemodification (machinemodificationmachineid, revisionid, parentglobalmodificationid, parentmachinemodificationid, modificationpriority, modificationdatetime, modificationauto, modificationreferencedtable, modificationid)
 VALUES (1, NULL, NULL, NULL, 100, '2016-01-27 15:10:02', FALSE, 'ReasonMachineAssociation', 10829);
";
        ExecuteRequest (request1);
        const string request2 = @"INSERT INTO ReasonMachineAssociation (machineid, reasonid, reasonmachineassociationbegin, reasonmachineassociationend, reasondetails, reasonmachineassociationoption, reasonmachineassociationreasonscore, reasonmachineassociationkind, modificationid)
 VALUES (1, 21, '2011-08-01 00:00:00', '2011-08-02 00:00:00', NULL, NULL, 100.0, 4, 10829);
";
        ExecuteRequest (request2);
        const string request3 = @"INSERT INTO machinemodificationstatus (machinemodificationstatusmachineid, modificationid)
VALUES (1, 10829)";
        ExecuteRequest (request3);
        
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);

        IMachineModification modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (10829, machine);
        Assert.IsNotNull (modification);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestFindById3 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);

        long associationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertManualReason (machine, R (1, 2), reason, 100, null);
        
        ModelDAOHelper.DAOFactory.Flush ();
        
        IMachineModification modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (associationId, machine);
        Assert.IsNotNull (modification);
        
        transaction.Rollback ();
      }
    }
    
    
    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestDelete ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);
        
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .Delete (machine, AnalysisStatus.Delete, null, 50, 2000, null);
        ModelDAOHelper.DAOFactory.Flush ();

        DateTime startDateTime = DateTime.UtcNow;
        IReasonMachineAssociation association = ModelDAOHelper.ModelFactory
          .CreateReasonMachineAssociation (machine, R(1, 2));
        association.SetManualReason (reason);
        association = new ReasonMachineAssociationDAO ()
          .MakePersistent (association);
        ModelDAOHelper.DAOFactory.Flush ();
        long associationId = ((IDataWithId<long>)association).Id;
        
        ((MachineModification)association).MarkAsTimeout (startDateTime);
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .MakePersistent (association);
        ModelDAOHelper.DAOFactory.Flush ();
        NHibernateHelper.GetCurrentSession ().Evict (association);
        
        IMachineModification machineModification;
        machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (associationId, machine);
        Assert.IsNotNull (machineModification);
        ModelDAOHelper.DAOFactory.Flush ();

        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .Delete (machine, AnalysisStatus.Timeout, null, 50, 2000, null);
        NHibernateHelper.GetCurrentSession ().Evict (machineModification);
        ModelDAOHelper.DAOFactory.Flush ();
        
        machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (associationId, machine);
        Assert.IsNull (machineModification);
        ModelDAOHelper.DAOFactory.Flush ();
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test FindById
    /// </summary>
    [Test]
    public void TestGetMaxModificationId ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (21);
        
        {
          long? maxModificationId = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetMaxModificationId (machine);
          Assert.That (!maxModificationId.HasValue);
        }

        long associationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertManualReason (machine, R (1, 2), reason, 100.0, "");
        
        ModelDAOHelper.DAOFactory.Flush ();
        
        {
          long? maxModificationId = ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetMaxModificationId (machine);
          Assert.Multiple (() => {
            Assert.That (maxModificationId.HasValue);
            Assert.That (associationId == maxModificationId.Value);
          });
        }
        
        transaction.Rollback ();
      }
    }
    
    void ExecuteRequest (string request)
    {
      var connection = ModelDAOHelper.DAOFactory.GetConnection ();
      using (var command = connection.CreateCommand ())
      {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        command.CommandText = request;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        command.ExecuteNonQuery();
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
