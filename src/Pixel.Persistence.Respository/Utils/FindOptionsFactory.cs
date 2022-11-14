using MongoDB.Driver;

namespace Pixel.Persistence.Respository.Utils;

public static class FindOptionsFactory
{
    public static FindOptions<TItem> LimitTo<TItem>(int limit)
    {
        return new FindOptions<TItem>()
        {
            Limit = limit,
        };
    }
}
