using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IUploadTemplateService
    {
        IEnumerable<UploadTemplate> GetAllUploadTemplates();
        IEnumerable<UploadTemplate> GetAllUploadTemplates(params string[] roleName);
        IEnumerable<UploadTemplate> GetAllUploadNonDownloadableTemplates(params string[] roleName);
        UploadTemplate GetUploadTemplate(int id);
        UploadTemplate GetUploadTemplate(string group);
        UploadTemplate CreateUploadTemplate(UploadTemplate item);
        bool UpdateTemplate(UploadTemplate item);
        bool DeleteTemplate(int id);
    }
}
