// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NUnit.Framework;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class WorkOrderProject
  /// </summary>
  [TestFixture]
  public class WorkOrderProject_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderProject_UnitTest).FullName);

    [Test]
    public void TestQuery ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        WorkOrderProject workOrderProject =
          session.CreateQuery ("from WorkOrderProject WorkOrderProject " +
                               "where WorkOrderProject.WorkOrder.Name=:WorkOrderName")
          .SetParameter ("WorkOrderName", "JOB1")
          .UniqueResult<WorkOrderProject> ();
        Assert.NotNull (workOrderProject);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)workOrderProject.WorkOrder).Id);

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
