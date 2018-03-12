using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IProjectService
    {
        IEnumerable<ProjectEvent> GetAllProjectEvents();
        ProjectEvent GetProjectEvent(int id);
        ProjectEvent GetProjectEvent(string code);
        ProjectEvent CreateProjectEvent(ProjectEvent info);
        bool UpdateProjectEvent(ProjectEvent info);
        bool DeleteProjectEvent(int id);
    }
}
