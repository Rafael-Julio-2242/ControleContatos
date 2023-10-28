using ControleContatos.Data;
using ControleContatos.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ControleContatos
{

    public class Program
    {

        static async Task Connections()
        {
            int port = 12345;

            TcpListener listener = new TcpListener(IPAddress.Any, port);

            listener.Start();
            Console.WriteLine("Aguardando Conexões na porta: " + port);

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                _ = HandleClientAsync(client);
            }
        }
        static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];

                int bytesRead;

                StringBuilder data = new StringBuilder();

                // Enquanto ainda houver dados para ler
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Armazena os dados em um objeto StringBuilder, que vai transformar
                    // Isto em uma string futuramente
                    data.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                }

                // Converter os dados recebidos de volta em uma string JSON
                string jsonData = data.ToString();
                Console.WriteLine("Recebido: " + jsonData);

            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync("Erro: " + e.Message);
            }
            finally { client.Close(); }
        }

        public static void Main(string[] args)
        {
            Connections();


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<BancoContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DataBase")));

            builder.Services.AddScoped<IContatoRepositorio, ContatoRepositorio>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }



    }
}