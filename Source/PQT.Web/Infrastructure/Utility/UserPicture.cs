using System;
using System.IO;
using System.Web;
using PQT.Web.Infrastructure.Helpers;

namespace PQT.Web.Infrastructure.Utility
{
    public class UserPicture
    {
        public static string UploadContract(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var guid = DateTime.Now.ToString("yyyyMMddHHmmss") + "_";
                    string fileName = guid + Domain.Helpers.StringHelper.RemoveSpecialCharacters(Path.GetFileNameWithoutExtension(file.FileName)) + Path.GetExtension(file.FileName);
                    string filePath = GetContractPath(fileName);
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
        protected static string GetContractPath(string fileName)
        {
            return HttpContext.Current.Server.MapPath("~/data/user_contract/" + fileName);
        }
        public static string GetContractUrl(string fileName)
        {
            return "/data/user_contract/" + fileName;
        }

        public static string GetfolderPath(int folder)
        {
            return HttpContext.Current.Server.MapPath("~/data/user_img/" + folder + "/");
        }
    }
}
