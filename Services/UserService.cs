using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureTaskManager.API.Data;
using SecureTaskManager.API.DTOs;
using SecureTaskManager.API.Models;

namespace SecureTaskManager.API.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _dbContext;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public UserService(ApplicationDbContext context, TokenService tokenService, ApplicationDbContext dbContext)
        {
            _context = context;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // Verifica se o e-mail já está cadastrado
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            // Gera o hash da senha
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                UserName = user.UserName,
                Token = token,
                Role = user.Role
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            var token = _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                UserName = user.UserName,
                Token = token,
                Role = user.Role
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    UserName = u.UserName,
                    Role = u.Role
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> UpdateUserRoleAsync(string userName, string role)
        {
            // Verifica se o usuário existe
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) return false;

            // Verifica se o papel fornecido é válido
            if (role != "Admin" && role != "User") return false;

            // Altera o papel do usuário
            user.Role = role;

            _context.Users.Update(user);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(int id)
        {
            // Busca o usuário pelo ID
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            _dbContext.Users.Update(user); // Atualiza o usuário no contexto
            var result = await _dbContext.SaveChangesAsync(); // Salva as alterações no banco

            return result > 0; // Retorna true se a atualização for bem-sucedida
        }

        public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
        {
            // Busca o usuário pelo UserName
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return user;
        }

        public async Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            // Verifica se a senha atual está correta
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                // Se a senha atual estiver incorreta, retorna false
                return false;
            }

            // Se a senha atual for válida, atualiza a senha
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

            // Salva as mudanças no banco de dados
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteUserAsync(ApplicationUser user)
        {
            _dbContext.Users.Remove(user);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

    }
}
