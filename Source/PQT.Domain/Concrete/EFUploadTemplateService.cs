using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFUploadTemplateService : Repository, IUploadTemplateService
    {
        public EFUploadTemplateService(DbContext db)
            : base(db)
        {
        }
        public IEnumerable<UploadTemplate> GetAllUploadTemplates()
        {
            return GetAll<UploadTemplate>().AsEnumerable();
        }
        public IEnumerable<UploadTemplate> GetAllUploadTemplates(params string[] roleName)
        {
            if (!roleName.Any())
            {
                return GetAll<UploadTemplate>().AsEnumerable();
            }
            return GetAll<UploadTemplate>(m => roleName.Select(r => r.ToLower()).Contains(m.Department.ToLower())).AsEnumerable();
        }

        public UploadTemplate GetUploadTemplate(int id)
        {
            return Get<UploadTemplate>(id);
        }
        public UploadTemplate GetUploadTemplate(string group)
        {
            return Get<UploadTemplate>(m => m.GroupNameEquals(group));
        }

        public UploadTemplate CreateUploadTemplate(UploadTemplate item)
        {
            return Create(item);
        }
        public bool UpdateTemplate(UploadTemplate item)
        {
            return Update(item);
        }

        public bool DeleteTemplate(int id)
        {
            return Delete<UploadTemplate>(id);
        }

    }
}
