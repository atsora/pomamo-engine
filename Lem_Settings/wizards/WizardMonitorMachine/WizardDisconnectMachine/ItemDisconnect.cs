// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;
using System.Linq;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Add or change the way a machine is monitored
  /// </summary>
  internal class ItemDisconnect : GenericItem, IWizard
  {
    internal const string MACHINE = "machine";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Disconnect machine"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "With this item you can disconnect a machine. The configuration of the acquisition will be kept " +
          "and the machine will be flagged as obsolete.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines", "monitored", "monitoring", "disconnect", "obsolete" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "disconnect_machine"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Configuration"; } }

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
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineMonitoringType)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
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
        pages.Add(new PageDisconnectMachine());
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
      
      // The machine may come from a previous data
      IMachine machine = null;
      if (otherData != null) {
        machine = otherData.IsStored<IMachine>(MACHINE) ?
          otherData.Get<IMachine>(MACHINE) : null;
      }

      // Common data
      data.CurrentPageName = "";
      data.InitValue(MACHINE, typeof(IMachine), machine, true);
      
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
          IModelFactory factory = ModelDAOHelper.ModelFactory;
          IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;
          
          // Get the machine
          var machine = data.Get<IMachine>(MACHINE);
          
          // Monitoring type changed to "obsolete"
          machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById(5); // Obsolete
          ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent(machine);
          
          // Delete the cnc acquisition
          var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machine.Id);
          if (moma != null) {
            foreach (var mamo in moma.MachineModules) {
              if (mamo.CncAcquisition != null) {
                ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakeTransient(mamo.CncAcquisition);
              }
            }
            if (moma.MainCncAcquisition != null) {
              ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakeTransient(moma.MainCncAcquisition);
            }
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
  }
}
