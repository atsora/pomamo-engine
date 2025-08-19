// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.DynamicTimes
{
  /// <summary>
  /// Dynamic time business response
  /// </summary>
  public interface IDynamicTimeResponse
  {
    /// <summary>
    /// If set, returned Dynamic time
    /// </summary>
    DateTime? Final { get; }

    /// <summary>
    /// Returns a date/time hint range where the final result will be
    /// </summary>
    UtcDateTimeRange Hint { get; }

    /// <summary>
    /// No dynamic time could be found in the limit (interrupted)
    /// </summary>
    bool NoData { get; set; }

    /// <summary>
    /// This dynamic time is not applicable for this machine
    /// 
    /// When NotApplicable is true, NoData is true too
    /// </summary>
    bool NotApplicable { get; }

    /// <summary>
    /// The request ended in timeout
    /// 
    /// When Timeout is true, NoData is true too
    /// </summary>
    bool Timeout { get; }

    /// <summary>
    /// Implementation type
    /// </summary>
    Type ImplementationType { get; }
  }

  public static class DynamicTimeResponseExtensions
  {
    /// <summary>
    /// Is the response pending?
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool IsPending (this IDynamicTimeResponse response)
      => !response.NoData && !response.Timeout && !response.Final.HasValue;
  }

  /// <summary>
  /// IDynamicTimeResponse implementation
  /// 
  /// <see cref="IDynamicTimeResponse"/>
  /// </summary>
  public class DynamicTimeResponse : IDynamicTimeResponse
  {
    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public UtcDateTimeRange Hint
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public DateTime? Final
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public bool NoData
    {
      get; set;
    }

    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public bool NotApplicable
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public bool Timeout { get; private set; } = false;

    /// <summary>
    /// <see cref="IDynamicTimeResponse"/>
    /// </summary>
    public Type ImplementationType { get; private set; }

    /// <summary>
    /// Create a response with a final date/time
    /// </summary>
    /// <param name="final"></param>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateFinal (Type implementationType, DateTime final)
    {
      var response = new DynamicTimeResponse ();
      response.Final = final;
      response.Hint = new UtcDateTimeRange (final, final, "[]");
      response.NoData = false;
      response.NotApplicable = false;
      response.ImplementationType = implementationType;
      return response;
    }

    /// <summary>
    /// Create a response with a hint date/time range
    /// </summary>
    /// <param name="hint"></param>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateWithHint (Type implementationType, UtcDateTimeRange hint)
    {
      if (hint.IsEmpty ()) {
        return CreateNoData (implementationType);
      }
      else {
        var response = new DynamicTimeResponse ();
        response.Hint = hint;
        response.NoData = false;
        response.NotApplicable = false;
        response.ImplementationType = implementationType;
        return response;
      }
    }

    /// <summary>
    /// No date/time can be returned yet
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreatePending (Type implementationType)
    {
      var response = new DynamicTimeResponse ();
      response.Hint = new UtcDateTimeRange ("(,)");
      response.NoData = false;
      response.NotApplicable = false;
      response.ImplementationType = implementationType;
      return response;
    }

    /// <summary>
    /// No dynamic time could be found in the limit (interrupted)
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateNoData (Type implementationType)
    {
      var response = new DynamicTimeResponse ();
      response.Hint = new UtcDateTimeRange ();
      response.NoData = true;
      response.NotApplicable = false;
      response.ImplementationType = implementationType;
      return response;
    }

    /// <summary>
    /// No dynamic time could be found in the limit (interrupted)
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateNoData (IDynamicTimeExtension extension)
    {
      return CreateNoData (extension.GetType ());
    }

    /// <summary>
    /// This dynamic time is currently not applicable for this machine
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateNotApplicable (Type implementationType)
    {
      var response = new DynamicTimeResponse ();
      response.Hint = new UtcDateTimeRange ();
      response.NoData = true;
      response.NotApplicable = true;
      response.ImplementationType = implementationType;
      return response;
    }

    /// <summary>
    /// The dynamic time request ended in timeout
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateTimeout (Type implementationType)
    {
      var response = new DynamicTimeResponse ();
      response.Hint = new UtcDateTimeRange ();
      response.NoData = true;
      response.NotApplicable = false;
      response.Timeout = true;
      response.ImplementationType = implementationType;
      return response;
    }
  }

  /// <summary>
  /// Answer of a IsApplicable request
  /// </summary>
  public enum DynamicTimeApplicableStatus
  {
    /// <summary>
    /// The dynamic time is never applicable, whichever the time
    /// </summary>
    Never,
    /// <summary>
    /// The dynamic time is always applicable, whichever the time
    /// </summary>
    Always,
    /// <summary>
    /// The dynamic time is applicable only for the specified date/time
    /// </summary>
    YesAtDateTime,
    /// <summary>
    /// The dynamic time is not applicable only for the specified date/time
    /// </summary>
    NoAtDateTime,
    /// <summary>
    /// The status is not known yet
    /// </summary>
    Pending,
  }

  /// <summary>
  /// Extension to set new dynamic times
  /// </summary>
  public interface IDynamicTimeExtension
    : IExtension, Extension.Categorized.INamed
  {
    /// <summary>
    /// Initialize the plugin
    /// 
    /// If false is returned, the plugin is disabled
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    bool Initialize (IMachine machine, string parameter);

    /// <summary>
    /// Associated machine
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Check if the dynamic time is applicable without specifying a date/time
    /// 
    /// If the result depends on the time, return true
    /// </summary>
    /// <returns></returns>
    bool IsApplicable ();

    /// <summary>
    /// Check if the dynamic time is applicable at the specific date/time
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime);

    /// <summary>
    /// Run the request: find a dynamic time after the specified UTC date/time
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="hint">suggestion of date/time range where to look for</param>
    /// <param name="limit"></param>
    /// <returns></returns>
    IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit);

    /// <summary>
    /// Return the cache timeout
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    TimeSpan GetCacheTimeout (IDynamicTimeResponse data);
  }

  /// <summary>
  /// Extensions of interface <see cref="IDynamicTimeExtension"/>
  /// </summary>
  public static class DynamicTimeExtensionExtensions
  {
    /// <summary>
    /// Create a response with a final date/time
    /// </summary>
    /// <param name="final"></param>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateFinal (this IDynamicTimeExtension extension, DateTime final)
      => DynamicTimeResponse.CreateFinal (extension.GetType (), final);

    /// <summary>
    /// Create a response with a hint date/time range
    /// </summary>
    /// <param name="hint"></param>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateWithHint (this IDynamicTimeExtension extension, UtcDateTimeRange hint)
      => DynamicTimeResponse.CreateWithHint (extension.GetType (), hint);

    /// <summary>
    /// No date/time can be returned yet
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreatePending (this IDynamicTimeExtension extension)
      => DynamicTimeResponse.CreatePending (extension.GetType ());

    /// <summary>
    /// No dynamic time could be found in the limit (interrupted)
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateNoData (this IDynamicTimeExtension extension)
      => DynamicTimeResponse.CreateNoData (extension.GetType ());

    /// <summary>
    /// This dynamic time is currently not applicable for this machine
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateNotApplicable (this IDynamicTimeExtension extension)
      => DynamicTimeResponse.CreateNotApplicable (extension.GetType ());

    /// <summary>
    /// The dynamic time request ended in timeout
    /// </summary>
    /// <returns></returns>
    public static IDynamicTimeResponse CreateTimeout (this IDynamicTimeExtension extension)
      => DynamicTimeResponse.CreateTimeout (extension.GetType ());
  }
}
