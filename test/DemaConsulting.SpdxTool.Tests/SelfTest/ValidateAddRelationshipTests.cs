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

using DemaConsulting.SpdxTool.SelfTest;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.Tests.SelfTest;

/// <summary>
///     Unit tests for the ValidateAddRelationship self-validation unit.
/// </summary>
[Collection("SelfTestValidation")]
public class ValidateAddRelationshipTests
{
    /// <summary>
    ///     Test that ValidateAddRelationship validation passes.
    /// </summary>
    /// <remarks>
    ///     The test method name <c>SpdxTool_AddRelationship</c> intentionally matches the
    ///     <c>TestResult.Name</c> value recorded by <see cref="ValidateAddRelationship.Run"/> so that
    ///     ReqStream can trace this xUnit test to the self-test result it exercises. This system-level
    ///     naming convention is appropriate for self-test integration tests.
    /// </remarks>
    [Fact]
    public void SpdxTool_AddRelationship()
    {
        // Arrange
        using var context = Context.Create(["--validate"]);
        var results = new DemaConsulting.TestResults.TestResults();

        // Act
        ValidateAddRelationship.Run(context, results);

        // Assert
        Assert.Single(results.Results);
        Assert.Equal(TestOutcome.Passed, results.Results[0].Outcome);
    }
}
