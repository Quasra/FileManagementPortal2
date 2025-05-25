using FileManagementPortal1.DTOs.Account;
using FileManagementPortal1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FileManagementPortal1.Repositories;

namespace FileManagementPortal1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;


        public UserController(
     UserManager<AppUser> userManager,
     RoleManager<AppRole> roleManager,
     IMapper mapper,
     ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "Current user not found" });

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (id != userUpdateDto.Id)
                return BadRequest(new { message = "User IDs do not match" });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            // Update user properties
            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.Username;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to update user", errors = result.Errors });

            var updatedUserDto = _mapper.Map<UserDto>(user);
            return Ok(updatedUserDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            try
            {
                // 1. İlişkili dosyaları kontrol et ve işle
                var files = _context.Files.Where(f => f.UserId == id).ToList();
                if (files.Any())
                {
                    _context.Files.RemoveRange(files);
                    await _context.SaveChangesAsync();
                }

                // 2. Identity tarafından yönetilen ilişkileri manuel olarak kontrol et
                // (Entity Framework bunları normalde kendisi yönetir, ancak sorun yaşıyorsak
                // manuel kontrol edebiliriz)

                // Kullanıcı rollerini temizle
                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
                _context.UserRoles.RemoveRange(userRoles);

                // Kullanıcı taleplerini temizle
                var userClaims = await _context.UserClaims.Where(uc => uc.UserId == id).ToListAsync();
                _context.UserClaims.RemoveRange(userClaims);

                // Kullanıcı girişlerini temizle
                var userLogins = await _context.UserLogins.Where(ul => ul.UserId == id).ToListAsync();
                _context.UserLogins.RemoveRange(userLogins);

                // Kullanıcı tokenlarını temizle
                var userTokens = await _context.UserTokens.Where(ut => ut.UserId == id).ToListAsync();
                _context.UserTokens.RemoveRange(userTokens);

                await _context.SaveChangesAsync();

                // 3. Şimdi kullanıcıyı sil
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    // Hataları detaylı olarak logla
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { message = "Failed to delete user", errors });
                }

                return Ok(new { message = $"User {user.UserName} deleted successfully" });
            }
            catch (Exception ex)
            {
                // Hatanın detaylarını logla ve daha fazla bilgi ver
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the user",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            // Kullanıcının kendi rollerini görebilmesi için kontrol ekle
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Sadece admin kullanıcılar veya kullanıcının kendisi erişebilir
            if (id != currentUserId && !isAdmin)
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddToRole(string id, [FromBody] AddToRoleDto addToRoleDto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var roleExists = await _roleManager.RoleExistsAsync(addToRoleDto.RoleName);

            if (!roleExists)
                return BadRequest(new { message = $"Role {addToRoleDto.RoleName} does not exist" });

            var result = await _userManager.AddToRoleAsync(user, addToRoleDto.RoleName);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to add user to role", errors = result.Errors });

            return Ok(new { message = $"Rol Eklendi. {user.UserName} -->  {addToRoleDto.RoleName}" });
        }

        [HttpDelete("{id}/roles/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFromRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            var result = await _userManager.RemoveFromRoleAsync(user, role);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to remove user from role", errors = result.Errors });

            return Ok(new { message = $"Removed {user.UserName} from role {role}" });
        }
        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await _userManager.Users.CountAsync();
            return Ok(new { totalUsers = count });
        }


    }
}