// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Display of an entity in the UI culture of the current request.
  ///
  /// The display column of a table (<see cref="DataWithDisplayFunction"/>) is computed by PostgreSQL,
  /// with the locale of the config key i18n.locale.default. When the UI culture differs from it
  /// (for example a web request with ?culture=de), the display pattern of the display table is applied
  /// in .NET instead, the same way as the SQL functions displayrequest and displayextract do:
  /// <list type="bullet">
  /// <item><c>&lt;%NameOrTranslation%&gt;</c>, <c>&lt;%Code%&gt;</c>...: property of the entity, trimmed</item>
  /// <item><c>&lt;%Display%&gt;</c>, <c>&lt;%Display_Variant%&gt;</c>: display of the entity, with that variant</item>
  /// <item><c>&lt;%Foreign.X%&gt;</c>: X of the entity in property Foreign, empty if not set</item>
  /// </list>
  ///
  /// In case a pattern can't be applied (unknown placeholder, lazy entity that can't be loaded...),
  /// the display computed by PostgreSQL is kept.
  /// </summary>
  internal static class DisplayTranslation
  {
    static readonly string DEFAULT_LOCALE_CONFIG_KEY = "i18n.locale.default";
    static readonly TimeSpan CACHE_TIMEOUT = TimeSpan.FromMinutes (1);
    static readonly int MAX_DEPTH = 5;
    static readonly Regex PLACEHOLDER_REGEX = new Regex (@"<%([\w.]+)%>", RegexOptions.Compiled);
    static readonly Regex FOREIGN_REGEX = new Regex (@"^(\w+)\.(\w+)$", RegexOptions.Compiled);

    static readonly ILog log = LogManager.GetLogger (typeof (DisplayTranslation).FullName);

    static readonly ConcurrentDictionary<string, (string pattern, DateTime expiration)> s_patterns = new ConcurrentDictionary<string, (string, DateTime)> ();
    static (string locale, DateTime expiration)? s_databaseLocale = null;

    /// <summary>
    /// Display of an entity in the current UI culture
    /// </summary>
    /// <param name="entity">not null</param>
    /// <param name="table">table in the display table, for example Reason</param>
    /// <param name="variant">nullable: variant of the display, for example Long</param>
    /// <param name="sqlDisplay">display computed by PostgreSQL</param>
    /// <returns>sqlDisplay if the UI culture is the one of the database, if the pattern can't be applied,
    /// or if the entity is not persistent yet (no display computed by the database)</returns>
    public static string GetDisplay (object entity, string table, string variant, string sqlDisplay)
    {
      if (IsTransient (entity, sqlDisplay) || !IsTranslationRequired ()) {
        return sqlDisplay;
      }
      try {
        return Evaluate (entity, table, variant, 0) ?? sqlDisplay;
      }
      catch (Exception ex) {
        log.Error ($"GetDisplay: exception for table {table} variant {variant} => return the display of the database {sqlDisplay}", ex);
        return sqlDisplay;
      }
    }

    /// <summary>
    /// Display of an entity whose display in the database is its translated name
    /// (tables of the SQL function displayfromnametranslationkeylist, for example role)
    /// </summary>
    /// <param name="entity">not null</param>
    /// <param name="getNameOrTranslation">not null</param>
    /// <param name="sqlDisplay">display computed by PostgreSQL</param>
    /// <returns>sqlDisplay if the UI culture is the one of the database,
    /// or if the entity is not persistent yet (no display computed by the database)</returns>
    public static string GetNameOrTranslationDisplay (object entity, Func<string> getNameOrTranslation, string sqlDisplay)
    {
      if (IsTransient (entity, sqlDisplay) || !IsTranslationRequired ()) {
        return sqlDisplay;
      }
      try {
        return getNameOrTranslation ()?.Trim () ?? "";
      }
      catch (Exception ex) {
        log.Error ($"GetNameOrTranslationDisplay: exception => return the display of the database {sqlDisplay}", ex);
        return sqlDisplay;
      }
    }

    /// <summary>
    /// Is the entity not persistent yet ? Then the database has not computed any display,
    /// which must stay null (for example in the XML serialization of a new entity).
    ///
    /// A persistent entity may also have a null display in the database, when its translation is missing:
    /// then it is computed in .NET
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="sqlDisplay"></param>
    /// <returns></returns>
    static bool IsTransient (object entity, string sqlDisplay)
    {
      if (sqlDisplay is not null) {
        return false;
      }
      var idProperty = entity.GetType ().GetProperty ("Id", BindingFlags.Public | BindingFlags.Instance);
      return idProperty?.GetValue (entity) is int id && 0 == id;
    }

    /// <summary>
    /// Is the UI culture different from the locale the database uses to compute the display columns ?
    /// </summary>
    /// <returns></returns>
    internal static bool IsTranslationRequired ()
    {
      var databaseLocale = GetDatabaseLocale ();
      if (databaseLocale is null) { // Unknown: keep the display of the database
        return false;
      }
      return !IsSameLocale (CultureInfo.CurrentUICulture.Name, databaseLocale);
    }

    /// <summary>
    /// Compare a culture name to a locale of the database (fr, fr-FR or fr_FR)
    /// </summary>
    /// <param name="cultureName"></param>
    /// <param name="databaseLocale"></param>
    /// <returns></returns>
    internal static bool IsSameLocale (string cultureName, string databaseLocale) =>
      string.Equals (cultureName, databaseLocale.Trim ().Replace ('_', '-'), StringComparison.OrdinalIgnoreCase);

    static string GetDatabaseLocale ()
    {
      var cached = s_databaseLocale;
      if (cached.HasValue && (DateTime.UtcNow < cached.Value.expiration)) {
        return cached.Value.locale;
      }
      string locale = null;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var config = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig (DEFAULT_LOCALE_CONFIG_KEY);
          locale = config?.Value as string ?? "";
        }
      }
      catch (Exception ex) {
        log.Error ($"GetDatabaseLocale: the config {DEFAULT_LOCALE_CONFIG_KEY} could not be read", ex);
        return cached?.locale; // Do not cache the failure
      }
      s_databaseLocale = (locale, DateTime.UtcNow.Add (CACHE_TIMEOUT));
      return locale;
    }

    /// <summary>
    /// Apply a display pattern to an entity
    /// </summary>
    /// <param name="pattern">display pattern, for example &lt;%NameOrTranslation%&gt; &lt;%Unit.Display_Brackets%&gt;</param>
    /// <param name="entity">not null</param>
    /// <param name="table"></param>
    /// <param name="getPattern">pattern of a table and a variant (nullable), null if there is none</param>
    /// <returns>null if the pattern can't be applied</returns>
    internal static string Apply (string pattern, object entity, string table, Func<string, string, string> getPattern) =>
      Apply (pattern, entity, table, getPattern, 0);

    static string Evaluate (object entity, string table, string variant, int depth)
    {
      var pattern = GetPattern (table, variant);
      if (pattern is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Evaluate: no pattern for table {table} variant {variant}");
        }
        return null;
      }
      return Apply (pattern, entity, table, GetPattern, depth);
    }

    static string Apply (string pattern, object entity, string table, Func<string, string, string> getPattern, int depth)
    {
      if (MAX_DEPTH < depth) {
        log.Error ($"Apply: max depth reached for table {table} and pattern {pattern}, probably a recursive pattern");
        return null;
      }
      var failed = false;
      var result = PLACEHOLDER_REGEX.Replace (pattern, match => {
        var value = failed ? null : GetPlaceholderValue (entity, table, match.Groups[1].Value, getPattern, depth);
        if (value is null) {
          failed = true;
          return "";
        }
        return value;
      });
      return failed ? null : result;
    }

    static string GetPlaceholderValue (object entity, string table, string placeholder, Func<string, string, string> getPattern, int depth)
    {
      var foreignMatch = FOREIGN_REGEX.Match (placeholder);
      if (foreignMatch.Success) { // Foreign.X
        var foreignTable = foreignMatch.Groups[1].Value;
        if (!TryGetProperty (entity, foreignTable, out var foreign)) {
          return null;
        }
        if (foreign is null) {
          return "";
        }
        return GetPlaceholderValue (foreign, foreignTable, foreignMatch.Groups[2].Value, getPattern, depth + 1);
      }

      if (placeholder.Equals ("Display", StringComparison.OrdinalIgnoreCase)
        || placeholder.StartsWith ("Display_", StringComparison.OrdinalIgnoreCase)) {
        var variant = placeholder.Length > "Display_".Length
          ? placeholder.Substring ("Display_".Length)
          : null;
        var pattern = getPattern (table, variant);
        if (pattern is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPlaceholderValue: no pattern for table {table} variant {variant}");
          }
          return null;
        }
        return Apply (pattern, entity, table, getPattern, depth + 1);
      }

      if (!TryGetProperty (entity, placeholder, out var v)) {
        return null;
      }
      return Convert.ToString (v, CultureInfo.InvariantCulture)?.Trim () ?? "";
    }

    static bool TryGetProperty (object entity, string propertyName, out object value)
    {
      var property = entity.GetType ().GetProperty (propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
      if (property is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetProperty: no property {propertyName} in {entity.GetType ()}");
        }
        value = null;
        return false;
      }
      value = property.GetValue (entity);
      return true;
    }

    static string GetPattern (string table, string variant)
    {
      var key = $"{table}/{variant}".ToLowerInvariant ();
      if (s_patterns.TryGetValue (key, out var cached) && (DateTime.UtcNow < cached.expiration)) {
        return cached.pattern;
      }
      string pattern;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        pattern = ModelDAOHelper.DAOFactory.DisplayDAO.FindWithTableVariant (table, variant)?.Pattern;
      }
      s_patterns[key] = (pattern, DateTime.UtcNow.Add (CACHE_TIMEOUT));
      return pattern;
    }
  }
}
