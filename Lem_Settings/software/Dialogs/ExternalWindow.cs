// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// External window displaying a wizard, a configurator or a view
  /// An unlimited number of windows are possible
  /// An external window comprising an editor:
  /// - cannot call another editor
  /// - is modal
  /// </summary>
  public partial class ExternalWindow : Form
  {
    [DllImport ("user32.dll")]
    static extern int GetWindowLong (IntPtr hWnd, int nIndex);

    [DllImport ("user32.dll")]
    static extern int SetWindowLong (IntPtr hWnd, int nIndex, int dwNewLong);

    static void EnableWindow (IWin32Window window)
    {
      const int GWL_STYLE = -16;
      const int WS_DISABLED = 0x08000000;
      SetWindowLong (window.Handle, GWL_STYLE,
                    GetWindowLong (window.Handle, GWL_STYLE) & ~WS_DISABLED);
    }

    #region Members
    GuiCenter m_guiCenter = null;
    IDictionary<Type, LemSettingsGlobal.InteractionType> m_mapTypes = null;
    static readonly IList<ExternalWindow> s_editorInstances = new List<ExternalWindow> ();
    static readonly IList<ExternalWindow> s_viewInstances = new List<ExternalWindow> ();
    static bool s_editorOpeningEnabled = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ExternalWindow).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return true if an external window already comprises an editor
    /// </summary>
    public static bool EditorInUse { get { return s_editorInstances.Count > 0; } }

    /// <summary>
    /// Return true if one or several views are open
    /// </summary>
    /// <returns></returns>
    public static bool ViewInUse { get { return (s_viewInstances.Count > 0); } }

    /// <summary>
    /// If set to true, views can call an editing item
    /// (in the case where no editing items are already open)
    /// </summary>
    public static bool EditorOpeningEnabled
    {
      set {
        s_editorOpeningEnabled = value;
        foreach (var window in s_viewInstances) {
          window.guiLeft.Enabled = s_editorOpeningEnabled;
        }
      }
    }
    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Event emitted when the editor item has finished
    /// If a configurator is used, this event is emitted even if it's canceled
    /// (we cannot be sure that no actions have been taken)
    /// </summary>
    public static event Action EditorItemFinished;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Create a window with an item inside
    /// </summary>
    /// <param name="item"></param>
    /// <param name="viewMode"></param>
    /// <param name="otherData"></param>
    ExternalWindow (IItem item, bool viewMode, ItemData otherData)
    {
      InitializeComponent ();
      SetWindowTitle (item.Title);

      // Panels
      UpdatePanelVisibilityInstance ();

      // Center
      if (item is IWizard) {
        // Editor: wizard
        m_guiCenter = new WizardGuiCenter ();
        InitCenter ();
        (m_guiCenter as WizardGuiCenter).SetCurrentItem (item as IWizard, otherData);
        s_editorInstances.Add (this);
      }
      else if (item is IConfigurator && !viewMode) {
        // Editor: configurator
        m_guiCenter = new ConfiguratorGuiCenter ();
        InitCenter ();
        (m_guiCenter as ConfiguratorGuiCenter).SetCurrentItem (item as IConfigurator, otherData);
        s_editorInstances.Add (this);
      }
      else if (item is IView || (item is IConfigurator && viewMode)) {
        // View: view or configurator in view mode
        m_guiCenter = new ViewGuiCenter ();
        InitCenter ();
        (m_guiCenter as ViewGuiCenter).SetCurrentItem (item, otherData);
        this.guiLeft.Enabled = s_editorOpeningEnabled;
        s_viewInstances.Add (this);
      }
      else {
        throw new Exception ("Invalid item in an external window");
      }

      // Types read and written
      m_mapTypes = item.Types;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Show an external window comprising a wizard, configurator or view
    /// </summary>
    /// <param name="item"></param>
    /// <param name="viewMode"></param>
    /// <param name="owner"></param>
    /// <param name="otherData"></param>
    public static void Show (IItem item, bool viewMode, Form owner, ItemData otherData)
    {
      // Owner is the last modal form
      if (s_editorInstances.Count > 0) {
        owner = s_editorInstances[s_editorInstances.Count - 1];
      }

      // Creation of the window
      var window = new ExternalWindow (item, viewMode, otherData);

      // Show the dialog (as modal if it is an editor)
      if (item is IWizard || (item is IConfigurator && !viewMode)) {
        window.ShowDialog (owner);

        // All existing views are re-enabled
        foreach (var view in s_viewInstances) {
          EnableWindow (view);
        }
      }
      else {
        window.Show ();
      }
    }

    /// <summary>
    /// Method called when data changed (an editor has been used)
    /// </summary>
    /// <param name="types"></param>
    public static void DataChanged (IList<Type> types)
    {
      foreach (var window in s_editorInstances) {
        window.DataChangedInstance (types);
      }

      foreach (var window in s_viewInstances) {
        window.DataChangedInstance (types);
      }
    }

    /// <summary>
    /// Triggered when time's out
    /// </summary>
    public static void TimeOut ()
    {
      foreach (var window in s_editorInstances) {
        window.TimeOutInstance ();
      }

      foreach (var window in s_viewInstances) {
        window.TimeOutInstance ();
      }
    }

    /// <summary>
    /// Reinit the wizard or configurator
    /// </summary>
    public static void Reinit ()
    {
      foreach (var window in s_editorInstances) {
        window.ReinitInstance ();
      }

      foreach (var window in s_viewInstances) {
        window.ReinitInstance ();
      }
    }

    /// <summary>
    /// Update the panel visibility for all windows
    /// </summary>
    public static void UpdatePanelVisibility ()
    {
      foreach (var window in s_editorInstances) {
        window.UpdatePanelVisibilityInstance ();
      }

      foreach (var window in s_viewInstances) {
        window.UpdatePanelVisibilityInstance ();
      }
    }

    void UpdatePanelVisibilityInstance ()
    {
      if (IniFilePreferences.Get (IniFilePreferences.Field.LEFT_PANEL_VISIBILITY) != "yes") {
        table.ColumnStyles[0].Width = 0;
      }
      else {
        table.ColumnStyles[0].Width = 150;
      }

      if (IniFilePreferences.Get (IniFilePreferences.Field.RIGHT_PANEL_VISIBILITY) != "yes") {
        table.ColumnStyles[2].Width = 0;
      }
      else {
        table.ColumnStyles[2].Width = 150;
      }
    }

    void InitCenter ()
    {
      m_guiCenter.SetHomeButtonVisible (false);
      m_guiCenter.ItemFinished += OnItemFinished;
      m_guiCenter.InitReferences (guiLeft, guiRight);
      table.Controls.Add (m_guiCenter, 1, 0);
      m_guiCenter.Dock = DockStyle.Fill;
    }

    void TimeOutInstance ()
    {
      m_guiCenter.TriggerTimeOut ();
    }

    void DataChangedInstance (ICollection<Type> types)
    {
      IItem item = GetItem ();
      bool impacted = false;
      foreach (Type type in item.Types.Keys) {
        impacted |= types.Contains (type);
      }

      if (impacted) {
        m_guiCenter.UpdateData ();
      }
    }

    void ReinitInstance ()
    {
      m_guiCenter.Reinit ();
    }

    void SetWindowTitle (String itemName)
    {
      String suffix = ContextManager.WindowTitleSuffix;
      if (!String.IsNullOrEmpty (itemName)) {
        this.Text = itemName + " - " + LemSettingsGlobal.SOFTWARE_TITLE + suffix;
      }
      else {
        this.Text = LemSettingsGlobal.SOFTWARE_TITLE + suffix;
      }
    }
    #endregion // Methods

    #region Event reactions
    void OnItemFinished ()
    {
      this.Close ();
    }

    void ExternalWindowFormClosing (object sender, FormClosingEventArgs e)
    {
      if (m_guiCenter != null && m_guiCenter.ProtectAgainstQuit) {
        DialogResult result = MessageBoxCentered.Show (
          this,
          "Modifications have not been taken into account yet.\n" +
          "Do you really want to close the window?",
          "Question",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question,
          MessageBoxDefaultButton.Button2);
        e.Cancel = (result == DialogResult.No);
      }
    }

    void ExternalWindowFormClosed (object sender, FormClosedEventArgs e)
    {
      // An item has been closed, it is removed from the static lists
      if (s_viewInstances.Contains (this)) {
        s_viewInstances.Remove (this);
      }
      else if (s_editorInstances.Contains (this)) {
        s_editorInstances.Remove (this);

        // Possibly update views
        IList<Type> types = new List<Type> ();

        if (m_mapTypes != null) {
          foreach (Type type in m_mapTypes.Keys) {
            if ((m_mapTypes[type] & LemSettingsGlobal.InteractionType.PRINCIPAL) != 0) {
              types.Add (type);
            }
          }
        }

        foreach (var window in s_viewInstances) {
          window.DataChangedInstance (types);
        }

        // Update the last item that has called the editor
        if (s_editorInstances.Count == 0) {
          EditorItemFinished ();
        }
        else {
          s_editorInstances[s_editorInstances.Count - 1].m_guiCenter.UpdateData ();
        }
      }

      // Destruction of the center
      m_guiCenter.Dispose ();
    }

    void ExternalWindowShown (object sender, EventArgs e)
    {
      // Re-enable all views if this window is an editor (modal)
      if (s_editorInstances.Contains (this)) {
        foreach (var view in s_viewInstances) {
          EnableWindow (view);
        }
      }
    }
    #endregion // Event reactions

    #region Print methods
    /// <summary>
    /// Return the main controls (to be printed)
    /// </summary>
    /// <returns></returns>
    static public IList<Control> GetMainControls ()
    {
      IList<Control> controls = new List<Control> ();
      foreach (var window in s_editorInstances) {
        controls.Add (window.m_guiCenter.GetMainControl ());
      }

      foreach (var window in s_viewInstances) {
        controls.Add (window.m_guiCenter.GetMainControl ());
      }

      return controls;
    }

    /// <summary>
    /// Return the list of all items displayed in views
    /// </summary>
    /// <returns></returns>
    static public IList<IItem> GetItems ()
    {
      IList<IItem> items = new List<IItem> ();
      foreach (var window in s_editorInstances) {
        items.Add (window.GetItem ());
      }

      foreach (var window in s_viewInstances) {
        items.Add (window.GetItem ());
      }

      return items;
    }

    /// <summary>
    /// Get the name of the main controls
    /// </summary>
    /// <returns></returns>
    static public IList<string> GetMainControlNames ()
    {
      IList<string> controlNames = new List<string> ();
      foreach (var window in s_editorInstances) {
        controlNames.Add (window.m_guiCenter.GetMainControlName ());
      }

      foreach (var window in s_viewInstances) {
        controlNames.Add (window.m_guiCenter.GetMainControlName ());
      }

      return controlNames;
    }

    /// <summary>
    /// If the view is displayed in an existing viewForm, this viewForm is brought
    /// to front and the function return true.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool BringToFront (IItem item)
    {
      // Editor?
      foreach (var window in s_editorInstances) {
        if (window.GetItem ().ID == item.ID && window.GetItem ().SubID == item.SubID) {
          window.MoveToFront ();
          return true;
        }
      }

      // View?
      foreach (var window in s_viewInstances) {
        if (window.GetItem ().ID == item.ID && window.GetItem ().SubID == item.SubID) {
          window.MoveToFront ();
          return true;
        }
      }

      return false;
    }

    IItem GetItem ()
    {
      return m_guiCenter.GetCurrentItem ();
    }

    void MoveToFront ()
    {
      // This window is shown
      this.Show ();
      if (this.WindowState == FormWindowState.Minimized) {
        this.WindowState = FormWindowState.Normal;
      }

      // And brought to front
      this.BringToFront ();
    }
    #endregion // Print methods
  }
}
