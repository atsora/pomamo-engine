// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Container for view pages
  /// </summary>
  public class ViewGuiCenter : GuiCenter
  {
    static readonly ILog log = LogManager.GetLogger(typeof(ViewGuiCenter).FullName);
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ViewGuiCenter() : base()
    {
      DisplayOk();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current view or configurator in view mode
    /// </summary>
    /// <param name="item"></param>
    /// <param name="otherData"></param>
    public void SetCurrentItem(IItem item, ItemData otherData)
    {
      // Variable initialization
      m_otherData = otherData;
      m_currentItem = item;
      m_itemPages.Clear();
      m_guiLeft.SetCurrentItem(m_currentItem);
      
      // Initialization of the item and its data
      if (m_currentItem is IView) {
        m_itemData = (m_currentItem as IView).Initialize(otherData);
      }
      else {
        m_itemData = (m_currentItem as IConfigurator).Initialize(otherData);
      }

      m_guiLeft.SetCurrentItemData(m_itemData);
      
      // Page list
      IList<IViewPage> listTmp = null;
      if (m_currentItem is IView) {
        listTmp = (m_currentItem as IView).Pages;
      } else if (m_currentItem is IConfigurator) {
        IList<IConfiguratorPage> confPages = (m_currentItem as IConfigurator).Pages;
        listTmp = new List<IViewPage>();
        foreach (IConfiguratorPage page in confPages) {
          listTmp.Add(page as IViewPage);
        }
      }
      foreach (IViewPage page in listTmp) {
        page.Initialize(m_currentItem.Context);
        page.SetTitle += OnSetTitle;
        page.SpecifyHeader += OnSpecifyHeader;
        page.DisplayPageEvent += OnDisplayPage;
        
        var control = page as UserControl;
        control.Dock = DockStyle.Fill;
        control.Padding = new Padding(0);
        
        m_itemPages.Add(page.GetType().Name, page);
      }
      m_currentPages.Add(listTmp[0]);
      DisplayLastPage();
    }
    
    /// <summary>
    /// Reinit the item displayed
    /// </summary>
    public override void Reinit()
    {
      IItem view = m_currentItem;
      ItemData data = m_otherData;
      EndView();
      SetCurrentItem(view, data);
    }
    
    /// <summary>
    /// Show or hide the buttons depending on the page and its configuration
    /// </summary>
    protected override void PrepareButtons()
    {
      if ((GetLastPage().Flags & LemSettingsGlobal.PageFlag.WITH_VALIDATION) != 0 &&
          m_currentPages.Count > 1) {
        DisplayCancel();
        SetLeftButtonVisible(true);
        SetRightButtonVisible(true);
      } else {
        DisplayPrevious();
        SetRightButtonVisible(false);
        SetLeftButtonVisible(m_currentPages.Count > 1);
      }
    }
    
    void EndView()
    {
      ProtectAgainstQuit = false;
      m_currentItem = null;
      m_currentPages.Clear();
      m_itemData = null;
      m_otherData = null;
      RemovePage();
      while (m_itemPages.Keys.Count > 0) {
        string key = m_itemPages.Keys.First();
        var page = m_itemPages[key];
        m_itemPages.Remove(key);
        page.Dispose();
      }
    }
    
    IViewPage GetLastPage()
    {
      return m_currentPages.Last() as IViewPage;
    }
    #endregion // Methods
    
    #region Event reactions
    void OnDisplayPage(string pageName, IList<string> errors, bool ignorePossible)
    {
      if (errors != null && errors.Count > 0) {
        if (!DisplayErrors(errors, ignorePossible)) {
          return;
        }
      }

      if (!m_itemPages.ContainsKey(pageName)) {
        errors = new List<string>();
        string text = "the page \"" + pageName + "\" couldn't be found";
        errors.Add(text);
        log.Error(text);
        DisplayErrors(errors, false);
        return; // Even if the error is ignored
      }
      
      GetLastPage().SavePageInData(m_itemData);
      var pageToDisplay = m_itemPages[pageName];
      int position = m_currentPages.IndexOf(pageToDisplay);
      if (position == -1) {
        m_currentPages.Add(pageToDisplay);
      }
      else {
        m_currentPages = m_currentPages.Take(position + 1).ToList();
      }

      DisplayLastPage ();
    }
    
    /// <summary>
    /// Method called when the right button is clicked
    /// </summary>
    protected override void OnRightButtonClick()
    {
      // Save the current page
      var lastPage = GetLastPage();
      lastPage.SavePageInData(m_itemData);

      // Display errors if any. Return if they are not ignored.
      IList<string> errors = lastPage.GetErrorsBeforeValidation(m_itemData);
      if (errors != null && errors.Any ()) {
        if (!DisplayErrors(errors, (lastPage.Flags &
                                    LemSettingsGlobal.PageFlag.IGNORE_IMPOSSIBLE) == 0)) {
          return;
        }
      }

      IList<string> warnings = new List<string>();
      int revisionId = 0;

      // If an exception occurs, the application continues to run
      MainForm.QuitIfException = false;
      lastPage.Validate(m_itemData, ref warnings, ref revisionId);
      MainForm.QuitIfException = true;
      
      // Process after validation
      lastPage.ProcessAfterValidation(m_itemData);
      
      // Display warnings
      if (warnings.Count > 0) {
        MessageBoxCentered.Show(this.Parent, String.Join("\n\n", warnings.ToArray()), "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // Change the page or end the view
      string nextPage = lastPage.GetPageAfterValidation(m_itemData);
      if (String.IsNullOrEmpty(nextPage)) {
        m_currentPages.RemoveAt(m_currentPages.Count() - 1);
      }
      else {
        var pageToDisplay = m_itemPages[nextPage];
        int position = m_currentPages.IndexOf(pageToDisplay);
        if (position == -1) {
          m_currentPages.RemoveAt(m_currentPages.Count() - 1);
        }
        else {
          m_currentPages = m_currentPages.Take(position + 1).ToList();
        }
      }
      
      DisplayLastPage();
    }
    
    /// <summary>
    /// Method called when the left button is clicked
    /// </summary>
    protected override void OnLeftButtonClick()
    {
      GetLastPage().SavePageInData(m_itemData);
      m_currentPages.RemoveAt(m_currentPages.Count() - 1);
      if (m_currentPages.Count > 0) {
        DisplayLastPage ();
      }
      else {
        EndView();
        EmitItemFinished();
      }
    }
    
    /// <summary>
    /// Method called when the button "home" is clicked
    /// </summary>
    protected override void OnButtonHomeClick()
    {
      EndView();
      EmitItemFinished();
    }
    #endregion // Event reactions
  }
}
