// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Iesi.Collections.Generic;

using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MachineConfig.
  /// </summary>
  public partial class MachineConfig
    : UserControl
    , IConfigControlObserver<ICompany>
    , IConfigControlObserver<IDepartment>
    , IConfigControlObserver<ICell>
    , IConfigControlObserver<IMachineCategory>
    , IConfigControlObserver<IMachineSubCategory>
    , IConfigControlObserver<IMonitoredMachine>
  {
    #region Members
    SortableBindingList<IMachine> m_machines = new SortableBindingList<IMachine> ();

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();

    IDictionary<int, ICompany> m_originalCompanies =
      new Dictionary<int, ICompany> ();
    IDictionary<int, IDepartment> m_originalDepartments =
      new Dictionary<int, IDepartment> ();
    IDictionary<int, ICell> m_originalCells =
      new Dictionary<int, ICell> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineConfig ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("Code");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      monitoringTypeColumn.HeaderText = PulseCatalog.GetString ("MachineMonitoringType");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");
      companyColumn.HeaderText = PulseCatalog.GetString ("Company");
      departmentColumn.HeaderText = PulseCatalog.GetString ("Department");
      cellColumn.HeaderText = PulseCatalog.GetString ("Cell");
      categoryColumn.HeaderText = PulseCatalog.GetString ("MachineCategory");
      subCategoryColumn.HeaderText = PulseCatalog.GetString ("MachineSubCategory");
      costOffColumn.HeaderText = PulseCatalog.GetString ("MachineCostOff", "Cost off");
      costInactiveColumn.HeaderText = PulseCatalog.GetString ("MachineCostInactive", "Cost inactive");
      costActiveColumn.HeaderText = PulseCatalog.GetString ("MachineCostActive", "Cost active");
      defaultMachineStateTemplateColumn.HeaderText = PulseCatalog.GetString ("MachineDefaultMachineStateTemplate", "Default Machine State Template");

      m_machines.SortColumns = false;

      {
        CompanyDialog dialog =
          new CompanyDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<ICompany> (dialog);
        companyColumn.CellTemplate = cell;
      }
      {
        DepartmentDialog dialog =
          new DepartmentDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IDepartment> (dialog);
        departmentColumn.CellTemplate = cell;
      }
      {
        CellDialog dialog =
          new CellDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<ICell> (dialog);
        cellColumn.CellTemplate = cell;
      }
      {
        MachineCategoryDialog dialog =
          new MachineCategoryDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineCategory> (dialog);
        categoryColumn.CellTemplate = cell;
      }
      {
        MachineSubCategoryDialog dialog =
          new MachineSubCategoryDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineSubCategory> (dialog);
        subCategoryColumn.CellTemplate = cell;
      }
      {
        MachineMonitoringTypeDialog dialog =
          new MachineMonitoringTypeDialog ();
        dialog.Nullable = false;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMonitoringType> (dialog);
        monitoringTypeColumn.CellTemplate = cell;
      }
      {
        MachineStateTemplateDialog dialog =
          new MachineStateTemplateDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineStateTemplate> (dialog);
        defaultMachineStateTemplateColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void MachineConfigLoad (object sender, EventArgs e)
    {
      MachineConfigLoad ();
    }

    void MachineConfigLoad ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ()) {
        IList<IMachine> machines =
          daoFactory.MachineDAO.FindAllWithChildren ();

        m_machines.Clear ();
        foreach (IMachine machine in machines) {
          m_machines.Add (machine);
          m_originalCompanies[machine.Id] = machine.Company;
          m_originalDepartments[machine.Id] = machine.Department;
          m_originalCells[machine.Id] = machine.Cell;
          if (machine is IMonitoredMachine) {
            IMonitoredMachine monitoredMachine = machine as IMonitoredMachine;
            ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock (monitoredMachine);
            ModelDAOHelper.DAOFactory.Initialize (monitoredMachine.MachineModules);
          }
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machines;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler (BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }

    void MachineConfigValidated (object sender, EventArgs e)
    {
      CommitChanges ();
    }

    void CommitChanges ()
    {
      if (0 == m_updateSet.Count) {
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        foreach (DataGridViewRow row in m_updateSet) {
          IMachine machine = row.DataBoundItem as IMachine;
          if (null == machine) {
            continue; // The row may have been deleted since
          }
          if (null == machine.MonitoringType) {
            machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById ((int)MachineMonitoringTypeId.Undefined);
          }

          if (0 != machine.Id) {
            if (!object.Equals (machine.Company, m_originalCompanies[machine.Id])) {
              // New company
              IMachineCompanyUpdate companyUpdate =
                ModelDAOHelper.ModelFactory.CreateMachineCompanyUpdate (machine,
                                                                        m_originalCompanies[machine.Id],
                                                                        machine.Company);
              ModelDAOHelper.DAOFactory.MachineCompanyUpdateDAO.MakePersistent (companyUpdate);
              m_originalCompanies[machine.Id] = machine.Company;
            }
            if (!object.Equals (machine.Department, m_originalDepartments[machine.Id])) {
              // New department
              IMachineDepartmentUpdate departmentUpdate =
                ModelDAOHelper.ModelFactory.CreateMachineDepartmentUpdate (machine,
                                                                           m_originalDepartments[machine.Id],
                                                                           machine.Department);
              ModelDAOHelper.DAOFactory.MachineDepartmentUpdateDAO.MakePersistent (departmentUpdate);
              m_originalDepartments[machine.Id] = machine.Department;
            }
            if (!object.Equals (machine.Cell, m_originalCells[machine.Id])) {
              // New cell
              IMachineCellUpdate cellUpdate =
                ModelDAOHelper.ModelFactory.CreateMachineCellUpdate (machine,
                                                                     m_originalCells[machine.Id],
                                                                     machine.Cell);
              ModelDAOHelper.DAOFactory.MachineCellUpdateDAO.MakePersistent (cellUpdate);
              m_originalCells[machine.Id] = machine.Cell;
            }
          }

          if ((machine is IMonitoredMachine) && (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored)) {
            IMonitoredMachine monitoredMachine = machine as IMonitoredMachine;
            if (null == monitoredMachine.MainMachineModule) {
              ICncAcquisition cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO.FindAll ()
                  .FirstOrDefault (c => string.Equals (machine.Name, c.Name, StringComparison.InvariantCultureIgnoreCase));
              if (null == cncAcquisition) {
                // Choose a LPost
                ComputerDialog dialog =
                  new ComputerDialog ();
                dialog.Nullable = false;
                dialog.DisplayedProperty = "SelectionText";
                dialog.Text = PulseCatalog.GetString ("CncAcquisitionComputerSelectionTitle");
                if (DialogResult.OK != dialog.ShowDialog ()) {
                  transaction.Rollback ();
                  return;
                }
                var computer = dialog.SelectedValue;
                cncAcquisition =
                  ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
                cncAcquisition.Name = machine.Name;
                cncAcquisition.Computer = computer;
                daoFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
              }
              var mainMachineModule =
                ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (monitoredMachine, machine.Name);
              mainMachineModule.CncAcquisition = cncAcquisition;
              daoFactory.MachineModuleDAO.MakePersistent (mainMachineModule);
              monitoredMachine.MainMachineModule = mainMachineModule;
            }
            daoFactory.MonitoredMachineDAO.MakePersistent (monitoredMachine);
          }
          else {
            daoFactory.MachineDAO.MakePersistent (machine);
          }
        }

        transaction.Commit ();
        m_updateSet.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserAddedRow (object sender, DataGridViewRowEventArgs e)
    {
      IMachine machine =
        e.Row.DataBoundItem
        as IMachine;
      if (null != machine) {
        m_updateSet.Add (e.Row);
      }
    }

    void DataGridViewCellValueChanged (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows[e.RowIndex];
        IMachine machine =
          row.DataBoundItem
          as IMachine;
        if (null != machine) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      // TODO: give the possibility not to create a MonitoredMachine but a simple Machine
      IMonitoredMachine monitoredMachine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        monitoredMachine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById (2); // Monitored
      }
      e.NewObject = monitoredMachine;
    }

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the CompanyConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<ICompany> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the CompanyConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<ICompany> updatedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the DepartmentConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<IDepartment> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the DepartmentConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<IDepartment> updatedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the CellConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<ICell> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the CellConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<ICell> updatedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineCategoryConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<IMachineCategory> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the MachineCategoryConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<IMachineCategory> updatedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MachineSubCategoryConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<IMachineSubCategory> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the MachineSubCategoryConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<IMachineSubCategory> updatedEntities)
    {
      // Do nothing
    }

    /// <summary>
    /// Update this control after some items have been deleted
    /// in the MonitoredMachineConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<IMonitoredMachine> deletedEntities)
    {
      MachineConfigLoad ();
    }

    /// <summary>
    /// Update this control after some items have been updated
    /// in the MonitoredMachineConfig control
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (ICollection<IMonitoredMachine> updatedEntities)
    {
      MachineConfigLoad ();
    }
    #endregion // IConfigControlObserver implementation
  }
}