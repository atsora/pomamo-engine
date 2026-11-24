// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class NonConformanceReason
  /// </summary>
  [TestFixture]
  public class NonConformanceReason_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NonConformanceReason_UnitTest).FullName);

    /// <summary>
    /// The color, the display priority and the category of a non conformance reason are saved and read back,
    /// with their default values
    /// </summary>
    [Test]
    public void TestColorPriorityCategory ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var defaultReason = ModelDAOHelper.ModelFactory.CreateNonConformanceReason ("TestDefaultReason");
        ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.MakePersistent (defaultReason);

        var fixableReason = ModelDAOHelper.ModelFactory.CreateNonConformanceReason ("TestFixableReason");
        fixableReason.Color = "#00FF00";
        fixableReason.DisplayPriority = 2.5;
        fixableReason.Category = NonConformanceReasonCategory.Fixable;
        ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.MakePersistent (fixableReason);

        NHibernateHelper.GetCurrentSession ().Flush ();
        NHibernateHelper.GetCurrentSession ().Clear ();

        var defaultReloaded = ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.FindById (defaultReason.Id);
        var fixableReloaded = ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.FindById (fixableReason.Id);
        Assert.Multiple (() => {
          Assert.That (defaultReloaded.Color, Is.EqualTo ("#FF8000"));
          Assert.That (defaultReloaded.DisplayPriority, Is.Null);
          Assert.That (defaultReloaded.Category, Is.EqualTo (NonConformanceReasonCategory.Scrap));
          Assert.That (fixableReloaded.Color, Is.EqualTo ("#00FF00"));
          Assert.That (fixableReloaded.DisplayPriority, Is.EqualTo (2.5));
          Assert.That (fixableReloaded.Category, Is.EqualTo (NonConformanceReasonCategory.Fixable));
        });

        transaction.Rollback ();
      }
    }
  }
}
