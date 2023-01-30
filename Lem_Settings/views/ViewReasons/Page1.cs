// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;
using System.Linq;

namespace ViewReasons
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericViewPage, IViewPage
  {
    #region Members
    bool m_loading = false;
    ReasonManager m_reasonManager = new ReasonManager();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Reasons"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select the machine mode in the left part of the page. " +
          "Then, in the right part you will see the corresponding default (in bold) and selectable reasons " +
          "for all planned states of the machine.\n\n" +
          "Reasons can be inherited from a higher machine mode level. In that case, they " +
          "are displayed in gray in the right part. Each override is displayed in blue in the tree.\n\n" +
          "Configuration problems that are detected are displayed in red in the right part. Typically, this " +
          "can be a missing default reason or missing selectable reasons."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      InitializeComponent();
      
      // Color of a line
      machineModeSelection.AddDetermineColorFunction(
        (obj) => {
          var machineMode = obj as IMachineMode;
          if (machineMode == null) {
            return Color.Gray;
          }

          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
              // Default reason?
              var mmdrs = ModelDAOHelper.DAOFactory
                .MachineModeDefaultReasonDAO.FindByMachineMode(machineMode);
              if (mmdrs.Count > 0) {
                return Color.Blue;
              }

              // Selectable reason?
              var rss = ModelDAOHelper.DAOFactory
                .ReasonSelectionDAO.FindByMachineMode(machineMode);
              foreach (var rs in rss) {
                if (rs.Selectable) {
                  return Color.Blue;
                }
              }
            }
          }
          
          return Color.Black;
        });
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Select the machine modes
      m_loading = true;
      machineModeSelection.SelectedMachineMode = data.Get<IMachineMode>(Item.MACHINE_MODE);
      m_loading = false;
      
      SelectionChanged();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Selected machine modes
      data.Store(Item.MACHINE_MODE, machineModeSelection.SelectedMachineMode);
    }
    #endregion // Page methods
    
    #region Private methods
    void SelectionChanged()
    {
      if (m_loading) {
        return;
      }

      using (new SuspendDrawing(verticalReasons)) {
        // Clear reasons
        verticalReasons.Clear();
        
        // Get all reasons for a machine mode
        var machineMode = machineModeSelection.SelectedMachineMode;
        if (machineMode == null) {
          return;
        }

        m_reasonManager.LoadReasons(machineMode);
        
        // Display all reasons
        int row = 0;
        foreach (var rbm in m_reasonManager.GetReasons()) {
          // Label for displaying the name of one or several MOS
          string mosText = "";
          foreach (var mos in rbm.MOSS) {
            if (mosText != "") {
              mosText += ", ";
            }

            mosText += mos.NameOrTranslation;
          }
          var label = new Label();
          label.Dock = DockStyle.Fill;
          label.TextAlign = ContentAlignment.MiddleLeft;
          label.ForeColor = Color.White;
          label.BackColor = LemSettingsGlobal.COLOR_CATEGORY;
          label.Font = new Font(label.Font, FontStyle.Bold);
          label.Text = mosText;
          label.Padding = new Padding(0);
          label.Margin = new Padding(0);
          label.Height = 45;
          verticalReasons.AddControl(label);
          
          // Warning?
          string warning = rbm.GetWarning();
          if (!string.IsNullOrEmpty(warning)) {
            var labelWarning = new Label();
            labelWarning.ForeColor = Color.Red;
            labelWarning.Font = new Font(labelWarning.Font, FontStyle.Bold);
            labelWarning.TextAlign = ContentAlignment.MiddleCenter;
            labelWarning.Padding = new Padding(0);
            labelWarning.Margin = new Padding(0);
            labelWarning.Dock = DockStyle.Fill;
            labelWarning.Text = warning;
            verticalReasons.AddControl(labelWarning);
          }
          
          // List of reasons
          foreach (var reason in rbm.Reasons) {
            var cell = new ReasonCell(reason);
            cell.Dock = DockStyle.Fill;
            verticalReasons.AddControl(cell);
          }
          
          row++;
        }
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void MachineModeSelectionAfterSelect(object sender, EventArgs e)
    {
      SelectionChanged();
    }
    #endregion // Event reactions
  }
}
