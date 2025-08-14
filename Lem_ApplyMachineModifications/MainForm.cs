// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using Lemoine.Model;

using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System.Diagnostics;
using System.Linq;

namespace Lem_ApplyMachineModifications
{
  /// <summary>
  /// Displays a progress bar tracking modifications analysis status
  /// </summary>
  public partial class MainForm : Form
  {
    #region Members
    readonly Options m_options;
    ICollection<IModification> m_allModifications = new List<IModification> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of modifications the analysis of which must be tracked
    /// </summary>
    public ICollection<IModification> Modifications
    {
      get { return m_allModifications; }
      set { m_allModifications = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm (Options options)
    {
      m_options = options;

      Application.ThreadException += this.UnhandledThreadExceptionHandler;

      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      this.Load += new System.EventHandler (this.ModificationProgressFormLoad);
    }

    #endregion // Constructors

    #region Methods
    void ModificationProgressFormLoad (object sender, EventArgs e)
    {
      this.modificationProgressBar1.AddModifications ((System.Collections.ICollection)this.Modifications);

      this.modificationProgressBar1.AnalysisProgressLoad ();
    }

    void ProgressBarDisposed (object sender, EventArgs e)
    {
      if (this.modificationProgressBar1.Progress == 100) {
        Environment.ExitCode = 0;
      }
      else {
        Environment.ExitCode = -1;
      }
      this.Dispose ();
      Application.Exit ();
    }

    /// <summary>
    /// Handle all UI exceptions that are not consumed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UnhandledThreadExceptionHandler (object sender, ThreadExceptionEventArgs e)
    {
      log.Fatal ($"UnhandledThreadExceptionHandler: exception {e} was raised", e.Exception);

      // Message to the user
      var dialog = new Lemoine.BaseControls.ExceptionDialog (e.Exception, true);
      dialog.ShowDialog (this);

      Application.Exit ();
    }
    #endregion // Methods

    private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }

    private void MainForm_Load (object sender, EventArgs e)
    {
      string usage = "";
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (0 == m_options.ModificationDescriptions.Count ()) {
          log.DebugFormat ("Main: " +
                           "no modification description " +
                           "=> nothing to do");
          return;
        }

        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (m_options.MachineId);
        if (null == machine) {
          log.ErrorFormat ("Main: " +
                           "machine {0} is not valid",
                           m_options.MachineId);
          RaiseArgumentError (usage,
                              Lemoine.I18N.PulseCatalog.GetString ("InvalidMachine"));
        }

        log.InfoFormat ("Main: " +
                        "options are MachineId={0} UpdaterId={1}",
                        m_options.MachineId, m_options.UpdaterId);
        foreach (var description in m_options.ModificationDescriptions) {
          log.InfoFormat ("Main: " +
                          "description={0}",
                          description);
        }

        using (IDAOTransaction transaction = session.BeginTransaction ("Lem_ApplyMachineModifications")) {
          IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
          revision.Application = "Lem_ApplyMachineModifications";
          revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().FirstOrDefault ();
          if (int.MinValue != m_options.UpdaterId) {
            IUser user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (m_options.UpdaterId);
            if (null != user) {
              revision.Updater = user;
            }
            // TODO: service
          }
          ModelDAOHelper.DAOFactory.RevisionDAO
            .MakePersistent (revision);

          foreach (string modificationDescription in m_options.ModificationDescriptions) {
            if (string.IsNullOrEmpty (modificationDescription)) {
              continue;
            }
            string[] descriptionItems = modificationDescription.Split (new char[] { ';' }, 4);
            if (descriptionItems.Length < 3) {
              log.ErrorFormat ("Main: " +
                               "invalid modification description, not 3 elements");
              RaiseArgumentError (usage,
                                  "invalid modification description " + modificationDescription);
              return;
            }
            UtcDateTimeRange range;
            if (string.IsNullOrEmpty (descriptionItems[1])) { // From now to +oo
              range = new UtcDateTimeRange (DateTime.UtcNow);
            }
            else {
              range = new UtcDateTimeRange (descriptionItems[1]);
            }
            IMachineModification modification;
            if (descriptionItems[0].Equals ("Reason")) {
              long modificationId;
              if (!string.IsNullOrEmpty (descriptionItems[2])
                  && !descriptionItems[2].Equals ("0")) {
                IReason reason = null;
                int reasonId;
                if (int.TryParse (descriptionItems[2], out reasonId)) {
                  reason = ModelDAOHelper.DAOFactory.ReasonDAO
                    .FindById (reasonId);
                }
                if (null == reason) {
                  reason = ModelDAOHelper.DAOFactory.ReasonDAO
                    .FindByCode (descriptionItems[2]);
                }
                if (null == reason) {
                  log.ErrorFormat ("Main: " +
                                   "reason {0} is not valid",
                                   descriptionItems[2]);
                  var dialog = new Lemoine.BaseControls.UsageDialog (usage,
                                                                     Lemoine.I18N.PulseCatalog.GetString ("InvalidReason"));
                  dialog.ShowDialog ();
                  continue;
                }
                Debug.Assert (null != reason);
                string details = null;
                if (4 == descriptionItems.Length) { // Extra reason
                  details = descriptionItems[3].Trim (new char[] { '\'', '\"' });
                  if (!string.IsNullOrEmpty (details)) {
                  }
                }
                modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertManualReason (machine, range, reason, 100.0, details, null);
              }
              else {
                modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertResetReason (machine, range);
              }
              modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
                .FindById (modificationId, machine);
              modification.Revision = revision;
              ModelDAOHelper.DAOFactory.MachineModificationDAO.MakePersistent (modification);
            }
            else if (descriptionItems[0].Equals ("MachineStateTemplate")) {
              IMachineStateTemplate machineStateTemplate = null;
              int machineStateTemplateId;
              if (int.TryParse (descriptionItems[2], out machineStateTemplateId)) {
                machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                  .FindById (machineStateTemplateId);
              }
              if (null == machineStateTemplate) {
                log.ErrorFormat ("Main: " +
                                 "machine state template Id {0} is not valid",
                                 descriptionItems[2]);
                var dialog = new Lemoine.BaseControls.UsageDialog (usage,
                                                                   Lemoine.I18N.PulseCatalog.GetString ("InvalidMachineStateTemplate"));
                dialog.ShowDialog ();
                continue;
              }
              var association = ModelDAOHelper.ModelFactory
                .CreateMachineStateTemplateAssociation (machine, machineStateTemplate, range);
              modification = association;
              if (4 == descriptionItems.Length) { // shift
                IShift shift = null;
                int shiftId;
                if (int.TryParse (descriptionItems[3], out shiftId)) {
                  shift = ModelDAOHelper.DAOFactory.ShiftDAO
                    .FindById (shiftId);
                }
                if (null == shift) {
                  log.ErrorFormat ("Main: " +
                                   "shift Id {0} is not valid",
                                   descriptionItems[3]);
                  var dialog = new Lemoine.BaseControls.UsageDialog (usage,
                                                                     Lemoine.I18N.PulseCatalog.GetString ("InvalidShift"));
                  dialog.ShowDialog ();
                }
                else {
                  association.Shift = shift;
                }
              }
              association.Revision = revision;
              ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
            }
            else if (descriptionItems[0].Equals ("ResetTask")) {
              var association = ModelDAOHelper.ModelFactory
                .CreateWorkOrderMachineAssociation (machine, null, range);
              modification = association;
              association.ResetManufacturingOrder = true;
              association.Revision = revision;
              ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (association);
            }
            else if (descriptionItems[0].Equals ("Task")) {
              IManufacturingOrder manufacturingOrder = null;
              int taskId;
              if (int.TryParse (descriptionItems[2], out taskId)) {
                manufacturingOrder = ModelDAOHelper.DAOFactory.ManufacturingOrderDAO
                  .FindById (taskId);
              }
              if (null == manufacturingOrder) {
                log.ErrorFormat ("Main: " +
                                 "task Id {0} is not valid",
                                 descriptionItems[2]);
                var dialog = new Lemoine.BaseControls.UsageDialog (usage,
                                                                   Lemoine.I18N.PulseCatalog.GetString ("InvalidTask"));
                dialog.ShowDialog ();
                continue;
              }
              var association = ModelDAOHelper.ModelFactory
                .CreateManufacturingOrderMachineAssociation (machine, manufacturingOrder, range);
              modification = association;
              association.Revision = revision;
              ModelDAOHelper.DAOFactory.ManufacturingOrderMachineAssociationDAO.MakePersistent (association);
            }
            else if (descriptionItems[0].Equals ("ResetWorkOrderComponent")) {
              { // WorkOrder
                var association = ModelDAOHelper.ModelFactory
                  .CreateWorkOrderMachineAssociation (machine, null, range);
                modification = association;
                association.Revision = revision;
                ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (association);
              }
              this.Modifications.Add (modification);
              { // Component
                var association = ModelDAOHelper.ModelFactory
                  .CreateComponentMachineAssociation (machine, null, range);
                modification = association;
                association.Revision = revision;
                ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (association);
              }
            }
            else if (descriptionItems[0].Equals ("ResetOperation")) {
              var association = ModelDAOHelper.ModelFactory
                .CreateOperationMachineAssociation (machine, range);
              modification = association;
              association.Revision = revision;
              ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
            }
            else {
              log.ErrorFormat ("Main: " +
                               "not supported modification type, skip it");
              continue;
            }
            this.Modifications.Add (modification);
          }

          transaction.Commit ();
        }
      }

      if (this.Modifications.Count > 0) {
        log.Info ($"Main: {this.Modifications.Count} modifications");
      }
      else {
        log.Error ("No modification");
        Environment.ExitCode = -2;
        return;
      }
    }

    static void RaiseArgumentError (string usage, string additionalText)
    {
      var dialog = new Lemoine.BaseControls.UsageDialog (usage, additionalText);
      dialog.ShowDialog ();
      Environment.Exit (1);
    }
  }
}
