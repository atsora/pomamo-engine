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
  /// Description of MachineFilterConfig.
  /// </summary>
  public partial class MachineFilterConfig
    : UserControl
    , IConfigControlObservable<IMachineFilter>
    , IConfigControlObserver<ICompany>
    , IConfigControlObserver<IDepartment>
    , IConfigControlObserver<ICell>
    , IConfigControlObserver<IMachineCategory>
    , IConfigControlObserver<IMachineSubCategory>
  {
    #region Members
    SortableBindingList<IMachineFilter> m_machineFilters =
      new SortableBindingList<IMachineFilter> ();

    BindingList<IMachineFilterItem> m_machineFilterItems = null;

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineFilter> m_deleteList =
      new List<IMachineFilter> ();
    IList<IMachineFilterItem> m_itemDeleteList =
      new List<IMachineFilterItem> ();

    ISet<IConfigControlObserver<IMachineFilter>> m_observers =
      new HashSet<IConfigControlObserver<IMachineFilter>> ();
    #endregion // Members

    #region Getter / Setter
    IMachineFilter SelectedMachineFilter
    {
      get
      {
        if (dataGridViewFilters.SelectedRows.Count == 1) {
          return dataGridViewFilters.SelectedRows[0].DataBoundItem as IMachineFilter;
        }

        return null;
      }
    }

    DataGridViewRow SelectedMachineFilterRow
    {
      get
      {
        if (dataGridViewFilters.SelectedRows.Count == 1) {
          return dataGridViewFilters.SelectedRows[0];
        }

        return null;
      }
    }
    #endregion Getter / Setter


    static readonly ILog log = LogManager.GetLogger (typeof (MachineFilterConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineFilterConfig ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      dataGridViewFilters.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineFilter");

      idMachineFilterColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameMachineFilterColumn.HeaderText = PulseCatalog.GetString ("Name");

      m_machineFilters.SortColumns = true;

      //DatagridView MachineFilterItem
      machineFilterItemIdColumn.HeaderText = PulseCatalog.GetString ("Id");
      //      .HeaderText = PulseCatalog.GetString ("MachineFilterConfigOrderItemColumnHeader");
      machineFilterItemRuleColumn.HeaderText = PulseCatalog.GetString ("MachineFilterConfigRuleItemColumnHeader");
      machineFilterItemDescriptionColumn.HeaderText = PulseCatalog.GetString ("MachineFilterConfigDescriptionItemColumnHeader");

      // DatagridView Button
      itemFilterAddButton.Text = PulseCatalog.GetString ("MachineFilterConfigItemAddButton");
    }
    #endregion // Constructors

    #region MachineFilter DataGridView

    void MachineFilterConfigLoad (object sender, EventArgs e)
    {
      MachineFilterConfigLoad ();
      MachineFilterItemLoad ();
    }

    void MachineFilterConfigLoad ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineFilterConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IList<IMachineFilter> machineFilters =
          daoFactory.MachineFilterDAO.FindAllMachineFilterForConfig ();

        m_machineFilters.Clear ();
        foreach (IMachineFilter machineFilter in machineFilters) {
          m_machineFilters.Add (machineFilter);
        }

        initialSetColumn.DataSource = Enum.GetValues (typeof (MachineFilterInitialSet));
        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineFilters;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler (BindingSourceAddingNew);
        dataGridViewFilters.AutoGenerateColumns = false;
        dataGridViewFilters.DataSource = bindingSource;
      }
    }

    void MachineFilterConfigValidated (object sender, EventArgs e)
    {
      CommitChanges ();
    }

    void CommitChanges ()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count) && (0 == m_itemDeleteList.Count)) {
        return;
      }
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        //Only Checked Item
        foreach (IMachineFilterItem machineFilterItem in m_itemDeleteList) {
          daoFactory.MachineFilterDAO.DeleteMachineFilterItem (machineFilterItem);
        }

        foreach (DataGridViewRow row in m_updateSet) {
          IMachineFilter machineFilter = row.DataBoundItem as IMachineFilter;
          if (null == machineFilter) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineFilterDAO.MakePersistent (machineFilter);
        }

        foreach (IMachineFilter machineFilter in m_deleteList) {
          foreach (IMachineFilterItem machineFilterItem in machineFilter.Items) {
            daoFactory.MachineFilterDAO.DeleteMachineFilterItem (machineFilterItem);
          }
          daoFactory.MachineFilterDAO.MakeTransient (machineFilter);
        }

        transaction.Commit ();

        NotifyDelete (m_deleteList);

        m_updateSet.Clear ();
        m_deleteList.Clear ();
        m_itemDeleteList.Clear ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineFilter machineFilter =
        e.Row.DataBoundItem
        as IMachineFilter;
      if (null != machineFilter) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (machineFilter);
      }
    }

    void DataGridViewUserAddedRow (object sender, DataGridViewRowEventArgs e)
    {
      IMachineFilter machineFilter =
        e.Row.DataBoundItem
        as IMachineFilter;
      if (null != machineFilter) {
        m_updateSet.Add (e.Row);
      }
    }

    void DataGridViewCellValueChanged (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridViewFilters.Rows[e.RowIndex];
        IMachineFilter machineFilter =
          row.DataBoundItem
          as IMachineFilter;
        if (null != machineFilter) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineFilter (" ", MachineFilterInitialSet.None);
    }

    void DataGridViewSelectionChanged (object sender, System.EventArgs e)
    {
      MachineFilterItemLoad ();
    }
    #endregion // MachineFilter DataGridView

    #region IConfigControlObserver implementation
    /// <summary>
    /// Update this control after some items have been deleted
    /// in the CompanyConfig control
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (ICollection<ICompany> deletedEntities)
    {
      MachineFilterConfigLoad ();
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
      MachineFilterConfigLoad ();
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
      MachineFilterConfigLoad ();
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
      MachineFilterConfigLoad ();
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
      MachineFilterConfigLoad ();
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
    #endregion // IConfigControlObserver implementation

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineFilter> observer)
    {
      m_observers.Add (observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineFilter> observer)
    {
      m_observers.Remove (observer);
    }

    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IMachineFilter> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineFilter> observer in m_observers) {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation

    #region DataGridViewFilterItems

    void MachineFilterItemLoad ()
    {
      if (SelectedMachineFilter != null) {
        m_machineFilterItems = new BindingList<IMachineFilterItem> (SelectedMachineFilter.Items);
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineFilterItems;
        bindingSource.AllowNew = true;
        machineFilterItemDataGridView.AutoGenerateColumns = false;
        machineFilterItemDataGridView.DataSource = bindingSource;
      }
      else {
        machineFilterItemDataGridView.AutoGenerateColumns = false;
        machineFilterItemDataGridView.DataSource = null;
        m_machineFilterItems = null;
      }
    }

    void MachineFilterItemDataGridViewCellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (this.machineFilterItemDataGridView.Columns[e.ColumnIndex].Name == "machineFilterItemDescriptionColumn") {
        DataGridViewRow row = machineFilterItemDataGridView.Rows[e.RowIndex];
        IMachineFilterItem machineFilterItem = row.DataBoundItem as IMachineFilterItem;
        string cellValue = null;
        if (null != machineFilterItem) {
          if (machineFilterItem is IMachineFilterMachine) {
            IMachineFilterMachine machine = machineFilterItem as IMachineFilterMachine;
            if (machine != null) {
              cellValue += PulseCatalog.GetString ("MachineFilter");
              cellValue += " : " + machine.Machine.Display;
            }
          }
          if (machineFilterItem is IMachineFilterMachineCategory) {
            IMachineFilterMachineCategory category = machineFilterItem as IMachineFilterMachineCategory;
            if (category != null) {
              cellValue += PulseCatalog.GetString ("MachineCategory");
              cellValue += " : " + category.MachineCategory.Display;
            }
          }
          if (machineFilterItem is IMachineFilterMachineSubCategory) {
            IMachineFilterMachineSubCategory subCategory = machineFilterItem as IMachineFilterMachineSubCategory;
            if (subCategory != null) {
              cellValue += PulseCatalog.GetString ("MachineSubCategory");
              cellValue += " : " + subCategory.MachineSubCategory.Display;
            }
          }
          if (machineFilterItem is IMachineFilterCompany) {
            IMachineFilterCompany company = machineFilterItem as IMachineFilterCompany;
            if (company != null) {
              cellValue += PulseCatalog.GetString ("Company");
              cellValue += " : " + company.Company.Display;
            }
          }
          if (machineFilterItem is IMachineFilterDepartment) {
            IMachineFilterDepartment department = machineFilterItem as IMachineFilterDepartment;
            if (department != null) {
              cellValue += PulseCatalog.GetString ("Deparment");
              cellValue += " : " + department.Department.Display;
            }
          }
          if (machineFilterItem is IMachineFilterCell) {
            IMachineFilterCell cell = machineFilterItem as IMachineFilterCell;
            if (cell != null) {
              cellValue += PulseCatalog.GetString ("Cell");
              cellValue += " : " + cell.Cell.Display;
            }
          }
        }
        e.Value = cellValue;
      }
    }

    /// <summary>
    /// Call the MasterConfigDialog
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ItemFilterAddButtonClick (object sender, System.EventArgs e)
    {
      if (SelectedMachineFilter != null) {
        if (m_machineFilterItems == null) {
          MachineFilterItemLoad ();
        }
        var dialog = new MachineFilterConfigDialog ();
        var orderDialog = new OrderDialog ();
        if (dialog.ShowDialog () != DialogResult.OK) {
          return;
        }

        Object[] dialogValue = dialog.SelectedValue;

        //combobox return internal index and not Enum index
        dialogValue[1] = (int)dialogValue[1] == 1 ? -1 : 1;
        if (orderDialog.ShowDialog () == DialogResult.OK) {
          IMachineFilterItem machineFilterItem = null;
          if (dialogValue[0] is IMachine) {
            IMachine machine = dialogValue[0] as IMachine;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (machine, (MachineFilterRule)dialogValue[1]);
          }
          if (dialogValue[0] is IMachineCategory) {
            IMachineCategory machineCategory = dialogValue[0] as IMachineCategory;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (machineCategory, (MachineFilterRule)dialogValue[1]);
          }
          if (dialogValue[0] is IMachineSubCategory) {
            IMachineSubCategory machineSubCategory = dialogValue[0] as IMachineSubCategory;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (machineSubCategory, (MachineFilterRule)dialogValue[1]);
          }
          if (dialogValue[0] is ICell) {
            ICell cell = dialogValue[0] as ICell;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (cell, (MachineFilterRule)dialogValue[1]);
          }
          if (dialogValue[0] is ICompany) {
            ICompany company = dialogValue[0] as ICompany;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (company, (MachineFilterRule)dialogValue[1]);
          }
          if (dialogValue[0] is IDepartment) {
            IDepartment department = dialogValue[0] as IDepartment;
            machineFilterItem = ModelDAOHelper.ModelFactory.CreateMachineFilterItem (department, (MachineFilterRule)dialogValue[1]);
          }
          if (machineFilterItem != null) {
            if (!FindIfDuplicate (machineFilterItem)) {
              FindOppositeItem (machineFilterItem);
              if (orderDialog.UserSpecifiedIndex) {
                m_machineFilterItems.Insert (orderDialog.SelectedValue, machineFilterItem);
              }
              else {
                m_machineFilterItems.Add (machineFilterItem);
              }

              m_updateSet.Add (SelectedMachineFilterRow);
            }
          }
        }
      }
      else {
        MessageBox.Show (PulseCatalog.GetString ("MachineFilterSelectionError-DialogMSG"),
                        PulseCatalog.GetString ("MachineFilterSelectionError-DialogTitle"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
      }
    }

    void MachineFilterItemDataGridViewUserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineFilterItem machineFilterItem =
        e.Row.DataBoundItem
        as IMachineFilterItem;
      if (null != machineFilterItem) {
         m_itemDeleteList.Add (machineFilterItem);
         m_updateSet.Add (SelectedMachineFilterRow);
      }
    }

    /// <summary>
    /// Find if Item have a duplicate
    /// </summary>
    /// <param name="newMachineFilterItem"></param>
    private Boolean FindIfDuplicate (IMachineFilterItem newMachineFilterItem)
    {
      foreach (IMachineFilterItem machineFilterItem in SelectedMachineFilter.Items) {
        if (machineFilterItem is IMachineFilterMachine && newMachineFilterItem is IMachineFilterMachine) {
          IMachineFilterMachine machine = machineFilterItem as IMachineFilterMachine;
          IMachineFilterMachine machineToCmp = newMachineFilterItem as IMachineFilterMachine;
          if (machine.Machine.Equals (machineToCmp.Machine) && machine.Rule.Equals (machineToCmp.Rule)) {
            return true;
          }
        }
        if (machineFilterItem is IMachineFilterMachineCategory && newMachineFilterItem is IMachineFilterMachineCategory) {
          IMachineFilterMachineCategory category = machineFilterItem as IMachineFilterMachineCategory;
          IMachineFilterMachineCategory categoryToCmp = newMachineFilterItem as IMachineFilterMachineCategory;
          if (category.MachineCategory.Equals (categoryToCmp.MachineCategory) && category.Rule.Equals (categoryToCmp.Rule)) {
            return true;
          }
        }
        if (machineFilterItem is IMachineFilterMachineSubCategory && newMachineFilterItem is IMachineFilterMachineSubCategory) {
          IMachineFilterMachineSubCategory subCategory = machineFilterItem as IMachineFilterMachineSubCategory;
          IMachineFilterMachineSubCategory subCategoryToCmp = newMachineFilterItem as IMachineFilterMachineSubCategory;
          if (subCategory.MachineSubCategory.Equals (subCategoryToCmp.MachineSubCategory) && subCategory.Rule.Equals (subCategoryToCmp.Rule)) {
            return true;
          }
        }
        if (machineFilterItem is IMachineFilterCompany && newMachineFilterItem is IMachineFilterCompany) {
          IMachineFilterCompany company = machineFilterItem as IMachineFilterCompany;
          IMachineFilterCompany companyToCmp = newMachineFilterItem as IMachineFilterCompany;
          if (company.Company.Equals (companyToCmp.Company) && company.Rule.Equals (companyToCmp.Rule)) {
            return true;
          }
        }
        if (machineFilterItem is IMachineFilterDepartment && newMachineFilterItem is IMachineFilterDepartment) {
          IMachineFilterDepartment department = machineFilterItem as IMachineFilterDepartment;
          IMachineFilterDepartment departmentToCmp = newMachineFilterItem as IMachineFilterDepartment;
          if (department.Department.Equals (departmentToCmp.Department) && department.Rule.Equals (departmentToCmp.Rule)) {
            return true;
          }
        }
        if (machineFilterItem is IMachineFilterCell && newMachineFilterItem is IMachineFilterCell) {
          IMachineFilterCell cell = machineFilterItem as IMachineFilterCell;
          IMachineFilterCell cellToCmp = newMachineFilterItem as IMachineFilterCell;
          if (cell.Cell.Equals (cellToCmp.Cell) && cell.Rule.Equals (cellToCmp.Rule)) {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Find opposite Item and remove it if exist
    /// </summary>
    /// <param name="newMachineFilterItem"></param>
    private void FindOppositeItem (IMachineFilterItem newMachineFilterItem)
    {
      for (int i = 0; i < SelectedMachineFilter.Items.Count; i++) {
        IMachineFilterItem machineFilterItem = SelectedMachineFilter.Items[i];
        if (machineFilterItem != null && machineFilterItem.Rule != (MachineFilterRule)newMachineFilterItem.Rule) {

          if (machineFilterItem is IMachineFilterMachine && newMachineFilterItem is IMachineFilterMachine) {
            IMachineFilterMachine machineFilterMachine = machineFilterItem as IMachineFilterMachine;
            IMachineFilterMachine newMachineFilterMachine = newMachineFilterItem as IMachineFilterMachine;
            if (machineFilterMachine != null && newMachineFilterMachine != null) {
              if (machineFilterMachine.Machine.Id == newMachineFilterMachine.Machine.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

          if (machineFilterItem is IMachineFilterMachineCategory && newMachineFilterItem is IMachineFilterMachineCategory) {
            IMachineFilterMachineCategory machineFilterMachineCategory = machineFilterItem as IMachineFilterMachineCategory;
            IMachineFilterMachineCategory newMachineFilterMachineCategory = newMachineFilterItem as IMachineFilterMachineCategory;
            if (machineFilterMachineCategory != null && newMachineFilterMachineCategory != null) {
              if (machineFilterMachineCategory.MachineCategory.Id == newMachineFilterMachineCategory.MachineCategory.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

          if (machineFilterItem is IMachineFilterMachineSubCategory && newMachineFilterItem is IMachineFilterMachineSubCategory) {
            IMachineFilterMachineSubCategory machineFilterMachineSubCategory = machineFilterItem as IMachineFilterMachineSubCategory;
            IMachineFilterMachineSubCategory newMachineFilterMachineSubCategory = newMachineFilterItem as IMachineFilterMachineSubCategory;
            if (machineFilterMachineSubCategory != null && newMachineFilterMachineSubCategory != null) {
              if (machineFilterMachineSubCategory.MachineSubCategory.Id == newMachineFilterMachineSubCategory.MachineSubCategory.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

          if (machineFilterItem is IMachineFilterCompany && newMachineFilterItem is IMachineFilterCompany) {
            IMachineFilterCompany machineFilterCompagny = machineFilterItem as IMachineFilterCompany;
            IMachineFilterCompany newMachineFilterCompagny = newMachineFilterItem as IMachineFilterCompany;
            if (machineFilterCompagny != null && newMachineFilterCompagny != null) {
              if (machineFilterCompagny.Company.Id == newMachineFilterCompagny.Company.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

          if (machineFilterItem is IMachineFilterDepartment && newMachineFilterItem is IMachineFilterDepartment) {
            IMachineFilterDepartment machineFilterDepartement = machineFilterItem as IMachineFilterDepartment;
            IMachineFilterDepartment newMachineFilterDepartement = newMachineFilterItem as IMachineFilterDepartment;
            if (machineFilterDepartement != null && newMachineFilterDepartement != null) {
              if (machineFilterDepartement.Department.Id == newMachineFilterDepartement.Department.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

          if (machineFilterItem is IMachineFilterCell && newMachineFilterItem is IMachineFilterCell) {
            IMachineFilterCell machineFilterCell = machineFilterItem as IMachineFilterCell;
            IMachineFilterCell newMachineFilterCell = newMachineFilterItem as IMachineFilterCell;
            if (machineFilterCell != null && newMachineFilterCell != null) {
              if (machineFilterCell.Cell.Id == newMachineFilterCell.Cell.Id) {
                m_machineFilterItems.Remove (machineFilterItem);
                m_itemDeleteList.Add (machineFilterItem);
              }
            }
          }

        }
      }
    }

    #endregion //DataGridViewFilterItems
  }
}
