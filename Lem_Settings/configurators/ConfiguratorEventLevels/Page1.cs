// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorEventLevel
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    bool m_toDelete = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Event level list"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "The different event levels are displayed in the list, the highest priorities " +
          "being on the top. Blue elements are levels already used by the system.\n\n" +
          "If you select an item, you can then edit and remove it if this item is not used. At any time, you can add a new level.\n\n" +
          "Changes will be taken into account after having validated this page."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    
    /// <summary>
    /// Get the current event level
    /// </summary>
    IEventLevel CurrentEventLevel {
      get { return listEventLevels.SelectedValue as IEventLevel; }
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
      baseLayout.ColumnStyles[1].Width = context.ViewMode ? 0 : 30;
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      listEventLevels.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IEventLevel> editedEl = data.Get<IList<IEventLevel>>(Item.EDITED_LEVELS);
          foreach (IEventLevel el in editedEl) {
            string txt = el.NameOrTranslation + " (priority " + el.Priority + ")";
            if (IsUsed(el)) {
              listEventLevels.AddItem(txt, el, el.Priority, Color.Blue, false, false);
            }
            else {
              listEventLevels.AddItem(txt, el, el.Priority);
            }
          }
          IList<IEventLevel> addedEl = data.Get<IList<IEventLevel>>(Item.ADDED_LEVELS);
          foreach (IEventLevel el in addedEl) {
            listEventLevels.AddItem(el.Name + " (priority " + el.Priority + ")", el, el.Priority);
          }
        }
      }
      
      listEventLevels.SelectedValue = data.Get<IEventLevel>(Item.CURRENT_LEVEL);
      if (data.Get<bool>(Item.EDITED)) {
        EmitProtectAgainstQuit (true);
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (m_toDelete) {
        if (data.Get<IList<IEventLevel>>(Item.EDITED_LEVELS).Remove(CurrentEventLevel)) {
          data.Get<IList<IEventLevel>>(Item.DELETED_LEVELS).Add(CurrentEventLevel);
        }
        else {
          data.Get<IList<IEventLevel>>(Item.ADDED_LEVELS).Remove(CurrentEventLevel);
        }

        m_toDelete = false;
        data.Store(Item.CURRENT_LEVEL, null);
        data.Store(Item.EDITED, true);
      } else {
        data.Store(Item.CURRENT_LEVEL, CurrentEventLevel);
      }
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
      
      // All names must be different or null
      IList<string> names = new List<string>();
      IList<IEventLevel> levels = data.Get<IList<IEventLevel>>(Item.EDITED_LEVELS);
      bool ok = true;
      foreach (IEventLevel level in levels) {
        if (!string.IsNullOrEmpty(level.Name)) {
          if (names.Contains(level.Name)) {
            ok = false;
            break;
          } else {
            names.Add(level.Name);
          }
        }
      }
      if (ok) {
        levels = data.Get<IList<IEventLevel>>(Item.ADDED_LEVELS);
        foreach (IEventLevel level in levels) {
          if (!string.IsNullOrEmpty(level.Name)) {
            if (names.Contains(level.Name)) {
              ok = false;
              break;
            } else {
              names.Add(level.Name);
            }
          }
        }
      }
      if (!ok) {
        errors.Add("all names must be different");
      }

      return errors;
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      IList<IEventLevel> elAdded = data.Get<IList<IEventLevel>>(Item.ADDED_LEVELS);
      IList<IEventLevel> elEdited = data.Get<IList<IEventLevel>>(Item.EDITED_LEVELS);
      IList<IEventLevel> elRemoved = data.Get<IList<IEventLevel>>(Item.DELETED_LEVELS);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          if (ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll().Count != elEdited.Count + elRemoved.Count) {
            throw new StaleException("EventLevels have been deleted or removed");
          }
        }
      }
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // Deleted levels
          foreach (IEventLevel el in elRemoved) {
            ModelDAOHelper.DAOFactory.EventLevelDAO.Lock(el);
            ModelDAOHelper.DAOFactory.EventLevelDAO.MakeTransient(el);
          }
          
          // Edited levels
          foreach (IEventLevel el in elEdited) {
            if (el.Name == "") {
              el.Name = null;
            }

            ModelDAOHelper.DAOFactory.EventLevelDAO.MakePersistent(el);
          }
          
          // New levels
          foreach (IEventLevel el in elAdded) {
            ModelDAOHelper.DAOFactory.EventLevelDAO.MakePersistent(el);
          }

          // Submit changes
          transaction.Commit();
        }
      }
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
    
    public bool IsUsed(IEventLevel eventLevel)
    {
      bool used = false;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          used =
            ModelDAOHelper.DAOFactory.EventCncValueConfigDAO.GetLevels().Contains(eventLevel) ||
            ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.GetLevels().Contains(eventLevel) ||
            ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.GetLevels().Contains(eventLevel);
        }
      }
      
      return used;
    }
    #endregion // Page methods
    
    #region Event reactions
    void ButtonEditClick(object sender, EventArgs e)
    {
      if (CurrentEventLevel != null) {
        EmitDisplayPageEvent("Page2", null);
      } else {
        MessageBoxCentered.Show(this, "Please select an event level first.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }
    
    void ButtonDeleteClick(object sender, EventArgs e)
    {
      if (CurrentEventLevel != null) {
        if (MessageBoxCentered.Show(this, "Are you sure you want to delete this event?", "Question",
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
          m_toDelete = true;
          EmitDisplayPageEvent("Page1", null);
        }
      } else {
        MessageBoxCentered.Show(this, "Please select an event level first.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      listEventLevels.SelectedIndex = -1;
      EmitDisplayPageEvent("Page2", null);
    }
    
    void ListEventLevelsItemDoubleClicked(string arg1, object arg2)
    {
      ButtonEditClick(null, null);
    }
    
    void ListEventLevelsItemChanged(string arg1, object arg2)
    {
      buttonEdit.Enabled = (arg2 != null);
      buttonDelete.Enabled = (arg2 != null && !IsUsed(CurrentEventLevel));
    }
    #endregion // Event reactions
  }
}
