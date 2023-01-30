// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Iesi.Collections.Generic;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MachineStateTemplateRightConfig.
  /// </summary>
  public partial class MachineStateTemplateRightConfig
    : UserControl
    , IConfigControlObservable<IMachineStateTemplateRight>
    , IConfigControlObserver<IMachineStateTemplate>
  {
    #region Members
    BindingList<IDataGridViewMachineStateTemplateRight> m_machineStateTemplateRights = new BindingList<IDataGridViewMachineStateTemplateRight>();
    
    ISet<IMachineStateTemplateRight> m_updateSet = new HashSet<IMachineStateTemplateRight> ();
    IList<IMachineStateTemplateRight> m_deleteList = new List<IMachineStateTemplateRight> ();
    
    ISet<IConfigControlObserver<IMachineStateTemplateRight>> m_observers = 
      new HashSet<IConfigControlObserver<IMachineStateTemplateRight>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateRightConfig).FullName);
    
    #region Getter/Setter
    /// <summary>
    /// Return Selected RightAccessPrivilege
    /// </summary>
    private RightAccessPrivilege SelectedAccessPrivilege{
      get {
        if(this.grantedRadioButton.Checked) {
          return RightAccessPrivilege.Granted;
        }

        if (this.deniedRadioButton.Checked) {
          return RightAccessPrivilege.Denied;
        }

        return RightAccessPrivilege.Granted;
      }
    }
    
    /// <summary>
    /// Return the opposite of selected RightAccessPrivilege
    /// </summary>
    private RightAccessPrivilege GetOppositeAccessPrivilege {
      get {
        if(this.grantedRadioButton.Checked) {
          return RightAccessPrivilege.Denied;
        }

        if (this.deniedRadioButton.Checked) {
          return RightAccessPrivilege.Granted;
        }

        return RightAccessPrivilege.Granted; //Never got this normaly
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateRightConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //i18n
      AddButton.Text = PulseCatalog.GetString("Add");
      roleSelectionGroupBox.Text = PulseCatalog.GetString("Roles");
      defaultLabel.Text = PulseCatalog.GetString("Default");
      grantedRadioButton.Text = PulseCatalog.GetString("Granted");
      deniedRadioButton.Text = PulseCatalog.GetString("Denied");
      defaultSelectionGroupBox.Text = PulseCatalog.GetString("DefaultRightAccess");
      exceptionGroupBox.Text = PulseCatalog.GetString("Exceptions");
      
      machineStateTemplateRightAccessColumn.HeaderText = PulseCatalog.GetString("RightAccessColumn");
      machineStateTemplateRightMachineStateTemplateColumn.HeaderText = PulseCatalog.GetString("MachineStateTemplateColumn");
    }
    #endregion // Constructors
    
    void MachineStateTemplateRightConfigLoad(object sender, EventArgs e)
    {
      MachineStateTemplateRightConfigLoad ();
    }
    
    void MachineStateTemplateRightConfigLoad()
    {
      IList<IMachineStateTemplateRight> machineStateTemplateRights;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(m_machineStateTemplateRights != null) {
        m_machineStateTemplateRights.Clear();
      }

      if (this.roleSelection.SelectedRoles.Count >=1){
        using (IDAOSession session = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          machineStateTemplateRights = daoFactory.MachineStateTemplateRightDAO.FindWithForConfig(this.roleSelection.SelectedRoles);
          
          foreach (IMachineStateTemplateRight mSTR in machineStateTemplateRights) {
            IDataGridViewMachineStateTemplateRight test= (IDataGridViewMachineStateTemplateRight)mSTR;
            m_machineStateTemplateRights.Add((IDataGridViewMachineStateTemplateRight)mSTR);
          }
        }
        
        //Show non commited row to DGV before Commit
        foreach(IMachineStateTemplateRight mSTR in m_updateSet){
          if(mSTR.Role.Equals(this.roleSelection.SelectedRoles[0])){
            m_machineStateTemplateRights.Add((IDataGridViewMachineStateTemplateRight)mSTR);
          }
        }
      }
      //Avoid Deleted Row to be show before Commit
      foreach(IMachineStateTemplateRight mSTR in m_deleteList){
        if(m_machineStateTemplateRights.Contains((IDataGridViewMachineStateTemplateRight)mSTR)) {
          m_machineStateTemplateRights.Remove((IDataGridViewMachineStateTemplateRight)mSTR);
        }
      }
      {
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineStateTemplateRights;
        bindingSource.AllowNew = true;
        machineStateTemplateRightDataGridView.AutoGenerateColumns = false;
        machineStateTemplateRightDataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineStateTemplateRightConfigValidated(object sender, EventArgs e)
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
        foreach (IMachineStateTemplateRight machineStateTemplateRight in m_updateSet) {
          if (null == machineStateTemplateRight) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineStateTemplateRightDAO.MakePersistent (machineStateTemplateRight);
        }

        foreach (IMachineStateTemplateRight machineStateTemplateRight in m_deleteList) {
          daoFactory.MachineStateTemplateRightDAO.MakeTransient (machineStateTemplateRight);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }
    
    #region Event
    void MachineStateTemplateRightDataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineStateTemplateRight machineStateTemplateRight = e.Row.DataBoundItem as IMachineStateTemplateRight;
      if (null != machineStateTemplateRight) {
        //Protect default MSTR to be deleted
        if(machineStateTemplateRight.MachineStateTemplate == null){
          e.Cancel = true;
        }
        else {
          m_updateSet.Remove (machineStateTemplateRight);
          m_deleteList.Add (machineStateTemplateRight);
        }
      }
    }
    
    void MachineStateTemplateRightDataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(this.machineStateTemplateRightDataGridView.Columns[e.ColumnIndex].Name == "machineStateTemplateRightMachineStateTemplateColumn") {
        DataGridViewRow row = machineStateTemplateRightDataGridView.Rows [e.RowIndex];
        IMachineStateTemplateRight machineStateTemplateRight = row.DataBoundItem as IMachineStateTemplateRight;
        if(machineStateTemplateRight != null){
          if(machineStateTemplateRight.MachineStateTemplate != null) {
            e.Value = machineStateTemplateRight.MachineStateTemplate.SelectionText;
          }
          else {
            e.Value = PulseCatalog.GetString("MachineStateTemplateRightConfig-All-MST-Selected");
            //            row.Visible = false; //TODO: find a way without crash
          }
        }
      }
    }
    
    void GrantedRadioButtonCheckedChanged(object sender, EventArgs e)
    {
      if(this.grantedRadioButton.Checked) {
        this.deniedRadioButton.Checked = false;
      }

      MachineStateTemplateRightConfigLoad (); //Needed Refresh for SetDefault()
      if(this.grantedRadioButton.Checked) {
        SetDefault (this.roleSelection.SelectedRoles[0], RightAccessPrivilege.Granted);
      }
    }
    
    void DeniedRadioButtonCheckedChanged(object sender, EventArgs e)
    {
      if(this.deniedRadioButton.Checked) {
        this.grantedRadioButton.Checked = false;
      }

      MachineStateTemplateRightConfigLoad (); //Needed Refresh for SetDefault()
      if(this.deniedRadioButton.Checked) {
        SetDefault (this.roleSelection.SelectedRoles[0], RightAccessPrivilege.Denied);
      }
    }
    
    void RoleSelectionAfterSelect(object sender, EventArgs e)
    {
      if(this.roleSelection.SelectedRoles.Count >= 1){
        if(CheckRightAccessPrivilegeFromRole(this.roleSelection.SelectedRoles[0]) == RightAccessPrivilege.Granted) {
          this.grantedRadioButton.Checked = true;
        }
        else {
          this.deniedRadioButton.Checked = true;
        }

        MachineStateTemplateRightConfigLoad ();
      }
    }
    
    void AddButtonClick(object sender, EventArgs e)
    {
      MachineStateTemplateDialog mSTDialog = new MachineStateTemplateDialog();
      mSTDialog.MultiSelect = true;
      mSTDialog.Nullable = false;
      
      if(mSTDialog.ShowDialog() == DialogResult.OK){
        foreach(IRole role in this.roleSelection.SelectedRoles){
          IMachineStateTemplateRight mSTR = null;
          foreach(IMachineStateTemplate machineStateTemplate in mSTDialog.SelectedValues) {
            mSTR = ModelDAOHelper.ModelFactory.CreateMachineStateTemplateRight(machineStateTemplate, role, this.GetOppositeAccessPrivilege);
            //Avoid to add existing MST/R
            bool canAdd = true;
            foreach(IMachineStateTemplateRight machineStateTemplateRight in m_machineStateTemplateRights){
              if(object.Equals(machineStateTemplateRight.MachineStateTemplate, machineStateTemplate)){
                canAdd = false;
              }
            }
            if(canAdd){
              m_machineStateTemplateRights.Add((IDataGridViewMachineStateTemplateRight)mSTR);
              m_updateSet.Add(mSTR);
              m_deleteList.Remove(mSTR);
            }
          }
        }
      }
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Return RightAccessPrivilege linked to Role
    /// </summary>
    /// <param name="role"></param>
    /// <returns>RightAccessPrivilege</returns>
    private RightAccessPrivilege CheckRightAccessPrivilegeFromRole(IRole role){
      RightAccessPrivilege rAP = RightAccessPrivilege.Granted;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(role != null){
        using (IDAOSession session = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          rAP = daoFactory.MachineStateTemplateRightDAO.GetDefault(role);
        }
      }
      return rAP;
    }
    
    /// <summary>
    /// Set Default RightAccessPrivilege to Role if not exist OR
    /// Set the specified RightAccessPrivilege with reset of all Exception
    /// </summary>
    /// <param name="role"></param>
    /// <param name="rightAccessPrivilege"></param>
    private void SetDefault(IRole role, RightAccessPrivilege rightAccessPrivilege){
      foreach (IMachineStateTemplateRight mSTR in m_machineStateTemplateRights) {
        if(mSTR.Role.Equals(role) && mSTR.AccessPrivilege.Equals(rightAccessPrivilege) && Object.Equals(mSTR.MachineStateTemplate, null)) {
          return;
        }
      }
      ResetException();
      IMachineStateTemplateRight machineStateTemplateRight = ModelDAOHelper.ModelFactory.CreateMachineStateTemplateRight(null, role, rightAccessPrivilege);
      m_machineStateTemplateRights.Add((IDataGridViewMachineStateTemplateRight)machineStateTemplateRight);
      m_updateSet.Add(machineStateTemplateRight);
      CommitChanges();
      MachineStateTemplateRightConfigLoad();
    }
    
    /// <summary>
    /// Reset all Exception of selected Role
    /// </summary>
    private void ResetException(){
      foreach (IMachineStateTemplateRight mSTR in m_machineStateTemplateRights) {
        m_deleteList.Add(mSTR);
        if(m_updateSet.Contains(mSTR)) {
          m_updateSet.Remove(mSTR);
        }
      }
    }
    #endregion
    
    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineStateTemplateRight> observer){
      this.m_observers.Add(observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineStateTemplateRight> observer){
      this.m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedMachineStateTemplateRights"></param>
    void NotifyDelete(IList<IMachineStateTemplateRight> deletedMachineStateTemplateRights){
      foreach(IConfigControlObserver<IMachineStateTemplateRight> observer in m_observers){
        observer.UpdateAfterDelete(deletedMachineStateTemplateRights);
      }
    }
    
    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedMachineStateTemplateRights"></param>
    void NotifyUpdate(IList<IMachineStateTemplateRight> updatedMachineStateTemplateRights){
      foreach(IConfigControlObserver<IMachineStateTemplateRight> observer in m_observers){
        observer.UpdateAfterUpdate(updatedMachineStateTemplateRights);
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
      MachineStateTemplateRightConfigLoad();
    }
    
    /// <summary>
    /// Called after (one or more) IMachineStateTemplate are updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate(ICollection<IMachineStateTemplate> updatedEntities)
    {
      MachineStateTemplateRightConfigLoad();
    }
    #endregion
  }
}
