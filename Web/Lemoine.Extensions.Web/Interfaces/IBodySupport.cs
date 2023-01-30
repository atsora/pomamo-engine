// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// To specify a <see cref="IHandler"/> supports a request body (for POST requests for example)
  /// </summary>
  public interface IBodySupport
  {
    /// <summary>
    /// Set the request body
    /// </summary>
    /// <param name="body"></param>
    void SetBody (Stream body);
  }
}
