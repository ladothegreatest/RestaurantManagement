using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Dtos.User;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<User> _userRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IMapper mapper,
            IRepository<User> userRepository,
            JwtService jwtService,
            ILogger<UserService> logger)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Attempting to register user with email {Email}", dto.Email);

            var emailExists = await _userRepository.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed - email {Email} already in use", dto.Email);
                throw new InvalidOperationException("Email already in use");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with ID {UserId}", user.Id);

            var token = _jwtService.GenerateToken(user);
            var userDto = _mapper.Map<UserResponseDto>(user);

            return new LoginResponseDto
            {
                Token = token,
                User = userDto
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - email {Email} not found", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                _logger.LogWarning("Login failed - invalid password for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            var token = _jwtService.GenerateToken(user);
            var userDto = _mapper.Map<UserResponseDto>(user);

            return new LoginResponseDto
            {
                Token = token,
                User = userDto
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete user with ID {UserId}", id);

            if (!await _userRepository.ExistsAsync(id))
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                return false;
            }

            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully", id);

            return true;
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all users");

            var users = await _userRepository.GetAllAsync();

            return _mapper.Map<List<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching user with ID {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return null;
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching user by email {Email}", email);

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", email);
                return null;
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto)
        {
            _logger.LogInformation("Updating user with ID {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update", id);
                return null;
            }

            var emailExists = await _userRepository.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailExists)
            {
                _logger.LogWarning("Update failed - email {Email} already in use by another user", dto.Email);
                throw new InvalidOperationException("Email already in use");
            }

            _mapper.Map(dto, user);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} updated successfully", id);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
        {
            _logger.LogInformation("Attempting password change for user with ID {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for password change", id);
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                _logger.LogWarning("Password change failed - incorrect current password for user {UserId}", id);
                throw new InvalidOperationException("Current password is incorrect");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user {UserId}", id);

            return true;
        }
    }
}