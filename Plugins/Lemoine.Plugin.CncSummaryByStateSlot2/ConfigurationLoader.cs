// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  class ConfigurationLoader: Lemoine.Extensions.Configuration.ConfigurationLoaderWithXml<Configuration>
  {
    public static readonly string CNC_FIELD_IDS_KEY = "fields";
   
    /// <summary>
    /// Load the configuration from an XML file as a SerialiableProperties
    /// </summary>
    /// <param name="xml"></param>
    protected override Configuration LoadXmlConfiguration (string xml)
    {
      SerializableProperties properties = new SerializableProperties ();
      properties.LoadFromText (xml);

      Configuration configuration = new Configuration ();

      { // Set-up
        string[] fields = properties.GetValue (CNC_FIELD_IDS_KEY).Split (new char[] { ',' });
        IList<int> setupIds = new List<int> ();
        foreach (var setup in fields) {
          int setupId;
          if (int.TryParse (setup, out setupId)) {
            setupIds.Add (setupId);
          }
        }
        configuration.CncFieldIds = setupIds;
      }

      return configuration;
    }
  }
}
