# ParrotWingsAPI

PW API provided the next functionallity:
  - User registration
  - User identification
  - User authentication
  - User aurotization
  - Logging In
  - Logging Out
  - Getting the autorized user name & PW balance
  - Making payments
  - Providing recipients list based on user input
  - Viewing users transaction history sorted by the date
  
Advantages of the solution:
  + Security: user passwords are not explicitly stored in the database (extremely important for money transfer services)
  + Security potential: authentication is based on "Json Web Token" (JWT) which could provided more security potential - with using refresh tokens for example
  + Security: JWT has a limited lifetime which means that in case of compromise the JWT token will soon become invalid
  + Convinience: the solution is based on "in memory" database which allows anyone at any time to run the API locally without having connection to a remote server with DB
  + Convinience: the solution uses a "Swagger" wrapper (part of Entity Framework) which makes it easier to test the API
  + Practicality: despite the in memory database, all database queries will also work for SQL using (Entity Framework)
  + Practicality: the solution is developed based on the latest version of C#, Entity Framework and latest version ASP.NET Core framework
  
Disadvantages of the solution:
  - Using an "in memory" database means that each program restart will cause a data reset (you will need to register users again)
  - The project was developed by a programmer who had never worked with web development and ASP.NET Core framework (but it was very exciting for him and he really tried)
  
Notes:
 - According the Swagger you need to Authorize with JWT token (top right corner of the Swagger interface). Input format is: "bearer " + JWT
 - It will emulate client http requests which contain this exactly JWT token
 - The JWT token is sent by the server in response to the successful registration of the user
 - Without authorization using a JWT token, it is impossible to access endpoints with the [Authorize] directive

__________________________________________
Ask the author any time! You are welcome!
__________________________________________

//Installed Packages
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="7.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="7.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.0" />
    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="6.25.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Filters" Version="7.0.6" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="6.25.0" />
