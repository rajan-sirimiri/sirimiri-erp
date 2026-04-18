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

            // Billing address — Zoho India uses "street" for address field
            string addrStr = c["Address"] != DBNull.Value ? c["Address"].ToString() : "";
            if (addrStr.Length > 100) addrStr = addrStr.Substring(0, 100);
            var billing = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(addrStr))
                billing["street"] = addrStr;
            if (c["City"] != DBNull.Value) billing["city"] = c["City"].ToString();
            if (c["State"] != DBNull.Value) billing["state"] = c["State"].ToString();
            if (c["PinCode"] != DBNull.Value) billing["zip"] = c["PinCode"].ToString();
            billing["country"] = "India";
            if (billing.Count > 1) contact["billing_address"] = billing;

            // Shipping address — use ShipTo if set, otherwise same as billing
            // Include customer name, contact person and phone directly in address field
            string custName = c["CustomerName"] != DBNull.Value ? c["CustomerName"].ToString() : "";
            string contactName = c["ContactPerson"] != DBNull.Value ? c["ContactPerson"].ToString() : "";
            string contactPhone = c["Phone"] != DBNull.Value ? c["Phone"].ToString() : "";

            // Build contact header for Ship To
            string shipHeader = custName;
            if (!string.IsNullOrEmpty(contactName)) shipHeader += ", Attn: " + contactName;
            if (!string.IsNullOrEmpty(contactPhone)) shipHeader += ", Ph: " + contactPhone;

            string shipStr = c.Table.Columns.Contains("ShipToAddress") && c["ShipToAddress"] != DBNull.Value
                ? c["ShipToAddress"].ToString().Trim() : "";
            if (!string.IsNullOrEmpty(shipStr))
            {
                string fullShipAddr = shipHeader + "\n" + shipStr;
                if (fullShipAddr.Length > 200) fullShipAddr = fullShipAddr.Substring(0, 200);
                var shipping = new Dictionary<string, object>();
                shipping["attention"] = custName;
                shipping["street"] = fullShipAddr;
                shipping["address"] = fullShipAddr;
                if (c["ShipToCity"] != DBNull.Value) shipping["city"] = c["ShipToCity"].ToString();
                if (c["ShipToState"] != DBNull.Value) shipping["state"] = c["ShipToState"].ToString();
                if (c["ShipToPinCode"] != DBNull.Value) shipping["zip"] = c["ShipToPinCode"].ToString();
                shipping["country"] = "India";
                if (!string.IsNullOrEmpty(contactPhone)) shipping["phone"] = contactPhone;
                contact["shipping_address"] = shipping;
            }
            else if (billing.Count > 1)
            {
                // No separate shipping — copy billing to shipping with contact info
                string billingStr = billing.ContainsKey("street") ? billing["street"].ToString() : "";
                string fullShipAddr = shipHeader + "\n" + billingStr;
                if (fullShipAddr.Length > 200) fullShipAddr = fullShipAddr.Substring(0, 200);
                var shipCopy = new Dictionary<string, object>();
                shipCopy["attention"] = custName;
                shipCopy["street"] = fullShipAddr;
                shipCopy["address"] = fullShipAddr;
                if (billing.ContainsKey("city")) shipCopy["city"] = billing["city"];
                if (billing.ContainsKey("state")) shipCopy["state"] = billing["state"];
                if (billing.ContainsKey("zip")) shipCopy["zip"] = billing["zip"];
                shipCopy["country"] = "India";
                if (!string.IsNullOrEmpty(contactPhone)) shipCopy["phone"] = contactPhone;
                contact["shipping_address"] = shipCopy;
            }

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
            string sAddr = s["Address"] != DBNull.Value ? s["Address"].ToString() : "";
            if (sAddr.Length > 100) sAddr = sAddr.Substring(0, 100);
            if (!string.IsNullOrEmpty(sAddr)) billing["street"] = sAddr;
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

        // ══════════════════════════════════════════════════════════════
        //  PRICING — MRP + Margin Calculation
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the effective margin % for a customer + product + channel.
        /// Checks product-specific override first, falls back to customer default.
        /// channel: "SM" (SuperMarket) or "GT" (General Trade)
        /// </summary>
        public static decimal GetEffectiveMargin(int customerId, int productId, string channel)
        {
            // Check product-specific override first
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT SuperMarketPct, GTPct FROM PK_CustomerProductMargins " +
                "WHERE CustomerID=?cid AND ProductID=?pid;", conn))
            {
                cmd.Parameters.AddWithValue("?cid", customerId);
                cmd.Parameters.AddWithValue("?pid", productId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        decimal? smOverride = rdr["SuperMarketPct"] != DBNull.Value ? (decimal?)Convert.ToDecimal(rdr["SuperMarketPct"]) : null;
                        decimal? gtOverride = rdr["GTPct"] != DBNull.Value ? (decimal?)Convert.ToDecimal(rdr["GTPct"]) : null;
                        if (channel == "SM" && smOverride.HasValue) return smOverride.Value;
                        if (channel == "GT" && gtOverride.HasValue) return gtOverride.Value;
                    }
                }
            }

            // Fall back to customer default
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT SuperMarketPct, GTPct FROM PK_CustomerMargins WHERE CustomerID=?cid;", conn))
            {
                cmd.Parameters.AddWithValue("?cid", customerId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        if (channel == "SM") return Convert.ToDecimal(rdr["SuperMarketPct"]);
                        return Convert.ToDecimal(rdr["GTPct"]);
                    }
                }
            }

            return 0; // No margin configured
        }

        /// <summary>
        /// Calculate invoice rate from MRP and margin.
        /// Rate = MRP × (1 - margin/100). Rate is inclusive of GST.
        /// </summary>
        public static decimal CalculateRate(decimal mrp, decimal marginPct)
        {
            if (mrp <= 0 || marginPct <= 0) return mrp;
            return Math.Round(mrp * (1 - marginPct / 100m), 2);
        }

        // ══════════════════════════════════════════════════════════════
        //  DC → ZOHO INVOICE
        // ══════════════════════════════════════════════════════════════

        /// <summary>Get DC details for invoice creation.</summary>
        public static DataRow GetDCForInvoice(int dcId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT dc.DCID, dc.DCNumber, dc.DCDate, dc.CustomerID, dc.Status, dc.Remarks, dc.Channel, " +
                "c.CustomerName, c.CustomerCode, c.GSTIN, c.State, c.City, c.Address, c.PinCode, c.CustomerType, " +
                "c.ContactPerson, c.Phone, c.Email, " +
                "c.ShipToAddress, c.ShipToCity, c.ShipToState, c.ShipToPinCode " +
                "FROM PK_DeliveryChallans dc " +
                "JOIN PK_Customers c ON c.CustomerID = dc.CustomerID " +
                "WHERE dc.DCID=?id;", conn))
            {
                cmd.Parameters.AddWithValue("?id", dcId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>Get DC lines with product details, selling form, and MRP.</summary>
        public static DataTable GetDCLinesForInvoice(int dcId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT l.LineID, l.ProductID, l.SellingForm, l.TotalPcs AS Qty, " +
                "l.MRP AS LineMRP, l.UnitRate AS LineRate, l.MarginPct AS LineMargin, " +
                "l.HSNCode AS LineHSN, l.GSTRate AS LineGSTRate, " +
                "p.ProductName, p.ProductCode, p.HSNCode, p.GSTRate, " +
                "IFNULL(p.ContainerType, 'JAR') AS ContainerType, " +
                "IFNULL(mrp_pcs.MRP, 0) AS MRP_PCS, " +
                "IFNULL(mrp_jar.MRP, 0) AS MRP_JAR, " +
                "IFNULL(mrp_box.MRP, 0) AS MRP_BOX, " +
                "IFNULL(mrp_case.MRP, 0) AS MRP_CASE " +
                "FROM PK_DCLines l " +
                "JOIN PP_Products p ON p.ProductID = l.ProductID " +
                "LEFT JOIN PK_ProductMRP mrp_pcs  ON mrp_pcs.ProductID=l.ProductID  AND mrp_pcs.SellingForm='PCS' " +
                "LEFT JOIN PK_ProductMRP mrp_jar  ON mrp_jar.ProductID=l.ProductID  AND mrp_jar.SellingForm='JAR' " +
                "LEFT JOIN PK_ProductMRP mrp_box  ON mrp_box.ProductID=l.ProductID  AND mrp_box.SellingForm='BOX' " +
                "LEFT JOIN PK_ProductMRP mrp_case ON mrp_case.ProductID=l.ProductID AND mrp_case.SellingForm='CASE' " +
                "WHERE l.DCID=?id;", conn))
            {
                cmd.Parameters.AddWithValue("?id", dcId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        /// <summary>
        /// Create a Zoho Books invoice from a finalised DC.
        /// channel: "SM" or "GT" — determines which margin to use.
        /// Returns "OK" or error message.
        /// </summary>
        public static string CreateInvoiceFromDC(int dcId, string channel, int userId)
        {
            // Check if already pushed — if so, we UPDATE instead of CREATE
            string existingZohoInvId = null;
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT ZohoInvoiceID FROM Zoho_InvoiceLog WHERE DCID=?id AND PushStatus='Pushed';", conn))
            {
                cmd.Parameters.AddWithValue("?id", dcId);
                conn.Open();
                var existing = cmd.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                    existingZohoInvId = existing.ToString();
            }

            bool isUpdate = !string.IsNullOrEmpty(existingZohoInvId);

            // Get DC header
            var dc = GetDCForInvoice(dcId);
            if (dc == null) return "DC not found";
            string dcStatus = dc["Status"].ToString();
            if (dcStatus != "FINALISED" && dcStatus != "DRAFT")
                return "DC status must be DRAFT or FINALISED to create invoice";

            int customerId = Convert.ToInt32(dc["CustomerID"]);

            // Get Zoho customer ID
            string zohoCustomerId = GetZohoCustomerId(customerId);
            if (string.IsNullOrEmpty(zohoCustomerId))
                return "Customer not synced to Zoho. Please sync customer first.";

            // Get DC lines
            var lines = GetDCLinesForInvoice(dcId);
            if (lines.Rows.Count == 0) return "DC has no line items";

            // Build Zoho invoice line items — use SellingForm from DC
            var lineItems = new List<Dictionary<string, object>>();
            foreach (DataRow line in lines.Rows)
            {
                int productId = Convert.ToInt32(line["ProductID"]);
                string zohoItemId = GetZohoItemId(productId);
                if (string.IsNullOrEmpty(zohoItemId))
                    return "Product '" + line["ProductName"] + "' not synced to Zoho. Sync products first.";

                // Read selling form and quantity from DC line
                string sellingForm = line.Table.Columns.Contains("SellingForm") && line["SellingForm"] != DBNull.Value
                    ? line["SellingForm"].ToString().ToUpper() : "JAR";
                int qty = Convert.ToInt32(line["Qty"]);

                // Use saved rate from DC line if available, otherwise calculate
                decimal rate = line["LineRate"] != DBNull.Value ? Convert.ToDecimal(line["LineRate"]) : 0;
                decimal mrp = line["LineMRP"] != DBNull.Value ? Convert.ToDecimal(line["LineMRP"]) : 0;

                if (rate <= 0)
                {
                    // Fallback: get MRP for the selling form and calculate
                    if (sellingForm == "PCS") mrp = Convert.ToDecimal(line["MRP_PCS"]);
                    else if (sellingForm == "JAR") mrp = Convert.ToDecimal(line["MRP_JAR"]);
                    else if (sellingForm == "BOX") mrp = Convert.ToDecimal(line["MRP_BOX"]);
                    else if (sellingForm == "CASE") mrp = Convert.ToDecimal(line["MRP_CASE"]);
                    if (mrp <= 0)
                        return "MRP not configured for '" + line["ProductName"] + "' (" + sellingForm + ").";
                    decimal marginPct = GetEffectiveMargin(customerId, productId, channel);
                    rate = CalculateRate(mrp, marginPct);
                }

                string hsn = line["LineHSN"] != DBNull.Value ? line["LineHSN"].ToString() : "";
                if (string.IsNullOrEmpty(hsn))
                    hsn = line["HSNCode"] != DBNull.Value ? line["HSNCode"].ToString() : "";
                decimal gstRate = line["LineGSTRate"] != DBNull.Value ? Convert.ToDecimal(line["LineGSTRate"]) : 0;
                if (gstRate <= 0)
                    gstRate = line["GSTRate"] != DBNull.Value ? Convert.ToDecimal(line["GSTRate"]) : 0;

                // Determine if interstate based on customer state vs org state (Tamil Nadu)
                string custSt = dc["State"] != DBNull.Value ? dc["State"].ToString().Trim().ToLower() : "";
                bool isInterState = !string.IsNullOrEmpty(custSt) && custSt != "tamil nadu";

                // Get the correct tax_id — IGST for interstate, GST for intrastate
                string taxId = isInterState ? GetZohoTaxId(gstRate, "IGST") : GetZohoTaxId(gstRate, "GST");

                var lineItem = new Dictionary<string, object>
                {
                    { "item_id", zohoItemId },
                    { "quantity", qty },
                    { "rate", rate },
                    { "unit", sellingForm },
                    { "hsn_or_sac", hsn }
                };

                if (!string.IsNullOrEmpty(taxId))
                    lineItem["tax_id"] = taxId;
                else if (gstRate > 0)
                    lineItem["tax_percentage"] = gstRate;
                else
                    lineItem["tax_exemption_id"] = GetZohoTaxExemptionId();

                string desc = "DC: " + dc["DCNumber"] + " | " + qty + " " + sellingForm;
                lineItem["description"] = desc;

                lineItems.Add(lineItem);
            }

            // Build invoice
            string custState = dc["State"] != DBNull.Value ? dc["State"].ToString().Trim() : "";
            string placeOfSupply = GetStateCode(custState);

            // Contact person
            string contactPerson = dc["ContactPerson"] != DBNull.Value ? dc["ContactPerson"].ToString() : "";
            string phone = dc["Phone"] != DBNull.Value ? dc["Phone"].ToString() : "";

            string channel2 = dc.Table.Columns.Contains("Channel") && dc["Channel"] != DBNull.Value
                ? dc["Channel"].ToString() : channel;

            var invoice = new Dictionary<string, object>
            {
                { "customer_id", zohoCustomerId },
                { "date", Convert.ToDateTime(dc["DCDate"]).ToString("yyyy-MM-dd") },
                { "reference_number", dc["DCNumber"].ToString() },
                { "notes", "DC: " + dc["DCNumber"] + " | Channel: " + (channel2 == "SM" ? "Super Market" : "General Trade")
                    + (!string.IsNullOrEmpty(contactPerson) ? " | Contact: " + contactPerson : "")
                    + (!string.IsNullOrEmpty(phone) ? " | Ph: " + phone : "") },
                { "is_inclusive_tax", true },
                { "line_items", lineItems }
            };

            if (!string.IsNullOrEmpty(placeOfSupply))
                invoice["place_of_supply"] = placeOfSupply;

            // Salesperson / contact info in custom fields or notes
            if (!string.IsNullOrEmpty(contactPerson))
                invoice["salesperson_name"] = contactPerson;

            try
            {
                // Log the invoice JSON for debugging
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string invoiceJson = serializer.Serialize(invoice);
                LogSync(isUpdate ? "UpdateInvoice_Debug" : "CreateInvoice_Debug", "Invoice", dcId.ToString(), null, "Debug",
                    invoiceJson.Length > 4000 ? invoiceJson.Substring(0, 4000) : invoiceJson);

                Dictionary<string, object> result;
                if (isUpdate)
                    result = ZohoPut("invoices/" + existingZohoInvId, invoice);
                else
                    result = ZohoPost("invoices", invoice);

                int code = Convert.ToInt32(result["code"]);
                if (code == 0)
                {
                    var zohoInv = result["invoice"] as Dictionary<string, object>;
                    string zohoInvId = zohoInv["invoice_id"].ToString();
                    string zohoInvNo = zohoInv["invoice_number"].ToString();
                    string zohoStatus = zohoInv.ContainsKey("status") ? zohoInv["status"].ToString() : "draft";

                    // Log success
                    SaveInvoiceLog(dcId, zohoInvId, zohoInvNo, zohoStatus, "Pushed", null);
                    string action = isUpdate ? "updated" : "created";
                    LogSync(isUpdate ? "UpdateInvoice" : "CreateInvoice", "Invoice", dcId.ToString(), zohoInvId, "Success",
                        "Invoice " + zohoInvNo + " " + action + " for DC " + dc["DCNumber"] + " (" + channel + ")");
                    return (isUpdate ? "UPDATED:" : "OK:") + zohoInvNo;
                }
                else
                {
                    string msg = result.ContainsKey("message") ? result["message"].ToString() : "Unknown error";
                    SaveInvoiceLog(dcId, null, null, null, "Error", msg);
                    LogSync(isUpdate ? "UpdateInvoice" : "CreateInvoice", "Invoice", dcId.ToString(), null, "Error", msg);
                    return "Error: " + msg;
                }
            }
            catch (Exception ex)
            {
                SaveInvoiceLog(dcId, null, null, null, "Error", ex.Message);
                LogSync(isUpdate ? "UpdateInvoice" : "CreateInvoice", "Invoice", dcId.ToString(), null, "Error", ex.Message);
                return "Error: " + ex.Message;
            }
        }

        private static void SaveInvoiceLog(int dcId, string zohoInvId, string zohoInvNo, string zohoStatus, string pushStatus, string error)
        {
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "INSERT INTO Zoho_InvoiceLog (DCID, ZohoInvoiceID, ZohoInvoiceNo, ZohoStatus, PushStatus, ErrorMessage, PushedAt) " +
                "VALUES(?dcid, ?zid, ?zno, ?zst, ?ps, ?err, NOW()) " +
                "ON DUPLICATE KEY UPDATE ZohoInvoiceID=IFNULL(?zid, ZohoInvoiceID), ZohoInvoiceNo=IFNULL(?zno, ZohoInvoiceNo), " +
                "ZohoStatus=IFNULL(?zst, ZohoStatus), PushStatus=?ps, ErrorMessage=?err, PushedAt=NOW();", conn))
            {
                cmd.Parameters.AddWithValue("?dcid", dcId);
                cmd.Parameters.AddWithValue("?zid", (object)zohoInvId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?zno", (object)zohoInvNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?zst", (object)zohoStatus ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?ps", pushStatus);
                cmd.Parameters.AddWithValue("?err", (object)error ?? DBNull.Value);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Check if a DC already has an invoice in Zoho.</summary>
        public static DataRow GetInvoiceLogForDC(int dcId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(ConnStr))
            using (var cmd = new MySqlCommand(
                "SELECT * FROM Zoho_InvoiceLog WHERE DCID=?id ORDER BY LogID DESC LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("?id", dcId);
                conn.Open(); dt.Load(cmd.ExecuteReader());
            }
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>Download invoice PDF from Zoho Books. Returns the PDF bytes.</summary>
        /// <summary>Map Indian state name to 2-letter GST state code for Zoho place_of_supply.</summary>
        // ── TAX LOOKUP ─────────────────────────────────────────────────────

        private static Dictionary<string, string> _taxCache = null; // key: "GST_5" or "IGST_5" → tax_id
        private static string _taxExemptionId = null;

        /// <summary>Get Zoho tax_id for a given GST rate and type (GST or IGST).</summary>
        private static string GetZohoTaxId(decimal gstRate, string taxType = "GST")
        {
            if (gstRate <= 0) return null;
            if (_taxCache == null) LoadZohoTaxes();
            string key = taxType + "_" + gstRate.ToString("0.##");
            if (_taxCache != null && _taxCache.ContainsKey(key)) return _taxCache[key];
            // Fallback: try the other type
            string altKey = (taxType == "GST" ? "IGST" : "GST") + "_" + gstRate.ToString("0.##");
            if (_taxCache != null && _taxCache.ContainsKey(altKey)) return _taxCache[altKey];
            return null;
        }

        /// <summary>Get the tax exemption ID for zero-rated items.</summary>
        private static string GetZohoTaxExemptionId()
        {
            if (_taxExemptionId != null) return _taxExemptionId;
            try
            {
                var result = ZohoGet("settings/taxexemptions");
                int code = Convert.ToInt32(result["code"]);
                if (code == 0 && result.ContainsKey("tax_exemptions"))
                {
                    var exemptions = result["tax_exemptions"] as System.Collections.ArrayList;
                    if (exemptions != null && exemptions.Count > 0)
                    {
                        var first = exemptions[0] as Dictionary<string, object>;
                        _taxExemptionId = first?["tax_exemption_id"]?.ToString() ?? "";
                        return _taxExemptionId;
                    }
                }
            }
            catch { }
            return "";
        }

        private static void LoadZohoTaxes()
        {
            _taxCache = new Dictionary<string, string>();
            try
            {
                var result = ZohoGet("settings/taxes");
                int code = Convert.ToInt32(result["code"]);
                if (code == 0 && result.ContainsKey("taxes"))
                {
                    var taxes = result["taxes"] as System.Collections.ArrayList;
                    if (taxes != null)
                    {
                        foreach (var item in taxes)
                        {
                            var tax = item as Dictionary<string, object>;
                            if (tax == null) continue;
                            string taxId = tax.ContainsKey("tax_id") ? tax["tax_id"]?.ToString() ?? "" : "";
                            string taxName = tax.ContainsKey("tax_name") ? tax["tax_name"].ToString().ToUpper() : "";
                            string taxType = tax.ContainsKey("tax_type") ? tax["tax_type"].ToString().ToUpper() : "";
                            decimal pct = 0;
                            if (tax.ContainsKey("tax_percentage"))
                                decimal.TryParse(tax["tax_percentage"].ToString(), out pct);
                            if (string.IsNullOrEmpty(taxId)) continue;

                            // Determine if IGST or GST (intra-state)
                            string cacheType;
                            if (taxName.Contains("IGST") || taxType == "IGST")
                                cacheType = "IGST";
                            else
                                cacheType = "GST";

                            string key = cacheType + "_" + pct.ToString("0.##");
                            if (!_taxCache.ContainsKey(key))
                                _taxCache[key] = taxId;
                        }
                    }
                }
                LogSync("LoadTaxes", null, null, null, "Success",
                    "Loaded " + _taxCache.Count + " taxes: " + string.Join(", ", _taxCache.Keys));
            }
            catch (Exception ex)
            {
                LogSync("LoadTaxes", null, null, null, "Error", ex.Message);
            }
        }

        private static string GetStateCode(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return "";
            string s = stateName.Trim().ToLower();
            // Map common state names to Zoho place_of_supply codes
            if (s.Contains("andhra"))    return "AP";
            if (s.Contains("arunachal")) return "AR";
            if (s.Contains("assam"))     return "AS";
            if (s.Contains("bihar"))     return "BR";
            if (s.Contains("chhattisgarh")) return "CG";
            if (s.Contains("goa"))       return "GA";
            if (s.Contains("gujarat"))   return "GJ";
            if (s.Contains("haryana"))   return "HR";
            if (s.Contains("himachal"))  return "HP";
            if (s.Contains("jharkhand")) return "JH";
            if (s.Contains("karnataka")) return "KA";
            if (s.Contains("kerala"))    return "KL";
            if (s.Contains("madhya"))    return "MP";
            if (s.Contains("maharashtra")) return "MH";
            if (s.Contains("manipur"))   return "MN";
            if (s.Contains("meghalaya")) return "ML";
            if (s.Contains("mizoram"))   return "MZ";
            if (s.Contains("nagaland"))  return "NL";
            if (s.Contains("odisha") || s.Contains("orissa")) return "OD";
            if (s.Contains("punjab"))    return "PB";
            if (s.Contains("rajasthan")) return "RJ";
            if (s.Contains("sikkim"))    return "SK";
            if (s.Contains("tamil"))     return "TN";
            if (s.Contains("telangana")) return "TS";
            if (s.Contains("tripura"))   return "TR";
            if (s.Contains("uttar pradesh")) return "UP";
            if (s.Contains("uttarakhand") || s.Contains("uttaranchal")) return "UK";
            if (s.Contains("west bengal")) return "WB";
            if (s.Contains("delhi"))     return "DL";
            if (s.Contains("jammu"))     return "JK";
            if (s.Contains("ladakh"))    return "LA";
            if (s.Contains("chandigarh")) return "CH";
            if (s.Contains("puducherry") || s.Contains("pondicherry")) return "PY";
            if (s.Contains("lakshadweep")) return "LD";
            if (s.Contains("daman") || s.Contains("dadra") || s.Contains("silvassa")) return "DN";
            if (s.Contains("andaman"))   return "AN";
            // If already a 2-letter code
            if (stateName.Trim().Length == 2) return stateName.Trim().ToUpper();
            return "";
        }

        public static byte[] GetInvoicePDF(string zohoInvoiceId)
        {
            string token = GetAccessToken();
            string url = GetApiDomain() + "/books/v3/invoices/" + zohoInvoiceId
                + "?organization_id=" + GetOrgId() + "&accept=pdf";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 30000;
            request.Headers.Add("Authorization", "Zoho-oauthtoken " + token);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>Fetch invoice from Zoho and sync changes back to ERP DC.</summary>
        public static ZohoSyncBackResult SyncInvoiceBack(int dcId)
        {
            var result = new ZohoSyncBackResult();
            try
            {
                // Get Zoho invoice ID from log
                var invLog = GetInvoiceLogForDC(dcId);
                if (invLog == null || invLog["PushStatus"].ToString() != "Pushed")
                {
                    result.Message = "No invoice found for this DC.";
                    return result;
                }
                string zohoInvId = invLog["ZohoInvoiceID"].ToString();

                // Fetch invoice from Zoho
                var zohoResp = ZohoGet("invoices/" + zohoInvId);
                int code = Convert.ToInt32(zohoResp["code"]);
                if (code != 0)
                {
                    result.Message = "Zoho API error: " + (zohoResp.ContainsKey("message") ? zohoResp["message"].ToString() : "Unknown");
                    return result;
                }

                var invoice = zohoResp["invoice"] as Dictionary<string, object>;
                if (invoice == null) { result.Message = "Invalid invoice response."; return result; }

                result.ZohoInvoiceNo = invoice.ContainsKey("invoice_number") ? invoice["invoice_number"].ToString() : "";
                result.ZohoStatus = invoice.ContainsKey("status") ? invoice["status"].ToString() : "";

                // Update invoice status in log
                string newStatus = result.ZohoStatus;
                SaveInvoiceLog(dcId, zohoInvId, result.ZohoInvoiceNo, newStatus, "Pushed", null);

                // Update DC header with invoice number and status
                string invNumber = result.ZohoInvoiceNo;
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "UPDATE PK_DeliveryChallans SET InvoiceNumber=?inv WHERE DCID=?id;", conn))
                    {
                        cmd.Parameters.AddWithValue("?inv", invNumber);
                        cmd.Parameters.AddWithValue("?id", dcId);
                        cmd.ExecuteNonQuery();
                    }
                }
                result.Changes.Add("Invoice status: " + newStatus.ToUpper());

                // Get Zoho line items
                var zohoLines = invoice.ContainsKey("line_items") ? invoice["line_items"] as System.Collections.ArrayList : null;
                if (zohoLines == null || zohoLines.Count == 0)
                {
                    result.Success = true;
                    result.Message = "Invoice synced (status only, no line items).";
                    return result;
                }

                // Get current ERP DC lines
                var erpLines = PKDatabaseHelper.GetDCLines(dcId);

                // Build map of Zoho items by item_id → {qty, rate, amount}
                var zohoItemMap = new Dictionary<string, Dictionary<string, object>>();
                foreach (var li in zohoLines)
                {
                    var lineItem = li as Dictionary<string, object>;
                    if (lineItem == null) continue;
                    string itemId = lineItem.ContainsKey("item_id") ? lineItem["item_id"].ToString() : "";
                    if (!string.IsNullOrEmpty(itemId))
                        zohoItemMap[itemId] = lineItem;
                }

                // Get FG stock for alerts
                var fgStock = PKDatabaseHelper.GetFGStockForShipment();

                // Compare and update each ERP line
                foreach (DataRow erpLine in erpLines.Rows)
                {
                    int productId = Convert.ToInt32(erpLine["ProductID"]);
                    string productName = erpLine["ProductName"].ToString();
                    int erpQty = Convert.ToInt32(erpLine["TotalPcs"]);
                    decimal erpTotal = erpLine["LineTotal"] != DBNull.Value ? Convert.ToDecimal(erpLine["LineTotal"]) : 0;

                    // Find matching Zoho item
                    string zohoItemId = GetZohoItemId(productId);
                    if (string.IsNullOrEmpty(zohoItemId) || !zohoItemMap.ContainsKey(zohoItemId)) continue;

                    var zohoItem = zohoItemMap[zohoItemId];
                    int zohoQty = zohoItem.ContainsKey("quantity") ? Convert.ToInt32(Convert.ToDouble(zohoItem["quantity"].ToString())) : 0;
                    decimal zohoRate = zohoItem.ContainsKey("rate") ? Convert.ToDecimal(zohoItem["rate"].ToString()) : 0;
                    decimal zohoAmount = zohoItem.ContainsKey("item_total") ? Convert.ToDecimal(zohoItem["item_total"].ToString()) : 0;
                    decimal zohoTax = zohoItem.ContainsKey("tax_amount") ? Convert.ToDecimal(zohoItem["tax_amount"].ToString()) : 0;

                    bool qtyChanged = zohoQty != erpQty;
                    bool amtChanged = Math.Abs(zohoAmount + zohoTax - erpTotal) > 1;

                    if (qtyChanged || amtChanged)
                    {
                        // Update ERP DC line
                        using (var conn = new MySqlConnection(ConnStr))
                        {
                            conn.Open();
                            using (var cmd = new MySqlCommand(
                                "UPDATE PK_DCLines SET TotalPcs=?qty, UnitRate=?rate, LineTotal=?lt," +
                                " TaxableValue=?tax WHERE DCID=?dcid AND ProductID=?pid ORDER BY LineID LIMIT 1;", conn))
                            {
                                cmd.Parameters.AddWithValue("?qty", zohoQty);
                                cmd.Parameters.AddWithValue("?rate", zohoRate);
                                cmd.Parameters.AddWithValue("?lt", zohoAmount + zohoTax);
                                cmd.Parameters.AddWithValue("?tax", zohoAmount);
                                cmd.Parameters.AddWithValue("?dcid", dcId);
                                cmd.Parameters.AddWithValue("?pid", productId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (qtyChanged)
                            result.Changes.Add(productName + ": Qty " + erpQty + " → " + zohoQty);
                        if (amtChanged)
                            result.Changes.Add(productName + ": Total ₹" + erpTotal.ToString("N0") + " → ₹" + (zohoAmount + zohoTax).ToString("N0"));

                        // Check stock availability if qty increased
                        if (zohoQty > erpQty)
                        {
                            DataRow stockRow = null;
                            foreach (DataRow fg in fgStock.Rows)
                                if (Convert.ToInt32(fg["ProductID"]) == productId) { stockRow = fg; break; }
                            if (stockRow != null)
                            {
                                int availTotal = Convert.ToInt32(stockRow["AvailableFGJars"]);
                                int increase = zohoQty - erpQty;
                                if (increase > availTotal)
                                    result.StockAlerts.Add(productName + ": Zoho qty increased by " + increase +
                                        " but only " + availTotal + " available in FG stock");
                            }
                        }
                    }

                    zohoItemMap.Remove(zohoItemId);
                }

                // Handle new items in Zoho not in ERP
                foreach (var kvp in zohoItemMap)
                {
                    var zohoItem = kvp.Value;
                    string itemName = zohoItem.ContainsKey("name") ? zohoItem["name"].ToString() : "Unknown";
                    int zohoQty = zohoItem.ContainsKey("quantity") ? Convert.ToInt32(Convert.ToDouble(zohoItem["quantity"].ToString())) : 0;
                    result.Changes.Add("NEW in Zoho: " + itemName + " × " + zohoQty + " (not yet in ERP DC)");
                    result.StockAlerts.Add("NEW item '" + itemName + "' added in Zoho — needs manual addition in ERP");
                }

                // Update DC grand total
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "UPDATE PK_DeliveryChallans SET GrandTotal=" +
                        "(SELECT IFNULL(SUM(LineTotal),0) FROM PK_DCLines WHERE DCID=?id) WHERE DCID=?id;", conn))
                    {
                        cmd.Parameters.AddWithValue("?id", dcId);
                        cmd.ExecuteNonQuery();
                    }
                }

                result.Success = true;
                result.Message = result.Changes.Count > 0
                    ? result.Changes.Count + " change(s) synced from Zoho."
                    : "No changes — ERP matches Zoho.";

                LogSync("SyncBack", "Invoice", dcId.ToString(), zohoInvId, "Success",
                    result.Message + (result.StockAlerts.Count > 0 ? " ALERTS: " + string.Join("; ", result.StockAlerts) : ""));
            }
            catch (Exception ex)
            {
                result.Message = "Sync error: " + ex.Message;
                LogSync("SyncBack", "Invoice", dcId.ToString(), null, "Error", ex.Message);
            }
            return result;
        }

        /// <summary>Sync all invoices in a consignment back from Zoho.</summary>
        public static List<ZohoSyncBackResult> SyncConsignmentBack(int consignmentId)
        {
            var results = new List<ZohoSyncBackResult>();
            var dcs = PKDatabaseHelper.GetDCsByConsignment(consignmentId);
            foreach (DataRow dc in dcs.Rows)
            {
                int dcId = Convert.ToInt32(dc["DCID"]);
                var r = SyncInvoiceBack(dcId);
                r.ZohoInvoiceNo = dc["DCNumber"].ToString() + (r.ZohoInvoiceNo != null ? " | " + r.ZohoInvoiceNo : "");
                results.Add(r);
            }
            return results;
        }
    }

    /// <summary>Result of syncing an invoice back from Zoho.</summary>
    public class ZohoSyncBackResult
    {
        public bool Success;
        public string Message;
        public string ZohoInvoiceNo;
        public string ZohoStatus;
        public System.Collections.Generic.List<string> Changes = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> StockAlerts = new System.Collections.Generic.List<string>();
    }
}
