using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EfBookingService : Repository, IBookingService
    {
        public EfBookingService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<Booking> GetAllBookings()
        {
            return GetAll<Booking>().AsEnumerable();
        }
        public IEnumerable<Booking> GetAllBookings(Func<Booking, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }

        public Booking GetBooking(int id)
        {
            return Get<Booking>(m => m.ID == id);
        }

        public Booking CreateBooking(Booking info)
        {
            return Create(info);
        }

        public Booking CreateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "")
        {
            return TransactionWrapper.Do(() =>
            {
                info.EventSessions = GetAll<EventSession>(r => sessionIds.Contains(r.ID)).ToList();
                info = Create(info);
                info.BookingStatusRecord = new BookingStatusRecord(info.ID, BookingStatus.Initial, userId, message);
                Update(info);
                return info;
            });
        }

        public bool UpdateBooking(Booking info)
        {
            return Update(info);
        }

        public bool UpdateBooking(Booking info, BookingStatus status, int? userId, string message = "")
        {
            return TransactionWrapper.Do(() =>
            {
                if (info == null)
                    return false;
                info.BookingStatusRecord = new BookingStatusRecord(info.ID, status, userId, message);
                return Update(info);
            });
        }

        public bool UpdateBooking(Booking info, IEnumerable<int> sessionIds, int? userId, string message = "")
        {
            return TransactionWrapper.Do(() =>
            {
                var existBooking = Get<Booking>(info.ID);
                if (existBooking == null)
                    return false;

                Update(info);

                existBooking.EventSessions.Clear();
                existBooking.EventSessions = GetAll<EventSession>(r => sessionIds.Contains(r.ID)).ToList();

                if (existBooking.BookingStatusRecord != BookingStatus.Approved)
                {
                    existBooking.BookingStatusRecord = new BookingStatusRecord(info.ID, BookingStatus.Initial, userId, message);
                }
                return Update(existBooking);
            });
        }

        public bool DeleteBooking(int id)
        {
            return Delete<Booking>(id);
        }
    }
}
