using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cleanApi.Modules.Auth
{
    public class AuthEntity:BaseEntity
    {
       public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public bool IsEmailVerified { get; private set; }

        public static User Create(string firstName, string lastName, string email, string passwordHash, string phoneNumber, string address)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHash,
                PhoneNumber = phoneNumber,
                Address = address,
                IsEmailVerified = false
            };
        }

        public void UpdateProfile(string firstName, string lastName, string phoneNumber, string address)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Address = address;
            SetUpdatedAt();
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            SetUpdatedAt();
        } 
    }
}