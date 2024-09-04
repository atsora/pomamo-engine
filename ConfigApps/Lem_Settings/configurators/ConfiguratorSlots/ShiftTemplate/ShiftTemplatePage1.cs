// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;

namespace ConfiguratorSlots.ShiftTemplate
{
  /// <summary>
  /// Description of ShiftTemplatePage1.
  /// </summary>
  internal class ShiftTemplatePage1 : IPartPage1
  {
    #region Getters / Setters
    /// <summary>
    /// Part of the help describing the content of the timeline
    /// </summary>
    public string HelpSub {
      get { return "This page comprises a timeline representing " +
          "the evolution of the shift template according to the time."; }
    }
    
    /// <summary>
    /// Return the name of the category of elements (can be "machines" for example)
    /// It must be in plural
    /// If null or empty, it's considered that only one element will be displayed in the 
    /// graphics, no filters will be necessary (no page 2)
    /// </summary>
    public string ElementsName { get { return null; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftTemplatePage1() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a timeline in the timelines, knowing the item
    /// This method is within a session
    /// </summary>
    /// <param name="timelines"></param>
    /// <param name="item"></param>
    public void AddTimeLine(TimelinesWidget timelines, object item)
    {
      timelines.AddTimeline("", new ShiftTemplateBarData());
    }
    #endregion // Methods
  }
}
