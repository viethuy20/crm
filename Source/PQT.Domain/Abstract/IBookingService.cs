using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookings();
        IEnumerable<Booking> GetAllBookings(Func<Booking, bool> predicate);
        Booking GetBooking(int id);
        Booking CreateBooking(Booking info);
        bool UpdateBooking(Booking info);
        bool DeleteBooking(int id);
    }
}
