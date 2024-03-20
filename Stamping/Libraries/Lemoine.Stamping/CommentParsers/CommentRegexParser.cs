// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Stamping.DataInterpreters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lemoine.Stamping.CommentParsers
{
  /// <summary>
  /// Implementation of <see cref="ICommentParser" /> using a list of regex
  /// </summary>
  public class CommentRegexParser : ICommentParser
  {
    readonly ILog log = LogManager.GetLogger (typeof (CommentRegexParser).FullName);

    IList<Regex> m_regex = new List<Regex> ();

    /// <summary>
    /// List of string regex
    /// </summary>
    public IList<string> RegexList
    {
      get => m_regex.Select (x => x.ToString ()).ToList ();
      set {
        m_regex = value.Select (x => new Regex (x, RegexOptions.Compiled)).ToList ();
      }
    }

    /// <summary>
    /// <see cref="ICommentParser"/>
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="stampingData"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool ParseComment (string comment, StampingData stampingData)
    {
      foreach (var r in m_regex) {
        var match = r.Match (comment);
        if (match.Success) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ParseComment: {comment} matches {r}");
          }
          foreach (Group group in match.Groups) {
            if (!int.TryParse (group.Name, out _)) {
              if (log.IsInfoEnabled) {
                log.Info ($"ParseComment: add {group.Name}={group.Value}");
              }
              stampingData.Add (group.Name, group.Value);
            }
          }
          return true;
        }
      }
      return false;
    }
  }
}
