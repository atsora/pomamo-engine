// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.SetupSwitcher
{
  class ConfigurationLoader : Lemoine.Extensions.Configuration.ConfigurationLoaderWithXml<Configuration>
  {
    public static readonly string SETUP_MACHINE_STATE_TEMPLATE_ID_KEY = "SetupMachineStateTemplateId";

    /// <summary>
    /// Load the configuration from an XML file as a SerialiableProperties
    /// </summary>
    /// <param name="xml"></param>
    protected override Configuration LoadXmlConfiguration (string xml)
    {
      SerializableProperties properties = new SerializableProperties ();
      properties.LoadFromText (xml);

      Configuration configuration = new Configuration ();
      
      var setupMachineStateTemplateIdString = properties.GetValue (SETUP_MACHINE_STATE_TEMPLATE_ID_KEY);
      if (null != setupMachineStateTemplateIdString) {
        int setupMachineStateTemplateId;
        if (int.TryParse (setupMachineStateTemplateIdString, out setupMachineStateTemplateId)) {
          configuration.SetupMachineStateTemplateId = setupMachineStateTemplateId;
        }
      }
      
      return configuration;
    }
  }
}
