// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lem_Settings
{
  /// <summary>
  /// Description of RevisionDetailsCell.
  /// </summary>
  public partial class RevisionDetailsCell : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RevisionDetailsCell).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RevisionDetailsCell(Image image, IModification modification)
    {
      InitializeComponent();
      bool child = (modification.Parent != null);

      // Image
      pictureBox.Image = image;
      
      // Type of modification, id
      var typeSplit = modification.GetType().ToString().Split('.');
      labelMainInformation.Text = typeSplit[typeSplit.Length - 1] + " (#" + ((IDataWithId<long>)modification).Id + ")";
      
      // Machine
      var machineModification = modification as IMachineModification;
      if (machineModification != null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            if (machineModification.Machine != null) {
              labelMainInformation.Text += " on ";
              if (machineModification.Machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Obsolete) {
                labelMainInformation.Text += "obsolete ";
              }

              labelMainInformation.Text += "machine '" + machineModification.Machine.Name + "'";
            }
          }
        }
      }
      
      // Status of the modification
      labelStatus.Text = "Status: " + modification.AnalysisStatus.GetDescription() +
        " (" + modification.AnalysisStatus + ")";
      
      // Details (iteration, machine)
      labelDetails.Text = (child ? "(CHILD) " : "") + modification.AnalysisIterations + " attempt" + (modification.AnalysisIterations > 1 ? "s" : "") +
        ", analysis duration: " + modification.AnalysisTotalDuration.TotalSeconds + " s";

      // Grey if it's a child
      if (child) {
        labelMainInformation.ForeColor = labelStatus.ForeColor = labelDetails.ForeColor = SystemColors.ControlDarkDark;
      }
    }
    #endregion // Constructors
  }
}
