using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AeroportApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var context = new AeroportContext();
		}
	}

	public class AeroportContext : DbContext
	{
		public DbSet<Plane> Planes { get; set; }
		public DbSet<PlaneModel> Models { get; set; }
		public DbSet<Flight> Flights { get; set; }
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<Passenger> Passengers { get; set; }


		public AeroportContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			//options.UseSqlite(@"Data Source=db.db");
			options.UseSqlServer(@"Server=localhost; Database=AeroportDb; Integrated Security=true; MultipleActiveResultSets=True; App=EntityFramework");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Passenger>(builder =>
			{
				builder.HasKey(a => a.Id);
				builder.HasMany(a => a.Tickets)
					.WithOne(a => a.Passenger);
			});

			modelBuilder.Entity<Ticket>(builder =>
			{
				builder.HasKey(a => new {a.FlightId, a.PassengerId});
				builder.HasOne(a => a.Flight).WithMany(a => a.Tickets);
				builder.HasOne(a => a.Passenger).WithMany(a => a.Tickets);
			});

			modelBuilder.Entity<Flight>(builder =>
			{
				builder.HasKey(a => a.Id);
				builder.HasOne(a => a.Plane).WithMany(a => a.Flights);
			});

			modelBuilder.Entity<Plane>(builder =>
			{
				builder.HasKey(a => a.Id);
				builder.HasOne(a => a.Model).WithMany(a => a.Planes);
			});
		}

		public async Task BookFlight(Passenger passenger, Flight flight)
		{
			//check available space
			var plane = await Planes.Include(a => a.Model).FirstOrDefaultAsync(a => a.Id == flight.PlaneId);
			var tickets = await Tickets.Where(a => a.FlightId == flight.Id).ToListAsync();

			if (plane.Model.Capacity < tickets.Count)
			{
				throw new Exception("Not enough space in a plane.");
			}

			if (tickets.Any(a => a.PassengerId == passenger.Id))
			{
				throw new Exception("A passenger can only book one ticket per flight");
			}

			Tickets.Add(new Ticket()
			{
				Flight = flight,
				Passenger = passenger
			});

			await SaveChangesAsync();
		}

		public async Task<List<Flight>> ListFlights()
		{
			return await Flights.ToListAsync();
		}

		public async Task<List<Passenger>> ListPassengers()
		{
			return await Passengers.ToListAsync();
		}

		public async Task SetDelay(Flight flight, TimeSpan span, string reason = null)
		{
			flight.Delay = span;
			flight.DelayReason = reason ?? flight.DelayReason;
			await SaveChangesAsync();
		}
	}

	public class PlaneModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double MaxSpeed { get; set; }
		public int Capacity { get; set; }

		public virtual ICollection<Plane> Planes { get; set; }
	}

	public class Plane
	{
		public int Id { get; set; }
		public int ModelId { get; set; }

		public virtual PlaneModel Model { get; set; }
		public virtual ICollection<Flight> Flights { get; set; }
	}

	public class Flight
	{
		public int Id { get; set; }
		public string FlightName { get; set; }
		public TimeSpan? Delay { get; set; }
		public string DelayReason { get; set; }
		public DateTime TakeOff { get; set; }
		public DateTime Landing { get; set; }
		public string Destination { get; set; }
		public string Origin { get; set; }
		public int PlaneId { get; set; }

		public virtual Plane Plane { get; set; }
		public virtual ICollection<Ticket> Tickets { get; set; }
	}

	public class Ticket
	{
		public int PassengerId { get; set; }
		public int FlightId { get; set; }

		public virtual Flight Flight { get; set; }
		public virtual Passenger Passenger { get; set; }
	}

	public class Passenger
	{
		public int Id { get; set; }
		public string PassengerName { get; set; }

		public virtual ICollection<Ticket> Tickets { get; set; }
	}
}


/*
 5.Написати реалізацію програми рейси аеропорту
Користувач програми – обслуговуючий аеропорту. Рейс може бути відкладеним
при затримці через погодні умови чи технічні причини. Користувач програми має змогу
продивитись існуючі в поточному аеропорту рейси, та перелік пасажирів, що придбали на
нього квитки, задати кінцевий час, після якого покупка квитків більше не можлива, а
також час затримки поточного рейсу. Рейс може обслуговувати певний літак, літаки
мають обмежену кількість місць у кожному, існують квитки на них, та клієнти, що вже
придбали квитки на певний рейс.
Основні сутності моделі: літак, рейс, клієнт, квиток.  
 */