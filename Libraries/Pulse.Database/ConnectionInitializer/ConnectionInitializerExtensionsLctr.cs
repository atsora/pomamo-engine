// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lemoine.Core.Hosting.LctrChecker;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;

namespace Pulse.Database.ConnectionInitializer
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.Interfaces.IConnectionInitializer"/>
  /// with NHibernate extensions for services that are on lctr
  /// </summary>
  public class ConnectionInitializerExtensionsLctr
    : ConnectionInitializerWithNHibernateExtensions
    , Lemoine.ModelDAO.Interfaces.IConnectionInitializer
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionInitializerExtensionsLctr (IConnectionInitializer connectionInitializer, Lemoine.Extensions.Interfaces.IExtensionsProvider extensionsProvider)
      : base (connectionInitializer, extensionsProvider, new FileRepoClientFactoryNotImplemented (), new ForceLctr ())
    {
    }
  }
}
