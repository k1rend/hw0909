using hw0909.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class LocalAttachmentStorage : IAttachmentStorage
    {
        private static readonly string[] Allowed =
            { ".txt", ".pdf", ".png", ".jpg", ".jpeg" };
        private const int MaxSize = 2 * 1024 * 1024;

        public string Save(string originalFileName, byte[] content)
        {
            if (string.IsNullOrEmpty(originalFileName) || content == null || content.Length == 0)
                return null;

            string safeName = Path.GetFileName(originalFileName);
            string ext = Path.GetExtension(safeName).ToLowerInvariant();

            if (!Allowed.Contains(ext))
                throw new InvalidOperationException("Недопустимый тип файла.");
            if (content.Length > MaxSize)
                throw new InvalidOperationException("Файл превышает 2 МБ.");

            string relativeDir = "~/App_Data/uploads";
            string physicalDir = HttpContext.Current.Server.MapPath(relativeDir);
            Directory.CreateDirectory(physicalDir);

            string unique = $"{Guid.NewGuid():N}_{safeName}";
            File.WriteAllBytes(Path.Combine(physicalDir, unique), content);
            return relativeDir + "/" + unique;
        }
    }
}