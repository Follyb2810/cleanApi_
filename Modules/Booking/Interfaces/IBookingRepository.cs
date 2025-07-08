using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Modules.Booking.Interfaces
{
    public class IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsForUser(int userId);

    }
}
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Booking>> GetBookingsForUser(int userId)
    {
        return await _dbSet.Where(b => b.UserId == userId).ToListAsync();
    }
}
