// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for  of IProject.
  /// </summary>
  public interface IProjectDAO:  IGenericUpdateDAO<IProject, int>
    , IMergeDAO<IProject>
  {
    /// <summary>
    /// Get the first project that matches a specific name (because it is not necessarily unique)
    /// </summary>
    IProject FindByName(string projectName);

    /// <summary>
    /// Find a Project by Code (unique)
    /// </summary>
    IProject FindByCode (string projectCode);

    /// <summary>
    /// Get all Project ordered by name
    /// </summary>
    /// <returns></returns>
    IList<IProject> FindAllOrderByName();
      
    /// <summary>
    /// Tests if exists others Project have same name like Project with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameName(String name, int id);
    
    /// <summary>
    /// Tests if exists others Project have same code like WorkOrder with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameCode(String code, int id);

    /// <summary>
    /// Tests if another project than the one with the given id has the same name
    ///
    /// To test a project that is not created yet, use an id of 0
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<bool> IfExistsOtherWithSameNameAsync (string name, int id);

    /// <summary>
    /// Tests if another project than the one with the given id has the same code
    ///
    /// To test a project that is not created yet, use an id of 0
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<bool> IfExistsOtherWithSameCodeAsync (string code, int id);

    /// <summary>
    /// Tests if exists Project have same name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameName(String name);

    /// <summary>
    /// Tests if exists Project have same code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameCode(String code);
      
    /// <summary>
    /// Get orphans Project, means project without link to WorkOrder
    /// </summary>
    /// <returns></returns>
    IList<IProject> GetOrphans();
    
    /// <summary>
    /// Initialize the associated components
    /// </summary>
    /// <param name="project"></param>
    void InitializeComponents (IProject project);

    /// <summary>
    /// Find the items whose name, code or external code contains the text, whatever the case,
    /// the archived ones last, then in the order of their ids
    /// </summary>
    /// <param name="search">not null or empty</param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns>not null</returns>
    System.Threading.Tasks.Task<IList<IProject>> FindBySearchAsync (string search, int maxResults);
  }
}
