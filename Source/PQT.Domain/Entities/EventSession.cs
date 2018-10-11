using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace PQT.Domain.Entities
{
    public class EventSession : Entity
    {
        public EventSession()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
        }
        public EventSession(EventSession info)
        {
            ID = info.ID;
            SessionTitle = info.SessionTitle;
            ShortDescription = info.ShortDescription;
            Description = info.Description;
            Address = info.Address;
            StartDate = info.StartDate;
            EndDate = info.EndDate;
            EventID = info.EventID;
            TrainerID = info.TrainerID;
            Trainer = info.Trainer;
            TrainerInvoice = info.TrainerInvoice;
            TrainerTicket = info.TrainerTicket;
            TrainerVisa = info.TrainerVisa;
            TrainerInsurance = info.TrainerInsurance;
            OperationTicket = info.OperationTicket;
            OperationVisa = info.OperationVisa;
            OperationInsurance = info.OperationInsurance;

            TrainerInvoiceRemark = info.TrainerInvoiceRemark;
            TrainerTicketRemark = info.TrainerTicketRemark;
            TrainerVisaRemark = info.TrainerVisaRemark;
            TrainerInsuranceRemark = info.TrainerInsuranceRemark;
            OperationTicketRemark = info.OperationTicketRemark;
            OperationVisaRemark = info.OperationVisaRemark;
            OperationInsuranceRemark = info.OperationInsuranceRemark;
            CreatedTime = info.CreatedTime;
            UpdatedTime = info.UpdatedTime;
        }
        public string SessionTitle { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string TrainerInvoice { get; set; }
        public string TrainerInvoiceRemark { get; set; }
        public string TrainerInvoiceUrl
        {
            get { return "/data/trainer/" + TrainerInvoice; }
        }
        [NotMapped]
        public HttpPostedFileBase TrainerInvoiceFile { get; set; }
        public string TrainerTicket { get; set; }
        public string TrainerTicketRemark { get; set; }
        public string TrainerTicketUrl
        {
            get { return "/data/trainer/" + TrainerTicket; }
        }
        [NotMapped]
        public HttpPostedFileBase TrainerTicketFile { get; set; }
        public string TrainerVisa { get; set; }
        public string TrainerVisaRemark { get; set; }
        public string TrainerVisaUrl
        {
            get { return "/data/trainer/" + TrainerVisa; }
        }
        [NotMapped]
        public HttpPostedFileBase TrainerVisaFile { get; set; }
        public string TrainerInsurance { get; set; }
        public string TrainerInsuranceRemark { get; set; }
        public string TrainerInsuranceUrl
        {
            get { return "/data/trainer/" + TrainerInsurance; }
        }
        [NotMapped]
        public HttpPostedFileBase TrainerInsuranceFile { get; set; }
        public string OperationTicket { get; set; }
        public string OperationTicketRemark { get; set; }
        public string OperationTicketUrl
        {
            get { return "/data/trainer/" + OperationTicket; }
        }
        [NotMapped]
        public HttpPostedFileBase OperationTicketFile { get; set; }
        public string OperationVisa { get; set; }
        public string OperationVisaRemark { get; set; }
        public string OperationVisaUrl
        {
            get { return "/data/trainer/" + OperationVisa; }
        }
        [NotMapped]
        public HttpPostedFileBase OperationVisaFile { get; set; }
        public string OperationInsurance { get; set; }
        public string OperationInsuranceRemark { get; set; }
        public string OperationInsuranceUrl
        {
            get { return "/data/trainer/" + OperationInsurance; }
        }
        [NotMapped]
        public HttpPostedFileBase OperationInsuranceFile { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; }

        public int? TrainerID { get; set; }
        [ForeignKey("TrainerID")]
        public virtual Trainer Trainer { get; set; }

        public string TrainerName
        {
            get
            {
                if (Trainer!=null)
                {
                    return Trainer.Name;
                }

                return "";
            }
        }
    }
}
