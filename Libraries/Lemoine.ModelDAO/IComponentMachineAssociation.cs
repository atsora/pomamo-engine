// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table ComponentMachineAssociation
  /// 
  /// In this table is stored a new row each time a component
  /// is associated to a machine.
  /// 
  /// It does not represent the current relation between the component
  /// and a machining resource, but all the manual or automatic associations
  /// that are made between a component and a machining resource.
  /// </summary>
  public interface IComponentMachineAssociation: IMachineAssociation
  {
    /// <summary>
    /// Reference to the Component persistent class
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Association option
    /// </summary>
    Nullable<AssociationOption> Option { get; set; }
  }
}
