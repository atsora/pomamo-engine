// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of DialogNewPart.
  /// </summary>
  public partial class DialogNewPart : Form
  {
    #region Members
    string m_lineName = "";
    string m_lineCode = "";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DialogNewPart).FullName);

    #region Getters / Setters
    /// <summary>
    /// True if an existing part has been chosen
    /// </summary>
    public bool Existing { get { return radioExisting.Checked; } }
    
    /// <summary>
    /// Selected part
    /// </summary>
    public IPart Part { get { return (IPart)comboPart.SelectedValue; } }
    
    /// <summary>
    /// Name of the new part
    /// </summary>
    public string PartName { get { return textPartName.Text; } }
    
    /// <summary>
    /// Code of the new part
    /// </summary>
    public string PartCode { get { return textPartCode.Text; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DialogNewPart(string lineName, string lineCode)
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (null != Form.ActiveForm) {
        this.Icon = Form.ActiveForm.Icon;
      }
      InitializeComponent();
      
      m_lineName = lineName;
      m_lineCode = lineCode;
      
      FillParts();
      
      this.ActiveControl = textPartName;
      textPartName.Focus();
    }
    #endregion // Constructors

    #region Methods
    void FillParts()
    {
      // List of parts (only parts whose operations only provide 1 iwp).
      comboPart.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IPart> parts = ModelDAOHelper.DAOFactory.PartDAO.FindAll();
          foreach (IPart part in parts) {
            if (part.ComponentIntermediateWorkPieces != null) {
              ICollection<IComponentIntermediateWorkPiece> ciwps = part.ComponentIntermediateWorkPieces;
              bool valid = true;
              IList<ISimpleOperation> operations = new List<ISimpleOperation>();
              foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
                if (ciwp.IntermediateWorkPiece.SimpleOperation == null ||
                    operations.Contains(ciwp.IntermediateWorkPiece.SimpleOperation)) {
                  valid = false;
                  break;
                } else {
                  operations.Add(ciwp.IntermediateWorkPiece.SimpleOperation);
                }
              }
              if (valid) {
                comboPart.AddItem(part.Display, part);
              }
            }
          }
        }
      }

      if (comboPart.Count == 0) {
        radioExisting.Enabled = false;
      }
      else {
        comboPart.SelectedIndex = 0;
      }
    }
    #endregion // Methods
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      if (radioNew.Checked && textPartName.Text == "") {
        MessageBoxCentered.Show("The name cannot be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }
    
    void ButtonCancelClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }
    
    void RadioNewCheckedChanged(object sender, EventArgs e)
    {
      label4.Enabled = label5.Enabled = textPartCode.Enabled = textPartName.Enabled =
        buttonCopy.Enabled = radioNew.Checked;
    }
    
    void RadioExistingCheckedChanged(object sender, EventArgs e)
    {
      comboPart.Enabled = radioExisting.Checked;
    }
    
    void ButtonCopyClick(object sender, EventArgs e)
    {
      textPartName.Text = m_lineName;
      textPartCode.Text = m_lineCode;
    }
    #endregion // Event reactions
  }
}
