using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CPO.Domain.Enum;

namespace CPO.Domain.Entities
{
    public class Consolidate : EntityBase
    {
        #region Factory

        public Consolidate()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Indents = new HashSet<Indent>();
        }

        #endregion

        #region Primitive

        public string ConsolidateName { get; set; }
        public string ConfirmReferenceNo { get; set; }

        [Display(Name = "Pro-forma Invoice No")]
        public string ProinformInvoiceNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime ConfirmDate { get; set; }
        #endregion

        #region Navigation
        public int? StatusRecordID { get; set; }
        [ForeignKey("StatusRecordID")]
        public virtual ConsolidateStatusRecord StatusRecord { get; set; }

        public virtual ICollection<Indent> Indents { get; set; }

        public virtual ICollection<IndentHistory> IndentHistories { get; set; }
        #endregion

        [NotMapped]
        public bool HasShippingConfirm { get; set; }

        #region LINQ helpers
        public static Func<Consolidate, bool> Is(ConsolidateStatus status, params ConsolidateStatus[] statuses)
        {
            return consolidate => consolidate.StatusRecord.Status == status ||
                                  statuses.Contains(consolidate.StatusRecord);
        }

        #endregion
    }
}
