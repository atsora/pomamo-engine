// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Last page of every wizard, summarizing the user inputs
  /// </summary>
  public partial class SummaryPage : GenericWizardPage, IWizardPage
  {
    #region Members
    string m_log = "";
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title
    {
      get { return "Summary"; }
    }

    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help
    {
      get {
        return "Here is a summary of all your inputs. Please review it before validating.";
      }
    }

    /// <summary>
    /// Return the summary formated for the logs
    /// </summary>
    public string Log { get { return m_log; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SummaryPage ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Refresh the summary contained in the page
    /// </summary>
    /// <param name="warnings">warnings to display first</param>
    /// <param name="pages">each page has a summary of the user inputs</param>
    /// <param name="data">data completed by the wizard</param>
    public void RefreshSummary (IList<string> warnings, IList<IItemPage> pages, ItemData data)
    {
      using (new SuspendDrawing (treeView)) {
        treeView.Nodes.Clear ();
        m_log = "";

        // Warnings first
        if (warnings != null && warnings.Count > 0) {
          string plural = (warnings.Count > 1) ? "s" : "";
          TreeNode node = treeView.Nodes.Add ("Warning" + plural);
          node.NodeFont = new Font (this.Font, FontStyle.Bold);
          node.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          m_log += "  - " + node.Text + "\n";
          foreach (string warning in warnings) {
            string[] warningLines = warning.Split ('\n');
            int nbLines = warningLines.GetLength (0);
            if (nbLines > 0) {
              TreeNode subNode = node.Nodes.Add (warningLines[0]);
              subNode.ForeColor = LemSettingsGlobal.COLOR_ERROR;
              m_log += "      " + warningLines[0] + "\n";
              for (int i = 1; i < nbLines; i++) {
                TreeNode subsubNode = subNode.Nodes.Add (warningLines[i]);
                subsubNode.ForeColor = LemSettingsGlobal.COLOR_ERROR;
                m_log += "        " + warningLines[i] + "\n";
              }
            }
          }
        }

        int numPage = 1;
        foreach (var page in pages) {
          // Top node
          TreeNode node = treeView.Nodes.Add ("Page " + (numPage++) + ": " + page.Title);
          node.NodeFont = new Font (this.Font, FontStyle.Bold);
          m_log += "  - " + node.Text + "\n";

          // Details
          IList<string> summary = (page as IWizardPage).GetSummary (data);
          if (summary is not null) {
            foreach (string summaryPart in summary) {
              string[] summaryLines = summaryPart.Split ('\n');
              int nbLines = summaryLines.GetLength (0);
              if (nbLines > 0) {
                TreeNode subNode = node.Nodes.Add (summaryLines[0]);
                m_log += "      " + summaryLines[0] + "\n";
                for (int i = 1; i < nbLines; i++) {
                  subNode.Nodes.Add (summaryLines[i]);
                  m_log += "        " + summaryLines[i] + "\n";
                }
              }
            }
          }
        }

        if (m_log != "" && m_log[m_log.Length - 1] == '\n') {
          m_log = m_log.Remove (m_log.Length - 1, 1);
        }

        // Expand the tree
        treeView.ExpandAll ();

        // Show first line
        if (treeView.Nodes.Count > 0) {
          treeView.Nodes[0].EnsureVisible ();
        }
      }
    }

    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data) { }

    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data) { }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data) { return null; }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data) { return null; }
    #endregion // Methods
  }
}
