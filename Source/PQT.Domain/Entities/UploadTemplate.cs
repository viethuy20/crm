using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class UploadTemplate : Entity
    {
        public UploadTemplate()
        {
            Departments = new List<string>();
        }
        public string Department { get; set; }
        public string GroupName { get; set; }
        public string FileName { get; set; }
        public bool ReadOnly { get; set; }
        public bool GroupNameEquals(string group)
        {
            return GroupNameCode == StringHelper.RemoveSpecialCharacters(group.Trim().ToLower());
        }
        public string GroupNameCode
        {
            get
            {
                return StringHelper.RemoveSpecialCharacters(GroupName.Trim().ToLower());
            }
        }
        public string Type
        {
            get
            {
                if (ReadOnly)
                {
                    return "Non Downloadable Template";
                }
                return "Template";
            }
        }
        [NotMapped]
        public List<string> Departments { get; set; }
        [NotMapped]
        public HttpPostedFileBase UploadFile { get; set; }
        public DateTime UploadTime
        {
            get
            {
                if (UpdatedTime != null)
                {
                    return Convert.ToDateTime(UpdatedTime);
                }
                return CreatedTime;
            }
        }
        public string UploadTimeStr
        {
            get
            {
                return UploadTime.ToString("dd/MM/yyyy");
            }
        }
    }
}
