// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="ConnectionParameters"/>
  /// </summary>
  public class ConnectionParameters_UnitTest
  {
    static readonly string UNIT_TEST_KEY = $"DbConnection.{Constants.DEFAULT_DSN_UNIT_TEST_NAME}";

    /// <summary>
    /// A development branch of the next major release is a dev instance by default
    /// </summary>
    [Test]
    public void TestDevDefault ()
    {
#if NEXT_MAJOR
      Assert.That (InstanceInfo.DEV_DEFAULT, Is.True);
#else
      Assert.That (InstanceInfo.DEV_DEFAULT, Is.False);
#endif
    }

    /// <summary>
    /// Normal instance: the unit test connection is read in DbConnection.UnitTests
    /// </summary>
    [Test]
    public void TestUnitTestConnection ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, false);
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY, "testuser:testpassword@testhost:5433/otherunittests");
        var connectionParameters = new ConnectionParameters (Constants.DEFAULT_DSN_UNIT_TEST_NAME);
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        Assert.Multiple (() => {
          Assert.That (connectionParameters.DsnName, Is.EqualTo (Constants.DEFAULT_DSN_UNIT_TEST_NAME));
          Assert.That (connectionParameters.Server, Is.EqualTo ("testhost"));
          Assert.That (connectionParameters.Port, Is.EqualTo ("5433"));
          Assert.That (connectionParameters.Username, Is.EqualTo ("testuser"));
          Assert.That (connectionParameters.Database, Is.EqualTo ("otherunittests"));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Dev instance: the unit test connection is read in DbConnection.UnitTests-dev,
    /// and the DSN name is unchanged
    /// </summary>
    [Test]
    public void TestDevUnitTestConnection ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, true);
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY, "testuser:testpassword@testhost:5433/releaseunittests");
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY + InstanceInfo.DEV_SUFFIX, "testuser:testpassword@testhost:5433/devunittests");
        var connectionParameters = new ConnectionParameters (Constants.DEFAULT_DSN_UNIT_TEST_NAME);
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        Assert.Multiple (() => {
          Assert.That (connectionParameters.DsnName, Is.EqualTo (Constants.DEFAULT_DSN_UNIT_TEST_NAME));
          Assert.That (connectionParameters.Database, Is.EqualTo ("devunittests"));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Dev instance: the unit test connection without suffix, the one of the normal instance, is never used
    /// </summary>
    [Test]
    public void TestDevIgnoresReleaseConnection ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, true);
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY, "testuser:testpassword@testhost:5433/releaseunittests");
        var connectionParameters = new ConnectionParameters (Constants.DEFAULT_DSN_UNIT_TEST_NAME);
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        // Either DbConnection.UnitTests-dev if it is configured on this computer, or the default one on localhost
        Assert.Multiple (() => {
          Assert.That (connectionParameters.Database, Is.Not.EqualTo ("releaseunittests"));
          Assert.That (connectionParameters.Server, Is.Not.EqualTo ("testhost"));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// A unit test connection without any database targets the unit test database,
    /// suffixed by -dev in a dev instance, never the default database
    /// </summary>
    [Test]
    public void TestUnitTestConnectionWithoutDatabase ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, false);
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY, "testuser:testpassword@testhost:5433");
        var connectionParameters = new ConnectionParameters (Constants.DEFAULT_DSN_UNIT_TEST_NAME);
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        Assert.That (connectionParameters.Database, Is.EqualTo (Constants.DEFAULT_DATABASE_UNIT_TEST_NAME));

        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, true);
        Lemoine.Info.ConfigSet.ForceValue (UNIT_TEST_KEY + InstanceInfo.DEV_SUFFIX, "testuser:testpassword@testhost:5433");
        connectionParameters = new ConnectionParameters (Constants.DEFAULT_DSN_UNIT_TEST_NAME);
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        Assert.That (connectionParameters.Database, Is.EqualTo (Constants.DEFAULT_DATABASE_UNIT_TEST_NAME + InstanceInfo.DEV_SUFFIX));
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Another DSN without any database keeps targeting the default database
    /// </summary>
    [Test]
    public void TestOtherConnectionWithoutDatabase ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, false);
        Lemoine.Info.ConfigSet.ForceValue ("DbConnection.TestOtherDsn", "testuser:testpassword@testhost");
        var connectionParameters = new ConnectionParameters ("TestOtherDsn");
        Assert.That (connectionParameters.LoadParameters (), Is.True);
        Assert.That (connectionParameters.Database, Is.EqualTo (Constants.DEFAULT_DATABASE_NAME));
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }
  }
}
