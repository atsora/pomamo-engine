// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
  /// Description of DayTemplateConfig.
  /// </summary>
  public partial class DayTemplateConfig : UserControl, IConfigControlObservable<IDayTemplate>
  {
    #region Members
    SortableBindingList<IDayTemplate> m_dayTemplates = new SortableBindingList<IDayTemplate>();
    
    BindingList<IDayTemplateItem> m_dayTemplateItems = null;
    
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IDayTemplate> m_deleteList = new List<IDayTemplate> ();
    
    IDictionary<int,IList<IDayTemplateItem>> m_itemDeleteList = new Dictionary<int,IList<IDayTemplateItem>>();
    
    ISet<IConfigControlObserver<IDayTemplate>> m_observers = new HashSet<IConfigControlObserver<IDayTemplate>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateConfig).FullName);

    #region Getters / Setters
    IDayTemplate SelectedDayTemplate {
      get {
        if (dayTemplateDataGridView.SelectedRows.Count == 1) {
          return dayTemplateDataGridView.SelectedRows[0].DataBoundItem as IDayTemplate;
        }

        return null;
      }
    }
    
    IDayTemplateItem SelectedDayTemplateItem {
      get {
        if (dayTemplateItemDataGridView.SelectedRows.Count == 1) {
          return dayTemplateItemDataGridView.SelectedRows[0].DataBoundItem as IDayTemplateItem;
        }

        return null;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DayTemplateConfig()
    {
      InitializeComponent();
      
      dayTemplateDataGridView.TopLeftHeaderCell.Value = "Day template";
      m_dayTemplates.SortColumns = false;
      
      // DayTemplateItem
      dayTemplateItemWeekDaysColumn.HeaderText = PulseCatalog.GetString ("DayTemplateItemWeekDaysColumn");
      dayTemplateItemCutOffColumn.HeaderText = PulseCatalog.GetString ("DayTemplateItemCutOffColumn");
      dayTemplateItemOrderColumn.HeaderText = PulseCatalog.GetString ("DayTemplateItemOrderColumn");
      dayTemplateItemIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      dayTemplateItemDataGridView.AutoGenerateColumns = false;

      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay>(dialog);
        dayTemplateItemWeekDaysColumn.CellTemplate = cell;
      }
      {
        TimeSpanDialog dialog = new TimeSpanDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimeSpan?>(dialog);
        dayTemplateItemCutOffColumn.CellTemplate = cell;
      }
      
      // DayTemplate
      dayTemplateNameColumn.HeaderText = PulseCatalog.GetString ("DayTemplateNameColumn");
      dayTemplateIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      dayTemplateDataGridView.AutoGenerateColumns = false;
    }
    #endregion // Constructors
    
    #region DayTemplate
    void DayTemplateConfigLoad(object sender, EventArgs e)
    {
      DayTemplateConfigLoad();
      DayTemplateItemLoad();
    }
    
    void DayTemplateConfigEnter(object sender, EventArgs e)
    {
      DayTemplateConfigLoad();
      DayTemplateItemLoad();
    }
    
    void DayTemplateConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("DayTemplateConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IDayTemplate> dayTemplates =
          daoFactory.DayTemplateDAO.FindAllForConfig ();

        m_dayTemplates.Clear ();
        foreach (IDayTemplate dayTemplate in dayTemplates) {
          m_dayTemplates.Add(dayTemplate);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_dayTemplates;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dayTemplateDataGridView.DataSource = bindingSource;
      }
    }
    
    void DayTemplateConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void DayTemplateConfigLeave(object sender, EventArgs e)
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
          IDayTemplate dayTemplate = row.DataBoundItem as IDayTemplate;
          if (null == dayTemplate) {
            continue; // The row may have been deleted since
          }
          daoFactory.DayTemplateDAO.MakePersistent (dayTemplate);
        }
        
        foreach (IDayTemplate dayTemplate in m_deleteList) {
          daoFactory.DayTemplateDAO.MakeTransient (dayTemplate);
        }
        transaction.Commit ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      if (m_deleteList.Count >= 1){
        NotifyDelete(m_deleteList);
      }
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
      m_itemDeleteList.Clear();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IDayTemplate dayTemplate = e.Row.DataBoundItem as IDayTemplate;
      if (null != dayTemplate) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (dayTemplate);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dayTemplateDataGridView.Rows [e.RowIndex];
        IDayTemplate dayTemplate =
          row.DataBoundItem
          as IDayTemplate;
        if (null != dayTemplate) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateDayTemplate();
    }
    
    void DayTemplateDataGridViewSelectionChanged(object sender, EventArgs e)
    {
      DayTemplateItemLoad();
    }
    
    /// <summary>
    /// Add Selected DayTemplate to UpdateList
    /// </summary>
    void AddDayTemplateToUpdate(){
      if(!m_updateSet.Contains(dayTemplateDataGridView.SelectedRows[0])) {
        m_updateSet.Add(dayTemplateDataGridView.SelectedRows[0]);
      }
    }
    #endregion

    #region DayTemplateItem
    void DayTemplateItemLoad(){
      if(SelectedDayTemplate != null){
        m_dayTemplateItems = new BindingList<IDayTemplateItem>(SelectedDayTemplate.Items.ToList());
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_dayTemplateItems;
        bindingSource.AllowNew = true;
        dayTemplateItemDataGridView.DataSource = bindingSource;
      }
      else {
        dayTemplateItemDataGridView.AutoGenerateColumns = false;
        dayTemplateItemDataGridView.DataSource = null;
        m_dayTemplateItems = null;
      }
    }
    
    void DayTemplateItemDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IDayTemplateItem dayTemplateItem = e.Row.DataBoundItem as IDayTemplateItem;
      if (null != dayTemplateItem) {
        AddDayTemplateToUpdate();
      }
    }
    
    void DayTemplateItemDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        AddDayTemplateToUpdate();
      }
    }
    
    void DayTemplateItemAddButtonClick(object sender, EventArgs e)
    {
      if (SelectedDayTemplate != null)
      {
        WeekDayDialog weekDayDialog = new WeekDayDialog ();
        if (weekDayDialog.ShowDialog() == DialogResult.OK)
        {
          TimeSpanDialog timeSpanDialog = new TimeSpanDialog();
          timeSpanDialog.Nullable = false;
          if (timeSpanDialog.ShowDialog() == DialogResult.OK)
          {
            IDayTemplateItem dayTemplateItem = SelectedDayTemplate.AddItem(
              timeSpanDialog.SelectedValue.Value, weekDayDialog.SelectedValue);
            AddDayTemplateToUpdate();
            DayTemplateItemLoad();
          }
        }
      }
    }
    #endregion
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IDayTemplate> observer)
    {
      this.m_observers.Add(observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IDayTemplate> observer)
    {
      this.m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedDayTemplates"></param>
    void NotifyDelete(IList<IDayTemplate> deletedDayTemplates){
      foreach(IConfigControlObserver<IDayTemplate> observer in m_observers){
        observer.UpdateAfterDelete(deletedDayTemplates);
      }
    }
    
    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedDayTemplates"></param>
    void NotifyUpdate(IList<IDayTemplate> updatedDayTemplates){
      foreach(IConfigControlObserver<IDayTemplate> observer in m_observers){
        observer.UpdateAfterUpdate(updatedDayTemplates);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
