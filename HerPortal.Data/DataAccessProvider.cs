using Microsoft.EntityFrameworkCore;
using HerPortal.BusinessLogic.Models;

namespace HerPortal.Data;

public class DataAccessProvider : IDataAccessProvider
{
    private readonly HerDbContext context;

    public DataAccessProvider(HerDbContext context)
    {
        this.context = context;
    }
}