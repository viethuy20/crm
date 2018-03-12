using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class ProjectSession : Entity
    {
        public ProjectSession()
        {
        }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ProjectEventID { get; set; }
        [ForeignKey("ProjectEventID")]
        public virtual ProjectEvent ProjectEvent { get; set; }

        public int? TrainerID { get; set; }
        [ForeignKey("TrainerID")]
        public virtual Trainer Trainer { get; set; }
    }
}
