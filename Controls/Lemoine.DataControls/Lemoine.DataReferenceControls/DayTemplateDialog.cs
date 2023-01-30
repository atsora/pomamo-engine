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
  /// Description of DayTemplateDialog.
  /// </summary>
  public partial class DayTemplateDialog : OKCancelDialog, IValueDialog<IDayTemplate>
  {
    #region Members
    private bool m_nullable;
    private bool m_multiSelet;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null DayTemplate a valid value ?
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
      get { return  dayTemplateSelection.DisplayedProperty; }
      set { dayTemplateSelection.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Return selected DayTemplate
    /// </summary>
    public IDayTemplate SelectedValue{
      get{
        return this.dayTemplateSelection.SelectedValue;
      }
      set {
        this.dayTemplateSelection.SelectedValue = value;
      }
    }
    
    /// <summary>
    /// Return selected DayTemplates
    /// </summary>
    public IList<IDayTemplate> SelectedValues{
      get{
        return this.dayTemplateSelection.SelectedDayTemplates;
      }
      set {
        this.dayTemplateSelection.SelectedDayTemplates = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DayTemplateDialog()
    {
      InitializeComponent();
      this.Text = "Day template selection";
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
      this.dayTemplateSelection.MultiSelect = m_multiSelet;
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
    
    void DayTemplateDialogLoad(object sender, EventArgs e)
    {
      this.dayTemplateSelection.MultiSelect = m_multiSelet;
      this.dayTemplateSelection.SetOkButton = this.okButton;
      this.dayTemplateSelection.Nullable = this.Nullable;
    }
  }
}
