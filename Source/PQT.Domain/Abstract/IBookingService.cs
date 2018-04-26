using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookings();
        IEnumerable<Booking> GetAllBookings(Func<Booking, bool> predicate);
        Booking GetBooking(int id);
        Booking CreateBooking(Booking info);
        Booking CreateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "");
        bool UpdateBooking(Booking info);
        bool UpdateBooking(Booking info, BookingStatus status, int? userId, string message = "");
        bool UpdateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "");
        bool DeleteBooking(int id);
    }
}
