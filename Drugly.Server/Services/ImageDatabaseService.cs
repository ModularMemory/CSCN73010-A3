using Drugly.Server.Models;
using Drugly.Server.Services.Interfaces;

namespace Drugly.Server.Services;

public class ImageDatabaseService : IImageDatabaseService
{
    private readonly string _folderPath = "images";

    public Task<Stream> GetImageById(string id, out string contentType)
    {
        id = Path.GetFileName(id);
        var path = Path.Combine(_folderPath, $"{id}.bin");

        if (!File.Exists(path))
        {
            throw new ImageNotFoundException();
        }


        contentType = "image/bmp";
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public async Task SetImageById(string id, string contentType, Stream content)
    {
        id = Path.GetFileName(id);
        var path = Path.Combine(_folderPath, $"{id}.bin");

        Directory.CreateDirectory(_folderPath);

        await using var fs = File.Create(path);
        await content.CopyToAsync(fs);
    }
}
