using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Modules.Auth
{
    public class AuthService
    {
        public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PhoneNumber,
        string Address
    );

        public record LoginCommand(string Email, string Password);

        public class AuthService
        {
            private readonly IRepository<User> _userRepository;

            public AuthService(IRepository<User> userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<Result<User>> RegisterAsync(RegisterUserCommand command)
            {
                // Check if user already exists
                var existingUsers = await _userRepository.GetAllAsync();
                if (existingUsers.Any(u => u.Email == command.Email))
                {
                    return Result<User>.Failure("User with this email already exists");
                }

                // Hash password (in real implementation, use proper password hashing)
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

                var user = User.Create(
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    passwordHash,
                    command.PhoneNumber,
                    command.Address
                );

                var createdUser = await _userRepository.AddAsync(user);
                return Result<User>.Success(createdUser);
            }

            public async Task<Result<User>> LoginAsync(LoginCommand command)
            {
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Email == command.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
                {
                    return Result<User>.Failure("Invalid email or password");
                }

                return Result<User>.Success(user);
            }

        }
    }
}