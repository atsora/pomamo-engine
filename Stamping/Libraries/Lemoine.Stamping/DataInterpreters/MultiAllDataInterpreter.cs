// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.DataInterpreters
{
  /// <summary>
  /// <see cref="IDataInterpreter"/> implementation using multiple <see cref="IDataInterpreter"/>.
  /// 
  /// All the <see cref="IDataInterpreter"/> are executed
  /// </summary>
  public class MultiAllDataInterpreter : IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (MultiAllDataInterpreter).FullName);

    readonly IEnumerable<IDataInterpreter> m_dataInterpreters;

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiAllDataInterpreter (params IDataInterpreter[] dataInterpreters)
    {
      m_dataInterpreters = dataInterpreters;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataInterpreters"></param>
    public MultiAllDataInterpreter (IEnumerable<IDataInterpreter> dataInterpreters)
    {
      m_dataInterpreters = dataInterpreters;
    }

    /// <summary>
    /// <see cref="IDataInterpreter"/>
    /// 
    /// Return true if one of the <see cref="IDataInterpreter"/> is successful
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    public bool Interpret (StampingData stampingData)
    {
      bool success = false;
      foreach (var dataInterpreter in m_dataInterpreters) {
        try {
          if (dataInterpreter.Interpret (stampingData)) {
            success = true;
          }
        }
        catch (Exception ex) {
          log.Error ($"Interpret: exception for {dataInterpreter}", ex);
          throw;
        }
      }
      return success;
    }
  }
}
