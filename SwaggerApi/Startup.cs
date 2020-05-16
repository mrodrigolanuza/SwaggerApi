using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace SwaggerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            
            //Configurar el servicio Swagger            
            services.AddSwaggerGen(config => {
                //Versionado de la API
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WeatherAPI",
                    Version = "v1"
                });
                config.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "WeatherAPI",
                    Version = "v2" 
                });

                //En caso de conflictos con métodos duplicados, utilizar el primero
                config.ResolveConflictingActions(e => e.First());

                //Los métodos de los endpoints de la API, incluyen comentarios utilizando una serie de etiquetas.
                //La documentación se puede indicar a Visual Studio que los genere al hacer build en un XML. Para ello marcamos el check el propiedades del proyecto e indicadmos la ruta de salida.
                //A Swagger, le indicamos que encontrará este fichero xml de comentario en la ruta siguiente. Se indica de manera genérica mediante reflection y appcontext.
                //Al ser una ruta relativa, cuando se sube a Azure funciona correctamente.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            //Incluimos el middleware en el pipeline de la request de Swagger, y de SwaggerUI, que nos permitirá genera una página interface 
            //descriptiva de los métodos, y con los que podremos probarlos.
            //IMPORTANTE: Como siempre es necesario intercalar el middleware en orden ocrrecto dentro del pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(config => {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherAPI v1");  //Endpoints con los versionados de la API.
                config.SwaggerEndpoint("/swagger/v2/swagger.json", "WeatherAPI v2");
                config.RoutePrefix = string.Empty;
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
