using HerPortal.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace HerPortal.ManagementShell
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private enum Subcommand
        {
            AddLas,
            RemoveLas,
            RemoveUser
        }
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
            var adminAction = new AdminAction(dbOperation, outputProvider);

            Subcommand command;
            var userEmailAddress = "";
            var custodianCodes = Array.Empty<string>();

            try
            {
                command = Enum.Parse<Subcommand>(args[0], true);
                userEmailAddress = args[1];
                custodianCodes = args.Skip(2).ToArray();
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
                    adminAction.TryRemoveUser(adminAction.GetUser(userEmailAddress));
                    return;
                case Subcommand.RemoveLas:
                    adminAction.RemoveLas(adminAction.GetUser(userEmailAddress), custodianCodes);
                    return;
                case Subcommand.AddLas:
                    adminAction.CreateOrUpdateUser(userEmailAddress, custodianCodes);
                    return;
                default:
                    outputProvider.Output("Invalid terminal command entered. Please refer to the documentation");
                    return;
            }
        }
    }
}