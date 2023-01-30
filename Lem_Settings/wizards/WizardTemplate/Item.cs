// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardTemplate
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal static readonly string LINE_NAME = "line_name";

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title => "Wizard";
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description => "This item serves as a template for all wizards.";

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords => new string[] {
      "templates"
    };

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName => "wizard";

    /// <summary>
    /// Default category
    /// </summary>
    public override string Category => "Templates";

    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory => "";
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(Int32)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages => new List<IWizardPage> () {
      new Page1 (),
      new Page2 (),
      new Page3 ()
    };
    #endregion // Getters / Setters

    #region Wizard methods
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
      data.InitValue(LINE_NAME, typeof(string), "", true);
      
      // Specific data for page 1
      data.CurrentPageName = "Page1";
      
      return data;
    }
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (var transaction = session.BeginTransaction()) {
          
          
          transaction.Commit ();
        }
      }
    }
    #endregion // Wizard methods
  }
}
