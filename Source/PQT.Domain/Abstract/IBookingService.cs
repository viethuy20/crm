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
        IEnumerable<Booking> GetAllBookings(Func<Booking, bool> predicate);
        IEnumerable<Booking> GetAllBookingsForKPI(int eventId, int userId, DateTime dateFrom, DateTime dateTo, string searchValue);
        IEnumerable<Booking> GetAllBookingsForTopSalesKPI(DateTime dateFrom, string searchValue);
        Booking GetBooking(int id);
        Booking GetBookingByLeadId(int leadId);
        Booking CreateBooking(Booking info);
        Booking CreateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "");
        bool UpdateBooking(Booking info);
        bool UpdateBooking(Booking info, BookingStatus status, int? userId, string message = "");
        bool UpdateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "");
        bool DeleteBooking(int id);


        PQT.Domain.Entities.Delegate GetDelegate(int id);
        bool UpdateDelegate(PQT.Domain.Entities.Delegate info);
    }
}
