// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of ShiftTemplateDialog.
  /// </summary>
  public partial class ShiftTemplateDialog : OKCancelDialog, IValueDialog<IShiftTemplate>
  {
    #region Members
    private bool m_nullable;
    private bool m_multiSelet;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null ShiftTemplate a valid value ?
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
      get { return  shiftTemplateSelection.DisplayedProperty; }
      set { shiftTemplateSelection.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Return selected ShiftTemplate
    /// </summary>
    public IShiftTemplate SelectedValue{
      get{
        return this.shiftTemplateSelection.SelectedValue;
      }
      set {
        this.shiftTemplateSelection.SelectedValue = value;
      }
    }
    
    /// <summary>
    /// Return selected ShiftTemplates
    /// </summary>
    public IList<IShiftTemplate> SelectedValues{
      get{
        return this.shiftTemplateSelection.SelectedShiftTemplates;
      }
      set {
        this.shiftTemplateSelection.SelectedShiftTemplates = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftTemplateDialog()
    {
      InitializeComponent();
      this.Text = "Shift template selection";
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
      this.shiftTemplateSelection.MultiSelect = m_multiSelet;
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
    
    void ShiftTemplateDialogLoad(object sender, EventArgs e)
    {
      this.shiftTemplateSelection.MultiSelect = m_multiSelet;
      this.shiftTemplateSelection.SetOkButton = this.okButton;
      this.shiftTemplateSelection.Nullable = this.Nullable;
    }
  }
}
