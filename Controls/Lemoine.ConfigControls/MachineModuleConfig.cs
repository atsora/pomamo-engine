// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
  /// Description of MachineModuleConfig.
  /// </summary>
  public partial class MachineModuleConfig
    : UserControl
    , IConfigControlObserver<IMonitoredMachine>
    , IConfigControlObserver<ICncAcquisition>
    , IConfigControlObservable<IMachineModule>
  {
    #region Members
    SortableBindingList<IMachineModule> m_machineModules = new SortableBindingList<IMachineModule>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineModule> m_deleteList =
      new List<IMachineModule> ();
    
    ISet<IConfigControlObserver<IMachineModule> > m_observers =
      new HashSet<IConfigControlObserver<IMachineModule> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModuleConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineModule");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      monitoredMachineColumn.HeaderText = PulseCatalog.GetString ("Machine");
      cncAcquisitionColumn.HeaderText = PulseCatalog.GetString ("CncAcquisition");
      configPrefixColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionConfigPrefix");
      configParametersColumn.HeaderText = PulseCatalog.GetString ("CncAcquisitionConfigParameters");
      SequenceVariableColumn.HeaderText = PulseCatalog.GetString ("SequenceVariable");
      CycleVariableColumn.HeaderText = PulseCatalog.GetString ("CycleVariable");
      startCycleVariableColumn.HeaderText = PulseCatalog.GetString ("StartCycleVariable");
      sequenceDetectionMethodColumn.HeaderText = PulseCatalog.GetString ("SequenceDetectionMethod");
      cycleDetectionMethodColumn.HeaderText = PulseCatalog.GetString ("CycleDetectionMethod");
      startCycleDetectionMethodColumn.HeaderText = PulseCatalog.GetString ("StartCycleDetectionMethod");
      detectionMethodVariableColumn.HeaderText = PulseCatalog.GetString ("DetectionMethodVariable");

      m_machineModules.SortColumns = false;

      {
        MonitoredMachineDialog dialog =
          new MonitoredMachineDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMonitoredMachine> (dialog);
        monitoredMachineColumn.CellTemplate = cell;
      }

      {
        CncAcquisitionDialog dialog =
          new CncAcquisitionDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<ICncAcquisition> (dialog);
        cncAcquisitionColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void MachineModuleConfigLoad(object sender, EventArgs e)
    {
      MachineModuleConfigLoad ();
    }
    
    void MachineModuleConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineModuleConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMachineModule> machineModules =
          daoFactory.MachineModuleDAO.FindAllForConfig ();

        m_machineModules.Clear ();
        foreach(IMachineModule machineModule in machineModules) {
          m_machineModules.Add(machineModule);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineModules;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineModuleConfigValidated(object sender, EventArgs e)
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
          IMachineModule machineModule = row.DataBoundItem as IMachineModule;
          if (null == machineModule) {
            continue; // The row may have been deleted since
          }
          if (AddNotNullProperties (machineModule)) {
            daoFactory.MachineModuleDAO.MakePersistent (machineModule);
          }
        }

        foreach (IMachineModule machineModule in m_deleteList) {
          daoFactory.MachineModuleDAO.MakeTransient (machineModule);
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
      IMachineModule machineModule =
        e.Row.DataBoundItem
        as IMachineModule;
      if (null != machineModule) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineModule);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IMachineModule machineModule =
        e.Row.DataBoundItem
        as IMachineModule;
      if (null != machineModule) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineModule machineModule =
          row.DataBoundItem
          as IMachineModule;
        if (null != machineModule) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            AddNotNullProperties (machineModule);
          }
          m_updateSet.Add (row);
        }
      }
    }

    bool AddNotNullProperties (IMachineModule machineModule)
    {
      if (null == machineModule.MonitoredMachine) {
        MonitoredMachineDialog dialog =
          new MonitoredMachineDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        if (DialogResult.OK == dialog.ShowDialog ()) {
          machineModule.MonitoredMachine = dialog.SelectedValue;
          return true;
        }
        else {
          return false;
        }
      }
      else {
        return true;
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = Lemoine.ModelDAO.ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (null, "New machine module");
    }

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MonitoredMachine control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMonitoredMachine> deletedEntities)
    {
      MachineModuleConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated or inserted
    /// in the MonitoredMachine control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMonitoredMachine> updatedEntities)
    {
      MachineModuleConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the CncAcquisition control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<ICncAcquisition> deletedEntities)
    {
      MachineModuleConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated or inserted
    /// in the MonitoredMachine control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<ICncAcquisition> updatedEntities)
    {
      MachineModuleConfigLoad ();
    }
    #endregion // IConfigControlObserver implementation
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineModule> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    ///
    /// This is the implementation of IConfigControlObservable"
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineModule> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after an update or an insert
    /// </summary>
    /// <param name="updatedEntities"></param>
    void NotifyUpdate (ICollection<IMachineModule> updatedEntities)
    {
      foreach (IConfigControlObserver<IMachineModule> observer in m_observers)
      {
        observer.UpdateAfterUpdate (updatedEntities);
      }
    }

    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    /// <param name="deletedEntities"></param>
    void NotifyDelete (IList<IMachineModule> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineModule> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
