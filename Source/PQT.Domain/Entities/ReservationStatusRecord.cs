using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class ReservationStatusRecord : StatusRecord<ReservationStatus>
    {
        public ReservationStatusRecord()
        {
        }

        public ReservationStatusRecord(int entryId)
            : base(entryId)
        {
        }

        public ReservationStatusRecord(int entryId, ReservationStatus status, string message = "")
            : base(entryId, status, message)
        {
        }
        public ReservationStatusRecord(int entryId, ReservationStatus status, int? userId, string message = "")
            : base(entryId, status, userId, message)
        {
        }
    }
}
