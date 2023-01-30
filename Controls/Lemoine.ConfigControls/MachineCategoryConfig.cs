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
  /// Description of MachineCategoryConfig.
  /// </summary>
  public partial class MachineCategoryConfig
    : UserControl
    , IConfigControlObservable<IMachineCategory>
  {
    #region Members
    SortableBindingList<IMachineCategory> m_machineCategorys = new SortableBindingList<IMachineCategory>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineCategory> m_deleteList =
      new List<IMachineCategory> ();

    ISet<IConfigControlObserver<IMachineCategory> > m_observers =
      new HashSet<IConfigControlObserver<IMachineCategory> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineCategoryConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineCategoryConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");
      
      m_machineCategorys.SortColumns = false;
    }
    #endregion // Constructors

    void MachineCategoryConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineCategoryConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        IList<IMachineCategory> machineCategories =
          daoFactory.MachineCategoryDAO.FindAll ();
        
        m_machineCategorys.Clear ();
        foreach(IMachineCategory machineCategory in machineCategories) {
          m_machineCategorys.Add(machineCategory);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineCategorys;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineCategoryConfigValidated(object sender, EventArgs e)
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
          IMachineCategory machineCategory = row.DataBoundItem as IMachineCategory;
          if (null == machineCategory) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineCategoryDAO.MakePersistent (machineCategory);
        }

        foreach (IMachineCategory machineCategory
                 in m_deleteList) {
          daoFactory.MachineCategoryDAO.MakeTransient (machineCategory);
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
      IMachineCategory machineCategory =
        e.Row.DataBoundItem
        as IMachineCategory;
      if (null != machineCategory) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineCategory);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IMachineCategory machineCategory =
        e.Row.DataBoundItem
        as IMachineCategory;
      if (null != machineCategory) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineCategory machineCategory =
          row.DataBoundItem
          as IMachineCategory;
        if (null != machineCategory) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineCategory ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineCategory> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineCategory> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IMachineCategory> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineCategory> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}