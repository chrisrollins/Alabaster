# Alabaster

##### Web Server framework for C#/.NET

##### Currently in testing. NuGet package will be publicly listed soon.  

## Hello World example:  

```
using Alabaster;

namespace HelloWorldExample
{
    class MyApp
    {
        static void Main(string[] args)
        {
            Server.Get("/", () => "Hello World!");            
            Server.Start(Port: 5000);
        }
    }
}
```

## API listing:  

### Configurable Values:  

int Server.Config.Port  
bool Server.Config.EnableCustomHTTPMethods  
string Server.Config.ServerID  
string FileIO.StaticFilesBaseDirectory  
  
### Functions:  

###### Server:  
void Server.Get(string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Get(string route, Response res)  
void Server.Delete(string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Delete(string route, Response res)  
void Server.Patch(string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Patch(string route, Response res)  
void Server.Post(string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Post(string route, Response res)  
void Server.Put(string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Put(string route, Response res)  
void Server.Route(string method, string route, RouteCallback_A/B/C/D/E/F callback)  
void Server.Route(string method, string route, Response res)  
void Server.All(string method, RouteCallback_A/B/C/D/E/F callback)  
void Server.All(string method, Response res)  
void Server.All(RouteCallback_A/B/C/D/E/F callback)  
void Server.All(Response res)  
void Server.AttachWebSocketModule(string route, WebSocketModule module)  
void Server.Start()  
void Server.Start(int Port)  
void Server.Start(ServerOptions options)  
void Server.Stop()  

###### FileIO:  
byte[] FileIO.GetFile(string file)  
byte[] FileIO.GetFile(string file, string baseDirectory)  
void FileIO.AllowFiles(params string[] files)  
void FileIO.AllowDirectories(params string[] directories)  
void FileIO.ForbidFiles(string files)  
void FileIO.ForbidDirectories(string directories)  
bool FileIO.IsFileAllowed(string file)  
bool FileIO.IsDirectoryAllowed(string directory)  
string FileIO.GetFileExtensionDirectory(string extension)  
void FileIO.SetFileExtensionDirectory(string extension, string directory)  
bool FileIO.RemoveFileExtensionDirectory(string extension)  
void FileIO.SetBlacklistMode()  
void FileIO.SetWhitelistMode()  

###### GlobalData:  
T GlobalData.RetrieveVariable<T>(string name) where T : struct  
string GlobalData.RetrieveVariable(string name)  
void GlobalData.StoreVariable<T>(string name, T value) where T : struct  
void GlobalData.StoreVariable(string name, string value)  

###### Client:  

async Task<string> Client.Get(string url, Scheme scheme = Scheme.HTTP)  
async Task<string> Client.Delete(string url, Scheme scheme = Scheme.HTTP)  
async Task<string> Client.Post(string url, string body, Scheme scheme = Scheme.HTTP)  
async Task<string> Client.Patch(string url, string body, Scheme scheme = Scheme.HTTP)  
async Task<string> Client.Put(string url, string body, Scheme scheme = Scheme.HTTP)  
async Task<string> Client.Request(string method, string url, string body, Scheme scheme = Scheme.HTTP)  

### Delegates:  

delegate Response RouteCallback_A(Request req)  
delegate void RouteCallback_B(Request req)  
delegate void RouteCallback_C()  
delegate T RouteCallback_D<T>(Request req) where T : struct  
delegate IEnumerable<T> RouteCallback_E<T>(Request req) where T : struct  
delegate IEnumerable<T> RouteCallback_F<T>() where T : struct  
delegate WebSocketMessageContext WebSocketCallback(WebSocketMessageContext context)  

### Classes:  

class Request  
class Session  
abstract class Response  
class StringResponse : Response  
class DataResponse : Response  
class EmptyResponse : Response  
class PassThrough : Response  
class RedirectResponse : Response  

class WebSocketMessageContext  
class WebSocketModule  
class WebSocketChannel  
class WebSocketConnection  

### Structs:

struct ServerOptions  

### Enums:  
Client.Scheme { HTTP, HTTPS }

