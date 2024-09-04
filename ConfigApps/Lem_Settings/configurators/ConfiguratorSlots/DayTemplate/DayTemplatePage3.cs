// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.DayTemplate
{
  /// <summary>
  /// Description of DayTemplatePage3.
  /// </summary>
  internal partial class DayTemplatePage3 : UserControl, IPartPage3
  {
    #region Getters / Setters
    /// <summary>
    /// Name of the category of element, plural with uppercase for the first letter
    /// Ex: "Machines"
    /// </summary>
    public string ElementName { get { return ""; } }
    
    /// <summary>
    /// Description of the page
    /// </summary>
    public string Description { get {
        return "After having selected the period in the previous page, " +
          "you can add here a new slot of a day template.\n\n" +
          "Select the day template you want to apply and then validate.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public IList<Type> EditableTypes { get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IDayTemplate));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DayTemplatePage3() : base()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    public void Initialize()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Day template list
          listDayTemplate.ClearItems();
          IList<IDayTemplate> templates = ModelDAOHelper.DAOFactory.DayTemplateDAO.FindAll();
          foreach (IDayTemplate template in templates) {
            listDayTemplate.AddItem(template.Display, template);
          }
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Properties
      if (listDayTemplate.ContainsObject(data.Get<IDayTemplate>(DayTemplateItem.DAY_TEMPLATE))) {
        listDayTemplate.SelectedValue = data.Get<IDayTemplate>(DayTemplateItem.DAY_TEMPLATE);
      }
      else {
        listDayTemplate.SelectedIndex = 0;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(DayTemplateItem.DAY_TEMPLATE, listDayTemplate.SelectedValue);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Prepare a modification to save
    /// This method is within a transaction
    /// </summary>
    /// <param name="data"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public IModification PrepareModification(ItemData data, object item)
    {
      IDayTemplate dayTemplate = data.Get<IDayTemplate>(DayTemplateItem.DAY_TEMPLATE);
      ModelDAOHelper.DAOFactory.DayTemplateDAO.Lock(dayTemplate);
      
      // Creation of the association with its attributes
      IDayTemplateChange assoc =
        ModelDAOHelper.ModelFactory.CreateDayTemplateChange(dayTemplate,
                                                            data.Get<DateTime>(AbstractItem.PERIOD_START).ToUniversalTime());
      if (data.Get<bool>(AbstractItem.PERIOD_HAS_END)) {
        assoc.End = data.Get<DateTime>(AbstractItem.PERIOD_END).ToUniversalTime();
      }
      ModelDAOHelper.DAOFactory.DayTemplateChangeDAO.MakePersistent(assoc);
      
      return assoc;
    }
    
    /// <summary>
    /// Get the name of the elements that are going to be modified
    /// </summary>
    /// <param name="items"></param>
    /// <returns>list of name, may be null</returns>
    public IList<string> GetElementName(IList<object> items)
    {
      return null;
    }
    #endregion // Methods
  }
}
