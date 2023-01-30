// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
  /// Description of ShiftTemplateConfig.
  /// </summary>
  public partial class ShiftTemplateConfig : UserControl, IConfigControlObservable<IShiftTemplate>
  {
    #region Members
    SortableBindingList<IShiftTemplate> m_shiftTemplates = new SortableBindingList<IShiftTemplate>();
    
    BindingList<IShiftTemplateItem> m_shiftTemplateItems = null;
    
    BindingList<IShiftTemplateBreak> m_shiftTemplateBreaks = new BindingList<IShiftTemplateBreak>();
    
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IShiftTemplate> m_deleteList = new List<IShiftTemplate> ();
    
    IDictionary<int,IList<IShiftTemplateItem>> m_itemDeleteList = new Dictionary<int,IList<IShiftTemplateItem>>();
    
    ISet<IConfigControlObserver<IShiftTemplate>> m_observers = new HashSet<IConfigControlObserver<IShiftTemplate>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateConfig).FullName);

    #region Getters / Setters
    IShiftTemplate SelectedShiftTemplate {
      get {
        if (shiftTemplateDataGridView.SelectedRows.Count == 1) {
          return shiftTemplateDataGridView.SelectedRows[0].DataBoundItem as IShiftTemplate;
        }

        return null;
      }
    }
    
    IShiftTemplateItem SelectedShiftTemplateItem {
      get {
        if (shiftTemplateItemDataGridView.SelectedRows.Count == 1) {
          return shiftTemplateItemDataGridView.SelectedRows[0].DataBoundItem as IShiftTemplateItem;
        }

        return null;
      }
    }
    
    IShiftTemplateBreak SelectedShiftTemplateBreak{
      get
      {
        if (shiftTemplateBreakDataGridView.SelectedRows.Count == 1) {
          return shiftTemplateBreakDataGridView.SelectedRows[0].DataBoundItem as IShiftTemplateBreak;
        }

        return null;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftTemplateConfig()
    {
      InitializeComponent();
      
      shiftTemplateDataGridView.TopLeftHeaderCell.Value = "Shift template";
      m_shiftTemplates.SortColumns = false;
      
      // ShiftTemplateBreak
      shiftTemplateBreakAddButton.Text = PulseCatalog.GetString ("ShiftTemplateBreakAddButton");
      shiftTemplateBreakGroupBox.Text = PulseCatalog.GetString("ShiftTemplateBreak");
      shiftTemplateBreakTimePeriodColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateBreakTimePeriodColumn");
      shiftTemplateBreakWeekDaysColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateBreakWeekDayColumn");
      shiftTemplateBreakDayColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateBreakDayColumn");
      shiftTemplateBreakIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      shiftTemplateBreakDataGridView.AutoGenerateColumns = false;
      
      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay> (dialog);
        shiftTemplateBreakWeekDaysColumn.CellTemplate = cell;
      }
      {
        TimePeriodOfDayDialog dialog = new TimePeriodOfDayDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimePeriodOfDay?>(dialog);
        shiftTemplateBreakTimePeriodColumn.CellTemplate = cell;
      }
      {
        DateSelectionDialog dialog = new DateSelectionDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<DateTime> (dialog);
        shiftTemplateBreakDayColumn.CellTemplate = cell;
      }
      
      // ShiftTemplateItem
      shiftTemplateItemDayColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemDayColumn");
      shiftTemplateItemTimePeriodOfDayColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemTimePeriodOfDayColumn");
      shiftTemplateItemWeekDaysColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemWeekDaysColumn");
      shiftTemplateItemShiftColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemShiftColumn");
      shiftTemplateItemOrderColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemOrderColumn");
      shiftTemplateItemIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      shiftTemplateItemDataGridView.AutoGenerateColumns = false;

      {
        TimePeriodOfDayDialog dialog = new TimePeriodOfDayDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimePeriodOfDay?>(dialog);
        shiftTemplateItemTimePeriodOfDayColumn.CellTemplate = cell;
      }
      {
        DateSelectionDialog dialog = new DateSelectionDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<DateTime>(dialog);
        shiftTemplateItemDayColumn.CellTemplate = cell;
      }
      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay>(dialog);
        shiftTemplateItemWeekDaysColumn.CellTemplate = cell;
      }
      {
        ShiftDialog dialog = new ShiftDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShift>(dialog);
        shiftTemplateItemShiftColumn.CellTemplate = cell;
      }
      
      // ShiftTemplate
      shiftTemplateNameColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateNameColumn");
      shiftTemplateIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      shiftTemplateDataGridView.AutoGenerateColumns = false;
    }
    #endregion // Constructors
    
    #region ShiftTemplate
    void ShiftTemplateConfigLoad(object sender, EventArgs e)
    {
      ShiftTemplateConfigLoad();
      ShiftTemplateItemLoad();
    }
    
    void ShiftTemplateConfigEnter(object sender, EventArgs e)
    {
      ShiftTemplateConfigLoad();
      ShiftTemplateItemLoad();
    }
    
    void ShiftTemplateConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ShiftTemplateConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IShiftTemplate> shiftTemplates =
          daoFactory.ShiftTemplateDAO.FindAllForConfig ();

        m_shiftTemplates.Clear ();
        foreach(IShiftTemplate shiftTemplate in shiftTemplates) {
          m_shiftTemplates.Add(shiftTemplate);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_shiftTemplates;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        shiftTemplateDataGridView.DataSource = bindingSource;
      }
    }
    
    void ShiftTemplateConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void ShiftTemplateConfigLeave(object sender, EventArgs e)
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
          IShiftTemplate shiftTemplate = row.DataBoundItem as IShiftTemplate;
          if (null == shiftTemplate) {
            continue; // The row may have been deleted since
          }
          daoFactory.ShiftTemplateDAO.MakePersistent (shiftTemplate);
        }
        
        foreach (IShiftTemplate shiftTemplate in m_deleteList) {
          daoFactory.ShiftTemplateDAO.MakeTransient (shiftTemplate);
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
      IShiftTemplate shiftTemplate = e.Row.DataBoundItem as IShiftTemplate;
      if (null != shiftTemplate) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (shiftTemplate);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = shiftTemplateDataGridView.Rows [e.RowIndex];
        IShiftTemplate shiftTemplate =
          row.DataBoundItem
          as IShiftTemplate;
        if (null != shiftTemplate) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateShiftTemplate("");
    }
    
    void ShiftTemplateDataGridViewSelectionChanged(object sender, EventArgs e)
    {
      ShiftTemplateItemLoad();
      ShiftTemplateBreakLoad();
    }
    
    /// <summary>
    /// Add Selected ShiftTemplate to UpdateList
    /// </summary>
    void AddShiftTemplateToUpdate(){
      if(!m_updateSet.Contains(shiftTemplateDataGridView.SelectedRows[0])) {
        m_updateSet.Add(shiftTemplateDataGridView.SelectedRows[0]);
      }
    }
    #endregion

    #region ShiftTemplateItem
    void ShiftTemplateItemLoad(){
      if(SelectedShiftTemplate != null){
        m_shiftTemplateItems = new BindingList<IShiftTemplateItem>(SelectedShiftTemplate.Items.ToList());
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_shiftTemplateItems;
        bindingSource.AllowNew = true;
        shiftTemplateItemDataGridView.DataSource = bindingSource;
      }
      else {
        shiftTemplateItemDataGridView.AutoGenerateColumns = false;
        shiftTemplateItemDataGridView.DataSource = null;
        m_shiftTemplateItems = null;
      }
    }
    
    void ShiftTemplateItemDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IShiftTemplateItem shiftTemplateItem = e.Row.DataBoundItem as IShiftTemplateItem;
      if (null != shiftTemplateItem) {
        AddShiftTemplateToUpdate();
      }
    }
    
    void ShiftTemplateItemDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        AddShiftTemplateToUpdate();
      }
    }
    
    void ShiftTemplateItemAddButtonClick(object sender, EventArgs e)
    {
      if (SelectedShiftTemplate != null) {
        ShiftDialog shiftDialog = new ShiftDialog ();
        shiftDialog.Nullable = false;
        shiftDialog.DisplayedProperty = "Display";
        if (shiftDialog.ShowDialog() == DialogResult.OK) {
          IShiftTemplateItem shiftTemplateItem = SelectedShiftTemplate.AddItem(shiftDialog.SelectedValue);
          shiftTemplateItem.WeekDays = WeekDay.AllDays;
          AddShiftTemplateToUpdate();
          ShiftTemplateItemLoad(); //TODO find better way or lighter
        }
      }
    }
    #endregion

    #region ShiftTemplateBreak
    void ShiftTemplateBreakLoad(){
      if(SelectedShiftTemplate != null){
        
        m_shiftTemplateBreaks.Clear();
        
        foreach (IShiftTemplateBreak msts in SelectedShiftTemplate.Breaks) {
          m_shiftTemplateBreaks.Add(msts);
        }
        
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_shiftTemplateBreaks;
        bindingSource.AllowNew = true;
        shiftTemplateBreakDataGridView.DataSource = bindingSource;
      }
      else {
        shiftTemplateBreakDataGridView.AutoGenerateColumns = false;
        shiftTemplateBreakDataGridView.DataSource = null;
        m_shiftTemplateBreaks.Clear();
      }
    }
    
    void ShiftTemplateBreakDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IShiftTemplateBreak shiftTemplateBreak = e.Row.DataBoundItem as IShiftTemplateBreak;
      if (null != shiftTemplateBreak) {
        SelectedShiftTemplate.Breaks.Remove(shiftTemplateBreak);
        AddShiftTemplateToUpdate();
      }
    }
    
    void ShiftTemplateBreakDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        AddShiftTemplateToUpdate();
      }
    }
    
    void ShiftTemplateBreakAddButtonClick(object sender, EventArgs e)
    {
      IShiftTemplateBreak mSTS = SelectedShiftTemplate.AddBreak();
      mSTS.WeekDays = WeekDay.AllDays;
      AddShiftTemplateToUpdate();
      ShiftTemplateBreakLoad();
    }
    #endregion
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IShiftTemplate> observer)
    {
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IShiftTemplate> observer)
    {
      this.m_observers.Remove(observer);
    }

    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedShiftTemplates"></param>
    void NotifyDelete(IList<IShiftTemplate> deletedShiftTemplates){
      foreach(IConfigControlObserver<IShiftTemplate> observer in m_observers){
        observer.UpdateAfterDelete(deletedShiftTemplates);
      }
    }

    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedShiftTemplates"></param>
    void NotifyUpdate(IList<IShiftTemplate> updatedShiftTemplates){
      foreach(IConfigControlObserver<IShiftTemplate> observer in m_observers){
        observer.UpdateAfterUpdate(updatedShiftTemplates);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
