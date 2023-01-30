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
  /// Description of MonitoredMachineConfig.
  /// </summary>
  public partial class MonitoredMachineConfig
    : UserControl
    , IConfigControlObserver<IMachineModule>
    , IConfigControlObserver<IField>
    , IConfigControlObserver<IStampingConfigByName>
    , IConfigControlObservable<IMonitoredMachine>
  {
    #region Members
    SortableBindingList<IDataGridViewMonitoredMachine> m_monitoredMachines = new SortableBindingList<IDataGridViewMonitoredMachine>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    
    ISet<IConfigControlObserver<IMonitoredMachine> > m_observers =
      new HashSet<IConfigControlObserver<IMonitoredMachine> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MonitoredMachineConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MonitoredMachine");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      monitoringTypeColumn.HeaderText = PulseCatalog.GetString ("MachineMonitoringType");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");
      mainMachineModuleColumn.HeaderText = PulseCatalog.GetString ("MainMachineModule");
      mainCncAcquisitionColumn.HeaderText = PulseCatalog.GetString ("MainCncAcquisition");
      performanceFieldColumn.HeaderText = PulseCatalog.GetString ("PerformanceField");
      operationBarColumn.HeaderText = PulseCatalog.GetString ("OperationBar");
      palletChangingDurationColumn.HeaderText = PulseCatalog.GetString ("PalletChangingDuration");
      operationFromCncColumn.HeaderText = PulseCatalog.GetString ("OperationFromCnc");
      stampingConfigColumn.HeaderText = PulseCatalog.GetString ("StampingConfig");
      
      m_monitoredMachines.SortColumns = false;

      {
        var dialog = new MachineModuleDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineModule> (dialog);
        mainMachineModuleColumn.CellTemplate = cell;
      }
      {
        var dialog = new FieldDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        var cell = new DataGridViewSelectionableCell<IField> (dialog);
        performanceFieldColumn.CellTemplate = cell;
      }
      {
        var dialog = new StampingConfigDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        var cell = new DataGridViewSelectionableCell<IStampingConfigByName> (dialog);
        stampingConfigColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void MonitoredMachineConfigLoad(object sender, EventArgs e)
    {
      MonitoredMachineConfigLoad ();
    }
    
    void MonitoredMachineConfigLoad ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.Error ("MonitoredMachineConfigLoad: no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMonitoredMachine> monitoredMachines =
          daoFactory.MonitoredMachineDAO.FindAllForConfig ();

        m_monitoredMachines.Clear ();
        foreach(IMonitoredMachine monitoredMachine in monitoredMachines) {
          m_monitoredMachines.Add((IDataGridViewMonitoredMachine) monitoredMachine);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_monitoredMachines;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MonitoredMachineConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if (0 == m_updateSet.Count) {
        return;
      }
      
      ISet<IMonitoredMachine> notifySet = new HashSet<IMonitoredMachine> ();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IMonitoredMachine monitoredMachine = row.DataBoundItem as IMonitoredMachine;
          if (null == monitoredMachine) {
            continue; // The row may have been deleted since
          }
          AddNotNullProperties (monitoredMachine);
          IMachineModule mainMachineModule = monitoredMachine.MainMachineModule;
          if (null != mainMachineModule.CncAcquisition) {
            daoFactory.CncAcquisitionDAO.MakePersistent (monitoredMachine.MainCncAcquisition);
          }
          if (0 == monitoredMachine.Id) {
            monitoredMachine.MainMachineModule = null;
            daoFactory.MonitoredMachineDAO.MakePersistent (monitoredMachine);
            monitoredMachine.MainMachineModule = mainMachineModule;
          }
          daoFactory.MachineModuleDAO.MakePersistent (monitoredMachine.MainMachineModule);
          daoFactory.MonitoredMachineDAO.MakePersistent (monitoredMachine);
          notifySet.Add (monitoredMachine);
        }
        transaction.Commit ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      NotifyUpdate (notifySet);
      
      m_updateSet.Clear ();
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IMonitoredMachine monitoredMachine =
        e.Row.DataBoundItem
        as IMonitoredMachine;
      if (null != monitoredMachine) {
        AddNotNullProperties (monitoredMachine);
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMonitoredMachine monitoredMachine =
          row.DataBoundItem
          as IMonitoredMachine;
        if (null != monitoredMachine) {
          AddNotNullProperties (monitoredMachine);
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddNotNullProperties (IMonitoredMachine monitoredMachine)
    {
      if (null == monitoredMachine.MonitoringType) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          monitoredMachine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById (2); // Monitored
        }
      }

      if (null == monitoredMachine.MainMachineModule) {
        monitoredMachine.MainMachineModule =
          ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (monitoredMachine, monitoredMachine.Name);
      }
      if ( (null == monitoredMachine.MainMachineModule.CncAcquisition)
          && (0 == monitoredMachine.Id)) { // Only for the new items
        monitoredMachine.MainMachineModule.CncAcquisition =
          ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
        monitoredMachine.MainCncAcquisition.Name = monitoredMachine.Name;
        // Choose a LPost
        ComputerDialog dialog =
          new ComputerDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        dialog.Text = PulseCatalog.GetString ("CncAcquisitionComputerSelectionTitleA") + monitoredMachine.Name;
        if (DialogResult.OK != dialog.ShowDialog ()) {
          // Reset the MainCncAcquisition because a computer is required
          log.WarnFormat ("AddNotNullProperties: " +
                          "do not add a MainCncAcquisition because a computer is required");
          monitoredMachine.MainCncAcquisition = null;
        }
        else { // OK
          monitoredMachine.MainCncAcquisition.Computer = dialog.SelectedValue;
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
    }

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineModule control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineModule> deletedEntities)
    {
      MonitoredMachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the CncAcquisition control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineModule> updatedEntities)
    {
      // Do nothing
    }
    #endregion // IConfigControlObserver implementation
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the Field control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IField> deletedEntities)
    {
      MonitoredMachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the Field control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IField> updatedEntities)
    {
      // Do nothing
    }
    #endregion // IConfigControlObserver implementation

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the StampingConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<IStampingConfigByName> deletedEntities)
    {
      MonitoredMachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the StampingConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<IStampingConfigByName> updatedEntities)
    {
      // Do nothing
    }
    #endregion // IConfigControlObserver implementation

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMonitoredMachine> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    ///
    /// This is the implementation of IConfigControlObservable"
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMonitoredMachine> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after an update or an insert
    /// </summary>
    /// <param name="updatedEntities"></param>
    void NotifyUpdate (ICollection<IMonitoredMachine> updatedEntities)
    {
      foreach (IConfigControlObserver<IMonitoredMachine> observer in m_observers)
      {
        observer.UpdateAfterUpdate (updatedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
    
  }
}
