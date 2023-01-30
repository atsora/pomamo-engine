// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.GDBMigration;
using Lemoine.Core.Log;
using Migrator;
using Migrator.Framework;
using Lemoine.Info;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lem_GDBMigrator
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    #region Members
    ConnectionParameters m_connection = new ConnectionParameters ();
    MigrationHelper m_migrationHelper = null;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm ()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
      LogManager.AddLog4net ();

      InitializeComponent ();

      // Data bindings: connectionStringTextBox automatically updated
      serverTextBox.DataBindings.Add ("Text", m_connection, "Server");
      portTextBox.DataBindings.Add ("Text", m_connection, "Port");
      databaseTextBox.DataBindings.Add ("Text", m_connection, "Database");
      userTextBox.DataBindings.Add ("Text", m_connection, "Username");
      passwordTextBox.DataBindings.Add ("Text", m_connection, "Password");
      connectionStringTextBox.DataBindings.Add ("Text", m_connection, "ConnectionString");

      m_connection.LoadGDBParameters ();

      UpdateConnection ();

      // Max version
      IList<long> appliedMigrations = m_migrationHelper.Migrator.AppliedMigrations;
      long maxVersion = -1;
      foreach (Type m in m_migrationHelper.Migrator.MigrationsTypes) {
        long version = MigrationLoader.GetMigrationVersion (m);
        if (version > maxVersion) {
          maxVersion = version;
        }
      }

      versionNumericUpDown.Maximum = maxVersion;
      versionNumericUpDown.Value = maxVersion;
    }
    #endregion

    #region Methods
    /// <summary>
    /// List the migrations
    /// </summary>
    public void ListMigrations ()
    {
      log.Debug ("ListMigrations /B");
      IList<long> appliedMigrations =
        m_migrationHelper.Migrator.AppliedMigrations;
      var lines = new List<String> ();
      foreach (Type m in m_migrationHelper.Migrator.MigrationsTypes) {
        long version = MigrationLoader.GetMigrationVersion (m);
        bool applied = appliedMigrations.Contains (version);
        string migrationsText =
          String.Format ("{0} {1} {2}\n",
                         applied ? "=>" : "  ",
                         version.ToString ().PadLeft (3),
                         StringUtils.ToHumanName (m.Name));
        lines.Add (migrationsText);
      }
      lines.Reverse ();
      migrationsTextBox.Lines = lines.ToArray ();
    }

    private void UpdateConnection ()
    {
      try {
        m_migrationHelper = new MigrationHelper (m_connection.ConnectionString, false);
        ListMigrations ();
      }
      catch (Exception ex) {
        log.Error ("UpdateConnection: exception", ex);
        migrationsTextBox.Clear ();
        MessageBox.Show ("Cannot connect to database with these parameters.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
    #endregion

    #region Event reactions
    /// <summary>
    /// Upgrade button was clicked => upgrade the database
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void UpgradeButtonClick (object sender, EventArgs e)
    {
      try {
        m_migrationHelper.Migrate ();

      }
      catch (Exception ex) {
        log.ErrorFormat ("UpgradeButtonClick: " +
                         "error {0} in migration",
                         ex);
        MessageBox.Show (String.Format ("Migration failed with error {0}",
                                        ex),
                         "Migration error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);

        MessageBox.Show (String.Format ("Migration failed with error {0}",
                                        ex),
                         "Migration error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);

      }
      ListMigrations ();
    }

    /// <summary>
    /// Migrate to a given version
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MigrateButtonClick (object sender, EventArgs e)
    {
      try {
        m_migrationHelper.Migrate ((int)versionNumericUpDown.Value);
      }
      catch (Exception ex) {
        log.ErrorFormat ("MigrateButtonClick: " +
                         "error {0} in migration",
                         ex);
        MessageBox.Show (String.Format ("Migration failed with error {0}",
                                        ex),
                         "Migration error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
      }
      ListMigrations ();
    }

    void ButtonRefreshClick (object sender, System.EventArgs e)
    {
      UpdateConnection ();
    }
    #endregion // Event reactions
  }
}
