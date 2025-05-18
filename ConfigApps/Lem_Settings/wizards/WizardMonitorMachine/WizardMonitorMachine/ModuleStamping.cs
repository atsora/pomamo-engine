// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Linq;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of ModuleStamping.
  /// </summary>
  public partial class ModuleStamping : UserControl
  {
    #region Members
    readonly IList<ModuleStampingGrid> m_stampingGrids = new List<ModuleStampingGrid> ();
    bool m_readOnlyState = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ModuleStamping).FullName);

    #region Getters / Setters
    /// <summary>
    /// Readonly state of the controls
    /// </summary>
    [DefaultValue (false)]
    [Description ("Readonly state of the controls.")]
    public bool ReadOnly
    {
      get { return m_readOnlyState; }
      set {
        m_readOnlyState = value;
        foreach (var grid in m_stampingGrids) {
          grid.ReadOnly = m_readOnlyState;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ModuleStamping ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the machine modules
    /// </summary>
    /// <param name="monitoredMachine">current monitored machine</param>
    public void Initialize (IMonitoredMachine monitoredMachine)
    {
      ClearTabs ();
      foreach (var mamo in monitoredMachine.MachineModules.OrderBy (x => x.ConfigPrefix)) {
        if (mamo.CncAcquisition != null) {
          AddTab (string.IsNullOrEmpty (mamo.ConfigPrefix) ? "Main" :
                 Regex.Replace (mamo.ConfigPrefix, "-", string.Empty),
                 mamo.StartCycleVariable,
                 mamo.CycleVariable,
                 mamo.SequenceVariable,
                 mamo.MilestoneVariable,
                 mamo.DetectionMethodVariable,
                 "");
        }
      }
      if (tabControl.TabPages.Count > 1) {
        tabControl.SelectedIndex = 0;
      }
    }

    /// <summary>
    /// Initialize the machine modules
    /// </summary>
    /// <param name="cncDoc">current configuration</param>
    public void Initialize (CncDocument cncDoc)
    {
      ClearTabs ();
      foreach (var module in cncDoc.Modules) {
        AddTab (string.IsNullOrEmpty (module.m_identifier) ? "Main" : module.m_identifier,
               module.m_startCycleVariable ? module.m_startCycleVariableValue ?? "" : null,
               module.m_cycleVariable ? module.m_cycleVariableValue ?? "" : null,
               module.m_sequenceVariable ? module.m_sequenceVariableValue ?? "" : null,
               module.m_milestoneVariable ? module.m_milestoneVariableValue ?? "" : null,
               module.m_detectionMethodVariable ? module.m_detectionMethodVariableValue ?? "" : null,
               module.m_description);
      }
      if (tabControl.TabPages.Count > 1) {
        tabControl.SelectedIndex = 0;
      }
    }

    /// <summary>
    /// Add an old configuration
    /// </summary>
    /// <param name="monitoredMachine">current monitored machine</param>
    /// <returns>true if success</returns>
    public bool AddOld (IMonitoredMachine monitoredMachine)
    {
      foreach (var mamo in monitoredMachine.MachineModules) {
        if (mamo.CncAcquisition != null) {
          string prefix = string.IsNullOrEmpty (mamo.ConfigPrefix) ? "" :
            Regex.Replace (mamo.ConfigPrefix, "-", string.Empty);

          bool ok = false;
          foreach (Control tabPage in tabControl.TabPages) {
            if (string.Compare (tabPage.Tag as string, prefix, StringComparison.InvariantCulture) == 0) {
              ok = true;
              break;
            }
          }

          if (!ok) {
            return false;
          }
        }
      }

      // Fill the existing tab with previous values
      foreach (var mamo in monitoredMachine.MachineModules) {
        if (mamo.CncAcquisition != null) {
          string prefix = string.IsNullOrEmpty (mamo.ConfigPrefix) ? "" :
            Regex.Replace (mamo.ConfigPrefix, "-", string.Empty);
          foreach (Control tabPage in tabControl.TabPages) {
            if (string.Compare (tabPage.Tag as string, prefix, StringComparison.InvariantCulture) == 0) {
              var grid = tabPage.Controls[0] as ModuleStampingGrid;
              grid.OldCycleStart = mamo.StartCycleVariable;
              grid.OldCycleEnd = mamo.CycleVariable;
              grid.OldSequence = mamo.SequenceVariable;
              grid.OldDetection = mamo.DetectionMethodVariable;
            }
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Save all values in a cnc document
    /// </summary>
    public void SaveValues (CncDocument cncDoc)
    {
      foreach (var grid in m_stampingGrids) {
        string identifier = grid.Tag as string;
        foreach (var module in cncDoc.Modules) {
          if (module.m_identifier == identifier) {
            module.m_startCycleVariableValue = grid.CycleStart;
            module.m_cycleVariableValue = grid.CycleEnd;
            module.m_sequenceVariableValue = grid.Sequence;
            module.m_milestoneVariableValue = grid.Milestone;
            module.m_detectionMethodVariableValue = grid.Detection;
            break;
          }
        }
      }
    }

    void ClearTabs ()
    {
      while (m_stampingGrids.Count > 0) {
        m_stampingGrids[0].Dispose ();
        m_stampingGrids.RemoveAt (0);
      }
      tabControl.TabPages.Clear ();
    }

    void AddTab (string name, string cycleStart, string cycleEnd, string sequence, string milestone,
                string detection, string description)
    {
      // Prepare the grid
      var grid = new ModuleStampingGrid (description);
      grid.ReadOnly = m_readOnlyState;
      grid.CycleStart = cycleStart;
      grid.CycleEnd = cycleEnd;
      grid.Sequence = sequence;
      grid.Milestone = milestone;
      grid.Detection = detection;
      grid.Dock = DockStyle.Fill;
      m_stampingGrids.Add (grid);

      // Prepare the tab
      var tabPage = new TabPage (name);
      tabPage.Tag = (name == "Main") ? "" : name;
      grid.Tag = tabPage.Tag;
      tabPage.Controls.Add (grid);

      tabControl.TabPages.Add (tabPage);
    }
    #endregion // Methods
  }
}
