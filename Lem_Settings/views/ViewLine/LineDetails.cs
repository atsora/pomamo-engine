// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLine
{
  /// <summary>
  /// Description of LineDetails.
  /// </summary>
  public partial class LineDetails : UserControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LineDetails()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Display the details of the line
    /// </summary>
    /// <param name="line"></param>
    public void Init(ILine line)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          
          labelName.Text = line.Name;
          labelCode.Text = line.Code;
          
          // List of parts
          listParts.ClearItems();
          foreach (IComponent component in line.Components) {
            string text = component.Name;
            if (component.Code != "") {
              text += " (" + component.Code + ")";
            }

            listParts.AddItem(text);
          }
          
          // List of periods
          listBoxPeriod.ClearItems();
          IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(line);
          foreach (IWorkOrderLine wol in wols) {
            string text = "";
            DateTime start = new DateTime(1970, 1, 1);
            if (wol.BeginDateTime.HasValue) {
              start = wol.BeginDateTime.Value.ToLocalTime();
              text += start.ToString("g");
            } else {
              text += "no start";
            }

            text += " - ";
            text += wol.Deadline.ToLocalTime().ToString("g");
            listBoxPeriod.AddItem(text, start, start);
          }
        }
      }
    }
    #endregion // Methods
  }
}
