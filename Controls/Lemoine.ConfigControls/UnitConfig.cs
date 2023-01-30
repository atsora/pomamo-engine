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
  /// Description of UnitConfig.
  /// </summary>
  public partial class UnitConfig
    : UserControl
    , IConfigControlObservable<IUnit>
  {
    #region Members
    SortableBindingList<IUnit> m_units = new SortableBindingList<IUnit>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IUnit> m_deleteList =
      new List<IUnit> ();
    
    ISet<IConfigControlObserver<IUnit> > m_observers =
      new HashSet<IConfigControlObserver<IUnit> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UnitConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UnitConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Unit");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("UnitName");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("UnitTranslationKey");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("UnitDescription");

      m_units.SortColumns = false;
      
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        translationKeyColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void UnitConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("UnitConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IUnit> units =
          daoFactory.UnitDAO.FindAll ();

        m_units.Clear ();
        foreach(IUnit unit in units) {
          m_units.Add(unit);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_units;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void UnitConfigValidated(object sender, EventArgs e)
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
          IUnit unit = row.DataBoundItem as IUnit;
          if (null == unit) {
            continue; // The row may have been deleted since
          }
          daoFactory.UnitDAO.MakePersistent (unit);
        }

        foreach (IUnit unit in m_deleteList) {
          daoFactory.UnitDAO.MakeTransient (unit);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IUnit unit =
        e.Row.DataBoundItem
        as IUnit;
      if (null != unit) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (unit);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IUnit unit =
        e.Row.DataBoundItem
        as IUnit;
      if (null != unit) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IUnit unit =
          row.DataBoundItem
          as IUnit;
        if (null != unit) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateUnit ();
    }

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IUnit> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IUnit> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IUnit> deletedEntities)
    {
      foreach (IConfigControlObserver<IUnit> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
