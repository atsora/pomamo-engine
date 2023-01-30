// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

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
  /// Description of UserConfig.
  /// </summary>
  public partial class UserConfig
    : UserControl
    , IConfigControlObservable<IUser>
  {
    #region Members
    SortableBindingList<IUser> m_users = new SortableBindingList<IUser> ();

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IUser> m_deleteList =
      new List<IUser> ();

    ISet<IConfigControlObserver<IUser>> m_observers =
      new HashSet<IConfigControlObserver<IUser>> ();

    string m_noPasswordString = null;
    int m_companyId = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (UserConfig).FullName);

    /// <summary>
    /// Restrict the data to this company id
    /// </summary>
    public int CompanyId
    {
      get { return m_companyId; }
      set {
        m_companyId = value;
        ToggleCompanyColumnVisibility (m_companyId <= 0);
      }
    }

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserConfig ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("User");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      codeColumn.HeaderText = PulseCatalog.GetString ("UserCode");
      externalCodeColumn.HeaderText = PulseCatalog.GetString ("ExternalCode");
      loginColumn.HeaderText = PulseCatalog.GetString ("Login");
      passwordColumn.HeaderText = PulseCatalog.GetString ("Password");
      shiftColumn.HeaderText = PulseCatalog.GetString ("Shift");
      mobileNumberColumn.HeaderText = PulseCatalog.GetString ("MobileNumber");
      companyColumn.HeaderText = PulseCatalog.GetString ("Company");
      roleColumn.HeaderText = PulseCatalog.GetString ("Role");

      m_noPasswordString = PulseCatalog.GetString ("NoPassword");

      m_users.SortColumns = false;

      {
        ShiftDialog dialog = new ShiftDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IShift> (dialog);
        shiftColumn.CellTemplate = cell;
      }

      {
        CompanyDialog dialog = new CompanyDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<ICompany> (dialog);
        companyColumn.CellTemplate = cell;
      }

      {
        RoleDialog dialog = new RoleDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "Display";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IRole> (dialog);
        roleColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void UserConfigLoad (object sender, EventArgs e)
    {
      UserConfigLoad ();
    }

    void UserConfigLoad ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("UserConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IEnumerable<IUser> users =
          daoFactory.UserDAO.FindAll ();
        if (0 < this.CompanyId) {
          users = users
            .Where (x => (null != x.Company) && (this.CompanyId == x.Company.Id));
        }

        m_users.Clear ();
        foreach (IUser user in users) {
          m_users.Add (user);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_users;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler (BindingSourceAddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }

    void UserConfigValidated (object sender, EventArgs e)
    {
      CommitChanges ();
    }

    public void CommitChanges ()
    {
      if ((0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        foreach (DataGridViewRow row in m_updateSet) {
          IUser user = row.DataBoundItem as IUser;
          if (user is null) {
            continue; // The row may have been deleted since
          }
          daoFactory.UserDAO.MakePersistent (user);
        }

        foreach (IUser user in m_deleteList) {
          daoFactory.UserDAO.MakeTransient (user);
        }
        transaction.Commit ();
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();

      if (m_deleteList.Count >= 1) {
        NotifyDelete (m_deleteList);
      }
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
    {
      IUser user =
        e.Row.DataBoundItem
        as IUser;
      if (null != user) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (user);
      }
    }

    void DataGridViewCellValueChanged (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows[e.RowIndex];
        IUser user =
          row.DataBoundItem
          as IUser;
        if (null != user) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      var user = ModelDAOHelper.ModelFactory.CreateUser ("", "");
      if (0 < this.CompanyId) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          user.Company = ModelDAOHelper.DAOFactory.CompanyDAO.FindById (this.CompanyId);
        }
      }
      e.NewObject = user;
    }

    void DataGridViewCellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (this.dataGridView.Columns[e.ColumnIndex].Name == "passwordColumn") {
        if (e.Value is null) {
          e.Value = m_noPasswordString;
        }
        else { // Not null
          string strValue = e.Value as string;
          if (String.IsNullOrEmpty (strValue)) {
            e.Value = m_noPasswordString;
            return;
          }
          e.Value = new String ('*', 3);
        }
      }
    }

    void ToggleCompanyColumnVisibility (bool visible)
    {
      var columns = this.dataGridView.Columns;
      var columnCount = columns.Count;
      for (int i = 0; i < columnCount; ++i) {
        var column = columns[i];
        if (column.Name.Equals ("companyColumn")) {
          column.Visible = visible;
        }
      }
    }

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IUser> observer)
    {
      this.m_observers.Add (observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IUser> observer)
    {
      this.m_observers.Remove (observer);
    }

    /// <summary>
    /// Notify all observer from delete action
    /// </summary>
    /// <param name="deletedUsers"></param>
    void NotifyDelete (IList<IUser> deletedUsers)
    {
      foreach (IConfigControlObserver<IUser> observer in m_observers) {
        observer.UpdateAfterDelete (deletedUsers);
      }
    }

    /// <summary>
    /// Notify all observer from update action
    /// </summary>
    /// <param name="updatedUsers"></param>
    void NotifyUpdate (IList<IUser> updatedUsers)
    {
      foreach (IConfigControlObserver<IUser> observer in m_observers) {
        observer.UpdateAfterUpdate (updatedUsers);
      }
    }
    #endregion
  }
}
