// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of DefaultReasonCell.
  /// </summary>
  public partial class DefaultReasonCell : UserControl
  {
    #region Events
    /// <summary>
    /// Emitted after the visibility of the configuration panel changed
    /// </summary>
    public event Action OnChecked;
    #endregion // Events
    
    #region Getters / Setters
    /// <summary>
    /// Get / set the checked state
    /// </summary>
    public bool Checked {
      get { return checkReason.Checked; }
      set { checkReason.Checked = value; }
    }
    
    /// <summary>
    /// Get / set the "overwrite required" parameter
    /// </summary>
    public bool OverwriteRequired {
      get { return checkOverwriteRequired.Checked; }
      set { checkOverwriteRequired.Checked = value; }
    }
    
    /// <summary>
    /// Get / set the machine filter for inclusion
    /// </summary>
    public IMachineFilter MachineFilterInclude {
      get { return comboboxMachineFilterInclude.SelectedValue as IMachineFilter; }
      set { comboboxMachineFilterInclude.SelectedValue = value; }
    }
    
    /// <summary>
    /// Get / set the machine filter for exclusion
    /// </summary>
    public IMachineFilter MachineFilterExclude {
      get { return comboboxMachineFilterExclude.SelectedValue as IMachineFilter; }
      set { comboboxMachineFilterExclude.SelectedValue = value; }
    }
    
    /// <summary>
    /// Get / set the maximum time a default reason is valid
    /// </summary>
    public TimeSpan? MaxTime {
      get {
        if (checkMaxTime.Checked) {
          return timeMax.Duration;
        }
        else {
          return null;
        }
      }
      set {
        if (value.HasValue) {
          checkMaxTime.Checked = true;
          timeMax.Duration = value.Value;
        } else {
          checkMaxTime.Checked = false;
          timeMax.Duration = new TimeSpan(0, 0, 0);
        }
      }
    }
    
    /// <summary>
    /// Get the reason
    /// </summary>
    public IReason Reason { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="reason"></param>
    public DefaultReasonCell(IReason reason)
    {
      Reason = reason;
      
      InitializeComponent();
      labelReasonDisplay.Text = reason.Display;
      if (String.IsNullOrEmpty(reason.Description)) {
        labelDescription.Text = "no description";
      }
      else {
        labelDescription.Text = reason.Description;
      }

      markerReasonColor.Brush = new SolidBrush(ColorTranslator.FromHtml(reason.Color));
      
      // List of machine filters
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineFilter> filters = ModelDAOHelper.DAOFactory.MachineFilterDAO.FindAll();
          foreach (IMachineFilter filter in filters) {
            comboboxMachineFilterInclude.AddItem(filter.Name, filter);
            comboboxMachineFilterExclude.AddItem(filter.Name, filter);
          }
        }
      }
      comboboxMachineFilterInclude.InsertItem("no machine filter", null, 0);
      comboboxMachineFilterExclude.InsertItem("no machine filter", null, 0);
      
      CheckReasonCheckedChanged(null, null);
    }
    #endregion // Constructors

    #region Event reactions
    void CheckReasonCheckedChanged(object sender, EventArgs e)
    {
      if (checkReason.Checked) {
        layoutConf.Show();
        this.MaximumSize = new Size(0, 100);
        this.MinimumSize = new Size(0, 100);
      } else {
        layoutConf.Hide();
        this.MinimumSize = new Size(0, 26);
        this.MaximumSize = new Size(0, 26);
      }
      if (OnChecked != null) {
        OnChecked ();
      }
    }
    
    void CheckMaxTimeCheckedChanged(object sender, EventArgs e)
    {
      timeMax.Enabled = checkMaxTime.Checked;
    }
    #endregion // Event reactions
  }
}
