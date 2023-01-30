// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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
  /// Description of MachineStateTemplateFlowConfig.
  /// </summary>
  public partial class MachineStateTemplateFlowConfig 
    : UserControl
    , IConfigControlObserver<IMachineStateTemplate>
  {
    #region Members
    SortableBindingList<IMachineStateTemplateFlow> m_MachineStateTemplateFlows = new SortableBindingList<IMachineStateTemplateFlow>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineStateTemplateFlow> m_deleteList =
      new List<IMachineStateTemplateFlow> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateFlowConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateFlowConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
//      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineStateTemplateFlow");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      machineStateTemplateFromColumn.HeaderText = PulseCatalog.GetString("From");
      machineStateTemplateToColumn.HeaderText = PulseCatalog.GetString("To");
      AddButton.Text = PulseCatalog.GetString("Add");
      
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog();
        dialog.Nullable = false;
        dialog.MultiSelect = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate>(dialog);
        machineStateTemplateFromColumn.CellTemplate = cell;
      }
      {
        MachineStateTemplateDialog dialog = new MachineStateTemplateDialog();
        dialog.Nullable = false;
        dialog.MultiSelect = false;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate>(dialog);
        machineStateTemplateToColumn.CellTemplate = cell;
      }

      m_MachineStateTemplateFlows.SortColumns = false;
    }
    #endregion // Constructors
    
    void MachineStateTemplateFlowConfigLoad(object sender, EventArgs e)
    {
      MachineStateTemplateFlowConfigLoad ();
    }
    
    void MachineStateTemplateFlowConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineStateTemplateFlowConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMachineStateTemplateFlow> MachineStateTemplateFlows =
          daoFactory.MachineStateTemplateFlowDAO.FindAll ();

        m_MachineStateTemplateFlows.Clear ();
        foreach(IMachineStateTemplateFlow machineStateTemplateFlow in MachineStateTemplateFlows) {
          ModelDAOHelper.DAOFactory.Initialize (machineStateTemplateFlow.From);
          ModelDAOHelper.DAOFactory.Initialize (machineStateTemplateFlow.To);
          m_MachineStateTemplateFlows.Add(machineStateTemplateFlow);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_MachineStateTemplateFlows;
        bindingSource.AllowNew = false;
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineStateTemplateFlowConfigValidated(object sender, EventArgs e)
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
          IMachineStateTemplateFlow MachineStateTemplateFlow = row.DataBoundItem as IMachineStateTemplateFlow;
          if (null == MachineStateTemplateFlow) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineStateTemplateFlowDAO.MakePersistent (MachineStateTemplateFlow);
        }

        foreach (IMachineStateTemplateFlow MachineStateTemplateFlow in m_deleteList) {
          daoFactory.MachineStateTemplateFlowDAO.MakeTransient (MachineStateTemplateFlow);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineStateTemplateFlow MachineStateTemplateFlow =
        e.Row.DataBoundItem
        as IMachineStateTemplateFlow;
      if (null != MachineStateTemplateFlow) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (MachineStateTemplateFlow);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineStateTemplateFlow MachineStateTemplateFlow =
          row.DataBoundItem
          as IMachineStateTemplateFlow;
        if (null != MachineStateTemplateFlow) {
          m_updateSet.Add (row);
        }
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      MachineStateTemplateDialog dialogFrom = new MachineStateTemplateDialog();
      dialogFrom.Nullable = false;
      dialogFrom.MultiSelect = false;
      dialogFrom.Text = PulseCatalog.GetString("From");
      if(dialogFrom.ShowDialog() == DialogResult.OK){
        MachineStateTemplateDialog dialogTo = new MachineStateTemplateDialog();
        dialogTo.Nullable = false;
        dialogTo.MultiSelect = false;
        dialogTo.Text = PulseCatalog.GetString("To");
        if(dialogTo.ShowDialog() == DialogResult.OK){
          IMachineStateTemplateFlow machineStateTemplateFlow = ModelDAOHelper.ModelFactory.CreateMachineStateTemplateFlow(dialogFrom.SelectedValue,dialogTo.SelectedValue);
          m_MachineStateTemplateFlows.Add(machineStateTemplateFlow);
          m_updateSet.Add(dataGridView.Rows[dataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible)]);
        }
      }
    }
    
    #region Observer implementation
    /// <summary>
    /// Called after (one or more) IMachineStateTemplate are deleted
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineStateTemplate> deletedEntities)
    {
      MachineStateTemplateFlowConfigLoad();
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
