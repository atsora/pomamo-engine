// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReorderOperation
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Members
    Order m_order = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Operations"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "The different operations of the chosen part are listed. " +
          "You can reorder them with the arrows."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
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
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Load the different operations
      m_order = data.Get<Order>(Item.ORDER);
      
      // Fill the list
      FillAndSelect(0);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Automatically done
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // If the last operation produces several iwps, the final workpiece should be one of them
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IOperation> operations = data.Get<Order>(Item.ORDER).Operations;
          if (operations.Count > 0) {
            IOperation lastOperation = operations[operations.Count - 1];
            ModelDAOHelper.DAOFactory.OperationDAO.Lock(lastOperation);
            IPart part = data.Get<IPart>(Item.PART);
            ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
            
            if (lastOperation.IntermediateWorkPieces != null &&
                lastOperation.IntermediateWorkPieces.Count > 1)
            {
              // Iwps of the part
              var partIwps = new List<IIntermediateWorkPiece>();
              foreach (var ciwp in part.ComponentIntermediateWorkPieces) {
                partIwps.Add(ciwp.IntermediateWorkPiece);
              }

              // Number of final iwp that are associated to the part
              int count = 0;
              foreach (var iwp in lastOperation.IntermediateWorkPieces) {
                if (partIwps.Contains(iwp)) {
                  count++;
                }
              }

              if (count != 1) {
                errors.Add("the last operation has several outputs for the current part " +
                           "and cannot be used at the end of the process");
              }
            }
          }
        }
      }
      
      return errors;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      return data.Get<Order>(Item.ORDER).GetSummary();
    }
    #endregion // Page methods
    
    #region Private methods
    void FillAndSelect(int index)
    {
      listBox.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IOperation operation in m_order.Operations) {
            listBox.AddItem(operation.Display);
          }
        }
      }
      
      if (index < 0) {
        index = 0;
      }
      else if (index >= listBox.Count) {
        index = listBox.Count;
      }

      listBox.SelectedIndex = index;
    }
    #endregion // Private methods
    
    #region Event reactions
    void ListBoxItemChanged(string arg1, object arg2)
    {
      buttonUp.Enabled = (listBox.SelectedIndex > 0);
      buttonDown.Enabled = (listBox.SelectedIndex < listBox.Count - 1);
    }
    
    void ButtonUpClick(object sender, EventArgs e)
    {
      int selectedIndex = listBox.SelectedIndex;
      if (selectedIndex != -1) {
        m_order.Up(selectedIndex);
      }

      FillAndSelect (selectedIndex - 1);
    }
    
    void ButtonDownClick(object sender, EventArgs e)
    {
      int selectedIndex = listBox.SelectedIndex;
      if (selectedIndex != -1) {
        m_order.Down(selectedIndex);
      }

      FillAndSelect (selectedIndex + 1);
    }
    #endregion // Event reactions
  }
}
