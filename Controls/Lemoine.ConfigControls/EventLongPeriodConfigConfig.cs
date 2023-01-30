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
  /// Description of EventLongPeriodConfigConfig.
  /// </summary>
  public partial class EventLongPeriodConfigConfig
    : UserControl
    , IConfigControlObservable<IEventLongPeriodConfig>
    , IConfigControlObserver<IEventLevel>
    , IConfigControlObserver<IMachineObservationState>
    , IConfigControlObserver<IMachineMode>
    , IConfigControlObserver<IMonitoredMachine>
  {
    #region Members
    BindingList<IEventLongPeriodConfig> m_eventLongPeriodConfigs =
      new BindingList<IEventLongPeriodConfig>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IEventLongPeriodConfig> m_deleteList =
      new List<IEventLongPeriodConfig> ();
    
    ISet<IConfigControlObserver<IEventLongPeriodConfig>> m_observers =
      new HashSet<IConfigControlObserver<IEventLongPeriodConfig>> ();
    #endregion // Members
    
    #region Getter/Setter
    
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (EventLongPeriodConfigConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventLongPeriodConfigConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      eventLongPeriodConfigDataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("EventLongPeriodConfig");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      monitoredMachineColumn.HeaderText = PulseCatalog.GetString ("MonitoredMachine");
      machineModeColumn.HeaderText = PulseCatalog.GetString("MachineMode");
      MachineObservationState.HeaderText = PulseCatalog.GetString("MachineObservationState");
      triggerDurationColumn.HeaderText = PulseCatalog.GetString("TriggerDuration");
      eventLevelColumn.HeaderText = PulseCatalog.GetString("EventLevel");
      
      addButton.Text = PulseCatalog.GetString("Add");
      
      {
        MonitoredMachineDialog dialog = new MonitoredMachineDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMonitoredMachine> (dialog);
        monitoredMachineColumn.CellTemplate = cell;
      }
      {
        MachineModeDialog dialog = new MachineModeDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMode> (dialog);
        machineModeColumn.CellTemplate = cell;
      }
      {
        MachineObservationStateDialog dialog = new MachineObservationStateDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState> (dialog);
        MachineObservationState.CellTemplate = cell;
      }
      {
        EventLevelDialog dialog = new EventLevelDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IEventLevel>(dialog);
        eventLevelColumn.CellTemplate = cell;
      }

    }
    #endregion // Constructors
    
    void EventLongPeriodConfigConfigLoad(object sender, EventArgs e)
    {
      EventLongPeriodConfigConfigLoad ();
    }
    
    void EventLongPeriodConfigConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("EventLongPeriodConfigConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IEventLongPeriodConfig> eventLongPeriodConfigs = daoFactory.EventLongPeriodConfigDAO.FindAllForConfig();

        m_eventLongPeriodConfigs.Clear();
        m_eventLongPeriodConfigs = new BindingList<IEventLongPeriodConfig>(eventLongPeriodConfigs);

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_eventLongPeriodConfigs;
        bindingSource.AllowNew = true;
        eventLongPeriodConfigDataGridView.AutoGenerateColumns = false;
        eventLongPeriodConfigDataGridView.DataSource = bindingSource;
      }
    }
    
    void EventLongPeriodConfigConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IEventLongPeriodConfig eventLongPeriodConfig = row.DataBoundItem as IEventLongPeriodConfig;
          if (null == eventLongPeriodConfig) {
            continue; // The row may have been deleted since
          }
          daoFactory.EventLongPeriodConfigDAO.MakePersistent (eventLongPeriodConfig);
        }

        foreach (IEventLongPeriodConfig eventLongPeriodConfig in m_deleteList) {
          daoFactory.EventLongPeriodConfigDAO.MakeTransient (eventLongPeriodConfig);
        }
        
        transaction.Commit ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      if(m_deleteList.Count >= 1){
        NotifyDelete(m_deleteList);
      }
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IEventLongPeriodConfig eventLongPeriodConfig = e.Row.DataBoundItem as IEventLongPeriodConfig;
      if (null != eventLongPeriodConfig) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (eventLongPeriodConfig);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = eventLongPeriodConfigDataGridView.Rows [e.RowIndex];
        IEventLongPeriodConfig eventLongPeriodConfig =
          row.DataBoundItem
          as IEventLongPeriodConfig;
        if (null != eventLongPeriodConfig) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      TimeSpanDialog timeSpanDialog = new TimeSpanDialog();
      timeSpanDialog.Nullable = false;
      EventLevelDialog eventLevelDialog = new EventLevelDialog();
      eventLevelDialog.Nullable = false;
      
      if(eventLevelDialog.ShowDialog() == DialogResult.OK){
        if(timeSpanDialog.ShowDialog() == DialogResult.OK){
          IEventLongPeriodConfig eventLongPeriodConfig = ModelDAOHelper.ModelFactory.CreateEventLongPeriodConfig(timeSpanDialog.SelectedValue.Value, eventLevelDialog.SelectedValue);
          m_eventLongPeriodConfigs.Add(eventLongPeriodConfig);
          m_updateSet.Add(eventLongPeriodConfigDataGridView.Rows[eventLongPeriodConfigDataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
        }
      }
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Called after (one or more) IField was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IEventLevel> deletedEntities)
    {
      EventLongPeriodConfigConfigLoad();
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
      EventLongPeriodConfigConfigLoad();
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
    public void UpdateAfterDelete(ICollection<IMachineMode> deletedEntities)
    {
      EventLongPeriodConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineMode was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineMode> updatedEntities)
    {
      //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IMonitoredMachine was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMonitoredMachine> deletedEntities)
    {
      EventLongPeriodConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMonitoredMachine was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMonitoredMachine> updatedEntities)
    {
      //Do nothing
    }
    #endregion
    
    #region IConfigControlObsable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IEventLongPeriodConfig> observer){
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IEventLongPeriodConfig> observer){
      this.m_observers.Remove(observer);
    }

    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedEventLongPeriodConfigs"></param>
    void NotifyDelete(IList<IEventLongPeriodConfig> deletedEventLongPeriodConfigs){
      foreach(IConfigControlObserver<IEventLongPeriodConfig> observer in m_observers){
        observer.UpdateAfterDelete(deletedEventLongPeriodConfigs);
      }
    }

    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedEventLongPeriodConfigs"></param>
    void NotifyUpdate(IList<IEventLongPeriodConfig> updatedEventLongPeriodConfigs){
      foreach(IConfigControlObserver<IEventLongPeriodConfig> observer in m_observers){
        observer.UpdateAfterUpdate(updatedEventLongPeriodConfigs);
      }
    }
    #endregion
  }
}
