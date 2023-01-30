// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.Config
{
  /// <summary>
  /// StampingConfigFromFile
  /// </summary>
  public class StampingConfigFromFile: IStampingConfigFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingConfigFromFile).FullName);

    /// <summary>
    /// File path of the stamping config file
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingConfigFromFile (string filePath)
    {
      this.FilePath = filePath;
    }

    /// <summary>
    /// <see cref="IStampingConfigFactory"/>
    /// </summary>
    /// <returns></returns>
    public StampingConfig CreateStampingConfig ()
    {
      try {
        var json = File.ReadAllText (this.FilePath);
        var stampingConfig = System.Text.Json.JsonSerializer.Deserialize<StampingConfig> (json);
        if (stampingConfig is null) {
          throw new NullReferenceException ("stampingConfig is null");
        }
        else {
          return stampingConfig;
        }
      }
      catch (Exception ex) {
        log.Error ($"CreateStampingConfig: error when trying to get a configuration from file {this.FilePath}", ex);
        throw;
      }
    }
  }
}
