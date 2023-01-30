# pomamo-engine

## Overview

Pomamo for (Powerful Maching Monitoring software) is a set of applications and services to monitor CNC machines.

This must be usually used with cnc modules.

## External library requirements

- get the SharedMemory.dll file in https://www.nuget.org/packages/SharedMemory , sign it and put it in folder 3rdParty/SharedMemory/netstandard2.0
- get the patched version of NHibernate that supports partitioned tables, compile it and put:
  - NHibernate.dll in folder 3rdParty/NHibernateAndCo/NHibernate/Required_Bins
  - NHibernate.Caches.CoreMemoryCache.dll in folder 3rdParty/NHibernateAndCo/NHibernate.Caches
- get IIOPChannel.dll from the project IIOP.net https://iiop-net.sourceforge.net/ and put it in folder 3rsParty/IIOP.NET/IIOPChannel.dll

## License

See file LICENSE for license details
