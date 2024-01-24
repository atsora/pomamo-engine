// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.AsyncProcess;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Graphql.Type;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Class to test a cnc acquisition configuration
  /// </summary>
  public class TestCncAcquisition
  {
    /// <summary>
    /// Check the configuration file exists first
    /// </summary>
    static readonly string CHECK_CONFIG_FILE_KEY = "Graphql.CncAcquisition.CheckConfigFile";
    static readonly bool CHECK_CONFIG_FILE_DEFAULT = true;

    /// <summary>
    /// Activate the new configuration key params
    /// </summary>
    static readonly string KEY_PARAMS_KEY = "Graphql.CncAcquisition.KeyParams";
    static readonly bool KEY_PARAMS_DEFAULT = true;

    /// <summary>
    /// Name of the Cnc Console application to consider
    /// </summary>
    static readonly string CNC_CONSOLE_APPLICATION_KEY = "Graphql.CncAcquisition.CncConsoleApp";
    static readonly string CNC_CONSOLE_APPLICATION_DEFAULT =
#if CONNECTOR
      "AconnectorCncConsole.exe";
#else
      "AtrackingCncConsole.exe";
#endif

    /// <summary>
    /// Name of the Open Cnc Console application to consider
    /// </summary>
    static readonly string OPEN_CNC_CONSOLE_APPLICATION_KEY = "Graphql.CncAcquisition.OpenCncConsoleApp";
    static readonly string OPEN_CNC_CONSOLE_APPLICATION_DEFAULT =
#if CONNECTOR
      "AconnectorOpenCncConsole.exe";
#else
      "Lem_CncConsole.exe";
#endif

    /// <summary>
    /// Directory where the cnc console application programs are located
    /// </summary>
    static readonly string CNC_CONSOLE_DIRECTORY_KEY = "Directory.CncConsoleApps";
    static readonly string CNC_CONSOLE_DIRECTORY_DEFAULT =
#if CONNECTOR
      Lemoine.Info.ProgramInfo.AbsoluteDirectory; // "C:\\Program Files (x86)\\Aconnector";
#else
      "C:\\Program Files (x86)\\AtsoraTracking Acquisition";
    // TODO: use registry keys to find the installation directory of AtsoraTracking Acquisition
#endif

    /// <summary>
    /// Filter the cnc data
    /// </summary>
    static readonly string FILTER_KEY = "Graphql.CncAcquisition.Filter";
    static readonly string FILTER_DEFAULT = "MachineModeActive,Feedrate,RapidTraverse,RapidTraverseRate,SpindleLoad,SpindleSpeed,FeedrateOverride,SpindleSpeedOverride,SpindleSpeedOverride,ProgramName,SubProgramName,ProgramComment,SubProgramComment,CncPartCount,CncModes,Hold,SingleBlock,ToolNumber,PalletNumber,PalletReady,StackLight,PartCode,CycleTimeTotalSeconds,Alarms,AcquisitionError,PingOk,AddressNotValid,ConnectionErrorMessage";

    /// <summary>
    /// Replace instruction for the cnc data
    /// </summary>
    static readonly string REPLACE_KEY = "Graphql.CncAcquisition.Replace";
    static readonly string REPLACE_DEFAULT =
#if CONNECTOR
      "MachineModeActive:Active";
#else
      "";
#endif

    readonly ILog log = LogManager.GetLogger<TestCncAcquisition> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public TestCncAcquisition ()
    {
    }

    /// <summary>
    /// Cnc config name
    /// </summary>
    public string? CncConfigName { get; set; }

    /// <summary>
    /// Parameters
    /// </summary>
    public IList<CncConfigParamValueInput> Parameters { get; set; } = new List<CncConfigParamValueInput> ();

    /// <summary>
    /// Test a cnc acquisition configuration
    /// </summary>
    /// <returns></returns>
    public async Task<CncAcquisitionTestResponse?> TestAsync ()
    {
      try {
        var cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
        if (log.IsDebugEnabled) {
          log.Debug ($"TestAsync: configName={this.CncConfigName} parameters={this.Parameters}");
        }
        cncAcquisition.ConfigFile = this.CncConfigName + ".xml";
        if (Lemoine.Info.ConfigSet.LoadAndGet (KEY_PARAMS_KEY, KEY_PARAMS_DEFAULT)) {
          cncAcquisition.ConfigKeyParams = CncConfigParamValueInput.GetKeyParams (this.Parameters);
        }
        else {
          cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
        }
        var response = new CncAcquisitionTestResponse (cncAcquisition);
        if (Lemoine.Info.ConfigSet.LoadAndGet (CHECK_CONFIG_FILE_KEY, CHECK_CONFIG_FILE_DEFAULT)
          && !response.CheckParameters (this.Parameters.ToDictionary (x => x.Name, x => x.Value))) {
          log.Error ("TestAsync: load error or invalid parameters");
          response.Aborted = true;
          return response;
        }

        var cncConfig = response.CncConfig;
        var application = cncConfig.HasLemoineModule
          ? Lemoine.Info.ConfigSet.LoadAndGet (OPEN_CNC_CONSOLE_APPLICATION_KEY, OPEN_CNC_CONSOLE_APPLICATION_DEFAULT)
          : Lemoine.Info.ConfigSet.LoadAndGet (CNC_CONSOLE_APPLICATION_KEY, CNC_CONSOLE_APPLICATION_DEFAULT);
        var directory = Lemoine.Info.ConfigSet.LoadAndGet (CNC_CONSOLE_DIRECTORY_KEY, CNC_CONSOLE_DIRECTORY_DEFAULT);
        var path = Path.Combine (directory, application);
        if (!File.Exists (path)) {
          path = Path.Combine (directory, "netframework", application);
          if (!File.Exists (path)) {
            path = Path.Combine (directory, "core", application);
          }
        }
        var jsonParameter = cncAcquisition.GetKeyParamsJson ();
        var escapedJsonParameter = jsonParameter.Replace ("\"", "\\\"");
        var filter = Lemoine.Info.ConfigSet.LoadAndGet (FILTER_KEY, FILTER_DEFAULT);
        var replace = Lemoine.Info.ConfigSet.LoadAndGet (REPLACE_KEY, REPLACE_DEFAULT);
        var arguments = $"-i 0 -f {cncAcquisition.ConfigFile} -j \"{escapedJsonParameter}\" -c 2 -k Cnc.Stdout.Json=True Cnc.Stdout.Replace={replace} Cnc.Stdout.Filter={filter}";
        var processResult = await RunCommandAsync (directory, path, arguments, null, null);
        if ((0 != processResult.ExitCode) && log.IsErrorEnabled) {
          log.Error ($"TestAsync: exit code={processResult.ExitCode} for {path} {arguments}");
        }
        if (log.IsInfoEnabled) {
          log.Info ($"TestAsync: standard output for {path} {arguments} is: {processResult.StandardOutput}");
        }
        if (!string.IsNullOrEmpty (processResult.StandardError) && log.IsErrorEnabled) {
          log.Error ($"TestAsync: standard error for {path} {arguments}: {processResult.StandardError}");
        }
        response.CncData = processResult.StandardOutput.Trim ().Trim (['\r', '\n']);
        response.ExitCode = processResult.ExitCode;

        return response;
      }
      catch (Exception ex) {
        log.Error ($"TestAsync: exception", ex);
        throw;
      }
    }

    public async Task<ProcessResult> RunCommandAsync (string directory, string program, string arguments, TextWriter? outStreamWriter = null, TextWriter? errStreamWriter = null)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo ();
      startInfo.FileName = program;
      startInfo.Arguments = arguments;
      startInfo.WorkingDirectory = directory;
      startInfo.UseShellExecute = false;
      startInfo.CreateNoWindow = true;
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.RedirectStandardOutput = true;
      startInfo.RedirectStandardError = true;

      return await startInfo.RunProcessAsync (outStreamWriter, errStreamWriter);
    }
  }

  /// <summary>
  /// Input graphql type for a new cnc acquisition
  /// </summary>
  public class TestCncAcquisitionInputType : InputObjectGraphType<TestCncAcquisition>
  {
    readonly ILog log = LogManager.GetLogger (typeof (TestCncAcquisitionInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TestCncAcquisitionInputType ()
    {
      Name = "TestCncAcquisition";
      Field<string> ("cncConfigName", nullable: false);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueInputType>>>> ("parameters");
    }
  }
}
