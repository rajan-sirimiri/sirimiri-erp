using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using MySql.Data.MySqlClient;

namespace StockApp.DAL
{
    /// <summary>
    /// Zoho Books API helper — token management, HTTP calls, master sync.
    /// All methods are static; token auto-refreshes when expired.
    /// India domain: zohoapis.in / accounts.zoho.in
    /// </summary>
    public static class ZohoHelper
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString;

        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 10 * 1024 * 1024 };

        // ══════════════════════════════════════════════════════════════
        //  CONFIG & TOKEN MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        private static DataRow GetConfig()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand("SELECT * FROM Zoho_Config WHERE ConfigID=1;", conn))
            { conn.Open(); dt.Load(cmd.ExecuteReader()); }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>Get a valid access token, refreshing if expired.</summary>
        public static string GetAccessToken()
        {
            var cfg = GetConfig();
            if (cfg == null) throw new Exception("Zoho_Config not found. Run Phase 0 SQL first.");

            string accessToken = cfg["AccessToken"]?.ToString();
            DateTime? expiry = cfg["AccessTokenExpiry"] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(cfg["AccessTokenExpiry"]) : null;

            // If token exists and not expired (with 5-min buffer), use it
            if (!string.IsNullOrEmpty(accessToken) && expiry.HasValue && expiry.Value > DateTime.UtcNow.AddMinutes(5))
                return accessToken;

            // Refresh the token
            string refreshToken = cfg["RefreshToken"]?.ToString();
            if (string.IsNullOrEmpty(refreshToken))
                throw new Exception("No refresh token stored. Re-run OAuth grant token exchange.");

            string clientId = cfg["ClientID"].ToString();
            string clientSecret = cfg["ClientSecret"].ToString();
            string accountsDomain = cfg["AccountsDomain"].ToString();

            string url = accountsDomain + "/oauth/v2/token";
            string postData = "refresh_token=" + Uri.EscapeDataString(refreshToken)
                + "&client_id=" + Uri.EscapeDataString(clientId)
                + "&client_secret=" + Uri.EscapeDataString(clientSecret)
                + "&grant_type=refresh_token";

            string response = HttpPost(url, postData, "application/x-www-form-urlencoded", null);
            var result = Json.Deserialize<Dictionary<string, object>>(response);

            if (result.ContainsKey("error"))
            {
                LogSync("TokenRefresh", "Config", "1", null, "Error", "Refresh failed: " + response);
                throw new Exception("Token refresh failed: " + result["error"]);
            }

            string newToken = result["access_token"].ToString();
            int expiresIn = Convert.ToInt32(result["expires_in"]);

            // Store new token in DB
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "UPDATE Zoho_Config SET AccessToken=?tok, AccessTokenExpiry=?exp WHERE ConfigID=1;", conn))
            {
                cmd.Parameters.AddWithValue("?tok", newToken);
                cmd.Parameters.AddWithValue("?exp", DateTime.UtcNow.AddSeconds(expiresIn - 60));
                conn.Open(); cmd.ExecuteNonQuery();
            }

            LogSync("TokenRefresh", "Config", "1", null, "Success", "Token refreshed, expires in " + expiresIn + "s");
            return newToken;
        }

        public static string GetOrgId()
        {
            var cfg = GetConfig();
            return cfg?["OrganizationID"]?.ToString() ?? "";
        }

        public static string GetApiDomain()
        {
            var cfg = GetConfig();
            return cfg?["ApiDomain"]?.ToString() ?? "https://www.zohoapis.in";
        }

        // ══════════════════════════════════════════════════════════════
        //  HTTP HELPERS
        // ══════════════════════════════════════════════════════════════

        private static string HttpPost(string url, string body, string contentType, string authToken)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            request.Timeout = 30000;
            if (!string.IsNullOrEmpty(authToken))
                request.Headers.Add("Authorization", "Zoho-oauthtoken " + authToken);

            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            return ReadResponse(request);
        }

        private static string HttpGet(string url, string authToken)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 30000;
            if (!string.IsNullOrEmpty(authToken))
                request.Headers.Add("Authorization", "Zoho-oauthtoken " + authToken);

            return ReadResponse(request);
        }

        private static string HttpPut(string url, string body, string authToken)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Timeout = 30000;
            if (!string.IsNullOrEmpty(authToken))
                request.Headers.Add("Authorization", "Zoho-oauthtoken " + authToken);

            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            return ReadResponse(request);
        }

        private static string ReadResponse(HttpWebRequest request)
        {
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                        return reader.ReadToEnd();
                }
                throw;
            }
        }

        /// <summary>Zoho Books API GET call with auto-token and org_id.</summary>
        public static Dictionary<string, object> ZohoGet(string endpoint)
        {
            string token = GetAccessToken();
            string url = GetApiDomain() + "/books/v3/" + endpoint
                + (endpoint.Contains("?") ? "&" : "?") + "organization_id=" + GetOrgId();
            string response = HttpGet(url, token);
            return Json.Deserialize<Dictionary<string, object>>(response);
        }

        /// <summary>Zoho Books API POST call with JSON body.</summary>
        public static Dictionary<string, object> ZohoPost(string endpoint, Dictionary<string, object> body)
        {
            string token = GetAccessToken();
            string url = GetApiDomain() + "/books/v3/" + endpoint + "?organization_id=" + GetOrgId();
            string jsonBody = Json.Serialize(body);
            string response = HttpPost(url, jsonBody, "application/json", token);
            return Json.Deserialize<Dictionary<string, object>>(response);
        }

        /// <summary>Zoho Books API PUT call with JSON body.</summary>
        public static Dictionary<string, object> ZohoPut(string endpoint, Dictionary<string, object> body)
        {
            string token = GetAccessToken();
            string url = GetApiDomain() + "/books/v3/" + endpoint + "?organization_id=" + GetOrgId();
            string jsonBody = Json.Serialize(body);
            string response = HttpPut(url, jsonBody, token);
            return Json.Deserialize<Dictionary<string, object>>(response);
        }

        // ══════════════════════════════════════════════════════════════
        //  MASTER SYNC — ITEMS (Products → Zoho)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Get all active FG products from ERP that need to go to Zoho.</summary>
        public static DataTable GetProductsForSync()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.Description, p.HSNCode, p.GSTRate, " +
                "p.ProductType, ou.Abbreviation AS Unit, " +
                "zm.ZohoItemID, zm.SyncStatus " +
                "FROM PP_Products p " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "LEFT JOIN Zoho_ItemMap zm ON zm.ProductID = p.ProductID " +
                "WHERE p.IsActive = 1 AND p.ProductType IN ('Core','Conversion','Prefilled Conversion') " +
                "ORDER BY p.ProductName;", conn))
            { conn.Open(); dt.Load(cmd.ExecuteReader()); }
            return dt;
        }

        /// <summary>Push a single product to Zoho Books as an Item. Creates or updates.</summary>
        public static string SyncProductToZoho(int productId)
        {
            // Get product details
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT p.ProductID, p.ProductCode, p.ProductName, p.Description, p.HSNCode, p.GSTRate, " +
                "ou.Abbreviation AS Unit " +
                "FROM PP_Products p " +
                "JOIN MM_UOM ou ON ou.UOMID = p.OutputUOMID " +
                "WHERE p.ProductID=?pid;", conn))
            {
                cmd.Parameters.AddWithValue("?pid", productId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            if (dt.Rows.Count == 0) return "Product not found";
            DataRow p = dt.Rows[0];

            // Check if already mapped
            string existingZohoId = GetZohoItemId(productId);

            var item = new Dictionary<string, object>
            {
                { "name", p["ProductName"].ToString() },
                { "sku", p["ProductCode"].ToString() },
                { "description", p["Description"] != DBNull.Value ? p["Description"].ToString() : "" },
                { "rate", 0 },
                { "unit", p["Unit"].ToString() },
                { "product_type", "goods" },
                { "is_taxable", true },
                { "hsn_or_sac", p["HSNCode"] != DBNull.Value ? p["HSNCode"].ToString() : "" },
                { "item_type", "sales_and_purchases" }
            };

            try
            {
                Dictionary<string, object> result;
                if (!string.IsNullOrEmpty(existingZohoId))
                {
                    // Update existing
                    result = ZohoPut("items/" + existingZohoId, item);
                }
                else
                {
                    // Create new
                    result = ZohoPost("items", item);
                }

                int code = Convert.ToInt32(result["code"]);
                if (code == 0)
                {
                    var zohoItem = result["item"] as Dictionary<string, object>;
                    string zohoItemId = zohoItem["item_id"].ToString();
                    string zohoName = zohoItem["name"].ToString();

                    // Upsert mapping
                    SaveItemMap(productId, zohoItemId, zohoName, "Synced");
                    LogSync("SyncItem", "Product", productId.ToString(), zohoItemId, "Success",
                        (string.IsNullOrEmpty(existingZohoId) ? "Created" : "Updated") + ": " + zohoName);
                    return "OK";
                }
                else
                {
                    string msg = result.ContainsKey("message") ? result["message"].ToString() : "Unknown error";
                    SaveItemMap(productId, existingZohoId ?? "", "", "Error");
                    LogSync("SyncItem", "Product", productId.ToString(), null, "Error", msg);
                    return "Error: " + msg;
                }
            }
            catch (Exception ex)
            {
                LogSync("SyncItem", "Product", productId.ToString(), null, "Error", ex.Message);
                return "Error: " + ex.Message;
            }
        }

        /// <summary>Sync ALL active products to Zoho.</summary>
        public static int SyncAllProducts()
        {
            var products = GetProductsForSync();
            int success = 0;
            foreach (DataRow r in products.Rows)
            {
                string result = SyncProductToZoho(Convert.ToInt32(r["ProductID"]));
                if (result == "OK") success++;
            }
            return success;
        }

        // ══════════════════════════════════════════════════════════════
        //  MASTER SYNC — CUSTOMERS (PK_Customers → Zoho Contacts)
        // ══════════════════════════════════════════════════════════════

        public static DataTable GetCustomersForSync()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT c.CustomerID, c.CustomerCode, c.CustomerName, c.ContactPerson, " +
                "c.Phone, c.Email, c.Address, c.City, c.State, c.PinCode, c.GSTIN, " +
                "zm.ZohoContactID, zm.SyncStatus " +
                "FROM PK_Customers c " +
                "LEFT JOIN Zoho_CustomerMap zm ON zm.CustomerID = c.CustomerID " +
                "WHERE c.IsActive = 1 " +
                "ORDER BY c.CustomerName;", conn))
            { conn.Open(); dt.Load(cmd.ExecuteReader()); }
            return dt;
        }

        public static string SyncCustomerToZoho(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM PK_Customers WHERE CustomerID=?cid;", conn))
            {
                cmd.Parameters.AddWithValue("?cid", customerId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            if (dt.Rows.Count == 0) return "Customer not found";
            DataRow c = dt.Rows[0];

            string existingZohoId = GetZohoCustomerId(customerId);

            string gstin = c["GSTIN"] != DBNull.Value ? c["GSTIN"].ToString().Trim() : "";
            bool hasGstin = !string.IsNullOrEmpty(gstin) && gstin.Length == 15;

            var contact = new Dictionary<string, object>
            {
                { "contact_name", c["CustomerName"].ToString() },
                { "contact_type", "customer" },
                { "customer_sub_type", "business" },
                { "company_name", c["CustomerName"].ToString() }
            };

            // GST treatment
            if (hasGstin)
            {
                contact["gst_treatment"] = "business_gst";
                contact["gst_no"] = gstin;
            }
            else
            {
                contact["gst_treatment"] = "consumer";
            }

            // Contact person
            string contactPerson = c["ContactPerson"] != DBNull.Value ? c["ContactPerson"].ToString() : "";
            string email = c["Email"] != DBNull.Value ? c["Email"].ToString() : "";
            string phone = c["Phone"] != DBNull.Value ? c["Phone"].ToString() : "";

            if (!string.IsNullOrEmpty(contactPerson))
            {
                contact["contact_persons"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "first_name", contactPerson },
                        { "email", email },
                        { "phone", phone },
                        { "is_primary_contact", true }
                    }
                };
            }

            // Billing address
            var billing = new Dictionary<string, object>();
            if (c["Address"] != DBNull.Value && !string.IsNullOrEmpty(c["Address"].ToString()))
                billing["address"] = c["Address"].ToString();
            if (c["City"] != DBNull.Value) billing["city"] = c["City"].ToString();
            if (c["State"] != DBNull.Value) billing["state"] = c["State"].ToString();
            if (c["PinCode"] != DBNull.Value) billing["zip"] = c["PinCode"].ToString();
            billing["country"] = "India";
            if (billing.Count > 1) contact["billing_address"] = billing;

            try
            {
                Dictionary<string, object> result;
                if (!string.IsNullOrEmpty(existingZohoId))
                    result = ZohoPut("contacts/" + existingZohoId, contact);
                else
                    result = ZohoPost("contacts", contact);

                int code = Convert.ToInt32(result["code"]);
                if (code == 0)
                {
                    var zohoContact = result["contact"] as Dictionary<string, object>;
                    string zohoId = zohoContact["contact_id"].ToString();
                    string zohoName = zohoContact["contact_name"].ToString();

                    SaveCustomerMap(customerId, zohoId, zohoName, "Synced");
                    LogSync("SyncCustomer", "Customer", customerId.ToString(), zohoId, "Success",
                        (string.IsNullOrEmpty(existingZohoId) ? "Created" : "Updated") + ": " + zohoName);
                    return "OK";
                }
                else
                {
                    string msg = result.ContainsKey("message") ? result["message"].ToString() : "Unknown error";
                    LogSync("SyncCustomer", "Customer", customerId.ToString(), null, "Error", msg);
                    return "Error: " + msg;
                }
            }
            catch (Exception ex)
            {
                LogSync("SyncCustomer", "Customer", customerId.ToString(), null, "Error", ex.Message);
                return "Error: " + ex.Message;
            }
        }

        public static int SyncAllCustomers()
        {
            var customers = GetCustomersForSync();
            int success = 0;
            foreach (DataRow r in customers.Rows)
            {
                string result = SyncCustomerToZoho(Convert.ToInt32(r["CustomerID"]));
                if (result == "OK") success++;
            }
            return success;
        }

        // ══════════════════════════════════════════════════════════════
        //  MASTER SYNC — SUPPLIERS (MM_Suppliers → Zoho Vendors)
        // ══════════════════════════════════════════════════════════════

        public static DataTable GetSuppliersForSync()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT s.SupplierID, s.SupplierCode, s.SupplierName, s.ContactPerson, " +
                "s.Phone, s.Email, s.GSTNo, s.PAN, s.Address, s.City, s.State, s.PinCode, " +
                "zm.ZohoContactID, zm.SyncStatus " +
                "FROM MM_Suppliers s " +
                "LEFT JOIN Zoho_SupplierMap zm ON zm.SupplierID = s.SupplierID " +
                "WHERE s.IsActive = 1 " +
                "ORDER BY s.SupplierName;", conn))
            { conn.Open(); dt.Load(cmd.ExecuteReader()); }
            return dt;
        }

        public static string SyncSupplierToZoho(int supplierId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM MM_Suppliers WHERE SupplierID=?sid;", conn))
            {
                cmd.Parameters.AddWithValue("?sid", supplierId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            if (dt.Rows.Count == 0) return "Supplier not found";
            DataRow s = dt.Rows[0];

            string existingZohoId = GetZohoSupplierId(supplierId);

            string gstin = s["GSTNo"] != DBNull.Value ? s["GSTNo"].ToString().Trim() : "";
            bool hasGstin = !string.IsNullOrEmpty(gstin) && gstin.Length == 15;

            var contact = new Dictionary<string, object>
            {
                { "contact_name", s["SupplierName"].ToString() },
                { "contact_type", "vendor" },
                { "company_name", s["SupplierName"].ToString() }
            };

            if (hasGstin)
            {
                contact["gst_treatment"] = "business_gst";
                contact["gst_no"] = gstin;
            }
            else
            {
                contact["gst_treatment"] = "business_none";
            }

            string contactPerson = s["ContactPerson"] != DBNull.Value ? s["ContactPerson"].ToString() : "";
            string email = s["Email"] != DBNull.Value ? s["Email"].ToString() : "";
            string phone = s["Phone"] != DBNull.Value ? s["Phone"].ToString() : "";

            if (!string.IsNullOrEmpty(contactPerson))
            {
                contact["contact_persons"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "first_name", contactPerson },
                        { "email", email },
                        { "phone", phone },
                        { "is_primary_contact", true }
                    }
                };
            }

            var billing = new Dictionary<string, object>();
            if (s["Address"] != DBNull.Value) billing["address"] = s["Address"].ToString();
            if (s["City"] != DBNull.Value) billing["city"] = s["City"].ToString();
            if (s["State"] != DBNull.Value) billing["state"] = s["State"].ToString();
            if (s["PinCode"] != DBNull.Value) billing["zip"] = s["PinCode"].ToString();
            billing["country"] = "India";
            if (billing.Count > 1) contact["billing_address"] = billing;

            try
            {
                Dictionary<string, object> result;
                if (!string.IsNullOrEmpty(existingZohoId))
                    result = ZohoPut("contacts/" + existingZohoId, contact);
                else
                    result = ZohoPost("contacts", contact);

                int code = Convert.ToInt32(result["code"]);
                if (code == 0)
                {
                    var zohoContact = result["contact"] as Dictionary<string, object>;
                    string zohoId = zohoContact["contact_id"].ToString();
                    string zohoName = zohoContact["contact_name"].ToString();

                    SaveSupplierMap(supplierId, zohoId, zohoName, "Synced");
                    LogSync("SyncSupplier", "Supplier", supplierId.ToString(), zohoId, "Success",
                        (string.IsNullOrEmpty(existingZohoId) ? "Created" : "Updated") + ": " + zohoName);
                    return "OK";
                }
                else
                {
                    string msg = result.ContainsKey("message") ? result["message"].ToString() : "Unknown error";
                    LogSync("SyncSupplier", "Supplier", supplierId.ToString(), null, "Error", msg);
                    return "Error: " + msg;
                }
            }
            catch (Exception ex)
            {
                LogSync("SyncSupplier", "Supplier", supplierId.ToString(), null, "Error", ex.Message);
                return "Error: " + ex.Message;
            }
        }

        public static int SyncAllSuppliers()
        {
            var suppliers = GetSuppliersForSync();
            int success = 0;
            foreach (DataRow r in suppliers.Rows)
            {
                string result = SyncSupplierToZoho(Convert.ToInt32(r["SupplierID"]));
                if (result == "OK") success++;
            }
            return success;
        }

        // ══════════════════════════════════════════════════════════════
        //  CONNECTION TEST
        // ══════════════════════════════════════════════════════════════

        /// <summary>Test the Zoho connection by calling GET /organizations.</summary>
        public static string TestConnection()
        {
            try
            {
                var result = ZohoGet("organizations");
                int code = Convert.ToInt32(result["code"]);
                if (code == 0)
                {
                    var orgs = result["organizations"] as System.Collections.ArrayList;
                    if (orgs != null && orgs.Count > 0)
                    {
                        var org = orgs[0] as Dictionary<string, object>;
                        string name = org?["name"]?.ToString() ?? "Unknown";
                        LogSync("TestConnection", null, null, null, "Success", "Connected to: " + name);
                        return "Connected to: " + name;
                    }
                    return "Connected but no organizations found.";
                }
                return "Error code: " + code;
            }
            catch (Exception ex)
            {
                LogSync("TestConnection", null, null, null, "Error", ex.Message);
                return "Error: " + ex.Message;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  MAPPING LOOKUPS
        // ══════════════════════════════════════════════════════════════

        public static string GetZohoItemId(int productId)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT ZohoItemID FROM Zoho_ItemMap WHERE ProductID=?pid;", conn))
            {
                cmd.Parameters.AddWithValue("?pid", productId);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? val.ToString() : null;
            }
        }

        public static string GetZohoCustomerId(int customerId)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT ZohoContactID FROM Zoho_CustomerMap WHERE CustomerID=?cid;", conn))
            {
                cmd.Parameters.AddWithValue("?cid", customerId);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? val.ToString() : null;
            }
        }

        public static string GetZohoSupplierId(int supplierId)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT ZohoContactID FROM Zoho_SupplierMap WHERE SupplierID=?sid;", conn))
            {
                cmd.Parameters.AddWithValue("?sid", supplierId);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value ? val.ToString() : null;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  MAPPING SAVE
        // ══════════════════════════════════════════════════════════════

        private static void SaveItemMap(int productId, string zohoItemId, string zohoName, string status)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "INSERT INTO Zoho_ItemMap (ProductID, ZohoItemID, ZohoItemName, SyncStatus, LastSyncAt) " +
                "VALUES(?pid, ?zid, ?zname, ?st, NOW()) " +
                "ON DUPLICATE KEY UPDATE ZohoItemID=?zid, ZohoItemName=?zname, SyncStatus=?st, LastSyncAt=NOW();", conn))
            {
                cmd.Parameters.AddWithValue("?pid", productId);
                cmd.Parameters.AddWithValue("?zid", zohoItemId);
                cmd.Parameters.AddWithValue("?zname", zohoName);
                cmd.Parameters.AddWithValue("?st", status);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        private static void SaveCustomerMap(int customerId, string zohoContactId, string zohoName, string status)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "INSERT INTO Zoho_CustomerMap (CustomerID, ZohoContactID, ZohoContactName, SyncStatus, LastSyncAt) " +
                "VALUES(?cid, ?zid, ?zname, ?st, NOW()) " +
                "ON DUPLICATE KEY UPDATE ZohoContactID=?zid, ZohoContactName=?zname, SyncStatus=?st, LastSyncAt=NOW();", conn))
            {
                cmd.Parameters.AddWithValue("?cid", customerId);
                cmd.Parameters.AddWithValue("?zid", zohoContactId);
                cmd.Parameters.AddWithValue("?zname", zohoName);
                cmd.Parameters.AddWithValue("?st", status);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        private static void SaveSupplierMap(int supplierId, string zohoContactId, string zohoName, string status)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "INSERT INTO Zoho_SupplierMap (SupplierID, ZohoContactID, ZohoContactName, SyncStatus, LastSyncAt) " +
                "VALUES(?sid, ?zid, ?zname, ?st, NOW()) " +
                "ON DUPLICATE KEY UPDATE ZohoContactID=?zid, ZohoContactName=?zname, SyncStatus=?st, LastSyncAt=NOW();", conn))
            {
                cmd.Parameters.AddWithValue("?sid", supplierId);
                cmd.Parameters.AddWithValue("?zid", zohoContactId);
                cmd.Parameters.AddWithValue("?zname", zohoName);
                cmd.Parameters.AddWithValue("?st", status);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  SYNC LOG
        // ══════════════════════════════════════════════════════════════

        public static void LogSync(string action, string entityType, string entityId, string zohoId, string status, string details)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                using (var cmd = new MySqlCommand(
                    "INSERT INTO Zoho_SyncLog (Action, EntityType, EntityID, ZohoID, Status, Details) " +
                    "VALUES(?a, ?et, ?eid, ?zid, ?st, ?d);", conn))
                {
                    cmd.Parameters.AddWithValue("?a", action);
                    cmd.Parameters.AddWithValue("?et", (object)entityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("?eid", (object)entityId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("?zid", (object)zohoId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("?st", status);
                    cmd.Parameters.AddWithValue("?d", details?.Length > 500 ? details.Substring(0, 500) : details);
                    conn.Open(); cmd.ExecuteNonQuery();
                }
            }
            catch { /* Don't let logging errors break the main flow */ }
        }

        /// <summary>Get recent sync log entries.</summary>
        public static DataTable GetSyncLog(int limit = 50)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM Zoho_SyncLog ORDER BY CreatedAt DESC LIMIT ?lim;", conn))
            {
                cmd.Parameters.AddWithValue("?lim", limit);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }
    }
}
