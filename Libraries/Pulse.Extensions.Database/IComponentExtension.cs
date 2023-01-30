// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// 
  /// </summary>
  public interface IComponentExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Additional actions to take when two components are merged
    /// </summary>
    /// <param name="oldComponent">not null</param>
    /// <param name="newComponent">not null</param>
    void Merge (IComponent oldComponent,
                IComponent newComponent);
  }
}
