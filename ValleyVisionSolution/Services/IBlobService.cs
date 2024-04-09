using System.IO;
using System.Threading.Tasks;
namespace ValleyVisionSolution.Services
{
    public interface IBlobService
    {
        Task UploadFileBlobAsync(string blobName, Stream content, string contentType);
        Task<Stream> DownloadFileBlobAsync(string blobName);
        Task DeleteFileBlobAsync(string blobName);
    }
}
