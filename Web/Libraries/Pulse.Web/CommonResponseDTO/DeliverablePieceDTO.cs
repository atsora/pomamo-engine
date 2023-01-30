// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for DeliverablePieceDTO.
  /// </summary>
  public class DeliverablePieceDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Code (serial number): nullable
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Associated work order (nullable)
    /// </summary>
    public WorkOrderDTO WorkOrder { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="display"></param>
    /// <returns></returns>
    public DeliverablePieceDTO (int id, string display)
    {
      this.Id = id;
      this.Display = display;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="deliverablePiece">not null</param>
    public DeliverablePieceDTO (IDeliverablePiece deliverablePiece)
    {
      Debug.Assert (null != deliverablePiece);

      this.Id = deliverablePiece.Id;
      this.Display = deliverablePiece.Display;
      if (!string.IsNullOrEmpty (deliverablePiece.Code)) {
        this.Code = deliverablePiece.Code;
      }
      if (null != deliverablePiece.WorkOrder) {
        this.WorkOrder = new WorkOrderDTOAssembler ().Assemble (deliverablePiece.WorkOrder);
      }
    }
  }
}
