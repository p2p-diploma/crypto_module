using MongoDB.Bson;

namespace Crypto.Data;

public static class ObjectIdExtension
{
    public static bool IsParsable(string s) => ObjectId.TryParse(s, out _);
}