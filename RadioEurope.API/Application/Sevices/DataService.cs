using StackExchange.Redis;
using RadioEurope.API.Models;
using RadioEurope.API.Application.Interfaces;
namespace RadioEurope.API.Application.Services;


public class DataService : IDataService
{
    private readonly IConnectionMultiplexer multiplexer;

    public DataService(IConnectionMultiplexer multiplexer)
    {
        this.multiplexer = multiplexer;
    }

    public async Task Write(LeftRightDiff Lrd)
    {
        if (multiplexer == null || string.IsNullOrEmpty(Lrd.ID))
        {
             await Task.CompletedTask;
             return;
        }
        var database = multiplexer.GetDatabase(1);
        var lrd1 = await ReadLRD(Lrd.ID);
        var left = "";
        var right = "";
        if (!string.IsNullOrEmpty( Lrd.Left))
        {
            left = Lrd.Left;
        }
        //the left value of input was empty then we maintain old value
        else if (!string.IsNullOrEmpty(lrd1?.Left))
        {
            left = lrd1.Left;
        }

        if (!string.IsNullOrEmpty( Lrd.Right))
        {
            right = Lrd.Right;
        }
        //the right value of input was empty then we maintain old value
        else if (!string.IsNullOrEmpty(lrd1?.Right))
        {
            right = lrd1.Right;
        }
        await database.HashSetAsync(Lrd.ID, new HashEntry[] { new HashEntry("Left", left), new HashEntry("Right", right) });
    }
    public async Task<LeftRightDiff?> ReadLRD(string Id)
    {
        var database = multiplexer.GetDatabase(1);
        if (database.KeyExists(Id))
        {
            var retreived = await database.HashGetAllAsync(Id);
            var retreivedLeft = retreived[0].Value;
            var retreivedRight = retreived[1].Value;
            return new LeftRightDiff
            {
                ID = Id,
                Left = retreivedLeft,
                Right = retreivedRight
            };
        }
        return null;
    }
}