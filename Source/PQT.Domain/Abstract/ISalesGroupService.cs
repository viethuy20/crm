using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ISalesGroupService
    {
        IEnumerable<SalesGroup> GetAllSalesGroups();
        SalesGroup GetSalesGroup(int id);
        SalesGroup GetSalesGroup(string name);
        SalesGroup CreateSalesGroup(SalesGroup info, IEnumerable<int> users);
        SalesGroup UpdateSalesGroup(int id, string groupName, IEnumerable<int> users);
        bool DeleteSalesGroup(int id);
    }
}
