using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NS;
using PQT.Web.Infrastructure.Helpers;

namespace PQT.Web.Infrastructure.Utility
{
    public class PhotoUpload
    {
        public static string Upload(PhotoUploadType type, HttpPostedFileBase file)
        {
            var thumbWidth = ImageHelper.MaxWidthThumbnailUpload;
            var thumbHeigth = ImageHelper.MaxHeightThumbnailUpload;
            try
            {
                if (file.ContentLength > 0)
                {
                    var guid = Guid.NewGuid().ToString("N");
                    string fileName = guid + Path.GetExtension(file.FileName);
                    string thumbName = guid + "_" + thumbWidth + "x" + thumbHeigth + Path.GetExtension(file.FileName);

                    string filePath = GetImagePath(type, fileName);

                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);

                    file.SaveAs(filePath);
                    ImageHelper.CreateImageHighQuality(GetfolderPath(type), GetfolderPath(type), fileName, thumbName, thumbHeigth, thumbWidth);

                    return thumbName;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        public static bool Delete(PhotoUploadType type, string fileName)
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

        protected static string GetImagePath(PhotoUploadType type, string fileName)
        {
            return HttpContext.Current.Server.MapPath("~/data/" + type.Value + "/" + fileName);
        }

        public static string GetfolderPath(PhotoUploadType type)
        {
            return HttpContext.Current.Server.MapPath(string.Format("~/data/{0}/", type.Value));
        }


    }

    public class PhotoUploadType : Enumeration
    {
        public static readonly PhotoUploadType Trainer = New<PhotoUploadType>("Trainer", "Trainer");
    }
}