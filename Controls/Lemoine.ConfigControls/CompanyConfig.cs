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
  /// Description of CompanyConfig.
  /// </summary>
  public partial class CompanyConfig : UserControl, IConfigControlObservable<ICompany>
  {
    #region Members
    SortableBindingList<ICompany> m_companys = new SortableBindingList<ICompany>();    
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<ICompany> m_deleteList =
      new List<ICompany> ();
    
    ISet<IConfigControlObserver<ICompany> > m_observers =
      new HashSet<IConfigControlObserver<ICompany> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CompanyConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CompanyConfig()
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
      
      m_companys.SortColumns = false;
    }
    #endregion // Constructors

    void CompanyConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("CompanyConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IList<ICompany> companies = 
          daoFactory.CompanyDAO.FindAll ();
        
        m_companys.Clear ();
        foreach(ICompany company in companies) {
          m_companys.Add(company);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_companys;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void CompanyConfigValidated(object sender, EventArgs e)
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
          ICompany company = row.DataBoundItem as ICompany;
          if (null == company) {
            continue; // The row may have been deleted since
          }
          daoFactory.CompanyDAO.MakePersistent (company);
        }
        
        foreach (ICompany company in m_deleteList) {
          daoFactory.CompanyDAO.MakeTransient (company);
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
      ICompany company =
        e.Row.DataBoundItem
        as ICompany;
      if (null != company) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (company);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      ICompany company =
        e.Row.DataBoundItem
        as ICompany;
      if (null != company) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        ICompany company =
          row.DataBoundItem
          as ICompany;
        if (null != company) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateCompany ();
    }

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<ICompany> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    ///
    /// This is the implementation of IConfigControlObservable"
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<ICompany> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    /// <param name="deletedEntities"></param>
    void NotifyDelete (IList<ICompany> deletedEntities)
    {
      foreach (IConfigControlObserver<ICompany> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}