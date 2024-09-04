// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;

namespace ConfiguratorRedirectEvent
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    bool m_viewMode;
    ActionManager m_actionManager = null;
    bool m_preparation = true;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Action list"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        return "Possible actions are listed here. By default everything is displayed " +
          "unless you filter them by event type and / or by action type.\n\n" +
          (m_viewMode ? "Each action can be activated or not by using the checkbox. " : "") +
          "You can read the corresponding .xml file.";
      }
    }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return ActionManager.IsAdminRightRequired() ?
          LemSettingsGlobal.PageFlag.ADMIN_RIGHT_REQUIRED :
          LemSettingsGlobal.PageFlag.NONE;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
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
    public void Initialize(ItemContext context)
    {
      m_viewMode = context.ViewMode;
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_actionManager = data.Get<ActionManager>(Item.ACTION_MANAGER);
      
      // Fill event and alert types
      m_preparation = true;
      comboEvent.ClearItems();
      foreach (var eventType in m_actionManager.GetPossibleEventTypes()) {
        comboEvent.AddItem(eventType, eventType);
      }

      comboEvent.InsertItem("all", "", 0);
      comboEvent.SelectedIndex = 0;
      
      comboAction.ClearItems();
      foreach (var alertType in m_actionManager.GetPossibleActionTypes()) {
        comboAction.AddItem(alertType, alertType);
      }

      comboAction.InsertItem("all", "", 0);
      comboAction.SelectedIndex = 0;
      m_preparation = false;
      
      FillActions("", "");
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Nothing to do
    }
    #endregion // Page methods
    
    #region Event reactions
    void ComboEventItemChanged(string arg1, object arg2)
    {
      if (m_preparation) {
        return;
      }

      FillActions (arg2 as string, comboAction.SelectedValue as string);
    }
    
    void ComboActionItemChanged(string arg1, object arg2)
    {
      if (m_preparation) {
        return;
      }

      FillActions (comboEvent.SelectedValue as string, arg2 as string);
    }
    
    void OnReadTriggered(Action alert)
    {
      var dialog = new XmlReaderDialog(alert.Name, alert.FullText);
      
      // Metadata highlighted
      dialog.HighlightTextBetween("<description>", "</description>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<title>", "</title>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<eventtype>", "</eventtype>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<actiontype>", "</actiontype>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<advanced>", "</advanced>", Color.BlanchedAlmond, true);
      
      dialog.Show();
    }
    
    void OnEnableChangeTriggered(Action alert, bool enableState)
    {
      EmitLogAction("OnEnableChangeTriggered", String.Format(
        "  - action file: {0}\n  - enable state: {1}", alert.Name, enableState), "ok");
    }
    #endregion // Event reactions
    
    #region Private methods
    void FillActions(string eventType, string actionType)
    {
      verticalScroll.Clear();
      bool alternate = false;
      foreach (var action in m_actionManager.GetAlerts(eventType, actionType)) {
        var cell = new ActionCell(action, m_viewMode);
        cell.ReadTriggered += OnReadTriggered;
        cell.EnableChanged += OnEnableChangeTriggered;
        cell.Dock = DockStyle.Fill;
        if (alternate) {
          cell.BackColor = LemSettingsGlobal.COLOR_ALTERNATE_ROW;
        }

        verticalScroll.AddControl(cell);
        
        alternate = !alternate;
      }
    }
    #endregion // Private methods
  }
}
