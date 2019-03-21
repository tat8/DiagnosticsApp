using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Services.Blobs
{
    public interface IBlobService
    {
        string UploadBlob(string filename, IFormFile file);
        string UploadBlob(string filename, byte[] data);
        string DownloadBlob(string reference);
        string GetTempFolderOrigin();
        string GetTempFolderParsed();
    }
}
