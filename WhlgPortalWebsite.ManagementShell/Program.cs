using System.Diagnostics.CodeAnalysis;
using HerPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace HerPortal.ManagementShell;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        var contextOptions = new DbContextOptionsBuilder<HerDbContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQLConnection") ??
                @"UserId=postgres;Password=postgres;Server=localhost;Port=5432;Database=herportaldev;Integrated Security=true;Include Error Detail=true;Pooling=true")
            .Options;

        using var context = new HerDbContext(contextOptions);
        var outputProvider = new OutputProvider();
        var dbOperation = new DatabaseOperation(context, outputProvider);
        var adminAction = new AdminAction(dbOperation);
        var commandHandler = new CommandHandler(adminAction, outputProvider);

        Subcommand command;
        var userEmailAddress = "";
        var codes = Array.Empty<string>();

        try
        {
            command = Enum.Parse<Subcommand>(args[0], true);
            if (new List<Subcommand>
                {
                    Subcommand.AddLas,
                    Subcommand.AddConsortia,
                    Subcommand.RemoveLas,
                    Subcommand.RemoveConsortia,
                    Subcommand.RemoveUser
                }.Contains(command))
            {
                userEmailAddress = args[1];
                codes = args.Skip(2).ToArray();
            }
        }
        catch (Exception)
        {
            var allSubcommands = string.Join(", ", Enum.GetValues<Subcommand>());
            outputProvider.Output(
                $"Please specify a valid subcommand - available options are: {allSubcommands}");
            return;
        }

        switch (command)
        {
            case Subcommand.RemoveUser:
                commandHandler.TryRemoveUser(commandHandler.GetUser(userEmailAddress));
                return;
            case Subcommand.AddLas:
                commandHandler.CreateOrUpdateUserWithLas(userEmailAddress, codes);
                return;
            case Subcommand.AddConsortia:
                commandHandler.CreateOrUpdateUserWithConsortia(userEmailAddress, codes);
                break;
            case Subcommand.RemoveLas:
                commandHandler.TryRemoveLas(commandHandler.GetUser(userEmailAddress), codes);
                return;
            case Subcommand.RemoveConsortia:
                commandHandler.TryRemoveConsortia(commandHandler.GetUser(userEmailAddress), codes);
                break;
            case Subcommand.FixAllUserOwnedConsortia:
                commandHandler.FixAllUserOwnedConsortia();
                break;
            case Subcommand.AddAllMissingAuthoritiesToDatabase:
                commandHandler.AddAllMissingAuthoritiesToDatabase();
                break;
            default:
                outputProvider.Output("Invalid terminal command entered. Please refer to the documentation");
                return;
        }
    }

    private enum Subcommand
    {
        AddLas,
        RemoveLas,
        RemoveUser,
        AddConsortia,
        RemoveConsortia,
        FixAllUserOwnedConsortia,
        AddAllMissingAuthoritiesToDatabase
    }
}