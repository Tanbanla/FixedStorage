namespace BIVN.FixedStorage.Identity.API.Infrastructure
{
    public static class UserManagerExtensions
    {
        /// <summary>
        /// Find user by user name.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>       
        /// <param name="source"></param>
        /// <param name="userName"></param>      
        /// <returns></returns>
        public static async Task<TUser> FindByUserNameAsync<TUser>(this UserManager<TUser> source, string userName) where TUser : AppUser
        {
            return await source.Users
                .Where(x => x.NormalizedUserName == source.NormalizeName(userName))
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Find user by user name include related entity.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="userName"></param>
        /// <param name="navigationPropertyPath">Navigation property expression</param>
        /// <returns></returns>
        public static async Task<TUser> FindByNameAsync<TUser, TProperty>(this UserManager<TUser> source, string userName,
            Expression<Func<TUser, TProperty>> navigationPropertyPath) where TUser : AppUser
        {
            return await source.Users.Include(navigationPropertyPath)
                .Where(x => x.NormalizedUserName == source.NormalizeName(userName))
                .SingleOrDefaultAsync();
        }

        public static async Task<TUser> FindByUserCodeAsync<TUser>(this UserManager<TUser> source, string userCode) where TUser : AppUser
        {
            return await source.Users
                .Where(x => x.Code.ToUpper() == userCode.ToUpper())
                .SingleOrDefaultAsync();
        }
    }
}
