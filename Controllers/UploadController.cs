using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace BuggyExtensionCheck.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadController(IWebHostEnvironment webHostEnvironment)
        {
            this._webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string filename)
        {
            string path = GetPathAndFilename(filename);
            if(System.IO.File.Exists(path))
            {
                return new FileContentResult(System.IO.File.ReadAllBytes(path), "image/png");
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            Startup.Progress = 0;

            string filename = GetFileName(file);

            if(!IsExtensionCorrect(filename))
            {
                return BadRequest("Please only send accepted (png/jpg/gif) images.");
            }

            byte[] buffer = new byte[8 * 1024];

            using (FileStream output = System.IO.File.Create(GetPathAndFilename(filename)))
            {
                using (Stream input = file.OpenReadStream())
                {
                    long totalReadBytes = 0;
                    int readBytes = 0;

                    while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        await output.WriteAsync(buffer, 0, readBytes);
                        totalReadBytes += readBytes;
                        Startup.Progress = (int)((float)totalReadBytes / (float)file.Length * 100.0);
                        await Task.Delay(500); // It is only to make the process slower
                    }
                }
            }

            return this.Content(filename);
        }

        [HttpPost]
        public ActionResult Progress()
        {
            return this.Content(Startup.Progress.ToString());
        }

        private string GetFileName(IFormFile file)
        {
            string filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"'); 

            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        private bool IsExtensionCorrect(string filename)
        {
            string ext = filename.Split(new char[] { '.' })[1];
            if(ext == "png" || ext == "gif" || ext == "jpg" || ext == "jpeg")
            {
                return true;
            }

            return false;
        }

        private string GetPathAndFilename(string filename)
        {
            string path = _webHostEnvironment.ContentRootPath + "\\Uploads\\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }
    }
}