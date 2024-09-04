// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Plugin;
using Lemoine.ConfigControls;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PagePluginConf.
  /// </summary>
  public partial class PagePluginConf : GenericConfiguratorPage, IConfiguratorPage
  {
    ILog log = LogManager.GetLogger (typeof (PagePluginConf).FullName);

    #region Members
    IPluginConfigurationControl m_configurationControl = null;
    string m_pluginName = "";
    IPackagePluginAssociation m_association;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title
    {
      get {
        string title = "Plugin configuration";
        if (!String.IsNullOrEmpty (m_pluginName)) {
          title += " (" + m_pluginName + ")";
        }

        return title;
      }
    }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return (m_configurationControl != null) ? m_configurationControl.Help :
          "Configure here a plugin for a specific package (a plugin can indeed be " +
          "configured differently according to the need of different packages).\n\n" +
          "The interface depends on the plugin and can be anything.\n\n" +
          "The help of this page is usually replaced by the help of the interface displayed.";
      }
    }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PagePluginConf ()
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
      EmitProtectAgainstQuit (true);
      IPluginDll pluginDll = data.Get<IPluginDll> (ItemPackage.CURRENT_PLUGIN);
      m_association = data.Get<IPackagePluginAssociation> (ItemPackage.CURRENT_ASSOCIATION);
      m_pluginName = pluginDll.Name;

      Debug.Assert (null != m_association);
      if (null != m_association.Name) {
        nameTextBox.Text = m_association.Name.Trim ();
      }
      else {
        nameTextBox.Text = "";
      }

      // Clear the central area
      panelParam.Controls.Clear ();

      m_configurationControl = pluginDll.ConfigurationControl;

      // Add parameters if any
      if (m_configurationControl == null) {
        var label = new Label ();
        label.Text = "no parameters";
        label.Font = new Font (label.Font, FontStyle.Bold | FontStyle.Italic);
        label.ForeColor = SystemColors.ControlDark;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Dock = DockStyle.Fill;
        panelParam.Controls.Add (label);
      }
      else {
        if (m_configurationControl is Lemoine.Extensions.Configuration.GuiBuilder.IConfigurationGuiBuilder) {
          var pluginConfig = new PluginConfig ();
          var configurationGuiBuilder =
            (Lemoine.Extensions.Configuration.GuiBuilder.IConfigurationGuiBuilder)m_configurationControl;
          configurationGuiBuilder.SetConfigControl (pluginConfig);
          pluginConfig.Dock = DockStyle.Fill;
          m_configurationControl.InitializeInterface ();
          panelParam.Controls.Add ((Control)pluginConfig);
        }
        else if (m_configurationControl is UserControl) {
          ((UserControl)m_configurationControl).Dock = DockStyle.Fill;
          panelParam.Controls.Add ((Control)m_configurationControl);
          m_configurationControl.InitializeInterface ();
        }
        else {
          log.Error ("LoadPageFromData: configuration control is neither a UserControl nor a ConfigurationGuiBuilder");
          m_configurationControl.InitializeInterface ();
        }

        foreach (var instance in pluginDll.Instances) {
          if (instance.PackageIdentifyingName.Equals (data.Get<string> (ItemPackage.CURRENT_PACKAGE_NAME))
              && object.Equals (instance.InstanceName, m_association.Name)) {
            try {
              m_configurationControl.LoadProperties (instance.InstanceParameters);
            }
            catch (Exception ex) {
              log.ErrorFormat ("LoadPageFromData: " +
                               "exception while loading the properties: {0}",
                               ex);
            }
          }
        }
      }

      // Fill header
      EmitSetTitle (Title + " (" + pluginDll.Name + ")");
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      // Parameters are stored in the validation method, if there is no error
    }

    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation (ItemData data)
    {
      IList<string> errors = new List<string> ();

      if (m_configurationControl != null) {
        IEnumerable<string> listTmp = m_configurationControl.GetErrors ();
        if (listTmp != null) {
          foreach (string error in listTmp) {
            errors.Add (error);
          }
        }
      }

      return errors;
    }

    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public override void Validate (ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      // Store the configuration in the plugin
      IPluginDll pluginDll = data.Get<IPluginDll> (ItemPackage.CURRENT_PLUGIN);
      if (m_configurationControl != null) {
        string configuration = m_configurationControl.GetProperties ();
        Debug.Assert (null != m_association);
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              IPackagePluginAssociation association = m_association;
              string oldName = "";
              if (null != m_association.Name) {
                oldName = m_association.Name;
              }
              if (!string.Equals (oldName.Trim (),
                                  nameTextBox.Text.Trim (),
                                  StringComparison.InvariantCultureIgnoreCase)) {
                // Name changed: Delete / Create
                association = ModelDAOHelper.ModelFactory
                  .CreatePackagePluginAssociation (m_association.Package,
                                                   m_association.Plugin,
                                                   nameTextBox.Text.Trim ());
                if (0 != m_association.Id) {
                  ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakeTransient (m_association);
                }
              }
              if (0 != association.Id) {
                ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.Lock (association);
              }
              if (!string.Equals (configuration, association.Parameters)) {
                association.Custom = true;
              }
              association.Parameters = configuration;
              ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO.MakePersistent (association);
              m_association = association;

              transaction.Commit ();
            }
          }
        }
        catch (Exception ex) {
          warnings.Add ("The following error has been encountered:\n\"" + ex.Message + "\".");
        }

        Lemoine.WebClient.Request.NotifyConfigUpdate ();

        // Reload items
        ExtensionManager.Reload ();
      }
    }
    #endregion // Page methods
  }
}
