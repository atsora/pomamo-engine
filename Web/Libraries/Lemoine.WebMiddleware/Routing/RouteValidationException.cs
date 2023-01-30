// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.WebMiddleware.Routing
{
  public class RouteValidationException : Exception
  {
    public RouteValidationException (string message) : base (message)
    {
    }

    public RouteValidationException (string message, Exception innerException) : base (message, innerException)
    {
    }
  }
}
