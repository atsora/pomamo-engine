// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="ShiftSummary"/>, <see cref="ShiftSummarySection"/>,
  /// <see cref="ShiftSummaryContent"/> and their DAOs
  /// </summary>
  public class ShiftSummary_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (ShiftSummary_UnitTest).FullName);

    static readonly DateTime DAY = new DateTime (2026, 01, 05);
    static readonly DateTime START = new DateTime (2026, 01, 05, 06, 00, 00, DateTimeKind.Utc);

    /// <summary>
    /// A summary is read back by its machine, day and shift, with its sections and contents
    /// </summary>
    [Test]
    public async Task TestMakePersistentAndFindAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO.FindByIdAsync (1);
          var range = new UtcDateTimeRange (START, START.AddHours (8));
          var summary = ModelDAOHelper.ModelFactory.CreateShiftSummary (machine, DAY, null, range);
          await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.MakePersistentAsync (summary);
          var section = ModelDAOHelper.ModelFactory.CreateShiftSummarySection (summary, "Default.Stop");
          section.Items = """[{"kind": "Stop", "text": "Stop 1h"}]""";
          await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.MakePersistentAsync (section);
          var content = ModelDAOHelper.ModelFactory.CreateShiftSummaryContent (summary, "Raw");
          content.Content = """{"sections": []}""";
          content.SourceVersion = "Default.Stop:1";
          await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.MakePersistentAsync (content);
          ModelDAOHelper.DAOFactory.Flush ();

          var found = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.FindAsync (machine, DAY, null);
          var overlaps = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.FindOverlapsRangeAsync (machine, new UtcDateTimeRange (START.AddHours (7), START.AddHours (9)));
          var sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (summary);
          var raw = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindAsync (summary, "Raw");
          var other = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindAsync (summary, "Claude");
          Assert.Multiple (() => {
            Assert.That (found?.Id, Is.EqualTo (summary.Id));
            Assert.That (found.Shift, Is.Null);
            Assert.That (found.DateTimeRange, Is.EqualTo (range));
            Assert.That (overlaps, Has.Count.EqualTo (1));
            Assert.That (sections.Select (x => x.SectionKey), Is.EqualTo (new[] { "Default.Stop" }));
            Assert.That (sections.Single ().UpdateRequired, Is.True, "a new section is to compute");
            Assert.That (sections.Single ().Items, Does.Contain ("Stop 1h"));
            Assert.That (raw?.SourceVersion, Is.EqualTo ("Default.Stop:1"));
            Assert.That (other, Is.Null);
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// A section is flagged by its day and shift, or by a date/time in its summary, only for
    /// its key, and its version is not changed
    /// </summary>
    [Test]
    public async Task TestSetUpdateRequiredAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO.FindByIdAsync (1);
          var range = new UtcDateTimeRange (START, START.AddHours (8));
          var summary = ModelDAOHelper.ModelFactory.CreateShiftSummary (machine, DAY, null, range);
          await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.MakePersistentAsync (summary);
          var stop = ModelDAOHelper.ModelFactory.CreateShiftSummarySection (summary, "Default.Stop");
          stop.UpdateRequired = false;
          stop.DateTime = START;
          await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.MakePersistentAsync (stop);
          var alarm = ModelDAOHelper.ModelFactory.CreateShiftSummarySection (summary, "Default.CncAlarm");
          alarm.UpdateRequired = false;
          alarm.DateTime = START;
          await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.MakePersistentAsync (alarm);
          ModelDAOHelper.DAOFactory.Flush ();
          var stopVersion = stop.Version;
          var lookBack = new UtcDateTimeRange (START.AddDays (-1), START.AddDays (1));

          var beforeFlag = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindToComputeAsync (machine, lookBack);
          ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.SetUpdateRequired (machine.Id, DAY, null, "Default.Stop");
          ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.SetUpdateRequired (machine.Id, DAY, 999, "Default.CncAlarm");
          ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.SetUpdateRequiredAt (machine.Id, START.AddDays (2), "Default.CncAlarm");
          ModelDAOHelper.DAOFactory.Flush ();
          Lemoine.Database.Persistent.NHibernateHelper.GetCurrentSession ().Clear ();
          var afterDayFlag = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindToComputeAsync (machine, lookBack);
          ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.SetUpdateRequiredAt (machine.Id, START.AddHours (2), "Default.CncAlarm");
          ModelDAOHelper.DAOFactory.Flush ();
          Lemoine.Database.Persistent.NHibernateHelper.GetCurrentSession ().Clear ();
          var afterAtFlag = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindToComputeAsync (machine, lookBack);
          var flaggedStop = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindByIdAsync (stop.Id);
          Assert.Multiple (() => {
            Assert.That (beforeFlag, Is.Empty);
            Assert.That (afterDayFlag.Select (x => x.SectionKey), Is.EqualTo (new[] { "Default.Stop" }));
            Assert.That (afterAtFlag.Select (x => x.SectionKey), Is.EquivalentTo (new[] { "Default.Stop", "Default.CncAlarm" }));
            Assert.That (flaggedStop.Version, Is.EqualTo (stopVersion));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
