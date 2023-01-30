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
  /// Only the first successful <see cref="IDataInterpreter"/> is run
  /// </summary>
  public class MultiFirstDataInterpreter : IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (MultiFirstDataInterpreter).FullName);

    readonly IEnumerable<IDataInterpreter> m_dataInterpreters;

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiFirstDataInterpreter (params IDataInterpreter[] dataInterpreters)
    {
      m_dataInterpreters = dataInterpreters;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataInterpreters"></param>
    public MultiFirstDataInterpreter (IEnumerable<IDataInterpreter> dataInterpreters)
    {
      m_dataInterpreters = dataInterpreters;
    }

    /// <summary>
    /// <see cref="IDataInterpreter"/>
    /// </summary>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    public bool Interpret (StampingData stampingData)
    {
      foreach (var dataInterpreter in m_dataInterpreters) {
        if (dataInterpreter.Interpret (stampingData)) {
          return true;
        }
      }
      return false;
    }
  }
}
