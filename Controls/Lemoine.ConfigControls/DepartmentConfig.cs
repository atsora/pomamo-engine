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
  /// Description of DepartmentConfig.
  /// </summary>
  public partial class DepartmentConfig
    : UserControl
    , IConfigControlObservable<IDepartment>
  {
    #region Members
    SortableBindingList<IDepartment> m_departments = new SortableBindingList<IDepartment>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IDepartment> m_deleteList =
      new List<IDepartment> ();

    ISet<IConfigControlObserver<IDepartment> > m_observers =
      new HashSet<IConfigControlObserver<IDepartment> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DepartmentConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DepartmentConfig()
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
      
      m_departments.SortColumns = false;
    }
    #endregion // Constructors

    void DepartmentConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("DepartmentConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        IList<IDepartment> departments =
          daoFactory.DepartmentDAO.FindAll ();

        m_departments.Clear ();
        foreach (IDepartment department in departments) {
          m_departments.Add(department);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_departments;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void DepartmentConfigValidated(object sender, EventArgs e)
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
          IDepartment department = row.DataBoundItem as IDepartment;
          if (null == department) {
            continue; // The row may have been deleted since
          }
          daoFactory.DepartmentDAO.MakePersistent (department);
        }

        foreach (IDepartment department in m_deleteList) {
          daoFactory.DepartmentDAO.MakeTransient (department);
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
      IDepartment department =
        e.Row.DataBoundItem
        as IDepartment;
      if (null != department) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (department);
      }
    }

    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IDepartment department =
        e.Row.DataBoundItem
        as IDepartment;
      if (null != department) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IDepartment department =
          row.DataBoundItem
          as IDepartment;
        if (null != department) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateDepartment ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IDepartment> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IDepartment> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IDepartment> deletedEntities)
    {
      foreach (IConfigControlObserver<IDepartment> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}