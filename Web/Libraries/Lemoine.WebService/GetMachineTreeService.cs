// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetMachineTree Service.
  /// </summary>
  public class GetMachineTreeService : GenericCachedService<Lemoine.DTO.GetMachineTree>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetMachineTreeService).FullName);

    #region Constructors
    /// <summary>
    /// GetMachineTree is a cached service.
    /// </summary>
    public GetMachineTreeService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request for GetMachineTree (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Lemoine.DTO.GetMachineTree request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        Lemoine.DTO.MachineTreeDTO response = new Lemoine.DTO.MachineTreeDTO();
        
        IList<ICompany> companyList =  ModelDAOHelper.DAOFactory.CompanyDAO.FindAll();
        if(companyList != null) {
          response.Companies = (new Lemoine.DTO.CompanyDTOAssembler()).Assemble(companyList).ToList();
        }
        
        IList<IDepartment> departmentList = ModelDAOHelper.DAOFactory.DepartmentDAO.FindAll();
        if(departmentList != null){
          response.Departments = (new Lemoine.DTO.DepartmentDTOAssembler()).Assemble(departmentList).ToList();
        }

        IList<IMachineCategory> machineCategoryList = ModelDAOHelper.DAOFactory.MachineCategoryDAO.FindAll();
        if(machineCategoryList != null){
          response.MachineCategories = (new Lemoine.DTO.MachineCategoryDTOAssembler()).Assemble(machineCategoryList).ToList();
        }
        
        IList<IMachineSubCategory> subCategoryList = ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.FindAll();
        if(subCategoryList != null){
          response.SubCategories = (new Lemoine.DTO.MachineSubCategoryDTOAssembler()).Assemble(subCategoryList).ToList();
        }
        
        IList<ICell> cellList = ModelDAOHelper.DAOFactory.CellDAO.FindAll();
        if(cellList != null){
          response.Cells = (new Lemoine.DTO.MachineCellDTOAssembler()).Assemble(cellList).ToList();
        }
        
        IList<IMonitoredMachine> monitoredMachineList = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAll();
        if (monitoredMachineList != null) {
          response.MachineDetails = (new Lemoine.DTO.MachineDetailsDTOAssembler()).Assemble(monitoredMachineList).ToList();
        }
        
        return response;
      }
    }
    #endregion // Methods
  }
}
