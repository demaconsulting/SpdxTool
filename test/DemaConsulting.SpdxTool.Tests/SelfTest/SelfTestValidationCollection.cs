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

// cspell:ignore Xunit
using Xunit;

namespace DemaConsulting.SpdxTool.Tests.SelfTest;

/// <summary>
///     xUnit v3 collection definition that enforces serial execution of all Self-Test validation
///     test classes. Two sources of global process state mutation require serial execution:
///     (1) all ValidateXxx self-test classes use the hardcoded relative path <c>validate.tmp</c>
///     via <c>Directory.SetCurrentDirectory</c>, which races across concurrent test classes; and
///     (2) <see cref="SelfTestTests"/> redirects <c>Console.Out</c> around each
///     <c>Validate.Run</c> call, which would corrupt output observed by concurrently running
///     tests. This collection prevents races on both sources of global process state.
/// </summary>
[CollectionDefinition("SelfTestValidation")]
public class SelfTestValidationCollection { }
