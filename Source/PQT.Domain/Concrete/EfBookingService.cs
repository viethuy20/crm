using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

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

        public bool UpdateBooking(Booking info)
        {
            return Update(info);
        }

        public bool DeleteBooking(int id)
        {
            return Delete<Booking>(id);
        }
    }
}
