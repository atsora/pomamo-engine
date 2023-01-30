// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Lemoine.Core.Log;
using Lemoine.GDBUtils;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Pulse.Database;
using Pulse.Database.ConnectionInitializer;

namespace Lem_DatabaseUpgrade
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    private static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Constructors
    /// <summary>
    /// Main form of the Lemoine Service Monitoring application
    /// </summary>
    public MainForm ()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new OsConfigReader ());
      LogManager.AddLog4net ();

      log.Debug ("Lem_DatabaseUpgrade /B");

      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    private void MainForm_Shown (object sender, EventArgs e)
    {
      var textWriter = new RichTextBoxTextWriter (richTextBox1);
      var messageLoggerFactory =
        new MessageLoggerFactory (textWriter, Level.Info);
      var upgradeLogger = messageLoggerFactory.GetLogger ("");
      var connectionInitializer = new ConnectionInitializerDatabaseUpgrade ();
      var defaultValuesFactory = new DefaultValuesFactory ();
      var databaseUpgrader = DatabaseUpgrader.Create<Lemoine.GDBPersistentClasses.Machine> (connectionInitializer, defaultValuesFactory);
      databaseUpgrader.Upgrade ("Lem_DatabaseUpgrade", upgradeLogger);
    }
  }
}
