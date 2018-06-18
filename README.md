# Alabaster

##### Web Server framework for C#/.NET

##### Currently in testing. NuGet package will be publicly listed soon.  

## Hello World example:  

```using Alabaster;

namespace HelloWorldExample
{
    class MyApp
    {
        static void Main(string[] args)
        {
            Server.Port = 5000;
            Server.Get("/", (Request req) => new StringResponse("Hello world!"));            
            Server.Start();
        }
    }
}
```

## API listing:  

### Configurable Values:  

int Server.Port  
string FileIO.StaticFilesBaseDirectory  
  
### Functions:  

###### Server:  
void Server.All(RouteCallback callback)  
void Server.All(string method, RouteCallback callback)  
void Server.AttachWebSocketModule(string route, WebSocketModule module)  
void Server.Delete(string route, RouteCallback callback)  
void Server.Get(string route, string file)  
void Server.Get(string route, RouteCallback callback)  
void Server.Patch(string route, RouteCallback callback)  
void Server.Post(string route, RouteCallback callback)  
void Server.Put(string route, RouteCallback callback)  
void Server.Route(string method, string route, RouteCallback callback)  
void Server.Start()  
void Server.Stop()  

###### FileIO:  
void FileIO.AllowDirectory(string directory)  
void FileIO.AllowFile(string file)  
void FileIO.ForbidDirectory(string directory)  
void FileIO.ForbidFile(string file)  
string FileIO.GetFileExtensionDirectory(string extension)  
byte[] FileIO.GetStaticFile(string file)  
bool FileIO.IsDirectoryAllowed(string directory)  
bool FileIO.IsFileAllowed(string file)  
bool FileIO.RemoveFileExtensionDirectory(string extension)  
void FileIO.SetBlacklistMode()  
void FileIO.SetFileExtensionDirectory(string extension, string directory)  
void FileIO.SetWhitelistMode()  

###### GlobalData:  
T GlobalData.RetrieveVariable<T>(string name) where T : struct  
string GlobalData.RetrieveVariable(string name)  
void GlobalData.StoreVariable<T>(string name, T value) where T : struct  
void GlobalData.StoreVariable(string name, string value)  

### Classes & Delegates:  
  
delegate Response RouteCallback(Request req)  
class Request  
class Session  
abstract class Response  
class StringResponse : Response  
class DataResponse : Response  
class EmptyResponse : Response  
class PassThrough : Response  
class RedirectResponse : Response  

delegate WebSocketMessageContext WebSocketCallback(WebSocketMessageContext context)  
class WebSocketMessageContext  
class WebSocketModule  
class WebSocketChannel  
class WebSocketConnection  
