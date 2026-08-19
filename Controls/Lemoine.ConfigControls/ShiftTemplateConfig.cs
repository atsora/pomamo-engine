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

    IShiftTemplate m_editedSubShiftTemplate = null;
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
      groupBox1.Text = PulseCatalog.GetString ("ShiftTemplateItem");
      shiftTemplateItemAddButton.Text = PulseCatalog.GetString ("ShiftTemplateItemAddButton");
      shiftTemplateItemAddSubButton.Text = PulseCatalog.GetString ("ShiftTemplateItemAddSubButton");
      shiftTemplateItemDayColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemDayColumn");
      shiftTemplateItemTimePeriodOfDayColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemTimePeriodOfDayColumn");
      shiftTemplateItemWeekDaysColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemWeekDaysColumn");
      shiftTemplateItemShiftColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemShiftColumn");
      shiftTemplateItemSubShiftTemplateColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemSubShiftTemplateColumn");
      shiftTemplateItemWeekYearColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemWeekYearColumn");
      shiftTemplateItemWeekNumberColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemWeekNumberColumn");
      shiftTemplateItemWeekFrequencyColumn.HeaderText = PulseCatalog.GetString ("ShiftTemplateItemWeekFrequencyColumn");
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
        // Note: not nullable, so that an item always keeps a reference to either
        // a shift or a shift template.
        // To change the kind of an item, remove it and add a new one
        ShiftDialog dialog = new ShiftDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShift>(dialog);
        shiftTemplateItemShiftColumn.CellTemplate = cell;
      }
      {
        // Note: not nullable, so that an item always keeps a reference to either
        // a shift or a shift template.
        // To change the kind of an item, remove it and add a new one
        ShiftTemplateDialog dialog = new ShiftTemplateDialog ();
        dialog.Nullable = false;
        dialog.MultiSelect = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShiftTemplate> (dialog);
        shiftTemplateItemSubShiftTemplateColumn.CellTemplate = cell;
      }
      {
        // An empty cell must be stored as null in the nullable properties, not as DBNull
        var nullableColumns = new DataGridViewColumn[] {
          shiftTemplateItemWeekYearColumn,
          shiftTemplateItemWeekNumberColumn,
          shiftTemplateItemWeekFrequencyColumn
        };
        foreach (var column in nullableColumns) {
          column.DefaultCellStyle.NullValue = null;
          column.DefaultCellStyle.DataSourceNullValue = null;
        }
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
        if (shiftTemplateItemDataGridView.Columns[e.ColumnIndex].Name
            == shiftTemplateItemSubShiftTemplateColumn.Name) {
          CheckSubShiftTemplateCell (shiftTemplateItemDataGridView.Rows[e.RowIndex]);
        }
        AddShiftTemplateToUpdate();
      }
    }

    /// <summary>
    /// Keep the sub shift template that is being edited, so that it can be restored
    /// when the new value would create a cycle
    /// </summary>
    void ShiftTemplateItemDataGridViewCellBeginEdit (object sender, DataGridViewCellCancelEventArgs e)
    {
      m_editedSubShiftTemplate = null;
      if ((0 <= e.RowIndex)
          && (shiftTemplateItemDataGridView.Columns[e.ColumnIndex].Name
              == shiftTemplateItemSubShiftTemplateColumn.Name)) {
        var item = shiftTemplateItemDataGridView.Rows[e.RowIndex].DataBoundItem as IShiftTemplateItem;
        m_editedSubShiftTemplate = item?.SubShiftTemplate;
      }
    }

    /// <summary>
    /// A property of an item may reject the entered value (an invalid week number for example):
    /// display the reason to the user instead of the default technical dialog
    /// </summary>
    void ShiftTemplateItemDataGridViewDataError (object sender, DataGridViewDataErrorEventArgs e)
    {
      log.Error ($"ShiftTemplateItemDataGridViewDataError: invalid value in column {e.ColumnIndex} of row {e.RowIndex}", e.Exception);
      e.Cancel = true;
      e.ThrowException = false;
      MessageBoxShow (PulseCatalog.GetString ("ShiftTemplateItemInvalidValue"));
    }

    /// <summary>
    /// Restore the previous sub shift template of a row when the new one would create a cycle
    /// </summary>
    /// <param name="row"></param>
    void CheckSubShiftTemplateCell (DataGridViewRow row)
    {
      var item = row.DataBoundItem as IShiftTemplateItem;
      if ((null == item) || (null == item.SubShiftTemplate) || (null == SelectedShiftTemplate)) {
        return;
      }

      if (IsCycle (SelectedShiftTemplate, item.SubShiftTemplate)) {
        log.Error ($"CheckSubShiftTemplateCell: {item.SubShiftTemplate} would create a cycle in {SelectedShiftTemplate} => restore the previous value");
        item.SubShiftTemplate = m_editedSubShiftTemplate;
        shiftTemplateItemDataGridView.InvalidateRow (row.Index);
        MessageBoxShow (PulseCatalog.GetString ("ShiftTemplateItemCycle"));
      }
    }

    /// <summary>
    /// Would applying recursively <paramref name="candidate"/> in <paramref name="shiftTemplate"/>
    /// create a cycle ?
    ///
    /// Note: the loaded shift templates are used to walk through the graph,
    /// because their items were fetched eagerly, unlike the ones of a template that comes from a dialog
    /// </summary>
    /// <param name="shiftTemplate">not null</param>
    /// <param name="candidate">not null</param>
    /// <returns></returns>
    bool IsCycle (IShiftTemplate shiftTemplate, IShiftTemplate candidate)
    {
      var shiftTemplatesById = new Dictionary<int, IShiftTemplate> ();
      foreach (var loaded in m_shiftTemplates) {
        shiftTemplatesById[loaded.Id] = loaded;
      }

      var visited = new HashSet<int> ();
      var pending = new Queue<int> ();
      pending.Enqueue (candidate.Id);
      while (0 < pending.Count) {
        var currentId = pending.Dequeue ();
        if (currentId == shiftTemplate.Id) {
          return true;
        }
        if (!visited.Add (currentId)) {
          continue;
        }
        if (shiftTemplatesById.TryGetValue (currentId, out var current)) {
          foreach (var item in current.Items) {
            if (null != item.SubShiftTemplate) {
              pending.Enqueue (item.SubShiftTemplate.Id);
            }
          }
        }
      }
      return false;
    }

    void MessageBoxShow (string message)
    {
      MessageBox.Show (message, PulseCatalog.GetString ("ShiftTemplate"),
                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

    /// <summary>
    /// Add an item that applies recursively another shift template
    /// </summary>
    void ShiftTemplateItemAddSubButtonClick (object sender, EventArgs e)
    {
      if (null == SelectedShiftTemplate) {
        return;
      }

      var shiftTemplateDialog = new ShiftTemplateDialog ();
      shiftTemplateDialog.Nullable = false;
      shiftTemplateDialog.MultiSelect = false;
      shiftTemplateDialog.DisplayedProperty = "Display";

      if (shiftTemplateDialog.ShowDialog () != DialogResult.OK) {
        return;
      }
      var subShiftTemplate = shiftTemplateDialog.SelectedValue;
      if (null == subShiftTemplate) {
        return;
      }
      if (IsCycle (SelectedShiftTemplate, subShiftTemplate)) {
        log.Error ($"ShiftTemplateItemAddSubButtonClick: {subShiftTemplate} would create a cycle in {SelectedShiftTemplate}");
        MessageBoxShow (PulseCatalog.GetString ("ShiftTemplateItemCycle"));
        return;
      }

      var shiftTemplateItem = SelectedShiftTemplate.AddItem (subShiftTemplate);
      shiftTemplateItem.WeekDays = WeekDay.AllDays;

      AddShiftTemplateToUpdate ();
      ShiftTemplateItemLoad ();
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
