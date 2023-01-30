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

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of DataStructureDisplayConfig.
  /// </summary>
  public partial class DataStructureDisplayConfig : UserControl
  {
    #region Members
    SortableBindingList<IDisplay> m_displays = new SortableBindingList<IDisplay>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DataStructureDisplayConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DataStructureDisplayConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("Display");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      tableColumn.HeaderText = PulseCatalog.GetString ("DataStructureDisplayTable");
      patternColumn.HeaderText = PulseCatalog.GetString ("DataStructureDisplayPattern");

      m_displays.SortColumns = false;
    }
    #endregion // Constructors
    
    void DataStructureDisplayConfigLoad(object sender, EventArgs e)
    {
      DataStructureDisplayConfigLoad ();
    }
    
    void DataStructureDisplayConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("DataStructureDisplayConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IDisplay> displays =
          daoFactory.DisplayDAO.FindAll ();

        m_displays.Clear ();
        foreach(IDisplay display in displays) {
          m_displays.Add(display);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_displays;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void DataStructureDisplayConfigValidated(object sender, EventArgs e)
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
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IDisplay display = row.DataBoundItem as IDisplay;
          if (null == display) {
            continue; // The row may have been deleted since
          }
          daoFactory.DisplayDAO.MakePersistent (display);
        }
        
        transaction.Commit ();
        m_updateSet.Clear ();
      }
      
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IDisplay display =
          row.DataBoundItem
          as IDisplay;
        if (null != display) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      var dialog = new TextDialog ();
      dialog.Message = "Table name";
      dialog.Text = "";
      if (DialogResult.OK != dialog.ShowDialog ()) {
        e.NewObject = ModelDAOHelper.ModelFactory.CreateDisplay ("");
      }
      else { // OK
        e.NewObject = ModelDAOHelper.ModelFactory.CreateDisplay (dialog.InputText);
      }
    }
  }
}
