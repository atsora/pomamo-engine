// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of ShiftConfig.
  /// </summary>
  public partial class ShiftConfig : UserControl
  {
    #region Members
    SortableBindingList<IShift> m_shifts = new SortableBindingList<IShift>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IShift> m_deleteList =
      new List<IShift> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftConfig()
    {
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Shift");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");

      m_shifts.SortColumns = false;
    }
    #endregion // Constructors
    
    void ShiftConfigLoad(object sender, EventArgs e)
    {
      ShiftConfigLoad ();
    }
    
    void ShiftConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ShiftConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IShift> shifts =
          daoFactory.ShiftDAO.FindAll ();

        m_shifts.Clear ();
        foreach(IShift shift in shifts) {
          m_shifts.Add(shift);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_shifts;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void ShiftConfigValidated(object sender, EventArgs e)
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
          IShift shift = row.DataBoundItem as IShift;
          if (null == shift) {
            continue; // The row may have been deleted since
          }
          daoFactory.ShiftDAO.MakePersistent (shift);
        }

        foreach (IShift shift in m_deleteList) {
          daoFactory.ShiftDAO.MakeTransient (shift);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      // (now forbidden)
      IShift shift = e.Row.DataBoundItem as IShift;
      if (null != shift) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (shift);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IShift shift =
          row.DataBoundItem
          as IShift;
        if (null != shift) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateShiftFromName ("");
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
      if (e.RowIndex >= 0 && e.ColumnIndex >= 0) {
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
  }
}
