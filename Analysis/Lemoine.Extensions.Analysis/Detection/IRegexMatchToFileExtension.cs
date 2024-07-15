// Copyright (C) 2024 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// Extension to get a file path from a regex match in the program comment
  /// </summary>
  public interface IRegexMatchToFileExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Get the file path from a regex match
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    string GetPath (Match match);
  }
}
