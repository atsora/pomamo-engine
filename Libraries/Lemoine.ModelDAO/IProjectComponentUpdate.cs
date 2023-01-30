// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ProjectComponentUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a Component and its Project.
  /// 
  /// It includes mainly the following two column to track the modifications:
  /// <item>Old Project ID: null in case a new project/component relation is set</item>
  /// <item>New Project ID: null in case the project/component relation is deleted</item>
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  public interface IProjectComponentUpdate: IGlobalModification
  {
    /// <summary>
    /// Associated component
    /// 
    /// Not null
    /// </summary>
    IComponent Component { get; set; }
    
    /// <summary>
    /// Old associated project
    /// 
    /// null in case a new project/component relation is set
    /// </summary>
    IProject OldProject { get; set; }
    
    /// <summary>
    /// New associated project
    /// 
    /// null in case the project/component relation is deleted
    /// </summary>
    IProject NewProject { get; set; }
  }
}
