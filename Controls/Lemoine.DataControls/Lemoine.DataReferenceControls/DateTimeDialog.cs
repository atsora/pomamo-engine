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
  /// Description of DateTimeDialog.
  /// </summary>
  public partial class DateTimeDialog : OKCancelDialog, IValueDialog<DateTime?>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DateTimeDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected DateTime
    /// </summary>
    public DateTime? SelectedValue {
      get {
        return this.dateTimeSelection.LocalDateTime;
      }
      set {
        this.dateTimeSelection.LocalDateTime = value;
      }
    }
    
    /// <summary>
    /// Return selected DateTimes
    /// </summary>
    public IList<DateTime?> SelectedValues {
      get {
        IList<DateTime?> dateTimes = new List<DateTime?> ();
        dateTimes.Add (this.SelectedValue);
        return dateTimes;
      }
      set {
        if (0 < value.Count) {
          this.SelectedValue = value [0];
        }
        else {
          this.SelectedValue = null;
        }
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">MultiSelect</see> implementation
    /// 
    /// Always false here
    /// </summary>
    public bool MultiSelect {
      get {
        return false;
      }
      set {
        if (value) {
          throw new ArgumentException ("Multi-select must be false");
        }
      }
    }
    
    /// <summary>
    /// Is nullable value valid ?
    /// </summary>
    public bool Nullable {
      get {
        return this.dateTimeSelection.Nullable;
      }
      set {
        this.dateTimeSelection.Nullable = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">NoSelectionText</see> implementation
    /// </summary>
    public string NoSelectionText {
      get {
        return this.dateTimeSelection.NoSelectionText;
      }
      set {
        this.dateTimeSelection.NoSelectionText = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DateTimeDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString("DateTimeDialog");
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