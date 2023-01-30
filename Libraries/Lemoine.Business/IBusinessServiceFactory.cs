// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Business
{
  /// <summary>
  /// Factory class
  /// </summary>
  public interface IBusinessServiceFactory
  {
    /// <summary>
    /// Create the business service
    /// </summary>
    /// <returns></returns>
    IService CreateBusiness ();
  }
}
