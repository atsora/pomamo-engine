// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.MachineState
{
  /// <summary>
  /// Description of MachineStatePage1.
  /// </summary>
  internal class MachineStatePage1 : IPartPage1
  {
    #region Getters / Setters
    /// <summary>
    /// Part of the help describing the content of the timeline
    /// </summary>
    public string HelpSub {
      get { return "This page comprises a timeline per machine, representing " +
          "the evolution of the state according to the time. " +
          "The list of machines displayed can be restricted by clicking on \"Change machines\"."; }
    }
    
    /// <summary>
    /// Return the name of the category of elements (can be "machines" for example)
    /// It must be in plural
    /// If null or empty, it's considered that only one element will be displayed in the 
    /// graphics, no filters will be necessary (no page 2)
    /// </summary>
    public string ElementsName { get { return "machines"; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStatePage1() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a timeline in the timelines, knowing the item id
    /// This method is within a session
    /// </summary>
    /// <param name="timelines"></param>
    /// <param name="item"></param>
    public void AddTimeLine(TimelinesWidget timelines, object item)
    {
      var machine = item as IMachine;
      ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
      if (machine != null) {
        timelines.AddTimeline(machine.Display, new MachineStateBarData(machine));
      }
    }
    #endregion // Methods
  }
}
