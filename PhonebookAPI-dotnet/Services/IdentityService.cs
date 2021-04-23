using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhonebookAPI_dotnet.Data;
using PhonebookAPI_dotnet.Domain;
using PhonebookAPI_dotnet.Options;
using PhonebookAPI_dotnet.Requests;
using static System.Guid;
using static System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace PhonebookAPI_dotnet.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPhonebookEntryService _phonebookEntryService;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;

        public IdentityService(UserManager<IdentityUser> userManager, IPhonebookEntryService phonebookEntryService, JwtSettings jwtSettings, TokenValidationParameters tokenValidationParameters, DataContext dataContext)
        {
            _userManager = userManager;
            _phonebookEntryService = phonebookEntryService;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _dataContext = dataContext;
        }

        public async Task<AuthenticationResult> RegisterAsync(UserRegistrationRequest userRegistrationRequest)
        {
            var existingUser = await _userManager.FindByEmailAsync(userRegistrationRequest.Email);

            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"User with this email address already exists"},
                };
            }

            var existingPhonebookEntry =
                await _phonebookEntryService.GetPhonebookEntryByPhoneNumberAsync(userRegistrationRequest.PhoneNumber);

            if (existingPhonebookEntry != null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"Phone number is already in use"},
                };
            }
            
            var newUser = new IdentityUser
            {
                Email = userRegistrationRequest.Email,
                UserName = userRegistrationRequest.Email
            };

            var createdUser = await _userManager.CreateAsync(newUser, userRegistrationRequest.Password);

            if (!createdUser.Succeeded)
            {
                return new AuthenticationResult
                {
                    Errors = createdUser.Errors.Select(x => x.Description),
                };
            }

            var user = await _userManager.FindByEmailAsync(userRegistrationRequest.Email);
            
            var phonebookEntry = new PhonebookEntry
            {
                FirstName = userRegistrationRequest.FirstName,
                LastName = userRegistrationRequest.LastName,
                PhoneNumber = userRegistrationRequest.PhoneNumber,
                UserId = user.Id
            };

            await _phonebookEntryService.CreatePhonebookEntryAsync(phonebookEntry);

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }

        public async Task<AuthenticationResult> LoginAsync(UserLoginRequest userLoginRequest)
        {
            var user = await _userManager.FindByEmailAsync(userLoginRequest.Email);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"User does not exist"},
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, userLoginRequest.Password);

            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"Invalid Credentials"},
                };
            }

            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var validatedToken = GetPrincipalFromToken(refreshTokenRequest.Token);

            if (validatedToken == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"Invalid Token"}
                };
            }

            var expiryDateUnix =
                long.Parse(validatedToken.Claims.Single(x => x.Type == Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This token hasn't expired yet"}
                };
            }

            var jti = validatedToken.Claims.Single(x => x.Type == Jti).Value;

            var storedRefreshToken =
                await _dataContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshTokenRequest.RefreshToken);

            if (storedRefreshToken == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This refresh token does not exist"}
                };
            }
            
            if(DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This refresh token has expired"}
                };
            }

            if (storedRefreshToken.Invalidated)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This refresh token has been invalidated"}
                };
            }

            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This refresh token has been used"}
                };
            }
            
            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"This refresh token does not match this JWT"}
                };
            }

            storedRefreshToken.Used = true;
            _dataContext.RefreshTokens.Update(storedRefreshToken);
            await _dataContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
            return await GenerateAuthenticationResultForUserAsync(user);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validateToken);
                if (!IsJwtWithValidSecurityAlgorithm(validateToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(Sub, user.Email), 
                    new Claim(Jti, NewGuid().ToString()),
                    new Claim(Email,user.Email),
                    new Claim("id", user.Id)
                }),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _dataContext.RefreshTokens.AddAsync(refreshToken);
            await _dataContext.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };
        }
    }
    }