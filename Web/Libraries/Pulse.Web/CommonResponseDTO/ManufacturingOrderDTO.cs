// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for ManufacturingOrder
  /// </summary>
  [Api ("ManufacturingOrder Response DTO")]
  public class ManufacturingOrderDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
  }

  /// <summary>
  /// Assembler for ManufacturingOrderDTO
  /// </summary>
  public class ManufacturingOrderDTOAssembler : IGenericDTOAssembler<ManufacturingOrderDTO, Lemoine.Model.IManufacturingOrder>
  {
    readonly ILog log = LogManager.GetLogger<ManufacturingOrderDTOAssembler> ();

    /// <summary>
    /// ManufacturingOrderDTO assembler
    /// </summary>
    /// <param name="manufacturingOrder">nullable</param>
    /// <returns></returns>
    public ManufacturingOrderDTO Assemble (Lemoine.Model.IManufacturingOrder manufacturingOrder)
    {
      if (null == manufacturingOrder) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (manufacturingOrder)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.ManufacturingOrderDTO.Assemble")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ManufacturingOrderDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)manufacturingOrder).Id);
            if (null == initialized) {
              log.Error ($"Assemble: manufacturing order with id {((Lemoine.Collections.IDataWithId<int>)manufacturingOrder).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initialized);
            }
          }
        }
      }
      ManufacturingOrderDTO dto = new ManufacturingOrderDTO ();
      dto.Id = ((Lemoine.Collections.IDataWithId<int>)manufacturingOrder).Id;
      dto.Display = manufacturingOrder.Display;
      return dto;
    }

    /// <summary>
    /// ManufacturingOrderDTO list assembler (default display)
    /// </summary>
    /// <param name="manufacturingOrders"></param>
    /// <returns></returns>
    public IEnumerable<ManufacturingOrderDTO> Assemble (IEnumerable<Lemoine.Model.IManufacturingOrder> manufacturingOrders)
    {
      Debug.Assert (null != manufacturingOrders);

      IList<ManufacturingOrderDTO> result = new List<ManufacturingOrderDTO> ();
      foreach (var manufacturingOrder in manufacturingOrders) {
        result.Add (Assemble (manufacturingOrder));
      }
      return result;
    }
  }
}
