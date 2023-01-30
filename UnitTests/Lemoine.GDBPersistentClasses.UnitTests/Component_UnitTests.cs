// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Componnet
  /// </summary>
  [TestFixture]
  public class Component_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Component_UnitTest).FullName);

    /// <summary>
    /// Test the save-update behavior
    /// </summary>
    [Test]
    public void TestSaveUpdate()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          IComponentType componentType = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById (1);
          IProject project = ModelDAOHelper.DAOFactory.ProjectDAO.FindById (1);
          IComponent component = ModelDAOHelper.ModelFactory.CreateComponentFromName (project, "TestSaveUpdate", componentType);
          Assert.AreEqual (0, ((Lemoine.Collections.IDataWithId)component).Id);
          ModelDAOHelper.DAOFactory.Flush ();
          Assert.AreEqual (0, ((Lemoine.Collections.IDataWithId)component).Id);
          ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
          Assert.AreNotEqual (0, ((Lemoine.Collections.IDataWithId)component).Id);
          ModelDAOHelper.DAOFactory.ComponentDAO.MakeTransient (component);
          Assert.IsFalse (NHibernateHelper.GetCurrentSession ().Contains (component));
          ModelDAOHelper.DAOFactory.Flush ();
          Assert.AreNotEqual (0, ((Lemoine.Collections.IDataWithId)component).Id);
          transaction.Rollback ();
        }
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
