using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using PhonebookAPI_dotnet.Domain;
using PhonebookAPI_dotnet.Extensions;
using PhonebookAPI_dotnet.Requests;
using PhonebookAPI_dotnet.Services;

namespace PhonebookAPI_dotnet.Controllers
{
    [Authorize(AuthenticationSchemes =  JwtBearerDefaults.AuthenticationScheme)]
    public class PhonebookEntriesController : Controller
    {
        private readonly IPhonebookEntryService _phonebookEntryService;

        public PhonebookEntriesController(IPhonebookEntryService phonebookEntryService)
        {
            _phonebookEntryService = phonebookEntryService;
        }

        [HttpGet(ApiRoutes.PhonebookEntries.Index)]
        public async Task<IActionResult> Index()
        {
            return Ok(await _phonebookEntryService.GetPhonebookEntriesAsync());
        }
        
        [HttpGet(ApiRoutes.PhonebookEntries.Get)]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            var userOwnsPhoneBookEntry =
                await _phonebookEntryService.UserOwnsPhoneBookEntry(id, HttpContext.GetUserId());

            if (!userOwnsPhoneBookEntry)
            {
                return NotFound();
            }
            
            var phonebookEntry = await _phonebookEntryService.GetPhonebookEntryByIdAsync(id);

            if (phonebookEntry == null)
            {
                return NotFound();
            }
            
            return Ok(phonebookEntry);
        }

        [HttpPost(ApiRoutes.PhonebookEntries.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePhonebookEntryRequest createPhonebookEntryRequest)
        {

            var userId = HttpContext.GetUserId();
            var exisitingPhonebookEntry =
                await _phonebookEntryService.GetPhonebookEntryByUserId(userId);

            if (exisitingPhonebookEntry != null)
            {
                return BadRequest(new
                {
                    error = "User already has a personal phonebook entry"
                });
            }
            
            var phonebookEntry = new PhonebookEntry
            {
                FirstName = createPhonebookEntryRequest.FirstName,
                LastName = createPhonebookEntryRequest.LastName,
                PhoneNumber = createPhonebookEntryRequest.PhoneNumber,
                UserId = userId
            };

            await _phonebookEntryService.CreatePhonebookEntryAsync(phonebookEntry);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUrl = baseUrl + "/" +
                              ApiRoutes.PhonebookEntries.Get.Replace("{id}", phonebookEntry.Id.ToString());

            
            return Created(locationUrl, phonebookEntry);
        }
        
        [HttpPut(ApiRoutes.PhonebookEntries.Update)]
        public async Task<IActionResult> Update([FromRoute]int id,[FromBody] UpdatePhonebookEntryRequest updatePhonebookEntryRequest)
        {
            var userOwnsPhoneBookEntry =
                await _phonebookEntryService.UserOwnsPhoneBookEntry(id, HttpContext.GetUserId());

            if (!userOwnsPhoneBookEntry)
            {
                return NotFound();
            }

            var phonebookEntry = await _phonebookEntryService.GetPhonebookEntryByIdAsync(id);
            var existingEntry =
                await _phonebookEntryService.GetPhonebookEntryByPhoneNumberAsync(
                    updatePhonebookEntryRequest.PhoneNumber);
            
            if (existingEntry != null)
            {
                return BadRequest(new
                {
                    error = "Phone number already in use"
                });
            }

            phonebookEntry.FirstName = updatePhonebookEntryRequest.FirstName;
            phonebookEntry.LastName = updatePhonebookEntryRequest.LastName;
            phonebookEntry.PhoneNumber = updatePhonebookEntryRequest.PhoneNumber;

            try
            {
                var updated = await _phonebookEntryService.UpdatePhoneBookEntryAsync(phonebookEntry);

                if (updated)
                    return Ok(phonebookEntry);

                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }

        [HttpDelete(ApiRoutes.PhonebookEntries.Delete)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var userOwnsPhoneBookEntry =
                await _phonebookEntryService.UserOwnsPhoneBookEntry(id, HttpContext.GetUserId());

            if (!userOwnsPhoneBookEntry)
            {
                return NotFound();
            }
            
            var deleted = await _phonebookEntryService.DeletePhoneBookEntryAsync(id);

            if (deleted)
                return NoContent();

            return NotFound();
        }
    }
}