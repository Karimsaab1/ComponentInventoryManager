using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ComponentInventoryManager
{
    public partial class MainForm : Form
    {
        private readonly Database db = new Database();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadComponents();
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new CategoryItem(0, "All Categories"));

            DataTable categories = db.GetCategories();
            foreach (DataRow row in categories.Rows)
            {
                cmbCategory.Items.Add(new CategoryItem(
                    Convert.ToInt32(row["category_id"]),
                    row["name"].ToString()));
            }
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadComponents()
        {
            int categoryId = 0;
            CategoryItem selected = cmbCategory.SelectedItem as CategoryItem;
            if (selected != null)
                categoryId = selected.Id;

            DataTable components = db.GetComponents(txtSearch.Text.Trim(), categoryId);
            gridComponents.DataSource = components;

            FormatGrid();
        }

        // Hides internal columns and highlights any row where quantity has
        // dropped below the component's own minimum threshold.
        private void FormatGrid()
        {
            if (gridComponents.Columns.Contains("component_id"))
                gridComponents.Columns["component_id"].Visible = false;
            if (gridComponents.Columns.Contains("min_quantity"))
                gridComponents.Columns["min_quantity"].Visible = false;
            if (gridComponents.Columns.Contains("datasheet_url"))
                gridComponents.Columns["datasheet_url"].Visible = false;

            foreach (DataGridViewRow row in gridComponents.Rows)
            {
                if (row.Cells["quantity"].Value == null) continue;

                int quantity = Convert.ToInt32(row.Cells["quantity"].Value);
                int minQuantity = Convert.ToInt32(row.Cells["min_quantity"].Value);

                if (quantity < minQuantity)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadComponents();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadComponents();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (ComponentEditForm form = new ComponentEditForm(db))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadComponents();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (gridComponents.CurrentRow == null) return;

            int componentId = Convert.ToInt32(gridComponents.CurrentRow.Cells["component_id"].Value);
            using (ComponentEditForm form = new ComponentEditForm(db, componentId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadComponents();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (gridComponents.CurrentRow == null) return;

            string name = gridComponents.CurrentRow.Cells["name"].Value.ToString();
            DialogResult confirm = MessageBox.Show(
                "Delete \"" + name + "\"? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                int componentId = Convert.ToInt32(gridComponents.CurrentRow.Cells["component_id"].Value);
                db.DeleteComponent(componentId);
                LoadComponents();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadComponents();
        }

        // Small helper so the category dropdown can show a name but carry an id.
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
