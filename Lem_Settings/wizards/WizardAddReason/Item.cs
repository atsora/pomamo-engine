// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardAddReason
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string REASON_NAME = "reason_name";
    internal const string REASON_CODE = "reason_code";
    internal const string REASON_DESCRIPTION = "reason_description";
    internal const string REASON_GROUP = "reason_group";
    internal const string REASON_OPERATION_DIRECTION = "reason_operation_direction";
    internal const string MACHINE_MODES = "machine_modes";
    internal const string MOSS = "machine_observation_states";
    internal const string DETAILS_REQUIRED = "details_required";
    internal const string MACHINE_FILTER = "machine_filter";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Add a reason"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Create a new reason and add it as a selectable reason to " +
          "a set of machine modes (what is detected) and machine observation states " +
          "(what is planned and expected).";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "reasons", "machines", "observations", "states", "modes" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Status"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IReason)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IReasonSelection)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineMode)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineObservationState)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineFilter)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new PageNewReason1());
        pages.Add(new PageNewReason3());
        pages.Add(new PageNewReason4());
        pages.Add(new Page2());
        pages.Add(new Page3());
        pages.Add(new PageSelectableReason());
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
      data.InitValue(REASON_NAME, typeof(string), "new reason", true);
      data.InitValue(REASON_CODE, typeof(string), "", true);
      data.InitValue(REASON_DESCRIPTION, typeof(string), "no description", true);
      data.InitValue(REASON_GROUP, typeof(IReasonGroup), null, true);
      data.InitValue(REASON_OPERATION_DIRECTION, typeof(LinkDirection), LinkDirection.None, true);
      data.InitValue(MACHINE_MODES, typeof(IList<IMachineMode>), new List<IMachineMode>(), true);
      data.InitValue(MOSS, typeof(IList<IMachineObservationState>), new List<IMachineObservationState>(), true);
      data.InitValue(DETAILS_REQUIRED, typeof(bool), false, true);
      data.InitValue(MACHINE_FILTER, typeof(IMachineFilter), null, true);
      
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

          // Retrieve and lock data
          IList<IMachineMode> mamos = Item.GetAndLockMachineModes(data);
          IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);

          // Create a reason
          IReason reason = ModelDAOHelper.ModelFactory.CreateReason(data.Get<IReasonGroup>(REASON_GROUP));
          reason.Name = data.Get<string>(REASON_NAME);
          reason.Code = data.Get<string>(REASON_CODE);
          reason.Description = data.Get<string>(REASON_DESCRIPTION);
          reason.LinkOperationDirection = data.Get<LinkDirection>(REASON_OPERATION_DIRECTION);
          ModelDAOHelper.DAOFactory.ReasonDAO.MakePersistent(reason);
          
          // Add selectable reasons
          foreach (IMachineMode mamo in mamos) {
            foreach (IMachineObservationState mos in moss) {
              IReasonSelection selectableReason = ModelDAOHelper.ModelFactory.CreateReasonSelection(mamo, mos);
              selectableReason.Reason = reason;
              selectableReason.Selectable = true;
              selectableReason.DetailsRequired = data.Get<bool>(DETAILS_REQUIRED);
              selectableReason.MachineFilter = data.Get<IMachineFilter>(MACHINE_FILTER);
              ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent(selectableReason);
            }
          }

          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
    
    #region Private methods
    /// <summary>
    /// Get and lock all machine modes
    /// The returned list is never null or empty, but can comprise a value "null"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static public IList<IMachineMode> GetAndLockMachineModes(ItemData data)
    {
      IList<IMachineMode> mamos = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
      foreach (IMachineMode mamo in mamos) {
        ModelDAOHelper.DAOFactory.MachineModeDAO.Lock(mamo);
      }

      return mamos;
    }
    
    /// <summary>
    /// Get and lock all machine observation states
    /// The returned list is never null or empty, but can comprise a value "null"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static public IList<IMachineObservationState> GetAndLockMachineObservationStates(ItemData data)
    {
      IList<IMachineObservationState> moss = data.Get<IList<IMachineObservationState>>(Item.MOSS);
      foreach (IMachineObservationState mos in moss) {
        ModelDAOHelper.DAOFactory.MachineObservationStateDAO.Lock(mos);
      }

      return moss;
    }
    
    static String HexConverter(Color c)
    {
      return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
    #endregion // Private methods
  }
}
