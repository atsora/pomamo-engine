// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Settings;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    bool m_byDepartment = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Goal assignment"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "This page is shown if you never chose how to assign the goals, " +
          "or if the goals already configured cannot determine how to assign them " +
          "(for example you just added a rule on the company or on a machine - which is common to both categorizations).\n\n" +
          "The goals can be assigned according to two ways:\n" +
          " - by company, department, cell and machine,\n" +
          " - by company, category, subcategory, machine.\n\n" +
          "Choose the option that suits you best."; } }
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
      // Nothing to load
      EmitProtectAgainstQuit(false);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.ASSIGNMENT_BY_DEPARTMENT, m_byDepartment);
    }
    #endregion // Page methods
    
    #region Event reactions
    void ButtonDepartmentClick(object sender, EventArgs e)
    {
      m_byDepartment = true;
      EmitDisplayPageEvent("Page3", null);
    }
    
    void ButtonCategoryClick(object sender, EventArgs e)
    {
      m_byDepartment = false;
      EmitDisplayPageEvent("Page3", null);
    }
    #endregion // Event reactions
  }
}
