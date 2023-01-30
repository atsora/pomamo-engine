// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of OrderDialog.
  /// </summary>
  public partial class OrderDialog : OKCancelDialog, IValueDialog<int>
  {
    #region Members
    private bool m_nullable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OrderDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null DateTime a valid value ?
    /// </summary>
    public bool Nullable {
      get { return this.m_nullable; }
      set { this.SetNullable(value);}
    }
    
    /// <summary>
    /// Get Index
    /// </summary>
    public int SelectedValue {
      get {
        return this.orderSelection.SelectedIndex;
      }
      set {;}
    }
    
    /// <summary>
    /// Not Implemented
    /// </summary>
    public System.Collections.Generic.IList<int> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    
    /// <summary>
    /// Set Maximum selectable index
    /// </summary>
    public int MaximumIndex{
      set { this.orderSelection.MaxIndex = value; }
      get { return this.orderSelection.MaxIndex;}
    }
    
    /// <summary>
    /// Set Minimum selectable index
    /// </summary>
    public int MinimumIndex{
      set { this.orderSelection.MinIndex = value; }
      get { return this.orderSelection.MinIndex;}
    }
    
    /// <summary>
    /// Did user set a index or not ?
    /// </summary>
    public bool UserSpecifiedIndex{
      get { return this.orderSelection.UserSpecifiedIndex; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create a Dialog for Order used to set Order 
    /// at creation (only) of certain object.
    /// </summary>
    public OrderDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("OrderDialogTitle");
      this.orderSelection.OkButton = this.okButton;
    }


    #endregion // Constructors

    #region Methods
    
    /// <summary>
    /// Set if the return value can be null or not
    /// Block user for returning good value
    /// </summary>
    /// <param name="value"></param>
    private void SetNullable(bool value){
      m_nullable = value;
      this.orderSelection.Nullable = m_nullable;
      if(!value){
        this.okButton.Enabled = false;
        this.cancelButton.Enabled = false;
      }
    }
    #endregion // Methods
    
  }
}
