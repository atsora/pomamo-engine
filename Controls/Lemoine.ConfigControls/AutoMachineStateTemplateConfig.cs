// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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
  /// Description of AutoMachineStateTemplateConfig.
  /// </summary>
  public partial class AutoMachineStateTemplateConfig
    : UserControl
    , IConfigControlObservable<IAutoMachineStateTemplate>
    , IConfigControlObserver<IMachineStateTemplate>
  {
    #region Members
    SortableBindingList<IAutoMachineStateTemplate> m_autoMachineStateTemplates = new SortableBindingList<IAutoMachineStateTemplate>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IAutoMachineStateTemplate> m_deleteList =
      new List<IAutoMachineStateTemplate> ();
    
    ISet<IConfigControlObserver<IAutoMachineStateTemplate>> m_observers =
      new HashSet<IConfigControlObserver<IAutoMachineStateTemplate>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AutoMachineStateTemplateConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public AutoMachineStateTemplateConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("AutoMachineStateTemplate");
      
      //I18N
      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      machineModeColumn.HeaderText = PulseCatalog.GetString("MachineMode");
      currentColumn.HeaderText = PulseCatalog.GetString("Current");
      newColumn.HeaderText = PulseCatalog.GetString("New");
      AddButton.Text = PulseCatalog.GetString("Add");
      
      //Dialog Selection
      {
        MachineModeDialog dialog = new MachineModeDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMode>(dialog);
        machineModeColumn.CellTemplate = cell;
      }
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog();
        dialog.Nullable = true;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate>(dialog);
        currentColumn.CellTemplate = cell;
      }
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog();
        dialog.Nullable = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate>(dialog);
        newColumn.CellTemplate = cell;
      }
      m_autoMachineStateTemplates.SortColumns = false;
    }
    #endregion // Constructors
    
    void AutoMachineStateTemplateConfigLoad(object sender, EventArgs e)
    {
      AutoMachineStateTemplateConfigLoad ();
    }
    
    void AutoMachineStateTemplateConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("AutoMachineStateTemplateConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IAutoMachineStateTemplate> autoMachineStateTemplates =
          daoFactory.AutoMachineStateTemplateDAO.FindAll ();

        m_autoMachineStateTemplates.Clear ();
        foreach(IAutoMachineStateTemplate autoMachineStateTemplate in autoMachineStateTemplates) {
          m_autoMachineStateTemplates.Add(autoMachineStateTemplate);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_autoMachineStateTemplates;
        bindingSource.AllowNew = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void AutoMachineStateTemplateConfigValidated(object sender, EventArgs e)
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
          IAutoMachineStateTemplate autoMachineStateTemplate = row.DataBoundItem as IAutoMachineStateTemplate;
          if (null == autoMachineStateTemplate) {
            continue; // The row may have been deleted since
          }
          daoFactory.AutoMachineStateTemplateDAO.MakePersistent (autoMachineStateTemplate);
        }

        foreach (IAutoMachineStateTemplate autoMachineStateTemplate in m_deleteList) {
          daoFactory.AutoMachineStateTemplateDAO.MakeTransient (autoMachineStateTemplate);
        }
        
        transaction.Commit ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      if(m_deleteList.Count >=1){
        NotifyDelete(m_deleteList);
      }
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IAutoMachineStateTemplate autoMachineStateTemplate =
        e.Row.DataBoundItem
        as IAutoMachineStateTemplate;
      if (null != autoMachineStateTemplate) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (autoMachineStateTemplate);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IAutoMachineStateTemplate autoMachineStateTemplate =
          row.DataBoundItem
          as IAutoMachineStateTemplate;
        if (null != autoMachineStateTemplate) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      MachineModeDialog dialog = new MachineModeDialog();
      dialog.Nullable = false;
      if(dialog.ShowDialog() == DialogResult.OK){
        MachineStateTemplateDialog dialogMST = new MachineStateTemplateDialog();
        dialogMST.Nullable = false;
        if(dialogMST.ShowDialog() == DialogResult.OK){
          IAutoMachineStateTemplate autoMachineStateTemplate =
            ModelDAOHelper.ModelFactory.CreateAutoMachineStateTemplate(dialog.SelectedValue,dialogMST.SelectedValue);
          m_autoMachineStateTemplates.Add(autoMachineStateTemplate);
          m_updateSet.Add(dataGridView.Rows[dataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
        }
      }
    }
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IConfigControlObserver<IAutoMachineStateTemplate> observer)
    {
      m_observers.Add(observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IConfigControlObserver<IAutoMachineStateTemplate> observer)
    {
      m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedAutoMachineStateTemplates"></param>
    void NotifyDelete(IList<IAutoMachineStateTemplate> deletedAutoMachineStateTemplates){
      foreach(IConfigControlObserver<IAutoMachineStateTemplate> observer in m_observers){
        observer.UpdateAfterDelete(deletedAutoMachineStateTemplates);
      }
    }
    
    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedAutoMachineStateTemplates"></param>
    void NotifyUpdate(IList<IAutoMachineStateTemplate> updatedAutoMachineStateTemplates){
      foreach(IConfigControlObserver<IAutoMachineStateTemplate> observer in m_observers){
        observer.UpdateAfterUpdate(updatedAutoMachineStateTemplates);
      }
    }
    #endregion
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Called after (one or more) IMachineStateTemplate are deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineStateTemplate> deletedEntities)
    {
      AutoMachineStateTemplateConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineStateTemplate are updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineStateTemplate> updatedEntities)
    {
      //Do nothing
    }
    #endregion
  }
}
