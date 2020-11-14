# Azure App Service HTTP requests to Azure VNET IP Addresses fail after upgrading to .NET 5.0 #

I have an ASP.NET Core 3.1 application running in Azure App Service. It makes calls to Elasticsearch running on an Ubuntu Server 16.04 LTS Azure Virtual Machine, connected to the App Service via an Azure VNet. When I upgraded the application to target `net5.0`, and upgraded to ASP.NET Core 5.0, all of my calls to Elasticsearch started failing with the following exception:

```
System.Net.Http.HttpRequestException: An attempt was made to access a socket in a way forbidden by its access permissions. (10.1.0.5:9200)
 ---> System.Net.Sockets.SocketException (10013): An attempt was made to access a socket in a way forbidden by its access permissions.
```

Through trial and error, I was able to eliminate Elasticsearch from the equation and reproduce the issue with simple HTTP requests via `HttpClient`:

```csharp
public async Task<IActionResult> VNetRequest()
{
    string? body;
    try
    {
        using var requestMsg = new HttpRequestMessage(HttpMethod.Get, _testRequest.VNetEndpoint);
        using var responseMsg = await _httpClient.SendAsync(requestMsg);
        body = await responseMsg.Content.ReadAsStringAsync();
    }
    catch (Exception ex)
    {
        body = ex.ToString();
    }

    return Content(body);
}
```

When targeting `netcoreapp3.1`, this call will succeed and display some nginx boilerplate text.

When targeting `net5.0`, this call will fail with the following exception:

```
System.Net.Http.HttpRequestException: An attempt was made to access a socket in a way forbidden by its access permissions. (10.1.0.4:80)
 ---> System.Net.Sockets.SocketException (10013): An attempt was made to access a socket in a way forbidden by its access permissions.
   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
   at System.Net.Http.HttpConnectionPool.DefaultConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
   at System.Net.Http.ConnectHelper.ConnectAsync(Func`3 callback, DnsEndPoint endPoint, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at System.Net.Http.ConnectHelper.ConnectAsync(Func`3 callback, DnsEndPoint endPoint, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.ConnectAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.CreateHttp11ConnectionAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.GetHttpConnectionAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.SendWithRetryAsync(HttpRequestMessage request, Boolean async, Boolean doRequestAuth, CancellationToken cancellationToken)
   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)
   at System.Net.Http.DiagnosticsHandler.SendAsyncCore(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)
   at Microsoft.Extensions.Http.Logging.LoggingHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Microsoft.Extensions.Http.Logging.LoggingScopeHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.HttpClient.SendAsyncCore(HttpRequestMessage request, HttpCompletionOption completionOption, Boolean async, Boolean emitTelemetryStartStop, CancellationToken cancellationToken)
   at AppServiceVNetCalls.Controllers.HomeController.VNetRequest() in D:\home\site\repository\src\AppServiceVNetCalls\Controllers\HomeController.cs:line 34
```

When I run these tests locally, I don't get any failures; everything works fine. 

At this point, I'm stuck, and I don't know how where to go next.


# What's in this repository #

This repository has two branches: `net31` and `net5`:

- `net31`: This demonstrates that `HttpClient` GET requests to an Azure VNet IP address succeed. It also demonstrates that a call to a non-Azure VNet IP address will succeed. See: https://appservicevnettest31.azurewebsites.net/
- `net5`: This demonstrates that `HttpClient` GET requests to an Azure VNet IP address *fail*. It also demonstrates that a call to a non-Azure VNet IP address will succeed. See: https://appservicevnettest50.azurewebsites.net/

The architecture of my Azure reproduction is as follows:

- An Azure App Service hosting the ASP.NET applications. It's an S1 App Service Plan hosted in US West.
- An Azure Virtual Machine running Ubuntu Server 20.04 LTS on a Gen2 Standard B1s VM. I installed nginx solely to respond on port 80.
- The VM is connected to the App Service via an Azure VNet. As I am not a systems engineer, setting this up was laborious, and I'm not sure I could properly document it. However, it did require setting up an Azure Gateway VPN to connect the App Service to the VM.

