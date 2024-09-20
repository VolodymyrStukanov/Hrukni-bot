using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HrukniHohlinaBot.Extentions
{
    public static class IQueryableExtention
    {
        public static IQueryable<T> IncludeAll<T>(this IQueryable<T> query) where T : class
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public).Where(x => x.PropertyType.IsClass);
            foreach (var property in properties)
            {
                query = query.Include(property.Name);
            }
            query.AsSplitQuery();
            return query;
        }
    }
}
