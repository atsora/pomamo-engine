// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Text.Json.Serialization;
using Lemoine.Cnc.OpcUaClientService.Configuration;
using Lemoine.Cnc.OpcUaClientService.Services;
using Lemoine.Cnc.OpcUaClientService.Web;

var builder = WebApplication.CreateBuilder (new WebApplicationOptions {
  Args = args,
  ContentRootPath = AppContext.BaseDirectory
});

// The logs go to the journal on Linux and to the event log on Windows: both hosts add the provider
// they need. Microsoft.Extensions.Logging is used everywhere in this service, and its license, MIT,
// suits the GPL-2.0 of this project, unlike log4net.
builder.Host.UseWindowsService (options => options.ServiceName = "Lem_OpcUaClientService");
builder.Host.UseSystemd ();

// Not appsettings.json, which every service of the product would share in the install directory
builder.Configuration.AddJsonFile (Path.Combine (AppContext.BaseDirectory, OpcUaClientServiceOptions.FileName),
  optional: true, reloadOnChange: false);

var section = builder.Configuration.GetSection (OpcUaClientServiceOptions.SectionName);
builder.Services.AddOptions<OpcUaClientServiceOptions> ().Bind (section);
builder.WebHost.UseUrls (section.Get<OpcUaClientServiceOptions> ()?.Url ?? new OpcUaClientServiceOptions ().Url);
builder.Services.AddSingleton<AcquisitionManager> ();
builder.Services.ConfigureHttpJsonOptions (options => {
  // The acquisition modules expect the members of the response to keep their name
  options.SerializerOptions.PropertyNamingPolicy = null;
  options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

var app = builder.Build ();

// The OPC UA client module is not created by the dependency injection: it is told once where to
// get its loggers from, so that its own logs land in the same journal as the ones of this service
Lemoine.Cnc.OpcUaClientLogging.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory> ();

app.MapOpcUaEndpoints ();

app.Run ();
