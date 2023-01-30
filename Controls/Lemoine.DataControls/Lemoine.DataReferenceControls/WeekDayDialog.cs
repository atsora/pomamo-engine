// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of WeekDayConfig.
  /// </summary>
  public partial class WeekDayDialog : OKCancelDialog, IValueDialog<WeekDay>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WeekDayDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected value(s)
    /// </summary>
    public WeekDay SelectedValue {
      get {
        return this.weekDaySelection1.SelectedDays;
      }
      set {
        this.weekDaySelection1.SelectedDays = value;
      }
    }
    
    /// <summary>
    /// No reason to be Implemented.
    /// </summary>
    public System.Collections.Generic.IList<WeekDay> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WeekDayDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("WeekDayDialogTitle");
      this.weekDaySelection1.SetOkButton = this.okButton;
    }

    #endregion // Constructors    
  }
}
