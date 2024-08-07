﻿// Copyright (C) 2024 Atsora Solutions

using Pulse.Extensions.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// Extension to get operation data from a regex match in the program comment
  /// </summary>
  public interface IRegexMatchToFileOpDataExtension
    : Lemoine.Extensions.IExtension
    , IInitializedByMachineExtension
  {
    /// <summary>
    /// Priority
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Get operation data for a specific data name, for example opName, partName, opCode
    /// Raise an exception if not applicable
    /// 
    /// Here are some possible data names:
    /// <item>partName</item>
    /// <item>partCode</item>
    /// <item>opName</item>
    /// <item>opCode</item>
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    string GetOpData (Match match, string dataName);
  }
}
