using System;
using System.Data;
using System.Windows.Forms;

namespace ComponentInventoryManager
{
    public partial class ComponentEditForm : Form
    {
        private readonly Database db;
        private readonly int? componentId;  // null when adding a new component

        // Constructor for ADD mode
        public ComponentEditForm(Database database)
        {
            InitializeComponent();
            db = database;
            componentId = null;
            Text = "Add Component";
        }

        // Constructor for EDIT mode
        public ComponentEditForm(Database database, int existingComponentId)
        {
            InitializeComponent();
            db = database;
            componentId = existingComponentId;
            Text = "Edit Component";
        }

        private void ComponentEditForm_Load(object sender, EventArgs e)
        {
            LoadCategoriesIntoCombo();

            if (componentId.HasValue)
                LoadExistingComponent(componentId.Value);
        }

        private void LoadCategoriesIntoCombo()
        {
            DataTable categories = db.GetCategories();
            foreach (DataRow row in categories.Rows)
            {
                cmbCategory.Items.Add(new CategoryItem(
                    Convert.ToInt32(row["category_id"]),
                    row["name"].ToString()));
            }
        }

        // Pulls this component's existing values back out of the grid via a fresh
        // query, so the edit form always shows current data.
        private void LoadExistingComponent(int id)
        {
            DataTable results = db.GetComponents("", 0);
            foreach (DataRow row in results.Rows)
            {
                if (Convert.ToInt32(row["component_id"]) != id) continue;

                txtName.Text = row["name"].ToString();
                numQuantity.Value = Convert.ToDecimal(row["quantity"]);
                numMinQuantity.Value = Convert.ToDecimal(row["min_quantity"]);
                txtLocation.Text = row["location"].ToString();
                txtDatasheetUrl.Text = row["datasheet_url"].ToString();
                txtNotes.Text = row["notes"].ToString();

                string categoryName = row["category"] == DBNull.Value ? "" : row["category"].ToString();
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    CategoryItem item = (CategoryItem)cmbCategory.Items[i];
                    if (item.Name == categoryName)
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
                break;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Component name is required.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = 0;
            CategoryItem selected = cmbCategory.SelectedItem as CategoryItem;
            if (selected != null)
                categoryId = selected.Id;

            if (componentId.HasValue)
            {
                db.UpdateComponent(
                    componentId.Value,
                    txtName.Text.Trim(),
                    categoryId,
                    (int)numQuantity.Value,
                    (int)numMinQuantity.Value,
                    txtLocation.Text.Trim(),
                    txtDatasheetUrl.Text.Trim(),
                    txtNotes.Text.Trim());
            }
            else
            {
                db.AddComponent(
                    txtName.Text.Trim(),
                    categoryId,
                    (int)numQuantity.Value,
                    (int)numMinQuantity.Value,
                    txtLocation.Text.Trim(),
                    txtDatasheetUrl.Text.Trim(),
                    txtNotes.Text.Trim());
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private class CategoryItem
        {
            public int Id { get; }
            public string Name { get; }

            public CategoryItem(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
