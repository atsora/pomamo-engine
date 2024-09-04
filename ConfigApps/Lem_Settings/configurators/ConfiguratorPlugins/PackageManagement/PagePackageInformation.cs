// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Lemoine.Core.Log;
using Lemoine.Settings;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PagePackageInformation.
  /// </summary>
  internal partial class PagePackageInformation : GenericConfiguratorPage, IConfiguratorPage
  {
    readonly ILog log = LogManager.GetLogger<PagePackageInformation> ();

    #region Members
    IPluginDll m_currentPlugin = null;
    IPackagePluginAssociation m_association = null;
    bool m_viewMode = false;
    int m_customAction = -1;
    string m_packageName = "";
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Package information"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get
      {
        string text = "Package properties can be found here:\n" +
          "- the displayed name,\n" +
          "- the identifying name and version,\n" +
          "- the description.\n\n" +
          "With the arrows you can display all the content of the package: the different " +
          "plugins, files and Lemoine Settings items if any.\n\n" +
          "For each plugin you can";
        if (m_viewMode) {
          text += " see detailed information.";
        }
        else {
          text += ":\n" +
            "- see detailed information,\n" +
            "- edit the parameters if any,\n" +
            "- access custom actions if any.";
        }

        return text;
      }
    }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get
      {
        return LemSettingsGlobal.PageFlag.NONE;
      }
    }

    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes
    {
      get
      {
        IList<Type> types = new List<Type> ();

        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PagePackageInformation ()
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
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_packageName = data.Get<string> (ItemPackage.CURRENT_PACKAGE_NAME);
      listPlugins.Clear ();
      listFiles.Clear ();
      listItems.Clear ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          IPackage package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (m_packageName);
          if (package == null) {
            throw new Exception ("Package not found");
          }

          // Fill package information
          labelDisplayedName.Text = package.Name;
          labelIdentifyingName.Text = String.Format ("{0} (version {1})",
                                                    package.IdentifyingName,
                                                    package.NumVersion);
          tagsLabel.Text = string.Join (", ", package.GetTags ());
          labelDescription.Text = package.Description;

          // Find all plugins
          FillPluginList (package);
        }
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemPackage.CURRENT_PLUGIN, m_currentPlugin);
      data.Store (ItemPackage.CURRENT_ASSOCIATION, m_association);
      data.Store (ItemPackage.CUSTOM_ACTION, m_customAction);
    }

    void FillPluginList (IPackage package)
    {
      var pluginAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
        .FindByPackage (package).ToList ()
        .OrderBy (o => o.Plugin.IdentifyingName);

      bool alternateRow = false;
      foreach (var pluginAssociation in pluginAssociations) {
        // Corresponding pluginDll
        var pluginDll = ExtensionManager.GetPlugin (pluginAssociation.Plugin.IdentifyingName);

        // Creation of a cell
        PluginCell pluginCell;
        if (pluginDll == null) {
          pluginCell = new PluginCell (pluginAssociation.Plugin.IdentifyingName, pluginAssociation);
        }
        else { // null != pluginDll
          pluginCell = new PluginCell (pluginDll, pluginAssociation);
          if (!m_viewMode) {
            pluginCell.AddActivateDeactivate ();
            pluginCell.AddConfigurationAndCustomActions ();
          }
        }

        // Configuration
        pluginCell.Dock = DockStyle.Fill;
        pluginCell.SettingsClicked += SettingsClicked;
        pluginCell.InfoClicked += InfoClicked;
        pluginCell.CustomActionClicked += CustomActionClicked;
        pluginCell.ActivateClicked += ActivateClicked;
        pluginCell.DeactivateClicked += DeactivateClicked;
        pluginCell.DeleteClicked += DeleteClicked;
        pluginCell.DuplicateClicked += DuplicateClicked;
        if (null != pluginDll) {
          pluginCell.AddDuplicateAction (pluginDll.MultipleConfigurations);
        }
        if (1 < pluginAssociations
            .Count (association => association.Package.IdentifyingName.Equals (pluginAssociation.Package.IdentifyingName))) {
          pluginCell.AddDeleteAction (pluginAssociation.Custom);
        }
        pluginCell.AddViewAction (pluginDll != null);
        if (alternateRow) {
          pluginCell.BackColor = LemSettingsGlobal.COLOR_ALTERNATE_ROW;
        }

        alternateRow = !alternateRow;
        listPlugins.AddControl (pluginCell);
      }

      listPlugins.Visible = pluginAssociations.Any ();
    }
    #endregion // Page methods

    #region Event reactions
    void SettingsClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
      m_currentPlugin = plugin;
      m_association = association;
      EmitDisplayPageEvent ("PagePluginConf", null);
    }

    void InfoClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
      m_currentPlugin = plugin;
      m_association = association;
      EmitDisplayPageEvent ("PagePluginInformation", null);
    }

    void CustomActionClicked (IPluginDll plugin, IPackagePluginAssociation association, int numAction)
    {
      if (numAction >= 0 && numAction < plugin.CustomActionControls.Count ()) {
        m_currentPlugin = plugin;
        m_association = association;
        m_customAction = numAction;
        EmitDisplayPageEvent ("PagePluginAction", null);
      }
    }

    void ActivateClicked (IPackagePluginAssociation association)
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .Lock (association);
            association.Active = true;
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .MakePersistent (association);
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("the package/plugin association could not be updated", ex);
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      Lemoine.Extensions.ExtensionManager.Reload ();
      EmitDisplayPageEvent ("PagePackageInformation", null);
    }

    void DeactivateClicked (IPackagePluginAssociation association)
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .Lock (association);
            association.Active = false;
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .MakePersistent (association);
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("the package/plugin association could not be updated", ex);
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      Lemoine.Extensions.ExtensionManager.Reload ();
      EmitDisplayPageEvent ("PagePackageInformation", null);
    }

    void DeleteClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
      IList<string> errors = new List<string> ();

      // Remove the association
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .Lock (association);
            ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
              .MakeTransient (association);
            transaction.Commit ();
          }
        }
      }
      catch {
        errors.Add ("the plugin is still registered in the database");
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      // Success or error message
      if (errors.Any ()) {
        EmitDisplayPageEvent ("Page1", errors);
      }
      else {
        EmitLogAction ("DeleteClicked", "removing association " + association.Name, "ok");
      }

      EmitDisplayPageEvent ("PagePackageInformation", null);
    }

    void DuplicateClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
      IPackagePluginAssociation newAssociation = ModelDAOHelper.ModelFactory
        .CreatePackagePluginAssociation (association.Package,
                                         association.Plugin,
                                         association.Name + " (copy)");
      newAssociation.Parameters = association.Parameters;
      newAssociation.Custom = true;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Settings.PagePackageInformation.Duplicate")) {
          ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (newAssociation);
          transaction.Commit ();
        }
      }
      plugin.AddInstance (newAssociation);
      m_currentPlugin = plugin;
      m_association = newAssociation;

      EmitDisplayPageEvent ("PagePluginConf", null);
    }

    void ButtonLeftClick (object sender, EventArgs e)
    {
      if (stackedWidget.SelectedTab == tabPage1) {
        stackedWidget.SelectedTab = tabPage3;
        labelStackedWidgetTitle.Text = "Lemoine Settings items";
      }
      else if (stackedWidget.SelectedTab == tabPage2) {
        stackedWidget.SelectedTab = tabPage1;
        labelStackedWidgetTitle.Text = "Plugins";
      }
      else if (stackedWidget.SelectedTab == tabPage3) {
        stackedWidget.SelectedTab = tabPage2;
        labelStackedWidgetTitle.Text = "Files";
      }
    }

    void ButtonRightClick (object sender, EventArgs e)
    {
      if (stackedWidget.SelectedTab == tabPage1) {
        stackedWidget.SelectedTab = tabPage2;
        labelStackedWidgetTitle.Text = "Files";
      }
      else if (stackedWidget.SelectedTab == tabPage2) {
        stackedWidget.SelectedTab = tabPage3;
        labelStackedWidgetTitle.Text = "Lemoine Settings items";
      }
      else if (stackedWidget.SelectedTab == tabPage3) {
        stackedWidget.SelectedTab = tabPage1;
        labelStackedWidgetTitle.Text = "Plugins";
      }
    }
    #endregion // Event reactions
  }
}
