// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLine
{
  /// <summary>
  /// Description of OperationDetails.
  /// </summary>
  public partial class OperationDetails : UserControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationDetails()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Display the details of an operation
    /// </summary>
    /// <param name="operation"></param>
    public void Init(IOperation operation)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(operation);
          labelName.Text = operation.Name;
          labelCode.Text = operation.Code;
          
          var numbers = new List<string>();
          if (operation.IntermediateWorkPieces != null) {
            foreach (IIntermediateWorkPiece iwp in operation.IntermediateWorkPieces) {
              numbers.Add(iwp.OperationQuantity.ToString());
            }

            if (numbers.Count == 0) {
              labelParts.Text = "-";
            }
            else if (numbers.Count == 1) {
              labelParts.Text = numbers[0];
            }
            else {
              labelParts.Text = "{" + String.Join(", ", numbers.ToArray()) + "}";
            }
          } else {
            labelParts.Text = "-";
          }
        }
      }
    }
    #endregion // Methods
  }
}
