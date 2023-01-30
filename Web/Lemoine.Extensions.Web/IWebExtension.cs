// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web
{
  /// <summary>
  /// Web extension
  /// </summary>
  public interface IWebExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Return a reference to the assembly that contains the web services
    /// </summary>
    /// <returns></returns>
    System.Reflection.Assembly GetAssembly ();
  }
}
