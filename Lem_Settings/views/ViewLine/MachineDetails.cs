// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLine
{
  /// <summary>
  /// Description of MachineDetails.
  /// </summary>
  public partial class MachineDetails : UserControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineDetails()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    public void Init(ILineMachine lineMachine)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineMachineDAO.Lock(lineMachine);
          labelName.Text = lineMachine.Machine.Name;
          labelCode.Text = lineMachine.Machine.Code;
          labelDepartment.Text = (lineMachine.Machine.Department != null) ?
            lineMachine.Machine.Department.Display : "-";
          if (lineMachine.LineMachineStatus == LineMachineStatus.Dedicated) {
            labelDedicated.Text = "yes";
          }
          else {
            labelDedicated.Text = "no";
          }
        }
      }
    }
    #endregion // Methods
  }
}
