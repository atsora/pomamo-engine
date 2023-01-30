// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of ChangePeriodDialog.
  /// </summary>
  public partial class ChangePeriodDialog : Form
  {
    #region Getters / Setters
    /// <summary>
    /// Get StartDateTime
    /// </summary>
    public DateTime StartDateTime {
      get {
        return date1.Value.Date.Add(time1.Value.TimeOfDay);
      }
    }
    
    /// <summary>
    /// Get EndDateTime
    /// </summary>
    public DateTime EndDateTime {
      get {
        return date2.Value.Date.Add(time2.Value.TimeOfDay);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ChangePeriodDialog(DateTime startDateTime, DateTime endDateTime)
    {
      InitializeComponent();
      
      date1.Value = time1.Value = startDateTime;
      date2.Value = time2.Value = endDateTime;
    }
    #endregion // Constructors
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      if (StartDateTime >= EndDateTime) {
        MessageBoxCentered.Show("The end of the period must be posterior to its beginning.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      } else {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }
    #endregion // Event reactions
  }
}
