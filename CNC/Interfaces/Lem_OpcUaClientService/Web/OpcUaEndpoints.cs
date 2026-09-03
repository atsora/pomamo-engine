// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using Lemoine.Cnc.OpcUaClientService.Services;

namespace Lemoine.Cnc.OpcUaClientService.Web
{
  /// <summary>
  /// Rest requests of the service
  ///
  /// They are the requests the CncCoreXmlPost, CncCoreGetSet and CncCoreData acquisition modules
  /// use, so that they can get their data from this service without any change
  /// </summary>
  public static class OpcUaEndpoints
  {
    /// <summary>
    /// Map all the requests of the service
    /// </summary>
    /// <param name="app">not null</param>
    public static void MapOpcUaEndpoints (this WebApplication app)
    {
      ArgumentNullException.ThrowIfNull (app);

      app.MapGet ("/ping", (AcquisitionManager manager) => Results.Json (new {
        Service = "Lem_OpcUaClientService",
        Acquisitions = manager.AcquisitionIdentifiers.ToList ()
      }));

      app.MapPost ("/xml", XmlAsync);
      app.MapGet ("/get", GetAsync);
      app.MapGet ("/set", SetAsync);
      app.MapGet ("/data", DataAsync);
    }

    /// <summary>
    /// POST /xml?acquisition=&lt;id&gt;, with the configuration of the machine in the body
    ///
    /// It configures the connection of the acquisition, and it returns all its data, which is what
    /// CncCoreXmlPost expects. The connection is initialized again whenever the posted
    /// configuration differs from the current one.
    /// </summary>
    static async Task<IResult> XmlAsync (HttpContext context, AcquisitionManager manager, ILoggerFactory loggerFactory,
      string? acquisition, CancellationToken cancellationToken)
    {
      var logger = loggerFactory.CreateLogger (typeof (OpcUaEndpoints));

      string body;
      using (var reader = new StreamReader (context.Request.Body)) {
        body = await reader.ReadToEndAsync (cancellationToken);
      }

      try {
        var configuration = XmlConfigurationParser.Parse (body);
        var session = manager.GetOrCreateSession (acquisition, configuration);
        return Results.Json (await session.ApplyConfigurationAsync (configuration, cancellationToken));
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (InvalidConfigurationException ex) {
        logger.LogError (ex, $"XmlAsync: invalid configuration for acquisition={acquisition}");
        return Results.Text (ex.Message, "text/plain", statusCode: StatusCodes.Status400BadRequest);
      }
      catch (Exception ex) {
        logger.LogInformation (ex, $"XmlAsync: acquisition={acquisition} failed");
        return Results.Text (ex.Message, "text/plain", statusCode: StatusCodes.Status503ServiceUnavailable);
      }
    }

    /// <summary>
    /// /get?acquisition=...&amp;moduleref=...&amp;method=...&amp;param=...
    /// or /get?acquisition=...&amp;moduleref=...&amp;property=...
    /// </summary>
    static async Task<IResult> GetAsync (HttpContext context, AcquisitionManager manager, ILoggerFactory loggerFactory,
      string? acquisition, string? moduleref, string? method, string? property, string? param, CancellationToken cancellationToken)
    {
      var logger = loggerFactory.CreateLogger (typeof (OpcUaEndpoints));
      var response = new SingleResponse (acquisition, moduleref, "get", method, property, param);

      var session = manager.TryGetSession (acquisition);
      if (session is null) {
        return Results.Json (response.SetError ($"Acquisition {acquisition} not found"));
      }

      try {
        if (!string.IsNullOrEmpty (property)) {
          return Results.Json (response.SetResult (await session.GetPropertyAsync (property, cancellationToken)));
        }
        if (method is null) {
          return Results.Json (response.SetError ("No method and no property in the request"));
        }
        return Results.Json (response.SetResult (await session.GetAsync (method, param ?? "", cancellationToken)));
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        logger.LogInformation (ex, $"GetAsync: acquisition={acquisition} method={method} property={property} param={param} failed");
        return Results.Json (response.SetError (ex.Message));
      }
    }

    /// <summary>
    /// /set?acquisition=...&amp;moduleref=...&amp;method=Write&amp;param=...&amp;&lt;type&gt;=...
    ///
    /// where &lt;type&gt; is long, int, double, boolean, string or v
    /// </summary>
    static async Task<IResult> SetAsync (HttpContext context, AcquisitionManager manager, ILoggerFactory loggerFactory,
      string? acquisition, string? moduleref, string? method, string? property, string? param, CancellationToken cancellationToken)
    {
      var logger = loggerFactory.CreateLogger (typeof (OpcUaEndpoints));
      var response = new SingleResponse (acquisition, moduleref, "set", method, property, param);

      var session = manager.TryGetSession (acquisition);
      if (session is null) {
        return Results.Json (response.SetError ($"Acquisition {acquisition} not found"));
      }
      if (!RequestValue.TryParse (context.Request.Query, out var v)) {
        return Results.Json (response.SetError ("No value in the request"));
      }

      try {
        if (!string.IsNullOrEmpty (property)) {
          // The connection properties belong to the acquisition, which sends them with /xml
          return Results.Json (response.SetError ("A property is set with a /xml request, not with a /set request"));
        }
        if (method is null) {
          return Results.Json (response.SetError ("No method in the request"));
        }
        await session.SetAsync (method, param ?? "", v, cancellationToken);
        return Results.Json (response.SetSuccess ());
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        logger.LogInformation (ex, $"SetAsync: acquisition={acquisition} method={method} param={param} failed");
        return Results.Json (response.SetError (ex.Message));
      }
    }

    /// <summary>
    /// /data?acquisition=...
    ///
    /// Return all the data of the configuration the last /xml request carried
    ///
    /// Unlike /get and /set, a failure is reported with an error status code, since the response
    /// of this request carries the data itself and cannot carry any status
    /// </summary>
    static async Task<IResult> DataAsync (AcquisitionManager manager, ILoggerFactory loggerFactory,
      string? acquisition, CancellationToken cancellationToken)
    {
      var logger = loggerFactory.CreateLogger (typeof (OpcUaEndpoints));

      var session = manager.TryGetSession (acquisition);
      if (session is null) {
        return Results.Text ($"Acquisition {acquisition} not found", "text/plain", statusCode: StatusCodes.Status404NotFound);
      }

      try {
        return Results.Json (await session.GetDataAsync (cancellationToken));
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        logger.LogInformation (ex, $"DataAsync: acquisition={acquisition} failed");
        return Results.Text (ex.Message, "text/plain", statusCode: StatusCodes.Status503ServiceUnavailable);
      }
    }
  }
}
