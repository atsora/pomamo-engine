// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Settings;
using Lemoine.ModelDAO;
using System.Linq;
using System.Diagnostics;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// This page displays information about Plugins
  /// It is shared between ItemPackage and ItemPlugin
  /// </summary>
  public partial class PagePluginInformation : GenericConfiguratorPage, IConfiguratorPage
  {
    ILog log = LogManager.GetLogger<PagePluginInformation> ();

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Plugin information"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "This page contains various information about a plugin:\n\n" +
          "- the name,\n" +
          "- identifying name and version,\n" +
          "- description,\n" +
          "- the path of the dll containing the plugin,\n" +
          "- the different packages using the plugin.";
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
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PagePluginInformation ()
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
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      EmitProtectAgainstQuit (false);
      IPluginDll pluginDll = data.Get<IPluginDll> (ItemPackage.CURRENT_PLUGIN);
      if (pluginDll is null) {
        log.Error ($"LoadPageFromData: plugin is null");
        labelName.Text = "Not loaded";
        labelIdentifier.Text = "";
        labelDescription.Text = "Invalid plugin";
        listPackages.ClearItems ();
        return;
      }

      labelName.Text = pluginDll.Name;
      labelIdentifier.Text = $"{pluginDll.IdentifyingName} (version {pluginDll.Version})";
      labelDescription.Text = pluginDll.Description;
      labelPath.Text = (pluginDll.Context != null ? pluginDll.Context.DllPath : "-");

      // Packages associated to this plugin
      listPackages.ClearItems ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var plugin = ModelDAOHelper.DAOFactory.PluginDAO.FindByName (pluginDll.IdentifyingName);
          if (plugin is null) {
            log.Error ($"PagePluginInformation: no plugin with identifier {pluginDll.IdentifyingName}");
          }
          else { // plugin != null
            var associations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.FindByPlugin (plugin);
            foreach (var association in associations) {
              // Attributes
              var attributes = new List<string> ();
              Debug.Assert (null != association.Package);
              attributes.Add (association.Package.Activated ? "active" : "inactive");
              IEnumerable<string> confErrors = pluginDll.GetConfigurationErrors (association.Parameters);
              bool confError = (null != confErrors) && confErrors.Any ();
              if (confError) {
                attributes.Add ("conf. error");
              }
              listPackages.AddItem (association.Package.Name + " (" +
                                   String.Join (", ", attributes.ToArray ()) + ")",
                                   null, association.Package.Name,
                                   confError ? LemSettingsGlobal.COLOR_ERROR : SystemColors.ControlText,
                                   false, confError);
            }
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
      // Nothing
    }
    #endregion // Page methods
  }
}
