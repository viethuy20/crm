using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class TrainerBank:Entity
    {
        public string BankName { get; set; }
        public string BankAccountHolderName { get; set; }
        public string BankAccountHolderAddress { get; set; }
        public string BankAccountNumber { get; set; }//Bank Account Number/ IBAN
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
    }
}
