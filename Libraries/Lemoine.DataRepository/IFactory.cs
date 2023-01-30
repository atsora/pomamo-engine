// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Xml;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Factory interface
  ///
  /// The aim of the factory is to build a DOMDocument from a data source
  /// (database, file...)
  /// </summary>
  public interface IFactory
  {
    /// <summary>
    /// Build the DOMDocument
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="optional">If optional, then null may be returned if the data is not found</param>
    /// <returns>The DOM Document</returns>
    XmlDocument GetData (CancellationToken cancellationToken, bool optional = false);

    /// <summary>
    /// Check if the factory has an action in case the synchronization is ok
    /// </summary>
    /// <returns></returns>
    bool CheckSynchronizationOkAction ();

    /// <summary>
    /// The synchronization was successful.
    /// Optionally flag the data as successfully synchronized.
    /// 
    /// Note this is only applicable for the main factory.
    /// </summary>
    void FlagSynchronizationAsSuccess (XmlDocument document);

    /// <summary>
    /// The synchronization failed.
    /// Optionally flag the data as not synchronized.
    /// 
    /// Note this is only applicable for the main factory.
    /// </summary>
    void FlagSynchronizationAsFailure (XmlDocument document);
  }
}
