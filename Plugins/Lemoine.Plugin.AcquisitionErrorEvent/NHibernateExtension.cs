// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
#if USE_FLUENT_NHIBERNATE
using FluentNHibernate.Cfg;
#endif // USE_FLUENT_NHIBERNATE
using Lemoine.Core.Log;

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  /// <summary>
  /// Description of NHibernateExtension.
  /// </summary>
  public class NHibernateExtension : Lemoine.Extensions.Database.INHibernateExtension
  {
#region Members
#endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (NHibernateExtension).FullName);

#region Getters / Setters
#endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public NHibernateExtension ()
    {
    }
#endregion // Constructors

#region INHibernateExtension implementation

    public bool ContainsMapping ()
    {
#if USE_FLUENT_NHIBERNATE
      return false;
#else // !USE_FLUENT_NHIBERNATE
      return true;
#endif // USE_FLUENT_NHIBERNATE
    }

    public void UpdateConfiguration (ref NHibernate.Cfg.Configuration configuration)
    {
#if USE_FLUENT_NHIBERNATE
      configuration = Fluently
        .Configure (configuration)
        .Mappings (m => m.FluentMappings.Add<EventAcquisitionErrorMap> ())
        .BuildConfiguration ();
#endif // USE_FLUENT_NHIBERNATE
    }
#endregion

#region IExtension implementation

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

#endregion
  }
}
