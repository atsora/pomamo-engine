// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    internal const string GOAL_TYPE = "goal_type";
    internal const string ASSIGNMENT_BY_DEPARTMENT = "assignment_by_department";
    internal const string GOALS = "goals";
    internal const string INITIAL_GOALS = "initial_goals";
    internal const string GOALS_AFTER_RESET = "goals_after_reset";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Goals"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Goals are used to create reports about the performance / use of the machines over a period.\n\n" +
          "Establish the rules defining the machine goals " +
          "according to the company / department / cell / machine / machine state or " +
          "company / category / subcategory / machine / machine state.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines", "goals", "objectives", "aims", "targets", "performances" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "icon"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Performance"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IGoal)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IGoalType)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineObservationState)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IConfiguratorPage> Pages {
      get {
        IList<IConfiguratorPage> pages = new List<IConfiguratorPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        pages.Add(new PageReset());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Configurator methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      data.InitValue(GOAL_TYPE, typeof(IGoalType), null, true);
      data.InitValue(ASSIGNMENT_BY_DEPARTMENT, typeof(bool), false, true);
      data.InitValue(GOALS, typeof(GoalManager), new GoalManager(), true);
      
      // Data specific for Page3
      data.CurrentPageName = "Page3";
      data.InitValue(INITIAL_GOALS, typeof(IList<IGoal>), new List<IGoal>(), false);
      
      // Data specific for PageReset
      data.CurrentPageName = "PageReset";
      data.InitValue(GOALS_AFTER_RESET, typeof(IDictionary<ICompany, double>),
                     new Dictionary<ICompany, double>(), true);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
