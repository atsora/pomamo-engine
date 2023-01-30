// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using Lemoine.Cnc.Asp;

namespace Lem_CncService.Wcf
{
  /// <summary>
  /// 
  /// </summary>
  [ServiceContract (SessionMode = SessionMode.Allowed)]
  public interface IWcfService
  {
    [OperationContract]
    [WebGet (UriTemplate = "get?acquisition={acquisition}&moduleref={moduleref}&method={method}&property={property}&param={param}", ResponseFormat = WebMessageFormat.Json)]
    Message Get (string acquisition, string moduleref, string method, string property, string param);

    [OperationContract]
    [WebGet (UriTemplate = "set?acquisition={acquisition}&moduleref={moduleref}&method={method}&property={property}&param={param}&v={v}&string={stringvalue}&long={longvalue}&int={intvalue}&double={doublevalue}&boolean={boolvalue}", ResponseFormat = WebMessageFormat.Json)]
    Message Set (string acquisition, string moduleref, string method, string property, string param, string v, string stringvalue, string longvalue, string intvalue, string doublevalue, string boolvalue);

    [OperationContract]
    [WebGet (UriTemplate = "data?acquisition={acquisition}", ResponseFormat = WebMessageFormat.Json)]
    Message GetData (string acquisition);

    [OperationContract]
    [WebInvoke (UriTemplate = "xml?acquisition={acquisition}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Message PostXml (string acquisition);

    [OperationContract]
    [WebGet (UriTemplate = "createApiKey", ResponseFormat = WebMessageFormat.Json)]
    Message CreateApiKey ();
  }
}
