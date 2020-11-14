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

There is a similar controller action that demonstrates a successful call to an external address.

This repository has two branches: `net31` and `net5`:

- `net31`: This demonstrates that `HttpClient` GET requests to an Azure VNet IP address succeed. It also demonstrates that a regular call to `httpbin.org` will succeed. See: https://appservicevnettest31.azurewebsites.net/
- `net5`: This demonstrates that `HttpClient` GET requests to an Azure VNet IP address *fail*. It also demonstrates that a regular call to `httpbin.org` will succeed. See: https://appservicevnettest50.azurewebsites.net/

The architecture of my Azure reproduction is as follows:

- An Azure App Service hosting the ASP.NET applications. It's an S1 App Service Plan hosted in US West.
- An Azure Virtual Machine running Ubuntu Server 20.04 LTS on a Gen2 Standard B1s VM. I installed nginx solely to respond on port 80.
- The VM is connected to the App Service via an Azure VNet. As I am not a systems engineer, Setting this up was laborious, and I'm not sure I could properly document it. However, it did require setting up an Azure Gateway VPN to connect the App Service to the VM.

