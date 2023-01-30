// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// To specify a <see cref="IHandler"/> response is specific to a user
  /// </summary>
  public interface IUserSpecificResponseSupport
  {
    /// <summary>
    /// Get/Set a specific user for the response
    /// </summary>
#if NETCOREAPP
#nullable enable
    System.Security.Claims.ClaimsPrincipal? SpecificUser { get; set; }
#nullable restore
#else // !NETCOREAPP
    System.Security.Claims.ClaimsPrincipal SpecificUser { get; set; }
#endif // !NETCOREAPP
  }
}
