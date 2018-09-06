using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class EntityUserStatus : Enumeration
    {
        public static readonly EntityUserStatus Normal = New<EntityUserStatus>(0, "Normal");
        public static readonly EntityUserStatus Deleted = New<EntityUserStatus>(2, "Deleted");
        public static readonly EntityUserStatus RequestEmployment = New<EntityUserStatus>(3, "Request Employment");
        public static readonly EntityUserStatus RejectEmployment = New<EntityUserStatus>(4, "Reject Employment");
        public static readonly EntityUserStatus ApprovedEmployment = New<EntityUserStatus>(5, "Approved Employment");
    }
}
