using Microsoft.Extensions.Logging;
using Moq;
using NodeServer.handlers;

namespace NodeServer.Tests.handlers;

public class BaseHandlerTests<T> : IDisposable
{
    protected readonly Mock<ILogger<T>> loggerMock = new();

    public BaseHandlerTests()
    {
        loggerMock.Setup(m => m.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
    }

    public virtual void Dispose()
    {
        loggerMock.VerifyNoOtherCalls();
        // should be in the ctor of any handler
        GC.SuppressFinalize(this);
    }
}