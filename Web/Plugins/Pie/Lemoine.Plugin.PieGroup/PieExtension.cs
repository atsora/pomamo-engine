// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Pulse.Extensions.Web;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.PieGroup
{
  public class PieExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IPieExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (PieExtension).FullName);

    Configuration m_configuration;

    public double Score
    {
      get { return m_configuration.Score; }
    }

    public string PieType
    {
      get { return m_configuration.PieType; }
    }

    public bool Permanent
    {
      get { return true; }
    }

    public bool Initialize (IGroup group)
    {
      if (null == group) {
        log.Fatal ("Initialize: empty group");
        Debug.Assert (false);
        return false;
      }

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: configuration error");
        return false;
      }

      var regex = new Regex ("^" + m_configuration.GroupRegex + "$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
      if (!regex.IsMatch (group.Id)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Initialize: group id {0} does not match regex {1}", group.Id, m_configuration.GroupRegex);
        }
        return false;
      }

      return true;
    }
  }
}
