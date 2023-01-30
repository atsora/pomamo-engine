// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Iesi.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MachineObservationStateConfig.
  /// </summary>
  public partial class MachineObservationStateConfig
    : UserControl
    , IConfigControlObservable<IMachineObservationState>
  {
    #region Members
    SortableBindingList<IMachineObservationState> m_machineObservationStates
      = new SortableBindingList<IMachineObservationState>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineObservationState> m_deleteList =
      new List<IMachineObservationState> ();

    ISet<IConfigControlObserver<IMachineObservationState> > m_observers =
      new HashSet<IConfigControlObserver<IMachineObservationState> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineObservationStateConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      userRequiredColumn.HeaderText = PulseCatalog.GetString ("MachineObservationStateUserRequired");
      shiftRequiredColumn.HeaderText = PulseCatalog.GetString ("MachineObservationStateShiftRequired");
      onSiteColumn.HeaderText = PulseCatalog.GetString ("MachineObservationStateOnSite");
      siteAttendanceChangeColumn.HeaderText = PulseCatalog.GetString ("MachineObservationStateSiteAttendanceChange");
      linkOperationDirectionColumn.HeaderText = PulseCatalog.GetString ("LinkOperationDirection");
      isProductionColumn.HeaderText = PulseCatalog.GetString ("MachineObservationStateIsProduction");
      
      m_machineObservationStates.SortColumns = false;
      
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        translationKeyColumn.CellTemplate = cell;
      }
      {
        MachineObservationStateDialog dialog =
          new MachineObservationStateDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState> (dialog);
        siteAttendanceChangeColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void MachineObservationStateConfigLoad(object sender, EventArgs e)
    {
      MachineObservationStateConfigLoad();
    }
    
    void MachineObservationStateConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineObservationStateConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        IList<IMachineObservationState> machineObservationStates =
          daoFactory.MachineObservationStateDAO.FindAll ();
        
        m_machineObservationStates.Clear ();
        foreach(IMachineObservationState machineObservationState in machineObservationStates)
        {
          m_machineObservationStates.Add(machineObservationState);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineObservationStates;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void DataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if ((e.RowIndex < 0) || (e.ColumnIndex < 0) ||
          (e.ColumnIndex > dataGridView.Columns.Count)) {
        return;
      }

      if (translationKeyColumn.Name.Equals (dataGridView.Columns [e.ColumnIndex].Name)) {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        if (DialogResult.OK == dialog.ShowDialog ()) {
          DataGridViewRow row = dataGridView.Rows [e.RowIndex];
          row.Cells [e.ColumnIndex].Value = dialog.SelectedValue;
        }
      }
    }
    
    void MachineObservationStateConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IList<IMachineObservationState> updateList = new List<IMachineObservationState> ();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IMachineObservationState machineObservationState = row.DataBoundItem as IMachineObservationState;
          if (null == machineObservationState) {
            continue; // The row may have been deleted since
          }
          updateList.Add (machineObservationState);
          daoFactory.MachineObservationStateDAO.MakePersistent (machineObservationState);
        }

        foreach (IMachineObservationState machineObservationState
                 in m_deleteList) {
          daoFactory.MachineObservationStateDAO.MakeTransient (machineObservationState);
        }
        
        transaction.Commit ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      NotifyUpdate (updateList);
      NotifyDelete (m_deleteList);
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
      
    }
    
    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineObservationState machineObservationState =
        e.Row.DataBoundItem
        as IMachineObservationState;
      if (null != machineObservationState) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineObservationState);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IMachineObservationState machineObservationState =
        e.Row.DataBoundItem
        as IMachineObservationState;
      if (null != machineObservationState) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineObservationState machineObservationState =
          row.DataBoundItem
          as IMachineObservationState;
        if (null != machineObservationState) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineObservationState ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineObservationState> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineObservationState> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after an update
    /// </summary>
    void NotifyUpdate (IList<IMachineObservationState> updatedEntities)
    {
      foreach (IConfigControlObserver<IMachineObservationState> observer in m_observers)
      {
        observer.UpdateAfterUpdate (updatedEntities);
      }
    }

    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IMachineObservationState> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineObservationState> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}