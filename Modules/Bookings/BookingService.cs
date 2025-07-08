// // Program.cs - Main entry point
// using CleanConnect.Infrastructure.Extensions;
// using CleanConnect.Modules.Users.Extensions;
// using CleanConnect.Modules.Bookings.Extensions;
// using CleanConnect.Modules.Payments.Extensions;
// using CleanConnect.Modules.Subscriptions.Extensions;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// // Add Authentication
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//         };
//     });

// // Add Infrastructure
// builder.Services.AddInfrastructure(builder.Configuration);

// // Add Modules
// builder.Services.AddUsersModule(builder.Configuration);
// builder.Services.AddBookingsModule(builder.Configuration);
// builder.Services.AddPaymentsModule(builder.Configuration);
// builder.Services.AddSubscriptionsModule(builder.Configuration);

// var app = builder.Build();

// // Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();

// app.Run();

// // ===== SHARED KERNEL =====

// // Shared/Domain/BaseEntity.cs
// namespace CleanConnect.Shared.Domain
// {
//     public abstract class BaseEntity
//     {
//         public Guid Id { get; protected set; } = Guid.NewGuid();
//         public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
//         public DateTime? UpdatedAt { get; protected set; }
        
//         protected void SetUpdatedAt()
//         {
//             UpdatedAt = DateTime.UtcNow;
//         }
//     }
// }

// // Shared/Domain/IRepository.cs
// namespace CleanConnect.Shared.Domain
// {
//     public interface IRepository<T> where T : BaseEntity
//     {
//         Task<T?> GetByIdAsync(Guid id);
//         Task<IEnumerable<T>> GetAllAsync();
//         Task<T> AddAsync(T entity);
//         Task UpdateAsync(T entity);
//         Task DeleteAsync(Guid id);
//     }
// }

// // Shared/Domain/Result.cs
// namespace CleanConnect.Shared.Domain
// {
//     public class Result<T>
//     {
//         public bool IsSuccess { get; private set; }
//         public T? Data { get; private set; }
//         public string? Error { get; private set; }

//         private Result(bool isSuccess, T? data, string? error)
//         {
//             IsSuccess = isSuccess;
//             Data = data;
//             Error = error;
//         }

//         public static Result<T> Success(T data) => new(true, data, null);
//         public static Result<T> Failure(string error) => new(false, default, error);
//     }
// }

// // ===== INFRASTRUCTURE MODULE =====

// // Infrastructure/Data/AppDbContext.cs
// using CleanConnect.Modules.Users.Domain;
// using CleanConnect.Modules.Bookings.Domain;
// using CleanConnect.Modules.Payments.Domain;
// using CleanConnect.Modules.Subscriptions.Domain;
// using Microsoft.EntityFrameworkCore;

// namespace CleanConnect.Infrastructure.Data
// {
//     public class AppDbContext : DbContext
//     {
//         public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//         // Users Module
//         public DbSet<User> Users { get; set; }
        
//         // Bookings Module
//         public DbSet<Appointment> Appointments { get; set; }
//         public DbSet<CleaningService> CleaningServices { get; set; }
        
//         // Payments Module
//         public DbSet<Payment> Payments { get; set; }
        
//         // Subscriptions Module
//         public DbSet<Subscription> Subscriptions { get; set; }
//         public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             // Configure entities
//             modelBuilder.Entity<User>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
//                 entity.HasIndex(e => e.Email).IsUnique();
//             });

//             modelBuilder.Entity<Appointment>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Status).HasConversion<string>();
//                 entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
//                 entity.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId);
//             });

//             modelBuilder.Entity<CleaningService>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
//             });

//             modelBuilder.Entity<Payment>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
//                 entity.Property(e => e.Status).HasConversion<string>();
//             });

//             modelBuilder.Entity<Subscription>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Status).HasConversion<string>();
//                 entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
//                 entity.HasOne(e => e.Plan).WithMany().HasForeignKey(e => e.PlanId);
//             });

//             modelBuilder.Entity<SubscriptionPlan>(entity =>
//             {
//                 entity.HasKey(e => e.Id);
//                 entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
//                 entity.Property(e => e.Duration).HasConversion<string>();
//             });
//         }
//     }
// }

// // Infrastructure/Extensions/ServiceCollectionExtensions.cs
// using CleanConnect.Infrastructure.Data;
// using CleanConnect.Shared.Domain;
// using Microsoft.EntityFrameworkCore;

// namespace CleanConnect.Infrastructure.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
//         {
//             services.AddDbContext<AppDbContext>(options =>
//                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

//             services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//             return services;
//         }
//     }

//     public class Repository<T> : IRepository<T> where T : BaseEntity
//     {
//         private readonly AppDbContext _context;
//         private readonly DbSet<T> _dbSet;

//         public Repository(AppDbContext context)
//         {
//             _context = context;
//             _dbSet = context.Set<T>();
//         }

//         public async Task<T?> GetByIdAsync(Guid id)
//         {
//             return await _dbSet.FindAsync(id);
//         }

//         public async Task<IEnumerable<T>> GetAllAsync()
//         {
//             return await _dbSet.ToListAsync();
//         }

//         public async Task<T> AddAsync(T entity)
//         {
//             await _dbSet.AddAsync(entity);
//             await _context.SaveChangesAsync();
//             return entity;
//         }

//         public async Task UpdateAsync(T entity)
//         {
//             _dbSet.Update(entity);
//             await _context.SaveChangesAsync();
//         }

//         public async Task DeleteAsync(Guid id)
//         {
//             var entity = await GetByIdAsync(id);
//             if (entity != null)
//             {
//                 _dbSet.Remove(entity);
//                 await _context.SaveChangesAsync();
//             }
//         }
//     }
// }

// // ===== USERS MODULE =====

// // Modules/Users/Domain/User.cs
// using CleanConnect.Shared.Domain;

// namespace CleanConnect.Modules.Users.Domain
// {
//     public class User : BaseEntity
//     {
//         public string FirstName { get; private set; } = string.Empty;
//         public string LastName { get; private set; } = string.Empty;
//         public string Email { get; private set; } = string.Empty;
//         public string PasswordHash { get; private set; } = string.Empty;
//         public string PhoneNumber { get; private set; } = string.Empty;
//         public string Address { get; private set; } = string.Empty;
//         public bool IsEmailVerified { get; private set; }

//         public static User Create(string firstName, string lastName, string email, string passwordHash, string phoneNumber, string address)
//         {
//             return new User
//             {
//                 FirstName = firstName,
//                 LastName = lastName,
//                 Email = email,
//                 PasswordHash = passwordHash,
//                 PhoneNumber = phoneNumber,
//                 Address = address,
//                 IsEmailVerified = false
//             };
//         }

//         public void UpdateProfile(string firstName, string lastName, string phoneNumber, string address)
//         {
//             FirstName = firstName;
//             LastName = lastName;
//             PhoneNumber = phoneNumber;
//             Address = address;
//             SetUpdatedAt();
//         }

//         public void VerifyEmail()
//         {
//             IsEmailVerified = true;
//             SetUpdatedAt();
//         }
//     }
// }

// // Modules/Users/Application/Commands/RegisterUserCommand.cs
// using CleanConnect.Shared.Domain;
// using CleanConnect.Modules.Users.Domain;

// namespace CleanConnect.Modules.Users.Application.Commands
// {
//     public record RegisterUserCommand(
//         string FirstName,
//         string LastName,
//         string Email,
//         string Password,
//         string PhoneNumber,
//         string Address
//     );

//     public record LoginCommand(string Email, string Password);

//     public class UserService
//     {
//         private readonly IRepository<User> _userRepository;

//         public UserService(IRepository<User> userRepository)
//         {
//             _userRepository = userRepository;
//         }

//         public async Task<Result<User>> RegisterAsync(RegisterUserCommand command)
//         {
//             // Check if user already exists
//             var existingUsers = await _userRepository.GetAllAsync();
//             if (existingUsers.Any(u => u.Email == command.Email))
//             {
//                 return Result<User>.Failure("User with this email already exists");
//             }

//             // Hash password (in real implementation, use proper password hashing)
//             var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

//             var user = User.Create(
//                 command.FirstName,
//                 command.LastName,
//                 command.Email,
//                 passwordHash,
//                 command.PhoneNumber,
//                 command.Address
//             );

//             var createdUser = await _userRepository.AddAsync(user);
//             return Result<User>.Success(createdUser);
//         }

//         public async Task<Result<User>> LoginAsync(LoginCommand command)
//         {
//             var users = await _userRepository.GetAllAsync();
//             var user = users.FirstOrDefault(u => u.Email == command.Email);

//             if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
//             {
//                 return Result<User>.Failure("Invalid email or password");
//             }

//             return Result<User>.Success(user);
//         }
//     }
// }

// // Modules/Users/Presentation/UsersController.cs
// using CleanConnect.Modules.Users.Application.Commands;
// using Microsoft.AspNetCore.Mvc;

// namespace CleanConnect.Modules.Users.Presentation
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class UsersController : ControllerBase
//     {
//         private readonly UserService _userService;

//         public UsersController(UserService userService)
//         {
//             _userService = userService;
//         }

//         [HttpPost("register")]
//         public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
//         {
//             var result = await _userService.RegisterAsync(command);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "User registered successfully", UserId = result.Data!.Id });
//         }

//         [HttpPost("login")]
//         public async Task<IActionResult> Login([FromBody] LoginCommand command)
//         {
//             var result = await _userService.LoginAsync(command);
            
//             if (!result.IsSuccess)
//                 return Unauthorized(result.Error);

//             // In real implementation, generate JWT token here
//             return Ok(new { Message = "Login successful", UserId = result.Data!.Id });
//         }
//     }
// }

// // Modules/Users/Extensions/ServiceCollectionExtensions.cs
// using CleanConnect.Modules.Users.Application.Commands;

// namespace CleanConnect.Modules.Users.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
//         {
//             services.AddScoped<UserService>();
//             return services;
//         }
//     }
// }

// // ===== BOOKINGS MODULE =====

// // Modules/Bookings/Domain/CleaningService.cs
// using CleanConnect.Shared.Domain;

// namespace CleanConnect.Modules.Bookings.Domain
// {
//     public class CleaningService : BaseEntity
//     {
//         public string Name { get; private set; } = string.Empty;
//         public string Description { get; private set; } = string.Empty;
//         public decimal Price { get; private set; }
//         public int DurationMinutes { get; private set; }
//         public bool IsActive { get; private set; }

//         public static CleaningService Create(string name, string description, decimal price, int durationMinutes)
//         {
//             return new CleaningService
//             {
//                 Name = name,
//                 Description = description,
//                 Price = price,
//                 DurationMinutes = durationMinutes,
//                 IsActive = true
//             };
//         }
//     }

//     public enum AppointmentStatus
//     {
//         Scheduled,
//         InProgress,
//         Completed,
//         Cancelled
//     }

//     public class Appointment : BaseEntity
//     {
//         public Guid UserId { get; private set; }
//         public Guid ServiceId { get; private set; }
//         public DateTime ScheduledDate { get; private set; }
//         public string Address { get; private set; } = string.Empty;
//         public string? SpecialInstructions { get; private set; }
//         public AppointmentStatus Status { get; private set; }
//         public decimal TotalAmount { get; private set; }

//         // Navigation properties
//         public CleaningService Service { get; private set; } = null!;

//         public static Appointment Create(Guid userId, Guid serviceId, DateTime scheduledDate, string address, decimal totalAmount, string? specialInstructions = null)
//         {
//             return new Appointment
//             {
//                 UserId = userId,
//                 ServiceId = serviceId,
//                 ScheduledDate = scheduledDate,
//                 Address = address,
//                 SpecialInstructions = specialInstructions,
//                 Status = AppointmentStatus.Scheduled,
//                 TotalAmount = totalAmount
//             };
//         }

//         public void UpdateStatus(AppointmentStatus status)
//         {
//             Status = status;
//             SetUpdatedAt();
//         }
//     }
// }

// // Modules/Bookings/Application/BookingService.cs
// using CleanConnect.Shared.Domain;
// using CleanConnect.Modules.Bookings.Domain;

// namespace CleanConnect.Modules.Bookings.Application
// {
//     public record BookAppointmentCommand(
//         Guid UserId,
//         Guid ServiceId,
//         DateTime ScheduledDate,
//         string Address,
//         string? SpecialInstructions
//     );

//     public class BookingService
//     {
//         private readonly IRepository<Appointment> _appointmentRepository;
//         private readonly IRepository<CleaningService> _serviceRepository;

//         public BookingService(IRepository<Appointment> appointmentRepository, IRepository<CleaningService> serviceRepository)
//         {
//             _appointmentRepository = appointmentRepository;
//             _serviceRepository = serviceRepository;
//         }

//         public async Task<Result<Appointment>> BookAppointmentAsync(BookAppointmentCommand command)
//         {
//             var service = await _serviceRepository.GetByIdAsync(command.ServiceId);
//             if (service == null)
//             {
//                 return Result<Appointment>.Failure("Service not found");
//             }

//             if (command.ScheduledDate <= DateTime.UtcNow)
//             {
//                 return Result<Appointment>.Failure("Appointment must be scheduled for a future date");
//             }

//             var appointment = Appointment.Create(
//                 command.UserId,
//                 command.ServiceId,
//                 command.ScheduledDate,
//                 command.Address,
//                 service.Price,
//                 command.SpecialInstructions
//             );

//             var createdAppointment = await _appointmentRepository.AddAsync(appointment);
//             return Result<Appointment>.Success(createdAppointment);
//         }

//         public async Task<Result<IEnumerable<Appointment>>> GetUserAppointmentsAsync(Guid userId)
//         {
//             var appointments = await _appointmentRepository.GetAllAsync();
//             var userAppointments = appointments.Where(a => a.UserId == userId);
//             return Result<IEnumerable<Appointment>>.Success(userAppointments);
//         }

//         public async Task<Result<IEnumerable<CleaningService>>> GetAvailableServicesAsync()
//         {
//             var services = await _serviceRepository.GetAllAsync();
//             var activeServices = services.Where(s => s.IsActive);
//             return Result<IEnumerable<CleaningService>>.Success(activeServices);
//         }
//     }
// }

// // Modules/Bookings/Presentation/BookingsController.cs
// using CleanConnect.Modules.Bookings.Application;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;

// namespace CleanConnect.Modules.Bookings.Presentation
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class BookingsController : ControllerBase
//     {
//         private readonly BookingService _bookingService;

//         public BookingsController(BookingService bookingService)
//         {
//             _bookingService = bookingService;
//         }

//         [HttpGet("services")]
//         public async Task<IActionResult> GetServices()
//         {
//             var result = await _bookingService.GetAvailableServicesAsync();
//             return Ok(result.Data);
//         }

//         [HttpPost("appointments")]
//         public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentCommand command)
//         {
//             var result = await _bookingService.BookAppointmentAsync(command);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "Appointment booked successfully", AppointmentId = result.Data!.Id });
//         }

//         [HttpGet("appointments/{userId}")]
//         public async Task<IActionResult> GetUserAppointments(Guid userId)
//         {
//             var result = await _bookingService.GetUserAppointmentsAsync(userId);
//             return Ok(result.Data);
//         }
//     }
// }

// // Modules/Bookings/Extensions/ServiceCollectionExtensions.cs
// using CleanConnect.Modules.Bookings.Application;

// namespace CleanConnect.Modules.Bookings.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddBookingsModule(this IServiceCollection services, IConfiguration configuration)
//         {
//             services.AddScoped<BookingService>();
//             return services;
//         }
//     }
// }

// // ===== PAYMENTS MODULE =====

// // Modules/Payments/Domain/Payment.cs
// using CleanConnect.Shared.Domain;

// namespace CleanConnect.Modules.Payments.Domain
// {
//     public enum PaymentStatus
//     {
//         Pending,
//         Completed,
//         Failed,
//         Refunded
//     }

//     public enum PaymentMethod
//     {
//         CreditCard,
//         DebitCard,
//         PayPal,
//         BankTransfer
//     }

//     public class Payment : BaseEntity
//     {
//         public Guid UserId { get; private set; }
//         public Guid? AppointmentId { get; private set; }
//         public Guid? SubscriptionId { get; private set; }
//         public decimal Amount { get; private set; }
//         public PaymentStatus Status { get; private set; }
//         public PaymentMethod Method { get; private set; }
//         public string? TransactionId { get; private set; }
//         public DateTime? ProcessedAt { get; private set; }

//         public static Payment Create(Guid userId, decimal amount, PaymentMethod method, Guid? appointmentId = null, Guid? subscriptionId = null)
//         {
//             return new Payment
//             {
//                 UserId = userId,
//                 AppointmentId = appointmentId,
//                 SubscriptionId = subscriptionId,
//                 Amount = amount,
//                 Status = PaymentStatus.Pending,
//                 Method = method
//             };
//         }

//         public void MarkAsCompleted(string transactionId)
//         {
//             Status = PaymentStatus.Completed;
//             TransactionId = transactionId;
//             ProcessedAt = DateTime.UtcNow;
//             SetUpdatedAt();
//         }

//         public void MarkAsFailed()
//         {
//             Status = PaymentStatus.Failed;
//             ProcessedAt = DateTime.UtcNow;
//             SetUpdatedAt();
//         }
//     }
// }

// // Modules/Payments/Application/PaymentService.cs
// using CleanConnect.Shared.Domain;
// using CleanConnect.Modules.Payments.Domain;

// namespace CleanConnect.Modules.Payments.Application
// {
//     public record ProcessPaymentCommand(
//         Guid UserId,
//         decimal Amount,
//         PaymentMethod Method,
//         Guid? AppointmentId = null,
//         Guid? SubscriptionId = null
//     );

//     public class PaymentService
//     {
//         private readonly IRepository<Payment> _paymentRepository;

//         public PaymentService(IRepository<Payment> paymentRepository)
//         {
//             _paymentRepository = paymentRepository;
//         }

//         public async Task<Result<Payment>> ProcessPaymentAsync(ProcessPaymentCommand command)
//         {
//             var payment = Payment.Create(
//                 command.UserId,
//                 command.Amount,
//                 command.Method,
//                 command.AppointmentId,
//                 command.SubscriptionId
//             );

//             // Simulate payment processing
//             var isSuccessful = await SimulatePaymentGateway(command.Amount, command.Method);

//             if (isSuccessful)
//             {
//                 payment.MarkAsCompleted($"TXN_{Guid.NewGuid():N}");
//             }
//             else
//             {
//                 payment.MarkAsFailed();
//             }

//             var processedPayment = await _paymentRepository.AddAsync(payment);
            
//             if (!isSuccessful)
//             {
//                 return Result<Payment>.Failure("Payment processing failed");
//             }

//             return Result<Payment>.Success(processedPayment);
//         }

//         private async Task<bool> SimulatePaymentGateway(decimal amount, PaymentMethod method)
//         {
//             // Simulate network delay
//             await Task.Delay(1000);
            
//             // Simulate 95% success rate
//             return Random.Shared.NextDouble() > 0.05;
//         }

//         public async Task<Result<IEnumerable<Payment>>> GetUserPaymentsAsync(Guid userId)
//         {
//             var payments = await _paymentRepository.GetAllAsync();
//             var userPayments = payments.Where(p => p.UserId == userId);
//             return Result<IEnumerable<Payment>>.Success(userPayments);
//         }
//     }
// }

// // Modules/Payments/Presentation/PaymentsController.cs
// using CleanConnect.Modules.Payments.Application;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;

// namespace CleanConnect.Modules.Payments.Presentation
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class PaymentsController : ControllerBase
//     {
//         private readonly PaymentService _paymentService;

//         public PaymentsController(PaymentService paymentService)
//         {
//             _paymentService = paymentService;
//         }

//         [HttpPost("process")]
//         public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
//         {
//             var result = await _paymentService.ProcessPaymentAsync(command);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "Payment processed successfully", PaymentId = result.Data!.Id });
//         }

//         [HttpGet("user/{userId}")]
//         public async Task<IActionResult> GetUserPayments(Guid userId)
//         {
//             var result = await _paymentService.GetUserPaymentsAsync(userId);
//             return Ok(result.Data);
//         }
//     }
// }

// // Modules/Payments/Extensions/ServiceCollectionExtensions.cs
// using CleanConnect.Modules.Payments.Application;

// namespace CleanConnect.Modules.Payments.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
//         {
//             services.AddScoped<PaymentService>();
//             return services;
//         }
//     }
// }

// // ===== SUBSCRIPTIONS MODULE =====

// // Modules/Subscriptions/Domain/SubscriptionPlan.cs
// using CleanConnect.Shared.Domain;

// namespace CleanConnect.Modules.Subscriptions.Domain
// {
//     public enum SubscriptionDuration
//     {
//         Monthly,
//         Quarterly,
//         SemiAnnually,
//         Annually
//     }

//     public class SubscriptionPlan : BaseEntity
//     {
//         public string Name { get; private set; } = string.Empty;
//         public string Description { get; private set; } = string.Empty;
//         public decimal Price { get; private set; }
//         public SubscriptionDuration Duration { get; private set; }
//         public int CleaningsPerMonth { get; private set; }
//         public bool IsActive { get; private set; }
//         public decimal? DiscountPercentage { get; private set; }

//         public static SubscriptionPlan Create(string name, string description, decimal price, SubscriptionDuration duration, int cleaningsPerMonth, decimal? discountPercentage = null)
//         {
//             return new SubscriptionPlan
//             {
//                 Name = name,
//                 Description = description,
//                 Price = price,
//                 Duration = duration,
//                 CleaningsPerMonth = cleaningsPerMonth,
//                 IsActive = true,
//                 DiscountPercentage = discountPercentage
//             };
//         }

//         public decimal GetTotalPrice()
//         {
//             var months = Duration switch
//             {
//                 SubscriptionDuration.Monthly => 1,
//                 SubscriptionDuration.Quarterly => 3,
//                 SubscriptionDuration.SemiAnnually => 6,
//                 SubscriptionDuration.Annually => 12,
//                 _ => 1
//             };

//             var totalPrice = Price * months;
            
//             if (DiscountPercentage.HasValue)
//             {
//                 totalPrice *= (1 - DiscountPercentage.Value / 100);
//             }

//             return totalPrice;
//         }
//     }

//     public enum SubscriptionStatus
//     {
//         Active,
//         Cancelled,
//         Expired,
//         Suspended
//     }

//     public class Subscription : BaseEntity
//     {
//         public Guid UserId { get; private set; }
//         public Guid PlanId { get; private set; }
//         public DateTime StartDate { get; private set; }
//         public DateTime EndDate { get; private set; }
//         public SubscriptionStatus Status { get; private set; }
//         public int RemainingCleanings { get; private set; }
//         public DateTime? NextRenewalDate { get; private set; }
//         public decimal AmountPaid { get; private set; }

//         // Navigation properties
//         public SubscriptionPlan Plan { get; private set; } = null!;

//         public static Subscription Create(Guid userId, Guid planId, SubscriptionPlan plan, decimal amountPaid)
//         {
//             var startDate = DateTime.UtcNow;
//             var endDate = plan.Duration switch
//             {
//                 SubscriptionDuration.Monthly => startDate.AddMonths(1),
//                 SubscriptionDuration.Quarterly => startDate.AddMonths(3),
//                 SubscriptionDuration.SemiAnnually => startDate.AddMonths(6),
//                 SubscriptionDuration.Annually => startDate.AddMonths(12),
//                 _ => startDate.AddMonths(1)
//             };

//             var totalCleanings = plan.CleaningsPerMonth * (plan.Duration switch
//             {
//                 SubscriptionDuration.Monthly => 1,
//                 SubscriptionDuration.Quarterly => 3,
//                 SubscriptionDuration.SemiAnnually => 6,
//                 SubscriptionDuration.Annually => 12,
//                 _ => 1
//             });

//             return new Subscription
//             {
//                 UserId = userId,
//                 PlanId = planId,
//                 StartDate = startDate,
//                 EndDate = endDate,
//                 Status = SubscriptionStatus.Active,
//                 RemainingCleanings = totalCleanings,
//                 NextRenewalDate = endDate,
//                 AmountPaid = amountPaid
//             };
//         }

//         public void UseCleaningService()
//         {
//             if (RemainingCleanings > 0)
//             {
//                 RemainingCleanings--;
//                 SetUpdatedAt();
//             }
//         }

//         public void Cancel()
//         {
//             Status = SubscriptionStatus.Cancelled;
//             NextRenewalDate = null;
//             SetUpdatedAt();
//         }

//         public void Renew(decimal amountPaid)
//         {
//             if (Status == SubscriptionStatus.Expired || Status == SubscriptionStatus.Cancelled)
//             {
//                 Status = SubscriptionStatus.Active;
//                 StartDate = DateTime.UtcNow;
//                 EndDate = Plan.Duration switch
//                 {
//                     SubscriptionDuration.Monthly => StartDate.AddMonths(1),
//                     SubscriptionDuration.Quarterly => StartDate.AddMonths(3),
//                     SubscriptionDuration.SemiAnnually => StartDate.AddMonths(6),
//                     SubscriptionDuration.Annually => StartDate.AddMonths(12),
//                     _ => StartDate.AddMonths(1)
//                 };
//                 NextRenewalDate = EndDate;
//                 AmountPaid = amountPaid;
                
//                 // Reset cleanings
//                 var totalCleanings = Plan.CleaningsPerMonth * (Plan.Duration switch
//                 {
//                     SubscriptionDuration.Monthly => 1,
//                     SubscriptionDuration.Quarterly => 3,
//                     SubscriptionDuration.SemiAnnually => 6,
//                     SubscriptionDuration.Annually => 12,
//                     _ => 1
//                 });
//                 RemainingCleanings = totalCleanings;
//                 SetUpdatedAt();
//             }
//         }

//         public bool IsExpired()
//         {
//             return DateTime.UtcNow > EndDate;
//         }
//     }
// }

// // Modules/Subscriptions/Application/SubscriptionService.cs
// using CleanConnect.Shared.Domain;
// using CleanConnect.Modules.Subscriptions.Domain;

// namespace CleanConnect.Modules.Subscriptions.Application
// {
//     public record CreateSubscriptionCommand(
//         Guid UserId,
//         Guid PlanId
//     );

//     public record RenewSubscriptionCommand(
//         Guid SubscriptionId,
//         decimal AmountPaid
//     );

//     public class SubscriptionService
//     {
//         private readonly IRepository<Subscription> _subscriptionRepository;
//         private readonly IRepository<SubscriptionPlan> _planRepository;

//         public SubscriptionService(IRepository<Subscription> subscriptionRepository, IRepository<SubscriptionPlan> planRepository)
//         {
//             _subscriptionRepository = subscriptionRepository;
//             _planRepository = planRepository;
//         }

//         public async Task<Result<IEnumerable<SubscriptionPlan>>> GetAvailablePlansAsync()
//         {
//             var plans = await _planRepository.GetAllAsync();
//             var activePlans = plans.Where(p => p.IsActive);
//             return Result<IEnumerable<SubscriptionPlan>>.Success(activePlans);
//         }

//         public async Task<Result<Subscription>> CreateSubscriptionAsync(CreateSubscriptionCommand command, decimal amountPaid)
//         {
//             var plan = await _planRepository.GetByIdAsync(command.PlanId);
//             if (plan == null)
//             {
//                 return Result<Subscription>.Failure("Subscription plan not found");
//             }

//             // Check if user already has an active subscription
//             var existingSubscriptions = await _subscriptionRepository.GetAllAsync();
//             var activeSubscription = existingSubscriptions.FirstOrDefault(s => 
//                 s.UserId == command.UserId && 
//                 s.Status == SubscriptionStatus.Active && 
//                 !s.IsExpired());

//             if (activeSubscription != null)
//             {
//                 return Result<Subscription>.Failure("User already has an active subscription");
//             }

//             var subscription = Subscription.Create(command.UserId, command.PlanId, plan, amountPaid);
//             var createdSubscription = await _subscriptionRepository.AddAsync(subscription);
            
//             return Result<Subscription>.Success(createdSubscription);
//         }

//         public async Task<Result<Subscription>> GetUserActiveSubscriptionAsync(Guid userId)
//         {
//             var subscriptions = await _subscriptionRepository.GetAllAsync();
//             var activeSubscription = subscriptions.FirstOrDefault(s => 
//                 s.UserId == userId && 
//                 s.Status == SubscriptionStatus.Active && 
//                 !s.IsExpired());

//             if (activeSubscription == null)
//             {
//                 return Result<Subscription>.Failure("No active subscription found");
//             }

//             return Result<Subscription>.Success(activeSubscription);
//         }

//         public async Task<Result<IEnumerable<Subscription>>> GetUserSubscriptionsAsync(Guid userId)
//         {
//             var subscriptions = await _subscriptionRepository.GetAllAsync();
//             var userSubscriptions = subscriptions.Where(s => s.UserId == userId);
//             return Result<IEnumerable<Subscription>>.Success(userSubscriptions);
//         }

//         public async Task<Result<Subscription>> CancelSubscriptionAsync(Guid subscriptionId)
//         {
//             var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
//             if (subscription == null)
//             {
//                 return Result<Subscription>.Failure("Subscription not found");
//             }

//             subscription.Cancel();
//             await _subscriptionRepository.UpdateAsync(subscription);
            
//             return Result<Subscription>.Success(subscription);
//         }

//         public async Task<Result<Subscription>> UseCleaningServiceAsync(Guid userId)
//         {
//             var activeSubscriptionResult = await GetUserActiveSubscriptionAsync(userId);
//             if (!activeSubscriptionResult.IsSuccess)
//             {
//                 return Result<Subscription>.Failure("No active subscription found");
//             }

//             var subscription = activeSubscriptionResult.Data!;
//             if (subscription.RemainingCleanings <= 0)
//             {
//                 return Result<Subscription>.Failure("No remaining cleanings in subscription");
//             }

//             subscription.UseCleaningService();
//             await _subscriptionRepository.UpdateAsync(subscription);
            
//             return Result<Subscription>.Success(subscription);
//         }

//         public async Task<Result<Subscription>> RenewSubscriptionAsync(RenewSubscriptionCommand command)
//         {
//             var subscription = await _subscriptionRepository.GetByIdAsync(command.SubscriptionId);
//             if (subscription == null)
//             {
//                 return Result<Subscription>.Failure("Subscription not found");
//             }

//             subscription.Renew(command.AmountPaid);
//             await _subscriptionRepository.UpdateAsync(subscription);
            
//             return Result<Subscription>.Success(subscription);
//         }
//     }
// }

// // Modules/Subscriptions/Presentation/SubscriptionsController.cs
// using CleanConnect.Modules.Subscriptions.Application;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;

// namespace CleanConnect.Modules.Subscriptions.Presentation
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class SubscriptionsController : ControllerBase
//     {
//         private readonly SubscriptionService _subscriptionService;

//         public SubscriptionsController(SubscriptionService subscriptionService)
//         {
//             _subscriptionService = subscriptionService;
//         }

//         [HttpGet("plans")]
//         public async Task<IActionResult> GetPlans()
//         {
//             var result = await _subscriptionService.GetAvailablePlansAsync();
//             return Ok(result.Data);
//         }

//         [HttpPost("subscribe")]
//         public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand command, [FromQuery] decimal amountPaid)
//         {
//             var result = await _subscriptionService.CreateSubscriptionAsync(command, amountPaid);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "Subscription created successfully", SubscriptionId = result.Data!.Id });
//         }

//         [HttpGet("user/{userId}")]
//         public async Task<IActionResult> GetUserSubscriptions(Guid userId)
//         {
//             var result = await _subscriptionService.GetUserSubscriptionsAsync(userId);
//             return Ok(result.Data);
//         }

//         [HttpGet("user/{userId}/active")]
//         public async Task<IActionResult> GetActiveSubscription(Guid userId)
//         {
//             var result = await _subscriptionService.GetUserActiveSubscriptionAsync(userId);
            
//             if (!result.IsSuccess)
//                 return NotFound(result.Error);

//             return Ok(result.Data);
//         }

//         [HttpPost("{subscriptionId}/cancel")]
//         public async Task<IActionResult> CancelSubscription(Guid subscriptionId)
//         {
//             var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "Subscription cancelled successfully" });
//         }

//         [HttpPost("{subscriptionId}/renew")]
//         public async Task<IActionResult> RenewSubscription(Guid subscriptionId, [FromBody] RenewSubscriptionCommand command)
//         {
//             var result = await _subscriptionService.RenewSubscriptionAsync(command);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { Message = "Subscription renewed successfully" });
//         }

//         [HttpPost("use-cleaning/{userId}")]
//         public async Task<IActionResult> UseCleaningService(Guid userId)
//         {
//             var result = await _subscriptionService.UseCleaningServiceAsync(userId);
            
//             if (!result.IsSuccess)
//                 return BadRequest(result.Error);

//             return Ok(new { 
//                 Message = "Cleaning service used successfully", 
//                 RemainingCleanings = result.Data!.RemainingCleanings 
//             });
//         }
//     }
// }

// // Modules/Subscriptions/Extensions/ServiceCollectionExtensions.cs
// using CleanConnect.Modules.Subscriptions.Application;

// namespace CleanConnect.Modules.Subscriptions.Extensions
// {
//     public static class ServiceCollectionExtensions
//     {
//         public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services, IConfiguration configuration)
//         {
//             services.AddScoped<SubscriptionService>();
//             return services;
//         }
//     }
// }

// // ===== CONFIGURATION FILES =====

// // appsettings.json
// {
//   "ConnectionStrings": {
//     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanConnectDb;Trusted_Connection=true;MultipleActiveResultSets=true"
//   },
//   "Jwt": {
//     "Key": "your-super-secret-key-that-is-at-least-32-characters-long",
//     "Issuer": "CleanConnect",
//     "Audience": "CleanConnectUsers"
//   },
//   "Logging": {
//     "LogLevel": {
//       "Default": "Information",
//       "Microsoft.AspNetCore": "Warning"
//     }
//   },
//   "AllowedHosts": "*"
// }

// // CleanConnect.csproj
// <Project Sdk="Microsoft.NET.Sdk.Web">

//   <PropertyGroup>
//     <TargetFramework>net8.0</TargetFramework>
//     <Nullable>enable</Nullable>
//     <ImplicitUsings>enable</ImplicitUsings>
//   </PropertyGroup>

//   <ItemGroup>
//     <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
//     <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
//     <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
//     <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
//     <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
//     <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
//   </ItemGroup>

// </Project>

// // Database Migration Commands (run in Package Manager Console)
// /*
// Add-Migration InitialCreate
// Update-Database

// // Sample data seeding (run once after migration)
// INSERT INTO CleaningServices (Id, Name, Description, Price, DurationMinutes, IsActive, CreatedAt) VALUES
// (NEWID(), 'Basic Cleaning', 'Standard house cleaning service', 75.00, 120, 1, GETUTCDATE()),
// (NEWID(), 'Deep Cleaning', 'Comprehensive deep cleaning service', 150.00, 240, 1, GETUTCDATE()),
// (NEWID(), 'Move-in/Move-out', 'Complete cleaning for moving', 200.00, 300, 1, GETUTCDATE());

// INSERT INTO SubscriptionPlans (Id, Name, Description, Price, Duration, CleaningsPerMonth, IsActive, DiscountPercentage, CreatedAt) VALUES
// (NEWID(), 'Monthly Basic', '1 cleaning per month', 70.00, 'Monthly', 1, 1, 5.00, GETUTCDATE()),
// (NEWID(), 'Quarterly Premium', '2 cleanings per month for 3 months', 130.00, 'Quarterly', 2, 1, 10.00, GETUTCDATE()),
// (NEWID(), 'Annual Deluxe', '4 cleanings per month for 12 months', 250.00, 'Annually', 4, 1, 20.00, GETUTCDATE());
// */