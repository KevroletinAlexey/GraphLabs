using System.IO;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace WebApplication2.Controllers;

public class ImagesLibraryController : ODataController
{
    private readonly IHostingEnvironment _env;
    private readonly IContentTypeProvider _contentTypeProvider;
        
    public ImagesLibraryController( 
        IHostingEnvironment env,
        IContentTypeProvider contentTypeProvider)
    {
        _env = env;
        _contentTypeProvider = contentTypeProvider;
    }

    [HttpGet]
    [AllowAnonymous]
    [ODataRoute("DownloadImage(name={name})")]
    public IActionResult DownloadImage(string name)
    {
        var targetPath = Path.Combine(
            "images_library",
            name);
            
        var file = _env.WebRootFileProvider.GetFileInfo(targetPath);

        if (file.Exists
            && !file.IsDirectory
            && _contentTypeProvider.TryGetContentType(targetPath, out var contentType))
        {
            return File(file.CreateReadStream(), contentType);
        }
        else
        {
            return NotFound();
        }
    }
}