using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class EmailStatus : Enumeration
    {
        public static readonly EmailStatus Initial = New<EmailStatus>("", "Initial");
        public static readonly EmailStatus NER = New<EmailStatus>("NER", "NER");
        public static readonly EmailStatus Rejection = New<EmailStatus>("Rejection", "Rejection");
        public static readonly EmailStatus Unsubscribe = New<EmailStatus>("Unsubscribe Complaint", "Unsubscribe Complaint");
        public static readonly EmailStatus WrongEmail = New<EmailStatus>("Wrong Email", "Wrong Email");
    }

    public class RoundStatus : Enumeration
    {
        public static readonly RoundStatus Initial = New<RoundStatus>("", "Initial");
        public static readonly RoundStatus Sending = New<RoundStatus>("Sending", "Sending");
        public static readonly RoundStatus Sent = New<RoundStatus>("Sent", "Sent");
    }
    public class SublistEmailStatus : Enumeration
    {
        public static readonly SublistEmailStatus Initial = New<SublistEmailStatus>("", "Initial");
        public static readonly SublistEmailStatus Request = New<SublistEmailStatus>("Request", "Request");
        public static readonly SublistEmailStatus Approved = New<SublistEmailStatus>("Approved", "Approved");
        public static readonly SublistEmailStatus Rejected = New<SublistEmailStatus>("Rejected", "Rejected");
    }

}
