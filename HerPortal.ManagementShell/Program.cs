using System.Security.Principal;
using HerPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace HerPortal.ManagementShell
{
    public static class Program
    {
        public enum UserStatus
        {
            New,
            Active
        }

        private enum Command
        {
            AddLas,
            RemoveLas,
            RemoveUser
        }
        public static void Main(string[] args)
        {
            var contextOptions = new DbContextOptionsBuilder<HerDbContext>()
                .UseNpgsql(
                    @"UserId=postgres;Password=postgres;Server=localhost;Port=5432;Database=herportaldev;Integrated Security=true;Include Error Detail=true;Pooling=true")
                .Options;

            using var context = new HerDbContext(contextOptions);
            var outputProvider = new OutputProvider();
            var dbOperation = new DatabaseOperation(context, outputProvider);
            var adminAction = new AdminAction(dbOperation, outputProvider);

            var devAction = "";
            var userEmailAddress = "";
            var custodianCodes = Array.Empty<string>();

            try
            {
                devAction = args[0];
                userEmailAddress = args[1];
                custodianCodes = args.Skip(2).ToArray();
            }
            catch (IndexOutOfRangeException)
            {
                outputProvider.Output(
                    "Please specify a database action, user email address and at least one custodian code");
            }

            var foundUserOrDefault = adminAction.GetUser(userEmailAddress);
            var userStatus = adminAction.GetUserStatus(foundUserOrDefault);

            bool userConfirmation;
            switch (Enum.Parse<Command>(devAction, true))
            {
                case Command.RemoveUser:
                    adminAction.TryRemoveUser(foundUserOrDefault);
                    break;
                case Command.RemoveLas:
                    userConfirmation = adminAction.ConfirmCustodianCodes(custodianCodes, userEmailAddress);
                    if (userConfirmation)
                    {
                        adminAction.RemoveLas(custodianCodes, foundUserOrDefault);
                    }

                    break;
                case Command.AddLas:
                    adminAction.DisplayUserStatus(userStatus);


                    userConfirmation = adminAction.ConfirmCustodianCodes(custodianCodes, userEmailAddress);

                    if (userConfirmation)
                    {
                        if (userStatus.Equals(UserStatus.Active))
                        {
                            adminAction.AddLas(custodianCodes, foundUserOrDefault);
                        }
                        else if (userStatus.Equals(UserStatus.New))
                        {
                            adminAction.TryCreateUser(userEmailAddress, custodianCodes);
                        }
                    }

                    break;
                default:
                    outputProvider.Output("Invalid terminal command entered. Please refer to the documentation");
                    break;
            }
        }
    }
}