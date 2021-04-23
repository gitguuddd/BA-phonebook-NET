using System.Collections.Generic;
using System.Threading.Tasks;
using PhonebookAPI_dotnet.Domain;

namespace PhonebookAPI_dotnet.Services
{
    public interface IPhonebookEntryService
    {
        Task<List<PhonebookEntry>> GetPhonebookEntriesAsync();

        Task<PhonebookEntry> GetPhonebookEntryByIdAsync(int id);
        
        Task<PhonebookEntry> GetPhonebookEntryByPhoneNumberAsync(string phoneNumber);

        Task<PhonebookEntry> GetPhonebookEntryByUserId(string userId);
        Task<bool> CreatePhonebookEntryAsync(PhonebookEntry phonebookEntryToCreate);

        Task<bool> UpdatePhoneBookEntryAsync(PhonebookEntry phonebookEntryToUpdate);
        
        Task<bool> DeletePhoneBookEntryAsync(int id);
        Task<bool> UserOwnsPhoneBookEntry(int id, string userId);
    }
}