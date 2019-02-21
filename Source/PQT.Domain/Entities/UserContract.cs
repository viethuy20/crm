using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace PQT.Domain.Entities
{
    public class UserContract : EntityBase
    {
        public int Order { get; set; }
        public int UserID { get; set; }
        public string SignedContract { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
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

        public string EmploymentDateDisplay
        {
            get
            {
                if (EmploymentDate != null)
                    return Convert.ToDateTime(EmploymentDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EmploymentEndDateDisplay
        {
            get
            {
                if (EmploymentEndDate != null)
                    return Convert.ToDateTime(EmploymentEndDate).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EvaluationDate1Display
        {
            get
            {
                if (EvaluationDate1 != null)
                    return Convert.ToDateTime(EvaluationDate1).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EvaluationDate2Display
        {
            get
            {
                if (EvaluationDate2 != null)
                    return Convert.ToDateTime(EvaluationDate2).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EvaluationDate3Display
        {
            get
            {
                if (EvaluationDate3 != null)
                    return Convert.ToDateTime(EvaluationDate3).ToString("dd/MM/yyyy");
                return "";
            }
        }
        public string EvaluationDate4Display
        {
            get
            {
                if (EvaluationDate4 != null)
                    return Convert.ToDateTime(EvaluationDate4).ToString("dd/MM/yyyy");
                return "";
            }
        }
    }
}
