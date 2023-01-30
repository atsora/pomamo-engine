// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// A line is made of several machines which are linked to an operation
  /// The result is a production of components, to answer a workorder
  /// </summary>
  public interface ILine: IVersionable, IDisplayable, IComparable<ILine>, ISerializableModel
  {
    /// <summary>
    /// Name of the line
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code used in some companies to identify a line
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Components produced by the line
    /// </summary>
    ICollection<IComponent> Components { get; }
    
    /// <summary>
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    void AddComponent(IComponent component);
        
    /// <summary>
    /// Remove a component
    /// </summary>
    /// <param name="component"></param>
    void RemoveComponent(IComponent component);
  }
}
