// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.I18N;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_AspService.Services
{
  public class DefaultCatalog : ICatalog
  {
    readonly ICatalog m_catalog;

    public DefaultCatalog ()
    {
      var multiCatalog = new MultiCatalog ();
      multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                            new TextFileCatalog ("WebServiceI18N",
                                                                 Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
      multiCatalog.Add (new DefaultTextFileCatalog ());
      m_catalog = new CachedCatalog (multiCatalog);
    }

    public string GetString (string key, CultureInfo cultureInfo)
    {
      return m_catalog.GetString (key, cultureInfo);
    }

    public string GetTranslation (string key, CultureInfo cultureInfo)
    {
      return m_catalog.GetTranslation (key, cultureInfo);
    }
  }
}
