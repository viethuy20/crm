using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Entities;

namespace PQT.Web.Models
{
    public class BrowseModel
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        public int MinMileage { get; set; }
        public int MaxMileage { get; set; }
        public string Model { get; set; }
        public string SortBy { get; set; }
    }
}