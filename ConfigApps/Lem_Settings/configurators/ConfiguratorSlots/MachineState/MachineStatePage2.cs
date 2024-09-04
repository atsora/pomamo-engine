// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.MachineState
{
  /// <summary>
  /// Description of MachineStatePage2.
  /// </summary>
  internal partial class MachineStatePage2 : UserControl, IPartPage2
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Choose machines"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose here the different machines whose state template " +
          "will be displayed and possibly edited."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStatePage2() : base()
    {
      InitializeComponent();
      displayableTreeView.AddOrder("Sort by category", new string[] {"Company", "Category"});
      displayableTreeView.AddOrder("Sort by department", new string[] {"Company", "Department"});
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    public void Initialize()
    {
      // Fill machines
      displayableTreeView.ClearElements();
      IList<IMachine> machines;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAllNotObsolete();
      }

      foreach (IMachine machine in machines) {
        displayableTreeView.AddElement(machine);
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      displayableTreeView.SelectedOrder = data.Get<int>(MachineStateItem.TREE_DISPLAY_ORDER);
      
      IList<object> items = data.Get<List<object>>(AbstractItem.SELECTED_ITEMS);
      IList<IDisplayable> elements = new List<IDisplayable>();
      foreach (object item in items) {
        elements.Add(item as IDisplayable);
      }

      displayableTreeView.SelectedElements = elements;
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(MachineStateItem.TREE_DISPLAY_ORDER, displayableTreeView.SelectedOrder);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (displayableTreeView.SelectedElements.Count == 0) {
        errors.Add("at least one element must be selected");
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
    public void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      IList<IDisplayable> elements = displayableTreeView.SelectedElements;
      IList<object> items = new List<object>();
      foreach (IDisplayable element in elements) {
        items.Add(element);
      }

      data.Store(AbstractItem.SELECTED_ITEMS, items);
      
      // The timelines need to be updated
      data.Store(AbstractItem.TIMELINE_UPDATE, true);
    }
    #endregion // Page methods
  }
}
