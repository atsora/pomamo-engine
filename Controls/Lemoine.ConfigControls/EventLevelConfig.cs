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
  /// Description of EventLevelConfig.
  /// </summary>
  public partial class EventLevelConfig
    : UserControl
    , IConfigControlObservable<IEventLevel>
  {
    #region Members
    BindingList<IDataGridViewEventLevel> m_eventlevels = new BindingList<IDataGridViewEventLevel>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IEventLevel> m_deleteList =
      new List<IEventLevel> ();
    
    ISet<IConfigControlObserver<IEventLevel>> m_observers =
      new HashSet<IConfigControlObserver<IEventLevel>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventLevelConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      eventLevelDataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("EventLevel");

      eventLevelidColumn.HeaderText = PulseCatalog.GetString ("Id");
      eventLevelnameColumn.HeaderText = PulseCatalog.GetString ("Name");
      eventLevelTranslationKeyColumn.HeaderText = PulseCatalog.GetString("EventLevelTranslationKeyColumn");
      eventLevelPriorityColumn.HeaderText = PulseCatalog.GetString("EventLevelPriorityColumn");
      
      //DataGridView
      {
        TranslationKeyDialog dialog = new TranslationKeyDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<String>(dialog);
        eventLevelTranslationKeyColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void EventLevelConfigLoad(object sender, EventArgs e)
    {
      EventLevelConfigLoad();
    }
    
    void EventLevelConfigEnter(object sender, EventArgs e)
    {
      EventLevelConfigLoad();
    }
    
    void EventLevelConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("EventLevelConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IEventLevel> eventlevels = daoFactory.EventLevelDAO.FindAllForConfig();

        if(m_eventlevels != null) {
          m_eventlevels.Clear ();
        }

        foreach (IEventLevel eventLevel in eventlevels) {
          m_eventlevels.Add((IDataGridViewEventLevel)eventLevel);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_eventlevels;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        eventLevelDataGridView.AutoGenerateColumns = false;
        eventLevelDataGridView.DataSource = bindingSource;
      }
    }
    
    void EventLevelConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IList<IEventLevel> updateList = new List<IEventLevel> ();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IEventLevel eventlevel = row.DataBoundItem as IEventLevel;
          if (null == eventlevel) {
            continue; // The row may have been deleted since
          }
          daoFactory.EventLevelDAO.MakePersistent (eventlevel);
          updateList.Add (eventlevel);
        }

        foreach (IEventLevel eventlevel in m_deleteList) {
          if (!daoFactory.EventLevelDAO.IsEventLevelUsed (eventlevel)) {
            daoFactory.EventLevelDAO.MakeTransient (eventlevel);
          }
        }
        
        transaction.Commit ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      NotifyDelete(m_deleteList);
      NotifyUpdate(updateList);
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IEventLevel eventlevel = e.Row.DataBoundItem as IEventLevel;
      if (null != eventlevel) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (eventlevel);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = eventLevelDataGridView.Rows [e.RowIndex];
        IEventLevel eventlevel =
          row.DataBoundItem
          as IEventLevel;
        if (null != eventlevel) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateEventLevelFromName (000, "New event level");
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IEventLevel> observer){
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IEventLevel> observer){
      this.m_observers.Remove(observer);
    }

    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedEventLevel"></param>
    void NotifyDelete(IList<IEventLevel> deletedEventLevel)
    {
      foreach(IConfigControlObserver<IEventLevel> observer in m_observers){
        observer.UpdateAfterDelete(deletedEventLevel);
      }
    }

    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedEventLevel"></param>
    void NotifyUpdate(IList<IEventLevel> updatedEventLevel)
    {
      foreach(IConfigControlObserver<IEventLevel> observer in m_observers){
        observer.UpdateAfterUpdate(updatedEventLevel);
      }
    }
    #endregion
  }
}
