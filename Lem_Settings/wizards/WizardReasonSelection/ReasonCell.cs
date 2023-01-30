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
  /// Description of ReasonCell.
  /// </summary>
  public partial class ReasonCell : UserControl
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
    /// Get / set the "details required" parameter
    /// </summary>
    public bool DetailsRequired {
      get { return checkDetailsRequired.Checked; }
      set { checkDetailsRequired.Checked = value; }
    }
    
    /// <summary>
    /// Get / set the machine filter
    /// </summary>
    public IMachineFilter MachineFilter {
      get { return comboboxMachineFilter.SelectedValue as IMachineFilter; }
      set { comboboxMachineFilter.SelectedValue = value; }
    }
    
    /// <summary>
    /// Get the reason
    /// </summary>
    public IReason Reason { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor (within a transaction)
    /// </summary>
    /// <param name="reason"></param>
    public ReasonCell(IReason reason)
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
            comboboxMachineFilter.AddItem(filter.Name, filter);
          }
        }
      }
      comboboxMachineFilter.InsertItem("no machine filter", null, 0);
      
      CheckBoxCheckedChanged(null, null);
    }
    #endregion // Constructors
    
    #region Event reactions
    void CheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (checkReason.Checked) {
        layoutConf.Show();
        this.MaximumSize = new Size(0, 75);
        this.MinimumSize = new Size(0, 75);
      } else {
        layoutConf.Hide();
        this.MinimumSize = new Size(0, 26);
        this.MaximumSize = new Size(0, 26);
      }
      if (OnChecked != null) {
        OnChecked ();
      }
    }
    #endregion // Event reactions
  }
}
