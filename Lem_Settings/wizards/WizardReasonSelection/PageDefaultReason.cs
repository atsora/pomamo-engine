// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of PageDefaultReason.
  /// </summary>
  internal partial class PageDefaultReason : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Default reasons"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select the default reason that will be assigned automatically " +
          "without the user intervention. This reason may depend on the context, you can thus choose several items.\n\n" +
          "For each default reason, you can force the user to fill another selectable reason (which will be chosen in the next page). " +
          "A machine filter can also be attached to include and / or exclude machines, if " +
          "the default reason is specific to some machines and is not applicable to others. If both filters are chosen, " +
          "the inclusion filter is applied first and then the exclusion filter is applied."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IReason));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageDefaultReason()
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
      // List of all reasons
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
            var control = new DefaultReasonCell(reason);
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
      IDictionary<IReason, DefaultReasonParameters> parameteredReason =
        data.Get<IDictionary<IReason, DefaultReasonParameters>>(Item.DEFAULT_REASONS);
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as DefaultReasonCell;
        if (cell != null) {
          if (parameteredReason.ContainsKey(cell.Reason)) {
            cell.OverwriteRequired = parameteredReason[cell.Reason].OverwriteRequired;
            cell.MachineFilterInclude = parameteredReason[cell.Reason].MachineFilterInclude;
            cell.MachineFilterExclude = parameteredReason[cell.Reason].MachineFilterExclude;
            cell.MaxTime = parameteredReason[cell.Reason].MaxTime;
            cell.Checked = true;
          } else {
            cell.OverwriteRequired = false;
            cell.MachineFilterInclude = null;
            cell.MachineFilterExclude = null;
            cell.MaxTime = null;
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
      IDictionary<IReason, DefaultReasonParameters> parameteredReasons = new Dictionary<IReason, DefaultReasonParameters>();
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as DefaultReasonCell;
        if (cell != null) {
          if (cell.Checked) {
            parameteredReasons[cell.Reason] = new DefaultReasonParameters(cell);
          }
        }
      }
      data.Store(Item.DEFAULT_REASONS, parameteredReasons);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // List of default reason parameters
      ICollection<DefaultReasonParameters> parameters =
        data.Get<IDictionary<IReason, DefaultReasonParameters>>(Item.DEFAULT_REASONS).Values;
      
      // At least one reason must be selected
      if (parameters.Count == 0) {
        errors.Add("at least one default reason must be selected");
      }
      else {
        // Maximum time, if specified, cannot be 0
        bool ok = true;
        foreach (DefaultReasonParameters parameter in parameters) {
          if (parameter.MaxTime.HasValue && parameter.MaxTime.Value.TotalSeconds < 1) {
            ok = false;
            break;
          }
        }
        if (!ok) {
          errors.Add("maximum time cannot be 0");
        }
        else {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
              
              // For all machines, list of all maximum times
              var machineTimes = new Dictionary<IMachine, IList<TimeSpan?>>();
              IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
              
              foreach (IMachine machine in machines) {
                machineTimes[machine] = new List<TimeSpan?>();
                foreach (DefaultReasonParameters parameter in parameters) {
                  ok = true;
                  if (parameter.MachineFilterInclude != null) {
                    ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(parameter.MachineFilterInclude);
                    ok &= parameter.MachineFilterInclude.IsMatch(machine);
                  }
                  if (parameter.MachineFilterExclude != null) {
                    ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(parameter.MachineFilterExclude);
                    ok &= !parameter.MachineFilterExclude.IsMatch(machine);
                  }
                  if (ok) {
                    machineTimes[machine].Add(parameter.MaxTime);
                  }
                }
              }
              
              // All machines must have a default reason, at any time
              var machinesWithNoDefaultReasons = new List<string>();
              
              // A machine cannot have two default reasons at the same time
              var machinesWithSeveralDefaultReasons = new List<string>();
              
              foreach (IMachine machine in machineTimes.Keys) {
                bool hasDefault = false;
                bool isUnique = true;
                for (int i = 0; i < machineTimes[machine].Count; i++) {
                  hasDefault |= !machineTimes[machine][i].HasValue;
                  
                  for (int j = i + 1; j < machineTimes[machine].Count; j++) {
                    isUnique &= !object.Equals(machineTimes[machine][i], machineTimes[machine][j]);
                  }
                }

                if (!hasDefault) {
                  machinesWithNoDefaultReasons.Add(machine.Display);
                }

                if (!isUnique) {
                  machinesWithSeveralDefaultReasons.Add(machine.Display);
                }
              }
              
              if (machinesWithNoDefaultReasons.Count > 0) {
                errors.Add("all machines must have a default reason in any context (" +
                           machinesWithNoDefaultReasons.Count + " machine" +
                           (machinesWithNoDefaultReasons.Count > 1 ? "s" : "") + " impacted)");
              }

              if (machinesWithSeveralDefaultReasons.Count > 0) {
                errors.Add("a machine cannot have more than one default reason in a specific context (" +
                           machinesWithSeveralDefaultReasons.Count + " machine" +
                           (machinesWithSeveralDefaultReasons.Count > 1 ? "s" : "") + " impacted)");
              }
            }
          }
        }
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
      return "PageSelectedReason";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      IDictionary<IReason, DefaultReasonParameters> parameteredReasons =
        data.Get<IDictionary<IReason, DefaultReasonParameters>>(Item.DEFAULT_REASONS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IReason reason in parameteredReasons.Keys) {
            ModelDAOHelper.DAOFactory.ReasonDAO.Lock(reason);
            DefaultReasonParameters param = parameteredReasons[reason];
            
            string text = reason.Display;
            
            if (param.MaxTime.HasValue) {
              text += "\nmaximum time " + param.MaxTime.Value;
            }
            else {
              text += "\nno maximum time";
            }

            if (param.OverwriteRequired) {
              text += "\noverwrite required";
            }
            else {
              text += "\noverwrite not required";
            }

            if (param.MachineFilterInclude == null) {
              text += "\nno machine filter for inclusion";
            }
            else {
              text += "\nmachine filter \"" + param.MachineFilterInclude.Name + "\" for inclusion";
            }

            if (param.MachineFilterExclude == null) {
              text += "\nno machine filter for exclusion";
            }
            else {
              text += "\nmachine filter \"" + param.MachineFilterExclude.Name + "\" for exclusion";
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
