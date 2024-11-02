// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Tool
  /// 
  /// This table lists the tools used in the workshop.
  /// </summary>
  public interface ITool: IVersionable, IDataWithIdentifiers, IDisplayable, ISerializableModel
  {
    /// <summary>
    /// Full tool name / description. This is optional
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Unique number that allows to identify in some companies a tool. This tool code might be found in the ISO file
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Diameter of the tool
    /// </summary>
    double? Diameter { get; set; }
    
    /// <summary>
    /// Diameter as string
    /// </summary>
    string DiameterAsString { get; set; }
    
    /// <summary>
    /// Radius of the tool
    /// </summary>
    double? Radius { get; set; }

    /// <summary>
    /// Size of the tool
    /// 
    /// This concatenates the diameter and the radius in case they exist,
    /// else an empty string is returned
    /// </summary>
    string Size { get; }
  }
}
