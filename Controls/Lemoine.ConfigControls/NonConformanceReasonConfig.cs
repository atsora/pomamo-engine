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
  /// Description of NonConformanceReasonConfig.
  /// </summary>
  public partial class NonConformanceReasonConfig 
    : UserControl
    , IConfigControlObservable<INonConformanceReason>
  {
    #region Members
    SortableBindingList<INonConformanceReason> m_nonConformanceReasons = 
      new SortableBindingList<INonConformanceReason>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<INonConformanceReason> m_deleteList =
      new List<INonConformanceReason> ();
    
    ISet<IConfigControlObserver<INonConformanceReason> > m_observers = 
      new HashSet<IConfigControlObserver<INonConformanceReason>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (NonConformanceReasonConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public NonConformanceReasonConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("NonConformanceReason");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString("Code");
      descriptionColumn.HeaderText = PulseCatalog.GetString("Description");
      
      m_nonConformanceReasons.SortColumns = false;
    }
    #endregion // Constructors
    
    void NonConformanceReasonConfigLoad(object sender, EventArgs e)
    {
      NonConformanceReasonConfigLoad ();
    }
    
    void NonConformanceReasonConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("NonConformanceReasonConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<INonConformanceReason> nonConformanceReasons =
          daoFactory.NonConformanceReasonDAO.FindAll ();

        m_nonConformanceReasons.Clear ();
        foreach(INonConformanceReason nonConformanceReason in nonConformanceReasons) {
          m_nonConformanceReasons.Add(nonConformanceReason);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_nonConformanceReasons;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void NonConformanceReasonConfigValidated(object sender, EventArgs e)
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
          INonConformanceReason nonConformanceReason = row.DataBoundItem as INonConformanceReason;
          if (null == nonConformanceReason) {
            continue; // The row may have been deleted since
          }
          daoFactory.NonConformanceReasonDAO.MakePersistent (nonConformanceReason);
        }

        foreach (INonConformanceReason nonConformanceReason in m_deleteList) {
          daoFactory.NonConformanceReasonDAO.MakeTransient (nonConformanceReason);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      INonConformanceReason nonConformanceReason =
        e.Row.DataBoundItem
        as INonConformanceReason;
      if (null != nonConformanceReason) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (nonConformanceReason);
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateNonConformanceReason("");
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        INonConformanceReason nonConformanceReason =
          row.DataBoundItem
          as INonConformanceReason;
        if (null != nonConformanceReason) {
          m_updateSet.Add (row);
        }
      }
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<INonConformanceReason> observer){
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<INonConformanceReason> observer){
      this.m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedNonConformanceReasons"></param>
    void NotifyDelete(IList<INonConformanceReason> deletedNonConformanceReasons){
      foreach(IConfigControlObserver<INonConformanceReason> observer in m_observers){
        observer.UpdateAfterDelete(deletedNonConformanceReasons);
      }
    }
    
    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedNonConformanceReasons"></param>
    void NotifyUpdate(IList<INonConformanceReason> updatedNonConformanceReasons){
      foreach(IConfigControlObserver<INonConformanceReason> observer in m_observers){
        observer.UpdateAfterUpdate(updatedNonConformanceReasons);
      }
    }
    #endregion
    
  }
}
