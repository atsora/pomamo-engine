// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.WebMiddleware.Assemblies;
using Lemoine.WebMiddleware.Handlers;
using Lemoine.WebMiddleware.Routing;
using NUnit.Framework;
using Pulse.Web.Signal;

namespace Pulse.Web.UnitTests.Signal
{
  /// <summary>
  /// Check the /Signal/ end point is registered in the routes of the web middleware
  /// and associated to <see cref="SignalService"/>.
  /// 
  /// This is a regression test for the 404 errors that are returned by the routing
  /// middleware when the request DTO is not discovered in the service assemblies.
  /// </summary>
  public class SignalRoute_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (SignalRoute_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SignalRoute_UnitTest ()
    { }

    /// <summary>
    /// Resolver on the assembly that contains the signal web service,
    /// as it is registered in the asp service
    /// </summary>
    static IServiceAssembliesResolver CreateAssembliesResolver () =>
      new ServiceAssembliesFromList (new List<Assembly> { typeof (SignalRequestDTO).Assembly });

    /// <summary>
    /// Test the routes that are declared on <see cref="SignalRequestDTO"/>
    /// </summary>
    [Test]
    public void TestDeclaredRoutes ()
    {
      var routes = typeof (SignalRequestDTO)
        .GetCustomAttributes<RouteAttribute> ()
        .ToList ();

      Assert.Multiple (() => {
        Assert.That (routes.Select (r => r.Endpoint), Does.Contain ("/Signal/"));
        Assert.That (routes.Select (r => r.Endpoint), Does.Contain ("/Signal/Get/{GroupId}"));
        Assert.That (routes.Select (r => r.Method), Has.All.EqualTo ("GET"));
      });
    }

    /// <summary>
    /// Test the request DTO is discovered in the assembly of the web services
    /// </summary>
    [Test]
    public void TestContractIsDiscovered ()
    {
      var routeInitializer = new ContractRouteInitializerAssemblies (CreateAssembliesResolver ());

      Assert.That (routeInitializer.ContractsType, Does.Contain (typeof (SignalRequestDTO)));
    }

    /// <summary>
    /// Test the requested paths are mapped to <see cref="SignalRequestDTO"/>
    /// and not answered with a 404 error
    /// </summary>
    /// <param name="path"></param>
    [TestCase ("/Signal/")]
    [TestCase ("/Signal")]
    [TestCase ("/Signal/Get/ALL")]
    public void TestPathIsMapped (string path)
    {
      var routeInitializer = new ContractRouteInitializerAssemblies (CreateAssembliesResolver ());

      var (contractType, pathParameters, _) = routeInitializer.Mapper
        .GetMatchingContractType (path, "GET");

      Assert.Multiple (() => {
        Assert.That (contractType, Is.EqualTo (typeof (SignalRequestDTO)));
        Assert.That (pathParameters, Is.Not.Null);
      });
    }

    /// <summary>
    /// Test the group id is extracted from the path of the /Signal/Get/{GroupId} route
    /// </summary>
    [Test]
    public void TestGroupIdPathParameter ()
    {
      var routeInitializer = new ContractRouteInitializerAssemblies (CreateAssembliesResolver ());

      var (_, pathParameters, _) = routeInitializer.Mapper
        .GetMatchingContractType ("/Signal/Get/ALL", "GET");

      Assert.That (pathParameters, Is.Not.Null);
      var groupIdParameter = pathParameters.Single ();
      Assert.Multiple (() => {
        Assert.That (groupIdParameter.MappedPropertyName, Is.EqualTo ("GroupId"));
        Assert.That (groupIdParameter.ParameterValue, Is.EqualTo ("ALL"));
      });
    }

    /// <summary>
    /// Test an unknown verb is not mapped
    /// </summary>
    [Test]
    public void TestPostIsNotMapped ()
    {
      var routeInitializer = new ContractRouteInitializerAssemblies (CreateAssembliesResolver ());

      var (contractType, _, _) = routeInitializer.Mapper
        .GetMatchingContractType ("/Signal/", "POST");

      Assert.That (contractType, Is.Null);
    }

    /// <summary>
    /// Test <see cref="SignalService"/> is the handler that is associated
    /// to <see cref="SignalRequestDTO"/>
    /// </summary>
    [Test]
    public void TestHandlerIsSignalService ()
    {
      var assembliesResolver = CreateAssembliesResolver ();
      var routeInitializer = new ContractRouteInitializerAssemblies (assembliesResolver);
      var handlerMapper = new HandlerMapperInitializer (routeInitializer, assembliesResolver);

      Assert.That (handlerMapper.ContractToHandler.ContainsKey (typeof (SignalRequestDTO)), Is.True);
      var (handlerType, method) = handlerMapper.ContractToHandler[typeof (SignalRequestDTO)];
      Assert.Multiple (() => {
        Assert.That (handlerType, Is.EqualTo (typeof (SignalService)));
        Assert.That (method.Name, Is.EqualTo ("Get"));
      });
    }
  }
}
