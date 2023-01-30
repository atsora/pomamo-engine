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
  /// Description of EventCncValueConfigConfig.
  /// </summary>
  public partial class EventCncValueConfigConfig
    : UserControl
    , IConfigControlObserver<IMachineFilter>
    , IConfigControlObserver<IEventLevel>
    , IConfigControlObserver<IField>
  {
    #region Members
    BindingList<IEventCncValueConfig> m_eventCncValueConfigs = null;
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IEventCncValueConfig> m_deleteList =
      new List<IEventCncValueConfig> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventCncValueConfigConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventCncValueConfigConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      eventCncValueConfigDataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("EventCncValueConfig");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      messageColumn.HeaderText = PulseCatalog.GetString("Message");
      fieldColumn.HeaderText = PulseCatalog.GetString("Field");
      machineFilterColumn.HeaderText = PulseCatalog.GetString("MachineFilter");
      conditionColumn.HeaderText = PulseCatalog.GetString("Condition");
      minDurationColumn.HeaderText = PulseCatalog.GetString("MinDuration");
      eventLevelColumn.HeaderText = PulseCatalog.GetString("EventLevel");
      
      addButton.Text = PulseCatalog.GetString("Add");

      {
        FieldDialog dialog = new FieldDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IField>(dialog);
        fieldColumn.CellTemplate = cell;
      }
      {
        MachineFilterDialog dialog = new MachineFilterDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter>(dialog);
        machineFilterColumn.CellTemplate = cell;
      }
      {
        EventLevelDialog dialog = new EventLevelDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IEventLevel>(dialog);
        eventLevelColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void EventCncValueConfigConfigLoad(object sender, EventArgs e)
    {
      EventCncValueConfigConfigLoad ();
    }
    
    void EventCncValueConfigConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("EventCncValueConfigConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IEventCncValueConfig> eventCncValueConfigs =
          daoFactory.EventCncValueConfigDAO.FindAllForConfig();

        if(m_eventCncValueConfigs != null) {
          m_eventCncValueConfigs.Clear ();
        }

        m_eventCncValueConfigs = new BindingList<IEventCncValueConfig>(eventCncValueConfigs);

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_eventCncValueConfigs;
        bindingSource.AllowNew = true;
        eventCncValueConfigDataGridView.DataSource = bindingSource;
      }
    }
    
    void EventCncValueConfigConfigValidated(object sender, EventArgs e)
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
          IEventCncValueConfig eventCncValueConfig = row.DataBoundItem as IEventCncValueConfig;
          if (null == eventCncValueConfig) {
            continue; // The row may have been deleted since
          }
          daoFactory.EventCncValueConfigDAO.MakePersistent (eventCncValueConfig);
        }

        foreach (IEventCncValueConfig eventCncValueConfig in m_deleteList) {
          daoFactory.EventCncValueConfigDAO.MakeTransient (eventCncValueConfig);
        }
        
        transaction.Commit ();
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IEventCncValueConfig eventCncValueConfig =
        e.Row.DataBoundItem
        as IEventCncValueConfig;
      if (null != eventCncValueConfig) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (eventCncValueConfig);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = eventCncValueConfigDataGridView.Rows [e.RowIndex];
        IEventCncValueConfig eventCncValueConfig =
          row.DataBoundItem
          as IEventCncValueConfig;
        if (null != eventCncValueConfig) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      FieldDialog fieldDialog = new FieldDialog();
      fieldDialog.Nullable = false;
      EventLevelDialog eventLevelDialog = new EventLevelDialog();
      eventLevelDialog.Nullable = false;
      EventCncValueInputDialog eventCncValueInputDialog = new EventCncValueInputDialog();
      
      if(eventCncValueInputDialog.ShowDialog() == DialogResult.OK){
        if(fieldDialog.ShowDialog() == DialogResult.OK){
          if(eventLevelDialog.ShowDialog() == DialogResult.OK){
            IEventCncValueConfig eventCnCValueConfig = ModelDAOHelper.ModelFactory
              .CreateEventCncValueConfig(eventCncValueInputDialog.SelectedName,fieldDialog.SelectedValue,
                                         eventLevelDialog.SelectedValue,eventCncValueInputDialog.SelectedMessage,
                                         eventCncValueInputDialog.SelectedCondition);
            m_eventCncValueConfigs.Add(eventCnCValueConfig);
            m_updateSet.Add(eventCncValueConfigDataGridView.Rows[eventCncValueConfigDataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
          }
        }
      }
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Called after (one or more) IMachineFilter was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineFilter> deletedEntities)
    {
      EventCncValueConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineFilter was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineFilter> updatedEntities)
    {
      //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IEventLevel was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IEventLevel> deletedEntities)
    {
      EventCncValueConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IEventLevel was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IEventLevel> updatedEntities)
    {
      //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IField was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IField> deletedEntities)
    {
      EventCncValueConfigConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IField was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IField> updatedEntities)
    {
      //Do nothing
    }
    #endregion
  }
}
