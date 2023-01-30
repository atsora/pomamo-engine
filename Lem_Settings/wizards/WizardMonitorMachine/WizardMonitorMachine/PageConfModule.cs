// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageConfModule.
  /// </summary>
  internal partial class PageConfModule : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Configuration of the data acquisition"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "You can choose in this page all parameters " +
"required to connect the new machine.\n\n" +
"The old configuration, if any, is displayed on the bottom of the page.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageConfModule ()
    {
      InitializeComponent ();
      textOldParameters.ReadOnly = true;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      var newCncDocument = xmlData[data.Get<string> (Item.CONFIG_FILE)];
      var oldCncDocument = data.Get<CncDocument> (Item.OLD_XML_DATA_FOR_PARAM);

      // Content of the new configuration
      cncConfiguratorControl.Init (newCncDocument,
                                  ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN,
                                  false);

      if (oldCncDocument == null) {
        // Either we display parameters as stored in the database if an old acquisition exists
        // Or nothing is displayed (new acquisition)
        LoadWithNoOldXml (newCncDocument, data.Get<IMachine> (Item.MACHINE));
      }
      else if (oldCncDocument.FileName == newCncDocument.FileName) {
        // The same xml file was used, we mix the two sets of parameters (new / old)
        LoadWithSameOldXml (newCncDocument, oldCncDocument);
      }
      else {
        // Another xml file is used, old parameters are displayed below the new ones
        LoadWithDifferentOldXml (newCncDocument, oldCncDocument);
      }
    }

    void LoadWithNoOldXml (CncDocument newCncDocument, IMachine machine)
    {
      // Name of the new configuration
      labelNewConfiguration.Text = "New configuration (" + newCncDocument.FileName + ")";

      // Try to find an old acquisition and display the old parameters as is
      string parameters = "";
      string oldConf = "";
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          ModelDAOHelper.DAOFactory.MachineDAO.Lock (machine);
          var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByMachine (machine);
          if (moma != null) {
            foreach (var mamo in moma.MachineModules) {
              if (mamo.CncAcquisition != null) {
                parameters = mamo.CncAcquisition.ConfigParameters;
                oldConf = mamo.CncAcquisition.ConfigFile;
                break;
              }
            }
          }
        }
      }

      if (string.IsNullOrEmpty (parameters)) {
        // Display nothing
        baseLayout.RowStyles[2].Height = 0;
        baseLayout.RowStyles[3].Height = 0;
        baseLayout.RowStyles[4].Height = 0;
        labelOldConfiguration.Hide ();
        cncConfiguratorControlOld.Hide ();
        textOldParameters.Hide ();
      }
      else {
        // Display the parameters
        var separator = parameters[0];
        parameters = parameters.Remove (0, 1);
        var parametersArray = parameters.Split (separator);

        baseLayout.RowStyles[2].Height = 18;
        baseLayout.RowStyles[3].Height = 0;
        baseLayout.RowStyles[4].Height = 22;
        labelOldConfiguration.Show ();
        cncConfiguratorControlOld.Hide ();
        textOldParameters.Show ();
        labelOldConfiguration.Text = "Old configuration (" + oldConf + ")";
        textOldParameters.Text = string.Join (" - ", parametersArray);
      }
    }

    void LoadWithSameOldXml (CncDocument newCncDocument, CncDocument oldCncDocument)
    {
      // Mix the old configurations with the new ones
      labelNewConfiguration.Text = "Change configuration (" + newCncDocument.FileName + ")";
      cncConfiguratorControl.ShowOld (oldCncDocument, ContextManager.UserCategory ==
                                     LemSettingsGlobal.UserCategory.SUPER_ADMIN);

      // Hide the lower part
      baseLayout.RowStyles[2].Height = 0;
      baseLayout.RowStyles[3].Height = 0;
      baseLayout.RowStyles[4].Height = 0;
      labelOldConfiguration.Hide ();
      cncConfiguratorControlOld.Hide ();
      textOldParameters.Hide ();
    }

    void LoadWithDifferentOldXml (CncDocument newCncDocument, CncDocument oldCncDocument)
    {
      // Show old configuration if the description matches
      labelNewConfiguration.Text = "New configuration (" + newCncDocument.FileName + ")";
      cncConfiguratorControl.ShowOld (oldCncDocument, ContextManager.UserCategory ==
                                     LemSettingsGlobal.UserCategory.SUPER_ADMIN);

      // Display the old configurations below the new ones
      baseLayout.RowStyles[2].Height = 18;
      labelOldConfiguration.Show ();
      labelOldConfiguration.Text = "Old configuration (" + oldCncDocument.FileName + ")";

      // Content of the old configuration
      baseLayout.RowStyles[3].Height = 40;
      baseLayout.RowStyles[4].Height = 0;
      cncConfiguratorControlOld.Show ();
      cncConfiguratorControlOld.Init (oldCncDocument,
                                     ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN,
                                     true);
      textOldParameters.Hide ();
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      // Nothing to do for the parameters: already stored
    }

    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext (ItemData data)
    {
      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      CncDocument cncDoc = xmlData[data.Get<string> (Item.CONFIG_FILE)];
      return cncDoc.GetErrors ();
    }

    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings (ItemData data)
    {
      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      CncDocument cncDoc = xmlData[data.Get<string> (Item.CONFIG_FILE)];
      return cncDoc.GetWarnings ();
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data) { return "PageStamping"; }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      CncDocument cncDocument = xmlData[data.Get<string> (Item.CONFIG_FILE)];
      return cncDocument.GetSummary (ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN);
    }
    #endregion // Page methods
  }
}
