// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.DataRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lem_SynchronizationService
{
  public class DummyFactory : IFactory
  {
    public bool CheckSynchronizationOkAction ()
    {
      throw new NotImplementedException ();
    }

    public void FlagSynchronizationAsFailure (XmlDocument document)
    {
      throw new NotImplementedException ();
    }

    public void FlagSynchronizationAsSuccess (XmlDocument document)
    {
      throw new NotImplementedException ();
    }

    public XmlDocument GetData (CancellationToken cancellationToken, bool optional = false)
    {
      throw new NotImplementedException ();
    }
  }
}
