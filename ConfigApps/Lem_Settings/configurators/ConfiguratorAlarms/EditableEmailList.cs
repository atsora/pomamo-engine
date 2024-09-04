// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of EditableEmailList.
  /// </summary>
  public partial class EditableEmailList : UserControl
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public EditableEmailList()
    {
      InitializeComponent();
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Add an email in the list
    /// </summary>
    /// <param name="email">email to be added</param>
    /// <returns></returns>
    public int AddEmail(EmailWithName email)
    {
      return listBoxEmail.Items.Add(email);
    }
    
    /// <summary>
    /// Check an element identified by its id
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isSelected"></param>
    public void SetChecked(int index, bool isSelected)
    {
      listBoxEmail.SetItemChecked(index, isSelected);
    }
    
    /// <summary>
    /// Return all emails checked
    /// </summary>
    /// <returns></returns>
    public IList<EmailWithName> GetCheckedEmails()
    {
      return listBoxEmail.CheckedItems.Cast<EmailWithName>().ToList();
    }
    
    /// <summary>
    /// Clear all items
    /// </summary>
    public void Clear()
    {
      listBoxEmail.Items.Clear();
    }
    
    /// <summary>
    /// Clear the edit box
    /// </summary>
    public void ResetEditBox()
    {
      textBoxEmail.Clear();
    }
    #endregion // Methods
    
    #region WidgetReaction
    void ButtonAddClick(object sender, EventArgs e)
    {
      EmailWithName newEmail = new EmailWithName(textBoxEmail.Text);
      
      // Is the email valid?
      if (newEmail.IsValid()) {
        textBoxEmail.ResetBackColor();
      } else {
        textBoxEmail.BackColor = Color.LightCoral;
        return;
      }

      if (!listBoxEmail.Items.Contains(newEmail)) {
        int index = listBoxEmail.Items.Add(newEmail, true);
        listBoxEmail.SelectedIndex = index;
      }
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      if (listBoxEmail.SelectedIndex != -1) {
        listBoxEmail.Items.RemoveAt(listBoxEmail.SelectedIndex);
      }
    }
    #endregion
  }
}
