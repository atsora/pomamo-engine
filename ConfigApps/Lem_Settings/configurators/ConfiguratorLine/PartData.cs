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
  /// Description of PartData.
  /// </summary>
  public class PartData
  {
    #region Members
    readonly IDictionary<IPart, string> m_partCodes = new Dictionary<IPart, string>();
    readonly IDictionary<IPart, string> m_partNames = new Dictionary<IPart, string>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PartData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Number of parts for the line
    /// </summary>
    public int Count { get { return m_partCodes.Count; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PartData() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Load part names and codes for a line
    /// </summary>
    /// <param name="line"></param>
    public void Load(ILine line)
    {
      m_partCodes.Clear();
      m_partNames.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          foreach (IComponent component in line.Components) {
            m_partCodes[component.Part] = component.Code;
            m_partNames[component.Part] = component.Name;
          }
        }
      }
    }
    
    /// <summary>
    /// Get a code
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetCode(int index)
    {
      return m_partCodes.Values.ToList()[index];
    }
    
    /// <summary>
    /// Get a name
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetName(int index)
    {
      return m_partNames.Values.ToList()[index];
    }
    
    /// <summary>
    /// Set a code
    /// </summary>
    /// <param name="index"></param>
    /// <param name="code"></param>
    public void SetCode(int index, string code)
    {
      IPart key = m_partCodes.Keys.ToList()[index];
      m_partCodes[key] = code;
    }
    
    /// <summary>
    /// Set a name
    /// </summary>
    /// <param name="index"></param>
    /// <param name="name"></param>
    public void SetName(int index, string name)
    {
      IPart key = m_partNames.Keys.ToList()[index];
      m_partNames[key] = name;
    }
    
    /// <summary>
    /// Save part names and codes
    /// </summary>
    public void Save()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          foreach (IPart part in m_partCodes.Keys) {
            ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
            part.Name = m_partNames[part];
            part.Code = m_partCodes[part];
          }
          transaction.Commit();
        }
      }
    }
    
    /// <summary>
    /// Check errors in the configuration
    /// </summary>
    /// <param name="errors"></param>
    public void GetErrors(ref IList<string> errors)
    {
      // Code already taken by another component
      IList<string> codes = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IComponent> components = ModelDAOHelper.DAOFactory.ComponentDAO.FindAll();
        foreach (IComponent component in components) {
          if (!m_partCodes.Keys.Contains(component.Part) && component.Code != null && component.Code != "") {
            codes.Add(component.Code);
          }
        }
      }
      
      foreach (IPart key in m_partCodes.Keys) {
        if (codes.Contains(m_partCodes[key])) {
          errors.Add("the code \"" + m_partCodes[key] + "\" is already taken by another part");
          break;
        } else {
          codes.Add(m_partCodes[key]);
        }
      }
      
      // Code name cannot be empty
      foreach (string name in m_partNames.Values) {
        if (name == "") {
          errors.Add("the code name cannot be empty");
          break;
        }
      }
      
      // Two part names cannot have the same value within a line
      IList<string> names = new List<string>();
      foreach (string name in m_partNames.Values) {
        if (names.Contains(name)) {
          errors.Add("parts must have different names within a line");
          break;
        } else {
          names.Add(name);
        }
      }
    }
    
    /// <summary>
    /// Textual description
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text = "";
      
      foreach (IPart key in m_partCodes.Keys) {
        if (text != "") {
          text += "\n";
        }

        text += String.Format("part = {0}, name = {1}, code = {2}", key, m_partNames[key], m_partCodes[key]);
      }
      
      return text;
    }
    #endregion // Methods
  }
}
