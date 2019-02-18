using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace PQT.Domain.Entities
{
    public class UserContract : Entity
    {
        public int Order { get; set; }
        public int UserID { get; set; }
        public string SignedContract { get; set; }
        public string ContractPeriod { get; set; }
        public DateTime? EvaluationDate1 { get; set; }
        public string EvaluationRemark1 { get; set; }
        public DateTime? EvaluationDate2 { get; set; }
        public string EvaluationRemark2 { get; set; }
        public DateTime? EvaluationDate3 { get; set; }
        public string EvaluationRemark3 { get; set; }
        public DateTime? EvaluationDate4 { get; set; }
        public string EvaluationRemark4 { get; set; }
        [NotMapped]
        public HttpPostedFileBase SignedContractFile { get; set; }
    }
}
