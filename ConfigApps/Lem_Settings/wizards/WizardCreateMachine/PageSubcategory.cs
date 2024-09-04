// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateMachine
{
  /// <summary>
  /// Description of PageSubcategory.
  /// </summary>
  internal partial class PageSubcategory : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Subcategory"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Please select a subcategory for the machine (not mandatory)."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachineSubCategory));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageSubcategory()
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
      list.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Subcategory
          IList<IMachineSubCategory> subcategories = ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.FindAll();
          foreach (IMachineSubCategory subcategorie in subcategories) {
            list.AddItem(subcategorie.Display, subcategorie);
          }
        }
      }
      list.InsertItem("none", null, 0, SystemColors.ControlText, true, false);
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      list.SelectedValue = data.Get<IMachineSubCategory>(AbstractItem.SUBCATEGORY);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.SUBCATEGORY, list.SelectedValue);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data) { return "PageCell"; }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<IMachineSubCategory>(AbstractItem.SUBCATEGORY) == null) {
        summary.Add("none");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IMachineSubCategory subCategory = data.Get<IMachineSubCategory>(AbstractItem.SUBCATEGORY);
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(subCategory);
            summary.Add("\"" + subCategory.Display + "\"");
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
