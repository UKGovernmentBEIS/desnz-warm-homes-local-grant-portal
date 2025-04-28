using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WhlgPortalWebsite.Data;

namespace WhlgPortalWebsite.ManagementShell;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        var contextOptions = new DbContextOptionsBuilder<WhlgDbContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQLConnection") ??
                @"UserId=postgres;Password=postgres;Server=localhost;Port=5432;Database=whlgportaldev;Integrated Security=true;Include Error Detail=true;Pooling=true")
            .Options;

        using var context = new WhlgDbContext(contextOptions);
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
            case Subcommand.AddServiceManager:
                commandHandler.TryAddServiceManager(userEmailAddress);
                return;
            case Subcommand.RemoveServiceManager:
                commandHandler.TryRemoveServiceManager(commandHandler.GetUser(userEmailAddress));
                return;
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
        AddServiceManager,
        RemoveServiceManager,
        FixAllUserOwnedConsortia,
        AddAllMissingAuthoritiesToDatabase
    }
}