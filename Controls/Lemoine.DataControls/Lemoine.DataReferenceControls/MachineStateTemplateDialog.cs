// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of MachineStateTemplateDialog.
  /// </summary>
  public partial class MachineStateTemplateDialog : OKCancelDialog, IValueDialog<IMachineStateTemplate>
  {
    #region Members
    private bool m_nullable;
    private bool m_multiSelet;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineStateTemplate a valid value ?
    /// </summary>
    public bool Nullable {
      get { return this.m_nullable; }
      set { this.SetNullable(value);}
    }
    
    /// <summary>
    /// allow/disallow multi-selection
    /// </summary>
    public bool MultiSelect {
      get { return this.m_multiSelet;  }
      set { this.SetMultiSelect(value);}
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineStateTemplateSelection.DisplayedProperty; }
      set { machineStateTemplateSelection.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Return selected MachineStateTemplate
    /// </summary>
    public IMachineStateTemplate SelectedValue{
      get{
        return this.machineStateTemplateSelection.SelectedValue;
      }
      set {
        this.machineStateTemplateSelection.SelectedValue = value;
      }
    }
    
    /// <summary>
    /// Return selected MachineStateTemplates
    /// </summary>
    public IList<IMachineStateTemplate> SelectedValues{
      get{
        return this.machineStateTemplateSelection.SelectedMachineStateTemplates;
      }
      set {
        this.machineStateTemplateSelection.SelectedMachineStateTemplates = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateDialog()
    {
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("MachineStateTemplateDialogTitle");
    }
    #endregion // Constructors

    #region Methods
    private void SetNullable(bool value){
      m_nullable = value;
      if(!value){
        this.okButton.Enabled = false;
      }
    }
    
    private void SetMultiSelect(bool value){
      m_multiSelet = value;
      this.machineStateTemplateSelection.MultiSelect = m_multiSelet;
    }
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Methods
    
    void MachineStateTemplateDialogLoad(object sender, EventArgs e)
    {
      this.machineStateTemplateSelection.MultiSelect = m_multiSelet;
      this.machineStateTemplateSelection.SetOkButton = this.okButton;
      this.machineStateTemplateSelection.Nullable = this.Nullable;
    }
  }
}
