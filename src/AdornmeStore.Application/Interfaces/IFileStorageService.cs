using Microsoft.AspNetCore.Http;

namespace AdornmeStore.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string? fileUrl,
        CancellationToken cancellationToken = default);
}