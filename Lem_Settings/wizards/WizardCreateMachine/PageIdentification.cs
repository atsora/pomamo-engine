// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateMachine
{
  /// <summary>
  /// Description of PageIdentification.
  /// </summary>
  internal partial class PageIdentification : GenericWizardPage, IWizardPage
  { 
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Identification of the machine"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "This page allows you to identify the machine, " +
          "by specifying a name and an optional code."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageIdentification()
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
      textName.Text = data.Get<string>(AbstractItem.MACHINE_NAME);
      textCode.Text = data.Get<string>(AbstractItem.MACHINE_CODE);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.MACHINE_NAME, textName.Text);
      data.Store(AbstractItem.MACHINE_CODE, textCode.Text);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // Empty name
      if (data.Get<string>(AbstractItem.MACHINE_NAME) == "") {
        errors.Add("the name cannot be empty");
      }

      // Code already taken
      bool ok = true;
      string code = data.Get<string>(AbstractItem.MACHINE_CODE);
      var currentMachine = data.Get<IMachine>(AbstractItem.MACHINE);
      if (code != "") {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          if (currentMachine != null) {
            machines.Remove(currentMachine);
          }

          foreach (IMachine machine in machines) {
            if (machine.Code == code) {
              ok = false;
              break;
            }
          }
        }
      }
      if (!ok) {
        errors.Add("the code is already taken by another machine");
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
      return "PageCompany";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<string>(AbstractItem.MACHINE_NAME) == "") {
        summary.Add("Name: -");
      }
      else {
        summary.Add("Name: \"" + data.Get<string>(AbstractItem.MACHINE_NAME) + "\"");
      }

      if (data.Get<string>(AbstractItem.MACHINE_CODE) == "") {
        summary.Add("Code: -");
      }
      else {
        summary.Add("Code: \"" + data.Get<string>(AbstractItem.MACHINE_CODE) + "\"");
      }

      return summary;
    }
    #endregion // Page methods
  }
}
