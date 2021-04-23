using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhonebookAPI_dotnet.Data;
using PhonebookAPI_dotnet.Domain;

namespace PhonebookAPI_dotnet.Services
{
    public class PhonebookEntryService: IPhonebookEntryService
    {
        private readonly DataContext _dataContext;

        public PhonebookEntryService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<PhonebookEntry>> GetPhonebookEntriesAsync()
        {
            return await _dataContext.PhonebookEntries.ToListAsync();
        }

        public async Task<PhonebookEntry> GetPhonebookEntryByIdAsync(int id)
        {
            return await _dataContext.PhonebookEntries.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PhonebookEntry> GetPhonebookEntryByPhoneNumberAsync(string phoneNumber)
        {
            return await _dataContext.PhonebookEntries.SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<PhonebookEntry> GetPhonebookEntryByUserId(string userId)
        {
            return await _dataContext.PhonebookEntries.SingleOrDefaultAsync(x => x.UserId == userId);
        }


        public async Task<bool> CreatePhonebookEntryAsync(PhonebookEntry phonebookEntry)
        {
            await _dataContext.PhonebookEntries.AddAsync(phonebookEntry);
            var created = await _dataContext.SaveChangesAsync();

            return created > 0;
        }

        public async Task<bool> UpdatePhoneBookEntryAsync(PhonebookEntry phonebookEntryToUpdate)
        {
            _dataContext.PhonebookEntries.Update(phonebookEntryToUpdate);
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeletePhoneBookEntryAsync(int id)
        {
            var phonebookEntry = await GetPhonebookEntryByIdAsync(id);
            if (phonebookEntry == null)
                return false;

            _dataContext.PhonebookEntries.Remove(phonebookEntry);
            var deleted = await _dataContext.SaveChangesAsync();

            return deleted > 0;
        }

        public async Task<bool> UserOwnsPhoneBookEntry(int id, string userId)
        {
            var phonebookEntry = await _dataContext.PhonebookEntries.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);

            if (phonebookEntry == null)
            {
                return false;
            }

            if (phonebookEntry.UserId != userId)
            {
                return false;
            }

            return true;
        }
    }
}