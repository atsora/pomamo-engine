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
    #region Members
    ICollection<string> m_collection;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CollectionAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Queue where the data is injected
    /// </summary>
    public ICollection<string> Collection
    {
      get { return m_collection; }
    }
    #endregion // Getters / Setters

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

    #region Methods
    /// <summary>
    /// Implements <see cref="IAction" />
    /// </summary>
    /// <param name="data"></param>
    public void Execute (XmlElement data)
    {
      m_collection.Add (data.OuterXml);
    }
    #endregion // Methods
  }
}
