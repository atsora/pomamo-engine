// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// XPath evaluation exception
  /// </summary>
  public class XPathException : RepositoryException
  {
    /// <summary>
    /// RepositoryException in case of an XPath evaluation problem
    /// 
    /// <see cref="RepositoryException"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public XPathException (string message, Exception innerException)
      : base (message, innerException)
    {
    }
  }
}
