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
  /// Description of FieldConfig.
  /// </summary>
  public partial class FieldConfig
    : UserControl
    , IConfigControlObserver<IUnit>
    , IConfigControlObservable<IField>
  {
    #region Members
    SortableBindingList<IField> m_fields = new SortableBindingList<IField>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IField> m_deleteList =
      new List<IField> ();
    
    ISet<IConfigControlObserver<IField> > m_observers =
      new HashSet<IConfigControlObserver<IField> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FieldConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Field");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      codeColumn.HeaderText = PulseCatalog.GetString ("FieldCode");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("FieldDescription");
      typeColumn.HeaderText = PulseCatalog.GetString ("FieldType");
      unitColumn.HeaderText = PulseCatalog.GetString ("Unit");
      stampingDataTypeColumn.HeaderText = PulseCatalog.GetString ("FieldStampingDataType");
      cncDataAggregationTypeColumn.HeaderText = PulseCatalog.GetString ("FieldCncDataAggregationType");
      associatedClassColumn.HeaderText = PulseCatalog.GetString ("FieldAssociatedClass");
      associatedPropertyColumn.HeaderText = PulseCatalog.GetString ("FieldAssociatedProperty");
      averageMinTimeColumn.HeaderText = PulseCatalog.GetString ("FieldAverageMinTime");
      averageMaxDeviationColumn.HeaderText = PulseCatalog.GetString ("FieldAverageMaxDeviation");

      m_fields.SortColumns = false;
      
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        translationKeyColumn.CellTemplate = cell;
      }
      
      {
        UnitDialog dialog =
          new UnitDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IUnit> (dialog);
        unitColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void FieldConfigLoad(object sender, EventArgs e)
    {
      FieldConfigLoad ();
    }

    void FieldConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("FieldConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IField> fields =
          daoFactory.FieldDAO.FindAll ();

        m_fields.Clear ();
        foreach(IField field in fields) {
          m_fields.Add(field);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_fields;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void FieldConfigValidated(object sender, EventArgs e)
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
          IField field = row.DataBoundItem as IField;
          if (null == field) {
            continue; // The row may have been deleted since
          }
          daoFactory.FieldDAO.MakePersistent (field);
        }

        foreach (IField field in m_deleteList) {
          daoFactory.FieldDAO.MakeTransient (field);
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
      IField field =
        e.Row.DataBoundItem
        as IField;
      if (null != field) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (field);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IField field =
        e.Row.DataBoundItem
        as IField;
      if (null != field) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IField field =
          row.DataBoundItem
          as IField;
        if (null != field) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateFieldFromName ("NewFieldCode", "NewFieldName");
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the UnitConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IUnit> deletedEntities)
    {
      FieldConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the UnitConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IUnit> updatedEntities)
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
    public void AddObserver (IConfigControlObserver<IField> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    ///
    /// This is the implementation of IConfigControlObservable"
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IField> observer)
    {
      m_observers.Remove (observer);
    }
    
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    /// <param name="deletedEntities"></param>
    void NotifyDelete (IList<IField> deletedEntities)
    {
      foreach (IConfigControlObserver<IField> observer in m_observers) {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
