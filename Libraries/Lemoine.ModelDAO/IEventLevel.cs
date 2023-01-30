// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table EventLevel
  /// </summary>
  public interface IEventLevel: IVersionable, IDataWithTranslation, ISerializableModel, IDisplayable, ISelectionable
  {    
    /// <summary>
    /// Event level priority
    /// </summary>
    int Priority { get; set; }
  }
  
  /// <summary>
  /// EventLevel interface for the data grid views (to bypass a limitation of the MS data grid views)
  /// </summary>
  public interface IDataGridViewEventLevel: IDataWithIdentifiers, ISerializableModel, IDisplayable, ISelectionable
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; } 
    
    /// <summary>
    /// Version
    /// </summary>
    int Version { get; }
    
    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string TranslationKey { get; set; }
    
    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    string NameOrTranslation { get; }
       
    /// <summary>
    /// Event level priority
    /// </summary>
    int Priority { get; set; }
  }
}
