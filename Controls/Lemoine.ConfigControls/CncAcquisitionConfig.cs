// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of CncAcquisitionConfig.
  /// </summary>
  public partial class CncAcquisitionConfig
    : UserControl
    , IConfigControlObservable<ICncAcquisition>
    , IConfigControlObserver<IMonitoredMachine>
  {
    #region Members
    SortableBindingList<ICncAcquisition> m_cncAcquisitions = new SortableBindingList<ICncAcquisition>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<ICncAcquisition> m_deleteList =
      new List<ICncAcquisition> ();
    
    ISet<IConfigControlObserver<ICncAcquisition> > m_observers =
      new HashSet<IConfigControlObserver<ICncAcquisition> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CncAcquisitionConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncAcquisitionConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("CncAcquisition");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      configFileColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionConfigFile");
      configPrefixColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionConfigPrefix");
      configParametersColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionConfigParameters");
      useProcessColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionUseProcess");
      computerColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionComputer");
      everyColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionEvery");
      notRespondingTimeoutColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionNotRespondingTimeout");
      sleepBeforeRestartColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionSleepBeforeRestart");
      
      m_cncAcquisitions.SortColumns = false;
      
      {
        ComputerDialog dialog =
          new ComputerDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IComputer> (dialog);
        computerColumn.CellTemplate = cell;
      }
      
      {
        FileRepositoryDialog dialog =
          new FileRepositoryDialog ();
        dialog.Nullable = true;
        dialog.NSpace = "cncconfigs";
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        configFileColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void CncAcquisitionConfigLoad(object sender, EventArgs e)
    {
      CncAcquisitionConfigLoad ();
    }
    
    void CncAcquisitionConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.Error ("CncAcquisitionConfigLoad: no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<ICncAcquisition> cncAcquisitions =
          daoFactory.CncAcquisitionDAO.FindAllWithChildren ();

        m_cncAcquisitions.Clear ();
        foreach(ICncAcquisition cncAcquisition in cncAcquisitions) {
          m_cncAcquisitions.Add(cncAcquisition);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_cncAcquisitions;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void CncAcquisitionConfigValidated(object sender, EventArgs e)
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
          ICncAcquisition cncAcquisition = row.DataBoundItem as ICncAcquisition;
          if (null == cncAcquisition) {
            continue; // The row may have been deleted since
          }
          if (AddNotNullProperties (cncAcquisition)) {
            daoFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
          }
        }

        foreach (ICncAcquisition cncAcquisition
                 in m_deleteList) {
          daoFactory.CncAcquisitionDAO.MakeTransient (cncAcquisition);
        }
        
        transaction.Commit ();
        
        NotifyDelete (m_deleteList);
        
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      ICncAcquisition cncAcquisition =
        e.Row.DataBoundItem
        as ICncAcquisition;
      if (null != cncAcquisition) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (cncAcquisition);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      ICncAcquisition cncAcquisition =
        e.Row.DataBoundItem
        as ICncAcquisition;
      if (null != cncAcquisition) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        ICncAcquisition cncAcquisition =
          row.DataBoundItem
          as ICncAcquisition;
        if (null != cncAcquisition) {
          AddNotNullProperties (cncAcquisition);
          m_updateSet.Add (row);
        }
      }
    }

    bool AddNotNullProperties (ICncAcquisition cncAcquisition)
    {
      if (null == cncAcquisition.Computer) {
        // Choose a LPost
        ComputerDialog dialog =
          new ComputerDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        dialog.Text = PulseCatalog.GetString ("CncAcquisitionComputerSelectionTitleA") + cncAcquisition.Name;
        if (DialogResult.OK != dialog.ShowDialog ()) {
          return false;
        }
        else { // OK
          cncAcquisition.Computer = dialog.SelectedValue;
        }
      }
      
      return true;
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<ICncAcquisition> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    ///
    /// This is the implementation of IConfigControlObservable"
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<ICncAcquisition> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    /// <param name="deletedEntities"></param>
    void NotifyDelete (IList<ICncAcquisition> deletedEntities)
    {
      foreach (IConfigControlObserver<ICncAcquisition> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MonitoredMachine control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMonitoredMachine> deletedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been updated or inserted
    /// in the MonitoredMachine control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMonitoredMachine> updatedEntities)
    {
      CncAcquisitionConfigLoad ();
    }
    #endregion // IConfigControlObserver implementation
  }
}
