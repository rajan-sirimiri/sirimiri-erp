using System;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;
using StockApp.DAL;

namespace StockApp
{
    public static class AuthHelper
    {
        // ── Session keys ──────────────────────────────────────────
        public const string SESS_USERID   = "UserID";
        public const string SESS_USERNAME = "Username";
        public const string SESS_FULLNAME = "FullName";
        public const string SESS_ROLE     = "Role";
        public const string SESS_STATEID  = "StateID";
        public const string SESS_MUSTCHG  = "MustChangePwd";

        // ── Password hashing ──────────────────────────────────────
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                string combined = salt + password;
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }

        public static bool VerifyPassword(string password, string salt, string storedHash)
            => HashPassword(password, salt).Equals(storedHash, StringComparison.OrdinalIgnoreCase);

        // ── Session helpers ───────────────────────────────────────
        public static bool IsLoggedIn(HttpSessionState session)
            => session[SESS_USERID] != null;

        public static int GetUserID(HttpSessionState session)
            => session[SESS_USERID] != null ? (int)session[SESS_USERID] : 0;

        public static string GetRole(HttpSessionState session)
            => session[SESS_ROLE]?.ToString() ?? "";

        public static int GetStateID(HttpSessionState session)
            => session[SESS_STATEID] != null ? (int)session[SESS_STATEID] : 0;

        public static bool IsAdmin(HttpSessionState session)   => GetRole(session) == "Admin";
        public static bool IsManager(HttpSessionState session) => GetRole(session) == "Manager";
        public static bool IsField(HttpSessionState session)   => GetRole(session) == "Field";
        public static bool CanSubmit(HttpSessionState session) => GetRole(session) != "Manager";
        public static bool MustChangePassword(HttpSessionState session)
            => session[SESS_MUSTCHG] != null && (bool)session[SESS_MUSTCHG];

        public static void SetSession(HttpSessionState session, DataRow user)
        {
            session[SESS_USERID]   = Convert.ToInt32(user["UserID"]);
            session[SESS_USERNAME] = user["Username"].ToString();
            session[SESS_FULLNAME] = user["FullName"].ToString();
            session[SESS_ROLE]     = user["Role"].ToString();
            session[SESS_STATEID]  = user["StateID"] != DBNull.Value ? (int?)Convert.ToInt32(user["StateID"]) : null;
            session[SESS_MUSTCHG]  = Convert.ToBoolean(user["MustChangePwd"]);
        }

        public static void ClearSession(HttpSessionState session)
            => session.Clear();

        // ── Page guard — call at top of Page_Load ─────────────────
        public static void RequireLogin(System.Web.UI.Page page)
        {
            if (!IsLoggedIn(page.Session))
            {
                page.Response.Redirect("~/Login.aspx", true);
                return;
            }
            if (MustChangePassword(page.Session))
            {
                page.Response.Redirect("~/ChangePassword.aspx", true);
                return;
            }
        }

        public static void RequireAdmin(System.Web.UI.Page page)
        {
            RequireLogin(page);
            if (!IsAdmin(page.Session))
                page.Response.Redirect("~/StockEntry.aspx", true);
        }
    }
}
