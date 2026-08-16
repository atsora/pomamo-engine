// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for ProductionClassDTO.
  /// </summary>
  public class ProductionClassDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
  }

  /// <summary>
  /// Assembler for ProductionClassDTO.
  /// </summary>
  public class ProductionClassDTOAssembler : IGenericDTOAssembler<ProductionClassDTO, Lemoine.Model.ProductionClass>
  {
    /// <summary>
    /// ProductionClassDTO assembler
    /// </summary>
    /// <param name="productionClass"></param>
    /// <returns></returns>
    public ProductionClassDTO Assemble (Lemoine.Model.ProductionClass productionClass)
    {
      ProductionClassDTO productionClassDTO = new ProductionClassDTO ();
      productionClassDTO.Id = (int)productionClass;
      var i18nKey = "ProductionClass" + productionClass.ToString ();
      productionClassDTO.Display = Lemoine.I18N.PulseCatalog.GetString (i18nKey, productionClass.ToString ());
      return productionClassDTO;
    }

    /// <summary>
    /// ProductionClassDTO list assembler
    /// </summary>
    /// <param name="productionClasses"></param>
    /// <returns></returns>
    public IEnumerable<ProductionClassDTO> Assemble (IEnumerable<Lemoine.Model.ProductionClass> productionClasses)
    {
      IList<ProductionClassDTO> productionClassesDTO = new List<ProductionClassDTO> ();
      foreach (Lemoine.Model.ProductionClass productionClass in productionClasses) {
        productionClassesDTO.Add (Assemble (productionClass));
      }
      return productionClassesDTO;
    }
  }
}
