// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string LINE = "line";
    internal const string WOS = "workorders";
    internal const string QUANTITIES = "quantities";
    internal const string CHANGE_GLOBAL = "change_global_quantity";
    internal const string GLOBAL_AUTO = "global_auto";
    internal const string GLOBAL_QUANTITY = "global_quantity";
    internal const string PRODUCTIONS_CHANGED = "productions_changed";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Change produced quantities"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "This wizard allows changing the quantities of parts " +
          "produced by the different operations of an existing production period. " +
          "Shifts / days may be added or removed.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "productions", "shifts", "targets", "quantities", "quantity",
          "parts", "periods" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Production line"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Production periods"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IWorkOrderLine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IWorkOrder)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IIntermediateWorkPieceTarget)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new Page0());
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        return pages;
      }
    }
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
      data.InitValue(LINE, typeof(ILine), null, true);
      data.InitValue(WOS, typeof(List<IWorkOrder>), new List<IWorkOrder>(), true);
      data.InitValue(CHANGE_GLOBAL, typeof(bool), true, true);
      data.InitValue(GLOBAL_AUTO, typeof(bool), true, false);
      data.InitValue(GLOBAL_QUANTITY, typeof(int), -1, true);
      data.InitValue(QUANTITIES, typeof(Quantities), new Quantities(), true);
      data.InitValue(PRODUCTIONS_CHANGED, typeof(bool), true, false);
      
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
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // Change the quantities in intermediate work piece summaries
          data.Get<Quantities>(QUANTITIES).SaveModifications();
          
          // Change the global target if needed
          if (data.Get<bool>(CHANGE_GLOBAL)) {
            ILine line = data.Get<ILine>(LINE);
            ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
            foreach (IWorkOrder wo in data.Get<IList<IWorkOrder>>(WOS)) {
              ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock(wo);
              IWorkOrderLine wol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(line, wo);
              wol.Quantity = data.Get<int>(GLOBAL_QUANTITY);
            }
          }
          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
  }
}
