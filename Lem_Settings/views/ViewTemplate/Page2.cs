// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ViewTemplate
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericViewPage, IViewPage
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Page with validation"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "This can be a page where parameters are asked to the user. " +
          "It's possible here to validate or cancel the edited configuration.\n" +
          "A button has been added to go back to the main page."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
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
      
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      
    }
    
    /// <summary>
    /// Method called every second
    /// </summary>
    public override void OnTimeOut()
    {
      
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns (not used for views)</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      
    }
    #endregion // Page methods
    
    #region Private methods
    #endregion // Private methods
    
    #region Event reactions
    void ButtonListClick(object sender, EventArgs e)
    {
      // Display the first page
      EmitDisplayPageEvent("Page1", null);
    }
    #endregion // Event reactions
  }
}
