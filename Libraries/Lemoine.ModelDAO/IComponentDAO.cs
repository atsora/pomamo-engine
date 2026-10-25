// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for  of IComponent.
  /// </summary>
  public interface IComponentDAO
    : IGenericUpdateDAO<IComponent, int>
    , IMergeDAO<IComponent>
  {
    /// <summary>
    /// Find the unique Component that matches a Name and a Project
    /// </summary>
    /// <param name="name">not null</param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    IComponent FindByNameAndProject(string name, IProject project);

    /// <summary>
    /// Find the unique Component that matches a Code and Project
    /// </summary>
    /// <param name="code">not null</param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    IComponent FindByCodeAndProject (string code, IProject project);

    /// <summary>
    /// Find components whose names match a pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    IList<IComponent> FindByNameStartPattern(string pattern);

    
    /// <summary>
    /// Tests if exists others Component have same name like Component with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameName(String name, int id, int projectId);
    
    /// <summary>
    /// Tests if exists others Component have same code like Component with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameCode(String code, int id, int projectId);

    /// <summary>
    /// Tests if another component than the one with the given id, in the given project, has the
    /// same name
    ///
    /// To test a component that is not created yet, use an id of 0
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<bool> IfExistsOtherWithSameNameAsync (string name, int id, int projectId);

    /// <summary>
    /// Tests if another component than the one with the given id, in the given project, has the
    /// same code
    ///
    /// To test a component that is not created yet, use an id of 0
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<bool> IfExistsOtherWithSameCodeAsync (string code, int id, int projectId);
    
    /// <summary>
    /// Tests if exists Component have same name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameName(String name, int projectId);
    
    /// <summary>
    /// Tests if exists Component have same code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameCode(String code, int projectId);
    
    /// <summary>
    /// Remove one of the intermediate work piece of the component
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateWorkPiece"></param>
    void RemoveIntermediateWorkPiece (IComponent component, IIntermediateWorkPiece intermediateWorkPiece);
    
    /// <summary>
    /// Change the associated project
    /// </summary>
    /// <param name="component"></param>
    /// <param name="newProject"></param>
    void ChangeProject (IComponent component, IProject newProject);

    /// <summary>
    /// Change the associated project, and record the change for the analysis
    /// </summary>
    /// <param name="component">not null</param>
    /// <param name="newProject">not null</param>
    /// <returns></returns>
    System.Threading.Tasks.Task ChangeProjectAsync (IComponent component, IProject newProject);

    /// <summary>
    /// Initialize the associated ComponentIntermediateWorkPieces
    /// </summary>
    /// <param name="component"></param>
    void InitializeComponentIntermediateWorkPieces (IComponent component);

    /// <summary>
    /// Find the items whose name, code or external code contains the text, whatever the case,
    /// the ones of an archived project last, then in the order of their ids
    /// </summary>
    /// <param name="search">not null or empty</param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns>not null</returns>
    System.Threading.Tasks.Task<IList<IComponent>> FindBySearchAsync (string search, int maxResults);
  }
}
