// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
  /// Description of ComputerConfig.
  /// </summary>
  public partial class ComputerConfig : UserControl
  {
    #region Members
    SortableBindingList<IComputer> m_computers = new SortableBindingList<IComputer>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IComputer> m_deleteList =
      new List<IComputer> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ComputerConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ComputerConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Computer");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      addressColumn.HeaderText = PulseCatalog.GetString("Address");
      isLctrColumn.HeaderText = PulseCatalog.GetString("IsLctr");
      isLpstColumn.HeaderText = PulseCatalog.GetString("IsLpst");
      isCncColumn.HeaderText = PulseCatalog.GetString("IsCnc");
      isWebColumn.HeaderText = PulseCatalog.GetString("IsWeb");

      m_computers.SortColumns = false;
    }
    #endregion // Constructors
    
    void ComputerConfigLoad(object sender, EventArgs e)
    {
      ComputerConfigLoad ();
    }
    
    void ComputerConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ComputerConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IComputer> computers =
          daoFactory.ComputerDAO.FindAll ();

        m_computers.Clear ();
        foreach(IComputer computer in computers) {
          m_computers.Add(computer);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_computers;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.AutoGenerateColumns = false;
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void ComputerConfigValidated(object sender, EventArgs e)
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
          IComputer computer = row.DataBoundItem as IComputer;
          if (null == computer) {
            continue; // The row may have been deleted since
          }
          daoFactory.ComputerDAO.MakePersistent (computer);
        }

        foreach (IComputer computer in m_deleteList) {
          daoFactory.ComputerDAO.MakeTransient (computer);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IComputer computer =
        e.Row.DataBoundItem
        as IComputer;
      if (null != computer) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (computer);
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IComputer computer = row.DataBoundItem as IComputer;
        if (null != computer) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateComputer("","");
    }
  }
}
