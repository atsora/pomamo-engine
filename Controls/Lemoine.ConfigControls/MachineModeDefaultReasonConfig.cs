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
  /// Description of MachineModeDefaultReasonConfig.
  /// </summary>
  public partial class MachineModeDefaultReasonConfig
    : UserControl
    , IConfigControlObserver<IMachineMode>
    , IConfigControlObserver<IMachineObservationState>
    , IConfigControlObserver<IReason>
    , IConfigControlObserver<IMachineFilter>
  {
    #region Members
    ISet<DataGridViewRow> m_updateSet = new HashSet<DataGridViewRow> ();
    IList<IMachineModeDefaultReason> m_deleteList = new List<IMachineModeDefaultReason> ();
    IList<MachineModeDefaultReasonWrapper> m_machineModeDefaultReasonWrappers = new List<MachineModeDefaultReasonWrapper> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeDefaultReasonConfig).FullName);
    static readonly string DISPLAYED_PROPERTY = "SelectionText";
    
    #region Getters / Setters
    IList<IMachineMode> MachineModes {
      get
      {
        Debug.Assert (null != machineModeSelection1);

        if (machineModeSelection1.SelectedMachineModes.Count > 0) {
          return machineModeSelection1.SelectedMachineModes;
        }
        else {
          return null;
        }
      }
    }
    
    IList<IMachineObservationState> MachineObservationStates {
      get
      {
        if (machineObservationStateSelection1.SelectedMachineObservationStates.Count > 0) {
          return machineObservationStateSelection1.SelectedMachineObservationStates;
        }
        else {
          return null;
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModeDefaultReasonConfig()
    {
      InitializeComponent();
      
      includeMachineFilterColumn.HeaderText = PulseCatalog.GetString ("IncludeMachineFilter");
      excludeMachineFilterColumn.HeaderText = PulseCatalog.GetString ("ExcludeMachineFilter");
      maximumDurationColumn.HeaderText = PulseCatalog.GetString ("DefaultReasonMaximumDuration");
      reasonColumn.HeaderText = PulseCatalog.GetString ("Reason");
      overwriteRequiredColumn.HeaderText = PulseCatalog.GetString ("DefaultReasonOverwriteRequired");
      setAllOverColumn.HeaderText = PulseCatalog.GetString ("SetAllOver");
      
      {
        MachineFilterDialog dialog = new MachineFilterDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter> (dialog);
        includeMachineFilterColumn.CellTemplate = cell;
      }
      {
        MachineFilterDialog dialog = new MachineFilterDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineFilter> (dialog);
        excludeMachineFilterColumn.CellTemplate = cell;
      }
      {
        ReasonReasonGroupDialog dialog = new ReasonReasonGroupDialog ();
        dialog.Nullable = false;
        dialog.CanSelectReasonGroup = false;
        dialog.ReasonDisplayedProperty = DISPLAYED_PROPERTY;
        dialog.ReasonGroupDisplayedProperty = DISPLAYED_PROPERTY;
        DataGridViewCell cell = new DataGridViewSelectionableCell<IReason> (dialog);
        reasonColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void MachineModeDefaultReasonConfigLoad(object sender, EventArgs e)
    {
      LoadDataGridView();
    }
    
    void LoadDataGridView()
    {
      if (this.MachineModes == null || this.MachineObservationStates == null)
      {
        // Nothing to display
        dataGridView.Visible = false;
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory)
      {
        log.ErrorFormat ("ReasonConfigLoad: no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession())
      {
        // Retrieve all default reasons, each associated to a machine mode and an observation state
        // The possible machine modes and observation states come from the lists
        IList<IMachineModeDefaultReason> machineModeDefaultReasons =
          daoFactory.MachineModeDefaultReasonDAO.FindWithForConfig(this.MachineModes, this.MachineObservationStates);
        m_machineModeDefaultReasonWrappers.Clear();
        
        // While the list is not empty
        while (machineModeDefaultReasons.Count > 0)
        {
          // We take the first element
          IMachineModeDefaultReason machineModeDefaultReason = machineModeDefaultReasons[0];
          machineModeDefaultReasons.RemoveAt(0);
          
          // We create a wrapper having the same characteristics
          MachineModeDefaultReasonWrapper tempWrapper = new MachineModeDefaultReasonWrapper();
          tempWrapper.MaximumDuration = machineModeDefaultReason.MaximumDuration;
          tempWrapper.Score = machineModeDefaultReason.Score;
          tempWrapper.Auto = machineModeDefaultReason.Auto;
          tempWrapper.OverwriteRequired = machineModeDefaultReason.OverwriteRequired;
          tempWrapper.Reason = machineModeDefaultReason.Reason;
          tempWrapper.Id = machineModeDefaultReason.Id.ToString();
          tempWrapper.IncludeMachineFilter = machineModeDefaultReason.IncludeMachineFilter;
          tempWrapper.ExcludeMachineFilter = machineModeDefaultReason.ExcludeMachineFilter;
          tempWrapper.machineModeDefaultReasons.Add(machineModeDefaultReason);
          
          // We review all remaining elements and we add them to the wrapper if the characteristics are similar
          int countTmp = machineModeDefaultReasons.Count;
          for (int i = countTmp - 1; i >= 0; i--)
          {
            IMachineModeDefaultReason machineModeDefaultReasonToCmp = machineModeDefaultReasons[i];
            if (machineModeDefaultReason.MaximumDuration.Equals(machineModeDefaultReasonToCmp.MaximumDuration) &&
                machineModeDefaultReason.Reason.Equals(machineModeDefaultReasonToCmp.Reason) &&
                (machineModeDefaultReason.Score == machineModeDefaultReasonToCmp.Score) &&
                (machineModeDefaultReason.Auto == machineModeDefaultReasonToCmp.Auto) &&
                Object.Equals(machineModeDefaultReason.OverwriteRequired, machineModeDefaultReasonToCmp.OverwriteRequired) &&
                Object.Equals(machineModeDefaultReason.IncludeMachineFilter, machineModeDefaultReasonToCmp.IncludeMachineFilter) &&
                Object.Equals(machineModeDefaultReason.ExcludeMachineFilter, machineModeDefaultReasonToCmp.ExcludeMachineFilter))
            {
              // Same characteristics
              tempWrapper.machineModeDefaultReasons.Add(machineModeDefaultReasonToCmp);
              tempWrapper.Id += ";" + machineModeDefaultReasonToCmp.Id.ToString();
              machineModeDefaultReasons.RemoveAt(i);
            }
          }
          
          // The color of the wrapper is defined (displayed as the background color of a row)
          // White if the reason comprises all machine modes and all observation states, otherwise gray
          if (tempWrapper.machineModeDefaultReasons.Count == MachineModes.Count * MachineObservationStates.Count) {
            tempWrapper.BackColor = Color.White;
          }
          else {
            tempWrapper.BackColor = Color.LightGray;
          }

          // Store the wrapper
          m_machineModeDefaultReasonWrappers.Add(tempWrapper);
        }
        
        // Clean TMP List
        machineModeDefaultReasons.Clear();
        
        // Note: the use of a bindingSource is necessary to add some new rows
        BindingSource bindingSource = new BindingSource();
        bindingSource.DataSource = m_machineModeDefaultReasonWrappers;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
      
      // Make needed CheckBox disabled
      foreach(DataGridViewRow row in dataGridView.Rows) {
        DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)row.Cells["setAllOverColumn"];
        MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = row.DataBoundItem as MachineModeDefaultReasonWrapper;
        if(machineModeDefaultReasonWrapper != null){
          if(machineModeDefaultReasonWrapper.BackColor == Color.White){
            checkCell.Value = false;
            checkCell.FlatStyle = FlatStyle.Flat;
            checkCell.Style.ForeColor = Color.DarkGray;
            checkCell.ReadOnly = true;
          }
        }
      }
      
      dataGridView.Visible = true;
    }
    
    void MachineModeDefaultReasonConfigValidated(object sender, EventArgs e)
    {
      CommitChanges();
      LoadDataGridView();
    }
    
    void CommitChanges()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {

          MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = row.DataBoundItem as MachineModeDefaultReasonWrapper;
          if (null == machineModeDefaultReasonWrapper) {
            continue; // The row may have been deleted since
          }
          foreach(IMachineModeDefaultReason machineModeDefaultReason in machineModeDefaultReasonWrapper.machineModeDefaultReasons){
            daoFactory.MachineModeDefaultReasonDAO.MakePersistent (machineModeDefaultReason);
          }
        }

        foreach (IMachineModeDefaultReason machineModeDefaultReason in m_deleteList) {
          daoFactory.MachineModeDefaultReasonDAO.MakeTransient (machineModeDefaultReason);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void BindingSourceAddingNew(object sender, AddingNewEventArgs e)
    {
      MachineModeDefaultReasonWrapper tempWrapper = new MachineModeDefaultReasonWrapper();
      
      foreach(IMachineMode mMode in MachineModes){
        foreach(IMachineObservationState mObservationState in MachineObservationStates) {
          tempWrapper.machineModeDefaultReasons.Add(ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason (mMode,
                                                                                                                mObservationState));
        }
      }
      e.NewObject = tempWrapper;
    }
    
    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = e.Row.DataBoundItem as MachineModeDefaultReasonWrapper;
      if(machineModeDefaultReasonWrapper != null) {
        m_updateSet.Remove (e.Row);
        foreach(IMachineModeDefaultReason machineModeDefaultReason in machineModeDefaultReasonWrapper.machineModeDefaultReasons){
          m_deleteList.Add (machineModeDefaultReason);
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
      MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = e.Row.DataBoundItem as MachineModeDefaultReasonWrapper;
      if (null != machineModeDefaultReasonWrapper) {
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
            MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = row.DataBoundItem as MachineModeDefaultReasonWrapper;
            IMachineModeDefaultReason referenceMachieModeDefaultReason = machineModeDefaultReasonWrapper.machineModeDefaultReasons[0];
            IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
            using (IDAOSession session = daoFactory.OpenSession ())
              using (IDAOTransaction transaction = session.BeginTransaction ()){
              
              foreach(IMachineMode mMode in MachineModes){
                foreach(IMachineObservationState mObservationState in MachineObservationStates) {
                  MachineModeDefaultReasonWrapper wrapperForCMP = new MachineModeDefaultReasonWrapper();
                  wrapperForCMP.machineModeDefaultReasons = daoFactory.MachineModeDefaultReasonDAO.FindWithForConfig (mMode,mObservationState);
                  
                  if(wrapperForCMP.checkIfContainSimilaryMachineModeDefaultReason(referenceMachieModeDefaultReason)){
                    if(!wrapperForCMP.checkIfContain(referenceMachieModeDefaultReason)){
                      machineModeDefaultReasonWrapper.machineModeDefaultReasons.Add(wrapperForCMP.findSimilaryMachineModeDefaultReason(referenceMachieModeDefaultReason));
                    }
                  }
                  else {
                    machineModeDefaultReasonWrapper.machineModeDefaultReasons.Add(ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason(mMode, mObservationState));
                  }
                }
              }
              
            }
            machineModeDefaultReasonWrapper.pushModification();
            m_updateSet.Add(row);
          }
          else {
            m_updateSet.Remove(row);
          }
        }
        else {
          MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = row.DataBoundItem as MachineModeDefaultReasonWrapper;
          if (null != machineModeDefaultReasonWrapper) {
            machineModeDefaultReasonWrapper.pushModification();
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
      MachineModeDefaultReasonWrapper machineModeDefaultReasonWrapper = row.DataBoundItem as MachineModeDefaultReasonWrapper;
      if (machineModeDefaultReasonWrapper != null) {
        dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = machineModeDefaultReasonWrapper.BackColor;
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
    /// in the MachineModeConfig control
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