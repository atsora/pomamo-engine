// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    enum InterfaceType
    {
      SELECTION,
      WIZARD,
      CONFIGURATOR,
      VIEW
    }

    #region Members
    readonly GuiLeft1 m_left1 = new GuiLeft1 ();
    readonly GuiLeft m_left23 = new GuiLeft ();
    readonly GuiRight1 m_right1 = new GuiRight1 ();
    readonly GuiRight m_right23 = new GuiRight ();
    readonly SelectionGuiCenter m_center1 = new SelectionGuiCenter ();
    // Item selection
    readonly WizardGuiCenter m_center2 = new WizardGuiCenter ();
    // Wizard
    readonly ConfiguratorGuiCenter m_center3 = new ConfiguratorGuiCenter ();
    // Configurator
    readonly ViewGuiCenter m_center4 = new ViewGuiCenter ();
    // View or configurator in view mode
    InterfaceType m_currentInterfaceType = InterfaceType.SELECTION;
    System.Windows.Forms.Timer m_timer = new System.Windows.Forms.Timer ();
    bool m_noConfirmationOnClose = false;
    IItem m_currentItem = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// If set to false, the software continue to run after an unhandled exception is raised
    /// This is used when configurators or wizards validate the changes.
    /// If an exception occurs, the user can try to validate again with other values
    /// Otherwise this is true
    /// </summary>
    static public bool QuitIfException { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public MainForm ()
    {
      QuitIfException = true;

      // Initialization of the interface
      InitializeComponent ();
      develToolStripMenuItem.Visible = false;
      m_left1.Dock = m_left23.Dock = DockStyle.Fill;
      m_right1.Dock = m_right23.Dock = DockStyle.Fill;
      m_center1.Dock = m_center2.Dock = m_center3.Dock = m_center4.Dock = DockStyle.Fill;
      m_left1.InitReferences (m_center1, m_right1);
      m_center1.InitReferences (m_left1);
      m_center2.InitReferences (m_left23, m_right23);
      m_center3.InitReferences (m_left23, m_right23);
      m_center4.InitReferences (m_left23, m_right23);

      // Connect parts of the interface
      ItemManager.ItemsChanged += OnItemsChanged;
      m_center1.ItemLaunched += OnItemLaunched;
      m_center2.ItemFinished += OnBackToSelection;
      m_center2.ItemLaunched += OnItemLaunched;
      m_center2.ShowRevisionClicked += OnShowRevisionClicked;
      m_center3.ItemFinished += OnBackToSelection;
      m_center3.ItemLaunched += OnItemLaunched;
      m_center3.ShowRevisionClicked += OnShowRevisionClicked;
      m_center4.ItemFinished += OnBackToSelection;
      m_center4.ItemLaunched += OnItemLaunched;
      m_center4.ShowRevisionClicked += OnShowRevisionClicked;
      TreeItem.ItemCalledEvent += OnItemLaunched;
      ExternalWindow.EditorItemFinished += UpdateMainEditor;
      m_center1.ItemClicked += OnItemClicked;

      // Display panels
      if (IniFilePreferences.Get (IniFilePreferences.Field.LEFT_PANEL_VISIBILITY) != "yes") {
        leftPanelToolStripMenuItem.Checked = false;
        LeftPanelMenuClick (null, null);
      }
      if (IniFilePreferences.Get (IniFilePreferences.Field.RIGHT_PANEL_VISIBILITY) != "yes") {
        rightPanelToolStripMenuItem.Checked = false;
        RightPanelMenuClick (null, null);
      }

      // Switch to select mode
      SwitchInterface (InterfaceType.SELECTION);

      // Activate timer
      m_timer.Interval = 1000;
      m_timer.Tick += TimeOut;
      m_timer.Start ();

      // Revisions
      RevisionManager.RevisionNumberChanged += RevisionNumberChanged;
      RevisionManager.RevisionFinished += RevisionFinished;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the form
    /// </summary>
    /// <returns>Success</returns>
    public void Initialize ()
    {
      ExtensionManager.Load ();

      // Current computer status
      ContextManager.IsLctr = ContextManager.Options.SimulateLCTR;
      ContextManager.IsLpost = ContextManager.Options.SimulateLPOST;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Settings.ShowDialog.Computer")) {
          var localComputer = ModelDAOHelper.DAOFactory.ComputerDAO.GetLocal ();
          if (null == localComputer) {
            log.WarnFormat ("ShowDialog: the local computer could not be found in the computer table");
          }
          else { // null != localComputer
            ContextManager.IsLctr |= localComputer.IsLctr;
            ContextManager.IsLpost |= localComputer.IsLpst;
          }
        }
      }

      // Load items (the user being known)
      ItemManager.RefreshItems ();

      if (!string.IsNullOrEmpty (ContextManager.Options.ItemId)) {
        IItem item = ItemManager.GetItem (ContextManager.Options.ItemId, ContextManager.Options.ItemSubId);

        if (item == null) {
          log.Error ($"Initialize: the item {ContextManager.Options.ItemId}/{ContextManager.Options.ItemSubId} does not exist => return false");
          throw new Exception ($"Item {ContextManager.Options.ItemId}/{ContextManager.Options.ItemSubId} does not exist");
        }
        else {
          // Execution of the item
          OnItemLaunched (item, ContextManager.Options.ViewMode, null, true);

          if (item is ILauncher || item is ILink) {
            // If the item is a launcher or a link:
            // - the main form is not shown,
            // - LemSettings closes immediately.
            throw new Exception ($"Invalid item");
          }
          else {
            // Otherwise, the main form is shown
            m_left1.Hide ();
            table.ColumnStyles[0].Width = 0;
            menuStrip.Hide ();
          }
        }
      }
      else {
        // Prepare categories, hide the splash screen and show the main form
        m_left1.UpdateCategories ();
      }
    }

    void SwitchInterface (InterfaceType interfaceType)
    {
      m_currentInterfaceType = interfaceType;

      // Possibility to open an editor from a view
      ExternalWindow.EditorOpeningEnabled = (interfaceType == InterfaceType.SELECTION);

      using (new SuspendDrawing (table)) {
        table.Controls.Clear ();
        switch (interfaceType) {
        case InterfaceType.SELECTION:
          table.Controls.Add (m_left1, 0, 0);
          table.Controls.Add (m_center1, 1, 0);
          table.Controls.Add (m_right1, 2, 0);
          SetWindowTitle ("");
          break;
        case InterfaceType.WIZARD:
          table.Controls.Add (m_left23, 0, 0);
          table.Controls.Add (m_center2, 1, 0);
          table.Controls.Add (m_right23, 2, 0);
          break;
        case InterfaceType.CONFIGURATOR:
          table.Controls.Add (m_left23, 0, 0);
          table.Controls.Add (m_center3, 1, 0);
          table.Controls.Add (m_right23, 2, 0);
          break;
        case InterfaceType.VIEW:
          table.Controls.Add (m_left23, 0, 0);
          table.Controls.Add (m_center4, 1, 0);
          table.Controls.Add (m_right23, 2, 0);
          break;
        }
      }

      // Focus (after SuspendDrawing)
      switch (interfaceType) {
      case InterfaceType.SELECTION:
        m_center1.Focus ();
        break;
      case InterfaceType.WIZARD:
        m_center2.Focus ();
        break;
      case InterfaceType.CONFIGURATOR:
        m_center3.Focus ();
        break;
      case InterfaceType.VIEW:
        m_center4.Focus ();
        break;
      }
    }

    string PrintControl (Control control, string name)
    {
      // Create bitmap
      var bitmap = new Bitmap (control.Width + 2, control.Height + 2);
      using (Graphics g = Graphics.FromImage (bitmap)) {
        g.FillRectangle (new SolidBrush (Color.Gray), 0, 0, control.Width + 2, control.Height + 2);
        g.CopyFromScreen (control.PointToScreen (Point.Empty), new Point (1, 1), control.Size);
      }

      // Save it
      string path = String.Format ("{0}\\{1}.png",
                                  Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
                                  name);
      bitmap.Save (path);

      return path;
    }
    #endregion // Methods

    #region Event reaction
    void OnItemsChanged ()
    {
      ItemManager.RefreshItems ();
      m_left1.UpdateCategories ();
    }

    void OnItemLaunched (IItem item, bool showAsView, ItemData otherData, bool nextItem)
    {
      // Save the last accessed date of the item
      item.LastUsed = DateTime.Now;
      if (m_left1.IsRecent) {
        m_left1.UpdateCategories ();
      }

      if (item is IView || (item is IConfigurator && showAsView &&
                            (item.Flags & LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED) != 0)) {
        item.Context.ViewMode = true;

        if (!string.IsNullOrEmpty (ContextManager.Options.ItemId) && nextItem) {
          // View in the main window
          SetWindowTitle (item.Title);
          m_center4.SetCurrentItem (item, otherData);
          SwitchInterface (InterfaceType.VIEW);
        }
        else {
          // View in an external window

          // Is it already open?
          if (!ExternalWindow.BringToFront (item)) {
            // Create dialog and display it
            try {
              ExternalWindow.Show (item, true, this, otherData);
            }
            catch (Exception ex) {
              ManageError (ex);
              return;
            }
          }

          if (nextItem) {
            // Main window is back to the selection state
            OnBackToSelection ();
          }
        }
      }
      else if (item is IWizard || item is IConfigurator) {
        item.Context.ViewMode = false;
        if (m_currentInterfaceType == InterfaceType.SELECTION || nextItem) {
          // Primary editor
          SetWindowTitle (item.Title);
          if (item is IConfigurator) {
            try {
              m_center3.SetCurrentItem (item as IConfigurator, otherData);
            }
            catch (Exception ex) {
              ManageError (ex);
              return;
            }
            SwitchInterface (InterfaceType.CONFIGURATOR);
          }
          else {
            try {
              m_center2.SetCurrentItem (item as IWizard, otherData);
            }
            catch (Exception ex) {
              ManageError (ex);
              return;
            }
            SwitchInterface (InterfaceType.WIZARD);
          }
        }
        else {
          // Editor in an external window
          try {
            ExternalWindow.Show (item, false, this, otherData);
          }
          catch (Exception ex) {
            log.Error ("OnItemLaunched: " +
                      "exception", ex);
            ManageError (ex);
            return;
          }
        }
      }
      else if (item is ILauncher) {
        // Software shortcut
        try {
          var launcher = item as ILauncher;
          var exePath = launcher.SoftwarePath;

          // New process
          var start = new ProcessStartInfo ();
          start.FileName = exePath;
          if (launcher.Arguments != null && launcher.Arguments.Count > 0) {
            start.Arguments = String.Join (" ", launcher.Arguments.ToArray ());
          }

          if ((launcher.Flags & LemSettingsGlobal.ItemFlag.PROCESS_MODAL) != 0 &&
              string.IsNullOrEmpty (ContextManager.Options.ItemId)) {
            using (Process proc = Process.Start (start)) {
              proc.WaitForExit ();
            }

            // Notify edited types
            IList<Type> editedTypes = new List<Type> ();
            foreach (Type type in launcher.Types.Keys) {
              if ((launcher.Types[type] & LemSettingsGlobal.InteractionType.PRINCIPAL) != 0) {
                editedTypes.Add (type);
              }
            }

            ExternalWindow.DataChanged (editedTypes);
            UpdateMainEditor ();
          }
          else {
            Process.Start (start);
          }
        }
        catch (Exception ex) {
          ManageError (ex);
        }
        if (nextItem) {
          // Main window is back to the selection state
          OnBackToSelection ();
        }
      }
      else if (item is ILink) {
        // Url shortcut
        try {
          Process.Start ((item as ILink).UrlLink);
        }
        catch (Exception ex) {
          ManageError (ex);
        }
        if (nextItem) {
          // Main window is back to the selection state
          OnBackToSelection ();
        }
      }
    }

    void ManageError (Exception ex)
    {
      string txt = (ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN) ?
        ex.Message :
        "Couldn't open the item: invalid or outdated item. Please contact your administrator.";
      log.Error ("Generic error " + ex.Message, ex);
      Debug.Write (ex);
      MessageBoxCentered.Show (txt, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    void OnItemClicked (IItem item)
    {
      m_right1.SetCurrentItem (item);
      m_currentItem = item;
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

    void OnBackToSelection ()
    {
      if (!string.IsNullOrEmpty (ContextManager.Options.ItemId)) {
        // One item was specified on start-up, now it is the end
        this.Close ();
      }

      // Back to wizard selection
      SwitchInterface (InterfaceType.SELECTION);
    }

    void LeftPanelMenuClick (object sender, System.EventArgs e)
    {
      IniFilePreferences.Set (IniFilePreferences.Field.LEFT_PANEL_VISIBILITY,
                             leftPanelToolStripMenuItem.Checked ? "yes" : "no");
      table.ColumnStyles[0].Width = (leftPanelToolStripMenuItem.Checked) ? 150 : 0;
      ExternalWindow.UpdatePanelVisibility ();
    }

    void RightPanelMenuClick (object sender, System.EventArgs e)
    {
      IniFilePreferences.Set (IniFilePreferences.Field.RIGHT_PANEL_VISIBILITY,
                             rightPanelToolStripMenuItem.Checked ? "yes" : "no");
      table.ColumnStyles[2].Width = (rightPanelToolStripMenuItem.Checked) ? 150 : 0;
      ExternalWindow.UpdatePanelVisibility ();
    }

    void PreferencesMenuClick (object sender, EventArgs e)
    {
      var dialog = new EditPreferences ();
      dialog.ShowDialog ();
      dialog.Dispose ();
    }

    void ToolStripMenuItemMouseLeave (object sender, System.EventArgs e)
    {
      var TSMI = sender as ToolStripMenuItem;
      if (!TSMI.DropDown.Visible) {
        TSMI.ForeColor = Color.White;
      }
    }

    void ToolStripMenuItemMouseEnter (object sender, System.EventArgs e)
    {
      var TSMI = sender as ToolStripMenuItem;
      TSMI.ForeColor = Color.Black;
    }

    void ToolStripMenuItemDropDownClosed (object sender, EventArgs e)
    {
      var TSMI = sender as ToolStripMenuItem;
      if (!TSMI.Selected) {
        TSMI.ForeColor = Color.White;
      }
    }

    void TimeOut (object sender, EventArgs e)
    {
      // Update the state of the revisions
      RevisionManager.OnTimeOut ();

      // Offer the possibility to items in external windows or in the main window to be updated
      // (if a reaction to the time out has been implemented for them)
      ExternalWindow.TimeOut ();
      if (m_currentInterfaceType == InterfaceType.CONFIGURATOR) {
        m_center3.TriggerTimeOut ();
      }
    }

    void MainFormLoad (object sender, EventArgs e)
    {
      Initialize ();

      // restore location and size of the form on the desktop
      var rect = IniFilePreferences.Get (IniFilePreferences.Field.WINDOW_POSITION);
      if (!string.IsNullOrEmpty (rect)) {
        try {
          var split = rect.Split (';');
          this.DesktopBounds = new Rectangle (int.Parse (split[0]), int.Parse (split[1]),
                                             int.Parse (split[2]), int.Parse (split[3]));
        }
        catch (Exception ex) {
          log.ErrorFormat ("MainForm - couldn't restore the position {0}: {1}", rect, ex);
        }
      }

      // restore form's window state
      var state = IniFilePreferences.Get (IniFilePreferences.Field.WINDOW_STATE);
      if (!string.IsNullOrEmpty (state)) {
        try {
          this.WindowState = (FormWindowState)Enum.Parse (typeof (FormWindowState), state);
        }
        catch (Exception ex) {
          log.ErrorFormat ("MainForm - couldn't restore the state {0}: {1}", state, ex);
        }
      }
    }

    void MainFormShown (object sender, EventArgs e)
    {
      m_center1.Focus ();
    }

    void MainFormFormClosing (object sender, FormClosingEventArgs e)
    {
      if (!m_noConfirmationOnClose && string.IsNullOrEmpty (ContextManager.Options.ItemId) &&
          (ExternalWindow.ViewInUse || ExternalWindow.EditorInUse ||
           m_currentInterfaceType != InterfaceType.SELECTION)) {
        DialogResult result = MessageBoxCentered.Show (this,
                                                      "All items are not closed. Really quit?",
                                                      "Question",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
        e.Cancel = (result == DialogResult.No);
      }
    }

    void MainFormFormClosed (object sender, FormClosedEventArgs e)
    {
      base.OnClosed (e);

      // Only save the WindowState if Normal or Maximized
      switch (this.WindowState) {
      case FormWindowState.Maximized:
        IniFilePreferences.Set (IniFilePreferences.Field.WINDOW_STATE, this.WindowState.ToString ());
        break;
      default:
        IniFilePreferences.Set (IniFilePreferences.Field.WINDOW_STATE, FormWindowState.Normal.ToString ());
        break;
      }

      // Reset window state to normal to get the correct bounds
      // Also make the form invisible to prevent distracting the user
      switch (this.WindowState) {
      case FormWindowState.Normal:
        IniFilePreferences.Set (IniFilePreferences.Field.WINDOW_POSITION,
                               this.DesktopBounds.X + ";" + this.DesktopBounds.Y + ";" +
                               this.DesktopBounds.Width + ";" + this.DesktopBounds.Height);
        break;
      default:
        IniFilePreferences.Set (IniFilePreferences.Field.WINDOW_POSITION,
                               this.RestoreBounds.X + ";" + this.RestoreBounds.Y + ";" +
                               this.RestoreBounds.Width + ";" + this.RestoreBounds.Height);
        break;
      }

      Application.Exit ();
    }

    void UpdateMainEditor ()
    {
      if (m_currentInterfaceType == InterfaceType.WIZARD) {
        m_center2.UpdateData ();
      }
      else if (m_currentInterfaceType == InterfaceType.CONFIGURATOR) {
        m_center3.UpdateData ();
      }
      else if (m_currentInterfaceType == InterfaceType.VIEW) {
        m_center4.UpdateData ();
      }
    }

    void OnlinehelpToolStripMenuItemClick (object sender, EventArgs e)
    {
      Process.Start (LemSettingsGlobal.LEMOINE_WIKI_ADDRESS);
    }

    /// <summary>
    /// Handle all UI exceptions that are not consumed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UnhandledThreadExceptionHandler (object sender, ThreadExceptionEventArgs e)
    {
      Debug.Assert (null != e.Exception);
      log.ErrorFormat ("UnhandledThreadExceptionHandler: {0} StackTrace: {1}", e.Exception, e.Exception.StackTrace);

      if (ExceptionTest.IsStale (e.Exception, log) ||
          ExceptionTest.IsTemporary (e.Exception, log) ||
          ExceptionTest.IsUnauthorized (e.Exception, log)) {
        // Reinit the elements (configurators & views)
        if (m_currentInterfaceType == InterfaceType.WIZARD) {
          m_center2.Reinit ();
        }
        else if (m_currentInterfaceType == InterfaceType.CONFIGURATOR) {
          m_center3.Reinit ();
        }
        else if (m_currentInterfaceType == InterfaceType.VIEW) {
          m_center4.Reinit ();
        }

        ExternalWindow.Reinit ();

        // Log the exception as error
        log.Error ("Items reinitialized:\n" + e.Exception);

        // Display a message to the user
        if (ExceptionTest.IsUnauthorized (e.Exception, log)) {
          MessageBoxCentered.Show (this, "Admin rights are required for this action.",
                                  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else {
          MessageBoxCentered.Show (this, "Database has been edited, all items are reinitialized.",
                                  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
      }
      else {
        // Log the exception as fatal
        string errorMsg = string.Format (
          "LemSettings version: {0}\nMessage: {1}\n\nStack trace:\n{2}",
          Assembly.GetExecutingAssembly ().GetName ().Version,
          e.Exception.Message, e.Exception.StackTrace);
        log.Fatal (errorMsg);

        // Message to the user
        var dialog = new ErrorDialog (errorMsg, QuitIfException);
        dialog.ShowDialog (this);

        // Close
        if (QuitIfException) {
          m_noConfirmationOnClose = true;
          Application.Exit ();
        }
      }
    }

    void PrintCentralAreaToolStripMenuItemClick (object sender, EventArgs e)
    {
      IList<string> paths = new List<string> ();

      // Main area
      {
        Control control = null;
        string name = "";
        switch (m_currentInterfaceType) {
        case InterfaceType.SELECTION:
          // Nothing
          break;
        case InterfaceType.WIZARD:
          control = m_center2.GetMainControl ();
          name = m_center2.GetMainControlName ();
          break;
        case InterfaceType.CONFIGURATOR:
          control = m_center3.GetMainControl ();
          name = m_center3.GetMainControlName ();
          break;
        case InterfaceType.VIEW:
          control = m_center4.GetMainControl ();
          name = m_center4.GetMainControlName ();
          break;
        }

        if (control != null) {
          paths.Add (PrintControl (control, name));
        }
      }

      // External windows
      {
        IList<Control> controls = ExternalWindow.GetMainControls ();
        IList<string> names = ExternalWindow.GetMainControlNames ();
        IList<IItem> items = ExternalWindow.GetItems ();
        for (int i = 0; i < controls.Count; i++) {
          ExternalWindow.BringToFront (items[i]);
          paths.Add (PrintControl (controls[i], names[i]));
        }
        this.BringToFront ();
      }

      // Inform the user
      if (paths.Count > 0) {
        MessageBoxCentered.Show (String.Format ("Item page{0} successfully printed in:\n- {1}.",
                                              paths.Count > 1 ? "s" : "",
                                              String.Join ("\n- ", paths.ToArray ())),
                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else {
        MessageBoxCentered.Show ("Nothing has been printed.", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    void GetWikiTextToolStripMenuItemClick (object sender, EventArgs e)
    {
      var wtf = new WikiTextFormatter (m_currentItem);
      var dialog = new TextDialog ();
      dialog.Text = m_currentItem.Title + " - Wiki text";
      dialog.DisplayedText = wtf.GetWikiText ();
      dialog.Show ();
    }

    void MainFormKeyDown (object sender, KeyEventArgs e)
    {
      if (e.Control && e.KeyCode == Keys.F) {
        FocusOnSearch ();
      }
    }

    void FocusOnSearch ()
    {
      if (table.ColumnStyles[0].Width > 0 && m_currentInterfaceType == InterfaceType.SELECTION) {
        m_left1.Focus ();
        m_left1.FocusOnSearch ();
      }
    }

    void GetReferencedassembliesToolStripMenuItemClick (object sender, EventArgs e)
    {
      // List of referenced assemblies
      var fullNames = new List<string> ();
      var assTmp = AppDomain.CurrentDomain.GetAssemblies ();
      foreach (var ass in assTmp) {
        var assTmp2 = ass.GetReferencedAssemblies ();
        foreach (var ass2 in assTmp2) {
          if (!fullNames.Contains (ass2.FullName)) {
            fullNames.Add (ass2.FullName);
          }
        }
      }
      fullNames.Sort ();

      // Write the full names in a file on the desktop
      string filePath = Path.Combine (
        Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
        "assemblies.txt");

      if (!File.Exists (filePath)) {
        File.WriteAllText (filePath, String.Join ("\n", fullNames.ToArray ()));
        MessageBoxCentered.Show (this,
                                "Referenced assemblies successfully listed in file '" + filePath + "'.",
                                "Referenced assemblies",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
      }
      else {
        MessageBoxCentered.Show (this,
                                "File '" + filePath + "' already exists.",
                                "Referenced assemblies",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
      }
    }
    void RevisionFinished (string itemId, string itemSubId)
    {
      // Find an existing item with {id, subid}
      var item = ItemManager.GetItem (itemId, itemSubId);
      if (item == null) {
        return;
      }

      // Find data types that might have changed
      IList<Type> editedTypes = new List<Type> ();
      foreach (Type type in item.Types.Keys) {
        if ((item.Types[type] & LemSettingsGlobal.InteractionType.PRINCIPAL) != 0) {
          editedTypes.Add (type);
        }
      }

      // Update open items having a type matching an edited type
      ExternalWindow.DataChanged (editedTypes);
      if (m_currentItem != null) {
        bool impacted = false;
        foreach (Type type in m_currentItem.Types.Keys) {
          impacted |= editedTypes.Contains (type);
        }

        if (impacted) {
          UpdateMainEditor ();
        }
      }
    }

    void RevisionNumberChanged (int number)
    {
      modificationToolStripMenuItem.Text = " " + number;
      modificationToolStripMenuItem.Visible = true;
    }

    void ModificationToolStripMenuItemClick (object sender, EventArgs e)
    {
      var dialog = new RevisionDialog ();
      dialog.ShowDialog ();
    }

    void OnShowRevisionClicked (IItem item)
    {
      var dialog = new RevisionDialog ();
      dialog.SelectRevision (item.ID, item.SubID);
      dialog.ShowDialog ();
    }
    #endregion // Event reaction
  }
}
