using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SecureTaskManager.API.DTOs;
using SecureTaskManager.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SecureTaskManager.API.Models;

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
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Nome de usu�rio, email e senha s�o obrigat�rios.");
            }

            var result = await _userService.RegisterAsync(request);

            if (result == null)
                return BadRequest("E-mail j� est� em uso.");

            return Ok(result);
        }


        // Endpoint de login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);

            if (result == null)
                return Unauthorized("Credenciais inv�lidas.");

            return Ok(result);
        }

        // Endpoint protegido: Perfil do Usu�rio
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usu�rio n�o autenticado.");

            var user = await _userService.GetUserByUserNameAsync(userName);

            if (user == null)
                return NotFound("Usu�rio n�o encontrado.");

            // Usando o UserManager para obter as roles do usu�rio
            var userRoles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user))?.FirstOrDefault() ?? "Sem fun��o atribu�da"
            };

            return Ok(userDto);
        }



        // Endpoint protegido: listar todos os usu�rios (apenas Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        

        // Endpoint para deletar um usu�rio
        [Authorize(Roles = "Admin")]
        [HttpPost("updateRole")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            // Verifica se os par�metros s�o nulos
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Role))
            {
                return BadRequest("Nome de usu�rio ou papel n�o podem ser nulos ou vazios.");
            }

            // Verifica se o usu�rio foi alterado com sucesso
            var result = await _userService.UpdateUserRoleAsync(request.UserName, request.Role);

            if (!result)
                return BadRequest("Falha ao atualizar o papel do usu�rio ou usu�rio n�o encontrado.");

            return Ok(new { Message = "Papel do usu�rio atualizado com sucesso." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("Usu�rio n�o encontrado.");

            user.UserName = request.UserName;
            user.FullName = request.FullName;
            user.Email = request.Email;

            var result = await _userService.UpdateUserAsync(user);

            if (!result)
                return BadRequest("Falha ao atualizar os dados do usu�rio.");

            return Ok(new { Message = "Dados do usu�rio atualizados com sucesso." });
        }

        [Authorize]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserRequest request)
        {
            if (User?.Identity?.Name is null)
                return Unauthorized("N�o foi poss�vel identificar o usu�rio logado.");

            // Obt�m o usu�rio atual pelo UserName (do token JWT)
            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            if (user == null)
                return NotFound("Usu�rio n�o encontrado.");

            // Permite que o usu�rio altere seus pr�prios dados, exceto o ID
            user.UserName = request.UserName;
            user.FullName = request.FullName;
            user.Email = request.Email;

            var result = await _userService.UpdateUserAsync(user);

            if (!result)
                return BadRequest("Falha ao atualizar os dados do usu�rio.");

            return Ok(new { Message = "Dados do perfil atualizados com sucesso." });
        }


        [Authorize]
        [HttpPatch("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (User?.Identity?.Name is null)
                return Unauthorized("N�o foi poss�vel identificar o usu�rio logado.");

            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            if (user == null)
                return NotFound("Usu�rio n�o encontrado.");

            var result = await _userService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result)
                return BadRequest("Falha ao alterar a senha.");

            return Ok(new { Message = "Senha alterada com sucesso." });
        }


        [Authorize]
        [HttpDelete("deleteAccount")]
        public async Task<IActionResult> DeleteOwnAccount()
        {
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Usu�rio n�o autenticado.");

            var user = await _userService.GetUserByUserNameAsync(userName);

            if (user == null)
                return NotFound("Usu�rio n�o encontrado.");

            var result = await _userService.DeleteUserAsync(user);

            if (!result)
                return BadRequest("Erro ao deletar a conta.");

            return Ok(new { Message = "Conta deletada com sucesso." });
        }



    }
}
