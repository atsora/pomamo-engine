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
  /// Description of CellConfig.
  /// </summary>
  public partial class CellConfig
    : UserControl
    , IConfigControlObservable<ICell>
  {
    #region Members
    SortableBindingList<ICell> m_cells = new SortableBindingList<ICell>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<ICell> m_deleteList =
      new List<ICell> ();

    ISet<IConfigControlObserver<ICell> > m_observers =
      new HashSet<IConfigControlObserver<ICell> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CellConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CellConfig()
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
      
      m_cells.SortColumns = false;
    }
    #endregion // Constructors

    void CellConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("CellConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        IList<ICell> cells =
          daoFactory.CellDAO.FindAll ();

        m_cells.Clear ();
        foreach (ICell cell in cells) {
          m_cells.Add(cell);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_cells;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void CellConfigValidated(object sender, EventArgs e)
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
          ICell cell = row.DataBoundItem as ICell;
          if (null == cell) {
            continue; // The row may have been deleted since
          }
          daoFactory.CellDAO.MakePersistent (cell);
        }

        foreach (ICell cell in m_deleteList) {
          daoFactory.CellDAO.MakeTransient (cell);
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
      ICell cell =
        e.Row.DataBoundItem
        as ICell;
      if (null != cell) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (cell);
      }
    }

    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      ICell cell =
        e.Row.DataBoundItem
        as ICell;
      if (null != cell) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        ICell cell =
          row.DataBoundItem
          as ICell;
        if (null != cell) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateCell ();
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<ICell> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<ICell> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<ICell> deletedEntities)
    {
      foreach (IConfigControlObserver<ICell> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}