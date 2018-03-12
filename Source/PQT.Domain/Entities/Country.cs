﻿namespace PQT.Domain.Entities
{
    public class Country : EntityBase
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DialingCode { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}