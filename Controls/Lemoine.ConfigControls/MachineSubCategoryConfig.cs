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
  /// Description of MachineSubCategoryConfig.
  /// </summary>
  public partial class MachineSubCategoryConfig
    : UserControl
    , IConfigControlObservable<IMachineSubCategory>
  {
    #region Members
    SortableBindingList<IMachineSubCategory> m_machineSubCategorys
      = new SortableBindingList<IMachineSubCategory>();

    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineSubCategory> m_deleteList =
      new List<IMachineSubCategory> ();

    ISet<IConfigControlObserver<IMachineSubCategory> > m_observers =
      new HashSet<IConfigControlObserver<IMachineSubCategory> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineSubCategoryConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineSubCategoryConfig()
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
      
      m_machineSubCategorys.SortColumns = false;
    }
    #endregion // Constructors

    void MachineSubCategoryConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineSubCategoryConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        IList<IMachineSubCategory> machineSubCategories =
          daoFactory.MachineSubCategoryDAO.FindAll ();
        
        m_machineSubCategorys.Clear ();
        foreach(IMachineSubCategory machineSubCategory in machineSubCategories) {
          m_machineSubCategorys.Add(machineSubCategory);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineSubCategorys;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineSubCategoryConfigValidated(object sender, EventArgs e)
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
          IMachineSubCategory machineSubCategory = row.DataBoundItem as IMachineSubCategory;
          if (null == machineSubCategory) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineSubCategoryDAO.MakePersistent (machineSubCategory);
        }

        foreach (IMachineSubCategory machineSubCategory
                 in m_deleteList) {
          daoFactory.MachineSubCategoryDAO.MakeTransient (machineSubCategory);
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
      IMachineSubCategory machineSubCategory =
        e.Row.DataBoundItem
        as IMachineSubCategory;
      if (null != machineSubCategory) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineSubCategory);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IMachineSubCategory machineSubCategory =
        e.Row.DataBoundItem
        as IMachineSubCategory;
      if (null != machineSubCategory) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineSubCategory machineSubCategory =
          row.DataBoundItem
          as IMachineSubCategory;
        if (null != machineSubCategory) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineSubCategory ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineSubCategory> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineSubCategory> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IMachineSubCategory> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineSubCategory> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}