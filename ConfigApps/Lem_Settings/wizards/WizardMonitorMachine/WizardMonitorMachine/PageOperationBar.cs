// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageOperationBar.
  /// </summary>
  internal partial class PageOperationBar : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Operation bar"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Check the box if you want the machine operations are visible in the operation bar."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageOperationBar ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      checkOperationBar.Checked = data.Get<bool> (Item.OPERATION_BAR);
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (Item.OPERATION_BAR, checkOperationBar.Checked);
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data)
    {
      return "PageComputer";
    }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IList<string> summary = new List<string> ();

      if (data.Get<bool> (Item.OPERATION_BAR)) {
        summary.Add ("machine operations displayed");
      }
      else {
        summary.Add ("machine operations NOT displayed");
      }

      return summary;
    }
    #endregion // Page methods
  }
}
