// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lemoine.Extensions.Package;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PagePackage.
  /// </summary>
  internal partial class PagePackage : GenericConfiguratorPage, IConfiguratorPage
  {
    readonly ILog log = LogManager.GetLogger (typeof (PagePackage).FullName);

    [Flags]
    enum PluginAction
    {
      CreateNew = 1,
      ReplaceExistingFile = 2,
      UpdatePlugin = 4
    }

    string m_currentPackageName = null;
    bool m_viewMode = false;
    int m_customAction = -1;
    readonly IDictionary<IPackage, PackageCell> m_packageCells = new Dictionary<IPackage, PackageCell> ();
    readonly IList<CheckBox> m_tagButtons = new List<CheckBox> ();

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Package list"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        string text = "All packages installed are listed here. " +
          "For each package you can";
        if (m_viewMode) {
          text += " view the content.";
        }
        else {
          text += ":\n\n" +
            "- view its content (and access custom actions),\n" +
            "- (de)activate its features,\n" +
            "- uninstall.\n\n" +
            "You can also install a new package with the button \"+\" below the list.";
        }

        return text;
      }
    }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get {
        return LemSettingsGlobal.PageFlag.NONE;
      }
    }

    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes
    {
      get {
        IList<Type> types = new List<Type> ();

        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PagePackage ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context)
    {
      // View mode prevent the user to add, configure and remove plugins
      m_viewMode = context.ViewMode;

      if (m_viewMode || (ContextManager.UserCategory != LemSettingsGlobal.UserCategory.SUPER_ADMIN)) {
        // Note: Add is restricted to super admin
        buttonAdd.Hide ();
        baseLayout.RowCount = 1;
        this.Margin = new Padding (0);
        verticalScrollLayout.BorderStyle = BorderStyle.None;
      }
    }

    void TagButtonClick (object sender, EventArgs e)
    {
      var button = (CheckBox)sender;
      var tag = button.Text;
      if (m_tagButtons.All (x => !x.Checked)) {
        foreach (var packageWithCell in m_packageCells) {
          packageWithCell.Value.Visible = true;
        }
        return;
      }
      else { // selection changed
        if (button.Checked) { // New restriction
          foreach (var packageWithCell in m_packageCells.Where (x => x.Value.Visible)) {
            packageWithCell.Value.Visible = packageWithCell.Key.GetTags ().Contains (tag);
          }
        }
        else { // it was unchecked => release some restrictions
          var selectedTags = m_tagButtons.Where (x => x.Checked).Select (x => x.Text); // At least one
          foreach (var packageWithCell in m_packageCells) {
            packageWithCell.Value.Visible = selectedTags.All (x => packageWithCell.Key.GetTags ().Contains (x));
          }
        }
      }
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_currentPackageName = data.Get<string> (ItemPackage.CURRENT_PACKAGE_NAME);
      m_customAction = data.Get<int> (ItemPackage.CUSTOM_ACTION);

      // Load all packages
      verticalScrollLayout.Clear ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var packages = ModelDAOHelper.DAOFactory.PackageDAO.FindAll ().ToList ();
          packages.Sort ((el1, el2) => string.Compare (el1.Name, el2.Name, true, System.Globalization.CultureInfo.InvariantCulture));
          bool alternateRow = false;

          if (m_tagButtons.Any ()) {
            m_tagButtons.Clear ();
            tagsFlowLayoutPanel.Controls.Clear ();
          }
          var tags = packages.SelectMany (x => x.GetTags ()).Distinct ();
          foreach (var tag in tags) {
            var tagButton = new CheckBox ();
            tagButton.Text = tag;
            tagButton.AutoSize = true;
            tagButton.Click += TagButtonClick;
            tagButton.Checked = false;
            tagButton.AutoCheck = true;
            tagButton.Parent = tagsFlowLayoutPanel;
            tagsFlowLayoutPanel.Controls.Add (tagButton);
            m_tagButtons.Add (tagButton);
          }

          if (m_packageCells.Any ()) {
            m_packageCells.Clear ();
          }
          foreach (var package in packages) {
            var packageCell = new PackageCell (package, m_viewMode);
            m_packageCells[package] = packageCell;
            packageCell.Dock = DockStyle.Fill;
            packageCell.ActivationClicked += this.ActivatedClicked;
            packageCell.DeleteClicked += this.DeleteClicked;
            packageCell.InfoClicked += this.InfoClicked;
            if (alternateRow) {
              packageCell.BackColor = LemSettingsGlobal.COLOR_ALTERNATE_ROW;
            }

            alternateRow = !alternateRow;
            verticalScrollLayout.AddControl (packageCell);
          }

          verticalScrollLayout.Visible = (packages.Count > 0);
        }
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemPackage.CURRENT_PACKAGE_NAME, m_currentPackageName);
      data.Store (ItemPackage.CUSTOM_ACTION, m_customAction);
    }
    #endregion // Page methods

    #region Event reactions
    void ButtonAddClick (object sender, EventArgs e)
    {
      IList<string> errors = new List<string> ();

      // Choose an archive
      var dialog = new OpenFileDialog ();
      dialog.InitialDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
      dialog.Filter = "package files (*.json)|*.json";
      if (dialog.ShowDialog () == DialogResult.OK) {
        try {
          PackageFile.InstallOrUpgrade (dialog.FileName, true);
        }
        catch (Exception ex) {
          log.ErrorFormat ("ButtonAddClick: " +
                           "InstallPackage failed with {0} \n {1}",
                           ex, ex.StackTrace);
          errors.Add ("couldn't install the package: " + ex.Message);
        }

        Lemoine.WebClient.Request.NotifyConfigUpdate ();

        // Reload plugins
        ExtensionManager.Reload ();
        EmitDisplayPageEvent ("PagePackage", errors, false);
      }
    }

    bool IsPackageValid (IPackage package)
    {
      var pluginAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.FindByPackage (package);
      foreach (var pluginAssociation in pluginAssociations.Where (a => a.Active)) {
        // Corresponding pluginDll
        var pluginDll = ExtensionManager.GetPlugin (pluginAssociation.Plugin.IdentifyingName);

        // If null the pluginDll has not been loaded => package not valid
        if (pluginDll == null) {
          return false;
        }
      }
      return true;
    }

    void ActivatedClicked (IPackage package, bool isActivated)
    {
      IList<string> errors = new List<string> ();

      if (package == null) {
        errors.Add ("package not found");
        EmitDisplayPageEvent ("PagePackage", errors);
        return;
      }

      m_currentPackageName = package.IdentifyingName;

      if (!isActivated) {
        RemoveConfig (package.IdentifyingName);
      }

      Lemoine.Extensions.Package.PackageMisc.ChangeActivation (package, isActivated);

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      ExtensionManager.Reload ();

      if (isActivated) {
        CheckConfigs ();
      }

      EmitDisplayPageEvent ("PagePackage", errors);
    }

    void CheckConfigs ()
    {
      var extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Business.Config.IInstallationExtension> ();
      foreach (var extension in extensions) {
        try {
          extension.CheckConfig ();
        }
        catch (Exception ex) {
          log.Error ($"CheckConfigs: CheckConfig of {extension} ended in exception", ex);
        }
      }
    }

    void RemoveConfig (string packageIdentifier)
    {
      var extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Business.Config.IInstallationExtension> (packageIdentifier: packageIdentifier);
      foreach (var extension in extensions) {
        try {
          extension.RemoveConfig ();
        }
        catch (Exception ex) {
          log.Error ($"RemoveConfig: RemoveConfig of {extension} with package identifier {packageIdentifier} ended in exception", ex);
        }
      }
    }

    void DeleteClicked (IPackage package)
    {
      if (MessageBoxCentered.Show (this, "Are you sure you want to uninstall this package?",
                                  "Warning", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK) {
        m_currentPackageName = "";
        IList<string> errors = new List<string> ();

        RemoveConfig (package.IdentifyingName);

        // Remove the package, corresponding plugin associations will be automatically removed
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              ModelDAOHelper.DAOFactory.PackageDAO.Lock (package);
              ModelDAOHelper.DAOFactory.PackageDAO.MakeTransient (package);
              transaction.Commit ();
            }
          }
        }
        catch {
          errors.Add ("the package is still registered in the database");
        }

        // Success or error message
        if (errors.Count == 0) {
          EmitLogAction ("DeleteClicked", "removing package " + package.IdentifyingName, "ok");
        }
        else {
          EmitDisplayPageEvent ("PagePackage", errors);
        }

        Lemoine.WebClient.Request.NotifyConfigUpdate ();

        // PagePackage is reloaded even if errors occured
        ExtensionManager.Reload ();
        EmitDisplayPageEvent ("PagePackage", null);
      }
    }

    void InfoClicked (IPackage package)
    {
      m_currentPackageName = package.IdentifyingName;
      EmitDisplayPageEvent ("PagePackageInformation", null);
    }
    #endregion // Event reactions
  }
}
