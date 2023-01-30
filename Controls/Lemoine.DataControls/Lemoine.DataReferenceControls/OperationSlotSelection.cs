// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select an operation slot
  /// </summary>
  public partial class OperationSlotSelection : UserControl
  {
    #region Members
    IMonitoredMachine m_machine = null;
    DateTime? m_date;
    TimeSpan? m_duration;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return dataGridView.MultiSelect; }
      set
      {
        dataGridView.MultiSelect = value;
      }
    }
    
    /// <summary>
    /// Is a null OperationSlot a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null OperationSlot valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }

    /// <summary>
    /// Selected Xxxs
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    public IList<IOperationSlot> SelectedOperationSlots {
      get
      {
        IList<IOperationSlot> list =
          new List<IOperationSlot> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in dataGridView.SelectedRows) {
            IOperationSlot operationSlot =
              item as IOperationSlot;
            list.Add (operationSlot);
          }
        }
        return list;
      }
    }
    
    /// <summary>
    /// Selected Machine
    /// </summary>
    public IMonitoredMachine Machine {
      get { return m_machine; }
      set { m_machine = value; }
    }
    
    /// <summary>
    /// Selected date
    /// </summary>
    public DateTime? Date {
      get { return m_date; }
      set { m_date = value; }
    }
    
    /// <summary>
    /// Selected duration
    /// </summary>
    public TimeSpan? Duration {
      get { return m_duration; }
      set { m_duration = value; }
    }

    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationSlotSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("OperationSlotNull");
      
      this.Nullable = false;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Raise the AfterSelect event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterSelect (EventArgs e)
    {
      if (null != AfterSelect) {
        AfterSelect (this, e);
      }
    }
    #endregion // Methods
    
    class CustomOperationSlotRow {
      IOperationSlot m_operationSlot;
      string m_operationDisplay;
      string m_workOrderDisplay;
      string m_componentDisplay;
      
      public string Begin {
        get {
          return m_operationSlot.BeginDateTime.HasValue
            ? m_operationSlot.BeginDateTime.Value.ToLocalTime().ToString(CultureInfo.CurrentUICulture)
            : "";
        }
      }
      
      public string End {
        get {
          if (m_operationSlot.EndDateTime.HasValue) {
            return m_operationSlot.EndDateTime.Value.ToLocalTime().ToString(CultureInfo.CurrentUICulture);
          } else {
            return "";
          }
        }
      }
      
      public string Operation {
        get { return m_operationDisplay; }
      }
      
      public string WorkOrder {
        get { return m_workOrderDisplay; }
      }
      
      public string Component {
        get { return m_componentDisplay; }
      }
      
      public CustomOperationSlotRow(IOperationSlot operationSlot) {
        m_operationSlot = operationSlot;
        if (operationSlot.Operation != null) {
          m_operationDisplay = operationSlot.Operation.Display;
        }
        if (operationSlot.WorkOrder != null) {
          m_workOrderDisplay = operationSlot.WorkOrder.Display;
        }
        if (operationSlot.Component != null) {
          m_componentDisplay = operationSlot.Component.Display;
        }
      }
    }
    
    /// <summary>
    /// Reload control
    /// </summary>
    public void Reload()
    {
      IList<IOperationSlot> operationSlots;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      if ((this.Machine != null) && (this.Date.HasValue) && (this.Duration.HasValue)) {
        using (IDAOSession session = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("OperationSlotSelection.Load"))
        {
          IList<CustomOperationSlotRow> customRows = new List<CustomOperationSlotRow>();
          
          operationSlots =
            daoFactory.OperationSlotDAO
            .FindOverlapsRange (this.Machine,
                                new UtcDateTimeRange (this.Date.Value.Subtract(this.Duration.Value),
                                                      this.Date.Value.Add(this.Duration.Value)));
          
          foreach (IOperationSlot operationSlot in operationSlots) {
            CustomOperationSlotRow row = new CustomOperationSlotRow(operationSlot);
            customRows.Add(row);
          }
          dataGridView.DataSource = customRows;
        }
      }
    }
    
    void OperationSlotSelectionLoad(object sender, EventArgs e)
    {
      this.Reload();
    }

    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        dataGridView.Enabled = false;
      }
      else {
        dataGridView.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
