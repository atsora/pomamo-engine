// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;

using Lemoine.Core.Log;

namespace Lemoine.Alert.TestActions
{
  /// <summary>
  /// Generic collection action
  /// </summary>
  public class CollectionAction: Lemoine.Extensions.Alert.IAction
  {
    ICollection<string> m_collection;

    static readonly ILog log = LogManager.GetLogger(typeof (CollectionAction).FullName);

    /// <summary>
    /// Queue where the data is injected
    /// </summary>
    public ICollection<string> Collection
    {
      get { return m_collection; }
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="collection"></param>
    public CollectionAction (ICollection<string> collection)
    {
      m_collection = collection;
    }
    #endregion // Constructors

    /// <summary>
    /// Implements <see cref="IAction" />
    /// </summary>
    /// <param name="data"></param>
    public void Execute (XmlElement data)
    {
      m_collection.Add (data.OuterXml);
    }
  }
}
