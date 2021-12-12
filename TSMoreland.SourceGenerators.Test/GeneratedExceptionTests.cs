using NUnit.Framework;
using TSMoreland.SourceGenerators.Consumers;

namespace TSMoreland.SourceGenerators.Test;

/// <summary>
/// mostly verifying what is fairly obvious, the real test is whether this compiles
/// </summary>
public class GeneratedExceptionTests
{
    /*
    [Test]
    public void SimpleException_ReturnsMessage_WhenGeneratedAndGivenMessage()
    {
        const string expected = "sample message";
        var exception = new SimpleException(expected);
        string actual = exception.Message;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SimpleException_ReturnsMessage_WhenGeneratedAndGivenException()
    {
        const string expected = "sample message";
        var inner = new NotImplementedException("not implemented");
        var exception = new SimpleException(expected, inner);
        string actual = exception.Message;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SimpleException_ReturnsInnerException_WhenGeneratedAndGivenException()
    {
        NotImplementedException expected = new("not implemented");
        var exception = new SimpleException("message", expected);
        var actual = exception.InnerException;
        Assert.That(actual, Is.EqualTo(expected));
    }
    */

}
