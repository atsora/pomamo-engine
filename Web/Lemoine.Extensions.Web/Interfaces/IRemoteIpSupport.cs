// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// To specify a <see cref="IHandler"/> supports remote a request remote IP
  /// </summary>
  public interface IRemoteIpSupport
  {
    /// <summary>
    /// Set the remote IP
    /// </summary>
    /// <param name="remoteIp"></param>
    void SetRemoteIp (string remoteIp);
  }
}
