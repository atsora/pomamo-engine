// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of ConfigConfig.
  /// </summary>
  public partial class ConfigConfig : UserControl, IConfigControlObservable<IConfig>
  {
    #region Members
    SortableBindingList<IConfig> m_configs = new SortableBindingList<IConfig>();
    
    string m_filter = "%";
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    
    ISet<IConfigControlObserver<IConfig>> m_observers =
      new HashSet<IConfigControlObserver<IConfig> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ConfigConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfigConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Config");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      keyColumn.HeaderText = PulseCatalog.GetString ("ConfigKey");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("ConfigDescription");
      valueColumn.HeaderText = PulseCatalog.GetString ("ConfigValue");
      activeColumn.HeaderText = PulseCatalog.GetString ("ConfigActive", "Active");

      m_configs.SortColumns = false;
      
      {
        DataGridViewCell cell = new DataGridViewObjectCell ();
        valueColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Config filter
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("%"), Description("Configuration filter")]
    public string Filter {
      get { return m_filter; }
      set { m_filter = value; }
    }
    #endregion // Getters / Setters
    
    void ConfigConfigLoad(object sender, EventArgs e)
    {
      ConfigConfigLoad ();
    }
    
    void ConfigConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ConfigConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IConfig> configs =
          daoFactory.ConfigDAO.FindLike (m_filter);

        m_configs.Clear ();
        foreach(IConfig config in configs) {
          m_configs.Add(config);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_configs;
        bindingSource.AllowNew = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void ConfigConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if (0 == m_updateSet.Count) {
        return;
      }
      
      IList<IConfig> configs = new List<IConfig>();
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IConfig config = row.DataBoundItem as IConfig;
          if (null == config) {
            continue; // The row may have been deleted since
          }
          else {
            configs.Add(config);
          }
          daoFactory.ConfigDAO.MakePersistent (config);
        }

        transaction.Commit ();
        m_updateSet.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      NotifyUpdated(configs);
    }

    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IConfig config =
          row.DataBoundItem
          as IConfig;
        if (null != config) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void DataGridViewCellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
      CommitChanges();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IConfigControlObserver<IConfig> observer)
    {
      m_observers.Add(observer);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IConfigControlObserver<IConfig> observer)
    {
      m_observers.Remove(observer);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedConfigs"></param>
    void NotifyUpdated(IList<IConfig> updatedConfigs){
      foreach (IConfigControlObserver<IConfig> observer in m_observers)
      {
        observer.UpdateAfterUpdate (updatedConfigs);
      }
    }
    #endregion
  }
}
