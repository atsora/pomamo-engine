// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserMachine
{
  /// <summary>
  /// Description of UserMachinePage1.
  /// </summary>
  public class UserMachinePage1 : IPartPage1
  {
    #region Getters / Setters
    /// <summary>
    /// Part of the help describing the content of the timeline
    /// </summary>
    public string HelpSub {
      get { return "This page comprises a timeline per user, representing its machine attendance. " +
          "The list of users displayed can be restricted by clicking on \"Change users\"."; }
    }
    
    /// <summary>
    /// Return the name of the category of elements (can be "machines" for example)
    /// It must be in plural
    /// If null or empty, it's considered that only one element will be displayed in the 
    /// graphics, no filters will be necessary (no page 2)
    /// </summary>
    public string ElementsName { get { return "users"; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserMachinePage1() {}
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
      IUser user = item as IUser;
      ModelDAOHelper.DAOFactory.UserDAO.Lock(user);
      if (user != null) {
        timelines.AddTimeline(user.Display, new UserMachineBarData(user));
      }
    }
    #endregion // Methods
  }
}
