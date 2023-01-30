// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of CompanyDTOAssembler.
  /// </summary>
  public class CompanyDTOAssembler
  {
    /// <summary>
    /// companyDTO assembler
    /// </summary>
    /// <param name="company"></param>
    /// <returns></returns>
    public CompanyDTO Assemble(Lemoine.Model.ICompany company) {
      CompanyDTO companyDTO = new CompanyDTO();
      companyDTO.Id = company.Id;
      companyDTO.Name = company.Display;
      return companyDTO;
    }
    
    /// <summary>
    /// CompanyDTO list assembler
    /// </summary>
    /// <param name="companyList"></param>
    /// <returns></returns>
    public IEnumerable<CompanyDTO> Assemble(IEnumerable<Lemoine.Model.ICompany> companyList) {
      IList<CompanyDTO> companyDTOList = new List<CompanyDTO>();
      foreach (Lemoine.Model.ICompany company in companyList) {
        companyDTOList.Add(Assemble(company));
      }
      return companyDTOList;
    }
  }
}
