// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of StructPart.
  /// </summary>
  public class StructPart
  {
    static readonly ILog log = LogManager.GetLogger(typeof (StructPart).FullName);

    #region Getters / Setters
    /// <summary>
    /// True if the part already exists
    /// </summary>
    public bool Existing { get; private set; }
    
    /// <summary>
    /// Code of the part
    /// </summary>
    public string PartCode { get; set; }
    
    /// <summary>
    /// Name of the part
    /// </summary>
    public string PartName { get; set; }
    
    /// <summary>
    /// Part
    /// </summary>
    public IPart Part { get; private set; }
    
    /// <summary>
    /// List of operations
    /// </summary>
    public IList<StructOperation> Operations { get; private set; }
    
    /// <summary>
    /// First operation, "null" if none
    /// </summary>
    public IOperation BaseOperation { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Existing part
    /// </summary>
    public StructPart(IPart part)
    {
      Existing = true;
      Part = part;
      BaseOperation = null;
      Operations = new List<StructOperation>();
      
      // Load information
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.PartDAO.Lock(Part);
          PartName = part.Name;
          PartCode = part.Code;
          
          ICollection<IComponentIntermediateWorkPiece> ciwps = part.ComponentIntermediateWorkPieces;
          
          foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
            IOperation operation = ciwp.IntermediateWorkPiece.Operation;
            StructOperation structOperation = new StructOperation(operation);
            structOperation.m_part = Part;
            Operations.Add(structOperation);
          }
        }
      }
    }
    
    /// <summary>
    /// New part
    /// </summary>
    public StructPart(string name, string code)
    {
      Existing = false;
      PartName = name;
      PartCode = code;
      BaseOperation = null;
      
      // Create a part (not saved yet!)
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // (1 is undefined)
          Part = ModelDAOHelper.ModelFactory.CreatePart(ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindById(1));
        }
      }
      
      Operations = new List<StructOperation>();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Textual description
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text;
      
      if (Existing) {
        text = "(existing part)";
      }
      else {
        text = "(new part)";
      }

      text += " name: \"" + PartName + "\"";
      if (PartCode != "") {
        text += " code: \"" + PartCode + "\"";
      }

      return text;
    }

    /// <summary>
    /// Retrieve ordre of the element
    /// </summary>
    /// <returns></returns>
    public string ToOrder()
    {
      if (Existing) {
        return "a" + this.ToString();
      }
      else {
        return "b" + this.ToString();
      }
    }
    #endregion // Methods
  }
}
