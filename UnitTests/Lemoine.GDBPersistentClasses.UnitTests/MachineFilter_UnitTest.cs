// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the persistent class MachineFilter.
  /// </summary>
  [TestFixture]
  public class MachineFilter_UnitTest
  {
    string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilter_UnitTest).FullName);

    /// <summary>
    /// Machine filter with All and remove
    /// </summary>
    [Test]
    public void TestAllRemove ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        ICompany company1 = ModelDAOHelper.DAOFactory.CompanyDAO
          .FindById (1);
        ICompany company2 = ModelDAOHelper.DAOFactory.CompanyDAO
          .FindById (2);
        IDepartment department1 = ModelDAOHelper.DAOFactory.DepartmentDAO
          .FindById (1);
        IDepartment department2 = ModelDAOHelper.DAOFactory.DepartmentDAO
          .FindById (2);
        IMachine machine1 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IMachine machine2 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (2);
        IMachine machine8 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (8);
        
        IMachineFilter machineFilter = ModelDAOHelper.ModelFactory
          .CreateMachineFilter ("test", MachineFilterInitialSet.All);
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (company1,
                                                           MachineFilterRule.Remove));
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (department1,
                                                           MachineFilterRule.Remove));
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (machine2,
                                                           MachineFilterRule.Add));
        ModelDAOHelper.DAOFactory.MachineFilterDAO.MakePersistent (machineFilter);
        
        Assert.IsFalse (machineFilter.IsMatch (machine1));
        Assert.IsTrue (machineFilter.IsMatch (machine2));
        Assert.IsTrue (machineFilter.IsMatch (machine8));
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Machine filter with None and add
    /// </summary>
    [Test]
    public void TestNoneAdd ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        ICompany company1 = ModelDAOHelper.DAOFactory.CompanyDAO
          .FindById (1);
        ICompany company2 = ModelDAOHelper.DAOFactory.CompanyDAO
          .FindById (2);
        IDepartment department1 = ModelDAOHelper.DAOFactory.DepartmentDAO
          .FindById (1);
        IDepartment department2 = ModelDAOHelper.DAOFactory.DepartmentDAO
          .FindById (2);
        IMachine machine1 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (1);
        IMachine machine2 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (2);
        IMachine machine8 = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (8);
        
        IMachineFilter machineFilter = ModelDAOHelper.ModelFactory
          .CreateMachineFilter ("test", MachineFilterInitialSet.None);
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (company1,
                                                           MachineFilterRule.Add));
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (department1,
                                                           MachineFilterRule.Add));
        machineFilter.Items.Add (ModelDAOHelper.ModelFactory
                                 .CreateMachineFilterItem (machine2,
                                                           MachineFilterRule.Remove));
        ModelDAOHelper.DAOFactory.MachineFilterDAO.MakePersistent (machineFilter);
        
        Assert.IsTrue (machineFilter.IsMatch (machine1));
        Assert.IsFalse (machineFilter.IsMatch (machine2));
        Assert.IsFalse (machineFilter.IsMatch (machine8));
        
        transaction.Rollback ();
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
