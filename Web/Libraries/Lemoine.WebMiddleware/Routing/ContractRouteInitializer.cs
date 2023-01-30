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

namespace Lemoine.WebMiddleware.Routing
{
  public class ContractRouteInitializer : IContractRouteInitializer
  {
    public RouteMapper Mapper { get; } = new RouteMapper ();
    public IEnumerable<Type> ContractsType { get; private set; }

    public ContractRouteInitializer (Assembly contractsAssembly)
    {
      var contracts = FindContracts (contractsAssembly);
      ContractsType = contracts;
      foreach (var contract in contracts) {
        this.AddContract (contract);
      }
    }

    List<Type> FindContracts (Assembly contractsAssembly)
    {
      List<Type> routeContracts = new List<Type> ();
      foreach (Type t in contractsAssembly.GetTypes ()) {
        if (t.IsDefined (typeof (RouteAttribute))) {
          routeContracts.Add (t);
        }
      }
      return routeContracts;
    }
  }
}
