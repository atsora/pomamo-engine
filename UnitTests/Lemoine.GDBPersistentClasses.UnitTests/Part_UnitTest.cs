// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Part
  /// </summary>
  [TestFixture]
  public class Part_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Part_UnitTest).FullName);

    /// <summary>
    /// Test the insertion of a new part
    /// </summary>
    [Test]
    public void TestInsert()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        ComponentType componentType = session.Get<ComponentType> (1);
        PartView part = new PartView ();
        part.Name = "TestInsertPart";
        part.Type = componentType;
        session.Save (part);
        session.Flush ();
        session.CreateQuery ("delete from PartView part " +
                             "where part.Name=?")
          .SetParameter (0, "TestInsertPart")
          .ExecuteUpdate ();
        session.Flush ();
      }
    }
    
    /// <summary>
    /// Test the insertion of a new part, then read a project
    /// </summary>
    [Test]
    public void TestInsertReadProject()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);
        IPartView part = new PartView ();
        part.Name = "TestInsertPart";
        part.Type = componentType;
        session.Save (part);
        session.Flush ();
        Project project = session.CreateCriteria<Project> ()
          .Add (Restrictions.Eq ("Name", "TestInsertPart"))
          .UniqueResult<Project> ();
        Assert.NotNull (project);
        Assert.AreEqual ("TestInsertPart", project.Name);
        project = session.CreateQuery ("from Project foo " +
                                       "where foo.Name=?")
          .SetParameter (0, "TestInsertPart")
          .UniqueResult<Project> ();
        Assert.NotNull (project);
        Assert.AreEqual ("TestInsertPart", project.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the FindPersistentClass method after an insertion
    /// </summary>
    [Test]
    public void TestFindPersistentClass()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById (1);
        IPartView part = new PartView ();
        part.Name = "TestInsertPart";
        part.Type = componentType;
        session.Save (part);
        session.Flush ();
        PartView part2 = new PartView ();
        part2.Name = "TestInsertPart";
        part = (IPartView) part2.FindPersistentClass (session);
        Assert.NotNull (part);
        Assert.AreEqual ("TestInsertPart", part.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test Delete
    /// </summary>
    [Test]
    public void TestDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById (1);
        IPartView part = new PartView ();
        part.Name = "TestInsertPart";
        part.Type = componentType;
        session.Save (part);
        session.Flush ();
        session.Delete (part);
        session.Flush ();
        transaction.Rollback ();
      }
      
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        PartView part = session.Get<PartView> (1);
        session.Delete (part);
        session.Flush ();
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test Update
    /// </summary>
    [Test]
    public void TestUpdate()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);
        IPartView part = new PartView ();
        part.Name = "TestInsertPart";
        part.Type = componentType;
        session.Save (part);
        session.Flush ();
        part.Code = "Code";
        session.Update (part);
        session.Flush ();
        PartView part2 = session.Get<PartView> (part.ComponentId);
        Assert.NotNull (part2);
        Assert.AreEqual ("Code", part2.Code);
        transaction.Rollback ();
      }
      
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        PartView part = session.Get<PartView> (1);
        part.Code = "TestCode";
        session.Update (part);
        session.Flush ();
        PartView part2 = session.Load<PartView> (1);
        Assert.NotNull (part2);
        Assert.AreEqual ("TestCode", part2.Code);
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test select is case insensitive on citext fields
    /// with Criteria
    /// </summary>
    [Test]
    public void TestCaseInsensitiveWithCriteria()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        PartView part = session.CreateCriteria<PartView> ()
          .Add (Restrictions.Eq ("Name", "component1"))
          .UniqueResult<PartView> ();
        Assert.NotNull (part);
        Assert.AreEqual (1, part.ComponentId);
      }
    }
    
    /// <summary>
    /// Test select is case insensitive on citext fields
    /// with HQL
    /// </summary>
    [Test]
    public void TestCaseInsensitiveWithHQL()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        PartView part =
          session.CreateQuery ("from PartView foo where foo.Name=?")
          .SetParameter (0, "component1")
          .UniqueResult<PartView> ();
        Assert.NotNull (part);
        Assert.AreEqual (1, part.ComponentId);
        Assert.AreEqual ("COMPONENT1", part.Name);
      }
    }
    
    /// <summary>
    /// Test the merge function with a created part
    /// </summary>
    [Test]
    public void TestMergeCreated()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);
        IPartView newTempPart = new PartView ();
        newTempPart.Name = "CreatedPart";
        newTempPart.Type = componentType;
        session.Save (newTempPart);
        session.Flush ();
        IPart oldPart = daoFactory.ComponentDAO.FindById(4).Part as IPart;
        IPart newPart = daoFactory.ComponentDAO.FindById(newTempPart.ComponentId).Part as IPart;
        IPart merged = daoFactory.PartDAO
          .Merge (oldPart, newPart,
                  ConflictResolution.Keep);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (newTempPart.ComponentId, merged.ComponentId);
        Assert.AreEqual ("CreatedPart", merged.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the merge function with an existing part
    /// </summary>
    [Test]
    public void TestMergeExisting()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IPart oldPart = daoFactory.ComponentDAO.FindById(2).Part as IPart;
        IPart newPart = daoFactory.ComponentDAO.FindById(4).Part as IPart;
        IPart merged = ModelDAOHelper.DAOFactory.PartDAO
          .Merge (oldPart, newPart,
                  ConflictResolution.Overwrite);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (4, merged.ComponentId);
        Assert.AreEqual ("Test", merged.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the merge function with some conflicts
    /// </summary>
    [Test]
    public void TestMergeConflicts()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IPart oldPart = daoFactory.ComponentDAO.FindById(2).Part as IPart;
        IPart newPart = daoFactory.ComponentDAO.FindById(4).Part as IPart;
        Assert.Throws<ConflictException> (
          new TestDelegate
          (delegate ()
           { ModelDAOHelper.DAOFactory.PartDAO
               .Merge (oldPart, newPart,
                       ConflictResolution.Exception); } ));
        NHibernateHelper.GetCurrentSession ().Flush ();
        transaction.Rollback ();
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IPart oldPart = daoFactory.ComponentDAO.FindById(2).Part as IPart;
        IPart newPart = daoFactory.ComponentDAO.FindById(4).Part as IPart;
        IPart merged = daoFactory.PartDAO
          .Merge (oldPart, newPart,
                  ConflictResolution.Overwrite);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (4, merged.ComponentId);
        Assert.AreEqual ("Test", merged.Name);
        transaction.Rollback ();
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IPart oldPart = daoFactory.ComponentDAO.FindById(2).Part as IPart;
        IPart newPart = daoFactory.ComponentDAO.FindById(4).Part as IPart;
        IPart merged = daoFactory.PartDAO
          .Merge (oldPart, newPart,
                  ConflictResolution.Keep);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (4, merged.ComponentId);
        Assert.AreEqual ("C3A02-2   ", merged.Name);
        transaction.Rollback ();
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
