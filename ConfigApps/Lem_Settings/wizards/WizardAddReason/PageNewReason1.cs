// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardAddReason
{
  /// <summary>
  /// Description of PageNewReason1.
  /// </summary>
  internal partial class PageNewReason1 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Identification of the new reason"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the name of the new reason to create. " +
          "A description and a code can be added (the description is recommanded)."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageNewReason1()
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
      textName.Text = data.Get<string>(Item.REASON_NAME);
      textCode.Text = data.Get<string>(Item.REASON_CODE);
      richTextDescription.Text = data.Get<string>(Item.REASON_DESCRIPTION);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.REASON_NAME, textName.Text);
      data.Store(Item.REASON_CODE, textCode.Text);
      data.Store(Item.REASON_DESCRIPTION, richTextDescription.Text);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (String.IsNullOrEmpty(data.Get<string>(Item.REASON_NAME))) {
        errors.Add("the reason name must be specified");
      }

      if (!String.IsNullOrEmpty(data.Get<string>(Item.REASON_CODE))) {
        string code = data.Get<string>(Item.REASON_CODE);
        string name = data.Get<string>(Item.REASON_NAME);
        bool codePresent = false;
        bool namePresent = false;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IReason> reasons = ModelDAOHelper.DAOFactory.ReasonDAO.FindAll();
            foreach (IReason reason in reasons) {
              if (reason.Code == code) {
                codePresent = true;
              }

              if (reason.Name == name) {
                namePresent = true;
              }
            }
          }
        }
        
        if (codePresent) {
          errors.Add("the code is already taken by another reason");
        }

        if (namePresent) {
          errors.Add("the name is already taken by another reason");
        }
      }
      
      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "PageNewReason3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      summary.Add(String.Format("name: \"{0}\"", data.Get<string>(Item.REASON_NAME)));
      summary.Add(String.Format("description: \"{0}\"", data.Get<string>(Item.REASON_DESCRIPTION)));
      
      return summary;
    }
    #endregion // Page methods
  }
}
