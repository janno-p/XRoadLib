using Calculator;
using Calculator.Contract;
using Calculator.WebService;
using XRoadLib.Extensions.AspNetCore;

var builder = WebApplication.CreateBuilder();

builder.Services.AddXRoadLib();
builder.Services.AddSingleton<ICalculate, CalculateWebService>();
builder.Services.AddSingleton<ISumOfIntegers, SumOfIntegersWebService>();
builder.Services.AddSingleton<CalculatorServiceManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseRouting();

app.MapGet("/", httpContext => httpContext.ExecuteWsdlRequest<CalculatorServiceManager>());
app.MapPost("/", httpContext => httpContext.ExecuteWebServiceRequest<CalculatorServiceManager>());

app.Run();
