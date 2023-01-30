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
  /// Description of EmailConfigConfig.
  /// </summary>
  public partial class EmailConfigConfig
    : UserControl
    , IConfigControlObserver<IEventLevel>
    , IConfigControlObserver<IMachine>
    , IConfigControlObserver<IMachineFilter>
  {
    #region Members
    SortableBindingList<IEmailConfig> m_emailConfigs = new SortableBindingList<IEmailConfig>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IEmailConfig> m_deleteList =
      new List<IEmailConfig> ();
    
    String m_priorityString = PulseCatalog.GetString("Priority");
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EmailConfigConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EmailConfigConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("EmailConfig");
      
      //Column
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      dataTypeColumn.HeaderText = PulseCatalog.GetString("DataType");
      freeFilterColumn.HeaderText = PulseCatalog.GetString("FreeFilter");
      maxLevelPriority.HeaderText = PulseCatalog.GetString("MaxLevelPriority");
      eventLevelColumn.HeaderText = PulseCatalog.GetString("EventLevel");
      machineColumn.HeaderText = PulseCatalog.GetString("Machine");
      machineFilterColumn.HeaderText = PulseCatalog.GetString("MachineFilter");
      toColumn.HeaderText = PulseCatalog.GetString("To");
      ccColumn.HeaderText = PulseCatalog.GetString("Cc");
      bccColumn.HeaderText = PulseCatalog.GetString("Bcc");
      activeColumn.HeaderText = PulseCatalog.GetString("Active");
      weekDaysColumn.HeaderText = PulseCatalog.GetString("WeekDays");
      timePeriodColumn.HeaderText = PulseCatalog.GetString("TimePeriod");
      beginColumn.HeaderText = PulseCatalog.GetString("Begin");
      endColumn.HeaderText = PulseCatalog.GetString("End");
      autoPurgeColumn.HeaderText = PulseCatalog.GetString("AutoPurge");

      m_emailConfigs.SortColumns = false;
      
      {
        EventLevelDialog dialog = new EventLevelDialog();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionTextWithPriority";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IEventLevel>(dialog);
        eventLevelColumn.CellTemplate = cell;
      }
      {
        MachineDialog dialog = new MachineDialog();
        dialog.MultipleSelection = true;
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachine>(dialog);
        machineColumn.CellTemplate = cell;
      }
      {
        MachineFilterDialog dialog = new MachineFilterDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter>(dialog);
        machineFilterColumn.CellTemplate = cell;
      }
      {
        WeekDayDialog dialog = new WeekDayDialog();
        DataGridViewCell cell = new DataGridViewSelectionableCell<WeekDay>(dialog);
        weekDaysColumn.CellTemplate = cell;
      }
      {
        TimePeriodOfDayDialog dialog = new TimePeriodOfDayDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<TimePeriodOfDay?>(dialog);
        timePeriodColumn.CellTemplate = cell;
      }
      {
        DateTimeDialog dialog = new DateTimeDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<DateTime?>(dialog);
        beginColumn.CellTemplate = cell;
      }
      {
        DateTimeDialog dialog = new DateTimeDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<DateTime?>(dialog);
        endColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void EmailConfigConfigLoad(object sender, EventArgs e)
    {
      EmailConfigConfigLoad ();
    }
    
    void EmailConfigConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("EmailConfigConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IEmailConfig> emailConfigs =
          daoFactory.EmailConfigDAO.FindAllForConfig ();

        m_emailConfigs.Clear ();
        foreach(IEmailConfig emailConfig in emailConfigs) {
          m_emailConfigs.Add(emailConfig);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_emailConfigs;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void EmailConfigConfigValidated(object sender, EventArgs e)
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
          IEmailConfig emailConfig = row.DataBoundItem as IEmailConfig;
          if (null == emailConfig) {
            continue; // The row may have been deleted since
          }
          daoFactory.EmailConfigDAO.MakePersistent (emailConfig);
        }

        foreach (IEmailConfig emailConfig in m_deleteList) {
          daoFactory.EmailConfigDAO.MakeTransient (emailConfig);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IEmailConfig emailConfig =
        e.Row.DataBoundItem
        as IEmailConfig;
      if (null != emailConfig) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (emailConfig);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IEmailConfig emailConfig =
          row.DataBoundItem
          as IEmailConfig;
        if (null != emailConfig) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      IEmailConfig emailConfig = ModelDAOHelper.ModelFactory.CreateEmailConfig ();
      emailConfig.MaxLevelPriority = 1000;
      e.NewObject = emailConfig;
    }
    
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(e.ColumnIndex == this.dataGridView.Columns["eventLevelColumn"].Index && e.Value != null){
        DataGridViewCell cell = this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        DataGridViewRow row =  this.dataGridView.Rows[e.RowIndex];
        IEmailConfig emailConfig = row.DataBoundItem as IEmailConfig;
        if(emailConfig != null){
          cell.ToolTipText = String.Format(m_priorityString+" {0}", emailConfig.EventLevel.Priority);
        }
      }
    }
    
    /// <summary>
    /// Hide the FreeFilter Column
    /// Used in AlertConfigGUI
    /// </summary>
    public void HideFreeFilterColumn(){
      this.freeFilterColumn.Visible = false;
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Called after (one or more) entity was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IEventLevel> deletedEntities)
    {
      EmailConfigConfigLoad();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IEventLevel> updatedEntities)
    {
      //Do nothing
    }

    /// <summary>
    /// Called after (one or more) entity was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachine> deletedEntities)
    {
      EmailConfigConfigLoad();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachine> updatedEntities)
    {
      //Do nothing
    }

    /// <summary>
    /// Called after (one or more) entity was deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineFilter> deletedEntities)
    {
      EmailConfigConfigLoad();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineFilter> updatedEntities)
    {
      //Do nothing
    }
    #endregion // IConfigControlObserver implementation
  }
}
