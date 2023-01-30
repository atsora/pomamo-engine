// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiCenter
  /// </summary>
  public partial class GuiCenter : UserControl
  // Note: not abstract, else any inheritance can't be used in the gui designer
  {
    /// <summary>
    /// log for history
    /// </summary>
    static protected readonly ILog logHistory = LogManager.GetLogger ("Lemoine.Lem_Settings.History");

    #region Events
    /// <summary>
    /// Event emitted when the configurator is closed
    /// </summary>
    public event Action ItemFinished;

    /// <summary>
    /// Event emitted when an item is double clicked
    /// First argument is the item to launch
    /// Second argument is true if the item has to be displayed as a view
    /// Third argument is the current data
    /// Fourth argument is for the next item
    /// </summary>
    public event Action<IItem, bool, ItemData, bool> ItemLaunched;

    /// <summary>
    /// Event emitted when an revision blocks the item and when the user wants to show it
    /// First argument is the item
    /// </summary>
    public event Action<IItem> ShowRevisionClicked;
    #endregion // Events

    #region Members
    Control m_currentControl = null;

    /// <summary>
    /// Reference to the left part of the GUI (related items)
    /// </summary>
    protected GuiLeft m_guiLeft = null;

    /// <summary>
    /// Reference to the right part of the GUI (help)
    /// </summary>
    protected GuiRight m_guiRight = null;

    /// <summary>
    /// Custom title
    /// </summary>
    protected string m_customTitle = "";

    /// <summary>
    /// Current item, that can be a view, wizard or configurator
    /// </summary>
    protected IItem m_currentItem = null;

    /// <summary>
    /// Current item data, passing through all pages of the current item
    /// </summary>
    protected ItemData m_itemData = null;

    /// <summary>
    /// Other data used to initialize the current item
    /// </summary>
    protected ItemData m_otherData = null;

    /// <summary>
    /// All possible item pages that can be displayed
    /// </summary>
    protected IDictionary<string, IItemPage> m_itemPages = new Dictionary<string, IItemPage> ();

    /// <summary>
    /// Current list of consecutive pages that have been displayed
    /// Allow us to go to the previous page (history)
    /// </summary>
    protected IList<IItemPage> m_currentPages = new List<IItemPage> ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Get or set the protection of the button "home"
    /// </summary>
    public bool ProtectAgainstQuit { get; set; }

    /// <summary>
    /// Get the current item
    /// </summary>
    /// <returns></returns>
    public IItem GetCurrentItem () { return m_currentItem; }

    /// <summary>
    /// True if the item has been disabled because at least one revision is in progress
    /// </summary>
    protected bool DisabledByRevisions { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected GuiCenter ()
    {
      DisabledByRevisions = false;
      ProtectAgainstQuit = false;
      InitializeComponent ();
      overlayDisable.Visible = false;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Init references to access foreign objects
    /// </summary>
    /// <param name="guiLeft"></param>
    /// <param name="guiRight"></param>
    public void InitReferences (GuiLeft guiLeft, GuiRight guiRight)
    {
      m_guiLeft = guiLeft;
      m_guiRight = guiRight;
    }

    /// <summary>
    /// Trigger time out for the current page
    /// </summary>
    public void TriggerTimeOut ()
    {
      if (m_currentPages.Count > 0) {
        m_currentPages.Last ().OnTimeOut ();
      }
    }

    /// <summary>
    /// Method called when the page needs to be updated (when data changed)
    /// </summary>
    public void UpdateData ()
    {
      // Try to update the data of the item.
      // If an error occurs, the item is reinitialized.
      try {
        if (m_currentPages.Count > 0) {
          var lastPage = GetLastPage ();
          lastPage.SavePageInData (m_itemData);
          lastPage.Initialize (m_currentItem.Context);
          DisplayLastPage ();
        }
      }
      catch (Exception ex) {
        logHistory.ErrorFormat ("Error on UpdateData() => item reinitialized: {0}", ex.Message);
        Reinit ();
      }
    }

    /// <summary>
    /// Reinit the item displayed
    /// 
    /// To overwrite
    /// </summary>
    public virtual void Reinit () { }

    /// <summary>
    /// Emit the signal "ItemFinished"
    /// </summary>
    protected void EmitItemFinished ()
    {
      ItemFinished ();
    }

    /// <summary>
    /// Emit the signal "ItemLaunched"
    /// </summary>
    /// <param name="item"></param>
    /// <param name="viewMode"></param>
    /// <param name="otherData"></param>
    /// <param name="nextItem"></param>
    protected void EmitItemLaunched (IItem item, bool viewMode, ItemData otherData, bool nextItem)
    {
      ItemLaunched (item, viewMode, otherData, nextItem);
    }

    /// <summary>
    /// Show a dialog for displaying errors
    /// </summary>
    /// <param name="errors">Errors to display, cannot be null or empty</param>
    /// <param name="ignorePossible">True if SuperAdmin can click on "Ignore"</param>
    /// <returns></returns>
    protected bool DisplayErrors (IEnumerable<string> errors, bool ignorePossible)
    {
      string plural = errors.Count () > 1 ? "s" : "";
      string text = "Please check the following point" + plural + ":\n - " +
        String.Join (",\n - ", errors.ToArray ()) + ".";
      var dialog = new WarningDialog (text, ignorePossible);
      return (dialog.ShowDialog () == DialogResult.Ignore);
    }

    IItemPage GetLastPage ()
    {
      return m_currentPages.Last ();
    }

    /// <summary>
    /// Display and prepare the last page asked by the user
    /// </summary>
    protected void DisplayLastPage ()
    {
      // If it's the root page: by default not protected against the button "home"
      // It it's not: by default protected
      // The page can then change this value thanks to the event "ProtectAgainstQuit"
      ProtectAgainstQuit = (m_currentPages.Count > 1);

      // Initialize the page to display
      var lastPage = GetLastPage ();
      m_itemData.CurrentPageName = lastPage.GetType ().Name;
      m_customTitle = "";
      InitHeader (lastPage);
      lastPage.LoadPageFromData (m_itemData);

      // Modify interface (title and description)
      SetTitle (lastPage.Title);
      m_guiRight.SetCurrentPage (lastPage);

      // Prepare the buttons
      if (RevisionManager.IsUsable (m_currentItem)) {
        overlayDisable.Hide ();
        PrepareButtons ();
      }
      else {
        overlayDisable.Show ();
        SetLeftButtonVisible (false);
        SetRightButtonVisible (false);
      }

      // Display the page
      SetPage (lastPage);
      ActiveControl = lastPage as Control;
    }
    #endregion // Methods

    #region Gui methods
    /// <summary>
    /// Remove the page displayed in the GUI
    /// </summary>
    protected void RemovePage ()
    {
      if (m_currentControl != null) {
        innerTable.Controls.Remove (m_currentControl);
      }

      m_currentControl = null;
    }

    /// <summary>
    /// Initialize the header based on the flags
    /// </summary>
    /// <param name="page"></param>
    protected void InitHeader (IItemPage page)
    {
      if ((page.Flags & LemSettingsGlobal.PageFlag.DANGEROUS_ACTIONS) != 0) {
        centralHeader.Set (LemSettingsGlobal.COLOR_ERROR, "Actions taken here may corrupt the database!");
      }
      else if ((page.Flags & LemSettingsGlobal.PageFlag.ADMIN_RIGHT_REQUIRED) != 0) {
        centralHeader.Set (LemSettingsGlobal.COLOR_ERROR, "Admin rights are required");
      }
      else {
        centralHeader.HideText ();
      }

      innerTable.RowStyles[0].Height = centralHeader.HasMessage ? 20 : 0;
    }

    /// <summary>
    /// Set a page in the GUI
    /// </summary>
    /// <param name="page"></param>
    void SetPage (IItemPage page)
    {
      using (new SuspendDrawing (panel)) {
        RemovePage ();
        m_currentControl = page as Control;
        innerTable.Controls.Add (m_currentControl, 0, 1);
        m_guiLeft.SetCurrentItemPage (page);
        if (!string.IsNullOrEmpty (ContextManager.Options.ItemId)) {
          baseLayout.ColumnStyles[0].Width = 0;
        }
      }
    }

    /// <summary>
    /// Show the left button "previous"
    /// </summary>
    protected void DisplayPrevious ()
    {
      buttonBack.Image = imageList.Images[2];
    }

    /// <summary>
    /// Show the left button "cancel"
    /// </summary>
    protected void DisplayCancel ()
    {
      buttonBack.Image = imageList.Images[0];
    }

    /// <summary>
    /// Show the right button "next"
    /// </summary>
    protected void DisplayNext ()
    {
      buttonNext.Image = imageList.Images[1];
    }

    /// <summary>
    /// Show the right button "ok"
    /// </summary>
    protected void DisplayOk ()
    {
      buttonNext.Image = imageList.Images[3];
    }

    /// <summary>
    /// Show or hide the left button
    /// </summary>
    /// <param name="visible"></param>
    protected void SetLeftButtonVisible (bool visible)
    {
      baseLayout.ColumnStyles[2].Width = visible ? 35 : 0;
    }

    /// <summary>
    /// Show or hide the right button
    /// </summary>
    /// <param name="visible"></param>
    protected void SetRightButtonVisible (bool visible)
    {
      baseLayout.ColumnStyles[3].Width = visible ? 35 : 0;
    }

    /// <summary>
    /// Enable or not the left button
    /// </summary>
    /// <param name="enabled"></param>
    protected void SetLeftButtonEnabled (bool enabled)
    {
      buttonBack.Enabled = enabled;
    }

    /// <summary>
    /// Enable or not the right button
    /// </summary>
    /// <param name="enabled"></param>
    protected void SetRightButtonEnabled (bool enabled)
    {
      buttonNext.Enabled = enabled;
    }

    /// <summary>
    /// Set the title shown above the page
    /// m_customTitle has the priority over the argument title
    /// </summary>
    /// <param name="title"></param>
    protected void SetTitle (string title)
    {
      labelTitle.Text = string.IsNullOrEmpty (m_customTitle) ? title : m_customTitle;
    }

    /// <summary>
    /// True if the "home" button is visible
    /// Public since SecondaryEditor is accessing it
    /// </summary>
    /// <param name="visible"></param>
    public void SetHomeButtonVisible (bool visible)
    {
      baseLayout.ColumnStyles[0].Width = visible ? 35 : 0;
    }

    /// <summary>
    /// Return the main control (to be printed)
    /// </summary>
    /// <returns></returns>
    public Control GetMainControl ()
    {
      if (innerTable.Controls.Count > 1) {
        return innerTable;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Get the name of the main control
    /// </summary>
    /// <returns></returns>
    public string GetMainControlName ()
    {
      if (innerTable.Controls.Count > 1) {
        return innerTable.Controls[1].GetType ().ToString ();
      }
      else {
        return "";
      }
    }

    /// <summary>
    /// Show or hide the buttons depending on the page and its configuration
    /// 
    /// To overwrite
    /// </summary>
    protected virtual void PrepareButtons () { }
    #endregion // Gui methods

    #region Event reactions
    /// <summary>
    /// Method called when the title is changed
    /// </summary>
    /// <param name="text"></param>
    protected void OnSetTitle (string text)
    {
      m_customTitle = text;
      SetTitle (GetLastPage ().Title);
    }

    void ButtonNextClick (object sender, EventArgs e)
    {
      OnRightButtonClick ();
    }

    /// <summary>
    /// Method called when the right button is clicked
    /// 
    /// To overwrite
    /// </summary>
    protected virtual void OnRightButtonClick ()
    {
      throw new NotImplementedException ();
    }

    void ButtonBackClick (object sender, EventArgs e)
    {
      OnLeftButtonClick ();
    }

    /// <summary>
    /// Method called when the left button is clicked
    /// 
    /// To overwrite
    /// </summary>
    protected virtual void OnLeftButtonClick ()
    {
      throw new NotImplementedException ();
    }

    void ButtonHomeClick (object sender, EventArgs e)
    {
      OnButtonHomeClick ();
    }

    /// <summary>
    /// Method called when the button "home" is clicked
    /// 
    /// To overwrite
    /// </summary>
    protected virtual void OnButtonHomeClick ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Method called to set the header
    /// </summary>
    /// <param name="color"></param>
    /// <param name="text"></param>
    protected void OnSpecifyHeader (System.Drawing.Color color, string text)
    {
      centralHeader.Set (color, text);
      innerTable.RowStyles[0].Height = centralHeader.HasMessage ? 20 : 0;
    }

    void OverlayDisableLinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (ShowRevisionClicked != null) {
        ShowRevisionClicked (m_currentItem);
      }
    }
    #endregion // Event reactions
  }
}
