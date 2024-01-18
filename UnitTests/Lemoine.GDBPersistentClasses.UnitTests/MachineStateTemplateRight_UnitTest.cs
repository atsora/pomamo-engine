// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class MachineStateTemplateRight
  /// </summary>
  [TestFixture]
  public class MachineStateTemplateRight_UnitTest
  {
    string previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateRight_UnitTest).FullName);

    /// <summary>
    /// Test the retrieval of machine state templates if granted is the default
    /// </summary>
    [Test]
    public void TestDefaultGranted()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        IRole operatorRole = ModelDAOHelper.DAOFactory.RoleDAO
          .FindById ((int) DefaultRole.Operator);
        
        { // Rights in database
          IMachineStateTemplateRight right1 = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateRight (attended, operatorRole, RightAccessPrivilege.Granted);
          ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO.MakePersistent (right1);
          IMachineStateTemplateRight right2 = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateRight (unattended, operatorRole, RightAccessPrivilege.Denied);
          ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO.MakePersistent (right2);
        }
        ModelDAOHelper.DAOFactory.FlushData ();
        
        { // Test the rights
          IList<IMachineStateTemplateRight> rights = ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .FindAll ();
          Assert.That (rights, Has.Count.EqualTo (2));
        }
        
        { // Test them
          IList<IMachineStateTemplate> machineStateTemplates =
            ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .GetGranted (operatorRole);
          Assert.That (machineStateTemplates, Has.Count.EqualTo (9));
          Assert.That (machineStateTemplates.Contains (unattended), Is.False);
        }
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the retrieval of machine state templates if denied is the default
    /// </summary>
    [Test]
    public void TestDefaultDenied()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        IRole operatorRole = ModelDAOHelper.DAOFactory.RoleDAO
          .FindById ((int) DefaultRole.Operator);
        
        { // Rights in database
          IMachineStateTemplateRight right0 = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateRight (null, operatorRole, RightAccessPrivilege.Denied);
          ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO.MakePersistent (right0);
          IMachineStateTemplateRight right1 = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateRight (attended, operatorRole, RightAccessPrivilege.Granted);
          ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO.MakePersistent (right1);
          IMachineStateTemplateRight right2 = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateRight (unattended, operatorRole, RightAccessPrivilege.Denied);
          ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO.MakePersistent (right2);
        }
        ModelDAOHelper.DAOFactory.FlushData ();
        
        { // Test the rights
          IList<IMachineStateTemplateRight> rights = ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .FindAll ();
          Assert.That (rights, Has.Count.EqualTo (3));
        }
        
        { // Test them
          IList<IMachineStateTemplate> machineStateTemplates =
            ModelDAOHelper.DAOFactory.MachineStateTemplateRightDAO
            .GetGranted (operatorRole);
          Assert.That (machineStateTemplates, Has.Count.EqualTo (1));
          Assert.That (machineStateTemplates.Contains (attended), Is.True);
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
