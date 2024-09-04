// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of OkNextDialog.
  /// </summary>
  public partial class OkNextDialog : Form
  {
    #region Members
    readonly IDictionary<string, IItem> m_nextItems = new Dictionary<string, IItem>();
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (OkNextDialog).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// Next item to display
    /// </summary>
    public IItem NextItem { get; private set; }
    
    /// <summary>
    /// View mode for the next item to display
    /// </summary>
    public bool ViewMode { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OkNextDialog(IList<string> warnings, IDictionary<string, string> nextItems)
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (null != Form.ActiveForm) {
        this.Icon = Form.ActiveForm.Icon;
      }
      InitializeComponent();
      
      // Process warnings and next items
      ProcessWarnings(warnings);
      ProcessNextItems(nextItems);
    }
    #endregion // Constructors

    #region Methods
    void ProcessWarnings(ICollection<string> warnings)
    {
      if (warnings == null || warnings.Count == 0) {
        richTextBox.AppendText("The operation completed successfully!");
        pictureBox.Image = SystemIcons.Information.ToBitmap();
        this.Text = "Information";
      } else {
        richTextBox.AppendText(String.Join("\n\n", warnings.ToArray()));
        pictureBox.Image = SystemIcons.Warning.ToBitmap();
        this.Text = "Warning";
      }
    }
    
    void ProcessNextItems(IDictionary<string, string> nextItems)
    {
      bool introductionWritten = false;
      if (nextItems != null && nextItems.Count > 0) {
        foreach (var nextItem in nextItems) {
          try {
            string itemID = nextItem.Key.Substring(0, nextItem.Key.LastIndexOf ('.'));
            string itemSubID = nextItem.Key.Substring (nextItem.Key.LastIndexOf ('.') + 1);
            var item = ItemManager.GetItem(itemID, itemSubID);
            if (item != null) {
              if (!introductionWritten) {
                richTextBox.AppendText("\n\nAfter this wizard, you can:");
                introductionWritten = true;
              }
              m_nextItems[nextItem.Value] = item;
              richTextBox.AppendText("\n- ");
              richTextBox.InsertLink(nextItem.Value, nextItem.Key);
            } else {
              log.ErrorFormat ("ProcessNextItems: couldn't find an item with id {0} and subid {1}", itemID, itemSubID);
            }
          } catch (Exception e) {
            log.ErrorFormat("Couldn't parse next item id {0} ({1}): {2}",
                            nextItem.Key, nextItem.Value, e);
          }
        }
      }
      
      if (!introductionWritten && this.Text == "Information") {
        richTextBox.Select(0, 0);
        richTextBox.SelectedText = "\n\n";
      }
    }
    #endregion // Methods
    
    #region Event reactions
    void RichTextBoxLinkClicked(object sender, LinkClickedEventArgs e)
    {
      NextItem = m_nextItems[e.LinkText];
      ViewMode = false;
      this.Close();
    }
    
    void ButtonOkClick(object sender, EventArgs e)
    {
      NextItem = null;
      this.Close();
    }
    #endregion // Event reactions
  }
}
