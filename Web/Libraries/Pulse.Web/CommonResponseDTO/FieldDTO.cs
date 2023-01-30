// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for Field
  /// </summary>
  [Api("Field Response DTO")]
  public class FieldDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of field
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Unit display
    /// </summary>
    public string Unit { get; set; }
  }
  
  /// <summary>
  /// Assembler for FieldDTO
  /// </summary>
  public class FieldDTOAssembler : IGenericDTOAssembler<FieldDTO, Lemoine.Model.IField>
  {
    /// <summary>
    /// FieldDTO assembler
    /// </summary>
    /// <param name="field">nullable</param>
    /// <returns></returns>
    public FieldDTO Assemble(Lemoine.Model.IField field)
    {
      if (null == field) {
        return null;
      }
      FieldDTO fieldDTO = new FieldDTO();
      fieldDTO.Id = ((Lemoine.Collections.IDataWithId<int>)field).Id;
      fieldDTO.Display = field.Display;
      if (null != field.Unit) {
        fieldDTO.Unit = field.Unit.Display;
      }
      return fieldDTO;
    }
    
    /// <summary>
    /// FieldDTO list assembler
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public IEnumerable<FieldDTO> Assemble(IEnumerable<Lemoine.Model.IField> fields)
    {
      Debug.Assert (null != fields);
      
      IList<FieldDTO> result = new List<FieldDTO> ();
      foreach (var field in fields) {
        result.Add (Assemble (field));
      }
      return result;
    }
  }
}
