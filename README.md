# BookingDDD

This example summarizes the week's DDD topics without Active Record:

- rich domain model
- `Resource` as aggregate root
- repository per aggregate root
- Dapper mapping
- Unit of Work and SQL transaction
- domain events published after commit
- multiple event handlers
- a small HTTP API and Axios example

## Run

1. Run `database/create-database.sql` against a local SQL Server.
2. Adjust `BookingDDD.Api/appsettings.json` if needed.
3. Start the API:

   ```powershell
   dotnet run --project BookingDDD.Api
   ```

4. Open `http://localhost:5080`, then use the examples in the browser
   console.

The SQL script seeds resource
`00000000-0000-0000-0000-000000000000`, open from 08:00 to 16:00.

Audit and calendar handlers write to SQL after the aggregate transaction
has committed. Notification handlers write simulated confirmations to the
API console.
