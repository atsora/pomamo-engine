// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// What the searches of the work information, by name, code or external code, have in
  /// common
  /// </summary>
  internal static class WorkInfoSearch
  {
    const char ESCAPE_CHAR = '\\';

    /// <summary>
    /// Escape the wildcards of a LIKE pattern, so that the text is matched as it is
    /// </summary>
    /// <param name="search">not null</param>
    /// <returns></returns>
    static string Escape (string search) => search
      .Replace ($"{ESCAPE_CHAR}", $"{ESCAPE_CHAR}{ESCAPE_CHAR}")
      .Replace ("%", $"{ESCAPE_CHAR}%")
      .Replace ("_", $"{ESCAPE_CHAR}_");

    /// <summary>
    /// Criterion that matches the text anywhere in the specified properties, whatever the
    /// case
    /// </summary>
    /// <param name="search">not null or empty</param>
    /// <param name="properties">not empty</param>
    /// <returns></returns>
    public static ICriterion Matches (string search, params string[] properties)
    {
      var escaped = Escape (search);
      var disjunction = new Disjunction ();
      foreach (var property in properties) {
        disjunction.Add (new LikeExpression (property, escaped, MatchMode.Anywhere, ESCAPE_CHAR, true));
      }
      return disjunction;
    }

    /// <summary>
    /// Criterion that matches the start of a property, as it is for its wildcards
    /// </summary>
    /// <param name="property">not null or empty</param>
    /// <param name="prefix">not null</param>
    /// <returns></returns>
    public static ICriterion StartsWith (string property, string prefix) =>
      new LikeExpression (property, Escape (prefix), MatchMode.Start, ESCAPE_CHAR, false);

    /// <summary>
    /// Criterion that matches the text anywhere in the name, the code or the external code,
    /// whatever the case
    /// </summary>
    /// <param name="search">not null or empty</param>
    /// <returns></returns>
    public static ICriterion MatchesNameOrCode (string search) =>
      Matches (search, "Name", "Code", "ExternalCode");

    /// <summary>
    /// Keep the items of the requested archive state, put the archived items last, then sort
    /// them by id
    ///
    /// The filter is in the query, before the limit, so that the limit applies to the
    /// filtered items
    ///
    /// In PostgreSQL, a descending order puts the nulls first: the items that are not
    /// archived come first
    /// </summary>
    /// <param name="criteria">not null</param>
    /// <param name="archiveDateTimeProperty">null if the items are never archived</param>
    /// <param name="archived">null: every item, false: the items that are not archived only,
    /// true: the archived items only</param>
    /// <param name="maxResults"></param>
    /// <returns></returns>
    public static ICriteria OrderAndLimit (ICriteria criteria, string archiveDateTimeProperty, bool? archived, int maxResults)
    {
      if (archived.HasValue) {
        if (archiveDateTimeProperty is null) {
          if (archived.Value) { // Never archived: nothing matches
            criteria = criteria.Add (Expression.Sql ("1=0"));
          }
        }
        else {
          criteria = criteria.Add (archived.Value
            ? Restrictions.IsNotNull (archiveDateTimeProperty)
            : Restrictions.IsNull (archiveDateTimeProperty));
        }
      }
      if (archiveDateTimeProperty is not null) {
        criteria = criteria.AddOrder (Order.Desc (archiveDateTimeProperty));
      }
      return criteria
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxResults);
    }
  }
}
