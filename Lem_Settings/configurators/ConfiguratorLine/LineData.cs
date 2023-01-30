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
  /// Description of LineData.
  /// </summary>
  public class LineData
  {
    #region Members
    IList<OperationData> m_operations = new List<OperationData>();
    bool m_modification = false;
    ILine m_currentLine = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (LineData).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LineData() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get all operations for a specific line
    /// </summary>
    /// <returns></returns>
    public IList<OperationData> GetOperations()
    {
      return m_operations;
    }
    
    /// <summary>
    /// Get all machines for a specific line and operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<MachineData> GetMachines(IOperation operation)
    {
      foreach (OperationData op in m_operations) {
        if (Object.Equals(op.Operation, operation)) {
          return op.Machines;
        }
      }

      return new List<MachineData>();
    }
    
    /// <summary>
    /// Remove a machine
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="machine"></param>
    public void RemoveMachine(IOperation operation, IMachine machine)
    {
      foreach (OperationData op in m_operations) {
        if (Object.Equals(op.Operation, operation)) {
          op.RemoveMachine(machine);
          m_modification = true;
          break;
        }
      }
    }
    
    /// <summary>
    /// Add a machine. Return true if duplicate
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool AddMachine(IOperation operation, IMachine machine)
    {
      foreach (OperationData op in m_operations) {
        if (Object.Equals(op.Operation, operation)) {
          m_modification = true;
          return op.AddMachine(machine);
        }
      }
      return false;
    }
    
    /// <summary>
    /// Change the machine status
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="machine"></param>
    /// <param name="isDedicated"></param>
    public void SetMachineDedicated(IOperation operation, IMachine machine, bool isDedicated)
    {
      foreach (OperationData op in m_operations) {
        if (Object.Equals(op.Operation, operation)) {
          op.SetMachineDedicated(machine, isDedicated);
          m_modification = true;
          break;
        }
      }
    }
    
    /// <summary>
    /// Load a line
    /// "null" reset it
    /// </summary>
    /// <param name="line"></param>
    public void LoadLine(ILine line)
    {
      if (!Object.Equals(m_currentLine, line))
      {
        // Set the line
        m_currentLine = line;
        
        // Load operations
        m_operations.Clear();
        if (m_currentLine != null) {
          IList<IOperation> operations = new List<IOperation>();
          
          // Load operations
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
              ModelDAOHelper.DAOFactory.LineDAO.Lock(m_currentLine);
              if (m_currentLine.Components != null) {
                ICollection<IComponent> components = m_currentLine.Components;
                IList<IComponentIntermediateWorkPiece> iwps = new List<IComponentIntermediateWorkPiece>();
                foreach (IComponent component in components) {
                  ModelDAOHelper.DAOFactory.ComponentDAO.Lock(component);
                  if (component.ComponentIntermediateWorkPieces != null) {
                    foreach (IComponentIntermediateWorkPiece iwp in component.ComponentIntermediateWorkPieces) {
                      iwps.Add(iwp);
                    }
                  }
                }
                foreach (IComponentIntermediateWorkPiece iwp in iwps) {
                  if (iwp.IntermediateWorkPiece != null && iwp.IntermediateWorkPiece.Operation != null) {
                    if (!operations.Contains(iwp.IntermediateWorkPiece.Operation)) {
                      operations.Add(iwp.IntermediateWorkPiece.Operation);
                    }
                  }
                }
              }
            }
          }
          
          m_modification = false;
          foreach (IOperation op in operations) {
            m_operations.Add(new OperationData(m_currentLine, op));
          }
        }
      }
    }
    
    void UpdateDisplay()
    {
      foreach (OperationData op in m_operations) {
        op.UpdateDisplay();
      }
    }
    
    /// <summary>
    /// List of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation()
    {
      IList<string> errors = new List<string>();
      
      // Load machines of all operations not used in the current line
      IList<IMachine> alreadyDedicatedMachines = new List<IMachine>();
      IList<IMachine> alreadyNonDedicatedMachines = new List<IMachine>();
      IList<string> alreadyDedicated = new List<string>();
      IList<string> alreadyUsed = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAll();
          foreach (ILineMachine lineMachine in lineMachines) {
            if (!Object.Equals(lineMachine.Line, m_currentLine)) {
              if (lineMachine.LineMachineStatus == LineMachineStatus.Dedicated &&
                  !alreadyDedicatedMachines.Contains(lineMachine.Machine)) {
                alreadyDedicatedMachines.Add(lineMachine.Machine);
              }
              else if (lineMachine.LineMachineStatus == LineMachineStatus.Extra &&
                       !alreadyNonDedicatedMachines.Contains(lineMachine.Machine)) {
                alreadyNonDedicatedMachines.Add(lineMachine.Machine);
              }
            }
          }
          
          // A machine already dedicated cannot be used anymore
          // A machine already used cannot be dedicated
          foreach (OperationData opData in m_operations) {
            foreach (MachineData machineData in opData.Machines) {
              // Check if the machine can be used
              if (alreadyDedicatedMachines.Contains(machineData.Machine)) {
                alreadyDedicated.Add(machineData.Machine.Display);
              }
              else if (machineData.Dedicated && alreadyNonDedicatedMachines.Contains(machineData.Machine)) {
                alreadyUsed.Add(machineData.Machine.Display);
              }

              // Add the machine to the list
              if (machineData.Dedicated) {
                alreadyDedicatedMachines.Add(machineData.Machine);
              }
              else {
                alreadyNonDedicatedMachines.Add(machineData.Machine);
              }
            }
          }
        }
      }
      
      if (alreadyDedicated.Count > 0) {
        errors.Add("a machine already dedicated to an operation cannot be used for another operation (\"" +
                   string.Join("\", \"", alreadyDedicated.ToArray()) + "\")");
      }
      
      if (alreadyUsed.Count > 0) {
        errors.Add("a machine already used by an operation cannot be dedicated to a new operation (\"" +
                   string.Join("\", \"", alreadyUsed.ToArray()) + "\")");
      }
      
      return errors;
    }
    
    /// <summary>
    /// Validate the modifications
    /// </summary>
    public void Validate()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          if (m_modification) {
            ModelDAOHelper.DAOFactory.LineDAO.Lock(m_currentLine);
          }

          foreach (OperationData op in m_operations) {
            op.ApplyModifications(m_currentLine);
          }

          transaction.Commit();
        }
      }
    }
    
    /// <summary>
    /// Text to display in logs
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      IList<string> textParts = new List<string>();
      if (m_modification) {
        string textPart = "Line = " + m_currentLine + " (";
        foreach (OperationData op in m_operations) {
          textPart += "\n  " + op.ToString();
        }

        textParts.Add(textPart);
      }
      
      return string.Join("\n", textParts.ToArray());
    }
    #endregion // Methods
  }
}
