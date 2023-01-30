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
    /// Initialize the associated ComponentIntermediateWorkPieces
    /// </summary>
    /// <param name="component"></param>
    void InitializeComponentIntermediateWorkPieces (IComponent component);
  }
}
