using XRoadLib.Soap;

namespace XRoadLib.Tests.Soap;

public class SoapFaultCodesTest
{
    [Fact]
    public void ClientFaultHasCorrectCode()
    {
        var faultCode = new ClientFaultCode("message content");
        Assert.Equal("Client.message content", faultCode.Value);
    }

    [Fact]
    public void ServerFaultHasCorrectCode()
    {
        var faultCode = new ServerFaultCode("server message content");
        Assert.Equal("Server.server message content", faultCode.Value);
    }

    [Fact]
    public void WithoutMessageCode()
    {
        var faultCode = new ClientFaultCode();
        Assert.Equal("Client", faultCode.Value);
    }

    [Fact]
    public void InternalServerError()
    {
        var faultCode = ServerFaultCode.InternalError;
        Assert.Equal("Server.InternalError", faultCode.Value);
    }
}