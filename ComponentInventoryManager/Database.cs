using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ComponentInventoryManager
{
    // All database access goes through this class, so the rest of the app
    // never has to touch SQL directly.
    public class Database
    {
        // Update these to match your own MySQL setup.
        private const string Server = "localhost";
        private const string DatabaseName = "component_inventory";
        private const string User = "root";
        private const string Password = "Karimsaab1";

        private string ConnectionString
        {
            get
            {
                return "Server=" + Server + ";Database=" + DatabaseName +
                       ";Uid=" + User + ";Pwd=" + Password + ";";
            }
        }

        // Returns every component, joined with its category name, optionally
        // filtered by a search term (matches on component name) and/or category.
        public DataTable GetComponents(string searchTerm, int categoryId)
        {
            string query =
                "SELECT c.component_id, c.name, cat.name AS category, c.quantity, " +
                "c.min_quantity, c.location, c.datasheet_url, c.notes, c.last_updated " +
                "FROM components c " +
                "LEFT JOIN categories cat ON c.category_id = cat.category_id " +
                "WHERE (@search = '' OR c.name LIKE @searchLike) " +
                "AND (@categoryId = 0 OR c.category_id = @categoryId) " +
                "ORDER BY c.name";

            DataTable table = new DataTable();

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@search", searchTerm ?? "");
                    cmd.Parameters.AddWithValue("@searchLike", "%" + (searchTerm ?? "") + "%");
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);

                    conn.Open();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        public DataTable GetCategories()
        {
            DataTable table = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT category_id, name FROM categories ORDER BY name", conn))
                {
                    conn.Open();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            return table;
        }

        public void AddComponent(string name, int categoryId, int quantity, int minQuantity,
            string location, string datasheetUrl, string notes)
        {
            string query =
                "INSERT INTO components (name, category_id, quantity, min_quantity, location, datasheet_url, notes) " +
                "VALUES (@name, @categoryId, @quantity, @minQuantity, @location, @datasheetUrl, @notes)";

            RunNonQuery(query, name, categoryId, quantity, minQuantity, location, datasheetUrl, notes, null);
        }

        public void UpdateComponent(int componentId, string name, int categoryId, int quantity,
            int minQuantity, string location, string datasheetUrl, string notes)
        {
            string query =
                "UPDATE components SET name = @name, category_id = @categoryId, quantity = @quantity, " +
                "min_quantity = @minQuantity, location = @location, datasheet_url = @datasheetUrl, notes = @notes " +
                "WHERE component_id = @componentId";

            RunNonQuery(query, name, categoryId, quantity, minQuantity, location, datasheetUrl, notes, componentId);
        }

        public void DeleteComponent(int componentId)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "DELETE FROM components WHERE component_id = @componentId", conn))
                {
                    cmd.Parameters.AddWithValue("@componentId", componentId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Shared helper for the Add/Update queries above, since they take the same parameters.
        private void RunNonQuery(string query, string name, int categoryId, int quantity,
            int minQuantity, string location, string datasheetUrl, string notes, int? componentId)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@minQuantity", minQuantity);
                    cmd.Parameters.AddWithValue("@location", location ?? "");
                    cmd.Parameters.AddWithValue("@datasheetUrl", datasheetUrl ?? "");
                    cmd.Parameters.AddWithValue("@notes", notes ?? "");
                    if (componentId.HasValue)
                        cmd.Parameters.AddWithValue("@componentId", componentId.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
