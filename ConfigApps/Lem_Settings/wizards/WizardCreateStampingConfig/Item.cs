// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace WizardCreateStampingConfig
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    static readonly internal string CONFIG_ID = "config_id";
    static readonly internal string CONFIG_NAME = "config_name";
    static readonly internal string TEMPLATE_PATH = "template_path";
    static readonly internal string CONFIG_JSON = "config_json"; // Config in json format
    static readonly internal string MONITORED_MACHINES = "monitored_machines";

    ILog log = LogManager.GetLogger<Item> ();

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Stamping config creation wizard"; } }

    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description
    {
      get {
        return "Wizard to create a new stamping configuration (for a specific post-processor)";
      }
    }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords
    {
      get {
        return new String[] { "stamping", "post-processor" };
      }
    }

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }

    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Stamping"; } }

    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return ""; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags
    {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types
    {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType> ();
        dic[typeof (Int32)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages => new List<IWizardPage> {
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
    public ItemData Initialize (ItemData otherData)
    {
      var data = new ItemData ();

      // Common data
      data.CurrentPageName = "";
      data.InitValue (CONFIG_ID, (int?)null, true);
      data.InitValue (CONFIG_NAME, "", true);
      data.InitValue (TEMPLATE_PATH, "", true);
      data.InitValue (CONFIG_JSON, "", true);
      data.InitValue (MONITORED_MACHINES, new List<IMonitoredMachine> (), true);

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
    public void Finalize (ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      try {
        var configName = data.Get<string> (CONFIG_NAME);
        var json = data.Get<string> (CONFIG_JSON);
        var stampingConfig = System.Text.Json.JsonSerializer.Deserialize<StampingConfig> (json);
        var monitoredMachines = data.Get<IList<IMonitoredMachine>> (MONITORED_MACHINES);
        var configId = data.Get<int?> (CONFIG_ID);

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Settings.WizardCreateStampingConfig")) {
            IStampingConfigByName stampingConfigByName;
            if (configId.HasValue) {
              stampingConfigByName = ModelDAOHelper.DAOFactory.StampingConfigByNameDAO
                .FindById (configId.Value);
              stampingConfigByName.Name = configName;
            }
            else {
              stampingConfigByName = ModelDAOHelper.ModelFactory.CreateStampingConfigByName (configName);
            }
            stampingConfigByName.Config = stampingConfig;
            ModelDAOHelper.DAOFactory.StampingConfigByNameDAO.MakePersistent (stampingConfigByName);

            foreach (var monitoredMachine in monitoredMachines) {
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock (monitoredMachine);
              monitoredMachine.StampingConfigByName = stampingConfigByName;
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (monitoredMachine);
            }

            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"Finalize: exception", ex);
        throw;
      }
    }
    #endregion // Wizard methods
  }
}
