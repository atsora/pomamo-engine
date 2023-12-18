// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.DataReferenceControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lem_CncGUI
{
  /// <summary>
  /// Description of ConfigParamDialog.
  /// </summary>
  internal partial class ConfigParamDialog : Form
  {
    #region Members
    IMachineModule m_machineModule1 = null;
    IMachineModule m_machineModule2 = null;
    IMachineModule m_machineModule3 = null;
    IMachineModule m_machineModule4 = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ConfigParamDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Created Cnc Acquisition
    /// </summary>
    public ICncAcquisition CncAcquisition
    {
      get
      {
        var name = nameTextBox.Text;
        ICncAcquisition cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
        cncAcquisition.Name = name;
        cncAcquisition.ConfigFile = fileRepositorySelection1.SelectedFiles [0];
        cncAcquisition.ConfigPrefix = prefixTextBox.Text;
        cncAcquisition.ConfigParameters = parametersTextBox.Text;
        // TODO: cncAcquisition.ConfigKeyParams
        if (null != m_machineModule1) {
          m_machineModule1.ConfigPrefix = prefixTextBox1.Text;
          m_machineModule1.ConfigParameters = parametersTextBox1.Text;
          cncAcquisition.MachineModules.Add (m_machineModule1);
        }
        else if (0 < parametersTextBox1.Text.Length) {
          var prefix = prefixTextBox1.Text;
          m_machineModule1 = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (null, name + "-" + prefix);
          m_machineModule1.ConfigPrefix = prefix;
          m_machineModule1.ConfigParameters = parametersTextBox1.Text;
          cncAcquisition.MachineModules.Add (m_machineModule1);
        }
        if (null != m_machineModule2) {
          m_machineModule2.ConfigPrefix = prefixTextBox2.Text;
          m_machineModule2.ConfigParameters = parametersTextBox2.Text;
          cncAcquisition.MachineModules.Add (m_machineModule2);
        }
        else if (0 < parametersTextBox2.Text.Length) {
          var prefix = prefixTextBox2.Text;
          m_machineModule2 = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (null, name + "-" + prefix);
          m_machineModule2.ConfigPrefix = prefix;
          m_machineModule2.ConfigParameters = parametersTextBox2.Text;
          cncAcquisition.MachineModules.Add (m_machineModule2);
        }
        if (null != m_machineModule3) {
          m_machineModule3.ConfigPrefix = prefixTextBox3.Text;
          m_machineModule3.ConfigParameters = parametersTextBox3.Text;
          cncAcquisition.MachineModules.Add (m_machineModule3);
        }
        else if (0 < parametersTextBox3.Text.Length) {
          var prefix = prefixTextBox3.Text;
          m_machineModule3 = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (null, name + "-" + prefix);
          m_machineModule3.ConfigPrefix = prefix;
          m_machineModule3.ConfigParameters = parametersTextBox3.Text;
          cncAcquisition.MachineModules.Add (m_machineModule3);
        }
        if (null != m_machineModule4) {
          m_machineModule4.ConfigPrefix = prefixTextBox4.Text;
          m_machineModule4.ConfigParameters = parametersTextBox4.Text;
          cncAcquisition.MachineModules.Add (m_machineModule4);
        }
        else if (0 < parametersTextBox4.Text.Length) {
          var prefix = prefixTextBox4.Text;
          m_machineModule4 = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (null, name + "-" + prefix);
          m_machineModule4.ConfigPrefix = prefix;
          m_machineModule4.ConfigParameters = parametersTextBox4.Text;
          cncAcquisition.MachineModules.Add (m_machineModule4);
        }
        return cncAcquisition;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfigParamDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }

    #endregion // Constructors

    #region Methods
    #endregion // Methods
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close ();
    }
    
    void MachineModuleLabel1Click(object sender, EventArgs e)
    {
      MachineModuleDialog dialog = new MachineModuleDialog ();
      dialog.DisplayedProperty = "SelectionText";
      if (DialogResult.OK == dialog.ShowDialog ()) {
        m_machineModule1 = dialog.SelectedValue;
        machineModuleLabel1.Text = m_machineModule1.SelectionText;
      }
    }

    void MachineModuleLabel2Click(object sender, EventArgs e)
    {
      MachineModuleDialog dialog = new MachineModuleDialog ();
      dialog.DisplayedProperty = "SelectionText";
      if (DialogResult.OK == dialog.ShowDialog ()) {
        m_machineModule2 = dialog.SelectedValue;
        machineModuleLabel2.Text = m_machineModule2.SelectionText;
      }
    }

    void MachineModuleLabel3Click(object sender, EventArgs e)
    {
      MachineModuleDialog dialog = new MachineModuleDialog ();
      dialog.DisplayedProperty = "SelectionText";
      if (DialogResult.OK == dialog.ShowDialog ()) {
        m_machineModule3 = dialog.SelectedValue;
        machineModuleLabel3.Text = m_machineModule3.SelectionText;
      }
    }

    void MachineModuleLabel4Click(object sender, EventArgs e)
    {
      MachineModuleDialog dialog = new MachineModuleDialog ();
      dialog.DisplayedProperty = "SelectionText";
      if (DialogResult.OK == dialog.ShowDialog ()) {
        m_machineModule4 = dialog.SelectedValue;
        machineModuleLabel4.Text = m_machineModule4.SelectionText;
      }
    }
  }
}
