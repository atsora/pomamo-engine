// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.NHibernateTypes;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class <see cref="JsonbType"/>
  /// </summary>
  [TestFixture]
  public class JsonbType_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JsonbType_UnitTest).FullName);

    /// <summary>
    /// The dirty check compares the JSON contents, not the texts
    /// </summary>
    [Test]
    public void TestIsEqual ()
    {
      var type = new JsonbType ();
      Assert.Multiple (() => {
        Assert.That (type.IsEqual ("""{"b":1,"a":2}""", """{"a": 2, "b": 1}"""), Is.True);
        Assert.That (type.IsDirty ("""{"b":1,"a":2}""", """{"a": 2, "b": 1}""", null), Is.False);
        Assert.That (type.IsEqual ("""{"a":1}""", """{"a":2}"""), Is.False);
        Assert.That (type.IsDirty ("""{"a":1}""", """{"a":2}""", null), Is.True);
        Assert.That (type.IsEqual (null, "{}"), Is.False);
        Assert.That (type.IsEqual ("{}", null), Is.False);
        Assert.That (type.IsEqual (null, null), Is.True);
        Assert.That (type.IsEqual ("{invalid", """{"a":1}"""), Is.False);
      });
    }

    /// <summary>
    /// An entity whose jsonb property is set again with the same content in another form
    /// (here the one System.Text.Json writes) is not updated
    /// </summary>
    [Test]
    public void TestNoUpdateWhenSameContent ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var daoFactory = ModelDAOHelper.DAOFactory;
          var machine = daoFactory.MonitoredMachineDAO.FindById (3);
          var inactive = daoFactory.MachineModeDAO.FindById (1);
          var attended = daoFactory.MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
          var reasonUnanswered = daoFactory.ReasonDAO.FindById (4);

          var compactJson = """{"frequencyType":"FixedDuration","seconds":3600,"label":"é"}""";

          IMachineStatus machineStatus = new MachineStatus (machine);
          machineStatus.CncMachineMode = inactive;
          machineStatus.MachineMode = inactive;
          machineStatus.MachineObservationState = attended;
          machineStatus.ManualActivity = false;
          machineStatus.Reason = reasonUnanswered;
          machineStatus.ReasonSlotEnd = new System.DateTime (2026, 10, 21, 0, 0, 0, System.DateTimeKind.Utc);
          machineStatus.JsonData = compactJson;
          daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
          daoFactory.Flush ();
          NHibernateHelper.GetCurrentSession ().Evict (machineStatus);

          var reloaded = daoFactory.MachineStatusDAO.FindById (machine.Id);
          var version = reloaded.Version;
          // Premise: PostgreSQL returns the jsonb in its own normalized form
          Assert.That (reloaded.JsonData, Is.Not.EqualTo (compactJson));

          reloaded.JsonData = compactJson;
          daoFactory.Flush ();
          Assert.That (reloaded.Version, Is.EqualTo (version), "same content: no update");

          reloaded.JsonData = """{"frequencyType":"FixedDuration","seconds":7200,"label":"é"}""";
          daoFactory.Flush ();
          Assert.That (reloaded.Version, Is.EqualTo (version + 1), "different content: update");
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
