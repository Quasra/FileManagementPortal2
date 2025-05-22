using FileManagementPortal1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementPortal1.Repositories
{
    public class FileRepository : GenericRepository<FileModel>
    {
        private readonly IWebHostEnvironment _environment;

        public FileRepository(ApplicationDbContext context, IWebHostEnvironment environment)
            : base(context)
        {
            _environment = environment;
        }

        public async Task<List<FileModel>> GetUserFilesAsync(string userId)
        {
            return await GetQueryable()
                .Include(f => f.User)
                .Include(f => f.Folder)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<FileModel> UploadFileAsync(IFormFile file, string userId, int? folderId = null)
        {
            
            string uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            
            string userFolder = Path.Combine(uploadsFolder, userId);
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(userFolder, uniqueFileName);

            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            
            var fileEntity = new FileModel
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FilePath = filePath,
                FileSize = file.Length,
                UserId = userId,
                FolderId = folderId
            };

            return await AddAsync(fileEntity);
        }

        public async Task<(byte[], string, string)> DownloadFileAsync(int id)
        {
            var file = await GetByIdAsync(id);
            if (file == null)
                return (null, null, null);

            byte[] fileBytes;

            try
            {
                fileBytes = await File.ReadAllBytesAsync(file.FilePath);
            }
            catch (Exception)
            {
                return (null, null, null);
            }

            return (fileBytes, file.ContentType, file.FileName);
        }
        public async Task<bool> HardDeleteFileAsync(int id, string userId)
        {
            var file = await GetByIdAsync(id);
            if (file == null || file.UserId != userId)
                return false;

            // Fiziksel dosyayı siliyoz
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }

            // Veritabanından tamamen siliyozz.
            return await DeleteAsync(id);
        }
        public async Task<bool> DeleteFileAsync(int id, string userId)
        {
            var file = await GetByIdAsync(id);
            if (file == null || file.UserId != userId)
                return false;

            
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }

            
             await DeleteAsync(id);
            return true;
        }



        internal void Remove(FileModel file)
        {
            throw new NotImplementedException();
        }

        internal async Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}