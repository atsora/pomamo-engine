// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PagePluginList.
  /// </summary>
  internal partial class PagePluginList : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IPluginDll m_currentPlugin = null;
    IPackagePluginAssociation m_association = null;
    bool m_viewMode = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Plugin list"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get
      {
        string text = "Here is the list of all instlaled plugins. For each plugin you can";
        if (m_viewMode) {
          text += " see detailed information.";
        }
        else {
          text += ":\n\n" +
            "- see detailed information,\n" +
            "- delete if no packages use it.";
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
    public PagePluginList ()
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
      verticalScrollLayout.Clear ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          // Find all plugins
          var plugins = ModelDAOHelper.DAOFactory.PluginDAO.FindAll ()
            .OrderBy (o => o.IdentifyingName);

          if (plugins.Any ()) {
            verticalScrollLayout.Show ();

            // Display them
            bool alternateRow = false;
            foreach (var plugin in plugins) {
              // Find all packages using this plugin
              var associations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.FindByPlugin (plugin);

              // Corresponding pluginDll
              var pluginDll = ExtensionManager.GetPlugin (plugin.IdentifyingName);

              // Creation of a cell
              PluginCell pluginCell;
              if (pluginDll == null) {
                pluginCell = new PluginCell (plugin.IdentifyingName, null);
              }
              else {
                pluginCell = new PluginCell (pluginDll, null);
                if (!m_viewMode) {
                  pluginCell.AddDeleteAction (!associations.Any ());
                }
              }

              // Configuration
              pluginCell.Dock = DockStyle.Fill;
              pluginCell.InfoClicked += InfoClicked;
              pluginCell.DeleteClicked += DeleteClicked;
              if (alternateRow) {
                pluginCell.BackColor = LemSettingsGlobal.COLOR_ALTERNATE_ROW;
              }

              alternateRow = !alternateRow;
              verticalScrollLayout.AddControl (pluginCell);
            }
          }
          else {
            verticalScrollLayout.Hide ();
          }
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
    }
    #endregion // Page methods

    #region Event reactions
    void SettingsClicked (IPluginDll plugin)
    {
      m_currentPlugin = plugin;
      EmitDisplayPageEvent ("PagePluginConf", null);
    }

    void DeleteClicked (IPluginDll pluginDll, IPackagePluginAssociation association)
    {
      // No configuration is associated to the plugin, else the Delete button is not enabled

      if (MessageBoxCentered.Show (this, "Are you sure you want to uninstall this plugin? " +
                                  "Data may be lost.",
                                  "Warning", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button2) == DialogResult.OK) {
        m_currentPlugin = null;
        IList<string> errors = new List<string> ();

        // Restore the database
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              pluginDll.Uninstall ();
              transaction.Commit ();
            }
          }
        }
        catch {
          errors.Add ("the restoration of the database failed");
        }

        // Unregister the plugin
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              var plugin = ModelDAOHelper.DAOFactory.PluginDAO.FindByName (pluginDll.IdentifyingName);
              ModelDAOHelper.DAOFactory.PluginDAO.MakeTransient (plugin);
              transaction.Commit ();
            }
          }
        }
        catch {
          errors.Add ("the plugin is still registered in the database");
        }

        // Success or error message
        if (errors.Count == 0) {
          EmitLogAction ("DeleteClicked", "removing plugin " + pluginDll.IdentifyingName, "ok");
        }
        else {
          EmitDisplayPageEvent ("Page1", errors);
        }

        // The current page is reloaded even if errors occured
        ExtensionManager.Reload ();
        EmitDisplayPageEvent ("PagePluginList", null);
      }
    }

    void SwitchActivationClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
    }

    void InfoClicked (IPluginDll plugin, IPackagePluginAssociation association)
    {
      m_currentPlugin = plugin;
      m_association = association;
      EmitDisplayPageEvent ("PagePluginInformation", null);
    }

    static void DeleteDirectory (string target_dir)
    {
      string[] files = Directory.GetFiles (target_dir);
      string[] dirs = Directory.GetDirectories (target_dir);

      foreach (string file in files) {
        File.SetAttributes (file, FileAttributes.Normal);
        File.Delete (file);
      }

      foreach (string dir in dirs) {
        DeleteDirectory (dir);
      }

      Directory.Delete (target_dir, false);
    }
    #endregion // Event reactions
  }
}
