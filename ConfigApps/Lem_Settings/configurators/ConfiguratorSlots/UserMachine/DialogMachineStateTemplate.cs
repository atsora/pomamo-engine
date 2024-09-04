// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserMachine
{
  /// <summary>
  /// Description of DialogMachineStateTemplate.
  /// </summary>
  public partial class DialogMachineStateTemplate : Form
  {
    #region Members
    IMachine m_machine = null;
    IMachineStateTemplate m_mst = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Get / set the machine
    /// </summary>
    public IMachine Machine {
      get { return m_machine; }
      set { treeMachines.SelectedElement = value; }
    }
    
    /// <summary>
    /// Get / set the machine state template
    /// </summary>
    public IMachineStateTemplate MachineStateTemplate {
      get { return m_mst; }
      set { listMachineStateTemplates.SelectedValue = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DialogMachineStateTemplate()
    {
      InitializeComponent();
      
      // Machine list
      treeMachines.AddOrder("Sort by department", new string[] {"Company", "Department"});
      treeMachines.AddOrder("Sort by category", new string[] {"Company", "Category"});
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Machine list
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          foreach (IMachine machine in machines) {
            treeMachines.AddElement(machine);
          }

          // Machine state template list
          IList<IMachineStateTemplate> msts = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindAll();
          foreach (IMachineStateTemplate mst in msts) {
            listMachineStateTemplates.AddItem(mst.Display, mst);
          }
        }
      }
      
      treeMachines.SelectedOrder = 0;
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonCancelClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }
    
    void ButtonOkClick(object sender, EventArgs e)
    {
      m_machine = treeMachines.SelectedElement as IMachine;
      m_mst = listMachineStateTemplates.SelectedValue as IMachineStateTemplate;
      
      if (Machine == null) {
        MessageBoxCentered.Show(this, "Please select a machine", "Warning");
      }
      else if (MachineStateTemplate == null) {
        MessageBoxCentered.Show(this, "Please select a machine state template", "Warning");
      }
      else {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }
    #endregion // Event reactions
  }
}
