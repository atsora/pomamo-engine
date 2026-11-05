// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Methods to normalize, parse and format the tags of an item (a doc, a machine, ...)
  /// </summary>
  public static class TagsHelper
  {
    /// <summary>
    /// Separators of the tags in a text, see <see cref="Parse(string)"/>
    /// </summary>
    static readonly char[] SEPARATORS = new char[] { ',', ';' };

    /// <summary>
    /// Separator to use to format the tags in a text, see <see cref="Format(IEnumerable{string})"/>
    /// </summary>
    public static readonly string FORMAT_SEPARATOR = ", ";

    /// <summary>
    /// Trim the tags, and remove the null, empty and duplicate ones, keeping the order
    /// </summary>
    /// <param name="tags">nullable</param>
    /// <returns>not null, a new list</returns>
    public static IList<string> Normalize (IEnumerable<string> tags) =>
      (tags ?? Enumerable.Empty<string> ())
        .Where (tag => null != tag)
        .Select (tag => tag.Trim ())
        .Where (tag => 0 < tag.Length)
        .Distinct (StringComparer.Ordinal)
        .ToList ();

    /// <summary>
    /// Parse tags that are separated by a comma (or a semicolon), for example "quality, safety"
    /// </summary>
    /// <param name="text">nullable</param>
    /// <returns>not null, normalized</returns>
    public static IList<string> Parse (string text) =>
      Normalize ((text ?? "").Split (SEPARATORS));

    /// <summary>
    /// Format tags in a text, separated by a comma, so that <see cref="Parse(string)"/> gets them back
    /// </summary>
    /// <param name="tags">nullable</param>
    /// <returns>not null, empty if there is no tag</returns>
    public static string Format (IEnumerable<string> tags) =>
      string.Join (FORMAT_SEPARATOR, Normalize (tags));
  }
}
