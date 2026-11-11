// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PackageCell.
  /// </summary>
  public partial class PackageCell : UserControl
  {
    readonly IPackage m_package = null;

    static readonly ILog log = LogManager.GetLogger(typeof (PackageCell).FullName);

    #region Events
    /// <summary>
    /// Emitted when the button "info" is clicked
    /// The first argument is the package
    /// </summary>
    public event Action<IPackage> InfoClicked;
    
    /// <summary>
    /// Emitted when the button "settings" is clicked
    /// The first argument is the package
    /// The second argument is the new activation state
    /// </summary>
    public event Action<IPackage, bool> ActivationClicked;
    
    /// <summary>
    /// Emitted when the button "delete" is clicked
    /// The first argument is the package
    /// </summary>
    public event Action<IPackage> DeleteClicked;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="package"></param>
    /// <param name="viewMode"></param>
    public PackageCell(IPackage package, bool viewMode)
    {
      Debug.Assert (null != package);
      
      m_package = package;
      InitializeComponent();
      
      labelName.Text = m_package.Name;
      if (!m_package.Activated) {
        labelName.ForeColor = SystemColors.GrayText;
        labelName.Text += " (deactivated)";
      }
      
      // Problem?
      if (HasPackageProblem()) {
        var toolTip = new ToolTip();
        toolTip.SetToolTip(pictureWarning, "Something is wrong with this package. " +
                           "See the content for more details.");
      }
      else {
        baseLayout.ColumnStyles[1].Width = 0;
        pictureWarning.Hide();
      }
      
      // Visibility of the buttons
      if (viewMode || m_package.Activated) {
        // Hide activate
        baseLayout.ColumnStyles[2].Width = 0;
        buttonActivate.Hide ();
      }
      if (viewMode || !m_package.Activated) {
        // Hide deactivate
        baseLayout.ColumnStyles[3].Width = 0;
        buttonDeactivate.Hide ();
      }
      if (viewMode || Lemoine.Settings.ContextManager.UserCategory != Lemoine.Settings.LemSettingsGlobal.UserCategory.SUPER_ADMIN) {
        // Hide delete
        baseLayout.ColumnStyles[4].Width = 0;
        buttonDelete.Hide ();
      }
    }
    #endregion // Constructors

    #region Event reactions
    void buttonActivate_Click (object sender, EventArgs e)
    {
      ActivationClicked (m_package, true);
    }

    void buttonDeactivate_Click (object sender, EventArgs e)
    {
      ActivationClicked (m_package, false);
    }

    void buttonDelete_Click (object sender, EventArgs e)
    {
      DeleteClicked (m_package);
    }

    void buttonView_Click (object sender, EventArgs e)
    {
      InfoClicked (m_package);
    }
    #endregion // Event reactions

    bool HasPackageProblem()
    {
      Debug.Assert (null != m_package);
      
      if (log.IsDebugEnabled) {
        log.Debug ($"HasPackageProblem: package {m_package.IdentifyingName}");
      }
      
      var pluginAssociations = ModelDAOHelper.DAOFactory.PackagePluginAssociationDAO
        .FindByPackage(m_package);
      
      foreach (var pluginAssociation in pluginAssociations.Where (a => a.Active)) {
        Debug.Assert (null != pluginAssociation.Plugin);
        
        if (log.IsDebugEnabled) {
          log.Debug ($"HasPackageProblem: check plugin {pluginAssociation.Plugin.IdentifyingName} association {pluginAssociation.Name}");
        }
        
        // Corresponding pluginDll
        var pluginDll = ExtensionManager.GetPlugin(pluginAssociation.Plugin.IdentifyingName);
        
        // No corresponding dll?
        if (null == pluginDll) {
          log.Error ($"HasPackageProblems: plugin {pluginAssociation.Plugin.IdentifyingName} association {pluginAssociation.Name} could not be loaded");
          return true;
        }
        
        // Configuration problem?
        if (null != pluginAssociation.Parameters) {
          IEnumerable<string> errors = pluginDll.GetConfigurationErrors (pluginAssociation.Parameters);
          if ( (null != errors) && errors.Any ()) {
            log.Error ($"HasPackageProblems: {errors.Count ()} configuration errors in plugin {pluginAssociation.Plugin.IdentifyingName} association {pluginAssociation.Name}");
            return true;
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"HasPackageProblems: plugin {pluginAssociation.Plugin.IdentifyingName} association {pluginAssociation.Name} is ok");
        }
      }
      
      return false;
    }
  }
}
