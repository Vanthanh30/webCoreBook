using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/role")]
    public class RoleApiController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleApiController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetRoles()
        {
            var adminRoleIds = await _roleService.GetAdminRoleIdsAsync();
            var result = new List<object>();

            foreach (var id in adminRoleIds)
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role != null)
                {
                    result.Add(new
                    {
                        id = role.Id,
                        name = role.Name
                    });
                }
            }

            return Ok(result);
        }
        [HttpGet("allRole")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();

            var result = new List<object>();

            foreach (var r in roles)
            {
                result.Add(new
                {
                    id = r.Id,
                    name = r.Name,
                    description = r.Description
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound();

            return Ok(new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description
            });
        }
    }
}
