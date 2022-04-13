using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using WebApplication2.DAL;
using WebApplication2.Entity;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebApplication2.Controllers;

[ODataRoutePrefix("taskModules")]
    public class TaskModulesController : ODataController
    {
        private readonly GraphLabsContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IContentTypeProvider _contentTypeProvider;

        public TaskModulesController(GraphLabsContext context,
            IHostingEnvironment hostingEnvironment,
            IContentTypeProvider contentTypeProvider)
        {
            _db = context;
            _hostingEnvironment = hostingEnvironment;
            _contentTypeProvider = contentTypeProvider;
        }

        [EnableQuery]
        public IQueryable<TaskModule> Get()
        {
            return _db.TaskModules;
        }

        [ODataRoute("({key})")]
        [EnableQuery]
        public SingleResult<TaskModule> Get(long key)
        {
            return SingleResult.Create(_db.TaskModules.Where(t => t.Id == key));
        }
        
        public async Task<IActionResult> RandomVariant(long key)
        {
            var variants = await _db.TaskVariants
                .Where(variant => variant.TaskModule.Id == key)
                .Select(variant => variant.Id)
                .ToArrayAsync();


            if (variants.Length == 0)
            {
                return new NotFoundResult();
            }
            
            var variantId = variants[new Random().Next(0, variants.Length)];
            var selectedVariant = await _db.TaskVariants
                .Include(v => v.TaskModule)
                .SingleAsync(v => v.Id == variantId);

            var result = new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "application/json",
                Content = TaskVariantConverter.ToJson(selectedVariant)
            };

            return result;
        }

        private string GetModulePath(int key)
            => Path.Combine("modules", key.ToString(CultureInfo.InvariantCulture));
        
        public IActionResult Download(int key, [FromODataUri]string path)
        {
            path = path
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            
            var targetPath = Path.Combine(
                GetModulePath(key),
                path);
            
            var file = _hostingEnvironment.WebRootFileProvider.GetFileInfo(targetPath);
        
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
        
        public IActionResult Upload(int key)
        {
            var targetPath = Path.Combine(
                _hostingEnvironment.WebRootPath,
                GetModulePath(key));
            
            var targetDirectory = new DirectoryInfo(targetPath);
            if (targetDirectory.Exists)
                targetDirectory.Delete(true);

            using (var stream = Request.Body)
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(targetPath);
            }
            
            var buildDirectory = new DirectoryInfo(Path.Combine(targetPath, "build"));
            var tempPath = targetPath.TrimEnd('/', '\\') + Guid.NewGuid().ToString("N");
            if (buildDirectory.Exists && targetDirectory.GetDirectories().Length == 1)
            {
                buildDirectory.MoveTo(tempPath);
                targetDirectory.Delete(true);
                Directory.Move(tempPath, targetPath);
            }

            return Ok();
        }
    }