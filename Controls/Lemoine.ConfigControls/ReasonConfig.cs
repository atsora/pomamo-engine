// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Description of ReasonConfig.
  /// </summary>
  public partial class ReasonConfig
    : UserControl
    , IConfigControlObserver<IReasonGroup>
    , IConfigControlObservable<IReason>
  {
    SortableBindingList<IReason> m_reasons
      = new SortableBindingList<IReason>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IReason> m_deleteList =
      new List<IReason> ();
    
    ISet<IConfigControlObserver<IReason> > m_observers =
      new HashSet<IConfigControlObserver<IReason> > ();

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonConfig).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Reason");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("Description");
      descriptionTranslationKeyColumn.HeaderText = PulseCatalog.GetString ("DescriptionTranslationKey");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");
      reportColorColumn.HeaderText = PulseCatalog.GetString ("ReportColor");
      customColorColumn.HeaderText = PulseCatalog.GetString ("CustomColor");
      customReportColorColumn.HeaderText = PulseCatalog.GetString ("CustomReportColor");
      linkOperationDirectionColumn.HeaderText = PulseCatalog.GetString ("LinkOperationDirection");
      reasonGroupColumn.HeaderText = PulseCatalog.GetString ("ReasonGroup");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");
      productionStateColumn.HeaderText = PulseCatalog.GetString ("ProductionState");

      m_reasons.SortColumns = false;
      
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        translationKeyColumn.CellTemplate = cell;
      }
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        descriptionTranslationKeyColumn.CellTemplate = cell;
      }
      {
        ReasonGroupDialog dialog =
          new ReasonGroupDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IReasonGroup> (dialog);
        reasonGroupColumn.CellTemplate = cell;
      }
      {
        ProductionStateDialog dialog =
          new ProductionStateDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IProductionState> (dialog);
        productionStateColumn.CellTemplate = cell;
      }
    }
    
    void ReasonConfigLoad(object sender, EventArgs e)
    {
      ReasonConfigLoad ();
    }
    
    void ReasonConfigLoad ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ReasonConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IReason> reasons = daoFactory.ReasonDAO.FindAllWithReasonGroup ();
        
        m_reasons.Clear ();
        foreach (IReason reason in reasons) {
          m_reasons.Add(reason);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_reasons;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
      dataGridView.Refresh();

    }
    
    void ReasonConfigValidated(object sender, EventArgs e)
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
          IReason reason = row.DataBoundItem as IReason;
          if (null == reason) {
            continue; // The row may have been deleted since
          }
          if (AddNotNullProperties (reason)) {
            daoFactory.ReasonDAO.MakePersistent (reason);
          }
        }

        foreach (IReason reason in m_deleteList) {
          daoFactory.ReasonDAO.MakeTransient (reason);
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
      IReason reason =
        e.Row.DataBoundItem
        as IReason;
      if (null != reason) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (reason);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IReason reason =
        e.Row.DataBoundItem
        as IReason;
      if (null != reason) {
        AddNotNullProperties (reason);
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IReason reason =
          row.DataBoundItem
          as IReason;
        if (null != reason) {
          AddNotNullProperties (reason);
          m_updateSet.Add (row);
        }
      }  
    }
    
    void DataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if(this.dataGridView.Columns[e.ColumnIndex].Name == "customColorColumn"
           || this.dataGridView.Columns[e.ColumnIndex].Name == "customReportColorColumn") {
          ColorDialog colorDialog = new ColorDialog();
          DataGridViewCell selectedCell = this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
          string cellValue = (string)selectedCell.Value;
          if(!String.IsNullOrEmpty(cellValue)){
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(cellValue);
          }
          DialogResult dialogResult = colorDialog.ShowDialog();
          Color selectedColor = Color.White;
          switch(dialogResult){
            case DialogResult.OK:
              {
                selectedColor = colorDialog.Color;
                break;
              }
            case DialogResult.Cancel:
              {
                if(!String.IsNullOrEmpty(cellValue)){
                  selectedColor = System.Drawing.ColorTranslator.FromHtml(cellValue);
                }
                break;
              }
            default:
              {
                selectedColor = Color.White;
                break;
              }
          }
          selectedCell.Style.BackColor = selectedColor;
          selectedCell.Value = "#" + selectedColor.R.ToString("X2") + selectedColor.G.ToString("X2") + selectedColor.B.ToString("X2");
          this.dataGridView.RefreshEdit();
        }
      }
    }
    
    bool AddNotNullProperties (IReason reason)
    {
      if ( (null == reason.ReasonGroup) || (0 == reason.ReasonGroup.Id)) {
        ReasonGroupDialog dialog =
          new ReasonGroupDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        if (DialogResult.OK != dialog.ShowDialog ()) {
          return false;
        }
        else { // OK
          reason.ReasonGroup = dialog.SelectedValue;
        }
      }
      
      return true;
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      IReasonGroup temporaryReasonGroup = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
      e.NewObject = ModelDAOHelper.ModelFactory.CreateReason (temporaryReasonGroup);
    }
    
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(this.dataGridView.Columns[e.ColumnIndex].Name == "customColorColumn" 
         || this.dataGridView.Columns[e.ColumnIndex].Name == "customReportColorColumn") {
        if(e.Value != null){
          e.CellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml(e.Value.ToString());
          e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
          e.CellStyle.SelectionForeColor = Color.Black;
        }
      }
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the ReasonGroupConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IReasonGroup> deletedEntities)
    {
      ReasonConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the ReasonGroupConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IReasonGroup> updatedEntities)
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
    public void AddObserver (IConfigControlObserver<IReason> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IReason> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IReason> deletedEntities)
    {
      foreach (IConfigControlObserver<IReason> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }

  /// <summary>
  /// DataGridViewCell for LinkDirection objects
  /// 
  /// TODO: to remove ?
  /// </summary>
  internal class DataGridViewLinkDirectionCell: DataGridViewTextBoxCell
  {
    static readonly Type DEFAULT_VALUE_TYPE = typeof (LinkDirection);

    static readonly ILog log = LogManager.GetLogger(typeof (DataGridViewLinkDirectionCell).FullName);

    public override Type ValueType
    {
      get
      {
        Type valueType = base.ValueType;
        if (null != valueType) {
          return valueType;
        }
        return DEFAULT_VALUE_TYPE;
      }
    }
    
    protected override object GetFormattedValue(object value,
                                                int rowIndex,
                                                ref DataGridViewCellStyle cellStyle,
                                                TypeConverter valueTypeConverter,
                                                TypeConverter formattedValueTypeConverter,
                                                DataGridViewDataErrorContexts context)
    {
      LinkDirection? linkDirection = value as LinkDirection?;
      if (null != linkDirection) {
        return base.GetFormattedValue (linkDirection.Value,
                                       rowIndex,
                                       ref cellStyle,
                                       valueTypeConverter,
                                       formattedValueTypeConverter,
                                       context);
      }
      else {
        return base.GetFormattedValue ("",
                                       rowIndex,
                                       ref cellStyle,
                                       valueTypeConverter,
                                       formattedValueTypeConverter,
                                       context);
      }
    }
  }
}
