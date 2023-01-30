// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls.List;
using Lemoine.Model;

namespace ConfiguratorMachineCategories
{
  /// <summary>
  /// Description of Page.
  /// </summary>
  internal partial class Page : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    string m_elementName;
    Container m_container = null;
    bool m_modified = false;
    bool m_viewMode = false;
    #endregion // Members
    
    #region Events
    /// <summary>
    /// Emitted when the validation is required
    /// </summary>
    public event Action<Container> OnValidate;
    
    /// <summary>
    /// Emitted when the list has to be filled
    /// </summary>
    public event Action<ListTextValue, Container.Element> OnFillMachines;
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title {
      get {
        if (m_viewMode) {
          return "View " + m_elementName.ToLower();
        }
        else {
          return "Edit " + m_elementName.ToLower();
        }
      }
    }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        string txt = "The list on the left side contains all existing " + m_elementName.ToLower() + ".";
        if (!m_viewMode) {
          txt += " You can reorder / edit / remove them, or create new ones. " +
            "By editing " + m_elementName.ToLower() + " you can change their name and their code.";
        }

        txt += "\n\nThe list on the right side, after an element has been selected, " +
          "comprises all corresponding machines.";
        return txt;
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
    
    int CurrentIndex {
      get { return listElements.SelectedIndex; }
      set { listElements.SelectedIndex = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="plural">name in plural, first letter uppercase</param>
    public Page(string plural)
    {
      m_elementName = plural;
      InitializeComponent();
      label.Text = plural;
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
      
      if (m_viewMode) {
        baseLayout.ColumnStyles[1].Width = 3;
        tableLayoutPanel.Hide();
      } else {
        baseLayout.ColumnStyles[1].Width = 34;
        tableLayoutPanel.Show();
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_container = data.Get<Container>(ItemCommon.ELEMENTS);
      
      listElements.ClearItems();
      foreach (Container.Element element in m_container.Elements) {
        string txt = element.Name;
        if (!String.IsNullOrEmpty(element.Code)) {
          txt += " (" + element.Code + ")";
        }

        listElements.AddItem(txt, element);
      }
      
      // Restore the selection in the list
      int position = data.Get<int>(ItemCommon.POSITION);
      if (position < 0 && listElements.Count > 0) {
        position = 0;
      }

      if (position < listElements.Count) {
        CurrentIndex = position;
      }
      else if (position - 1 >= 0) {
        CurrentIndex = position - 1;
      }
      else {
        CurrentIndex = -1;
        ListElementsItemChanged(null, null);
      }
      
      if (m_modified) {
        EmitProtectAgainstQuit (true);
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(ItemCommon.POSITION, CurrentIndex);
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
      
      // Codes must be different
      if (!m_container.AreCodesUnique) {
        errors.Add("all codes must be different");
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
      OnValidate(data.Get<Container>(ItemCommon.ELEMENTS));
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
    
    #region Event reactions
    void ButtonTopClick(object sender, EventArgs e)
    {
      m_modified = true;
      m_container.Top(CurrentIndex);
      CurrentIndex = 0;
      EmitDisplayPageEvent("Page", null);
    }
    
    void ButtonUpClick(object sender, EventArgs e)
    {
      m_modified = true;
      m_container.Up(CurrentIndex);
      CurrentIndex--;
      EmitDisplayPageEvent("Page", null);
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      var dialog = new CodeNameDialog();
      dialog.Text = "Add a new element";
      dialog.ShowDialog();
      
      if (dialog.DialogResult == DialogResult.OK) {
        m_modified = true;
        m_container.AddElement(0, dialog.ElementName, dialog.ElementCode);
        dialog.Dispose();
        CurrentIndex = 0;
        EmitDisplayPageEvent("Page", null);
      }
    }
    
    void ButtonEditClick(object sender, EventArgs e)
    {
      var element = listElements.SelectedValue as Container.Element;
      if (element == null) {
        return;
      }

      var dialog = new CodeNameDialog();
      dialog.Text = "Edit an existing element";
      dialog.ElementName = element.Name;
      dialog.ElementCode = element.Code;
      dialog.ShowDialog();
      
      if (dialog.DialogResult == DialogResult.OK) {
        m_modified = true;
        m_container.Set(CurrentIndex, dialog.ElementName, dialog.ElementCode);
        dialog.Dispose();
        EmitDisplayPageEvent("Page", null);
      }
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      m_modified = true;
      m_container.Delete(CurrentIndex);
      EmitDisplayPageEvent("Page", null);
    }
    
    void ButtonDownClick(object sender, EventArgs e)
    {
      m_modified = true;
      m_container.Down(CurrentIndex);
      CurrentIndex++;
      EmitDisplayPageEvent("Page", null);
    }
    
    void ButtonBottomClick(object sender, EventArgs e)
    {
      m_modified = true;
      m_container.Bottom(CurrentIndex);
      CurrentIndex = listElements.Count - 1;
      EmitDisplayPageEvent("Page", null);
    }
    
    void ListElementsItemChanged(string arg1, object arg2)
    {
      int currentIndex = CurrentIndex;
      if (currentIndex == -1) {
        buttonTop.Enabled = buttonUp.Enabled = false;
        buttonEdit.Enabled = buttonRemove.Enabled = false;
        buttonDown.Enabled = buttonBottom.Enabled = false;
        listMachines.ClearItems();
      } else {
        buttonTop.Enabled = buttonUp.Enabled = (currentIndex > 0);
        buttonEdit.Enabled = buttonRemove.Enabled = true;
        buttonDown.Enabled = buttonBottom.Enabled = (currentIndex < listElements.Count - 1);
        OnFillMachines(listMachines, listElements.SelectedValue as Container.Element);
      }
    }
    #endregion // Event reactions
  }
}
