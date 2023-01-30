// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Operation.
  /// </summary>
  public class StructOperation
  {
    #region Members
    public int m_maxPartsPerCycle = 1;
    public IList<StructMachine> m_machines = new List<StructMachine>();
    public IPart m_part = null;
    #endregion // Members
    
    #region Getters / setters
    /// <summary>
    /// True if the operation already exists
    /// </summary>
    public bool Existing { get; private set; }
    
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Operation
    /// </summary>
    public IOperation Operation { get; private set; }
    #endregion // Getters / setters

    #region Constructors
    /// <summary>
    /// Existing operation
    /// </summary>
    public StructOperation(IOperation operation)
    {
      Existing = true;
      Name = operation.Name;
      Code = operation.Code;
      Operation = operation;
    }
    
    /// <summary>
    /// New operation
    /// </summary>
    public StructOperation(string name, string code)
    {
      Existing = false;
      Name = name;
      Code = code;
      
      // Create a new operation (not saved yet!)
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          Operation = ModelDAOHelper.ModelFactory.CreateOperation(
            ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById(1)); // (1 is undefined)
        }
      }
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Textual description of the data
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text = String.Format(
        "\nName = \"{0}\", code = \"{1}\"\n" +
        "Max parts per cycle = {2}\n" +
        "Part = {3}, {4} operation = {5}",
        Name, Code, m_maxPartsPerCycle, m_part,
        Existing ? "Existing" : "Non existing",
        Operation);
      foreach (StructMachine structMachine in m_machines) {
        text += "\n" + structMachine;
      }

      return text;
    }

    /// <summary>
    /// Return true if the operation already contains a specific machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool ContainsMachine(IMachine machine)
    {
      foreach (StructMachine structMachine in m_machines) {
        if (Object.Equals(structMachine.m_machine, machine)) {
          return true;
        }
      }

      return false;
    }
    #endregion // Methods
  }
}
