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
                    string thumbName = userId + "_400x400" + Path.GetExtension(file.FileName);

                    string filePath = GetImagePath(userId, fileName);

                    string directory = Path.GetDirectoryName(filePath);
                    if (directory != null) Directory.CreateDirectory(directory);

                    file.SaveAs(filePath);
                    ImageHelper.CreateImageHighQuality(GetfolderPath(userId), GetfolderPath(userId), fileName, thumbName, ImageHelper.MaxHeightUserAvatar, ImageHelper.MaxWidthUserAvatar);

                    return thumbName;
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
            return HttpContext.Current.Server.MapPath("~/data/user_img/" + folder+"/");
        }
    }
}
