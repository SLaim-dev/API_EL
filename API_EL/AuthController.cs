using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;

    public AuthController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new ArgumentNullException(nameof(configuration), "Строка подключения не найдена!");
    }

    // ✅ Регистрация нового пользователя
    [HttpPost("register")]
    public IActionResult Register([FromBody] UserDto user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("❌ Заполните все поля.");
        }

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        // Проверяем, есть ли уже такой пользователь
        string checkUserQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";
        using var checkCmd = new MySqlCommand(checkUserQuery, connection);
        checkCmd.Parameters.AddWithValue("@username", user.Username);
        checkCmd.Parameters.AddWithValue("@email", user.Email);
        int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (userExists > 0)
        {
            return BadRequest("❌ Такой пользователь уже существует!");
        }

        // Хэшируем пароль
        string passwordHash = HashPassword(user.Password);

        // Добавляем пользователя в базу
        string insertQuery = "INSERT INTO users (username, email, password_hash) VALUES (@username, @email, @password)";
        using var insertCmd = new MySqlCommand(insertQuery, connection);
        insertCmd.Parameters.AddWithValue("@username", user.Username);
        insertCmd.Parameters.AddWithValue("@email", user.Email);
        insertCmd.Parameters.AddWithValue("@password", passwordHash);
        insertCmd.ExecuteNonQuery();

        return Ok("✅ Пользователь успешно зарегистрирован!");
    }

    // ✅ Авторизация пользователя
    [HttpPost("login")]
    public IActionResult Login([FromBody] UserDto user)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        // Ищем пользователя в базе
        string query = "SELECT password_hash FROM users WHERE username = @username";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@username", user.Username);

        object result = cmd.ExecuteScalar();
        if (result == null)
        {
            return Unauthorized("❌ Неверный логин или пароль.");
        }

        string storedHash = result?.ToString() ?? string.Empty;
        if (!VerifyPassword(user.Password, storedHash))
        {
            return Unauthorized("❌ Неверный логин или пароль.");
        }

        return Ok("✅ Вход выполнен!");
    }

    // ✅ Функция хеширования пароля
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    // ✅ Функция проверки пароля
    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        string inputHash = HashPassword(inputPassword);
        return inputHash == storedHash;
    }
}

// ✅ DTO для пользователя
public class UserDto
{
    public required string Username { get; set; }
    public string? Email { get; set; } // Email не обязателен для входа в аккаунт
    public required string Password { get; set; }
}

