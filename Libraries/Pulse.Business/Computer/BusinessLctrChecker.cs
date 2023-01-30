// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;

namespace Pulse.Business.Computer
{
  /// <summary>
  /// Class that implements <see cref="ILctrChecker"/> using the business layer
  /// </summary>
  public class BusinessLctrChecker : ILctrChecker, IValidServerChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (BusinessLctrChecker).FullName);

    Lemoine.Business.IService m_service;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service"></param>
    public BusinessLctrChecker (Lemoine.Business.IService service)
    {
      Debug.Assert (null != service);

      m_service = service;
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool IsLctr ()
    {
      return m_service.Get (new Lemoine.Business.Computer.IsLctr ());
    }

    /// <summary>
    /// <see cref="IValidServerChecker"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool IsValidServerForService ()
    {
      return IsLctr ();
    }
  }
}
