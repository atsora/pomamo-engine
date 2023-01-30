// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using NServiceKit.ServiceInterface;

namespace Pulse.Web.User
{
  /// <summary>
  /// Description of ProductionMachiningStatusServices.
  /// </summary>
  public class UserServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to POST request User/Permissions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (UserPermissionsPostRequestDTO request)
    {
      return new UserPermissionsService ()
        .Post (request, this.Request);
    }

    /// <summary>
    /// Response to POST request User/ChangePassword
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (UserChangePasswordRequestDTO request)
    {
      return new UserChangePasswordService ()
        .Post (request, this.Request);
    }
  }
}
#endif // NSERVICEKIT
