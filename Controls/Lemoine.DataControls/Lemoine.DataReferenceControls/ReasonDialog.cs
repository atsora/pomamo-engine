// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of ReasonDialog.
  /// </summary>
  public partial class ReasonDialog : OKCancelDialog, IValueDialog<IReason>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected Reason
    /// </summary>
    public IReason SelectedValue {
      get {
        return this.reasonSelection.SelectedT;
      }
      set {
        this.reasonSelection.SelectedT = value;
      }
    }
    
    /// <summary>
    /// Return selected Reasons
    /// </summary>
    public System.Collections.Generic.IList<IReason> SelectedValues {
      get {
        return this.reasonSelection.SelectedTs;
      }
      set {
        this.reasonSelection.SelectedTs = value;
      }
    }
    
    /// <summary>
    /// Set/Get if nullable Reason are ok
    /// </summary>
    public bool Nullable {
      get {
        return this.reasonSelection.Nullable;
      }
      set {
        this.reasonSelection.Nullable = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString("ReasonDialog");
    }
    #endregion // Constructors

    #region Methods

    #endregion // Methods
  }
}
