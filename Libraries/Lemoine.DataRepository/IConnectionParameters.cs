// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Interface for ODBC connection parameters
  /// </summary>
  public interface IConnectionParameters
  {
    /// <summary>
    /// Connection string
    /// </summary>
    string ConnectionString ();

    /// <summary>
    /// ODBC connection string
    /// </summary>
    string OdbcConnectionString ();

    /// <summary>
    /// build connection parameters from XML file
    /// </summary>
    void Build (XmlDocument doc);
  }
}
