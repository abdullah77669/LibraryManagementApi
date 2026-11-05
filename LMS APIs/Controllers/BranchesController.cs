using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/branches")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly IBranch _branchService;

        public BranchesController(IBranch branchService)
        {
            _branchService = branchService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Branch>>> GetBranches()
        {
            return Ok(await _branchService.GetBranchesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetBranchById(int id)
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null) return NotFound();
            return branch;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<Branch>> PostBranch(Branch branch)
        {
            var result = await _branchService.CreateBranchAsync(branch);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return CreatedAtAction(nameof(GetBranchById), new { id = branch.BranchID }, branch);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdateBranch(int id, Branch branch)
        {
            var result = await _branchService.UpdateBranchAsync(id, branch);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var result = await _branchService.DeleteBranchAsync(id);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }
    }
}