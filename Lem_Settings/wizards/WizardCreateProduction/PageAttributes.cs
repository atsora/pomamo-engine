// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lemoine.Settings;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of PageAttributes.
  /// </summary>
  internal partial class PageAttributes : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Production target and name"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Here you can specify the global target and the name of a production. " +
          "If several productions are created, they will all have the same target.\n\n" +
          "The name can be made of a pattern, as explained."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageAttributes()
    {
      InitializeComponent();
      labelDescription.Text = "Several fields are available to automatically generate the name:\n\n" +
        "\t<%L%>\tline name\n" +
        "\t<%Y%>\tstart production year\n" +
        "\t<%M%>\tstart production month\n" +
        "\t<%W%>\tstart production week\n" +
        "\t<%D%>\tstart production day\n" +
        "\t<%R%>\tnumber of the recurrence, if any\n\n" +
        "\"Line 2015 W15\" could be produced by \"<%L%> <%Y%> W<%W%>\"";
      
      // Add default inputs
      comboName.AddItem("<%L%> <%Y%> W<%W%>");
      comboName.AddItem("<%L%> <%M%>-<%D%>-<%Y%>");
      comboName.AddItem("<%L%> - name #<%R%>");
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
      // Other information
      comboName.SelectedText = data.Get<string>(Item.PRODUCTION_NAME);
      numericQuantity.Value = data.Get<int>(Item.PRODUCTION_QUANTITY);
      numericQuantity.Text = ((int)numericQuantity.Value).ToString();
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.PRODUCTION_NAME, comboName.SelectedText);
      data.Store(Item.PRODUCTION_QUANTITY, (int)numericQuantity.Value);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // Validity of the pattern
      if (data.Get<string>(Item.PRODUCTION_NAME) == "") {
        errors.Add("the name cannot be empty");
      }
      else {
        Regex pattern = new Regex("<%([^<%>]*)%>");
        MatchCollection matches = pattern.Matches(data.Get<string>(Item.PRODUCTION_NAME));
        foreach (Match match in matches) {
          string subStr = match.Groups[1].Value;
          if (subStr.Length != 1 || (subStr[0] != 'L' && subStr[0] != 'Y' &&
                                     subStr[0] != 'M' && subStr[0] != 'D' &&
                                     subStr[0] != 'W' && subStr[0] != 'R')) {
            errors.Add("invalid field (see the help provided)");
            break;
          }
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
      return null;
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
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      summary.Add(data.Get<string>(Item.PRODUCTION_NAME));
      return summary;
    }
    #endregion // Page methods
  }
}
