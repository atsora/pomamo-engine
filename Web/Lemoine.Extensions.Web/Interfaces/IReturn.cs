// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// Interface for an input DTO
  /// </summary>
  /// <typeparam name="TResp">Output DTO</typeparam>
  public interface IReturn<TResp> //Consider restricting T  to a return type where the TResp is a HasStatusResponseCode
  {
  }
}
