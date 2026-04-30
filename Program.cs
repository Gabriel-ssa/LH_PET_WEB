using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using LH_PET_WEB.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do padrão MVC
builder.Services.AddControllersWithViews();

// 1. INJEÇÃO DE DEPENDÊNCIA
builder.Services.AddScoped<IEmailService, EmailService>();

// 2. CONFIGURAÇÃO DO BANCO DE DADOS
var connectionString = builder.Configuration.GetConnectionString("VetPlusConnection");

if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("SUA_SENHA_AQUI"))
{
    throw new InvalidOperationException("A ConnectionString não foi encontrada ou ainda está com a senha de exemplo.");
}

var versaoServidor = new MySqlServerVersion(new Version(8, 0, 0));

builder.Services.AddDbContext<ContextoBanco>(options =>
    options.UseMySql(connectionString, versaoServidor));

// 3. CONFIGURAÇÃO DE SEGURANÇA (JWT e Cookies)
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Contains("PRODUCAO"))
{
    throw new InvalidOperationException("A Chave Secreta do JWT não está configurada corretamente.");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Autenticacao/Login";
    options.AccessDeniedPath = "/Autenticacao/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// 4. PIPELINE HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ROTA PADRÃO
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacao}/{action=Login}/{id?}");

// =======================================================
// 5. DATA SEEDING (Semeio de Dados Seguro)
// =======================================================
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ContextoBanco>();
    var configuracao = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    string? adminEmail = configuracao["AdminInicial:Email"];
    string? adminSenha = configuracao["AdminInicial:Senha"];
    
    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminSenha))
    {
        if (!contexto.Usuarios.Any(u => u.Perfil == "Admin"))
        {
            var adminInicial = new Usuario
            {
                Nome = "Administrador", // <--- NOME INSERIDO AQUI
                Email = adminEmail,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminSenha),
                Perfil = "Admin",
                Ativo = true,
                SenhaTemporaria = false
            };
            
            contexto.Usuarios.Add(adminInicial);
            contexto.SaveChanges();
            
            Console.WriteLine("========================================");
            Console.WriteLine("🚀 ADMIN INICIAL CRIADO COM SUCESSO DE FORMA SEGURA!");
            Console.WriteLine("========================================");
        }
    }
}

app.Run();