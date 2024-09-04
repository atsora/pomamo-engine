// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Machine.
  /// </summary>
  public class StructMachine
  {
    #region Members
    public string m_name = "";
    public int? m_bestPerformance = null;
    public int m_bestPerformanceUnit = 0;
    public int? m_expectedPerformance = null;
    public int m_expectedPerformanceUnit = 0;
    public bool m_dedicated = true;
    public IMachine m_machine = null;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="name"></param>
    public StructMachine(IMachine machine, string name)
    {
      m_machine = machine;
      m_name = name;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Textual description of the data
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text = "   Machine ";
      if (!m_dedicated) {
        text += "not ";
      }

      text += "dedicated, " + m_machine + " (" + m_name + ")";
      
      return text;
    }

    #endregion
  }
}
