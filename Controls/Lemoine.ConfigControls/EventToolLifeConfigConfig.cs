// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Iesi.Collections.Generic;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of EventToolLifeConfigConfig.
  /// </summary>
  public partial class EventToolLifeConfigConfig : UserControl,
  IConfigControlObservable<IEventToolLifeConfig>,
  IConfigControlObserver<IEventLevel>,
  IConfigControlObserver<IMachineObservationState>,
  IConfigControlObserver<IMachineFilter>
  {
    #region Members
    BindingList<IEventToolLifeConfig> m_eventToolLifeConfigs = new BindingList<IEventToolLifeConfig>();
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IEventToolLifeConfig> m_deleteList = new List<IEventToolLifeConfig> ();
    
    readonly ISet<IConfigControlObserver<IEventToolLifeConfig>> m_observers =
      new HashSet<IConfigControlObserver<IEventToolLifeConfig>> ();
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (EventToolLifeConfigConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventToolLifeConfigConfig()
    {
      InitializeComponent();
      
      eventToolLifeConfigDataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("EventToolLifeConfig");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      machineFilterColumn.HeaderText = PulseCatalog.GetString ("MachineFilter");
      mosColumn.HeaderText = PulseCatalog.GetString("MachineObservationState");
      eventTypeColumn.HeaderText = PulseCatalog.GetString("ToolLifeEventType");
      eventLevelColumn.HeaderText = PulseCatalog.GetString("EventLevel");
      
      addButton.Text = PulseCatalog.GetString("Add");
      
      {
        var dialog = new MachineFilterDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter> (dialog);
        machineFilterColumn.CellTemplate = cell;
      }
      {
        var dialog = new MachineObservationStateDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState> (dialog);
        mosColumn.CellTemplate = cell;
      }
      {
        var dialog = new EventLevelDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IEventLevel>(dialog);
        eventLevelColumn.CellTemplate = cell;
      }

    }
    #endregion // Constructors
    
    void EventToolLifeConfigConfigLoad(object sender, EventArgs e)
    {
      EventToolLifeConfigConfigLoad();
    }
    
    void EventToolLifeConfigConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("EventToolLifeConfigConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IEventToolLifeConfig> eventToolLifeConfigs = daoFactory.EventToolLifeConfigDAO.FindAllForConfig();

        m_eventToolLifeConfigs.Clear();
        m_eventToolLifeConfigs = new BindingList<IEventToolLifeConfig>(eventToolLifeConfigs);

        // Note: the use of a bindingSource is necessary to add some new rows
        var bindingSource = new BindingSource ();
        bindingSource.DataSource = m_eventToolLifeConfigs;
        bindingSource.AllowNew = true;
        eventToolLifeConfigDataGridView.AutoGenerateColumns = false;
        eventToolLifeConfigDataGridView.DataSource = bindingSource;
      }
    }
    
    void EventToolLifeConfigConfigValidated(object sender, EventArgs e)
    {
      CommitChanges();
    }
    
    void CommitChanges()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          var eventToolLifeConfig = row.DataBoundItem as IEventToolLifeConfig;
          if (null == eventToolLifeConfig) {
            continue; // The row may have been deleted since
          }
          daoFactory.EventToolLifeConfigDAO.MakePersistent(eventToolLifeConfig);
        }

        foreach (IEventToolLifeConfig eventToolLifeConfig in m_deleteList) {
          daoFactory.EventToolLifeConfigDAO.MakeTransient(eventToolLifeConfig);
        }
        
        transaction.Commit ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      if (m_deleteList.Count >= 1) {
        NotifyDelete(m_deleteList);
      }
      
      m_updateSet.Clear();
      m_deleteList.Clear();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      var eventToolLifeConfig = e.Row.DataBoundItem as IEventToolLifeConfig;
      if (null != eventToolLifeConfig) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (eventToolLifeConfig);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = eventToolLifeConfigDataGridView.Rows [e.RowIndex];
        var eventToolLifeConfig = row.DataBoundItem as IEventToolLifeConfig;
        if (null != eventToolLifeConfig) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      var eventLevelDialog = new EventLevelDialog();
      eventLevelDialog.Nullable = false;
      
      if (eventLevelDialog.ShowDialog() == DialogResult.OK) {
        IEventToolLifeConfig eventToolLifeConfig = ModelDAOHelper.ModelFactory.CreateEventToolLifeConfig(
          EventToolLifeType.Unknown, eventLevelDialog.SelectedValue);
        m_eventToolLifeConfigs.Add(eventToolLifeConfig);
        m_updateSet.Add(eventToolLifeConfigDataGridView.Rows[
          eventToolLifeConfigDataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
      }
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Called after (one or more) IField was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IEventLevel> deletedEntities)
    {
      EventToolLifeConfigConfigLoad();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IEventLevel> updatedEntities)
    {
      //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IMachineObservationState was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineObservationState> deletedEntities)
    {
      EventToolLifeConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineObservationState was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineObservationState> updatedEntities)
    {
      //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IMachineMode was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineFilter> deletedEntities)
    {
      EventToolLifeConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineMode was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineFilter> updatedEntities)
    {
      //Do nothing
    }
    #endregion
    
    #region IConfigControlObsable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IConfigControlObserver<IEventToolLifeConfig> observer)
    {
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IConfigControlObserver<IEventToolLifeConfig> observer)
    {
      this.m_observers.Remove(observer);
    }

    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedEventToolLifeConfigs"></param>
    void NotifyDelete(IList<IEventToolLifeConfig> deletedEventToolLifeConfigs)
    {
      foreach (IConfigControlObserver<IEventToolLifeConfig> observer in m_observers) {
        observer.UpdateAfterDelete(deletedEventToolLifeConfigs);
      }
    }

    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedEventToolLifeConfigs"></param>
    void NotifyUpdate(IList<IEventToolLifeConfig> updatedEventToolLifeConfigs)
    {
      foreach (IConfigControlObserver<IEventToolLifeConfig> observer in m_observers) {
        observer.UpdateAfterUpdate(updatedEventToolLifeConfigs);
      }
    }
    #endregion
  }
}
