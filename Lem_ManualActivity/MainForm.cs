// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.ModelDAO.Interfaces;

namespace Lem_ManualActivity
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Event reactions
    void Button1Click (object sender, EventArgs e)
    {
      // Get selected machines
      IList<IDisplayable> elts = machineTreeView.SelectedElements;
      IList<IMachine> machines = new List<IMachine> ();
      foreach (var elt in elts) {
        if (elt is IMachine) {
          machines.Add (elt as IMachine);
        }
      }

      // Get selected machine modes
      IList<IMachineMode> machineModes = machineModeSelection1.SelectedMachineModes;

      // Check that at least one machine is selected
      if (machines.Count == 0) {
        MessageBox.Show ("Select at least one machine first");
        return;
      }

      // Check that at least one machine mode is selected
      if (machineModes.Count == 0) {
        MessageBox.Show ("Select at least one machine mode first");
        return;
      }
      // if no end date option is activated
      if (!checkBox_noEndDate.Checked) {
        // check that start date is less than end date
        if (DateTime.Compare (this.dateTimePicker_Start.Value, this.dateTimePicker_End.Value) > 0) {
          MessageBox.Show ("Start date (From) should be less than end date (To)");
          return;
        }
      }

      IMonitoredMachine monitoredMachine;
      using (var daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machines[0].Id);
      }

      DateTime begin = dateTimePicker_Start.Value;
      UpperBound<DateTime> end = (checkBox_noEndDate.Checked)
        ? new UpperBound<DateTime> (null)
        : new UpperBound<DateTime> (dateTimePicker_End.Value);
      begin = DateTime.SpecifyKind (begin, DateTimeKind.Local);
      begin.Subtract (TimeSpan.FromMilliseconds (begin.Millisecond));
      if (end.HasValue) {
        end = DateTime.SpecifyKind (end.Value, DateTimeKind.Local);
        end = end.Value.Subtract (TimeSpan.FromMilliseconds (end.Value.Millisecond));
        if (end <= begin) {
          MessageBox.Show ("Invalid date/time selection, end before start", "Lem_ManualActivity", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
      }

      ManualActivityProgressForm manualActivityProgressForm = new ManualActivityProgressForm ();
      manualActivityProgressForm.StartPosition = FormStartPosition.CenterParent;
      manualActivityProgressForm.MonitoredMachine = monitoredMachine;
      manualActivityProgressForm.MachineMode = machineModes[0];
      manualActivityProgressForm.BeginTime = begin;
      manualActivityProgressForm.IsEndDate = !checkBox_noEndDate.Checked;
      manualActivityProgressForm.EndTime = end;

      manualActivityProgressForm.ShowDialog ();
    }

    void CheckBox_noEndDateCheckedChanged (object sender, EventArgs e)
    {
      this.dateTimePicker_End.Enabled = !this.checkBox_noEndDate.Checked;
    }

    void MainFormLoad (object sender, EventArgs e)
    {
      dateTimePicker_Start.Value = DateTime.Now;
      dateTimePicker_End.Value = DateTime.Now;

      // Machine list
      machineTreeView.ClearOrders ();
      machineTreeView.AddOrder ("Sort by department", new string[] { "Company", "Department" });
      machineTreeView.AddOrder ("Sort by category", new string[] { "Company", "Category" });
      machineTreeView.ClearElements ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll ();
          // Insert all machines
          foreach (var machine in machines) {
            machineTreeView.AddElement (machine);
          }
        }
      }
      machineTreeView.RefreshTreeview ();
      machineTreeView.SelectedOrder = 0;
    }
    #endregion // Event reactions

    private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }
  }
}
