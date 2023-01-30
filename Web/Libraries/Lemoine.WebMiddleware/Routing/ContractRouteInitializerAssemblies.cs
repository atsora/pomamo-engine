// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Assemblies;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.WebMiddleware.Routing
{
  /// <summary>
  /// IContractRouteInitializer from several assemblies
  /// </summary>
  public class ContractRouteInitializerAssemblies: IContractRouteInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ContractRouteInitializerAssemblies).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ContractRouteInitializerAssemblies (IServiceAssembliesResolver serviceAssembliesFactory)
    {
      var assemblies = serviceAssembliesFactory.GetServiceAssemblies ();
      this.Mapper = new RouteMapper ();
      this.ContractsType = new List<Type> ();
      foreach (var assembly in assemblies) {
        BuildContractStore (assembly);
      }
    }

    public RouteMapper Mapper { get; }

    public IEnumerable<Type> ContractsType { get; private set; }
    #endregion // Constructors

    void BuildContractStore (Assembly contractsAssembly)
    {
      var contracts = FindContracts (contractsAssembly);
      this.ContractsType = this.ContractsType.Concat (contracts);
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
