namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRestClient _restClient;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger,
                              IRestClient restClient,
                              IWebHostEnvironment webHostEnvironment
                            )
        {
            _logger = logger;
            _restClient = restClient;
            _env = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("prevent-access")]
        [AllowAnonymous]
        public async Task<IActionResult> PreventAccessPage()
        {
            return View("~/Views/Home/PreventAccess.cshtml");
        }

        [HttpGet("file-template/component")]
        [Authorize]
        public async Task<IActionResult> DownloadImportComponentFile()
        {
            var filePath = @"FileExcelTemplate/TemplateImportComponentList.xlsx";
            var fullFilePath = Path.Combine(_env.WebRootPath, filePath);

            if (!System.IO.File.Exists(fullFilePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, commonAPIConstant.FileResponse.StreamType, "TemplateImportComponentList.xlsx");
        }

        [HttpGet("file-template/input-storage")]
        [Authorize]
        public async Task<IActionResult> DownloadInputStorageTemplate()
        {
            var filePath = @"FileExcelTemplate/TemplateImportInputStorage.csv";
            var fullFilePath = Path.Join(_env.WebRootPath, filePath);

            if (!System.IO.File.Exists(fullFilePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, commonAPIConstant.FileResponse.StreamType, "TemplateImportInputStorage.csv");
        }

        [HttpGet("file/template/{fileKey}")]
        [Authorize]
        public async Task<IActionResult> DownloadFileTemplate(string fileKey)
        {
            if (string.IsNullOrEmpty(fileKey))
            {
                ModelState.TryAddModelError(nameof(fileKey), "Vui lòng nhập key.");
            }

            var existFileKey = FileTemplate.filePaths.Any(x => x.Key == fileKey);
            if (!existFileKey)
            {
                ModelState.TryAddModelError(nameof(fileKey), "Không tìm thấy file nào.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                });
            }

            var filePath = FileTemplate.filePaths.GetValueOrDefault(fileKey);
            var fullFilePath = Path.Join(_env.WebRootPath, Path.Combine(FileTemplate.rootPath, filePath));

            if (!System.IO.File.Exists(fullFilePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, commonAPIConstant.FileResponse.StreamType, filePath);
        }
    }
}
