using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NS;
using PQT.Web.Infrastructure.Helpers;

namespace PQT.Web.Infrastructure.Utility
{
    public class FileUpload
    {
        public static string Upload(FileUploadType type, HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var guid = DateTime.Now.ToString("yyyyMMddHHmmss") + "_";
                    string fileName = guid + Domain.Helpers.StringHelper.RemoveSpecialCharacters(Path.GetFileNameWithoutExtension(file.FileName)) + Path.GetExtension(file.FileName);
                    string filePath = GetImagePath(type, fileName);
                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);
                    file.SaveAs(filePath);
                    return fileName;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        public static bool Delete(FileUploadType type, string fileName)
        {
            try
            {
                string path = GetImagePath(type, fileName);
                File.Delete(path);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected static string GetImagePath(FileUploadType type, string fileName)
        {
            return HttpContext.Current.Server.MapPath("~/data/" + type.Value + "/" + fileName);
        }

        public static string GetImageUrl(FileUploadType type, string fileName)
        {
            return "/data/" + type.Value + "/" + fileName;
        }

        public static string GetfolderPath(FileUploadType type)
        {
            return HttpContext.Current.Server.MapPath(string.Format("~/data/{0}/", type.Value));
        }


    }

    public class FileUploadType : Enumeration
    {
        public static readonly FileUploadType Event = New<FileUploadType>("Event", "Event");
        public static readonly FileUploadType Trainer = New<FileUploadType>("Trainer", "Trainer");
        public static readonly FileUploadType Lead = New<FileUploadType>("Lead", "Lead");
        public static readonly FileUploadType Booking = New<FileUploadType>("Booking", "Booking");
        public static readonly FileUploadType KPI = New<FileUploadType>("KPI", "KPI");
    }
}