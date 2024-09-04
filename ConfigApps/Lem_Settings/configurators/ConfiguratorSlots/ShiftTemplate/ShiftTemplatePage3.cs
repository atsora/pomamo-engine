// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.ShiftTemplate
{
  /// <summary>
  /// Description of ShiftTemplatePage3.
  /// </summary>
  internal partial class ShiftTemplatePage3 : UserControl, IPartPage3
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
          "you can add here a new slot of a shift template.\n\n" +
          "Select the shift template you want to apply and then validate. " +
          "You can force the association.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public IList<Type> EditableTypes { get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IShiftTemplate));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftTemplatePage3() : base()
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
          // Shift template list
          listShiftTemplate.ClearItems();
          IList<IShiftTemplate> templates = ModelDAOHelper.DAOFactory.ShiftTemplateDAO.FindAll();
          foreach (IShiftTemplate template in templates) {
            listShiftTemplate.AddItem(template.Display, template);
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
      if (listShiftTemplate.ContainsObject(data.Get<IShiftTemplate>(ShiftTemplateItem.SHIFT_TEMPLATE))) {
        listShiftTemplate.SelectedValue = data.Get<IShiftTemplate>(ShiftTemplateItem.SHIFT_TEMPLATE);
      }
      else {
        listShiftTemplate.SelectedIndex = 0;
      }

      checkForceAssociation.Checked = data.Get<bool>(ShiftTemplateItem.FORCE_ASSOCIATION);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(ShiftTemplateItem.SHIFT_TEMPLATE, listShiftTemplate.SelectedValue);
      data.Store(ShiftTemplateItem.FORCE_ASSOCIATION, checkForceAssociation.Checked);
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
      IShiftTemplate shiftTemplate = data.Get<IShiftTemplate>(ShiftTemplateItem.SHIFT_TEMPLATE);
      ModelDAOHelper.DAOFactory.ShiftTemplateDAO.Lock(shiftTemplate);
      
      // Creation of the association with its attributes
      IShiftTemplateAssociation assoc =
        ModelDAOHelper.ModelFactory.CreateShiftTemplateAssociation(shiftTemplate,
          data.Get<DateTime>(AbstractItem.PERIOD_START).ToUniversalTime());
      if (data.Get<bool>(AbstractItem.PERIOD_HAS_END)) {
        assoc.End = data.Get<DateTime>(AbstractItem.PERIOD_END).ToUniversalTime();
      }

      assoc.Force = data.Get<bool>(ShiftTemplateItem.FORCE_ASSOCIATION);
      ModelDAOHelper.DAOFactory.ShiftTemplateAssociationDAO.MakePersistent(assoc);
      
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
