// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Xml;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Builder interface
  ///
  /// The aim of the builder is to build an XmlDocument from a data source
  /// (database, file...)
  /// </summary>
  public interface IBuilder
  {
    /// <summary>
    /// Build the wanted repository from the DOMDocument
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="cancellationToken"></param>
    void Build (XmlDocument doc, CancellationToken cancellationToken);

    /// <summary>
    /// Give the possibility to use an asynchronous commit
    /// </summary>
    void SetAsynchronousCommit ();
  }
}
