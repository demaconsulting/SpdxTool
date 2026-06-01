// Copyright (c) 2024 DEMA Consulting
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
using DemaConsulting.SpdxTool.Spdx;

namespace DemaConsulting.SpdxTool.Tests.Spdx;

/// <summary>
///     Tests for <see cref="RelationshipDirectionExtensions"/>.
/// </summary>
public class RelationshipDirectionTests
{
    /// <summary>
    ///     Test that Describes relationship maps to Parent direction
    /// </summary>
    [Fact]
    public void RelationshipDirectionExtensions_GetDirection_DescribesRelationship_ReturnsParent()
    {
        // Arrange/Act: Get direction for Describes relationship
        var direction = SpdxRelationshipType.Describes.GetDirection();

        // Assert: direction is Parent
        Assert.Equal(RelationshipDirection.Parent, direction);
    }

    /// <summary>
    ///     Test that DescribedBy relationship maps to Child direction
    /// </summary>
    [Fact]
    public void RelationshipDirectionExtensions_GetDirection_DescribedByRelationship_ReturnsChild()
    {
        // Arrange/Act: Get direction for DescribedBy relationship
        var direction = SpdxRelationshipType.DescribedBy.GetDirection();

        // Assert: direction is Child
        Assert.Equal(RelationshipDirection.Child, direction);
    }

    /// <summary>
    ///     Test that DependencyManifestOf relationship maps to Sibling direction
    /// </summary>
    [Fact]
    public void RelationshipDirectionExtensions_GetDirection_DependencyManifestOfRelationship_ReturnsSibling()
    {
        // Arrange/Act: Get direction for DependencyManifestOf relationship
        var direction = SpdxRelationshipType.DependencyManifestOf.GetDirection();

        // Assert: direction is Sibling
        Assert.Equal(RelationshipDirection.Sibling, direction);
    }

    /// <summary>
    ///     Test that an unmapped relationship type defaults to Sibling direction
    /// </summary>
    [Fact]
    public void RelationshipDirectionExtensions_GetDirection_UnmappedRelationshipType_ReturnsSibling()
    {
        // Arrange/Act: Get direction for an unmapped relationship type (Other is not in DirectionMap)
        var direction = SpdxRelationshipType.Other.GetDirection();

        // Assert: unmapped types default to Sibling
        Assert.Equal(RelationshipDirection.Sibling, direction);
    }
}
