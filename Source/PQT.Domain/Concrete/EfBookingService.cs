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
            return GetAll(predicate, m => m.Delegates).AsEnumerable();
        }

        public IEnumerable<Booking> GetAllBookingsForKPI(int eventId, int userId, Func<Booking, bool> predicate)
        {
            Func<Booking, bool> predicate2 = null;
            if (eventId > 0 && userId > 0)
            {
                predicate2 =
                    m => m.EventID == eventId &&
                         (m.SalesmanID == userId 
                         //|| m.Salesman.TransferUserID == userId
                         )
                         && predicate(m);
            }
            else if (eventId > 0)
            {
                predicate2 =
                    m => m.EventID == eventId
                         && predicate(m);
            }
            else if (userId > 0)
            {
                predicate2 =
                    //m => (m.SalesmanID == userId || m.Salesman.TransferUserID == userId)
                    m => (m.SalesmanID == userId )
                         && predicate(m);
            }
            else
            {
                predicate2 =
                    m => predicate(m);
            }

            return _db.Set<Booking>()
                .Include(m => m.BookingStatusRecord)
                .Include(m => m.Salesman)
                .Where(predicate2).AsEnumerable();

        }
        public Booking GetBooking(int id)
        {
            return Get<Booking>(m => m.ID == id);
        }
        public Booking GetBookingByLeadId(int leadId)
        {
            return Get<Booking>(m => m.LeadID == leadId);
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

                if (info.Delegates != null && info.Delegates.Any())
                {
                    foreach (var item in existBooking.Delegates.Where(m => !info.Delegates.Select(n => n.ID).Contains(m.ID)).ToList())
                    {
                        existBooking.Delegates.Remove(item);
                        Delete(item);
                    }
                    UpdateCollection(info, m => m.ID == info.ID, m => m.Delegates, m => m.ID);
                }
                else if (existBooking.Delegates != null)
                    foreach (var item in existBooking.Delegates.ToList())
                    {
                        existBooking.Delegates.Remove(item);
                        Delete(item);
                    }

                return Update(existBooking);
            });
        }

        public bool DeleteBooking(int id)
        {
            return Delete<Booking>(id);
        }

        public Entities.Delegate GetDelegate(int id)
        {
            return Get<Entities.Delegate>(id);
        }

        public bool UpdateDelegate(Entities.Delegate info)
        {
            return Update(info);
        }
    }
}
