using Microsoft.AspNetCore.Http;

namespace foodbook.Helpers
{
    public static class SessionHelper
    {
        public static bool IsAdmin(this ISession session)
        {
            var role = session.GetString("role");
            return role == "admin";
        }
        
        public static bool IsLoggedIn(this ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString("user_id"));
        }
        
        public static string GetUserId(this ISession session)
        {
            return session.GetString("user_id") ?? "";
        }
        
        public static string GetUsername(this ISession session)
        {
            return session.GetString("username") ?? "";
        }
        
        public static string GetUserEmail(this ISession session)
        {
            return session.GetString("user_email") ?? "";
        }
        
        public static string GetFullName(this ISession session)
        {
            return session.GetString("full_name") ?? "";
        }
    }
}
