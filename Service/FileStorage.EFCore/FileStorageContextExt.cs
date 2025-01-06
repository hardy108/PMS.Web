using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;

namespace FileStorage.EFCore
{
    public static class FileStorageContextExt
    {
        public static long UploadFile(this FilestorageContext context,byte[] fileContents, string fileType, string userName)
        {
            File file = new File
            {
                FileContent = fileContents,                
                CreatedBy = userName,
                Created = DateTime.Now,
                FileType = fileType
            };
            var entry = context.Entry<File>(file);
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Added;
            context.SaveChanges();
            return entry.Entity.FileID;
        }

        public static bool DeleteFile(this FilestorageContext context, long fileId,string userName)
        {
            File file = context.File.Find(fileId);
            if (file == null)
                throw new Exception("File not found");
            var entry = context.Entry<File>(file);
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            context.SaveChanges();
            return true;
        }

        public static bool DeleteFiles(this FilestorageContext context, IEnumerable<long> fileIds,string userName)
        {
            List<File> files = context.File.Where(o => fileIds.Contains(o.FileID)).ToList();
            if (!files.Any())
                throw new Exception("File not found");
            context.File.RemoveRange(files);
            context.SaveChanges();
            return true;
        }

        public static File GetFile(this FilestorageContext context, long Id)
        {
            File file = context.File.Find(Id);
            if (file == null)
                throw new Exception("File not found");
            return file;
        }

        public static bool UseFile(this FilestorageContext context, long Id, string usedBy)
        {
            if (string.IsNullOrWhiteSpace(usedBy))
                throw new Exception("Please specify file usage");

            File file = context.File.Find(Id);
            if (file == null)
                throw new Exception("File not found");
            file.UsedBy = usedBy;
            context.Entry<File>(file).State = EntityState.Modified;
            context.SaveChanges();
            return true;
        }

        
    }
}
