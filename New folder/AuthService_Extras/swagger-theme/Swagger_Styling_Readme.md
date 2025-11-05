# Swagger UI Styling – How to Apply

## 1) Add static files
Create the folder: `src/AuthService.Api/wwwroot/swagger-ui/` and copy **swagger-ui-custom.css** into it.

```
src/AuthService.Api/
  wwwroot/
    swagger-ui/
      swagger-ui-custom.css
```

## 2) Enable Static Files and inject stylesheet
In `Program.cs` of **AuthService.Api**:

```csharp
app.UseStaticFiles(); // add this if not present
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // This makes the CSS available as /swagger-ui/swagger-ui-custom.css
    c.InjectStylesheet("/swagger-ui/swagger-ui-custom.css");
});
```

> If you have environment-based Swagger, place the snippet within that block.

## 3) Run
- `dotnet run --project src/AuthService.Api`
- Open http://localhost:8080/swagger
- You should see the new theme.
