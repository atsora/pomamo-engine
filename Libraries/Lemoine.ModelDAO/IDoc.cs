// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table doc
  /// 
  /// A doc is a reference to a document that may be associated to any other item
  /// of the system (a task template, an operation, a machine, ...).
  /// 
  /// The document itself is not stored in the database: only its path is.
  /// How the document is effectively stored, listed, downloaded or uploaded
  /// is delegated to an implementation of IDocExtension, that is only keyed by the path
  /// (a git repository for the first implementation).
  /// 
  /// This way a document may be referenced at any time, even if no IDocExtension
  /// implementation is available.
  /// </summary>
  public interface IDoc : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Effective path of the document, as used by the IDocExtension implementations
    /// 
    /// Unique, not null and not empty
    /// </summary>
    string Path { get; set; }
  }
}
