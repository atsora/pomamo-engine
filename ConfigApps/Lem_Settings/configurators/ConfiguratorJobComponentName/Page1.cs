// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorJobComponentName
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    ItemData m_itemData = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Alert list"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Components containing an anomaly in the name are listed here. " +
          "When you click on a component, the corresponding isofiles are listed to the right. " +
          "It is then possible to rename the component and the associated job in the lower part " +
          "of the interface. Click on \"Rename\" each time you want to save the new names."; } }
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
      DisplayElements(GetElementsToRename());
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_itemData = data;
      
      // Update the list if a previous item has been renamed
      if (data.Get<ElementToRename>(Item.ELEMENT_TO_RENAME) != null) {
        listElements.UpdateItemText(data.Get<ElementToRename>(Item.ELEMENT_TO_RENAME),
                                    data.Get<string>(Item.COMPONENT_NAME));
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
    #endregion // Page methods
    
    #region Event reactions
    void ButtonRenameClick(object sender, EventArgs e)
    {
      m_itemData.Store(Item.COMPONENT_NAME, textBoxComponent.Text);
      m_itemData.Store(Item.JOB_NAME, textBoxJob.Text);
      m_itemData.Store(Item.ELEMENT_TO_RENAME, GetSelectedElementToRename());
      
      Rename(m_itemData);
    }
    
    void ListElementsItemChanged(string arg1, object arg2)
    {
      // Check if the previous element has been edited
      
      
      listIsofiles.ClearItems();
      var element = arg2 as ElementToRename;
      if (element != null) {
        // Display the isofiles
        DisplayIsoFiles(element.GetIsoFiles());
        
        // Pre fill the names
        textBoxJob.Text = element.JobName;
        textBoxComponent.Text = element.ComponentName;
      }
    }
    #endregion // Event reactions
    
    #region Private methods
    IList<ElementToRename> GetElementsToRename()
    {
      IList<ElementToRename> elements = new List<ElementToRename>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var cs = ModelDAOHelper.DAOFactory.ComponentDAO.FindByNameStartPattern("?");
          foreach (var c in cs) {
            // Get the corresponding project
            var project = c.Project;
            if (project == null) {
              continue;
            }

            elements.Add(new ElementToRename(project, c));
          }
        }
      }
      
      return elements;
    }
    
    void DisplayElements(IList<ElementToRename> elements)
    {
      listElements.ClearItems();
      foreach (var element in elements) {
        listElements.AddItem(element.ComponentName + " (" + element.Date.ToString("d") + ")", element,
                             element.Date.ToString("yyyy-MM-dd hh:mm:ss") + element.ComponentName);
      }
    }
    
    void DisplayIsoFiles(IDictionary<DateTime, string> isofiles)
    {
      foreach (var elt in isofiles) {
        listIsofiles.AddItem(elt.Value + " (" + elt.Key.ToShortDateString() + ")", elt.Value,
                             elt.Key.ToString("yyyy-MM-dd hh:mm:ss") + elt.Value);
      }
    }
    
    ElementToRename GetSelectedElementToRename()
    {
      return listElements.SelectedValue as ElementToRename;
    }
    
    void Rename(ItemData data)
    {
      // Check errors
      var errors = new List<string>();
      if (String.IsNullOrEmpty(data.Get<string>(Item.COMPONENT_NAME))) {
        errors.Add("component name cannot be null");
      }

      if (String.IsNullOrEmpty(data.Get<string>(Item.JOB_NAME))) {
        errors.Add("job name cannot be null");
      }

      if (data.Get<ElementToRename>(Item.ELEMENT_TO_RENAME) == null) {
        errors.Add("an element must be selected in the list");
      }

      if (errors.Count > 0) {
        EmitDisplayPageEvent ("Page1", errors, false);
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            data.Get<ElementToRename>(Item.ELEMENT_TO_RENAME).SetNames(
              data.Get<string>(Item.JOB_NAME),
              data.Get<string>(Item.COMPONENT_NAME));
            transaction.Commit();
          }
        }
        
        EmitLogAction("Rename", data.ToString(), "ok");
        EmitDisplayPageEvent("Page1", null, false);
      }
    }
    #endregion // Private methods
  }
}
