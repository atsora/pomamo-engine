// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// To specify a <see cref="IHandler"/> forces a specific response content type
  /// </summary>
  public interface IResponseContentTypeSupport
  {
    /// <summary>
    /// Response content type to use, for example:
    /// <item>application/octet-stream</item>
    /// </summary>
    /// <returns></returns>
    string GetResponseContentType ();
  }
}
