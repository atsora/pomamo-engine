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
  /// Container for configurator pages
  /// </summary>
  public class ConfiguratorGuiCenter : GuiCenter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfiguratorGuiCenter).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfiguratorGuiCenter () : base ()
    {
      DisplayOk ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current configurator
    /// </summary>
    /// <param name="configurator"></param>
    /// <param name="otherData"></param>
    public void SetCurrentItem (IConfigurator configurator, ItemData otherData)
    {
      // Variable initialization
      m_otherData = otherData;
      m_currentItem = configurator;
      m_itemPages.Clear ();
      m_guiLeft.SetCurrentItem (m_currentItem);

      // Initialization of the item and its data
      m_itemData = (m_currentItem as IConfigurator).Initialize (otherData);
      m_guiLeft.SetCurrentItemData (m_itemData);

      // Page list
      IList<IConfiguratorPage> listTmp = (m_currentItem as IConfigurator).Pages;
      foreach (IConfiguratorPage page in listTmp) {
        page.Initialize (m_currentItem.Context);
        page.SetTitle += OnSetTitle;
        page.SpecifyHeader += OnSpecifyHeader;
        page.DisplayPageEvent += OnDisplayPage;
        page.DataChangedEvent += OnDataChanged;
        page.ProtectAgainstQuit += OnProtectAgainstQuit;
        page.LogAction += OnLogAction;

        var control = page as UserControl;
        control.Dock = DockStyle.Fill;
        control.Padding = new Padding (0);

        m_itemPages.Add (page.GetType ().Name, page);
      }
      m_currentPages.Add (listTmp[0]);
      DisplayLastPage ();
    }

    /// <summary>
    /// Reinit the item displayed
    /// </summary>
    public override void Reinit ()
    {
      var configurator = m_currentItem as IConfigurator;
      ItemData data = m_otherData;
      EndConfigurator ();
      SetCurrentItem (configurator, data);
    }

    /// <summary>
    /// Show or hide the buttons depending on the page and its configuration
    /// </summary>
    protected override void PrepareButtons ()
    {
      if ((GetLastPage ().Flags & LemSettingsGlobal.PageFlag.WITH_VALIDATION) != 0) {
        DisplayCancel ();
        SetLeftButtonVisible (true);
        SetRightButtonVisible (true);
      }
      else {
        DisplayPrevious ();
        SetRightButtonVisible (false);
        SetLeftButtonVisible (m_currentPages.Count > 1);
      }
    }

    void Log (IList<string> warnings, long validationTimeMs)
    {
      // Description of the context
      string context = "\n### CONFIGURATOR \"" + m_currentItem.Title + "\" VALIDATED ###\n" +
        "  - Page     = \"" + GetLastPage ().Title + "\"\n" +
        "  - ID       = " + m_currentItem.ID + "\n" +
        "  - Sub ID   = " + m_currentItem.SubID + "\n" +
        "  - Dll path = " + m_currentItem.DllPath + "\n" +
        "  - Ini path = " + m_currentItem.IniPath;

      // Description of the data
      string data = "### DESCRIPTION OF THE DATA ###\n" + m_itemData.ToString ();

      // Result
      string result = "### RESULT ###\n  - " + string.Join ("\n  - ", warnings.ToArray ());
      if (warnings.Count == 0) {
        result += "ok";
      }

      // Elapsed time
      string elapsedTime = "Elapsed time: " + ((double)validationTimeMs) / 1000 + "s";

      // Send log
      logHistory.Info (context + "\n\n" + data + "\n\n" + result + "\n\n" + elapsedTime + "\n");
    }

    void EndConfigurator ()
    {
      ProtectAgainstQuit = false;
      m_currentItem = null;
      m_currentPages.Clear ();
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

    IConfiguratorPage GetLastPage ()
    {
      return m_currentPages.Last () as IConfiguratorPage;
    }
    #endregion // Methods

    #region Event reactions
    void OnDisplayPage (string pageName, IList<string> errors, bool ignorePossible)
    {
      if (errors != null && errors.Count > 0) {
        if (!DisplayErrors (errors, ignorePossible)) {
          return;
        }
      }

      if (!m_itemPages.ContainsKey (pageName)) {
        errors = new List<string> ();
        string text = "the page \"" + pageName + "\" couldn't be found";
        errors.Add (text);
        log.Error (text);
        DisplayErrors (errors, false);
        return; // Even if the error is ignored
      }

      GetLastPage ().SavePageInData (m_itemData);
      var pageToDisplay = m_itemPages[pageName];
      int position = m_currentPages.IndexOf (pageToDisplay);
      if (position == -1) {
        m_currentPages.Add (pageToDisplay);
      }
      else {
        m_currentPages = m_currentPages.Take (position + 1).ToList ();
      }

      DisplayLastPage ();
    }

    void OnDataChanged (IRevision revision)
    {
      if (revision != null) {
        // Add information to the revision
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.RevisionDAO.Lock (revision);
            revision.Application = typeof (Program).Assembly.GetName ().Name;
            if (Lemoine.Info.ComputerInfo.GetIPAddresses ().Any ()) {
              revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().First ();
            }

            revision.Comment = "Configurator " + m_currentItem.ID + "." + m_currentItem.SubID + " (" +
              m_currentItem.Title + "), page \"" + GetLastPage ().Title + "\"";
            transaction.Commit ();
          }
        }

        // Show a message so that the user knows something is being updated in the database
        MessageBoxCentered.Show (this, "The database is being updated. This is usually fast but may require some minutes (revision #" + revision.Id + ").",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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

      // Refresh the current page
      DisplayLastPage ();
    }

    void OnProtectAgainstQuit (bool isOn)
    {
      ProtectAgainstQuit = isOn;
    }

    void OnLogAction (string functionName, string dataDescription, string result)
    {
      // Description of the context
      string context = "\n### CONFIGURATOR \"" + m_currentItem.Title + "\" USED ###\n" +
        "  - Page     = \"" + GetLastPage ().Title + "\"\n" +
        "  - Function = " + functionName + "\n" +
        "  - ID       = " + m_currentItem.ID + "\n" +
        "  - Sub ID   = " + m_currentItem.SubID + "\n" +
        "  - Dll path = " + m_currentItem.DllPath + "\n" +
        "  - Ini path = " + m_currentItem.IniPath;

      // Description of the data
      string data = "### DESCRIPTION OF THE DATA ###\n" + dataDescription;

      // Result
      result = "### RESULT ###\n" + result;

      // Send log
      logHistory.Info (context + "\n\n" + data + "\n\n" + result + "\n");

      if ((GetLastPage ().Flags & LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION) == 0) {
        MessageBoxCentered.Show (this.Parent, "The operation completed successfully!", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    /// <summary>
    /// Method called when the right button is clicked
    /// </summary>
    protected override void OnRightButtonClick ()
    {
      // Save the current page
      var lastPage = GetLastPage ();
      lastPage.SavePageInData (m_itemData);

      // Display errors if any. Return if they are not ignored.
      IList<string> errors = lastPage.GetErrorsBeforeValidation (m_itemData);
      if (errors != null && errors.Any ()) {
        if (!DisplayErrors (errors, (lastPage.Flags &
                                    LemSettingsGlobal.PageFlag.IGNORE_IMPOSSIBLE) == 0)) {
          return;
        }
      }

      IList<string> warnings = new List<string> ();
      int revisionId = 0;

      // If an exception occurs, the application continues to run
      MainForm.QuitIfException = false;
      Stopwatch stopWatch = Stopwatch.StartNew ();
      lastPage.Validate (m_itemData, ref warnings, ref revisionId);
      MainForm.QuitIfException = true;

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      if (0 != revisionId) {
        // Add information to the revision
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var revision = ModelDAOHelper.DAOFactory.RevisionDAO.FindById (revisionId);
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            revision.Application = typeof (Program).Assembly.GetName ().Name;
            if (Lemoine.Info.ComputerInfo.GetIPAddresses ().Any ()) {
              revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().First ();
            }

            revision.Comment = "Configurator " + m_currentItem.ID + "." + m_currentItem.SubID + " (" +
              m_currentItem.Title + "), page \"" + lastPage.Title + "\"";
            transaction.Commit ();
          }

          // Show a message so that the user knows something is being updated in the database
          warnings.Add ("The database is being updated. This is usually fast but may require some minutes (revision #" + revision.Id + ").");

          // Display the progression of the revision
          RevisionManager.AddRevision (revision);
        }
      }

      // Process after validation
      lastPage.ProcessAfterValidation (m_itemData);

      // Log what has just happened
      if ((lastPage.Flags & LemSettingsGlobal.PageFlag.DONT_LOG_VALIDATION) == 0) {
        Log (warnings, stopWatch.ElapsedMilliseconds);
      }

      stopWatch.Stop ();

      // Preparation for showing a success popup
      Control parent = this.Parent;
      var successfullPage = lastPage;

      if (warnings.Count == 0) {
        if ((successfullPage.Flags & LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION) == 0) {
          MessageBoxCentered.Show (parent, "The operation completed successfully!", "Information",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      else {
        MessageBoxCentered.Show (parent, String.Join ("\n\n", warnings.ToArray ()), "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // Change the page or end the configurator
      string nextPage = lastPage.GetPageAfterValidation (m_itemData);
      if (String.IsNullOrEmpty (nextPage)) {
        m_currentPages.RemoveAt (m_currentPages.Count () - 1);
      }
      else {
        var pageToDisplay = m_itemPages[nextPage];
        int position = m_currentPages.IndexOf (pageToDisplay);
        if (position == -1) {
          m_currentPages.RemoveAt (m_currentPages.Count () - 1);
        }
        else {
          m_currentPages = m_currentPages.Take (position + 1).ToList ();
        }
      }

      if (m_currentPages.Count > 0) {
        DisplayLastPage ();
      }
      else {
        EndConfigurator ();
        EmitItemFinished ();
      }
    }

    /// <summary>
    /// Method called when the left button is clicked
    /// </summary>
    protected override void OnLeftButtonClick ()
    {
      GetLastPage ().SavePageInData (m_itemData);
      m_currentPages.RemoveAt (m_currentPages.Count () - 1);
      if (m_currentPages.Count > 0) {
        DisplayLastPage ();
      }
      else {
        EndConfigurator ();
        EmitItemFinished ();
      }
    }

    /// <summary>
    /// Method called when the button "home" is clicked
    /// </summary>
    protected override void OnButtonHomeClick ()
    {
      if (ProtectAgainstQuit) {
        if (MessageBoxCentered.Show (this, "Modifications have not been taken into account yet.\n" +
                                    "Do you really want to quit the configurator?", "Question",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
          EndConfigurator ();
          EmitItemFinished ();
        }
      }
      else {
        EndConfigurator ();
        EmitItemFinished ();
      }
    }
    #endregion // Event reactions
  }
}
