// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.Extensions.Plugin;
using Lemoine.Core.Log;
using Lemoine.ConfigControls;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of PagePluginAction.
  /// </summary>
  public partial class PagePluginAction : GenericConfiguratorPage, IConfiguratorPage
  {
    ILog log = LogManager.GetLogger (typeof (PagePluginAction));

    #region Members
    IPluginCustomActionControl m_customActionControl = null;
    string m_pluginName = "";
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title {
      get {
        string title = (m_customActionControl != null) ? m_customActionControl.Title :
          "Plugin action";
        if (!String.IsNullOrEmpty(m_pluginName)) {
          title += " (" + m_pluginName + ")";
        }

        return title;
      }
    }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        return (m_customActionControl != null) ? m_customActionControl.Help :
          "Here is the interface for an action of the plugin. " +
          "This interface is dependent on the action to be done and can thus be anything.\n\n" +
          "Fill the different fields and then validate.\n\n" +
          "The help of this page is usually replaced by the help of the interface displayed.";
      }
    }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PagePluginAction()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      EmitProtectAgainstQuit(true);
      IPluginDll pluginDll = data.Get<IPluginDll>(ItemPackage.CURRENT_PLUGIN);
      m_pluginName = pluginDll.Name;
      
      // Clear the central area
      panelParam.Controls.Clear();
      
      // Add the custom action control
      m_customActionControl = pluginDll.CustomActionControls[data.Get<int>(ItemPackage.CUSTOM_ACTION)];

      if (m_customActionControl is Lemoine.Extensions.Configuration.GuiBuilder.ICustomActionGuiBuilder) {
        var pluginConfig = new PluginConfig ();
        var configurationGuiBuilder =
          (Lemoine.Extensions.Configuration.GuiBuilder.ICustomActionGuiBuilder)m_customActionControl;
        configurationGuiBuilder.SetConfigControl (pluginConfig);
        pluginConfig.Dock = DockStyle.Fill;
        m_customActionControl.InitializeInterface ();
        panelParam.Controls.Add ((Control)pluginConfig);
      }
      else if (m_customActionControl is UserControl) {
        ((UserControl)m_customActionControl).Dock = DockStyle.Fill;
        panelParam.Controls.Add ((Control)m_customActionControl);
        m_customActionControl.InitializeInterface ();
      }
      else {
        log.Error ("LoadPageFromData: configuration control is neither a UserControl nor a ConfigurationGuiBuilder");
        m_customActionControl.InitializeInterface ();
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Nothing
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      IList<string> listTmp = m_customActionControl.GetErrors();
      if (listTmp != null) {
        foreach (string error in listTmp) {
          errors.Add(error);
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
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      m_customActionControl.DoAction(ref warnings, ref revisionId);
    }
    
    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public override void ProcessAfterValidation(ItemData data)
    {
      EmitDataChangedEvent(null);
    }
    #endregion // Page methods
  }
}
