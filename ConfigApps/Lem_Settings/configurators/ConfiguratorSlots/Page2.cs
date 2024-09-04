// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IPartPage2 m_part = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.DONT_LOG_VALIDATION |
          LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION |
          LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    
    /// <summary>
    /// Enable the step of validation
    /// This provides two different behaviours:
    /// - either each input of the user is directly processed (validation not needed)
    /// - or each input is taken into account during the validation step
    /// </summary>
    public bool WithValidation { get { return true; } }
    
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return m_part.Title; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return m_part.Help; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2(IPartPage2 part) : base()
    {
      InitializeComponent();
      m_part = part;
      this.Controls.Add(m_part as Control);
      (m_part as Control).Dock = DockStyle.Fill;
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
      m_part.Initialize();
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_part.LoadPageFromData(data);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      m_part.SavePageInData(data);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return m_part.GetErrorsBeforeValidation(data);
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
      m_part.Validate(data, ref warnings, ref revisionId);
    }
    #endregion // Page methods
  }
}
