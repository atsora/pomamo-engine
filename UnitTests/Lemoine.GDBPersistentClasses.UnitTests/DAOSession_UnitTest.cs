// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the classes DAOSession and DAOSessionAdvanced: use of a session that is inherited by some tasks
  /// </summary>
  public class DAOSession_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DAOSession_UnitTest).FullName);

    static readonly string ADVANCED_SESSION_KEY = "Database.Session.Advanced";
    static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds (10);

    /// <summary>
    /// The default session is a DAOSession, a DAOSessionAdvanced if Database.Session.Advanced is true
    /// </summary>
    [Test]
    public void TestSessionType ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        Assert.That (session, Is.InstanceOf<DAOSession> ());
      }
      try {
        Lemoine.Info.ConfigSet.ForceValue (ADVANCED_SESSION_KEY, true);
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          Assert.That (session, Is.InstanceOf<DAOSessionAdvanced> ());
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// A nested DAOSession in the same async context re-uses the session
    /// </summary>
    [Test]
    public void TestNestedSessionReuse ([Values (false, true)] bool advanced)
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (ADVANCED_SESSION_KEY, advanced);
        using (var outerSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var innerSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
            Assert.That (NHibernateHelper.GetSession (innerSession), Is.SameAs (NHibernateHelper.GetSession (outerSession)));
          }
          Assert.That (NHibernateHelper.GetSession (outerSession).IsOpen, Is.True);
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// A sequential task that inherits the session re-uses it
    /// </summary>
    [Test]
    public async System.Threading.Tasks.Task TestSequentialTaskReuse ([Values (false, true)] bool advanced)
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (ADVANCED_SESSION_KEY, advanced);
        using (var outerSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var outerNHibernateSession = NHibernateHelper.GetSession (outerSession);
          var taskSession = await System.Threading.Tasks.Task.Run (() => {
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              return NHibernateHelper.GetSession (session);
            }
          });
          Assert.That (taskSession, Is.SameAs (outerNHibernateSession));
          Assert.That (outerNHibernateSession.IsOpen, Is.True);
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Two tasks that inherit the session and that use it at the same time:
    /// <item>DAOSession: they share the same session</item>
    /// <item>DAOSessionAdvanced: the second one uses an isolated session</item>
    /// </summary>
    [Test]
    public async System.Threading.Tasks.Task TestConcurrentTasks ([Values (false, true)] bool advanced)
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (ADVANCED_SESSION_KEY, advanced);
        using (var outerSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var outerNHibernateSession = NHibernateHelper.GetSession (outerSession);
          var firstOpened = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
          var release = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);

          var task1 = System.Threading.Tasks.Task.Run (async () => {
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              firstOpened.SetResult (true);
              await release.Task;
              return NHibernateHelper.GetSession (session);
            }
          });
          Assert.That (await System.Threading.Tasks.Task.WhenAny (firstOpened.Task, System.Threading.Tasks.Task.Delay (TIMEOUT)), Is.SameAs (firstOpened.Task));

          var task2 = System.Threading.Tasks.Task.Run (() => {
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              var nhibernateSession = NHibernateHelper.GetSession (session);
              Assert.That (nhibernateSession.IsOpen, Is.True);
              return nhibernateSession;
            }
          });
          var session2 = await task2;
          release.SetResult (true);
          var session1 = await task1;

          Assert.That (session1, Is.SameAs (outerNHibernateSession));
          if (advanced) {
            Assert.That (session2, Is.Not.SameAs (outerNHibernateSession));
            Assert.That (session2.IsOpen, Is.False);
          }
          else {
            Assert.That (session2, Is.SameAs (outerNHibernateSession));
          }
          Assert.That (outerNHibernateSession.IsOpen, Is.True);
        }
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// DAOSessionAdvanced: the creator of a session is disposed while a task still uses the session:
    /// the session is closed only once the task does not use it any more
    /// </summary>
    [Test]
    public async System.Threading.Tasks.Task TestAdvancedPostponedClose ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (ADVANCED_SESSION_KEY, true);

        var opened = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
        NHibernate.ISession outerNHibernateSession;
        System.Threading.Tasks.Task<NHibernate.ISession> task;

        using (var outerSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          outerNHibernateSession = NHibernateHelper.GetSession (outerSession);
          task = System.Threading.Tasks.Task.Run (async () => {
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              opened.SetResult (true);
              await release.Task;
              var nhibernateSession = NHibernateHelper.GetSession (session);
              Assert.That (nhibernateSession.IsOpen, Is.True);
              return nhibernateSession;
            }
          });
          Assert.That (await System.Threading.Tasks.Task.WhenAny (opened.Task, System.Threading.Tasks.Task.Delay (TIMEOUT)), Is.SameAs (opened.Task));
        }

        Assert.That (outerNHibernateSession.IsOpen, Is.True, "the session was closed while it was still used by the task");
        Assert.That (ModelDAOHelper.DAOFactory.IsSessionActive (), Is.False);

        release.SetResult (true);
        var taskSession = await task;
        Assert.That (taskSession, Is.SameAs (outerNHibernateSession));
        Assert.That (outerNHibernateSession.IsOpen, Is.False);
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }
  }
}
