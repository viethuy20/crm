using System;
using System.IO;
using System.Web;
using PQT.Web.Infrastructure.Helpers;

namespace PQT.Web.Infrastructure.Utility
{
    public class UserPicture
    {
        public static string Upload(int userId, HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string fileName = userId + Path.GetExtension(file.FileName);
                    //string thumbName = userId + "_400x400" + Path.GetExtension(file.FileName);

                    string filePath = GetImagePath(userId, fileName);

                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);

                    file.SaveAs(filePath);
                    //if (ImageHelper.CreateImageHighQuality(GetfolderPath(userId), GetfolderPath(userId), fileName, thumbName, ImageHelper.MaxHeightUserAvatar, ImageHelper.MaxWidthUserAvatar))
                    //    return thumbName;
                    return fileName;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }
        public static string Upload(int userId, string base64String)
        {
            try
            {
                if (!string.IsNullOrEmpty(base64String))
                {
                    byte[] contents = Convert.FromBase64String(base64String.Replace("data:image/png;base64,", "")
                        .Replace("data:image/jpeg;base64,", "").Replace("data:image/jpg;base64,", ""));
                    var guid = Guid.NewGuid().ToString("N");
                    string fileName = guid + ".png";
                    string filePath = GetImagePath(userId, fileName);
                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);
                    File.WriteAllBytes(filePath, contents);
                    return fileName;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        public static string UploadBackground(int userId, string base64String)
        {
            try
            {
                if (!string.IsNullOrEmpty(base64String))
                {
                    byte[] contents = Convert.FromBase64String(base64String.Replace("data:image/png;base64,", "")
                        .Replace("data:image/jpeg;base64,", "").Replace("data:image/jpg;base64,", ""));
                    var guid = Guid.NewGuid().ToString("N");
                    string fileName = guid + "_background.png";
                    //string fileResizeName = guid + "_background_2.png";

                    string filePath = GetImagePath(userId, fileName);
                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);
                    File.WriteAllBytes(filePath, contents);
                    //if (ImageHelper.CreateImageHighQuality(GetfolderPath(userId), GetfolderPath(userId), fileName, fileResizeName, ImageHelper.MaxHeightUserBackground, ImageHelper.MaxWidthUserBackground))
                    //    return fileResizeName;
                    return fileName;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        public static bool Delete(int userId, string fileName)
        {
            try
            {
                string path = GetImagePath(userId, fileName);
                File.Delete(path);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected static string GetImagePath(int userId, string fileName)
        {
            return HttpContext.Current.Server.MapPath("~/data/user_img/" + userId + "/" + fileName);
        }

        public static string GetfolderPath(int folder)
        {
            return HttpContext.Current.Server.MapPath("~/data/user_img/" + folder + "/");
        }
    }
}
