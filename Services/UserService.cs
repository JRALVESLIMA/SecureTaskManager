using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecureTaskManager.API.Data;
using SecureTaskManager.API.DTOs;
using SecureTaskManager.API.Models;

namespace SecureTaskManager.API.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, TokenService tokenService, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.UserName))
            {
                throw new Exception("Dados obrigatórios ausentes.");
            }

            
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("E-mail já está em uso.");
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Erro ao criar usuário: {errors}");
            }

            
            await _userManager.AddToRoleAsync(user, "User");

            var token = await _tokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                UserName = user.UserName,
                Token = token,
                Role = roles.FirstOrDefault() ?? "User"
            };
        }



        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return null;

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            var passwordValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, request.Password);
            if (passwordValid == PasswordVerificationResult.Failed)
                return null;

            var token = await _tokenService.GenerateTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return new AuthResponse
            {
                UserName = user.UserName ?? string.Empty,
                Token = token,
                Role = role
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()!
                });
            }

            return userDtos;
        }



        public async Task<bool> UpdateUserRoleAsync(string userName, string role)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return false;

            // Validação simples dos roles aceitos
            var validRoles = new List<string> { "Admin", "User", "Master" };
            if (!validRoles.Contains(role))
                return false;

            // Remove todos os roles atuais
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return false;

            // Adiciona o novo role
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
                return false;

            return true;
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

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            if (user == null)
                return false;

            _context.Users.Update(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            if (user == null || user.PasswordHash == null)
                return false;

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserByInstanceAsync(ApplicationUser user)
        {
            if (user == null)
                return false;

            _context.Users.Remove(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Método para criar usuário master a partir das configurações no appsettings.json
        public async Task SeedMasterUserAsync(UserManager<ApplicationUser> userManager)
        {
            var masterUserName = _configuration["MasterUser:UserName"];
            var masterEmail = _configuration["MasterUser:Email"];
            var masterPassword = _configuration["MasterUser:Password"];

            if (string.IsNullOrWhiteSpace(masterUserName))
                throw new InvalidOperationException("Configuração 'MasterUser:UserName' não pode ser nula ou vazia.");

            if (string.IsNullOrWhiteSpace(masterEmail))
                throw new InvalidOperationException("Configuração 'MasterUser:Email' não pode ser nula ou vazia.");

            if (string.IsNullOrWhiteSpace(masterPassword))
                throw new InvalidOperationException("Configuração 'MasterUser:Password' não pode ser nula ou vazia.");

            var masterUser = await userManager.FindByNameAsync(masterUserName);
            if (masterUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = masterUserName,
                    Email = masterEmail,
                };

                var result = await userManager.CreateAsync(user, masterPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Master");
                    Console.WriteLine("Usuário master criado com sucesso com role Master!!");
                }
                else
                {
                    foreach (var error in result.Errors)
                        Console.WriteLine($" - {error.Description}");
                }
            }
            else
            {
                Console.WriteLine("Usuário master já existe.");
            }
        }


        public async Task<int> CountUsersInRoleAsync(string role)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            return usersInRole.Count;
        }


    }
}
