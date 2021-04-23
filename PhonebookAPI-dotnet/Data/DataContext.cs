using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhonebookAPI_dotnet.Domain;

namespace PhonebookAPI_dotnet.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
        
        public DbSet<PhonebookEntry> PhonebookEntries { get; set; }
        
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}