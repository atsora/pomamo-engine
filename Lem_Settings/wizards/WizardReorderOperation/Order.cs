// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Linq;
using Lemoine.Core.Log;

namespace WizardReorderOperation
{
  /// <summary>
  /// Description of Order.
  /// </summary>
  public class Order
  {
    #region Members
    IPart m_part = null;
    bool m_noOrder = false;
    bool m_notLinear = false;
    int m_operationNumber = 0;
    bool m_valid = true;
    bool m_potentialConflict = false;
    bool m_onlySimpleOperation = true;
    readonly IList<IOperation> m_operations = new List<IOperation>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Order).FullName);

    #region Getters / Setters
    /// <summary>
    /// Retrieve all operations, ordered
    /// </summary>
    public IList<IOperation> Operations { get { return m_operations; } }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Order() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Load operations of a part
    /// </summary>
    /// <param name="part"></param>
    public void Load(IPart part)
    {
      if (!object.Equals(m_part, part)) {
        // Variable initialization
        m_noOrder = false;
        m_notLinear = false;
        m_part = part;
        m_operationNumber = 0;
        m_valid = true;
        m_potentialConflict = false;
        m_onlySimpleOperation = true;
        
        // Retrieve all operations and check validity
        int maxPossibleNextOperations = 0;
        IDictionary<int, IList<IOperation>> dicOperations = new Dictionary<int, IList<IOperation>>();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            // The part is locked
            ModelDAOHelper.DAOFactory.PartDAO.Lock(m_part);
            foreach (IComponentIntermediateWorkPiece ciwp in m_part.ComponentIntermediateWorkPieces) {
              ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.Lock(ciwp);
              if (ciwp.IntermediateWorkPiece != null) {
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(ciwp.IntermediateWorkPiece);
              }
            }
            
            m_operationNumber = m_part.ComponentIntermediateWorkPieces.Count;
            foreach (IComponentIntermediateWorkPiece ciwp in m_part.ComponentIntermediateWorkPieces) {
              IIntermediateWorkPiece iwp = ciwp.IntermediateWorkPiece;
              if (iwp != null) {
                if (iwp.PossibleNextOperations != null && iwp.PossibleNextOperations.Count > 0) {
                  int maxPossibleNextOperationsTmp = 0;
                  
                  // Count the number of next operations within the same part
                  foreach (IOperation op in iwp.PossibleNextOperations) {
                    foreach (IComponentIntermediateWorkPiece ciwpTmp in m_part.ComponentIntermediateWorkPieces) {
                      if (ciwpTmp.IntermediateWorkPiece != null && ciwpTmp.IntermediateWorkPiece.Operation != null &&
                          object.Equals(ciwpTmp.IntermediateWorkPiece.Operation, op)) {
                        maxPossibleNextOperationsTmp++;
                        break;
                      }
                    }
                  }
                  if (iwp.Operation != null && iwp.Operation.IntermediateWorkPieces != null) {
                    m_onlySimpleOperation |= (iwp.Operation.IntermediateWorkPieces.Count > 1);
                  }

                  m_potentialConflict |= (maxPossibleNextOperationsTmp < iwp.PossibleNextOperations.Count);
                  if (maxPossibleNextOperationsTmp > maxPossibleNextOperations) {
                    maxPossibleNextOperations = maxPossibleNextOperationsTmp;
                  }
                }
                
                // Store the operation
                if (iwp.Operation != null) {
                  int order = -1;
                  if (ciwp.Order.HasValue) {
                    order = ciwp.Order.Value;
                  }

                  if (!dicOperations.ContainsKey(order)) {
                    dicOperations[order] = new List<IOperation>();
                  }

                  dicOperations[order].Add(iwp.Operation);
                }
              } else {
                m_valid = false;
              }
            }
          }
        }
        
        m_noOrder = (maxPossibleNextOperations == 0);
        m_notLinear |= (maxPossibleNextOperations > 1);
        
        m_operations.Clear();
        List<int> orders = dicOperations.Keys.ToList();
        orders.Sort();
        foreach (int order in orders) {
          foreach (IOperation op in dicOperations[order]) {
            m_operations.Add(op);
          }
        }
      }
    }
    
    /// <summary>
    /// Get the errors after having loaded the operations
    /// </summary>
    /// <returns></returns>
    public void GetErrors(ref IList<string> errors)
    {
      if (!m_valid) {
        errors.Add("the part is not valid");
      }
      else if (m_operationNumber < 2) {
        errors.Add("the part must comprise at least 2 operations");
      }
    }
    
    /// <summary>
    /// Get the warnings after having loaded the operations
    /// </summary>
    /// <returns></returns>
    public void GetWarnings(ref IList<string> warnings)
    {
      // No order specified yet
      if (!m_noOrder) {
        warnings.Add("The operation order of the part was already specified.");
      }

      // Non linear order
      if (m_notLinear || !m_onlySimpleOperation) {
        warnings.Add("The process is not linear, you will not be able to reproduce it with this wizard.");
      }

      if (m_potentialConflict) {
        warnings.Add("Changing the operation order of this part may break the operation order of another part.");
      }
    }
    
    /// <summary>
    /// Move an item toward the top
    /// </summary>
    /// <param name="index"></param>
    public void Up(int index) {
      if (index > 0 && index < m_operations.Count) {
        IOperation opTmp = m_operations[index];
        m_operations[index] = m_operations[index - 1];
        m_operations[index - 1] = opTmp;
      }
    }
    
    /// <summary>
    /// Move an item toward the bottom
    /// </summary>
    /// <param name="index"></param>
    public void Down(int index) {
      if (index >= 0 && index < m_operations.Count - 1) {
        IOperation opTmp = m_operations[index];
        m_operations[index] = m_operations[index + 1];
        m_operations[index + 1] = opTmp;
      }
    }
    
    /// <summary>
    /// Get the summary associated to the order of the different operations
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary()
    {
      IList<string> summary = new List<string>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          int order = 0;
          foreach (IOperation op in m_operations) {
            ModelDAOHelper.DAOFactory.OperationDAO.Lock(op);
            summary.Add(String.Format("Order {0}: operation \"{1}\"", order++, op.Display));
          }
        }
      }
      
      return summary;
    }
    
    /// <summary>
    /// Save the modifications in the database
    /// </summary>
    public void Save()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // The part is locked
          ModelDAOHelper.DAOFactory.PartDAO.Lock(m_part);
          foreach (IComponentIntermediateWorkPiece ciwp in m_part.ComponentIntermediateWorkPieces) {
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.Lock(ciwp);
          }
          
          // Lock all operations
          foreach (IOperation op in m_operations) {
            ModelDAOHelper.DAOFactory.OperationDAO.Lock(op);
            foreach (IIntermediateWorkPiece iwp in op.IntermediateWorkPieces) {
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(iwp);
            }
          }
          
          // For each operation
          for (int i = 0; i < m_operations.Count; i++) {
            IOperation operation = m_operations[i];
            
            // For each iwp
            if (operation.IntermediateWorkPieces != null) {
              foreach (IIntermediateWorkPiece iwp in operation.IntermediateWorkPieces)
              {
                // Clear next operations (belonging to the part)
                ICollection<IOperation> nextOps = new List<IOperation>(iwp.PossibleNextOperations);
                if (nextOps != null) {
                  foreach (IOperation nextOp in nextOps) {
                    foreach (IOperation opTmp in m_operations) {
                      if (opTmp == nextOp) {
                        iwp.RemovePossibleNextOperation(nextOp);
                        break;
                      }
                    }
                  }
                }
                
                // Add next operation
                if (i < m_operations.Count - 1) {
                  iwp.AddPossibleNextOperation(m_operations[i + 1]);
                }

                // Specify order
                foreach (IComponentIntermediateWorkPiece ciwp in m_part.ComponentIntermediateWorkPieces) {
                  if (Object.Equals(ciwp.IntermediateWorkPiece, iwp)) {
                    ciwp.Order = i;
                  }
                }
              }
            }
          }
          
          // Specify the new final iwp
          IOperation lastOperation = m_operations.Last();
          if (lastOperation.IntermediateWorkPieces == null ||
              lastOperation.IntermediateWorkPieces.Count == 0) {
            m_part.FinalWorkPiece = null;
          }

          if (lastOperation.IntermediateWorkPieces.Count == 1) {
            m_part.FinalWorkPiece = lastOperation.IntermediateWorkPieces.First();
          }
          else {
            // Iwps of the part
            var partIwps = new List<IIntermediateWorkPiece>();
            foreach (var ciwp in m_part.ComponentIntermediateWorkPieces) {
              partIwps.Add(ciwp.IntermediateWorkPiece);
            }

            IIntermediateWorkPiece finalIwp = null;
            foreach (var iwp in lastOperation.IntermediateWorkPieces) {
              if (partIwps.Contains(iwp)) {
                finalIwp = iwp;
                break;
              }
            }
            
            m_part.FinalWorkPiece = finalIwp;
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Methods
  }
}
