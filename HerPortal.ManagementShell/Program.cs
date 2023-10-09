using HerPortal.BusinessLogic.Models;
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
                adminAction.Output(
                    "Please specify a database action, user email address and at least one custodian code");
            }

            var foundUserOrDefault = adminAction.GetUser(userEmailAddress);
            Enum userStatus = adminAction.GetUserStatus(foundUserOrDefault);

            bool userConfirmation;
            switch (devAction)
            {
                case "remove-user":
                    adminAction.TryRemoveUser(foundUserOrDefault);
                    break;
                case "remove-las":
                    userConfirmation = adminAction.ConfirmCustodianCodes(custodianCodes, userEmailAddress);
                    if (userConfirmation)
                    {
                        adminAction.RemoveLas(custodianCodes, foundUserOrDefault);
                    }

                    break;
                case "add-las":
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
            }
        }

        private class DatabaseOperation : IDatabaseOperation
        {
            private readonly HerDbContext dbContext;
            private readonly OutputProvider outputProvider;

            public DatabaseOperation(HerDbContext dbContext, OutputProvider outputProvider)
            {
                this.dbContext = dbContext;
                this.outputProvider = outputProvider;
            }

            public List<User> GetUsersWithLocalAuthorities()
            {
                return dbContext.Users
                    .Include(user => user.LocalAuthorities)
                    .ToList();
            }

            public void RemoveUserOrLogError(User? user)
            {
                using var dbContextTransaction = dbContext.Database.BeginTransaction();
                try
                {
                    switch (user)
                    {
                        // removing a user also deletes all associated rows in the LocalAuthorityUser table
                        case null:
                            outputProvider.Output("User not found");
                            break;
                        default:
                        {
                            {
                                dbContext.Users.Remove(user);
                                dbContext.SaveChanges();
                                var deletionConfirmation = outputProvider.Confirm(
                                    "Attention! Deletion from a database. Are you sure you want to commit this transaction? (y/n)");

                                if (deletionConfirmation)
                                {
                                    dbContextTransaction.Commit();
                                }
                                else
                                {
                                    dbContextTransaction.Rollback();
                                    outputProvider.Output("Rollback complete");
                                }
                            }

                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
                    dbContextTransaction.Rollback();
                }
            }

            public void CreateUserOrLogError(string userEmailAddress, string[]? custodianCodes)
            {
                using var dbContextTransaction = dbContext.Database.BeginTransaction();
                try
                {
                    var newLaUser = new User
                    {
                        EmailAddress = userEmailAddress,
                        HasLoggedIn = false,
                        LocalAuthorities = custodianCodes!
                            .Select(code => dbContext.LocalAuthorities
                                .Single(la => la.CustodianCode == code))
                            .ToList()
                    };
                    dbContext.Add(newLaUser);
                    dbContext.SaveChanges();
                    outputProvider.Output("Operation successful");
                    dbContextTransaction.Commit();
                }
                catch (Exception e)
                {
                    outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
                    dbContextTransaction.Rollback();
                }
            }

            public void RemoveLasFromUser(string[]? custodianCodes, User? user)
            {
                var lasToRemove = user?.LocalAuthorities.Where(la => custodianCodes!.Contains(la.CustodianCode)).ToList();
                using var dbContextTransaction = dbContext.Database.BeginTransaction();
                try
                {
                    if (lasToRemove != null)
                        foreach (var la in lasToRemove)
                        {
                            user?.LocalAuthorities.Remove(la);
                        }

                    dbContext.SaveChanges();
                    outputProvider.Output("Operation successful");
                    dbContextTransaction.Commit();
                }
                catch (Exception e)
                {
                    outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
                    dbContextTransaction.Rollback();
                }
            }

            public void AddLasToUser(string[]? custodianCodes, User? user)
            {
                if (custodianCodes == null) return;
                using var dbContextTransaction = dbContext.Database.BeginTransaction();
                try
                {
                    foreach (var code in custodianCodes)
                    {
                        var la = dbContext.LocalAuthorities.SingleOrDefault(la => la.CustodianCode == code);
                        user?.LocalAuthorities.Add(la);
                    }

                    dbContext.SaveChanges();
                    outputProvider.Output("Operation successful");
                    dbContextTransaction.Commit();
                }
                catch (Exception e)
                {
                    outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
                    dbContextTransaction.Rollback();
                }
            }
        }

        private class OutputProvider : IOutputProvider
        {
            public void Output(string outputString)
            {
                Console.WriteLine(outputString);
            }

            public bool Confirm(string outputString)
            {
                Console.WriteLine(outputString);
                var inputString = Console.ReadLine();
                return inputString?.Trim() == "y";
            }
        }
    }
}