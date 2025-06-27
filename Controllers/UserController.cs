using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SecureTaskManager.API.DTOs;
using SecureTaskManager.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SecureTaskManager.API.Models;
using System.Security.Claims;


namespace SecureTaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        // Endpoint de registro
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Nome de usuário, email e senha são obrigatórios.");
            }

            try
            {
                var result = await _userService.RegisterAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Endpoint de login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);

            if (result == null)
                return Unauthorized("Credenciais inválidas.");

            return Ok(result);
        }

        // Endpoint exibição do Perfil do Usuário
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usuário não autenticado.");

            var user = await _userService.GetUserByUserNameAsync(userName);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            // Usando o UserManager para obter as roles do usuário
            var userRoles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = userRoles?.FirstOrDefault() ?? "Sem função atribuída"
            };

            return Ok(userDto);
        }

        // Endpoint de listar todos os usuários
        [Authorize(Roles = "Master,Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Erro ao buscar usuários: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        //Endpoint para atualização de dados pelo Administrador.
        [Authorize(Roles = "Master")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            user.UserName = request.UserName;
            user.FullName = request.FullName;
            user.Email = request.Email;

            var result = await _userService.UpdateUserAsync(user);

            if (!result)
                return BadRequest("Falha ao atualizar os dados do usuário.");

            return Ok(new { Message = "Dados do usuário atualizados com sucesso." });
        }

        // Endpoint Atualização de Perfil do usuario
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserRequest request)
        {
            if (User?.Identity?.Name is null)
                return Unauthorized("Não foi possível identificar o usuário logado.");

            // Obtém o usuário atual pelo UserName (do token JWT)
            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            // Permite que o usuário altere seus próprios dados, exceto o ID e Role
            user.UserName = request.UserName;
            user.FullName = request.FullName;
            user.Email = request.Email;

            var result = await _userService.UpdateUserAsync(user);

            if (!result)
                return BadRequest("Falha ao atualizar os dados do usuário.");

            return Ok(new { Message = "Dados do perfil atualizados com sucesso." });
        }

        // Endpoint para atualização de senha do usuario
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (User?.Identity?.Name is null)
                return Unauthorized("Não foi possível identificar o usuário logado.");

            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            var result = await _userService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result)
                return BadRequest("Falha ao alterar a senha.");

            return Ok(new { Message = "Senha alterada com sucesso." });
        }

        // Endponit para deletar a propria conta.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("deleteAccount")]
        public async Task<IActionResult> DeleteOwnAccount()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usuário não autenticado.");

            var user = await _userService.GetUserByUserNameAsync(userName);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            var result = await _userService.DeleteUserAsync(user.Id);

            if (!result)
                return BadRequest("Erro ao deletar a conta.");

            return Ok(new { Message = "Conta deletada com sucesso." });
        }

        //Endpoint de deleção de usuários pelos Administradores
        [Authorize(Roles = "Master,Admin")]
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
                return Unauthorized("Usuário atual não autenticado.");

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Unauthorized("Usuário atual não encontrado.");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var targetUserRoles = await _userManager.GetRolesAsync(user);

            // Impede excluir Master
            if (targetUserRoles.Contains("Master"))
                return Forbid("Não é permitido excluir o usuário Master.");

            // Admin só pode excluir usuários comuns (com role 'User')
            if (currentUserRoles.Contains("Admin") && !targetUserRoles.Contains("User"))
                return Forbid("Admin só pode excluir usuários comuns.");

            // Master pode excluir qualquer um (exceto ele mesmo se quiser limitar)
            await _userService.DeleteUserAsync(userId);

            return Ok("Usuário excluído com sucesso.");
        }


        // Endponit de promoção de Role.
        [Authorize(Roles = "Master,Admin")]
        [HttpPut("promote/{userId}")]
        public async Task<IActionResult> PromoteUser([FromBody] PromoteUserRequest request)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Usuário atual não autenticado.");

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Unauthorized("Usuário atual não encontrado.");

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            var allowedRoles = new[] { "Master", "Admin", "User" };
            if (!allowedRoles.Contains(request.NewRole))
                return BadRequest("Role inválido. Use Master, Admin ou User.");

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var isUserMaster = userRoles.Contains("Master");
            var isUserAdmin = userRoles.Contains("Admin");
            var isUserComum = userRoles.Contains("User");

            // Protege o último Master de ser rebaixado
            if (isUserMaster && request.NewRole != "Master")
            {
                var mastersCount = await _userService.CountUsersInRoleAsync("Master");
                if (mastersCount <= 1)
                    return Forbid("Não é permitido rebaixar o último usuário com role Master.");
            }

            // Regras específicas se quem está promovendo for Admin
            if (currentUserRoles.Contains("Admin"))
            {
                if (request.NewRole == "Master")
                    return Forbid("Admin não pode promover para Master.");

                if (!isUserComum)
                    return Forbid("Admin só pode promover usuários do tipo User.");

                if (request.NewRole != "Admin")
                    return Forbid("Admin só pode promover usuários para Admin.");
            }

            // Somente Master pode promover para Master
            if (request.NewRole == "Master" && !currentUserRoles.Contains("Master"))
            {
                return Forbid("Somente usuários Master podem promover outros para Master.");
            }

            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRoleAsync(user, request.NewRole);

            return Ok("Usuário promovido/rebaixado com sucesso.");
        }

    }
}
