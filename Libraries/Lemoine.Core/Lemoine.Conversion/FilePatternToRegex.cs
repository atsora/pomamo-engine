// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Convert a file pattern to a regex
  /// </summary>
  public static class FilePatternToRegex
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FilePatternToRegex).ToString ());

    static readonly Regex HAS_QUESTION_MARK = new Regex (@"\?", RegexOptions.Compiled);
    static readonly Regex ILLEGAL_CHARACTERS = new Regex ("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
    static readonly Regex CATCH_EXTENSION = new Regex (@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
    static readonly string NON_DOT_CHARACTERS = @"[^.]*";

    /// <summary>
    /// Convert a file pattern to a regex
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static Regex ConvertFilePatternToRegex (string pattern)
    {
      if (pattern == null) {
        throw new ArgumentNullException ();
      }
      pattern = pattern.Trim ();
      if (pattern.Length == 0) {
        log.ErrorFormat ("Convert: empty pattern {0}", pattern);
        throw new ArgumentException ("Pattern is empty.");
      }
      if (ILLEGAL_CHARACTERS.IsMatch (pattern)) {
        log.ErrorFormat ("Convert: pattern {0} contains illegal characters", pattern);
        throw new ArgumentException ("Pattern contains illegal characters.");
      }
      bool hasExtension = CATCH_EXTENSION.IsMatch (pattern);
      bool matchExact = false;
      if (HAS_QUESTION_MARK.IsMatch (pattern)) {
        matchExact = true;
      }
      else if (hasExtension) {
        matchExact = CATCH_EXTENSION.Match (pattern).Groups[1].Length != 3;
      }
      string regexString = Regex.Escape (pattern);
      regexString = "^" + Regex.Replace (regexString, @"\\\*", ".*");
      regexString = Regex.Replace (regexString, @"\\\?", ".");
      if (!matchExact && hasExtension) {
        regexString += NON_DOT_CHARACTERS;
      }
      regexString += "$";
      Regex regex = new Regex (regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
      return regex;
    }
  }
}
