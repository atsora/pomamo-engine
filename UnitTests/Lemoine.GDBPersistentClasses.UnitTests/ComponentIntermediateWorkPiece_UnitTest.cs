// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using NHibernate;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ComponentIntermediateWorkPiece
  /// </summary>
  [TestFixture]
  public class ComponentIntermediateWorkPiece_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ComponentIntermediateWorkPiece_UnitTest).FullName);

    /// <summary>
    /// Test a CreateQuery
    /// </summary>
    [Test]
    public void TestQuery ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        ComponentIntermediateWorkPiece ciwp =
          session.CreateQuery (@"from ComponentIntermediateWorkPiece foo
where foo.Component.Id=:Component_Id
  and foo.Component.Name=:Component_Name
  and foo.Component.Type.Id=:Component_Type_Id
  and foo.Component.Type.TranslationKey=:Component_Type_TranslationKey
  and foo.Component.Project.Id=:Component_Project_Id
  and foo.Component.Project.Name=:Component_Project_Name
  and foo.IntermediateWorkPiece.Id=:Component_IntermediateWorkPiece_Id
  and foo.IntermediateWorkPiece.Name=:Component_IntermediateWorkPiece_Name")
          .SetParameter ("Component_Id", 18514)
          .SetParameter ("Component_Name", "PART1")
          .SetParameter ("Component_Type_Id", 1)
          .SetParameter ("Component_Type_TranslationKey", "UndefinedValue")
          .SetParameter ("Component_Project_Id", 128)
          .SetParameter ("Component_Project_Name", "PART1")
          .SetParameter ("Component_IntermediateWorkPiece_Id", 11008)
          .SetParameter ("Component_IntermediateWorkPiece_Name", "SIMPLEOPERATION1")
          .UniqueResult<ComponentIntermediateWorkPiece> ();
        Assert.IsNull (ciwp);

        // Note there is a bug with Criteria and composite keys
        // http://www.codewrecks.com/blog/index.php/2009/04/29/nhibernate-icriteria-and-composite-id-with-key-many-to-one/
        // This prevents from using this for example:
        // WorkOrderProject workOrderProject =
        //   session.CreateCriteria<WorkOrderProject> ()
        //   .CreateAlias ("WorkOrder", "WorkOrder")
        //   .Add (Restrictions.Eq ("WorkOrder.Name", "JOB1"))
        //   .CreateAlias ("Project", "Project", JoinType.InnerJoin)
        //   .Add (Restrictions.Eq ("Project.Name", "JOB1"))
        //   .UniqueResult<WorkOrderProject> ();
      }
    }

    /// <summary>
    /// Test FindPersistentClass
    /// </summary>
    [Test]
    public void TestFindPersistentClass1 ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        Component component = session.Get<Component> (18514);
        IntermediateWorkPiece intermediateWorkPiece =
          session.Get<IntermediateWorkPiece> (11008);
        ComponentIntermediateWorkPiece ciwp =
          new ComponentIntermediateWorkPiece (component,
                                              intermediateWorkPiece);
        BaseData baseData = ciwp as BaseData;
        Assert.IsNotNull (baseData);
        ciwp = (ComponentIntermediateWorkPiece) baseData.FindPersistentClass (session);
        Assert.IsNull (ciwp);
      }
    }
    
    /// <summary>
    /// Test the FindPersistentClass method after an insertion
    /// </summary>
    [Test]
    public void TestFindPersistentClass2()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        IProject project = ModelDAOHelper.ModelFactory
          .CreateProjectFromCode ("ProjectCode");
        ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
        IComponentType componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);
        IComponent component = ModelDAOHelper.ModelFactory
          .CreateComponentFromCode (project, "CompCode", componentType);
        ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
        IOperationType operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
        IOperation operation = ModelDAOHelper.ModelFactory
          .CreateOperation (operationType);
        operation.Code = "IwpCode";
        ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
        IIntermediateWorkPiece iwp = ModelDAOHelper.ModelFactory
          .CreateIntermediateWorkPiece (operation);
        iwp.Code = "IwpCode";
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
          .MakePersistent (iwp);
        IComponentIntermediateWorkPiece compIwp = component.AddIntermediateWorkPiece (iwp); 
        ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (compIwp);
        session.Flush ();
        
        IIntermediateWorkPiece iwp2 = ModelDAOHelper.ModelFactory
          .CreateIntermediateWorkPiece (null);
        iwp2.Code = "IwpCode";
        IComponentIntermediateWorkPiece compIwp2 = component
          .AddIntermediateWorkPiece (iwp2);
        IComponentIntermediateWorkPiece iwp3 = (IComponentIntermediateWorkPiece) ((ComponentIntermediateWorkPiece)compIwp2)
          .FindPersistentClass (new string[] {"Component", "IntermediateWorkPiece.Code"});
        Assert.NotNull (iwp3);
        Assert.AreEqual (iwp, iwp3.IntermediateWorkPiece);
        
        transaction.Rollback ();
      }
    }
        
    /// <summary>
    /// Test the FindPersistentClass method after an insertion
    /// </summary>
    [Test]
    public void TestFindPersistentClass3()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        IProject project = ModelDAOHelper.ModelFactory
          .CreateProjectFromCode ("ProjectCode");
        ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
        IComponentType componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);
        IComponent component = ModelDAOHelper.ModelFactory
          .CreateComponentFromCode (project, "CompCode", componentType);
        ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
        IOperationType operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
        IOperation operation = ModelDAOHelper.ModelFactory
          .CreateOperation (operationType);
        operation.Code = "IwpCode";
        ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
        IIntermediateWorkPiece iwp = ModelDAOHelper.ModelFactory
          .CreateIntermediateWorkPiece (operation);
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
          .MakePersistent (iwp);
        IComponentIntermediateWorkPiece compIwp = component
          .AddIntermediateWorkPiece (iwp);
        ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (compIwp);
        session.Flush ();
        
        IOperation operation2 = ModelDAOHelper.ModelFactory
          .CreateOperation (operationType);
        operation2.Code = "IwpCode";
        IIntermediateWorkPiece iwp2 = ModelDAOHelper.ModelFactory
          .CreateIntermediateWorkPiece (operation2);
        IComponentIntermediateWorkPiece compIwp2 = component
          .AddIntermediateWorkPiece (iwp2);
        IComponentIntermediateWorkPiece iwp3 = (IComponentIntermediateWorkPiece) ((ComponentIntermediateWorkPiece)compIwp2)
          .FindPersistentClass (new string[] {"Component", "IntermediateWorkPiece.Operation.Code"});
        Assert.NotNull (iwp3);
        Assert.AreEqual (iwp, iwp3.IntermediateWorkPiece);
        
        transaction.Rollback ();
      }
    }
        
    /// <summary>
    /// Test transient objects are not updated
    /// </summary>
    [Test]
    public void TestTransientObjectsNotUpdated ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentIntermediateWorkPiece xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Order=""50"">
  <Component Id=""18514"" />
  <IntermediateWorkPiece Id=""11008"" />
</ComponentIntermediateWorkPiece>");
        XmlSerializer deserializer = new XmlSerializer (typeof (ComponentIntermediateWorkPiece));
        ComponentIntermediateWorkPiece ciwp =
          (ComponentIntermediateWorkPiece) deserializer.Deserialize (textReader);
        ciwp.Component = session.Get<Component> (18514);
        ciwp.IntermediateWorkPiece = session.Get<IntermediateWorkPiece> (11008);
        session.Save (ciwp);
        session.Flush ();
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the transient ComponentIntermediateWorkPiece is
    /// not saved in case it is overwritten by another one
    /// </summary>
    [Test]
    public void TestTransientComponentIntermediateWorkPiece ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentIntermediateWorkPiece xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Order=""50"">
  <Component Id=""1"" />
  <IntermediateWorkPiece Id=""2"" />
</ComponentIntermediateWorkPiece>");
        XmlSerializer deserializer = new XmlSerializer (typeof (ComponentIntermediateWorkPiece));
        ComponentIntermediateWorkPiece ciwp =
          (ComponentIntermediateWorkPiece) deserializer.Deserialize (textReader);
        ciwp.Component = session.Get<Component> (1);
        ciwp.IntermediateWorkPiece = session.Get<IntermediateWorkPiece> (2);
        ciwp = session.Get<ComponentIntermediateWorkPiece> (2);
        session.Flush ();
        transaction.Rollback ();
      }

      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        ComponentIntermediateWorkPiece ciwp2 = session.Get<ComponentIntermediateWorkPiece> (2);
        session.Flush ();
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
