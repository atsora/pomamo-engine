// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string MACHINE_MODE = "machine_mode";
    internal const string MOSS = "machine_observation_states";
    internal const string DEFAULT_REASONS = "default_reasons";
    internal const string SELECTABLE_REASONS = "selectable_reasons";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Change default and selectable reasons"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Configure the possible reasons for a set of " +
          "machine modes (what is detected) and machine observation states (what is planned and expected).\n\n" +
          "The assignment logic is further defined:\n" +
          " - the definition of which reasons can be assigned by default, and in which context;\n" +
          " - the definition of the selectable reasons, if a default reason needs to be overwritten;\n" +
          " - selectable reasons may need further details.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "reasons", "selections", "machines", "observations", "states", "modes",
          "selectable", "default" };
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
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IReasonSelection)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineModeDefaultReason)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IReason)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        var pages = new List<IWizardPage>();
        pages.Add(new Page2());
        pages.Add(new Page3());
        pages.Add(new PageDefaultReason());
        pages.Add(new PageSelectedReason());
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
      data.InitValue(MACHINE_MODE, typeof(IMachineMode), null, true);
      data.InitValue(MOSS, typeof(IList<IMachineObservationState>), new List<IMachineObservationState>(), true);
      data.InitValue(DEFAULT_REASONS, typeof(IDictionary<IReason, DefaultReasonParameters>), new Dictionary<IReason, DefaultReasonParameters>(), true);
      data.InitValue(SELECTABLE_REASONS, typeof(IDictionary<IReason, ReasonParameters>), new Dictionary<IReason, ReasonParameters>(), true);
      
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

          var defaultReasons = data.Get<IDictionary<IReason, DefaultReasonParameters>>(Item.DEFAULT_REASONS);
          foreach (IReason reason in defaultReasons.Keys) {
            ModelDAOHelper.DAOFactory.ReasonDAO.Lock(reason);
          }

          // Retrieve and lock data
          IList<IMachineMode> mamos = new List<IMachineMode>();
          mamos.Add(Item.GetAndLockMachineMode(data));
          IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
          
          // Delete existing default and selectable reasons
          DeleteExistingReasons(mamos, moss);
          
          // Add default reasons
          foreach (IReason reason in defaultReasons.Keys) {
            DefaultReasonParameters param = defaultReasons[reason];
            foreach (IMachineMode mamo in mamos) {
              foreach (IMachineObservationState mos in moss) {
                AddDefaultReason(mamo, mos, reason, param);
              }
            }
          }
          
          transaction.Commit();
        }
      }
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          
          var reasons = data.Get<IDictionary<IReason, ReasonParameters>>(Item.SELECTABLE_REASONS);
          foreach (IReason reason in reasons.Keys) {
            ModelDAOHelper.DAOFactory.ReasonDAO.Lock(reason);
          }

          // Retrieve and lock data
          IMachineMode mamo = Item.GetAndLockMachineMode(data);
          IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
          
          // Add selectable reasons
          foreach (IReason reason in reasons.Keys) {
            ReasonParameters param = reasons[reason];
            foreach (IMachineObservationState mos in moss) {
              AddSelectableReason(mamo, mos, reason, param);
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
    static public IMachineMode GetAndLockMachineMode(ItemData data)
    {
      IMachineMode mamo = data.Get<IMachineMode>(Item.MACHINE_MODE);
      ModelDAOHelper.DAOFactory.MachineModeDAO.Lock(mamo);
      return mamo;
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
    
    void DeleteExistingReasons(IList<IMachineMode> mamos, IList<IMachineObservationState> moss)
    {
      var reasonSelections = ModelDAOHelper.DAOFactory.ReasonSelectionDAO.FindWithForConfig (mamos, moss);
      foreach (var reasonSelection in reasonSelections) {
        ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakeTransient(reasonSelection);
      }

      var defaultReasons = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.FindWithForConfig (mamos, moss);
      foreach (var defaultReason in defaultReasons) {
        ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakeTransient(defaultReason);
      }
    }
    
    void AddDefaultReason(IMachineMode mamo, IMachineObservationState mos, IReason reason, DefaultReasonParameters param)
    {
      // Reason selection - not selectable
      IReasonSelection selectableReason = ModelDAOHelper.ModelFactory.CreateReasonSelection(mamo, mos);
      selectableReason.Reason = reason;
      selectableReason.Selectable = false;
      selectableReason.DetailsRequired = false;
      
      ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent(selectableReason);
      
      // Default reason
      IMachineModeDefaultReason defaultReason = ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason(mamo, mos);
      defaultReason.Reason = reason;
      defaultReason.OverwriteRequired = param.OverwriteRequired;
      defaultReason.MaximumDuration = param.MaxTime;
      defaultReason.IncludeMachineFilter = param.MachineFilterInclude;
      defaultReason.ExcludeMachineFilter = param.MachineFilterExclude;
      
      ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent(defaultReason);
    }
    
    void AddSelectableReason(IMachineMode mamo, IMachineObservationState mos, IReason reason, ReasonParameters param)
    {
      IReasonSelection selectableReason = ModelDAOHelper.ModelFactory.CreateReasonSelection(mamo, mos);
      selectableReason.Reason = reason;
      selectableReason.Selectable = true;
      selectableReason.DetailsRequired = param.DetailsRequired;
      selectableReason.MachineFilter = param.MachineFilter;
      
      ModelDAOHelper.DAOFactory.ReasonSelectionDAO.MakePersistent(selectableReason);
    }
    #endregion // Private methods
  }
}
