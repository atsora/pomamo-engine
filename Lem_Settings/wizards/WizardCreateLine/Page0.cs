// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Page0.
  /// </summary>
  internal partial class Page0 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Identification of the line"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Fill here the name and the code of the line.\n\nOnly the name is mandatory. " +
          "If a code is set, it has to be different from all other codes previously set."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page0()
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
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      textBoxName.Text = data.Get<string>(Item.LINE_NAME);
      textBoxCode.Text = data.Get<string>(Item.LINE_CODE);
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.LINE_NAME, textBoxName.Text);
      data.Store(Item.LINE_CODE, textBoxCode.Text);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page1";
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<string>(Item.LINE_NAME) == "") {
        errors.Add("the name of the line cannot be empty");
      }

      // Verify that the code is not already used in the database
      IList<string> existingCodes = new List<string>();
      IList<string> existingPartCodes = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
      {
        if (data.Get<string>(Item.LINE_CODE) != "") {
          IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAll();
          foreach (ILine line in lines) {
            existingCodes.Add(line.Code);
          }
        }
      }
      if (existingCodes.Contains(data.Get<string>(Item.LINE_CODE))) {
        errors.Add("the code \"" + data.Get<string>(Item.LINE_CODE) + "\" is already taken for a line");
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
      // At least one part => removed because the users is induced to create another part
      //if (data.Get<List<StructPart>>(Item.PARTS).Count == 0)
      //  data.Get<List<StructPart>>(Item.PARTS).Add(new StructPart(
      //    data.Get<string>(Item.LINE_NAME), data.Get<string>(Item.LINE_CODE)));
      
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs, which will be displayed in a tree
    /// Each string in the list will be a rootnode. If line breaks are present, all lines
    /// after the first one will be shown as child nodes.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      summary.Add("name: " + data.Get<string>(Item.LINE_NAME));
      if (data.Get<string>(Item.LINE_CODE) == "") {
        summary.Add("no code specified");
      }
      else {
        summary.Add("code: " + data.Get<string>(Item.LINE_CODE));
      }

      return summary;
    }
    #endregion // Page methods
  }
}
