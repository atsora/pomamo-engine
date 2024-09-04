// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of OperationData.
  /// </summary>
  public class OperationData
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Displayed string of an operation
    /// </summary>
    public string Display { get; private set; }
    
    /// <summary>
    /// Operation
    /// </summary>
    public IOperation Operation { get; private set; }
    
    public IList<MachineData> Machines { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="line"></param>
    /// <param name="operation"></param>
    public OperationData(ILine line, IOperation operation)
    {
      Machines = new List<MachineData>();
      Operation = operation;
      UpdateDisplay();
      LoadMachines(line);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Update the display value of a simple operation
    /// </summary>
    public void UpdateDisplay()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(Operation);
          Display = Operation.Display;
        }
      }
    }
    
    void LoadMachines(ILine line)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(Operation);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          
          IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAllByLineOperation(line, Operation);
          foreach (ILineMachine lineMachine in lineMachines) {
            bool present = false;
            foreach (MachineData machine in Machines) {
              if (Object.Equals(machine.Machine, lineMachine.Machine)) {
                present = true;
                break;
              }
            }
            if (!present) {
              Machines.Add(new MachineData(lineMachine));
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Remove a machine
    /// </summary>
    /// <param name="machine"></param>
    public void RemoveMachine(IMachine machine)
    {
      foreach (MachineData machineData in Machines) {
        if (Object.Equals(machineData.Machine, machine)) {
          Machines.Remove(machineData);
          return;
        }
      }
    }
    
    /// <summary>
    /// Add a machine. Return true if duplicate
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool AddMachine(IMachine machine)
    {
      foreach (MachineData machineData in Machines) {
        if (Object.Equals(machineData.Machine, machine)) {
          return true;
        }
      }

      Machines.Add(new MachineData(machine));
      return false;
    }
    
    /// <summary>
    /// Change the machine status
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="isDedicated"></param>
    public void SetMachineDedicated(IMachine machine, bool isDedicated)
    {
      foreach (MachineData machineData in Machines) {
        if (Object.Equals(machineData.Machine, machine)) {
          machineData.Dedicated = isDedicated;
          return;
        }
      }
    }
    
    /// <summary>
    /// Apply the modifications in the database
    /// This method is already within a transaction
    /// </summary>
    /// <param name="line"></param>
    public void ApplyModifications(ILine line)
    {
      if (line != null && Operation != null)
      {
        ModelDAOHelper.DAOFactory.OperationDAO.Lock(Operation);
        
        // Existing lineMachines in the database
        IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAllByLineOperation(line, Operation);
        
        foreach (MachineData machineData in Machines) {
          bool found = false;
          
          foreach (ILineMachine lineMachine in lineMachines) {
            if (Object.Equals(lineMachine.Machine, machineData.Machine))
            {
              // Edit existing lineMachine
              if (machineData.Dedicated) {
                lineMachine.LineMachineStatus = LineMachineStatus.Dedicated;
              }
              else {
                lineMachine.LineMachineStatus = LineMachineStatus.Extra;
              }

              found = true;
              break;
            }
          }
          
          if (!found) {
            // Create a new LineMachine
            IMachine machine = machineData.Machine;
            ILineMachine lm = ModelDAOHelper.ModelFactory.CreateLineMachine(line, machine, Operation);
            if (machineData.Dedicated) {
              lm.LineMachineStatus = LineMachineStatus.Dedicated;
            }
            else {
              lm.LineMachineStatus = LineMachineStatus.Extra;
            }

            ModelDAOHelper.DAOFactory.LineMachineDAO.MakePersistent(lm);
          }
        }
        
        foreach (ILineMachine lineMachine in lineMachines) {
          bool found = false;
          foreach (MachineData machineData in Machines) {
            if (Object.Equals(machineData.Machine, lineMachine.Machine)) {
              found = true;
              break;
            }
          }
          
          // Remove lineMachine if not found
          if (!found) {
            ModelDAOHelper.DAOFactory.LineMachineDAO.MakeTransient(lineMachine);
          }
        }
      }
    }
    
    /// <summary>
    /// Text to display in logs
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text = "Operation = " + Operation + " (" + Display + ")";
      foreach (MachineData machine in Machines) {
        text += "\n    " + machine.ToString();
      }

      return text;
    }
    #endregion // Methods
  }
}
