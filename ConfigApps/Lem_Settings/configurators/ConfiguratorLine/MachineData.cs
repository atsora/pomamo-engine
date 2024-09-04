// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of MachineData.
  /// </summary>
  public class MachineData
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Displayed string for a machine
    /// </summary>
    public string Display { get; private set; }
    
    /// <summary>
    /// True if the machine is dedicated
    /// </summary>
    public bool Dedicated { get; set; }
    
    /// <summary>
    /// Machine
    /// </summary>
    public IMachine Machine { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// This constructor will be used in a DAO session
    /// </summary>
    /// <param name="lineMachine"></param>
    public MachineData(ILineMachine lineMachine)
    {
      Display = lineMachine.Machine.Display;
      Dedicated = (lineMachine.LineMachineStatus == LineMachineStatus.Dedicated);
      Machine = lineMachine.Machine;
    }
    
    /// <summary>
    /// Alternative constructor (new machine added)
    /// </summary>
    /// <param name="machine"></param>
    public MachineData(IMachine machine)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
          if (machine != null) {
            Display = machine.Display;
            Dedicated = true;
            Machine = machine;
          } else {
            Display = "-";
            Dedicated = false;
            Machine = null;
          }
        }
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Text to show in log
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("Machine = {0}, dedicated={1}]", Machine, Dedicated);
    }
    #endregion // Methods
  }
}
