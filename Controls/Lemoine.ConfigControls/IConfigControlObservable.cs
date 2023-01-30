// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Observable interface of the Observer pattern
  /// </summary>
  internal interface IConfigControlObservable<EntityType>
  {
    /// <summary>
    /// Add an observer to a this control
    /// </summary>
    /// <param name="observer"></param>
    void AddObserver (IConfigControlObserver<EntityType> observer);
    
    /// <summary>
    /// Remove an observer from this control
    /// </summary>
    /// <param name="observer"></param>
    void RemoveObserver (IConfigControlObserver<EntityType> observer);
  }
}
