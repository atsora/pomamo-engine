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
  /// Description of ReasonGroupDialog.
  /// </summary>
  public partial class ReasonGroupDialog : OKCancelDialog, IValueDialog<IReasonGroup>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonGroupDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null ReasonGroup a valid value ?
    /// </summary>
    public bool Nullable {
      get { return reasonGroupSelection1.Nullable; }
      set { reasonGroupSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  reasonGroupSelection1.DisplayedProperty; }
      set { reasonGroupSelection1.DisplayedProperty = value; }
    }

    /// <summary>
    /// Selected ReasonGroup or null if no ReasonGroup is selected
    /// 
    /// Setable selection
    /// </summary>
    public IReasonGroup SelectedValue {
      get
      {
        return this.reasonGroupSelection1.SelectedReasonGroup;
      }
      set {
        this.reasonGroupSelection1.SelectedReasonGroup = value;
      }
    }
    
    /// <summary>
    /// Selected ReasonGroup or null if no ReasonGroup is selected
    /// 
    /// Setable selection
    /// </summary>
    public IList<IReasonGroup> SelectedValues {
      get
      {
        return this.reasonGroupSelection1.SelectedReasonGroups;
      }
      set {
        this.reasonGroupSelection1.SelectedReasonGroups = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonGroupDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("ReasonGroupDialogTitle");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
  }
}
