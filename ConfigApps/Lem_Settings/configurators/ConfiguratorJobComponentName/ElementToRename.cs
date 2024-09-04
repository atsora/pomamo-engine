// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorJobComponentName
{
  /// <summary>
  /// Description of ElementToRename.
  /// </summary>
  public class ElementToRename
  {
    #region Members
    readonly IProject m_project;
    readonly IComponent m_component;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ElementToRename).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// Component name to be changed
    /// </summary>
    public string ComponentName { get; private set; }
    
    /// <summary>
    /// Job name to be changed
    /// </summary>
    public string JobName { get; private set; }
    
    /// <summary>
    /// Date of the project
    /// </summary>
    public DateTime Date { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor of an ElementToRename
    /// Within a transaction
    /// </summary>
    /// <param name="project"></param>
    /// <param name="component"></param>
    public ElementToRename(IProject project, IComponent component)
    {
      m_project = project;
      m_component = component;
      
      // Fill the attributes
      ComponentName = component.Name;
      JobName = project.Name;
      Date = project.CreationDateTime;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Configure new names
    /// Must be called within a transaction
    /// </summary>
    /// <param name="jobName"></param>
    /// <param name="componentName"></param>
    public void SetNames(string jobName, string componentName)
    {
      ModelDAOHelper.DAOFactory.ProjectDAO.Lock(m_project);
      ModelDAOHelper.DAOFactory.ComponentDAO.Lock(m_component);
      
      // Change the name of the associated workorders
      if (m_project.WorkOrders != null) {
        foreach (var wo in m_project.WorkOrders) {
          if (wo.Name == m_project.Name) {
            wo.Name = jobName;
            ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent(wo);
          }
        }
      }
      
      m_project.Name = jobName;
      m_component.Name = componentName;
      ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent(m_project);
      ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent(m_component);
      
      JobName = jobName;
      ComponentName = componentName;
    }
    
    /// <summary>
    /// IsoFiles involved in the component
    /// Component -> IntermediateWorkPiece -> Operation -> Sequence -> Stamp -> IsoFile
    /// </summary>
    /// <returns></returns>
    public IDictionary<DateTime, String> GetIsoFiles()
    {
      var isofiles = new Dictionary<DateTime, string>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.ComponentDAO.Lock(m_component);
          
          if (m_component.ComponentIntermediateWorkPieces != null) {
            foreach (var ciwp in m_component.ComponentIntermediateWorkPieces) {
              if (ciwp != null && ciwp.IntermediateWorkPiece != null &&
                  ciwp.IntermediateWorkPiece.Operation != null) {
                var operation = ciwp.IntermediateWorkPiece.Operation;
                foreach (var sequence in operation.Sequences) {
                  if (sequence != null) {
                    foreach (var stamp in sequence.Stamps) {
                      if (stamp != null && stamp.IsoFile != null) {
                        isofiles[stamp.IsoFile.StampingDateTime] = stamp.IsoFile.Name;
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      
      return isofiles;
    }
    #endregion // Methods
  }
}
