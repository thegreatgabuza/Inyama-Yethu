# Database Migration Instructions for Inyama Yethu

## Add FinancialTransactions Table

Follow these steps to add the new FinancialTransaction table to your database:

### 1. Open a Command Prompt or Terminal

Open a command prompt or terminal in your project directory (where the `.csproj` file is located).

### 2. Create the Migration

Run the following command to create a new migration:

```
dotnet ef migrations add AddFinancialTransactions
```

This will create a new migration file in your `Migrations` folder.

### 3. Apply the Migration to the Database

Run the following command to apply the migration to your database:

```
dotnet ef database update
```

### 4. Verify the Migration

You can verify that the migration was successful by:

1. Checking your database to see if the `FinancialTransactions` table was created
2. Opening SQL Server Management Studio (SSMS) or another database tool
3. Connecting to your database
4. Checking that the table exists with all the expected columns

## Troubleshooting

If you encounter any issues:

1. Make sure Entity Framework Core tools are installed:
   ```
   dotnet tool install --global dotnet-ef
   ```

2. If you get errors about missing dependencies, make sure you have the required packages:
   ```
   dotnet add package Microsoft.EntityFrameworkCore.Design
   ```

3. If you have pending migrations that are causing conflicts, you can force a new migration:
   ```
   dotnet ef migrations add AddFinancialTransactions --force
   ```

4. If you need to remove the last migration (if it hasn't been applied yet):
   ```
   dotnet ef migrations remove
   ```

## Testing the Migration

After applying the migration:

1. Run the application and navigate to the Financial Records > Transactions page
2. Try adding a new transaction
3. Verify that the transaction appears in the list
4. Check if editing and deleting work as expected

If everything works correctly, your database migration was successful! 