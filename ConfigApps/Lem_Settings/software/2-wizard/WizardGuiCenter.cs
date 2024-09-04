// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Container for wizard pages
  /// </summary>
  public class WizardGuiCenter : GuiCenter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WizardGuiCenter).FullName);

    #region Members
    readonly IList<IList<string>> m_currentWarnings = new List<IList<string>> ();
    bool m_isRightButtonOk = false;
    SummaryPage m_summaryPage = new SummaryPage ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WizardGuiCenter ()
    {
      DisplayPrevious ();
      PreparePage (m_summaryPage);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current wizard
    /// </summary>
    /// <param name="wizard"></param>
    /// <param name="otherData"></param>
    public void SetCurrentItem (IWizard wizard, ItemData otherData)
    {
      // Variable initialization
      m_otherData = otherData;
      m_currentItem = wizard;
      m_itemPages.Clear ();
      m_guiLeft.SetCurrentItem (m_currentItem);

      // Initialization of the item and its data
      m_itemData = (m_currentItem as IWizard).Initialize (otherData);
      m_guiLeft.SetCurrentItemData (m_itemData);

      // Specific to wizards
      ProtectAgainstQuit = false;
      m_currentWarnings.Clear ();

      // Page list
      var pages = (m_currentItem as IWizard).Pages;
      foreach (var page in pages) {
        page.Initialize (m_currentItem.Context);
        page.SetTitle += OnSetTitle;
        page.SpecifyHeader += OnSpecifyHeader;
        PreparePage (page);
        m_itemPages.Add (page.GetType ().Name, page);
      }
      m_currentPages.Add (pages[0]);
      DisplayLastPage ();
    }

    void PreparePage (IItemPage page)
    {
      Control control = page as UserControl;
      control.Dock = DockStyle.Fill;
      control.Padding = new Padding (0);
    }

    /// <summary>
    /// Reinit the item displayed
    /// </summary>
    public override void Reinit ()
    {
      var wizard = m_currentItem as IWizard;
      ItemData data = m_otherData;
      EndWizard ();
      SetCurrentItem (wizard, data);
    }

    /// <summary>
    /// Show or hide the buttons depending on the page and its configuration
    /// </summary>
    protected override void PrepareButtons ()
    {
      SetLeftButtonEnabled (m_currentPages.Count > 1);
      SetLeftButtonVisible (true);

      m_isRightButtonOk = (GetLastPage ().GetType ().Name == "SummaryPage");
      if (m_isRightButtonOk) {
        DisplayOk ();
      }
      else {
        DisplayNext ();
      }

      SetRightButtonVisible (true);
    }

    void DisplayWarnings (IList<string> warnings)
    {
      string text = String.Join ("\n\n", warnings.ToArray ());
      MessageBoxCentered.Show (this, text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void Log (IList<string> warnings, long validationTimeMs)
    {
      // Description of the context
      string context = "\n### WIZARD \"" + m_currentItem.Title + "\" VALIDATED ###\n" +
        "  - ID       = " + m_currentItem.ID + "\n" +
        "  - Sub ID   = " + m_currentItem.SubID + "\n" +
        "  - Dll path = " + m_currentItem.DllPath + "\n" +
        "  - Ini path = " + m_currentItem.IniPath;

      // Description of the data
      string data = "### DESCRIPTION OF THE DATA ###\n" + m_itemData.ToString ();

      // Summary
      string summary = "### SUMMARY ###\n" + m_summaryPage.Log;

      // Result
      string result = "### RESULT ###\n  - " + string.Join ("\n  - ", warnings.ToArray ());
      if (warnings.Count == 0) {
        result += "ok";
      }

      // Elapsed time
      string elapsedTime = "Elapsed time: " + ((double)validationTimeMs) / 1000 + "s";

      // Send log
      logHistory.Info (context + "\n\n" + data + "\n\n" + summary + "\n\n" +
                      result + "\n\n" + elapsedTime + "\n");
    }

    void EndWizard ()
    {
      ProtectAgainstQuit = false;
      m_currentItem = null;
      m_currentPages.Clear ();
      m_currentWarnings.Clear ();
      m_itemData = null;
      m_otherData = null;
      RemovePage ();
      while (m_itemPages.Keys.Count > 0) {
        string key = m_itemPages.Keys.First ();
        var page = m_itemPages[key];
        m_itemPages.Remove (key);
        page.Dispose ();
      }
    }

    IWizardPage GetLastPage ()
    {
      return m_currentPages.Last () as IWizardPage;
    }
    #endregion // Methods

    #region Event reactions
    /// <summary>
    /// Method called when the right button is clicked
    /// </summary>
    protected override void OnRightButtonClick ()
    {
      ProtectAgainstQuit = true;
      var lastPage = GetLastPage ();
      lastPage.SavePageInData (m_itemData);

      // Display errors if any. Return if they are not ignored.
      IList<string> errors = lastPage.GetErrorsToGoNext (m_itemData);
      if (errors != null && errors.Count > 0) {
        if (!DisplayErrors (errors, (lastPage.Flags &
                                    LemSettingsGlobal.PageFlag.IGNORE_IMPOSSIBLE) == 0)) {
          return;
        }
      }

      // Display warnings if any
      IList<string> warnings = lastPage.GetWarnings (m_itemData) ?? new List<string> ();
      if (warnings.Count > 0) {
        DisplayWarnings (warnings);
      }

      m_currentWarnings.Add (warnings);

      if (m_isRightButtonOk) {
        warnings.Clear ();
        IRevision revision = null;

        // If an exception occurs, the application continues to run
        MainForm.QuitIfException = false;
        Stopwatch stopWatch = Stopwatch.StartNew ();
        (m_currentItem as IWizard).Finalize (m_itemData, ref warnings, ref revision);
        MainForm.QuitIfException = true;

        Lemoine.WebClient.Request.NotifyConfigUpdate ();

        if (revision != null) {
          // Add information to the revision
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              ModelDAOHelper.DAOFactory.RevisionDAO.Lock (revision);
              revision.Application = typeof (Program).Assembly.GetName ().Name;
              if (Lemoine.Info.ComputerInfo.GetIPAddresses ().Any ()) {
                revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().First ();
              }

              revision.Comment = "Wizard " + m_currentItem.ID + "." + m_currentItem.SubID + " (" +
                m_currentItem.Title + ")";
              transaction.Commit ();
            }
          }

          // Show a message so that the user knows something is being updated in the database
          warnings.Add ("The database is being updated. This is usually fast but may require some minutes (revision #" + revision.Id + ").");

          // Display the progression of the revision
          RevisionManager.AddRevision (revision);
        }

        // Notify edited types
        IList<Type> editedTypes = new List<Type> ();
        foreach (Type type in m_currentItem.Types.Keys) {
          if ((m_currentItem.Types[type] & LemSettingsGlobal.InteractionType.PRINCIPAL) != 0) {
            editedTypes.Add (type);
          }
        }

        ExternalWindow.DataChanged (editedTypes);

        // Log what has just happened
        Log (warnings, stopWatch.ElapsedMilliseconds);
        stopWatch.Stop ();

        // Show success message
        var finalDialog = new OkNextDialog (warnings, (m_currentItem as IWizard).GetPossibleNextItems (m_itemData));
        finalDialog.ShowDialog ();

        // Quit the wizard
        var otherData = m_itemData;
        EndWizard ();
        if (finalDialog.NextItem == null) {
          // Back to GuiCenter1
          EmitItemFinished ();
        }
        else {
          // Another item is triggered
          EmitItemLaunched (finalDialog.NextItem, finalDialog.ViewMode, otherData, true);
        }
      }
      else {
        string nextPageName = lastPage.GetNextPageName (m_itemData);
        if (String.IsNullOrEmpty (nextPageName)) {
          // Prepare warnings
          IList<string> allWarnings = new List<string> ();
          foreach (IList<string> subWarnings in m_currentWarnings) {
            foreach (string warning in subWarnings) {
              allWarnings.Add (warning);
            }
          }

          // Prepare the summary page
          m_summaryPage.RefreshSummary (allWarnings, m_currentPages, m_itemData);
          m_currentPages.Add (m_summaryPage);
        }
        else {
          m_currentPages.Add (m_itemPages[nextPageName]);
        }

        DisplayLastPage ();
      }
    }

    /// <summary>
    /// Method called when the left button is clicked
    /// </summary>
    protected override void OnLeftButtonClick ()
    {
      var lastPage = GetLastPage ();
      lastPage.SavePageInData (m_itemData);
      lastPage.DoSomethingBeforePrevious (m_itemData);
      m_currentPages.RemoveAt (m_currentPages.Count () - 1);
      m_currentWarnings.RemoveAt (m_currentWarnings.Count () - 1);
      DisplayLastPage ();
    }

    /// <summary>
    /// Method called when the button "home" is clicked
    /// </summary>
    protected override void OnButtonHomeClick ()
    {
      if (ProtectAgainstQuit) {
        if (MessageBoxCentered.Show (this, "Modifications have not been taken into account yet.\n" +
                                    "Do you really want to quit the wizard?", "Question",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
          EndWizard ();
          EmitItemFinished ();
        }
      }
      else {
        EmitItemFinished ();
        EndWizard ();
      }
    }
    #endregion // Event reactions
  }
}
