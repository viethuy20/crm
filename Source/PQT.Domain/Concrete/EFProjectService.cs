using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFProjectService : Repository, IProjectService
    {
        public EFProjectService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<ProjectEvent> GetAllProjectEvents()
        {
            return GetAll<ProjectEvent>().AsEnumerable();
        }

        public ProjectEvent GetProjectEvent(int id)
        {
            return Get<ProjectEvent>(id);
        }
        public ProjectEvent GetProjectEvent(string code)
        {
            return Get<ProjectEvent>(m => m.Code == code.Trim().ToUpper());
        }

        public ProjectEvent CreateProjectEvent(ProjectEvent info)
        {
            info.Code = info.Code.Trim().ToUpper();
            return Create(info);
        }

        public bool UpdateProjectEvent(ProjectEvent info)
        {
            info.Code = info.Code.Trim().ToUpper();
            return Update(info);
        }

        public bool DeleteProjectEvent(int id)
        {
            return Delete<ProjectEvent>(id);
        }
    }
}
