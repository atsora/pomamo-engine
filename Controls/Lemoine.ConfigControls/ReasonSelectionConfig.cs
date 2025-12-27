// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
  /// Description of ReasonSelectionConfig.
  /// </summary>
  public partial class ReasonSelectionConfig
    : UserControl
    , IConfigControlObserver<IMachineMode>
    , IConfigControlObserver<IMachineObservationState>
    , IConfigControlObserver<IReason>
    , IConfigControlObserver<IMachineFilter>
  {
    #region Members
    SortableBindingList<ReasonSelectionWrapper> m_reasonSelectionWrappers
      = new SortableBindingList<ReasonSelectionWrapper>();

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IReasonSelection> m_deleteList =
      new List<IReasonSelection> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSelectionConfig).FullName);

    static readonly string DISPLAYED_PROPERTY = "SelectionText";
    static readonly string NAME_PROPERTY = "Name";
    
    IList<IMachineMode> MachineModes {
      get
      {
        if (0 == machineModeSelection1.SelectedMachineModes.Count) {
          return null;
        }
        else {
          Debug.Assert (1 <= machineModeSelection1.SelectedMachineModes.Count);
          return machineModeSelection1.SelectedMachineModes;
        }
      }
    }
    
    IList<IMachineObservationState> MachineObservationStates {
      get
      {
        if (0 == machineObservationStateSelection1.SelectedMachineObservationStates.Count) {
          return null;
        }
        else {
          Debug.Assert (1 <= machineObservationStateSelection1.SelectedMachineObservationStates.Count);
          return machineObservationStateSelection1.SelectedMachineObservationStates;
        }
      }
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonSelectionConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      reasonColumn.HeaderText = PulseCatalog.GetString ("Reason");
      selectableColumn.HeaderText = PulseCatalog.GetString ("ReasonSelectable");
      detailsRequiredColumn.HeaderText = PulseCatalog.GetString ("ReasonDetailsRequired");
      noDetailsColumn.HeaderText = PulseCatalog.GetString ("ReasonNoDetails", "No detail");
      machineFilterColumn.HeaderText = PulseCatalog.GetString ("MachineFilter");
      setAllOverColumn.HeaderText = PulseCatalog.GetString("SetAllOver");
      
      m_reasonSelectionWrappers.SortColumns = false;
      
      {
        ReasonReasonGroupDialog dialog =
          new ReasonReasonGroupDialog ();
        dialog.Nullable = false;
        dialog.CanSelectReasonGroup = false;
        dialog.ReasonDisplayedProperty = DISPLAYED_PROPERTY;
        dialog.ReasonGroupDisplayedProperty = DISPLAYED_PROPERTY;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IReason> (dialog);
        reasonColumn.CellTemplate = cell;
      }
      
      {
        MachineFilterDialog dialog =
          new MachineFilterDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = NAME_PROPERTY;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter> (dialog);
        machineFilterColumn.CellTemplate = cell;
      }
    }

    void ReasonSelectionConfigLoad(object sender, EventArgs e)
    {
      LoadDataGridView ();
    }
    
    void LoadDataGridView ()
    {
      if ( (null == this.MachineModes)
          || (null == this.MachineObservationStates)) {
        // Nothing to display
        dataGridView.Visible = false;
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("LoadDataGridView: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        // Retrieve all reasons, each associated to a machine mode and an observation state
        // The possible machine modes and observation states come from the lists
        IList<IReasonSelection> reasonSelections =
          daoFactory.ReasonSelectionDAO
          .FindWithForConfig (this.MachineModes,
                              this.MachineObservationStates);
        m_reasonSelectionWrappers.Clear();
        
        // While the list is not empty
        while (reasonSelections.Count > 0)
        {
          // We take the first element
          IReasonSelection reasonSelection = reasonSelections[0];
          reasonSelections.RemoveAt(0);
          
          // We create a wrapper having the same characteristics
          ReasonSelectionWrapper tempWrapper = new ReasonSelectionWrapper();
          tempWrapper.DetailsRequired = reasonSelection.DetailsRequired;
          tempWrapper.Selectable = reasonSelection.Selectable;
          tempWrapper.Reason = reasonSelection.Reason;
          tempWrapper.MachineFilter = reasonSelection.MachineFilter;
          tempWrapper.Id = reasonSelection.Id.ToString();
          tempWrapper.reasonSelections.Add(reasonSelection);
          
          // We review all remaining elements and we add them to the wrapper if the characteristics are similar
          int countTmp = reasonSelections.Count;
          for (int i = countTmp - 1; i >= 0; i--)
          {
            IReasonSelection reasonSelectionToCmp = reasonSelections[i];
            if (Object.Equals(reasonSelection.DetailsRequired, reasonSelectionToCmp.DetailsRequired) &&
                Object.Equals(reasonSelection.Selectable, reasonSelectionToCmp.Selectable) &&
                reasonSelection.Reason.Equals(reasonSelectionToCmp.Reason) &&
                Object.Equals(reasonSelection.MachineFilter, reasonSelectionToCmp.MachineFilter))
            {
              // Same characteristics
              tempWrapper.reasonSelections.Add(reasonSelectionToCmp);
              tempWrapper.Id += ";" + reasonSelectionToCmp.Id.ToString();
              reasonSelections.RemoveAt(i);
            }
          }
          
          // The color of the wrapper is defined (displayed as the background color of a row)
          // White if the reason comprises all machine modes and all observation states, otherwise gray
          if (tempWrapper.reasonSelections.Count == MachineModes.Count * MachineObservationStates.Count) {
            tempWrapper.BackColor = Color.White;
          }
          else {
            tempWrapper.BackColor = Color.LightGray;
          }

          // Store the wrapper
          m_reasonSelectionWrappers.Add(tempWrapper);
        }
        
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_reasonSelectionWrappers;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew +=
          new AddingNewEventHandler (AddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
      
      // Make needed CheckBox disabled
      foreach(DataGridViewRow row in dataGridView.Rows){
        DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)row.Cells["setAllOverColumn"];
        ReasonSelectionWrapper reasonSelectionWrapper = row.DataBoundItem as ReasonSelectionWrapper;
        if(reasonSelectionWrapper != null){
          if(reasonSelectionWrapper.BackColor == Color.White){
            checkCell.Value = false;
            checkCell.FlatStyle = FlatStyle.Flat;
            checkCell.Style.ForeColor = Color.DarkGray;
            checkCell.ReadOnly = true;
          }
        }
      }
      
      dataGridView.Visible = true;
    }
    
    void ReasonSelectionConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
      LoadDataGridView ();
    }
    
    void CommitChanges ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("CommitChanges: " +
                         "no DAO factory is defined");
        return;
      }
      
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          ReasonSelectionWrapper reasonSelectionWrapper = row.DataBoundItem as ReasonSelectionWrapper;
          if (null == reasonSelectionWrapper) {
            continue; // The row may have been deleted since
          }
          foreach(IReasonSelection reasonSelection in reasonSelectionWrapper.reasonSelections){
            daoFactory.ReasonSelectionDAO.MakePersistent (reasonSelection);
          }
        }
        
        foreach (IReasonSelection reasonSelection
                 in m_deleteList) {
          daoFactory.ReasonSelectionDAO
            .MakeTransient (reasonSelection);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void AddingNew (object sender,
                    AddingNewEventArgs e)
    {
      ReasonSelectionWrapper tempWrapper = new ReasonSelectionWrapper();
      
      foreach(IMachineMode mMode in MachineModes){
        foreach(IMachineObservationState mObservationState in MachineObservationStates) {
          tempWrapper.reasonSelections.Add(ModelDAOHelper.ModelFactory.CreateReasonSelection (mMode,
                                                                                              mObservationState));
        }
      }
      e.NewObject = tempWrapper;
    }
    
    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      ReasonSelectionWrapper reasonSelectionWrapper =
        e.Row.DataBoundItem
        as ReasonSelectionWrapper;
      if (null != reasonSelectionWrapper) {
        m_updateSet.Remove (e.Row);
        foreach(IReasonSelection reasonSelection in reasonSelectionWrapper.reasonSelections){
          m_deleteList.Add (reasonSelection);
        }
      }
    }
    
    void MachineModeSelection1AfterSelect(object sender, EventArgs e)
    {
      CommitChanges ();
      LoadDataGridView ();
    }
    
    void MachineObservationStateSelection1AfterSelect(object sender, EventArgs e)
    {
      CommitChanges ();
      LoadDataGridView ();
    }
    
    void DataGridViewUserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      ReasonSelectionWrapper reasonSelectionWrapper =
        e.Row.DataBoundItem
        as ReasonSelectionWrapper;
      if (null != reasonSelectionWrapper) {
        m_updateSet.Add (e.Row);
      }
    }

    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        if(this.dataGridView.Columns[e.ColumnIndex].Name == "setAllOverColumn"){
          DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)row.Cells["setAllOverColumn"];
          if((Boolean)checkCell.Value){
            ReasonSelectionWrapper reasonSelectionWrapper = row.DataBoundItem as ReasonSelectionWrapper;
            IReasonSelection referenceReasonSelection = reasonSelectionWrapper.reasonSelections[0];
            IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
            using (IDAOSession session = daoFactory.OpenSession ())
              using (IDAOTransaction transaction = session.BeginTransaction ()){
              
              foreach(IMachineMode mMode in MachineModes){
                foreach(IMachineObservationState mObservationState in MachineObservationStates) {
                  ReasonSelectionWrapper tmpWrapper = new ReasonSelectionWrapper();
                  tmpWrapper.reasonSelections = daoFactory.ReasonSelectionDAO
                    .FindWithForConfig (mMode,mObservationState);
                  
                  if(tmpWrapper.checkIfContainSimilareReasonSelection(referenceReasonSelection)){
                    if(!tmpWrapper.checkIfContain(referenceReasonSelection)){
                      reasonSelectionWrapper.reasonSelections.Add(tmpWrapper.findSimilareReasonSelection(referenceReasonSelection));
                    }
                  }
                  else {
                    reasonSelectionWrapper.reasonSelections.Add(ModelDAOHelper.ModelFactory.CreateReasonSelection(mMode, mObservationState));
                  }
                }
              }
              
            }
            reasonSelectionWrapper.pushModification();
            m_updateSet.Add(row);
          }
          else {
            m_updateSet.Remove(row);
          }
        }
        else {
          ReasonSelectionWrapper reasonSelectionWrapper =
            row.DataBoundItem
            as ReasonSelectionWrapper;
          if (null != reasonSelectionWrapper) {
            reasonSelectionWrapper.pushModification();
            m_updateSet.Add (row);
          }
        }
      }
    }
    
    /// <summary>
    /// Set the color of the row.
    /// </summary>
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      DataGridViewRow row = dataGridView.Rows [e.RowIndex];
      ReasonSelectionWrapper reasonSelectionWrapper = row.DataBoundItem as ReasonSelectionWrapper;
      if(reasonSelectionWrapper != null) {
        dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = reasonSelectionWrapper.BackColor;
      }
    }
    
    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineModeConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineMode> deletedEntities)
    {
      machineModeSelection1.Reload ();
      LoadDataGridView ();
    }
    
    /// <summary>
    /// Update this control after some items have been updated
    /// in the MachineObservationStateConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineMode> updatedEntities)
    {
      machineModeSelection1.Reload ();
      LoadDataGridView ();
    }
    
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineObservationStateConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineObservationState> deletedEntities)
    {
      machineObservationStateSelection1.Reload ();
      LoadDataGridView ();
    }
    
    /// <summary>
    /// Update this control after some items have been updated
    /// in the MachineObservationStateConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineObservationState> updatedEntities)
    {
      machineObservationStateSelection1.Reload ();
      LoadDataGridView ();
    }
    
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the ReasonConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IReason> deletedEntities)
    {
      LoadDataGridView ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the ReasonConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IReason> updatedEntities)
    {
      // Do nothing
    }
    
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineFilterConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete(ICollection<IMachineFilter> deletedEntities)
    {
      LoadDataGridView ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the MachineFilterConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineFilter> updatedEntities)
    {
      // Do nothing
    }
    #endregion // IConfigControlObserver implementation
  }
}