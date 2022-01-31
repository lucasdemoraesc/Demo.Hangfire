using System;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Hangfire
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddHangfire(configuration => configuration
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage());

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3, DelaysInSeconds = new int[] { 300 } });

            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IBackgroundJobClient backgroundJobs, IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            FireAndForgetJobExample();
            DelayedJobsExample();
            RecurringJobsExample();
            ContinuationJobExample();
            JobFailExample();
        }

        /// <summary>
        /// Jobs fire-and-forget são executados imediatamente e depois "esquecidos".
        /// Você ainda poderá voltar a executá-los manualmente sempre que quiser
        /// graças ao Dashboard e ao histórico do Hangfire.
        /// </summary>
        public void FireAndForgetJobExample()
        {
            BackgroundJob.Enqueue(() => Console.WriteLine("Exemplo de job Fire-and-forget!"));
        }

        /// <summary>
        /// Jobs delayed são executados em uma data futura pré-definida.
        /// </summary>
        public void DelayedJobsExample()
        {
            BackgroundJob.Schedule(
                () => Console.WriteLine("Exemplo de job Delayed executado 2 minutos após o início da aplicação"),
                TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Reurring jobs permitem que você agende a execução de jobs recorrentes utilizando uma notação Cron.
        /// </summary>
        public void RecurringJobsExample()
        {
            RecurringJob.AddOrUpdate(
                "Meu job recorrente",
                () => Console.WriteLine((new Random().Next(1, 200) % 2 == 0)
                    ? "Job recorrente gerou um número par"
                    : "Job recorrente gerou um número ímpar"),
                Cron.Minutely,
                TimeZoneInfo.Local);
        }

        /// <summary>
        /// Esta abordagem permite que você defina para a execução de um job iniciar
        /// apenas após a conclusão de um job pai.
        /// </summary>
        public void ContinuationJobExample()
        {
            var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Job fire-and-forget pai!"));
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine($"Job fire-and-forget filho! (Continuação do job {jobId})"));
        }

        /// <summary>
        /// Quando um job falha o Hangfire irá adiciona-la a fila para executar novamente.
        /// Por padrão ele irá tentar reexecutar 10 vezes, você pode alterar este comportamento
        /// adicionando a seguinte configuração ao método ConfigureServices:
        /// <code>GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3, DelaysInSeconds = new int[] { 300 } });</code>
        /// </summary>
        public void JobFailExample()
        {
            BackgroundJob.Enqueue(() => FalharJob());
        }

        public void FalharJob()
        {
            throw new Exception("Deu ruim hein...");
        }
    }
}
