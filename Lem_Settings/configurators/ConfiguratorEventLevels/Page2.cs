// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorEventLevel
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    bool m_viewMode;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Level properties"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose here the name of the level, and its priority.\n\n" +
          "0 is the highest priority and 1000 is the lowest.\n\nThe name of generic event levels is not editable."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION |
          LemSettingsGlobal.PageFlag.DONT_LOG_VALIDATION |
          LemSettingsGlobal.PageFlag.WITH_VALIDATION;
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
    public void Initialize(ItemContext context)
    {
      m_viewMode = context.ViewMode;
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      IEventLevel el = data.Get<IEventLevel>(Item.CURRENT_LEVEL);
      if (el == null) {
        textBox.Text = "new level";
        numeric.Value = 500;
        data.Store(Item.HAS_TRANSLATION_KEY, false);
        textBox.Enabled = true;
      } else {
        textBox.Text = el.NameOrTranslation;
        numeric.Value = el.Priority;
        textBox.Enabled = string.IsNullOrEmpty(el.TranslationKey);
        data.Store(Item.HAS_TRANSLATION_KEY, !string.IsNullOrEmpty(el.TranslationKey));
      }
      if (m_viewMode) {
        numeric.Enabled = textBox.Enabled = false;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.PRIORITY, (int)numeric.Value);
      data.Store(Item.NAME, textBox.Text);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<string>(Item.NAME) == "" && !data.Get<bool>(Item.HAS_TRANSLATION_KEY)) {
        errors.Add("the name cannot be empty");
      }

      if (data.Get<int>(Item.PRIORITY) < 0 || data.Get<int>(Item.PRIORITY) > 1000) {
        errors.Add("the priority must be comprised between 0 (included) and 1000 (included)");
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
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      IEventLevel el = data.Get<IEventLevel>(Item.CURRENT_LEVEL);
      if (el == null) {
        var name = data.Get<string> (Item.NAME);
        el = ModelDAOHelper.ModelFactory.CreateEventLevelFromName (data.Get<int>(Item.PRIORITY), name);
        data.Get<IList<IEventLevel>>(Item.ADDED_LEVELS).Add(el);
        data.Store(Item.CURRENT_LEVEL, el);
      } else {
        el.Name = data.Get<string>(Item.NAME);
        el.Priority = data.Get<int>(Item.PRIORITY);
      }
      
      data.Store(Item.EDITED, true);
    }
    #endregion // Page methods
  }
}
