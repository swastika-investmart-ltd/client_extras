using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Components.Helpers
{
    public class SaveFilesInFolder
    {
        public bool UploadIFormFileInFolder(IFormFile FileUpload, string RootPath, string FolderName, string FileName)
        {
            bool IsFileSaved = false;
            try
            {
                string filePath = Path.Combine(RootPath, "Documents", FolderName);
                //Check if directory not exist - Create Documents Folder 
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                string FilePath = Path.Combine(filePath, FileName);
                using (var fileStream = new FileStream(FilePath, FileMode.Create))
                {
                    FileUpload.CopyTo(fileStream);
                    fileStream.Flush();
                }

                IsFileSaved = true;
            }
            catch (Exception ex)
            {
                IsFileSaved = false;
                //LogNLog _logger = new();
               // _logger.Debug($@"UploadIFormFileInFolder: " + ex.InnerException.Message.ToString());
            }
            return IsFileSaved;
        }
    }
}
