﻿//
// Copyright © 2021 Terry Moreland
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using TSMoreland.SourceGenerators.Consumers;

namespace TSMoreland.SourceGenerators.Test;

public sealed class DebuggerDisplayTest
{
    [Test]
    public void GeneratedDebuggerDisplay_ShouldIncludeAllProperties()
    {
        const string expectedValue = "Id: {IdTruncated} FirstName: {FirstNameTruncated} LastName: {LastNameTruncated} MiddleName: {MiddleNameTruncated} Email: {EmailTruncated} ";
        SampleDto dto = new(Guid.NewGuid(), "abcdefghijklmnopqrstuvwxyz", "12345678901234567890") { Email = "alpha@example.com" };

        dto.Should().NotBeNull();
        DebugDisplay.Generator.GenerateDebugDisplayAttribute? generateAttribute = typeof(SampleDto).GetCustomAttribute<DebugDisplay.Generator.GenerateDebugDisplayAttribute>();
        DebuggerDisplayAttribute? attribute = typeof(SampleDto).GetCustomAttribute<DebuggerDisplayAttribute>();

        generateAttribute.Should().NotBeNull();
        attribute.Should().NotBeNull();

        attribute!.Value.Should().Be(expectedValue);
    }
}
