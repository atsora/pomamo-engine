// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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
  /// Description of MachineStatusConfig.
  /// </summary>
  public partial class MachineStatusConfig 
    : UserControl
    , IConfigControlObserver<IMonitoredMachine>
    , IConfigControlObserver<IMachineMode>
    , IConfigControlObserver<IMachineObservationState>
    , IConfigControlObserver<IShift>
    , IConfigControlObserver<IReason>
  {
    #region Members
    SortableBindingList<IMachineStatus> m_machineStatuss = new SortableBindingList<IMachineStatus>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineStatus> m_deleteList =
      new List<IMachineStatus> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStatusConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStatusConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineStatus");
      
      //I18N
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      monitoredMachineColumn.HeaderText = PulseCatalog.GetString("MonitoredMachine");
      cncMachineModeColumn.HeaderText = PulseCatalog.GetString("CncMachineMode");
      machineModeColumn.HeaderText = PulseCatalog.GetString("MachineMode");
      machineObservationStateColumn.HeaderText = PulseCatalog.GetString("MachineObservationState");
      shiftColumn.HeaderText = PulseCatalog.GetString("Shift");
      reasonColumn.HeaderText = PulseCatalog.GetString("Reason");
      reasonDetailsColumn.HeaderText = PulseCatalog.GetString("ReasonDetails");
      defaultReasonColumn.HeaderText = PulseCatalog.GetString("DefaultReason");
      reasonMachineAssociationEndColumn.HeaderText = PulseCatalog.GetString("ReasonMachineAssociationEnd");
      manualActivityColumn.HeaderText = PulseCatalog.GetString("ManualActivity");
      manualActivityEndColumn.HeaderText = PulseCatalog.GetString("ManualActivityEnd");
      
      //Selection Dialog
      {
        MonitoredMachineDialog dialog = new MonitoredMachineDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMonitoredMachine>(dialog);
        monitoredMachineColumn.CellTemplate = cell;
      }
      {
        MachineModeDialog dialog = new MachineModeDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMode>(dialog);
        cncMachineModeColumn.CellTemplate = cell;
      }
      {
        MachineModeDialog dialog = new MachineModeDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMode>(dialog);
        machineModeColumn.CellTemplate = cell;
      }
      {
        MachineObservationStateDialog dialog = new MachineObservationStateDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState>(dialog);
        machineObservationStateColumn.CellTemplate = cell;
      }
      {
        ShiftDialog dialog = new ShiftDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShift>(dialog);
        shiftColumn.CellTemplate = cell;
      }
      {
        ReasonDialog dialog = new ReasonDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IReason>(dialog);
        reasonColumn.CellTemplate = cell;
      }

      m_machineStatuss.SortColumns = false;
    }
    #endregion // Constructors
    
    void MachineStatusConfigLoad(object sender, EventArgs e)
    {
      MachineStatusConfigLoad ();
    }
    
    void MachineStatusConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineStatusConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMachineStatus> machineStatuss =
          daoFactory.MachineStatusDAO.FindAll ();

        m_machineStatuss.Clear ();
        foreach(IMachineStatus machineStatus in machineStatuss) {
          m_machineStatuss.Add(machineStatus);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineStatuss;
        bindingSource.AllowNew = false;
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineStatusConfigValidated(object sender, EventArgs e)
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
          IMachineStatus machineStatus = row.DataBoundItem as IMachineStatus;
          if (null == machineStatus) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineStatusDAO.MakePersistent (machineStatus);
        }

        foreach (IMachineStatus machineStatus in m_deleteList) {
          daoFactory.MachineStatusDAO.MakeTransient (machineStatus);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineStatus machineStatus =
        e.Row.DataBoundItem
        as IMachineStatus;
      if (null != machineStatus) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineStatus);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineStatus machineStatus =
          row.DataBoundItem
          as IMachineStatus;
        if (null != machineStatus) {
          m_updateSet.Add (row);
        }
      }
    }

    
    void AddButtonClick(object sender, EventArgs e)
    {
      MonitoredMachineDialog dialog = new MonitoredMachineDialog();
      dialog.Nullable = false;
      if(dialog.ShowDialog() == DialogResult.OK){
        IMachineStatus machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus(dialog.SelectedValue);
        m_machineStatuss.Add(machineStatus);
        m_updateSet.Add(dataGridView.Rows[dataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
      }
    }
    
    #region IConfigControlObserver Implementation
    /// <summary>
    /// Called after (one or more) IMonitoredMachine was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMonitoredMachine> deletedEntities)
    {
      MachineStatusConfigLoad ();
    }
    
    /// <summary>
    /// Called after (one or more) IMonitoredMachine was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMonitoredMachine> updatedEntities)
    {
       //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IMachineMode was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineMode> deletedEntities)
    {
      MachineStatusConfigLoad ();
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
    /// Called after (one or more) IMachineObservationState was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineObservationState> deletedEntities)
    {
      MachineStatusConfigLoad ();
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
    /// Called after (one or more) IShift was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IShift> deletedEntities)
    {
      MachineStatusConfigLoad ();
    }
    
    /// <summary>
    /// Called after (one or more) IShift was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IShift> updatedEntities)
    {
       //Do nothing
    }
    
    /// <summary>
    /// Called after (one or more) IReason was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IReason> deletedEntities)
    {
      MachineStatusConfigLoad ();
    }
    
    /// <summary>
    /// Called after (one or more) IReason was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IReason> updatedEntities)
    {
      //Do nothing
    }
    #endregion
  }
}
