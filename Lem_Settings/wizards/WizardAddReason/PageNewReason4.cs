// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;

namespace WizardAddReason
{
  /// <summary>
  /// Description of PageNewReason4.
  /// </summary>
  internal partial class PageNewReason4 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Link with operations"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select here the way the reason is linked to an operation. " +
          "Three options are available as explained.\n\nIf you don't know, select the first choice."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageNewReason4()
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
      switch (data.Get<LinkDirection>(Item.REASON_OPERATION_DIRECTION)) {
        case LinkDirection.None:
          radioNone.Checked = true;
          break;
        case LinkDirection.Right:
          radioRight.Checked = true;
          break;
        case LinkDirection.Left:
          radioLeft.Checked = true;
          break;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (radioNone.Checked) {
        data.Store(Item.REASON_OPERATION_DIRECTION, LinkDirection.None);
      }
      else if (radioRight.Checked) {
        data.Store(Item.REASON_OPERATION_DIRECTION, LinkDirection.Right);
      }
      else {
        data.Store(Item.REASON_OPERATION_DIRECTION, LinkDirection.Left);
      }
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page2";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      switch (data.Get<LinkDirection>(Item.REASON_OPERATION_DIRECTION)) {
        case LinkDirection.None:
          summary.Add("no changes regarding the previous and following operation periods");
          break;
        case LinkDirection.Right:
          summary.Add("the period of the operation to come will be extended to include this reason");
          break;
        case LinkDirection.Left:
          summary.Add("the period of the previous operation will be extended to include this reason");
          break;
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
