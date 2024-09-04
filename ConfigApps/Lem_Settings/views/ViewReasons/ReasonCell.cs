// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace ViewReasons
{
  /// <summary>
  /// Description of ReasonCell.
  /// </summary>
  public partial class ReasonCell : UserControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="reason"></param>
    public ReasonCell(OrderedReason reason)
    {
      InitializeComponent();
      labelReasonDisplay.Text = reason.Description;
      if (reason.IsDefault) {
        labelReasonDisplay.Font = new Font(labelReasonDisplay.Font, FontStyle.Bold);
        labelReasonDisplay.Text += " (default)";
      }
      markerReasonColor.Brush = new SolidBrush(reason.Color);
      
      if (reason.IsInherited) {
        labelReasonDisplay.ForeColor = Color.DarkGray;
        labelReasonDetails.ForeColor = Color.DarkGray;
      }
      
      var details = new List<string>();
      if (reason.DetailsRequired) {
        details.Add("details required");
      }

      if (reason.OverwriteRequired) {
        details.Add("overwrite required");
      }

      if (reason.MachineFilterExclude != null) {
        details.Add("exclusion filter: " + reason.MachineFilterExclude.Name);
      }

      if (reason.MachineFilterInclude != null) {
        details.Add("inclusion filter: " + reason.MachineFilterInclude.Name);
      }

      if (reason.MaxTime.HasValue) {
        details.Add("max time: " + reason.MaxTime.Value + " s");
      }

      bool firstDetail = true;
      labelReasonDetails.Text = "";
      foreach (var detail in details) {
        if (firstDetail) {
          firstDetail = false;
          labelReasonDetails.Text += detail.First().ToString().ToUpper() + detail.Substring(1);
        } else {
          labelReasonDetails.Text += ", " + detail;
        }
      }
    }
    #endregion // Constructors
  }
}
