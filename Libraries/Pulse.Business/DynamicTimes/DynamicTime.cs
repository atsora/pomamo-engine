// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Lemoine.Business.Extension;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using System.Text.RegularExpressions;

namespace Lemoine.Business.DynamicTimes
{
  /// <summary>
  /// No dynamic time exception: the dynamic time extension does not exist
  /// </summary>
  public class NoDynamicTime : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public NoDynamicTime (string message)
      : base (message)
    { }
  }

  /// <summary>
  /// Methods to get a dynamic end
  /// </summary>
  public static class DynamicTime
  {
    static ILog log = LogManager.GetLogger (typeof (DynamicTime).FullName);

    static readonly Regex NAME_REGEX = new Regex (@"([a-zA-Z]+)(\((.*)\))?");

    static ILog GetDynamicTimeLogger (string name, IMachine machine)
    {
      Debug.Assert (null != machine);

      string category = "DynamicTime." + name + "." + machine.Id;
      return LogManager.GetLogger (category);
    }

    static void LogDynamicTimeResponse (string name, string parameter, IMachine machine, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit, IDynamicTimeResponse response, bool notApplicableExpected = false)
    {
      var logger = GetDynamicTimeLogger (name, machine);
      if (response.Timeout) {
        if (logger.IsErrorEnabled) {
          logger.ErrorFormat ("({0}): Timeout, dateTime={1} hint={2} limit={3}",
            parameter, dateTime, hint, limit);
        }
      }
      else if (response.NotApplicable) {
        if (notApplicableExpected) {
          if (logger.IsInfoEnabled) {
            logger.Info ($"({parameter}): Not applicable, dateTime={dateTime} hint={hint} limit={limit}");
          }
        }
        else {
          if (logger.IsErrorEnabled) {
            logger.Error ($"({parameter}): Not applicable, dateTime={dateTime} hint={hint} limit={limit}");
          }
        }
      }
      else if (response.NoData) {
        if (response.Final.HasValue) {
          if (logger.IsInfoEnabled) {
            logger.InfoFormat ("({0}): No data with final={4}, dateTime={1} hint={2} limit={3}",
              parameter, dateTime, hint, limit, response.Final);
          }
        }
        else {
          if (logger.IsInfoEnabled) {
            logger.InfoFormat ("({0}): No data, dateTime={1} hint={2} limit={3}",
              parameter, dateTime, hint, limit);
          }
        }
      }
      else if (response.Final.HasValue) {
        if (logger.IsInfoEnabled) {
          logger.InfoFormat ("({0}): final={4}, dateTime={1} hint={2} limit={3}",
            parameter, dateTime, hint, limit, response.Final);
        }
      }
      else {
        if (logger.IsDebugEnabled) {
          logger.DebugFormat ("({0}): pending, new hint={4}, dateTime={1} hint={2} limit={3}",
            parameter, dateTime, hint, limit, response.Hint);
        }
      }
    }

    /// <summary>
    /// Get a dynamic time given an extension
    /// </summary>
    /// <param name="extension">not null and initialized with the machine</param>
    /// <param name="dateTime"></param>
    /// <param name="hint">in limit</param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public static IDynamicTimeResponse GetDynamicTime (IDynamicTimeExtension extension, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      Debug.Assert (null != extension);
      Debug.Assert (null != extension.Machine);
      Debug.Assert (limit.Overlaps (hint));

      // Applicable ? (static cache)
      if (!IsApplicable (extension)) {
        return extension.CreateNotApplicable ();
      }

      try { // Time
        var request = new DynamicTimeRequest (extension, dateTime, hint, limit);
        var cacheData = ServiceProvider.GetCacheData<IDynamicTimeResponse> (request);
        request.CacheData = cacheData;
        var response = ServiceProvider.Get<IDynamicTimeResponse> (request);
        if (response.Final.HasValue
          && !response.NoData
          && !limit.ContainsElement (response.Final.Value)) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetDynamicTime: final {response.Final} not in limit {limit} => NoData");
          }
          response.NoData = true;
        }
        if (!response.Final.HasValue && !response.NoData && !response.Hint.Overlaps (limit)) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetDynamicTime: new hint {response.Hint} does not overlap limit {limit} => NoData");
          }
          response.NoData = true;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetDynamicTime: return {response}");
        }
        return response;
      }
      catch (Exception ex) {
        log.Error ($"GetDynamicTime: exception", ex);
        // TODO: dynamic time in error ?
        throw;
      }
    }

    /// <summary>
    /// Get a dynamic end given a dynamic time name
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <param name="range">The dynamic time is after the lower bound of the range by default, or the upper bound of the range in case the name contains the suffix '+'</param>
    /// <returns>not null</returns>
    public static IDynamicTimeResponse GetDynamicTime (string name, IMachine machine, UtcDateTimeRange range)
    {
      DateTime start;
      UtcDateTimeRange limit;
      string effectiveName = PreProcessNameForRange (name, range, out start, out limit);
      if (!limit.ContainsElement (start)) {
        log.Fatal ($"GetDynamicTime: effective limit={limit} does not contain {start}");
      }
      return GetDynamicTime (effectiveName, machine, start, new UtcDateTimeRange ("(,)"), limit);
    }

    /// <summary>
    /// Get a dynamic time given a dynamic time name
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <param name="range">The dynamic time considers the lower bound of the range by default, or the upper bound of the range in case the name contains the suffix '+'</param>
    /// <param name="hint"></param>
    /// <param name="limit"></param>
    /// <param name="notApplicableExpected">A NotApplicable answer is possible / expected</param>
    /// <returns>not null</returns>
    public static IDynamicTimeResponse GetDynamicTime (string name, IMachine machine, UtcDateTimeRange range, UtcDateTimeRange hint, UtcDateTimeRange limit, bool notApplicableExpected = false)
    {
      DateTime start;
      UtcDateTimeRange limit2;
      string effectiveName = PreProcessNameForRange (name, range, out start, out limit2);
      var effectiveLimit = new UtcDateTimeRange (limit.Intersects (limit2));
      if (!effectiveLimit.ContainsElement (start)) {
        log.Fatal ($"GetDynamicTime: effective limit={effectiveLimit} does not contain {start}, limit in args={limit}, limit2={limit2}");
      }
      return GetDynamicTime (effectiveName, machine, start, hint, effectiveLimit, notApplicableExpected: notApplicableExpected);
    }

    /// <summary>
    /// Get a dynamic time given a dynamic time name
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <param name="dateTime">The dynamic time is after this UTC date/time</param>
    /// <param name="notApplicableExpected">A NotApplicable answer is possible / expected</param>
    /// <returns>not null</returns>
    public static IDynamicTimeResponse GetDynamicTime (string name, IMachine machine, DateTime dateTime, bool notApplicableExpected = false)
    {
      return GetDynamicTime (name, machine, dateTime, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"), notApplicableExpected: notApplicableExpected);
    }

    static (string, string) ExtractNameParameter (string name)
    {
      var match = NAME_REGEX.Match (name);
      if (!match.Success) {
        log.ErrorFormat ("ExtractNameParameter: invalid name {0}", name);
        throw new ArgumentException ("Invalid dynamic time name", "name");
      }
      else {
        if (match.Groups[3].Success) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("ExtractNameParameter: name={0} parameter={1}", match.Groups[1].Value, match.Groups[3].Value);
          }
          return (match.Groups[1].Value, match.Groups[3].Value);
        }
        else {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("ExtractNameParameter: name={0} with no parameter", name);
          }
          return (name, null);
        }
      }
    }

    /// <summary>
    /// Get a dynamic time given a dynamic time name
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <param name="dateTime">Start UTC date/time</param>
    /// <param name="hint"></param>
    /// <param name="limit"></param>
    /// <param name="notApplicableExpected">A NotApplicable answer is possible / expected</param>
    /// <returns>not null</returns>
    public static IDynamicTimeResponse GetDynamicTime (string name, IMachine machine, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit, bool notApplicableExpected = false)
    {
      var (nameWithoutParameter, parameter) = ExtractNameParameter (name);
      var interrupted = true;
      try {
        var response = GetDynamicTime (nameWithoutParameter, parameter, machine, dateTime, hint, limit, notApplicableExpected: notApplicableExpected);
        interrupted = false;
        return response;
      }
      catch (Exception ex) {
        var logger = GetDynamicTimeLogger (nameWithoutParameter, machine);
        logger.Error ($"GetDynamicTime: exception for {name} dateTime={dateTime}", ex);
        interrupted = false;
        throw;
      }
      finally {
        if (interrupted) {
          var logger = GetDynamicTimeLogger (nameWithoutParameter, machine);
          logger.Error ($"GetDynamicTime: interrupted for {name}");
        }
      }
    }

    /// <summary>
    /// Get the associated extensions
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public static IEnumerable<IDynamicTimeExtension> GetExtensions (string name, IMachine machine)
    {
      var (nameWithoutParameter, parameter) = ExtractNameParameter (name);
      return GetExtensions (nameWithoutParameter, parameter, machine);
    }

    /// <summary>
    /// Get the associated extensions
    /// </summary>
    /// <param name="nameWithoutParameter"></param>
    /// <param name="parameter"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    static IEnumerable<IDynamicTimeExtension> GetExtensions (string nameWithoutParameter, string parameter, IMachine machine)
    {
      var extensionsRequest = new NameMachineExtensions<IDynamicTimeExtension> (nameWithoutParameter, machine, (ext, m) => ext.Initialize (m, parameter));
      var extensions = ServiceProvider.Get (extensionsRequest);
      return extensions;
    }

    /// <summary>
    /// Get a dynamic time given a dynamic time name
    /// </summary>
    /// <param name="nameWithoutParameter">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="parameter"></param>
    /// <param name="machine"></param>
    /// <param name="dateTime">Start UTC date/time</param>
    /// <param name="hint"></param>
    /// <param name="limit"></param>
    /// <param name="notApplicableExpected">A NotApplicable answer is possible / expected</param>
    /// <returns>not null</returns>
    static IDynamicTimeResponse GetDynamicTime (string nameWithoutParameter, string parameter, IMachine machine, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit, bool notApplicableExpected = false)
    {
      Debug.Assert (limit.Overlaps (hint));

      var extensions = GetExtensions (nameWithoutParameter, parameter, machine);

      IDynamicTimeResponse response;

      if (!extensions.Any ()) {
        log.ErrorFormat ("GetDynamicTime: no dynamic time extension with name {0}", nameWithoutParameter);
        var logger = GetDynamicTimeLogger (nameWithoutParameter, machine);
        if (logger.IsFatalEnabled) {
          logger.Fatal ("no dynamic time with that name");
        }
        var message = "No DynamicTime with name " + nameWithoutParameter;
        if (!string.IsNullOrEmpty (parameter)) {
          message += "(" + parameter + ")";
        }
        throw new NoDynamicTime (message);
      }
      else if (1 == extensions.Count ()) { // A unique extension
        var extension = extensions.First ();
        response = GetDynamicTime (extension, dateTime, hint, limit);
        LogDynamicTimeResponse (nameWithoutParameter, parameter, machine, dateTime, hint, limit, response, notApplicableExpected: notApplicableExpected);
        return response;
      }
      else { // More than 1 extension
        log.WarnFormat ("GetDynamicTime: more than one dynamic end with name {0}", nameWithoutParameter);
        IEnumerable<(IDynamicTimeExtension,IDynamicTimeResponse)> responses = extensions
          .Select (extension => (extension,GetDynamicTime (extension, dateTime, hint, limit)));

        var withEnd = responses.Where (ext => ext.Item2.Final.HasValue);
        if (withEnd.Any ()) {
          var a = withEnd
            .OrderBy (x => x.Item2.Final.Value)
            .First ();
          var end = a.Item2.Final.Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetDynamicTime: return end {end} from multiple extensions");
          }
          response = a.Item1.CreateFinal (end);
          LogDynamicTimeResponse (nameWithoutParameter, parameter, machine, dateTime, hint, limit, response, notApplicableExpected: notApplicableExpected);
          return response;
        }

        var notApplicable = responses.All (ext => ext.Item2.NotApplicable);
        if (notApplicable) {
          response = responses.First ().Item1.CreateNotApplicable ();
          LogDynamicTimeResponse (nameWithoutParameter, parameter, machine, dateTime, hint, limit, response, notApplicableExpected: notApplicableExpected);
          return response;
        }

        var noData = responses.All (ext => ext.Item2.NoData);
        if (noData) {
          response = responses.First ().Item1.CreateNoData ();
          LogDynamicTimeResponse (nameWithoutParameter, parameter, machine, dateTime, hint, limit, response, notApplicableExpected: notApplicableExpected);
          return response;
        }

        var responseHint = responses
          .Select (r => r.Item2.Hint)
          .Aggregate (new UtcDateTimeRange ("(,)"), (a, b) => new UtcDateTimeRange (a.Intersects (b)));
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetDynamicTime: return hint {0} from multiple extensions", hint);
        }
        response = responses.First ().Item1.CreateWithHint (hint);
        LogDynamicTimeResponse (nameWithoutParameter, parameter, machine, dateTime, hint, limit, response, notApplicableExpected: notApplicableExpected);
        return response;
      }
    }

    /// <summary>
    /// Pre-process the name to get a start from the specified range
    /// 
    /// By default the lower bound is considered for start
    /// 
    /// If the name finishes by '+', then the upper bound is considered for start
    /// </summary>
    /// <param name="name"></param>
    /// <param name="range"></param>
    /// <param name="start"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    static string PreProcessNameForRange (string name, UtcDateTimeRange range, out DateTime start, out UtcDateTimeRange limit)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      if (name.EndsWith ("+")) {
        Debug.Assert (range.Upper.HasValue);
        if (!range.Upper.HasValue) {
          log.ErrorFormat ("PreProcessName: dynamic time {0} with the + suffix and no upper bound in {1}", name, range);
          throw new ArgumentException ("incompatible name and range");
        }
        if (range.UpperInclusive) {
          start = range.Upper.Value;
        }
        else {
          start = range.Upper.Value.Subtract (TimeSpan.FromSeconds (1));
        }
        limit = new UtcDateTimeRange ("(,)");
        return name.Substring (0, name.Length - 1);
      }
      else { // Default => lower
        Debug.Assert (range.Lower.HasValue);
        start = range.Lower.Value;
        limit = new UtcDateTimeRange (new LowerBound<DateTime> (null), range.Upper, false, true);
        return name;
      }
    }

    /// <summary>
    /// Check if a dynamic time is applicable, without specifying any date/time
    /// </summary>
    /// <param name="extension">not null and initialized with the machine</param>
    /// <returns></returns>
    public static bool IsApplicable (IDynamicTimeExtension extension)
    {
      Debug.Assert (null != extension);
      Debug.Assert (null != extension.Machine);

      var request = new DynamicTimeApplicable (extension);
      return ServiceProvider.Get<bool> (request);
    }

    /// <summary>
    /// Check if a dynamic time is applicable, without specifying any date/time
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public static bool IsApplicable (string name, IMachine machine)
    {
      var (nameWithoutParameter, parameter) = ExtractNameParameter (name);
      return IsDynamicTimeApplicable (nameWithoutParameter, parameter, machine);
    }

    /// <summary>
    /// Check if a dynamic time is applicable, without specifying any date/time
    /// </summary>
    /// <param name="nameWithoutParameter">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="parameter"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    static bool IsDynamicTimeApplicable (string nameWithoutParameter, string parameter, IMachine machine)
    {
      var extensions = GetExtensions (nameWithoutParameter, parameter, machine);

      if (!extensions.Any ()) {
        log.ErrorFormat ("IsDynamicTimeApplicable: no dynamic time extension with name {0}", nameWithoutParameter);
        var logger = GetDynamicTimeLogger (nameWithoutParameter, machine);
        if (logger.IsFatalEnabled) {
          logger.Fatal ("no dynamic time with that name");
        }
        var message = "No DynamicTime with name " + nameWithoutParameter;
        if (!string.IsNullOrEmpty (parameter)) {
          message += "(" + parameter + ")";
        }
        throw new NoDynamicTime (message);
      }
      else if (1 == extensions.Count ()) { // A unique extension
        var extension = extensions.First ();
        return IsApplicable (extension);
      }
      else { // More than 1 extension
        log.WarnFormat ("IsDynamicTimeApplicable: more than one dynamic end with name {0}", nameWithoutParameter);
        return extensions
          .Any (extension => IsApplicable (extension));
      }
    }


    /// <summary>
    /// Check if a dynamic time is applicable at a specified date/time
    /// </summary>
    /// <param name="extension">not null and initialized with the machine</param>
    /// <param name="at"></param>
    /// <returns></returns>
    static DynamicTimeApplicableStatus IsApplicableAt (IDynamicTimeExtension extension, DateTime at)
    {
      Debug.Assert (null != extension);
      Debug.Assert (null != extension.Machine);

      var request = new DynamicTimeApplicableAt (extension, at);
      return ServiceProvider.Get<DynamicTimeApplicableStatus> (request);
    }

    /// <summary>
    /// Check if a dynamic time is applicable at a specified date/time
    /// </summary>
    /// <param name="name">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public static DynamicTimeApplicableStatus IsApplicableAt (string name, IMachine machine, DateTime at)
    {
      var (nameWithoutParameter, parameter) = ExtractNameParameter (name);
      return IsApplicableAt (nameWithoutParameter, parameter, machine, at);
    }

    /// <summary>
    /// Check if a dynamic time is applicable at a specified date/time
    /// </summary>
    /// <param name="nameWithoutParameter">Name of the dynamic time (See Devel.StandardDynamicTimes for standard dynamic times)</param>
    /// <param name="parameter"></param>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    static DynamicTimeApplicableStatus IsApplicableAt (string nameWithoutParameter, string parameter, IMachine machine, DateTime at)
    {
      var extensions = GetExtensions (nameWithoutParameter, parameter, machine);
      if (!extensions.Any ()) {
        log.ErrorFormat ("IsApplicableAt: no dynamic time extension with name {0}", nameWithoutParameter);
        var logger = GetDynamicTimeLogger (nameWithoutParameter, machine);
        if (logger.IsFatalEnabled) {
          logger.Fatal ("no dynamic time with that name");
        }
        var message = "No DynamicTime with name " + nameWithoutParameter;
        if (!string.IsNullOrEmpty (parameter)) {
          message += "(" + parameter + ")";
        }
        throw new NoDynamicTime (message);
      }
      else {
        return IsApplicableAt (extensions, machine, at);
      }
    }

    /// <summary>
    /// Check if a dynamic time is applicable at a specified date/time
    /// </summary>
    /// <param name="extensions"></param>
    /// <param name="machine"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public static DynamicTimeApplicableStatus IsApplicableAt (IEnumerable<IDynamicTimeExtension> extensions, IMachine machine, DateTime at)
    {
      if (!extensions.Any ()) {
        log.Error ("IsApplicableAt: no dynamic time extension");
        throw new NoDynamicTime ("No extension in parameter");
      }
      else if (1 == extensions.Count ()) { // A unique extension
        var extension = extensions.First ();
        return IsApplicableAt (extension, at);
      }
      else { // More than 1 extension
        log.WarnFormat ("IsApplicableAt: more than one dynamic end with name {0}", extensions.First ().Name);
        var results = extensions
          .Select (extension => IsApplicableAt (extension, at));
        var firstResult = results.First ();
        if (results.All (x => x.Equals (firstResult))) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("IsApplicableAt: same result in all the extensions: {0}", firstResult);
          }
          return firstResult;
        }
        if (results.Any (x => x.Equals (DynamicTimeApplicableStatus.Pending))) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("IsApplicableAt: one extension returned Pending => Pending");
          }
          return DynamicTimeApplicableStatus.Pending;
        }
        if (results.Any (x => x.Equals (DynamicTimeApplicableStatus.Always) || x.Equals (DynamicTimeApplicableStatus.YesAtDateTime))) {
          return DynamicTimeApplicableStatus.YesAtDateTime;
        }
        if (results.All (x => x.Equals (DynamicTimeApplicableStatus.Never) || x.Equals (DynamicTimeApplicableStatus.NoAtDateTime))) {
          return DynamicTimeApplicableStatus.NoAtDateTime;
        }
        log.FatalFormat ("IsApplicableAt: one unexpected case");
        Debug.Assert (false);
        throw new InvalidProgramException ();
      }
    }
  }
}
