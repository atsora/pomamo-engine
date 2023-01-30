// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Web;

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Description of TestService
  /// </summary>
  public class TestService
    : GenericNoCacheService<TestRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TestService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public TestService ()
      : base ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(TestRequestDTO request)
    {
      return new TestResponseDTO ();
    }
    #endregion // Methods
  }

  /// <summary>
  /// Description of TestService
  /// </summary>
  public class TestAuthorizeService
    : GenericNoCacheService<TestAuthorizeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TestAuthorizeService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public TestAuthorizeService ()
      : base ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (TestAuthorizeRequestDTO request)
    {
      return new TestResponseDTO ();
    }
    #endregion // Methods
  }
}
