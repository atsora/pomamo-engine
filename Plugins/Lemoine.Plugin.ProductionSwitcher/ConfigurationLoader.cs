// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.ProductionSwitcher
{
  class ConfigurationLoader : Lemoine.Extensions.Configuration.ConfigurationLoaderWithXml<Configuration>
  {
    public static readonly string CYCLE_DURATION_PERCENTAGE_KEY = "CycleDurationPercentageTrigger";
    public static readonly string BETWEEN_CYCLES_DURATION_PERCENTAGE_KEY = "BetweenCyclesDurationPercentageTrigger";
    public static readonly string SETUP_MACHINE_STATE_TEMPLATE_IDS_KEY = "SetupMachineStateTemplateId";
    public static readonly string PRODUCTION_MACHINE_STATE_TEMPLATE_ID_KEY = "ProductionMachineStateTemplateId";

    /// <summary>
    /// Load the configuration from an XML file as a SerialiableProperties
    /// </summary>
    /// <param name="xml"></param>
    protected override Configuration LoadXmlConfiguration (string xml)
    {
      SerializableProperties properties = new SerializableProperties ();
      properties.LoadFromText (xml);

      Configuration configuration = new Configuration ();

      { // Cycle duration limit %
        string p = properties.GetValue (CYCLE_DURATION_PERCENTAGE_KEY);
        int percentage;
        if (int.TryParse (p, out percentage)) {
          configuration.CycleDurationPercentageTrigger = percentage;
        }
      }
      { // Cycle duration limit %
        string p = properties.GetValue (BETWEEN_CYCLES_DURATION_PERCENTAGE_KEY);
        int percentage;
        if (int.TryParse (p, out percentage)) {
          configuration.BetweenCyclesDurationPercentageTrigger = percentage;
        }
      }
      { // Set-up
        string[] setups = properties.GetValue (SETUP_MACHINE_STATE_TEMPLATE_IDS_KEY).Split (new char[] { ',' });
        foreach (var setup in setups) {
          int setupId;
          if (int.TryParse (setup, out setupId)) {
            configuration.SetupMachineStateTemplateIds.Add (setupId);
          }
        }
      }
      { // Production
        string p = properties.GetValue (PRODUCTION_MACHINE_STATE_TEMPLATE_ID_KEY);
        if (!string.IsNullOrEmpty (p)) {
          int id;
          if (int.TryParse (p, out id)) {
            configuration.ProductionMachineStateTemplateId = id;
          }
        }
      }

      return configuration;
    }
  }
}
