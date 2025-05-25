var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure HttpClient for API calls
builder.Services.AddHttpClient();

// Configure CORS if needed
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseCors("AllowAll");

// Configure API URL rewriting
app.Use(async (context, next) => {
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var apiBaseUrl = app.Configuration["ApiSettings:ApiBaseUrl"] ?? "https://localhost:7064";
        var targetPath = context.Request.Path.Value.Replace("/api", "/api");
        var targetUrl = apiBaseUrl + targetPath + context.Request.QueryString;

        var httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Copy request headers if needed
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", context.Request.Headers["Authorization"].ToString());
        }

        HttpResponseMessage response;

        if (context.Request.Method == "GET")
        {
            response = await httpClient.GetAsync(targetUrl);
        }
        else if (context.Request.Method == "POST")
        {
            var requestContent = new StreamContent(context.Request.Body);
            if (context.Request.Headers.ContainsKey("Content-Type"))
            {
                requestContent.Headers.Add("Content-Type", context.Request.Headers["Content-Type"].ToString());
            }
            response = await httpClient.PostAsync(targetUrl, requestContent);
        }
        else if (context.Request.Method == "PUT")
        {
            var requestContent = new StreamContent(context.Request.Body);
            if (context.Request.Headers.ContainsKey("Content-Type"))
            {
                requestContent.Headers.Add("Content-Type", context.Request.Headers["Content-Type"].ToString());
            }
            response = await httpClient.PutAsync(targetUrl, requestContent);
        }
        else if (context.Request.Method == "DELETE")
        {
            response = await httpClient.DeleteAsync(targetUrl);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Unsupported HTTP method");
            return;
        }

        // Copy response headers
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // Copy content headers
        foreach (var header in response.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // Set response status code
        context.Response.StatusCode = (int)response.StatusCode;

        // Copy response body
        await response.Content.CopyToAsync(context.Response.Body);
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();





//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();