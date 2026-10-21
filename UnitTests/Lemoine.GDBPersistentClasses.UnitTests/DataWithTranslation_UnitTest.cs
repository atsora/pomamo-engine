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
        Assert.That (workOrderStatus.Display, Is.AnyOf ("Non défini", "Undefined"));
      }
    }

    /// <summary>
    /// The Display of a role is read from the display column: its name, else the translation
    /// of its translation key
    /// </summary>
    [Test]
    public void TestRoleDisplay ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var roles = ModelDAOHelper.DAOFactory.RoleDAO.FindAll ();
        Assert.That (roles, Is.Not.Empty);
        Assert.Multiple (() => {
          foreach (var role in roles) {
            Assert.That (role.Display, Is.Not.Null.And.Not.Empty, $"role {role.Id}");
            if (!string.IsNullOrEmpty (role.Name)) {
              Assert.That (role.Display, Is.EqualTo (role.Name), $"role {role.Id}");
            }
          }
        });
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
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
