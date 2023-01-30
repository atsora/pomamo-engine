// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Observer interface of the oberserver pattern
  /// 
  /// Once registered, this class can be notified of an update 
  /// in a Config control.
  /// </summary>
  public interface IConfigControlObserver<EntityType>
  {
    /// <summary>
    /// Update this class after an item or a list of items
    /// where deleted in the observed config control.
    /// </summary>
    /// <param name="deletedEntities"></param>
    void UpdateAfterDelete (ICollection<EntityType> deletedEntities);
    
    /// <summary>
    /// Update this class after an item or a list of items
    /// where updated or inserted in the observed config control.
    /// </summary>
    /// <param name="updatedEntities"></param>
    void UpdateAfterUpdate (ICollection<EntityType> updatedEntities);
  }
}
