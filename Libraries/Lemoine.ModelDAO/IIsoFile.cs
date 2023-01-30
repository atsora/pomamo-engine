// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table IsoFile
  /// </summary>
  public interface IIsoFile: IVersionable, IDataWithIdentifiers, IDisplayable
  {
    /// <summary>
    /// File name of the program without the path, with the extension
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Reference to the Computer where the stamping took place
    /// </summary>
    IComputer Computer { get; set; }
    
    /// <summary>
    /// Directory of the source ISO file (before stamping)
    /// </summary>
    string SourceDirectory { get; set; }
    
    /// <summary>
    /// Directory where the ISO file was stamped
    /// </summary>
    string StampingDirectory { get; set; }
    
    /// <summary>
    /// Size of the ISO file, in block number,
    /// line number or bytes according to the post-processor setting
    /// </summary>
    int? Size { get; set; }
    
    /// <summary>
    /// UTC date/time of the stamping
    /// </summary>
    DateTime StampingDateTime { get; set; }
    
    /// <summary>
    /// List of stamps that are associated to this ISO file
    /// </summary>
    ICollection<IStamp> Stamps { get; }
  }
}
