// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of DepartmentDTOAssembler.
  /// </summary>
  public class DepartmentDTOAssembler
  {
    /// <summary>
    /// DepartmentDTO assembler
    /// </summary>
    /// <param name="department"></param>
    /// <returns></returns>
    public DepartmentDTO Assemble(Lemoine.Model.IDepartment department) {
      DepartmentDTO departmentDTO = new DepartmentDTO();
      departmentDTO.Id = department.Id;
      departmentDTO.Name = department.Display;
      return departmentDTO;
    }
    
    /// <summary>
    /// DepartmentDTO list assembler
    /// </summary>
    /// <param name="departmentList"></param>
    /// <returns></returns>
    public IEnumerable<DepartmentDTO> Assemble(IEnumerable<Lemoine.Model.IDepartment> departmentList) {
      IList<DepartmentDTO> departmentDTOList = new List<DepartmentDTO>();
      foreach (Lemoine.Model.IDepartment department in departmentList) {
        departmentDTOList.Add(Assemble(department));
      }
      return departmentDTOList;
    }
  }
}
