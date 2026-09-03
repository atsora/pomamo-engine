# Lem_OpcUaClientService

REST gateway in front of the `Lemoine.Cnc.OpcUaClient` module.

The service keeps one OPC UA connection per machine and exposes the data of those machines with
the `/xml`, `/get`, `/set` and `/data` requests, so that a remote acquisition can collect them with
the standard `CncCoreXmlPost`, `CncCoreGetSet` and `CncCoreData` modules, without embedding the
OPC UA stack itself.

**The service holds no machine configuration of its own.** The url of the OPC UA server, the
credentials and the list of values to read all belong to the acquisition, which sends them with its
`/xml` requests. Whenever what an acquisition sends differs from what the service currently holds
for it — a new server url, a new user, one more value to read — the connection of that machine, and
only that one, is initialized again.

Unlike the generic cnc core service, it needs no configuration file of its own to know which cnc
module to load: it is dedicated to OPC UA and always uses `Lemoine.Cnc.OpcUaClient`. There is
therefore no equivalent of `Gateway-Okuma.xml`, and the `moduleref` of the requests, which the
acquisition modules always send, is accepted and echoed back but selects nothing.

## Why a separate service

`Lemoine.Cnc.OpcUaClient` is licensed under the GPL-2.0, which is not compatible with the GPL-3.0
nor with the Apache-2.0 license. This service is therefore GPL-2.0 as well, it references only
`Lemoine.Cnc.OpcUaClient`, and none of its files derives from a GPL-3.0 or Apache-2.0 file of the
super-repository. See the `LICENSE` file next to this one.

## Requests

All the responses are JSON. `format=json`, which the acquisition modules append, is accepted and
ignored.

**The service has no authentication, and it listens on the loopback interface only.** It is meant
to run on the same machine as the acquisition. Its requests configure connections and, through
`/set`, write on the machines: do not bind it to another interface without putting an
authenticating reverse proxy in front of it.

### `POST /xml?acquisition=<id>`

The body is what `CncCoreXmlPost` builds. It configures the machine and returns its data:

```xml
<root>
  <moduleref ref="opcua" ServerUrl="opc.tcp://192.168.0.10:4840" UseSecurity="false" TimeoutSeconds="10">
    <property name="Password">a password with special characters</property>
    <get method="GetDouble" param="/Channel/State/actFeedRateIpo[1]">RawFeedrate</get>
    <get method="GetString" param="/Channel/ProgramInfo/progName[1]">ProgramName</get>
    <get property="ConnectionError">AcquisitionError</get>
  </moduleref>
</root>
```

```json
{ "RawFeedrate": 1250.0, "ProgramName": "O1234", "AcquisitionError": false }
```

- The **attributes** of `moduleref`, apart from `ref` and `starterror`, are the connection
  properties. The accepted set is explicit, in `Configuration/ConnectionProperty.cs`:
  `ServerUrl` (**required**), `UseSecurity`, `SecurityMode`, `DefaultNamespace`, `Username`,
  `Password`, `CertificatePassword`, `RenewCertificate`, `TimeoutSeconds`, `BrowseAndLog`,
  `CncAlarmSubscription`, `CncAlarmNamespace`, `CncAcquisitionId`. The name is not case sensitive.
  An unknown key, a missing `ServerUrl` or a value that does not convert answers 400 with a message
  that names the problem, rather than leaving the connection half configured.
- A **`property` element** carries the same thing, for a value an attribute holds badly.
- The **`get` elements** are the values to read. `method` is a get method of the module, `property`
  one of its properties, and the text of the element is the key in the returned object.

Since the whole list arrives at once, the first request already returns every value: there is no
warm-up. Reposting the same configuration costs nothing — no reconnection, and the values come from
the cache within `CacheDurationMs`.

An unknown acquisition answers 404, an invalid body 400, and a connection failure 503, all with a
plain text body, so that the module flags the request as failed.

### `GET /data?acquisition=<id>`

Return the data of the configuration the last `/xml` request carried, which is what `CncCoreData`
expects. It requires that machine to have been configured by a `/xml` request first.

### `GET /get?acquisition=<id>&method=<method>&param=<param>`
### `GET /get?acquisition=<id>&property=<property>`

Read one value on a machine a `/xml` request already configured, which is what `CncCoreGetSet`
expects:

```json
{
  "AcquisitionIdentifier": "machine1", "Moduleref": "opcua", "Instruction": "get",
  "Method": "GetDouble", "Property": null, "Param": "/Channel/State/feedRateOvr",
  "Success": true, "Result": 100.0, "Error": null
}
```

A failure is always reported with `"Success": false` and an `Error` message, in a 200 response,
since that is what the module checks.

`method` is one of the get methods of the module: `Get`, `GetBool`, `GetChar`, `GetByte`,
`GetInt16`, `GetUInt16`, `GetInt32`, `GetUInt32`, `GetInt`, `GetUInt`, `GetInt64`, `GetUInt64`,
`GetFloat`, `GetDouble`, `GetString`, plus `DirectRead` and `DirectReadDouble`.

`param` is an OPC UA node, `ns=2;s=<node>` or `<node>` alone when `DefaultNamespace` is set,
optionally followed by `|<index range>`.

### `GET /set?acquisition=<id>&method=Write&param=<param>&<type>=<value>`

Write one value. `<type>` is `long`, `int`, `double`, `boolean`, `string` or the generic `v`, which
is what `CncCoreGetSet` sends. The response has the same shape as `/get`, with a null `Result`.

Setting a *property* is refused here on purpose: the connection properties belong to the
acquisition and change through `/xml`, so that the service never holds a configuration the
acquisition does not know about.

### `GET /ping`

Name of the service and identifiers of the acquisitions that are currently configured.

## Lem_OpcUaClientService.json

Optional, next to the executable. It is **not** `appsettings.json`: every service of the product is
installed in the same directory, so they would all share that file. It only holds what does not
depend on a machine, and every key has a working default, so the service starts without it:

```json
{
  "OpcUaClientService": {
    "Url": "http://127.0.0.1:4841",
    "CacheDurationMs": 500,
    "RegistrationDelayMs": 2000
  }
}
```

| Key | Meaning |
| --- | --- |
| `Url` | Endpoint the service listens on. Loopback by default, see above |
| `CacheDurationMs` | Duration during which the values that were read stay valid. All the requests of a same acquisition cycle then share a single read on the OPC UA server |
| `RegistrationDelayMs` | Only for the parameters a `/get` request asks for and that no `/xml` declared. See below |

The default endpoint is **`http://127.0.0.1:4841`** (the OPC UA servers themselves usually listen
on 4840).

## Logs

The service logs through `Microsoft.Extensions.Logging` (MIT), which suits its GPL-2.0 license,
unlike log4net. It adds no logging package: `UseSystemd` sends the logs to the journal on Linux,
`UseWindowsService` to the event log on Windows, and the console provider is used when it runs in a
terminal. The levels are set in the `Logging` section of `Lem_OpcUaClientService.json`.

```
journalctl -u atracking-opcua -f
```

`Lemoine.Cnc.OpcUaClient` logs the same way, and this service tells it where to send its logs, at
startup:

```csharp
Lemoine.Cnc.OpcUaClientLogging.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory> ();
```

A cnc module is created by its host without dependency injection, so its logger factory can not be
injected: it is set once on that static hook. An acquisition that loads the module directly, with a
`-D` configuration, gets its logs too: `Lemoine.Cnc.CncModuleLogging`, in the acquisition engine,
sets the same hook with a factory that forwards to log4net, keeping the category names.

## Installation

- **Windows**: the MSI installs it as the `Lem_OpcUaClientService` service, from the
  `C_opcUaClient` component group of `atracking-acquisition-msi`.
- **Linux**: the Debian package ships `atracking-opcua.service`, offered as the `OpcUa` choice of
  the services to install.

Both are started automatically and restart on failure. The service is only useful together with an
acquisition whose configuration delegates the OPC UA connection to it, such as
`Siemens-OPC_UA.xml` (the `-D` variants keep the connection inside the acquisition).

## Two behaviours worth knowing

- **Parameters that no `/xml` declared.** The module only records the parameters to monitor until
  its query is prepared, so taking a new one into account means opening the connection again. The
  parameters a `/xml` carries are all known at once and cost one single connection. A parameter that
  only a `/get` request asks for is gathered with the others for `RegistrationDelayMs`, and the
  requests for it fail during that delay. Declaring every value in the `/xml` avoids this entirely.
- **Direct reads.** `DirectRead` resets the prepared query of the module instance it runs on. The
  service therefore opens a second, dedicated OPC UA session for them, the first time one is
  requested, so that they never disturb the cached reads.

## Acquisition side

```xml
<module type="Lemoine.Cnc.CncCoreXmlPost, Lemoine.Cnc.CncCoreClient"
        Port="4841"
        ServerUrl="opc.tcp://{Param1}:{Param2}" DefaultNamespace="{Param3}"
        Username="{Param4}" Password="{Param5}" UseSecurity="false">
  <get method="GetDouble" param="/Channel/State/actFeedRateIpo[1]">RawFeedrate</get>
  <get method="GetString" param="/Channel/ProgramInfo/progName[1]">ProgramName</get>
</module>
```

`CncCoreXmlPost` forwards the attributes of the module element on the `moduleref` element, apart
from its own (`BaseUrl`, `Port`, `ApiKey`, `ModuleRef`) and from the ones the acquisition engine
processes.
An attribute that is not a property of `CncCoreXmlPost` only makes the engine log a warning once,
when the module is loaded (`CncDataHandler.SetProperty`), and it is forwarded as it should be.

`Port` is a shortcut for `BaseUrl`, added to `CncCoreXmlPost` for a service that runs on the same
machine as the acquisition: it means `http://localhost:<port>`. `BaseUrl` still comes first when
both are set, so the Okuma and Haas gateways are unaffected.

## Running it

```powershell
dotnet run --project Lem_OpcUaClientService.csproj
```

It runs as a Windows service and as a systemd service without any change: the host detects both.
`Lem_OpcUaClientService.http` holds a request of each kind.
