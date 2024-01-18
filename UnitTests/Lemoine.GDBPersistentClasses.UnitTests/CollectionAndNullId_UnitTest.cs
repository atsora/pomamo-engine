// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Collections;
using NHibernate;
using NUnit.Framework;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for collections of Persistent objects (with Ids going from 0 to non-zero)
  /// </summary>
  [TestFixture]
  public class CollectionAndNullId_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CollectionAndNullId_UnitTest).FullName);

    #region  Members
    IDAOFactory m_daoFactory;
    IDAOSession m_daoSession;
    ISession m_session;
    string m_previousDSNName;
    #endregion // Members

    #region Getters/Setters
    IDAOFactory DaoFactory { get { return m_daoFactory; } }
    ISession Session { get { return m_session; } }
    #endregion // Getters/Setters

    // simple test (for those classes having no DAO)
    public void TestSimpleNullIdToNonNullId<T, ID> (T ObjWithId, T OtherObjWithId)
    {
      string FailString = "TestSimpleNullIdToNonNullId failure on class " + typeof (T).FullName;
      ISet<T> customSet = new InitialNullIdSet<T, ID> ();

      Assert.That (OtherObjWithId, Is.Not.EqualTo (ObjWithId), FailString);

      Lemoine.Collections.IDataWithId<ID> CastedObjWithId = (Lemoine.Collections.IDataWithId<ID>)ObjWithId;
      Lemoine.Collections.IDataWithId<ID> CastedOtherObjWithId = (Lemoine.Collections.IDataWithId<ID>)OtherObjWithId;

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);
      });

      customSet.Add (ObjWithId);
      customSet.Add (OtherObjWithId);

      Assert.Multiple (() => {
        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });
    }

    public void TestNullIdToNonNullId<T, ID> (T ObjWithId, T OtherObjWithId,
                                             IGenericDAO<T, ID> daoFactory)
    // where T : Lemoine.Collections.IDataWithId
    {
      string FailString = "TestNullIdToNonNullId failure on class " + typeof (T).FullName;
      ISet<T> customSet = new InitialNullIdSet<T, ID> ();

      Assert.That (OtherObjWithId, Is.Not.EqualTo (ObjWithId), FailString);

      Lemoine.Collections.IDataWithId<ID> CastedObjWithId = (Lemoine.Collections.IDataWithId<ID>)ObjWithId;
      Lemoine.Collections.IDataWithId<ID> CastedOtherObjWithId = (Lemoine.Collections.IDataWithId<ID>)OtherObjWithId;

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);
      });

      customSet.Add (ObjWithId);
      customSet.Add (OtherObjWithId);

      Assert.Multiple (() => {
        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });

      daoFactory.MakePersistent (ObjWithId);

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Not.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);

        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });
    }

    public void TestNullIdToNonNullIdByMachine<T, ID> (T ObjWithId, T OtherObjWithId,
                                             IGenericByMachineDAO<T, ID> daoFactory)
      where T : IPartitionedByMachine
      // where T : Lemoine.Collections.IDataWithId
    {
      string FailString = "TestNullIdToNonNullId failure on class " + typeof (T).FullName;
      ISet<T> customSet = new InitialNullIdSet<T, ID> ();

      Assert.That (OtherObjWithId, Is.Not.EqualTo (ObjWithId), FailString);

      Lemoine.Collections.IDataWithId<ID> CastedObjWithId = (Lemoine.Collections.IDataWithId<ID>)ObjWithId;
      Lemoine.Collections.IDataWithId<ID> CastedOtherObjWithId = (Lemoine.Collections.IDataWithId<ID>)OtherObjWithId;

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);
      });

      customSet.Add (ObjWithId);
      customSet.Add (OtherObjWithId);

      Assert.Multiple (() => {
        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });

      daoFactory.MakePersistent (ObjWithId);

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Not.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);

        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });
    }

    public void TestNullIdToNonNullIdSorted<T, ID> (T ObjWithId, T OtherObjWithId,
                                                   IGenericDAO<T, int> daoFactory)
      where T : IComparable
      // where T : Lemoine.Collections.IDataWithId
    {
      string FailString = "TestNullIdToNonNullIdSorted failure on class " + typeof (T).FullName;
      ISet<T> customSet = new InitialNullIdSortedSet<T, ID> ();

      Assert.That (OtherObjWithId, Is.Not.EqualTo (ObjWithId), FailString);

      Lemoine.Collections.IDataWithId<ID> CastedObjWithId = (Lemoine.Collections.IDataWithId<ID>)ObjWithId;
      Lemoine.Collections.IDataWithId<ID> CastedOtherObjWithId = (Lemoine.Collections.IDataWithId<ID>)OtherObjWithId;

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);
      });

      customSet.Add (ObjWithId);
      customSet.Add (OtherObjWithId);

      Assert.Multiple (() => {
        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });

      daoFactory.MakePersistent (ObjWithId);

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Not.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);

        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });
    }

    public void TestNullIdToNonNullIdBag<T, ID> (T ObjWithId, T OtherObjWithId,
                                                IGenericDAO<T, int> daoFactory)
    {
      // bag means something implementing IList
      string FailString = "TestNullIdToNonNullIdSorted failure on class " + typeof (T).FullName;
      IList<T> customSet = new List<T> ();

      Assert.That (OtherObjWithId, Is.Not.EqualTo (ObjWithId), FailString);

      Lemoine.Collections.IDataWithId<ID> CastedObjWithId = (Lemoine.Collections.IDataWithId<ID>)ObjWithId;
      Lemoine.Collections.IDataWithId<ID> CastedOtherObjWithId = (Lemoine.Collections.IDataWithId<ID>)OtherObjWithId;

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);
      });

      customSet.Add (ObjWithId);
      customSet.Add (OtherObjWithId);

      Assert.Multiple (() => {
        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });

      daoFactory.MakePersistent (ObjWithId);

      Assert.Multiple (() => {
        Assert.That (CastedObjWithId.Id, Is.Not.Zero, FailString);
        Assert.That (CastedOtherObjWithId.Id, Is.Zero, FailString);

        Assert.That (customSet.Contains (ObjWithId), Is.EqualTo (true), FailString);
        Assert.That (customSet.Contains (OtherObjWithId), Is.EqualTo (true), FailString);
      });
    }

    /// <summary>
    /// Test combination of InitialNullIdSet with persistent objects which go into NH collections
    /// Also tests Equals and GetHashCode methods of persistent objects
    /// </summary>
    [Test]
    public void TestNullIdToNonNull ()
    {
      // test for bag: roughly demonstrates no custom implementation needed for <bag> NH mapping
      // if actually implemented as a List (or any structure not relying on GetHashCode)
      TestNullIdToNonNullIdBag<IProject, int> (new Project (), new Project (), DaoFactory.ProjectDAO);

      // test for Project
      TestNullIdToNonNullId<IProject, int> (new Project (), new Project (), DaoFactory.ProjectDAO);

      // test for Sequence (need to build an object that can be persisted)
      // Sequence is ordered, no two sequences mapped in same sorted collection should
      // have same order (otherwise one is lost)
      ISequence sequence1 = new Sequence ();
      sequence1.Order = 1;
      ISequence sequence2 = new Sequence ();
      sequence2.Order = 2;
      IOperationType operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
      IOperation operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
      IPath path = new Path ();
      path.Operation = operation;
      sequence1.Path = path;
      sequence2.Path = path;
      DaoFactory.OperationDAO.MakePersistent (operation);
      DaoFactory.PathDAO.MakePersistent (path);

      TestNullIdToNonNullIdSorted<ISequence, int> (sequence1, sequence2, DaoFactory.SequenceDAO);
      // test for MachineModule
      IMachineModule machineModule1 = new MachineModule (NHibernateHelper.GetCurrentSession ().Get<MonitoredMachine> (1));
      IMachineModule machineModule2 = new MachineModule (NHibernateHelper.GetCurrentSession ().Get<MonitoredMachine> (2));
      TestNullIdToNonNullId<IMachineModule, int> (machineModule1, machineModule2, DaoFactory.MachineModuleDAO);

      // test for Operation
      IOperation operation2 = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
      IOperation operation3 = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
      TestNullIdToNonNullId<IOperation, int> (operation2, operation3, DaoFactory.OperationDAO);

      // test for Component
      IComponentType componentType = NHibernateHelper.GetCurrentSession ().Get<ComponentType> (1);
      IComponent component1 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
      IComponent component2 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
      TestNullIdToNonNullId<IComponent, int> (component1, component2, DaoFactory.ComponentDAO);

      // test for IntermediateWorkPiece
      IIntermediateWorkPiece iwp1 = new IntermediateWorkPiece (operation);
      IIntermediateWorkPiece iwp2 = new IntermediateWorkPiece (operation);
      TestNullIdToNonNullId<IIntermediateWorkPiece, int> (iwp1, iwp2, DaoFactory.IntermediateWorkPieceDAO);

      // test for Machine
      IMachine machine1 = new Machine ();
      machine1.MonitoringType = NHibernateHelper.GetCurrentSession ().Get<MachineMonitoringType> (1);
      IMachine machine2 = new Machine ();
      machine2.MonitoringType = machine1.MonitoringType;
      TestNullIdToNonNullId<IMachine, int> (machine1, machine2, DaoFactory.MachineDAO);

      // test for MonitoredMachines
      IMonitoredMachine mmachine1 = new MonitoredMachine ();
      mmachine1.MonitoringType = NHibernateHelper.GetCurrentSession ().Get<MachineMonitoringType> (1);
      IMonitoredMachine mmachine2 = new MonitoredMachine ();
      mmachine2.MonitoringType = mmachine1.MonitoringType;
      TestNullIdToNonNullId<IMonitoredMachine, int> (mmachine1, mmachine2, DaoFactory.MonitoredMachineDAO);

      // test for Project
      IProject proj1 = new Project ();
      IProject proj2 = new Project ();
      TestNullIdToNonNullId<IProject, int> (proj1, proj2, DaoFactory.ProjectDAO);

      // test for Reason
      IReasonGroup reasonGroup = ModelDAOHelper.DAOFactory.ReasonGroupDAO.FindById (1);
      IReason reason1 = ModelDAOHelper.ModelFactory.CreateReason (reasonGroup);
      IReason reason2 = ModelDAOHelper.ModelFactory.CreateReason (reasonGroup);
      TestNullIdToNonNullId<IReason, int> (reason1, reason2, DaoFactory.ReasonDAO);

      // test for Modification
      IMachine machineForModif = new Machine ();
      machineForModif.MonitoringType = NHibernateHelper.GetCurrentSession ().Get<MachineMonitoringType> (1);
      DaoFactory.MachineDAO.MakePersistent (machineForModif);
      IOperationMachineAssociation modif1 =
        ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machineForModif,
                                                                       DateTime.UtcNow);

      IOperationMachineAssociation modif2 =
        ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machineForModif,
                                                                       DateTime.UtcNow);

      TestNullIdToNonNullIdByMachine<IOperationMachineAssociation, long> (modif1, modif2,
                                                                DaoFactory.OperationMachineAssociationDAO);

      // test for Stamp
      IStamp stamp1 = new Stamp ();
      IStamp stamp2 = new Stamp ();
      TestNullIdToNonNullId<IStamp, int> (stamp1, stamp2, DaoFactory.StampDAO);

      // test for StampingValue

      ISequence seqForStampingValue = new Sequence ();
      IPath pathForStampingValue = new Path ();
      IOperation opForStampingValue = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
      seqForStampingValue.Path = pathForStampingValue;
      pathForStampingValue.Operation = opForStampingValue;
      DaoFactory.OperationDAO.MakePersistent (opForStampingValue);
      DaoFactory.PathDAO.MakePersistent (pathForStampingValue);
      IField fieldForStampingValue1 = ModelDAOHelper.ModelFactory.CreateFieldFromName ("fieldcode1", "fieldname1");
      IField fieldForStampingValue2 = ModelDAOHelper.ModelFactory.CreateFieldFromName ("fieldcode2", "fieldname2");
      // no cascade on StampingValue thus need to explicitly persist Sequence and Field
      DaoFactory.SequenceDAO.MakePersistent (seqForStampingValue);
      DaoFactory.FieldDAO.MakePersistent (fieldForStampingValue1);
      DaoFactory.FieldDAO.MakePersistent (fieldForStampingValue2);
      IStampingValue stampingValue1 = new StampingValue (seqForStampingValue, fieldForStampingValue1);
      IStampingValue stampingValue2 = new StampingValue (seqForStampingValue, fieldForStampingValue2);
      TestNullIdToNonNullId<IStampingValue, int> (stampingValue1, stampingValue2, DaoFactory.StampingValueDAO);

      // test for WorkOrder
      IWorkOrder workOrder1 = new WorkOrder ();
      IWorkOrder workOrder2 = new WorkOrder ();
      TestNullIdToNonNullId<IWorkOrder, int> (workOrder1, workOrder2, DaoFactory.WorkOrderDAO);
    }

    [SetUp]
    public void Setup ()
    {
      m_daoSession = m_daoFactory.OpenSession ();
      m_session = NHibernateHelper.GetCurrentSession ();
      //      m_transaction = m_daoSession.BeginTransaction();
    }

    [TearDown]
    public void TearDown ()
    {
      m_session.Dispose ();
      m_daoSession.Dispose ();
    }

    [OneTimeSetUp]
    public void Init ()
    {

      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      m_daoFactory = ModelDAOHelper.DAOFactory;
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }

  }
}
