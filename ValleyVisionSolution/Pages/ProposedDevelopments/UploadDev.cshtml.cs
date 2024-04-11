using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValleyVisionSolution.Pages.DataClasses;
using ValleyVisionSolution.Pages.DB;
using ValleyVisionSolution.Services;

namespace ValleyVisionSolution.Pages.ProposedDevelopments
{
    public class UploadDevModel : PageModel
    {

        private readonly IBlobService _blobService;

        public UploadDevModel(IBlobService blobService)
        {
            _blobService = blobService;
        }


        public void OnGet()
        {
            TempData["ShowModal"] = true;
        }

        public async Task<IActionResult> OnPostAsync(IFormFile file)
        {
            int initID = HttpContext.Session.GetInt32("InitID") ?? 0;
            if (file != null && file.Length > 0)
            {
                // Generate a unique file name to avoid overwriting existing files
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;

                // Use the IBlobService to upload the file
                using (var fileStream = file.OpenReadStream())
                {
                    await _blobService.UploadFileBlobAsync(uniqueFileName, fileStream, file.ContentType);
                }

                // Prepare the file metadata
                var fileMeta = new FileMeta
                {
                    FileName_ = fileName + fileExtension,
                    FilePath = uniqueFileName,
                    FileType = fileExtension,
                    UploadedDateTime = DateTime.Now,
                    userID = HttpContext.Session.GetInt32("UserID")
                };

                // Update the database with the file metadata
                int? devID = HttpContext.Session.GetInt32("devID");
                DBClass.UploadDevFile(initID, fileMeta, devID);


                return RedirectToPage("/ProposedDevelopments/ProposedDevelopmentsPage");
            }

            ViewData["ErrorMessage"] = "You must select a file.";
            return Page();
        }
    }
}
