using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AdornmeStore.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    private static readonly Dictionary<string, string[]> AllowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" },
            [".webp"] = new[] { "image/webp" }
        };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Image file is required.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException(
                "Image size cannot exceed 5 MB.");

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedMimeTypes.TryGetValue(
                extension,
                out var allowedMimeTypes))
        {
            throw new ArgumentException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        if (!allowedMimeTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The uploaded file type does not match its extension.");
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        Directory.CreateDirectory(webRootPath);

        var uploadsFolder = Path.Combine(
            webRootPath,
            folder);

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        return $"/{folder}/{fileName}";
    }

    public Task DeleteFileAsync(
        string? fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var relativePath = fileUrl
            .TrimStart('/')
            .Replace(
                '/',
                Path.DirectorySeparatorChar);

        var filePath = Path.Combine(
            webRootPath,
            relativePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}