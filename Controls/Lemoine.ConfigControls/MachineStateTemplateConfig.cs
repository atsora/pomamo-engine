// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
  /// Description of MachineStateTemplateConfig.
  /// </summary>
  public partial class MachineStateTemplateConfig : UserControl, IConfigControlObservable<IMachineStateTemplate>
  {
    #region Members
    SortableBindingList<IMachineStateTemplate> m_machineStateTemplates = new SortableBindingList<IMachineStateTemplate>();
    
    BindingList<IMachineStateTemplateItem> m_machineStateTemplateItems = null;
    
    BindingList<IMachineStateTemplateStop> m_machineStateTemplateStops = new BindingList<IMachineStateTemplateStop>();
    
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IMachineStateTemplate> m_deleteList = new List<IMachineStateTemplate> ();
    
    IDictionary<int,IList<IMachineStateTemplateItem>> m_itemDeleteList = new Dictionary<int,IList<IMachineStateTemplateItem>>();
    IDictionary<int,IList<IMachineStateTemplateStop>> m_stopDeleteList = new Dictionary<int,IList<IMachineStateTemplateStop>>();
    
    ISet<IConfigControlObserver<IMachineStateTemplate>> m_observers = new HashSet<IConfigControlObserver<IMachineStateTemplate>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateConfig).FullName);

    #region Getter / Setter
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
    #endregion //Getter/Setter
    
    #region Constructors
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
        TimeSpanDialog dialog = new TimeSpanDialog();
        dialog.Nullable = true;
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
      machineStateTemplateItemGroupBox.Text = PulseCatalog.GetString("MachineStateTemplateItem");
      machineStateTemplateItemDayColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemDayColumn");
      machineStateTemplateItemTimePeriodOfDayColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemTimePeriodOfDayColumn");
      machineStateTemplateItemWeekDaysColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemWeekDaysColumn");
      machineStateTemplateItemShiftColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemShiftColumn");
      machineStateTemplateItemMachineObservationStateColumn.HeaderText = PulseCatalog.GetString ("MachineStateTemplateItemMachineObservationStateColumn");
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
        MachineObservationStateDialog dialog = new MachineObservationStateDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineObservationState> (dialog);
        machineStateTemplateItemMachineObservationStateColumn.CellTemplate = cell;
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
    }
    #endregion // Constructors
    
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
        AddMachineStateTemplateToUpdate();
      }
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
