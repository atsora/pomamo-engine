// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of PageSelectedReason.
  /// </summary>
  internal partial class PageSelectedReason : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Selectable reasons"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select the different selectable reasons you want for the machine modes and " +
          "machine observation states previously selected.\n\n" +
          "For each reason, you can force the user to add details. A machine filter can also be attached if " +
          "the reason is specific to some machines and is not applicable to others.\n\n" +
          "If a reason has to be overwritten for a machine, take care at least 2 selectable reasons are provided."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        // IReason not added since the previous page uses it
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageSelectedReason()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
      verticalScroll.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IReason> reasons = ModelDAOHelper.DAOFactory.ReasonDAO.FindAllWithReasonGroup();
          (reasons as List<IReason>).Sort((x, y) => {
                                            string a = "";
                                            if (x.ReasonGroup != null) {
              a = x.ReasonGroup.Display;
            }

            a += "_" + x.Display;
                                            
                                            string b = "";
                                            if (y.ReasonGroup != null) {
              b = y.ReasonGroup.Display;
            }

            b += "_" + y.Display;
                                            
                                            return a.CompareTo(b);
                                          });
          
          IReasonGroup currentReasonGroup = null;
          bool firstElement = true;
          foreach (IReason reason in reasons) {
            // Label of the group
            if (firstElement || !object.Equals(currentReasonGroup, reason.ReasonGroup)) {
              firstElement = false;
              Label label = new Label();
              if (reason.ReasonGroup != null) {
                label.Text = reason.ReasonGroup.Display;
              }
              else {
                label.Text = "No group";
              }

              currentReasonGroup = reason.ReasonGroup;
              label.Dock = DockStyle.Fill;
              label.TextAlign = ContentAlignment.MiddleLeft;
              label.Font = new Font(label.Font, FontStyle.Bold);
              verticalScroll.AddControl(label);
            }
            
            // Reason
            var control = new ReasonCell(reason);
            control.Dock = DockStyle.Fill;
            control.OnChecked += OnCellChecked;
            verticalScroll.AddControl(control);
          }
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      IDictionary<IReason, ReasonParameters> parameteredReason =
        data.Get<IDictionary<IReason, ReasonParameters>>(Item.SELECTABLE_REASONS);
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as ReasonCell;
        if (cell != null) {
          if (parameteredReason.ContainsKey(cell.Reason)) {
            cell.DetailsRequired = parameteredReason[cell.Reason].DetailsRequired;
            cell.MachineFilter = parameteredReason[cell.Reason].MachineFilter;
            cell.Checked = true;
          } else {
            cell.DetailsRequired = false;
            cell.MachineFilter = null;
            cell.Checked = false;
          }
        }
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IDictionary<IReason, ReasonParameters> parameteredReasons = new Dictionary<IReason, ReasonParameters>();
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as ReasonCell;
        if (cell != null) {
          if (cell.Checked) {
            parameteredReasons[cell.Reason] = new ReasonParameters(cell);
          }
        }
      }
      data.Store(Item.SELECTABLE_REASONS, parameteredReasons);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      var defaultReasons = data.Get<IDictionary<IReason, DefaultReasonParameters>>(Item.DEFAULT_REASONS);
      var selectableReasons = data.Get<IDictionary<IReason, ReasonParameters>>(Item.SELECTABLE_REASONS);
      
      // Default reasons, selectable reasons and overwrite per machine
      var machineDefaultReasons = new Dictionary<IMachine, IList<IReason>>();
      var machineOverwrites = new Dictionary<IMachine, bool>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          foreach (IMachine machine in machines) {
            
            // Initialization
            machineDefaultReasons[machine] = new List<IReason>();
            machineOverwrites[machine] = false;
            
            // Filling
            foreach (IReason reason in defaultReasons.Keys) {
              DefaultReasonParameters parameter = defaultReasons[reason];
              bool ok = true;
              if (parameter.MachineFilterInclude != null) {
                ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(parameter.MachineFilterInclude);
                ok &= parameter.MachineFilterInclude.IsMatch(machine);
              }
              if (parameter.MachineFilterExclude != null) {
                ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(parameter.MachineFilterExclude);
                ok &= !parameter.MachineFilterExclude.IsMatch(machine);
              }
              if (ok) {
                machineDefaultReasons[machine].Add(reason);
                machineOverwrites[machine] |= defaultReasons[reason].OverwriteRequired;
              }
            }
          }
        }
      }
      
      var machineSelectableReasons = new Dictionary<IMachine, IList<IReason>>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          foreach (IMachine machine in machines) {
            
            // Initialization
            machineSelectableReasons[machine] = new List<IReason>();
            
            // Filling
            foreach (IReason reason in selectableReasons.Keys) {
              ReasonParameters parameter = selectableReasons[reason];
              bool ok = true;
              if (parameter.MachineFilter != null) {
                ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(parameter.MachineFilter);
                ok &= parameter.MachineFilter.IsMatch(machine);
              }
              if (ok) {
                machineSelectableReasons[machine].Add(reason);
              }
            }
          }
        }
      }
      
      // At least two reasons must be provided for machines having default reasons that must be overwritten
      int count = 0;
      foreach (IMachine machine in machineOverwrites.Keys) {
        if (machineOverwrites[machine] && machineSelectableReasons[machine].Count < 2) {
          count++;
        }
      }
      if (count > 0) {
        errors.Add("a machine with a default reason which needs to be overwritten must have at least 2 " +
                   "selectable reasons (" + count + " machine" + (count > 1 ? "s" : "") + " impacted)");
      }

      // A machine cannot have a default reason which is also a selectable reason
      count = 0;
      foreach (IMachine machine in machineDefaultReasons.Keys) {
        foreach (IReason reason in machineDefaultReasons[machine]) {
          if (machineSelectableReasons[machine].Contains(reason)) {
            ++count;
            break;
          }
        }
      }
      if (count > 0) {
        errors.Add("a machine cannot have a default reason which is also a selectable reason " +
                   "(" + count + " machine" + (count > 1 ? "s" : "") + " impacted)");
      }

      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      IDictionary<IReason, ReasonParameters> parameteredReasons =
        data.Get<IDictionary<IReason, ReasonParameters>>(Item.SELECTABLE_REASONS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IReason reason in parameteredReasons.Keys) {
            ModelDAOHelper.DAOFactory.ReasonDAO.Lock(reason);
            ReasonParameters param = parameteredReasons[reason];
            
            string text = reason.Display;
            
            if (param.DetailsRequired) {
              text += "\ndetails required";
            }
            else {
              text += "\ndetails not required";
            }

            if (param.MachineFilter == null) {
              text += "\nno machine filter for inclusion";
            }
            else {
              text += "\nmachine filter \"" + param.MachineFilter.Name + "\" for inclusion";
            }

            summary.Add(text);
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Event reactions
    void OnCellChecked()
    {
      verticalScroll.UpdateScroll();
    }
    #endregion // Event reactions
  }
}
