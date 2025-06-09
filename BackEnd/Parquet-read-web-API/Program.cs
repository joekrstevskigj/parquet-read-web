
namespace ParquetReadWeb_API
{
    using ParquetReadWeb_API.Data;
    using ParquetReadWeb_API.Interfaces.Data;
    using ParquetReadWeb_API.Interfaces.Repositories;
    using ParquetReadWeb_API.Interfaces.Services;
    using ParquetReadWeb_API.Repositories;
    using ParquetReadWeb_API.Services;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IParquetDataRepository, ParquetDataRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IDuckDbConnectionFactory, DuckDbConnectionFactory>();
            builder.Services.AddScoped<IFilterDefaultsService, FilterDefaultsService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
