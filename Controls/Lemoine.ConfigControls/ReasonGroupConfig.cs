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
  /// Description of ReasonGroupConfig.
  /// </summary>
  public partial class ReasonGroupConfig : UserControl, IConfigControlObservable<IReasonGroup>
  {
    #region Members
    SortableBindingList<IReasonGroup> m_reasonGroups
      = new SortableBindingList<IReasonGroup>();

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IReasonGroup> m_deleteList =
      new List<IReasonGroup> ();
    
    ISet<IConfigControlObserver<IReasonGroup> > m_observers =
      new HashSet<IConfigControlObserver<IReasonGroup> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonGroupConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonGroupConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("ReasonGroup");
      
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("Description");
      descriptionTranslationKeyColumn.HeaderText = PulseCatalog.GetString ("DescriptionTranslationKey");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");
      reportColorColumn.HeaderText = PulseCatalog.GetString ("ReportColor");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");

      m_reasonGroups.SortColumns = false;
      
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string>(dialog);
        translationKeyColumn.CellTemplate = cell;
      }
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string>(dialog);
        descriptionTranslationKeyColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void ReasonGroupConfigLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat("ReasonGroupConfigLoad: " +
                        "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession())
      {
        IList<IReasonGroup> reasonGroups = daoFactory.ReasonGroupDAO.FindAll();
        
        m_reasonGroups.Clear ();
        foreach (IReasonGroup reasonGroup in reasonGroups) {
          m_reasonGroups.Add(reasonGroup);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        
        BindingSource bindingSource = new BindingSource();
        bindingSource.DataSource = m_reasonGroups;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void ReasonGroupConfigValidated(object sender, EventArgs e)
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
          IReasonGroup reasonGroup = row.DataBoundItem as IReasonGroup;
          if (null == reasonGroup) {
            continue; // The row may have been deleted since
          }
          daoFactory.ReasonGroupDAO.MakePersistent(reasonGroup);
        }
        
        foreach (IReasonGroup reasonGroup in m_deleteList) {
          daoFactory.ReasonGroupDAO.MakeTransient(reasonGroup);
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
      
      IReasonGroup reasonGroup =
        e.Row.DataBoundItem
        as IReasonGroup;
      if (null != reasonGroup) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (reasonGroup);
      }
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      IReasonGroup reasonGroup =
        e.Row.DataBoundItem
        as IReasonGroup;
      if (null != reasonGroup) {
        m_updateSet.Add (e.Row);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IReasonGroup reasonGroup =
          row.DataBoundItem
          as IReasonGroup;
        if (null != reasonGroup) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
    }
    
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn"
         || this.dataGridView.Columns[e.ColumnIndex].Name == "reportColorColumn") {
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
        if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn"
           || this.dataGridView.Columns[e.ColumnIndex].Name == "reportColorColumn") {
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
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IReasonGroup> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IReasonGroup> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IReasonGroup> deletedEntities)
    {
      foreach (IConfigControlObserver<IReasonGroup> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
