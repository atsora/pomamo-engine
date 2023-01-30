// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
  /// Description of FieldLegendConfig.
  /// </summary>
  public partial class FieldLegendConfig
    : UserControl
    , IConfigControlObserver<IField>
  {
    #region Members
    SortableBindingList<IFieldLegend> m_fieldLegends = new SortableBindingList<IFieldLegend>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IFieldLegend> m_deleteList =
      new List<IFieldLegend> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldLegendConfig).FullName);
    
    #region Getters / Setters
    IField Field {
      get
      {
        if (0 == fieldSelection1.SelectedFields.Count) {
          return null;
        }
        else {
          Debug.Assert (1 == fieldSelection1.SelectedFields.Count);
          return fieldSelection1.SelectedFields [0];
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FieldLegendConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("FieldLegend");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      stringValueColumn.HeaderText = PulseCatalog.GetString ("FieldLegendStringValue");
      minValueColumn.HeaderText = PulseCatalog.GetString ("MinValue");
      maxValueColumn.HeaderText = PulseCatalog.GetString ("MaxValue");
      textColumn.HeaderText = PulseCatalog.GetString ("Text");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");

      m_fieldLegends.SortColumns = false;
    }
    #endregion // Constructors
    
    void FieldLegendConfigLoad(object sender, EventArgs e)
    {
      FieldLegendConfigLoad ();
    }
    
    void FieldLegendConfigLoad()
    {
      LoadDataGridView ();
    }

    void LoadDataGridView ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("FieldLegendConfigLoad: " +
                         "no DAO factory is defined");
        dataGridView.Visible = true;
        return;
      }

      if (null == this.Field) {
        // Nothing to display
        dataGridView.Visible = false;
        return;
      }
            
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IFieldLegend> fieldLegends =
          daoFactory.FieldLegendDAO.FindAllWithField (this.Field);

        m_fieldLegends.Clear ();
        foreach(IFieldLegend fieldLegend in fieldLegends) {
          m_fieldLegends.Add(fieldLegend);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_fieldLegends;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
      
      dataGridView.Visible = true;
    }
    
    void FieldLegendConfigValidated(object sender, EventArgs e)
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
          IFieldLegend fieldLegend = row.DataBoundItem as IFieldLegend;
          if (null == fieldLegend) {
            continue; // The row may have been deleted since
          }
          daoFactory.FieldLegendDAO.MakePersistent (fieldLegend);
        }

        foreach (IFieldLegend fieldLegend in m_deleteList) {
          daoFactory.FieldLegendDAO.MakeTransient (fieldLegend);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IFieldLegend fieldLegend =
        e.Row.DataBoundItem
        as IFieldLegend;
      if (null != fieldLegend) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (fieldLegend);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IFieldLegend fieldLegend =
          row.DataBoundItem
          as IFieldLegend;
        if (null != fieldLegend) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateFieldLegend (this.Field, "New", "#000000");
    }
    
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn") {
        if(e.Value != null){
          e.CellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml(e.Value.ToString());
          e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
          e.CellStyle.SelectionForeColor = Color.Black;
        }
      }
    }
    
    void DataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn") {
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
    
    void FieldSelection1AfterSelect(object sender, EventArgs e)
    {
      CommitChanges ();
      LoadDataGridView ();
    }

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the FieldConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IField> deletedEntities)
    {
      LoadDataGridView ();
    }
    
    /// <summary>
    /// Update this control after some items have been updated
    /// in the FieldConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IField> updatedEntities)
    {
      // Do nothing
    }
    #endregion // IConfigControlObserver implementation
  }
}
