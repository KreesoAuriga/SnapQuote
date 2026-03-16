using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;

namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll(User currentUser);
        User GetById(int id);
        void Register(RegisterRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
        void ChangePassword(int id, ChangePasswordRequest model);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            if (user == null || HashPassword(model.Password) != user.PasswordHash)
                throw new AppException("Username or password is incorrect");

            var response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            return response;
        }

        public IEnumerable<User> GetAll(User currentUser)
        {
            if (currentUser == null)
                return _context.Users;

            if (currentUser.Role == "SuperAdmin")
                return _context.Users;

            if (currentUser.Role == "CompanyAdmin" || currentUser.Role == "User")
                return _context.Users.Where(u => u.CompanyId == currentUser.CompanyId);

            return _context.Users;
        }

        // public IEnumerable<User> GetAll()
        // {
        //     return _context.Users;
        // }

        public User GetById(int id)
        {
            return getUser(id);
        }

        public void Register(RegisterRequest model)
        {
            if (_context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            var user = _mapper.Map<User>(model);
            user.Role = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;
            user.CompanyId = model.CompanyId;

            user.PasswordHash = HashPassword(model.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(int id, UpdateRequest model)
        {
            var user = getUser(id);

            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = HashPassword(model.Password);

            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = getUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public void ChangePassword(int id, ChangePasswordRequest model)
        {
            var user = getUser(id);

            if (HashPassword(model.OldPassword) != user.PasswordHash)
                throw new AppException("Current password is incorrect");

            user.PasswordHash = HashPassword(model.NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // private string GenerateToken(User user)
        // {
        //     var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dev-secret-key"));
        //     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //     var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: creds);
        //     return new JwtSecurityTokenHandler().WriteToken(token);
        // }

        // helper methods

        private static string HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private User getUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }

        private bool ValidatePasswordComplexity(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6) return false;
            return password.Any(char.IsDigit) && password.Any(char.IsLetter);
        }

        private string SanitizeUsername(string username)
        {
            return username?.Trim().ToLowerInvariant();
        }
    }
}