using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class BookingStatusRecord : StatusRecord<BookingStatus>
    {
        public BookingStatusRecord()
        {
        }

        public BookingStatusRecord(int entryId)
            : base(entryId)
        {
        }

        public BookingStatusRecord(int entryId, BookingStatus status, int? userId, string message = "")
            : base(entryId, status, userId, message)
        {
        }
    }

}
