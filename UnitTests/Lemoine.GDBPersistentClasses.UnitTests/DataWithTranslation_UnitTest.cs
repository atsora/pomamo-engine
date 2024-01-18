// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.I18N;
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
  /// Unit tests for the class DataWithTranslation
  /// </summary>
  [TestFixture]
  public class DataWithTranslation_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (DataWithTranslation_UnitTest).FullName);

    /// <summary>
    /// Test the Display getter of the DataWithTranslation classes
    /// </summary>
    [Test]
    public void TestDisplay()
    {
     
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IWorkOrderStatus workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindById(1);
        Assert.That (workOrderStatus.Display, Is.EqualTo ("Non défini"));
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

      {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                              new TextFileCatalog ("AlertServiceI18N",
                                                                   Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
        //multiCatalog.Add (new DefaultTextFileCatalog ());
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);
      }
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
