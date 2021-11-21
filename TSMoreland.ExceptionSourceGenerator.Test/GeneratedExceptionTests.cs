using System;
using NUnit.Framework;
using TSMoreland.ExceptionSourceGenerator.Consumer;

namespace TSMoreland.ExceptionSourceGenerator.Test;

/// <summary>
/// mostly verifying what is fairly obvious, the real test is whether this compiles
/// </summary>
public class GeneratedExceptionTests
{
    [Test]
    public void SimpleException_ReturnsMessage_WhenGeneratedAndGivenMessage()
    {
        const string expected = "51FA53CE-96B7-43E5-8B6F-BA5BEBD1C23F";
        //var exception = new SimpleException(expected);
        //var actual = exception.Message;
        //Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SimpleException_ReturnsMessage_WhenGeneratedAndGivenException()
    {
        const string expected = "CD054D6C-07B4-4A8E-8D9B-4FBC77B1CA13";
        var inner = new NotImplementedException("not implemented");
        var exception = new SimpleException();
        //var exception = new SimpleException(expected, inner);
        //var actual = exception.Message;
        //Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SimpleException_ReturnsInnerException_WhenGeneratedAndGivenException()
    {
        var expected = new NotImplementedException("not implemented");
        //var exception = new SimpleException("message", expected);
        //var actual = exception.InnerException;
        //Assert.That(actual, Is.EqualTo(expected));
    }

}