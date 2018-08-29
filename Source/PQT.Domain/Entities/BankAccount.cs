using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class BankAccount : Entity
    {
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountHolderAddress { get; set; }
        public string AccountNumber { get; set; }//Bank Account Number/ IBAN
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
        public SalaryCurrency Currency { get; set; }

        public string CurrencyDisplay
        {
            get { return Currency.DisplayName; }
        }

        public string CurrencyCode
        {
            get { return Currency.Value; }
        }

        public string BankNameDescription
        {
            get { return BankName + " - " + AccountNumber; }
        }
    }
}
