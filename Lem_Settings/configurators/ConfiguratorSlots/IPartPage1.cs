// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.BaseControls;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of IPartPage1.
  /// </summary>
  public interface IPartPage1
  {
    /// <summary>
    /// Part of the help describing the content of the timeline
    /// </summary>
    string HelpSub { get; }
    
    /// <summary>
    /// Return the name of the category of elements (can be "machines" for example)
    /// It must be in plural
    /// If null or empty, it's considered that only one element will be displayed in the 
    /// graphics, no filters will be necessary (no page 2)
    /// </summary>
    string ElementsName { get; }
    
    /// <summary>
    /// Add a timeline in the timelines, knowing the item
    /// This method is within a session
    /// </summary>
    /// <param name="timelines"></param>
    /// <param name="item"></param>
    void AddTimeLine(TimelinesWidget timelines, object item);
  }
}
