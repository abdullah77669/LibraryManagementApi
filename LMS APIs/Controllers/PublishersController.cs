using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/publishers")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisher _publisherService;

        public PublishersController(IPublisher publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Publishers>>> GetPublishers()
        {
            return Ok(await _publisherService.GetPublishersAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Publishers>> GetPublisherById(int id)
        {
            var publisher = await _publisherService.GetPublisherByIdAsync(id);
            if (publisher == null) return NotFound();
            return publisher;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<Publishers>> PostPublisher(Publishers publisher)
        {
            var result = await _publisherService.CreatePublisherAsync(publisher);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return CreatedAtAction(nameof(GetPublisherById), new { id = publisher.PublisherID }, publisher);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdatePublisher(int id, Publishers publisher)
        {
            var result = await _publisherService.UpdatePublisherAsync(id, publisher);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var result = await _publisherService.DeletePublisherAsync(id);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }
    }
}