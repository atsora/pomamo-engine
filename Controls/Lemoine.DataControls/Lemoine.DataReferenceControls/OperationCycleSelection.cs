// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
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
  /// Control to select an operation cycle
  /// </summary>
  public partial class OperationCycleSelection : UserControl
  {
    #region Members
    IMonitoredMachine m_machine = null;
    DateTime? m_date;
    TimeSpan? m_duration;
    bool m_OperationSlotMustExit = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleSelection).FullName);

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
    /// Is a null OperationCycle a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null Operation Cycle valid ?")]
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
    /// Selected OperationCycles
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    public IList<IOperationCycle> SelectedOperationCycles {
      get
      {
        IList<IOperationCycle> list =
          new List<IOperationCycle> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in dataGridView.SelectedRows) {
            CustomOperationCycleRow row = (CustomOperationCycleRow) (item as DataGridViewRow).DataBoundItem;
            list.Add (row.GetCycle());
          }
        }
        return list;
      }
    }
    
    private CustomOperationCycleRow FirstSelectedRow {
      get {
        if (dataGridView.SelectedRows.Count >= 1) {
          IEnumerator itemEnum = dataGridView.SelectedRows.GetEnumerator();
          itemEnum.MoveNext();
          CustomOperationCycleRow row = (CustomOperationCycleRow) (itemEnum.Current as DataGridViewRow).DataBoundItem;
          return row;
        }
        return null;
      }
    }
    /// <summary>
    /// First selected operation cycle
    /// </summary>
    public IOperationCycle SelectedOperationCycle {
      get {
        if (FirstSelectedRow != null) {
          return FirstSelectedRow.GetCycle();
        }
        return null;
      }
    }
    
    /// <summary>
    /// First selected deliverable piece
    /// </summary>
    public IDeliverablePiece SelectedDeliverablePiece {
      get {
        if (FirstSelectedRow != null) {
          return FirstSelectedRow.GetDeliverablePiece();
        }
        return null;
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
    
    /// <summary>
    /// Fetch cycles with an operation slot only
    /// </summary>
    public bool OperationSlotMustExist {
      get { return m_OperationSlotMustExit; }
      set { m_OperationSlotMustExit = value; }
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
    public OperationCycleSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("OperationCycleNull");
      
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
    
    class CustomOperationCycleRow {
      IOperationCycle m_operationCycle;
      IDeliverablePiece m_deliverablePiece;
      string m_deliverablePieceDisplay;
      string m_operationDisplay;
      string m_workOrderDisplay;
      string m_componentDisplay;
      
      public string Begin {
        get {
          if (m_operationCycle.Begin.HasValue) {
            return m_operationCycle.Begin.Value.ToLocalTime().ToString(CultureInfo.CurrentUICulture);
          } else {
            return "";
          }
        }
      }
      
      public string End {
        get {
          if (m_operationCycle.End.HasValue) {
            return m_operationCycle.End.Value.ToLocalTime().ToString(CultureInfo.CurrentUICulture);
          } else {
            return "";
          }
        }
      }
      
      public string SerialNumber {
        get { return m_deliverablePieceDisplay; }
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
      
      public IOperationCycle GetCycle() {
        return m_operationCycle;
      }
      
      public IDeliverablePiece GetDeliverablePiece() {
        return m_deliverablePiece;
      }
      
      public CustomOperationCycleRow(IOperationCycle operationCycle, IDeliverablePiece deliverablePiece) {
        m_operationCycle = operationCycle;
        m_deliverablePiece = deliverablePiece;
        IOperationSlot operationSlot = operationCycle.OperationSlot;
        if (operationSlot != null) {
          if (operationSlot.Operation != null) {
            m_operationDisplay = operationSlot.Operation.Display;
          }
          if (operationSlot.WorkOrder != null) {
            m_workOrderDisplay = operationSlot.WorkOrder.Display;
          }
          if (operationSlot.Component != null) {
            m_componentDisplay = operationSlot.Component.Display;
          }
          if (deliverablePiece != null) {
            m_deliverablePieceDisplay = deliverablePiece.Code; // TODO: correctly setup display
          }
          
        }
      }
    }
    
    /// <summary>
    /// Reload control
    /// </summary>
    public void Reload()
    {
      IList<IOperationCycle> operationCycles;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      if ((this.Machine != null) && (this.Date.HasValue) && (this.Duration.HasValue)) {
        using (IDAOSession session = daoFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("OperationCycleSelection.Load"))
        {
          IList<CustomOperationCycleRow> customRows = new List<CustomOperationCycleRow>();
          
          operationCycles =
            daoFactory.OperationCycleDAO.FindAllInRange (this.Machine,
                                                         new UtcDateTimeRange (this.Date.Value.Subtract(this.Duration.Value),
                                                                               this.Date.Value.Add(this.Duration.Value)));
          
          for (int i = operationCycles.Count - 1; 0 <= i; i--) { // From the last to the first
            IOperationCycle operationCycle = operationCycles [i];
            if (this.OperationSlotMustExist && operationCycle.OperationSlot == null) {
              continue;
            }
            IList<IOperationCycleDeliverablePiece>
              deliverablePieces = daoFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(operationCycle);
            if (deliverablePieces.Count > 0) {
              foreach (IOperationCycleDeliverablePiece operationCycleDeliverablePiece in deliverablePieces) {
                CustomOperationCycleRow row =
                  new CustomOperationCycleRow(operationCycle, operationCycleDeliverablePiece.DeliverablePiece);
                customRows.Add(row);
              }
            } else {
              CustomOperationCycleRow row =
                new CustomOperationCycleRow(operationCycle, null);
              customRows.Add(row);
            }
          }
          dataGridView.DataSource = customRows;
        }
      }
    }
    
    void OperationCycleSelectionLoad(object sender, EventArgs e)
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
    
    void DataGridViewSelectionChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
    
    /// <summary>
    /// Reset the selection
    /// </summary>
    public void ResetSelection ()
    {
      dataGridView.ClearSelection ();
    }
  }
}
