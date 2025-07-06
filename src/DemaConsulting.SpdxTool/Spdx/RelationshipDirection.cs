// Copyright (c) 2025 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.SpdxModel;

namespace DemaConsulting.SpdxTool.Spdx;

/// <summary>
///     Relationship direction enumeration
/// </summary>
public enum RelationshipDirection
{
    /// <summary>
    ///     ID is the parent of the related element
    /// </summary>
    Parent,

    /// <summary>
    ///     ID is the child of the related element
    /// </summary>
    Child,

    /// <summary>
    ///     ID and related element are siblings
    /// </summary>
    Sibling
}

/// <summary>
///     Relationship Direction Extensions
/// </summary>
public static class RelationshipDirectionExtensions
{
    /// <summary>
    ///     Dictionary of SPDX relationship types to relationship directions
    /// </summary>
    private static readonly Dictionary<SpdxRelationshipType, RelationshipDirection> DirectionMap = new()
    {
        { SpdxRelationshipType.Describes, RelationshipDirection.Parent },
        { SpdxRelationshipType.DescribedBy, RelationshipDirection.Child },
        { SpdxRelationshipType.Contains, RelationshipDirection.Parent },
        { SpdxRelationshipType.ContainedBy, RelationshipDirection.Child },
        { SpdxRelationshipType.DependsOn, RelationshipDirection.Parent },
        { SpdxRelationshipType.DependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.DependencyManifestOf, RelationshipDirection.Sibling },
        { SpdxRelationshipType.BuildDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.DevDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.OptionalDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.ProvidedDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.TestDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.RuntimeDependencyOf, RelationshipDirection.Child },
        { SpdxRelationshipType.Generates, RelationshipDirection.Parent },
        { SpdxRelationshipType.GeneratedFrom, RelationshipDirection.Child },
        { SpdxRelationshipType.DistributionArtifact, RelationshipDirection.Child },
        { SpdxRelationshipType.PatchFor, RelationshipDirection.Child },
        { SpdxRelationshipType.PatchApplied, RelationshipDirection.Child },
        { SpdxRelationshipType.DynamicLink, RelationshipDirection.Parent },
        { SpdxRelationshipType.StaticLink, RelationshipDirection.Parent },
        { SpdxRelationshipType.BuildToolOf, RelationshipDirection.Child },
        { SpdxRelationshipType.DevToolOf, RelationshipDirection.Child },
        { SpdxRelationshipType.TestToolOf, RelationshipDirection.Child },
        { SpdxRelationshipType.DocumentationOf, RelationshipDirection.Child },
        { SpdxRelationshipType.OptionalComponentOf, RelationshipDirection.Child },
        { SpdxRelationshipType.PackageOf, RelationshipDirection.Child },
        { SpdxRelationshipType.PrerequisiteFor, RelationshipDirection.Child },
        { SpdxRelationshipType.HasPrerequisite, RelationshipDirection.Parent },
    };
    
    /// <summary>
    ///     Get the direction of a relationship
    /// </summary>
    /// <param name="type">Relationship type</param>
    /// <returns>Relationship direction</returns>
    public static RelationshipDirection GetDirection(this SpdxRelationshipType type)
    {
        return DirectionMap.GetValueOrDefault(type, RelationshipDirection.Sibling);
    }
}