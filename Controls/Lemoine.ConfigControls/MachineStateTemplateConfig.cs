// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Iesi.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.Core.Log;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MachineStateTemplateConfig.
  /// </summary>
  public partial class MachineStateTemplateConfig : UserControl, IConfigControlObservable<IMachineStateTemplate>
  {
    SortableBindingList<IMachineStateTemplate> m_machineStateTemplates = new SortableBindingList<IMachineStateTemplate>();
    
    BindingList<IMachineStateTemplateItem> m_machineStateTemplateItems = null;
    
    BindingList<IMachineStateTemplateStop> m_machineStateTemplateStops = new BindingList<IMachineStateTemplateStop>();
    
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IMachineStateTemplate> m_deleteList = new List<IMachineStateTemplate> ();
    
    IDictionary<int,IList<IMachineStateTemplateItem>> m_itemDeleteList = new Dictionary<int,IList<IMachineStateTemplateItem>>();
    IDictionary<int,IList<IMachineStateTemplateStop>> m_stopDeleteList = new Dictionary<int,IList<IMachineStateTemplateStop>>();
    
    ISet<IConfigControlObserver<IMachineStateTemplate>> m_observers = new HashSet<IConfigControlObserver<IMachineStateTemplate>> ();

    /// <summary>
    /// Sub machine state template of the cell that is being edited, to restore it in case of a cycle
    /// </summary>
    IMachineStateTemplate m_editedSubMachineStateTemplate = null;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateConfig).FullName);

    IMachineStateTemplate SelectedMachineStateTemplate {
      get {
        if (machineStateTemplateDataGridView.SelectedRows.Count == 1) {
          return machineStateTemplateDataGridView.SelectedRows[0].DataBoundItem as IMachineStateTemplate;
        }

        return null;
      }
    }
    
    IMachineStateTemplateItem SelectedMachineStateTemplateItem {
      get {
        if (machineStateTemplateItemDataGridView.SelectedRows.Count == 1) {
          return machineStateTemplateItemDataGridView.SelectedRows[0].DataBoundItem as IMachineStateTemplateItem;
        }

        return null;
      }
    }
    
    IMachineStateTemplateStop SelectedMachineStateTemplateStop{
      get
      {
        if (machineStateTemplateStopDataGridView.SelectedRows.Count == 1) {
          return machineStateTemplateStopDataGridView.SelectedRows[0].DataBoundItem as IMachineStateTemplateStop;
        }

        return null;
      }
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateConfig()
    {
      InitializeComponent();
      
      machineStateTemplateDataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineStateTemplate");
      m_machineStateTemplates.SortColumns = false;

      // MachineStateTemplateStop
      machineStateTemplateStopAddButton.Text = PulseCatalog.GetString ("MachineStateTemplateStopAddButton");
      machineStateTemplateStopGroupBox.Text = PulseCatalog.GetString("MachineStateTemplateStop");
      machineStateTemplateStopLocalTimeColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateStopLocalTimeColumn");
      machineStateTemplateStopWeekDaysColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateStopWeekDaysColumn");
      machineStateTemplateStopIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      
      machineStateTemplateStopDataGridView.AutoGenerateColumns = false;
      
      {
        // Note: TimeSpanDialog is a IValueDialog<TimeSpan?>, but with Nullable set to false
        // it never returns a null value, which the not nullable LocalTime requires
        TimeSpanDialog dialog = new TimeSpanDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimeSpan?>(dialog);
        machineStateTemplateStopLocalTimeColumn.CellTemplate = cell;
      }
      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay> (dialog);
        machineStateTemplateStopWeekDaysColumn.CellTemplate = cell;
      }
      
      // MachineStateTemplateItem
      machineStateTemplateItemAddButton.Text = PulseCatalog.GetString ("MachineStateTemplateItemAddButton");
      machineStateTemplateItemAddSubButton.Text = PulseCatalog.GetString ("MachineStateTemplateItemAddSubButton");
      machineStateTemplateItemGroupBox.Text = PulseCatalog.GetString("MachineStateTemplateItem");
      machineStateTemplateItemDayColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemDayColumn");
      machineStateTemplateItemTimePeriodOfDayColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemTimePeriodOfDayColumn");
      machineStateTemplateItemWeekDaysColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemWeekDaysColumn");
      machineStateTemplateItemShiftColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemShiftColumn");
      machineStateTemplateItemMachineObservationStateColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemMachineObservationStateColumn");
      machineStateTemplateItemSubMachineStateTemplateColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemSubMachineStateTemplateColumn");
      machineStateTemplateItemWeekYearColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemWeekYearColumn");
      machineStateTemplateItemWeekNumberColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemWeekNumberColumn");
      machineStateTemplateItemWeekFrequencyColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemWeekFrequencyColumn");
      machineStateTemplateItemYearlyRepeatColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemYearlyRepeatColumn");
      machineStateTemplateItemOrderColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemOrderColumn");
      machineStateTemplateItemIdColumn.HeaderText = PulseCatalog.GetString ("Id");

      machineStateTemplateItemDataGridView.AutoGenerateColumns = false;

      {
        TimePeriodOfDayDialog dialog = new TimePeriodOfDayDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimePeriodOfDay?>(dialog);
        machineStateTemplateItemTimePeriodOfDayColumn.CellTemplate = cell;
      }
      {
        DateSelectionDialog dialog = new DateSelectionDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<DateTime>(dialog);
        machineStateTemplateItemDayColumn.CellTemplate = cell;
      }
      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay>(dialog);
        machineStateTemplateItemWeekDaysColumn.CellTemplate = cell;
      }
      {
        ShiftDialog dialog = new ShiftDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShift>(dialog);
        machineStateTemplateItemShiftColumn.CellTemplate = cell;
      }
      {
        // An empty cell must be stored as null in the nullable properties, not as DBNull
        var nullableColumns = new DataGridViewColumn[] {
          machineStateTemplateItemWeekYearColumn,
          machineStateTemplateItemWeekNumberColumn,
          machineStateTemplateItemWeekFrequencyColumn
        };
        foreach (var column in nullableColumns) {
          column.DefaultCellStyle.NullValue = null;
          column.DefaultCellStyle.DataSourceNullValue = null;
        }
      }
      {
        MachineObservationStateDialog dialog = new MachineObservationStateDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState> (dialog);
        machineStateTemplateItemMachineObservationStateColumn.CellTemplate = cell;
      }
      {
        // Note: not nullable, so that an item always keeps a reference to either
        // a machine observation state or a machine state template.
        // To change the kind of an item, remove it and add a new one
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog ();
        dialog.Nullable = false;
        dialog.MultiSelect = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate> (dialog);
        machineStateTemplateItemSubMachineStateTemplateColumn.CellTemplate = cell;
      }

      //MachineStateTemplate
      categoryColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateCategory");
      machineStateTemplateSiteAttendanceChangeColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateSiteAttendanceChangeColumn");
      machineStateTemplateOnSiteColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateOnSiteColumn");
      machineStateTemplateShiftRequiredColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateShiftRequiredColumn");
      machineStateTemplateUserRequiredColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateUserRequiredColumn");
      linkOperationDirectionColumn.HeaderText = PulseCatalog.GetString ("LinkOperationDirection");
      machineStateTemplateNameColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateNameColumn");
      machineStateTemplateTranslationkeyColum.HeaderText = PulseCatalog.GetString ("MachineStateTemplateTranslationkeyColum");
      machineStateTemplateIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      machineStateTemplateColorColumn.HeaderText = PulseCatalog.GetString ("Color", "Color");
      dynamicEndColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateDynamicEnd", "Dynamic End");
      nextMachineStateTemplateColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateNextMachineStateTemplate", "Next Machine State Template");
      
      machineStateTemplateDataGridView.AutoGenerateColumns = false;
      
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog ();
        dialog.Nullable = true;
        dialog.MultiSelect = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate> (dialog);
        machineStateTemplateSiteAttendanceChangeColumn.CellTemplate = cell;
      }
      {
        TranslationKeyDialog dialog = new TranslationKeyDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<String>(dialog);
        machineStateTemplateTranslationkeyColum.CellTemplate = cell;
      }
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog ();
        dialog.Nullable = true;
        dialog.MultiSelect = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate> (dialog);
        nextMachineStateTemplateColumn.CellTemplate = cell;
      }
    }
    
    #region MachineStateTemplate
    void MachineStateTemplateConfigLoad(object sender, EventArgs e)
    {
      MachineStateTemplateConfigLoad();
      MachineStateTemplateItemLoad();
      MachineStateTemplateStopLoad();
    }
    
    void MachineStateTemplateConfigEnter(object sender, EventArgs e)
    {
      MachineStateTemplateConfigLoad();
      MachineStateTemplateItemLoad();
      MachineStateTemplateStopLoad();
    }
    
    void MachineStateTemplateConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineStateTemplateConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMachineStateTemplate> machineStateTemplates =
          daoFactory.MachineStateTemplateDAO.FindAllForConfig ();

        m_machineStateTemplates.Clear ();
        foreach(IMachineStateTemplate machineStateTemplate in machineStateTemplates) {
          m_machineStateTemplates.Add(machineStateTemplate);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineStateTemplates;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        machineStateTemplateDataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineStateTemplateConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void MachineStateTemplateConfigLeave(object sender, EventArgs e)
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
          IMachineStateTemplate machineStateTemplate = row.DataBoundItem as IMachineStateTemplate;
          if (null == machineStateTemplate) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineStateTemplateDAO.MakePersistent (machineStateTemplate);
        }
        
        foreach (IMachineStateTemplate machineStateTemplate in m_deleteList) {
          daoFactory.MachineStateTemplateDAO.MakeTransient (machineStateTemplate);
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
      m_stopDeleteList.Clear();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      e.Cancel = true;

      /*IMachineStateTemplate machineStateTemplate = e.Row.DataBoundItem as IMachineStateTemplate;
      if (null != machineStateTemplate) {
        bool isMachineStateTemplateLinked = false;
        foreach(IMachineStateTemplate machineStateTemplateToCmp in m_machineStateTemplates){
          if(machineStateTemplate.Equals(machineStateTemplateToCmp.SiteAttendanceChange) && machineStateTemplate.Id != machineStateTemplateToCmp.Id){
            isMachineStateTemplateLinked = true;
          }
        }
        if(isMachineStateTemplateLinked){
          MachineStateTemplateWarningDialog dialog = new MachineStateTemplateWarningDialog();
          dialog.Title = PulseCatalog.GetString("MachineStateTemplateDataIntegrityDialogWarningTitle");
          dialog.Message = PulseCatalog.GetString("MachineStateTemplateDataIntegrityDialogWarningText");
          DialogResult dialogResult = dialog.ShowDialog();
          switch(dialogResult){
            case DialogResult.OK:
              {
                foreach(DataGridViewRow dataGridRow in this.machineStateTemplateDataGridView.Rows){
                  IMachineStateTemplate machineStateTemplateToCmp = dataGridRow.DataBoundItem as IMachineStateTemplate;
                  if(machineStateTemplateToCmp != null){
                    if(machineStateTemplate.Equals(machineStateTemplateToCmp.SiteAttendanceChange) && machineStateTemplate.Id != machineStateTemplateToCmp.Id){
                      machineStateTemplateToCmp.SiteAttendanceChange = null;
                      m_updateSet.Add(dataGridRow);
                    }
                  }
                }
                m_updateSet.Remove (e.Row);
                m_deleteList.Add (machineStateTemplate);
                break;
              }
            case DialogResult.Cancel:
              {
                e.Cancel = true;
                break;
              }
            default:
              {
                e.Cancel = true;
                break;
              }
          }
        }
        else {
          m_updateSet.Remove (e.Row);
          m_deleteList.Add (machineStateTemplate);
        }
      }*/
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = machineStateTemplateDataGridView.Rows [e.RowIndex];
        IMachineStateTemplate machineStateTemplate =
          row.DataBoundItem
          as IMachineStateTemplate;
        if (null != machineStateTemplate) {
          m_updateSet.Add (row);
        }
      }
    }

    void DataGridViewCellDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if (this.machineStateTemplateDataGridView.Columns[e.ColumnIndex].Name == "machineStateTemplateColorColumn"
           || this.machineStateTemplateDataGridView.Columns[e.ColumnIndex].Name == "machineStateTemplateColorColumn") {
          ColorDialog colorDialog = new ColorDialog ();
          DataGridViewCell selectedCell = this.machineStateTemplateDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
          string cellValue = (string)selectedCell.Value;
          if (!String.IsNullOrEmpty (cellValue)) {
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml (cellValue);
          }
          DialogResult dialogResult = colorDialog.ShowDialog ();
          Color selectedColor = Color.White;
          switch (dialogResult) {
            case DialogResult.OK: {
                selectedColor = colorDialog.Color;
                break;
              }
            case DialogResult.Cancel: {
                if (!String.IsNullOrEmpty (cellValue)) {
                  selectedColor = System.Drawing.ColorTranslator.FromHtml (cellValue);
                }
                break;
              }
            default: {
                selectedColor = Color.White;
                break;
              }
          }
          selectedCell.Style.BackColor = selectedColor;
          selectedCell.Value = "#" + selectedColor.R.ToString ("X2") + selectedColor.G.ToString ("X2") + selectedColor.B.ToString ("X2");
          this.machineStateTemplateDataGridView.RefreshEdit ();
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate("");
    }
    
    void MachineStateTemplateDataGridViewSelectionChanged(object sender, EventArgs e)
    {
      MachineStateTemplateItemLoad();
      MachineStateTemplateStopLoad();
    }
    
    /// <summary>
    /// Add Selected MachineStateTemplate to UpdateList
    /// </summary>
    void AddMachineStateTemplateToUpdate(){
      if(!m_updateSet.Contains(machineStateTemplateDataGridView.SelectedRows[0])) {
        m_updateSet.Add(machineStateTemplateDataGridView.SelectedRows[0]);
      }
    }
    #endregion
    
    #region MachineStateTemplateItem
    void MachineStateTemplateItemLoad(){
      if(SelectedMachineStateTemplate != null){
        m_machineStateTemplateItems = new BindingList<IMachineStateTemplateItem>(SelectedMachineStateTemplate.Items);
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineStateTemplateItems;
        bindingSource.AllowNew = true;
        machineStateTemplateItemDataGridView.DataSource = bindingSource;
      }
      else {
        machineStateTemplateItemDataGridView.AutoGenerateColumns = false;
        machineStateTemplateItemDataGridView.DataSource = null;
        m_machineStateTemplateItems = null;
      }
    }
    
    void MachineStateTemplateItemDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineStateTemplateItem machineStateTemplateItem = e.Row.DataBoundItem as IMachineStateTemplateItem;
      if (null != machineStateTemplateItem) {
        AddMachineStateTemplateToUpdate();
      }
    }
    
    void MachineStateTemplateItemDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if (machineStateTemplateItemDataGridView.Columns[e.ColumnIndex].Name
            == machineStateTemplateItemSubMachineStateTemplateColumn.Name) {
          CheckSubMachineStateTemplateCell (machineStateTemplateItemDataGridView.Rows[e.RowIndex]);
        }
        AddMachineStateTemplateToUpdate();
      }
    }

    /// <summary>
    /// Keep the sub machine state template that is being edited, so that it can be restored
    /// when the new value would create a cycle
    /// </summary>
    void MachineStateTemplateItemDataGridViewCellBeginEdit (object sender, DataGridViewCellCancelEventArgs e)
    {
      m_editedSubMachineStateTemplate = null;
      if ((0 <= e.RowIndex)
          && (machineStateTemplateItemDataGridView.Columns[e.ColumnIndex].Name
              == machineStateTemplateItemSubMachineStateTemplateColumn.Name)) {
        var item = machineStateTemplateItemDataGridView.Rows[e.RowIndex].DataBoundItem as IMachineStateTemplateItem;
        m_editedSubMachineStateTemplate = item?.SubMachineStateTemplate;
      }
    }

    /// <summary>
    /// A property of an item may reject the entered value (an invalid week number for example):
    /// display the reason to the user instead of the default technical dialog
    /// </summary>
    void MachineStateTemplateItemDataGridViewDataError (object sender, DataGridViewDataErrorEventArgs e)
    {
      log.Error ($"MachineStateTemplateItemDataGridViewDataError: invalid value in column {e.ColumnIndex} of row {e.RowIndex}", e.Exception);
      e.Cancel = true;
      e.ThrowException = false;
      MessageBoxShow (PulseCatalog.GetString ("MachineStateTemplateItemInvalidValue"));
    }

    /// <summary>
    /// Restore the previous sub machine state template of a row when the new one would create a cycle
    /// </summary>
    /// <param name="row"></param>
    void CheckSubMachineStateTemplateCell (DataGridViewRow row)
    {
      var item = row.DataBoundItem as IMachineStateTemplateItem;
      if ((null == item) || (null == item.SubMachineStateTemplate) || (null == SelectedMachineStateTemplate)) {
        return;
      }

      if (IsCycle (SelectedMachineStateTemplate, item.SubMachineStateTemplate)) {
        log.Error ($"CheckSubMachineStateTemplateCell: {item.SubMachineStateTemplate} would create a cycle in {SelectedMachineStateTemplate} => restore the previous value");
        item.SubMachineStateTemplate = m_editedSubMachineStateTemplate;
        machineStateTemplateItemDataGridView.InvalidateRow (row.Index);
        MessageBoxShow (PulseCatalog.GetString ("MachineStateTemplateItemCycle"));
      }
    }

    /// <summary>
    /// Would applying recursively <paramref name="candidate"/> in <paramref name="machineStateTemplate"/>
    /// create a cycle ?
    ///
    /// Note: the loaded machine state templates are used to walk through the graph,
    /// because their items were fetched eagerly, unlike the ones of a template that comes from a dialog
    /// </summary>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="candidate">not null</param>
    /// <returns></returns>
    bool IsCycle (IMachineStateTemplate machineStateTemplate, IMachineStateTemplate candidate)
    {
      var machineStateTemplatesById = new Dictionary<int, IMachineStateTemplate> ();
      foreach (var loaded in m_machineStateTemplates) {
        machineStateTemplatesById[loaded.Id] = loaded;
      }

      var visited = new HashSet<int> ();
      var pending = new Queue<int> ();
      pending.Enqueue (candidate.Id);
      while (0 < pending.Count) {
        var currentId = pending.Dequeue ();
        if (currentId == machineStateTemplate.Id) {
          return true;
        }
        if (!visited.Add (currentId)) {
          continue;
        }
        if (machineStateTemplatesById.TryGetValue (currentId, out var current)) {
          foreach (var item in current.Items) {
            if (null != item.SubMachineStateTemplate) {
              pending.Enqueue (item.SubMachineStateTemplate.Id);
            }
          }
        }
      }
      return false;
    }

    void MessageBoxShow (string message)
    {
      MessageBox.Show (message, PulseCatalog.GetString ("MachineStateTemplate"),
                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    void MachineStateTemplateItemAddButtonClick(object sender, EventArgs e)
    {
      MachineObservationStateDialog machineObersvationStateDialog = new MachineObservationStateDialog ();
      machineObersvationStateDialog.Nullable = false;
      machineObersvationStateDialog.DisplayedProperty = "Display";

      if(machineObersvationStateDialog.ShowDialog() == DialogResult.OK){
        if(SelectedMachineStateTemplate != null){
          OrderDialog orderDialog = new OrderDialog();
          orderDialog.Nullable = false;
          orderDialog.MinimumIndex = 0;
          orderDialog.MaximumIndex = SelectedMachineStateTemplate.Items.Count;

          IMachineStateTemplateItem machineStateTemplateItem = null;

          if(orderDialog.ShowDialog() == DialogResult.OK && orderDialog.UserSpecifiedIndex){
            machineStateTemplateItem = SelectedMachineStateTemplate.InsertItem(orderDialog.SelectedValue,machineObersvationStateDialog.SelectedValue);
          }
          else {
            machineStateTemplateItem = SelectedMachineStateTemplate.AddItem(machineObersvationStateDialog.SelectedValue);
          }

          if(SelectedMachineStateTemplate.ShiftRequired){
            ShiftDialog shiftDialog = new ShiftDialog ();
            shiftDialog.Nullable = false;
            shiftDialog.DisplayedProperty = "Display";
            if(shiftDialog.ShowDialog() == DialogResult.OK){
              machineStateTemplateItem.Shift = shiftDialog.SelectedValue;
            }
          }

          machineStateTemplateItem.WeekDays = WeekDay.AllDays;

          AddMachineStateTemplateToUpdate();
          MachineStateTemplateItemLoad(); //TODO find better way or lighter
        }
      }
    }

    /// <summary>
    /// Add an item that applies recursively another machine state template
    /// </summary>
    void MachineStateTemplateItemAddSubButtonClick (object sender, EventArgs e)
    {
      if (null == SelectedMachineStateTemplate) {
        return;
      }

      var machineStateTemplateDialog = new MachineStateTemplateDialog ();
      machineStateTemplateDialog.Nullable = false;
      machineStateTemplateDialog.MultiSelect = false;
      machineStateTemplateDialog.DisplayedProperty = "Display";

      if (machineStateTemplateDialog.ShowDialog () != DialogResult.OK) {
        return;
      }
      var subMachineStateTemplate = machineStateTemplateDialog.SelectedValue;
      if (null == subMachineStateTemplate) {
        return;
      }
      if (IsCycle (SelectedMachineStateTemplate, subMachineStateTemplate)) {
        log.Error ($"MachineStateTemplateItemAddSubButtonClick: {subMachineStateTemplate} would create a cycle in {SelectedMachineStateTemplate}");
        MessageBoxShow (PulseCatalog.GetString ("MachineStateTemplateItemCycle"));
        return;
      }

      var orderDialog = new OrderDialog ();
      orderDialog.Nullable = false;
      orderDialog.MinimumIndex = 0;
      orderDialog.MaximumIndex = SelectedMachineStateTemplate.Items.Count;

      IMachineStateTemplateItem machineStateTemplateItem;
      if ((orderDialog.ShowDialog () == DialogResult.OK) && orderDialog.UserSpecifiedIndex) {
        machineStateTemplateItem = SelectedMachineStateTemplate.InsertItem (orderDialog.SelectedValue, subMachineStateTemplate);
      }
      else {
        machineStateTemplateItem = SelectedMachineStateTemplate.AddItem (subMachineStateTemplate);
      }
      machineStateTemplateItem.WeekDays = WeekDay.AllDays;

      AddMachineStateTemplateToUpdate ();
      MachineStateTemplateItemLoad ();
    }
    #endregion
    
    #region MachineStateTemplateStop
    void MachineStateTemplateStopLoad(){
      if(SelectedMachineStateTemplate != null){
        
        m_machineStateTemplateStops.Clear();
        
        foreach (IMachineStateTemplateStop msts in SelectedMachineStateTemplate.Stops) {
          m_machineStateTemplateStops.Add(msts);
        }
        
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineStateTemplateStops;
        bindingSource.AllowNew = true;
        machineStateTemplateStopDataGridView.DataSource = bindingSource;
      }
      else {
        machineStateTemplateStopDataGridView.AutoGenerateColumns = false;
        machineStateTemplateStopDataGridView.DataSource = null;
        m_machineStateTemplateStops.Clear();
      }
    }
    
    void MachineStateTemplateStopDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineStateTemplateStop machineStateTemplateStop = e.Row.DataBoundItem as IMachineStateTemplateStop;
      if (null != machineStateTemplateStop) {
        SelectedMachineStateTemplate.Stops.Remove(machineStateTemplateStop);
        AddMachineStateTemplateToUpdate();
      }
    }
    
    void MachineStateTemplateStopDataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        AddMachineStateTemplateToUpdate();
      }
    }
    
    void MachineStateTemplateStopAddButtonClick(object sender, EventArgs e)
    {
      IMachineStateTemplateStop mSTS = SelectedMachineStateTemplate.AddStop();
      mSTS.WeekDays = WeekDay.AllDays;
      AddMachineStateTemplateToUpdate();
      MachineStateTemplateStopLoad();
    }
    #endregion
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineStateTemplate> observer){
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineStateTemplate> observer){
      this.m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedMachineStateTemplates"></param>
    void NotifyDelete(IList<IMachineStateTemplate> deletedMachineStateTemplates){
      foreach(IConfigControlObserver<IMachineStateTemplate> observer in m_observers){
        observer.UpdateAfterDelete(deletedMachineStateTemplates);
      }
    }
    
    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedMachineStateTemplates"></param>
    void NotifyUpdate(IList<IMachineStateTemplate> updatedMachineStateTemplates){
      foreach(IConfigControlObserver<IMachineStateTemplate> observer in m_observers){
        observer.UpdateAfterUpdate(updatedMachineStateTemplates);
      }
    }
    #endregion
  }
}
