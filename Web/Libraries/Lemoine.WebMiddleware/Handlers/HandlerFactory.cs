// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Lemoine.WebMiddleware.Handlers
{
  public class HandlerFactory
  {
    private readonly Dictionary<Type, (Type, MethodInfo)> ContractToHandler;
    private readonly IServiceProvider ioc;

    public HandlerFactory (HandlerMapperInitializer initializer, IServiceProvider services)
    {
      ContractToHandler = initializer.ContractToHandler;
      this.ioc = services;
    }

    public (IHandler?, MethodInfo?) ResolveHandler (Type contract)
    {
      Type handlerType;
      MethodInfo handlingMethod;
      if (!ContractToHandler.ContainsKey (contract)) {
        return (null, null);
      }
      (handlerType, handlingMethod) = ContractToHandler[contract];
      return (ioc.GetService (handlerType) as IHandler, handlingMethod);
    }
  }
}
