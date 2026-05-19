using MySqlConnector;

var cs = "Server=127.0.0.1;Port=3306;Database=PharmaDeskDB;User=root;Password=1414;TreatTinyAsBoolean=true;AllowPublicKeyRetrieval=True;SslMode=None;";
await using var conn = new MySqlConnection(cs);
try
{
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT DATABASE(), VERSION()";
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"CONNECTED Database={reader.GetValue(0)} Version={reader.GetValue(1)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Environment.ExitCode = 1;
}
