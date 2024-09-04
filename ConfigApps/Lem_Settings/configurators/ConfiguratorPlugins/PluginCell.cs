// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PluginCell.
  /// </summary>
  public partial class PluginCell : UserControl
  {
    #region Members
    readonly ContextMenuStrip m_menu = new ContextMenuStrip ();
    readonly IPluginDll m_plugin = null;
    readonly IPackagePluginAssociation m_association = null;
    #endregion // Members

    #region Events
    /// <summary>
    /// Emitted when the button "info" is clicked
    /// The first argument is the plugin dll
    /// The second argument is the plugin/package association
    /// </summary>
    public event Action<IPluginDll, IPackagePluginAssociation> InfoClicked;

    /// <summary>
    /// Emitted when the button "settings" is clicked
    /// The first argument is the plugin dll
    /// The second argument is the plugin/package association
    /// </summary>
    public event Action<IPluginDll, IPackagePluginAssociation> SettingsClicked;

    /// <summary>
    /// Emitted when a custom action has been clicked
    /// The first argument is the plugin dll
    /// The second argument is the plugin/package association
    /// The third argument is the number of the custom action
    /// </summary>
    public event Action<IPluginDll, IPackagePluginAssociation, int> CustomActionClicked;

    /// <summary>
    /// Emitted when the button "activate" is clicked
    /// The argument is the plugin/package association
    /// </summary>
    public event Action<IPackagePluginAssociation> ActivateClicked;

    /// <summary>
    /// Emitted when the button "deactivate" is clicked
    /// The argument is the plugin/package association
    /// </summary>
    public event Action<IPackagePluginAssociation> DeactivateClicked;

    /// <summary>
    /// Emitted when the button "delete" has been clicked
    /// The first argument is the plugin dll
    /// The second argument is the plugin/package association
    /// </summary>
    public event Action<IPluginDll, IPackagePluginAssociation> DeleteClicked;

    /// <summary>
    /// Emitted when the button "duplicate" has been clicked
    /// The first argument is the plugin dll
    /// The second argument is the plugin/package association
    /// </summary>
    public event Action<IPluginDll, IPackagePluginAssociation> DuplicateClicked;
    #endregion // Events

    static readonly ILog log = LogManager.GetLogger (typeof (PluginCell).FullName);

    #region Constructors
    /// <summary>
    /// PluginCell for a valid plugin (the dll has been loaded)
    /// </summary>
    /// <param name="pluginDll">plugin to display</param>
    /// <param name="association">package/plugin association</param>
    public PluginCell (IPluginDll pluginDll, IPackagePluginAssociation association)
    {
      if (pluginDll is null) {
        log.Error ($"PluginCell: pluginDll is null");
      }

      m_plugin = pluginDll;
      m_association = association;
      InitializeComponent ();

      // Hide all buttons at first
      HideButtons ();

      // Configuration error?
      if (null != association) {
        var confErrors = pluginDll.GetConfigurationErrors (association.Parameters);
        if ((null != confErrors) && (confErrors.Any ())) {
          // Warning: bad configuration
          string text = (confErrors.Count () > 1 ?
                         "Configuration errors have been found:\n- " :
                         "A configuration error has been found:\n- ") +
            string.Join (";\n- ", confErrors.ToArray ()) + ".";
          ShowWarning (text);
        }
      }
      else { // null == association
        var errors = pluginDll.GetConfigurationErrors ();
        if ((null != errors) && (errors.Any ())) {
          ShowWarning ("the plugin has invalid configurations for at least one package");
        }
      }

      labelName.Text = m_plugin.Name;
      if (null != association) {
        labelName.Text += " / " + association.Name;
      }
      if ((null == association) || !association.Package.Activated) {
        if (!pluginDll.Instances.Any (instance => instance.InstanceActive)) {
          labelName.ForeColor = SystemColors.GrayText;
          labelName.Text += " (inactive)";
        }
      }
    }

    /// <summary>
    /// PluginCell for an invalid plugin (dll not loaded for various reasons)
    /// </summary>
    /// <param name="identifyingName"></param>
    /// <param name="association"></param>
    public PluginCell (string identifyingName, IPackagePluginAssociation association)
    {
      m_association = association;
      InitializeComponent ();

      // Hide all buttons at first
      HideButtons ();

      // Try to find an error and a dll path associated to the identifying name
      {
        var loadErrorPlugins = ExtensionManager.LoadErrorPlugins;
        var pluginDllLoader = loadErrorPlugins.FirstOrDefault (p => string.Equals (p.PluginName,
          identifyingName, StringComparison.InvariantCulture));
        if (pluginDllLoader != null) {
          string dllPath = (pluginDllLoader.Plugin != null && pluginDllLoader.Plugin.Context != null) ?
            pluginDllLoader.Plugin.Context.DllPath : "";

          string warningText = "status: " + pluginDllLoader.Status.ToString ();
          if (!string.IsNullOrEmpty(dllPath)) {
            warningText += "\n" + "dll path: " + dllPath;
          }
          // TODO: more information could be returned, but the details are in the logs

          ShowWarning (warningText);
        }
      }

      if (m_association == null || !m_association.Active) {
        labelName.Text = string.Format ("Inactive configuration {0} / {1}", identifyingName, m_association != null ? m_association.Name : "-");
        labelName.ForeColor = SystemColors.GrayText;

        AddActivateDeactivate ();
      }
      else { // Active
        labelName.Text = "Invalid plugin \"" + identifyingName + "\"";
        if (null != association) {
          labelName.Text += " / " + association.Name;
        }
      }
    }
    #endregion // Constructors

    #region Methods
    void ShowWarning (string text)
    {
      var toolTip = new ToolTip ();
      toolTip.SetToolTip (pictureWarning, text);
      baseLayout.ColumnStyles[1].Width = 27;
      pictureWarning.Show ();
    }

    /// <summary>
    /// Add the button Activate or Deactivate
    /// </summary>
    public void AddActivateDeactivate ()
    {
      if (null != m_association) {
        if (m_association.Active) {
          // Show button "Deactivate"
          baseLayout.ColumnStyles[3].Width = 29;
          buttonDeactivate.Show ();
        }
        else {
          // Show button "Activate"
          baseLayout.ColumnStyles[2].Width = 29;
          buttonActivate.Show ();
        }
      }
    }

    /// <summary>
    /// Display the item "Configure" and custom actions in the menu
    /// </summary>
    public void AddConfigurationAndCustomActions ()
    {
      // Configuration
      if (m_plugin.ConfigurationControl != null) {
        baseLayout.ColumnStyles[4].Width = 29;
        buttonConfigure.Show ();
      }

      // Custom actions
      if (m_plugin.CustomActionControls != null && m_plugin.CustomActionControls.Any ()) {
        // Fill the menu
        int num = 0;
        foreach (var customAction in m_plugin.CustomActionControls) {
          ToolStripItem itemTmp = m_menu.Items.Add (customAction.Title);
          itemTmp.Tag = num++;
        }

        // Show the menu
        m_menu.ItemClicked += MenuClicked;
        baseLayout.ColumnStyles[8].Width = 32;
        buttonMenu.Show ();
      }
    }

    /// <summary>
    /// Add the action View, enabled or not
    /// </summary>
    /// <param name="enabled"></param>
    public void AddViewAction (bool enabled)
    {
      baseLayout.ColumnStyles[5].Width = 29;
      buttonView.Show ();
      buttonView.Enabled = enabled;
    }

    /// <summary>
    /// Add the action duplicate, enabled or not
    /// </summary>
    /// <param name="enabled"></param>
    public void AddDuplicateAction (bool enabled)
    {
      baseLayout.ColumnStyles[5].Width = 29;
      buttonDuplicate.Show ();
      buttonDuplicate.Enabled = enabled;
    }

    /// <summary>
    /// Add the action delete, enabled or not
    /// </summary>
    public void AddDeleteAction (bool enabled)
    {
      baseLayout.ColumnStyles[6].Width = 29;
      buttonDelete.Show ();
      buttonDelete.Enabled = enabled;
    }

    void HideButtons ()
    {
      baseLayout.ColumnStyles[1].Width = 0;
      pictureWarning.Hide ();
      baseLayout.ColumnStyles[2].Width = 0;
      buttonActivate.Hide ();
      baseLayout.ColumnStyles[3].Width = 0;
      buttonDeactivate.Hide ();
      baseLayout.ColumnStyles[4].Width = 0;
      buttonConfigure.Hide ();
      baseLayout.ColumnStyles[5].Width = 0;
      buttonDuplicate.Hide ();
      baseLayout.ColumnStyles[6].Width = 0;
      buttonDelete.Hide ();
      baseLayout.ColumnStyles[7].Width = 0;
      buttonView.Hide ();
      baseLayout.ColumnStyles[8].Width = 0;
      buttonMenu.Hide ();
    }
    #endregion // Methods

    #region Event reactions
    void ButtonMenuClick (object sender, EventArgs e)
    {
      m_menu.Show (buttonMenu, new Point (0, buttonMenu.Height));
    }

    void MenuClicked (Object sender, ToolStripItemClickedEventArgs e)
    {
      m_menu.Hide ();
      if (e.ClickedItem.Tag == null) {
        return;
      }

      if (e.ClickedItem.Tag is int) {
        CustomActionClicked (m_plugin, m_association, (int)e.ClickedItem.Tag);
      }
    }

    void buttonActivate_Click (object sender, EventArgs e)
    {
      ActivateClicked (m_association);
    }

    void buttonDeactivate_Click (object sender, EventArgs e)
    {
      DeactivateClicked (m_association);
    }

    void buttonConfigure_Click (object sender, EventArgs e)
    {
      SettingsClicked (m_plugin, m_association);
    }

    void buttonDuplicate_Click (object sender, EventArgs e)
    {
      DuplicateClicked (m_plugin, m_association);
    }

    void buttonDelete_Click (object sender, EventArgs e)
    {
      DeleteClicked (m_plugin, m_association);
    }

    void buttonView_Click (object sender, EventArgs e)
    {
      InfoClicked (m_plugin, m_association);
    }
    #endregion // Event reactions
  }
}
