using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UMC.OQC.Models;

namespace FujiXeroxShippingSystem
{
    public partial class FormModel : Form
    {
        Models_BUS models_BUS = new Models_BUS();
        public FormModel()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            var models = models_BUS.GetAll("FujiXerox");
            //autocomplete model
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();

            foreach (var item in models)
            {
                collection.Add(item.ModelName);
            }
            txtSearch.AutoCompleteCustomSource = collection;

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = models;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtModelName.Text))
            {
                errorProvider1.SetError(txtModelName, "Vui lòng nhập vào Model Name!");
                txtModelName.Focus();
                return;
            }

            if (txtQuantity.Value <= 0)
            {
                errorProvider1.SetError(txtQuantity, "Vui lòng nhập vào Số lượng!");
                txtQuantity.Focus();
                return;
            }

            if (chkHP.Checked == true)
            {
                if (string.IsNullOrEmpty(txtQuantityHP.Text))
                {
                    errorProvider1.SetError(txtQuantityHP, "Vui lòng nhập vào số lượng!");
                    txtQuantityHP.Focus();
                    return;
                }
            }
            if (lblID.Text != "")
            {
                var item = models_BUS.Get(lblID.Text);
                if (item != null)
                {
                    item.Quantity = Convert.ToInt32(txtQuantity.Value);
                    if (chkHP.Checked == true)
                    {
                        item.FujiHP = chkHP.Checked;
                        item.QuantityHP = Convert.ToInt32(txtQuantityHP.Value);
                    }
                    try
                    {
                        models_BUS.Update(item);
                        LoadData();
                        MessageBox.Show($"Lưu thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                var model = new Models()
                {
                    ModelID = Guid.NewGuid().ToString(),
                    ModelName = txtModelName.Text,
                    Quantity = Convert.ToInt32(txtQuantity.Value),
                    CustomerName = "FujiXerox",
                    CodeMurata = null,
                    CheckWidthModelCus = false,
                    FujiHP = chkHP.Checked,
                    QuantityHP = Convert.ToInt32(txtQuantityHP.Value)
                };

                try
                {
                    var item = models_BUS.GetModelByName(model.ModelName);
                    if (item == null)
                    {
                        models_BUS.Add(model);
                        LoadData();
                        MessageBox.Show($"Lưu thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Model [{model.ModelName}] này đã tồn tại rồi. Vui lòng kiểm tra lại! ", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            txtModelName.ResetText();
            txtQuantity.ResetText();
            chkHP.Checked = false;
            txtQuantityHP.ResetText();
            txtModelName.Focus();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                List<Models> models = new List<Models>();
                var item = models_BUS.GetModelByName(txtSearch.Text);
                if (item != null)
                {
                    models.Add(item);
                    txtSearch.ResetText();
                    txtSearch.Focus();
                    dataGridView1.DataSource = models;
                }
                    
            }
            else
            {
                MessageBox.Show("Vui lòng nhập Model cần tìm!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSearch.Focus();
            }
        }

        private void checkWidthLabelMurata_CheckedChanged(object sender, EventArgs e)
        {
            if (chkHP.Checked == true)
            {
                txtQuantityHP.Enabled = true;
                txtQuantityHP.Focus();
            }
            else
            {
                txtQuantityHP.Enabled = false;
                txtModelName.Focus();
            }
        }

        private void MenuItemCopyCell_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string modelName = Convert.ToString(selectedRow.Cells["colModelName"].Value);
                Clipboard.SetText(modelName);
            }
        }

        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa bản mạch này không?", "Xóa bản ghi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];

                    string modelName = Convert.ToString(selectedRow.Cells["colModelName"].Value);
                    try
                    {
                        var model = models_BUS.GetModelByName(modelName);
                        if(model != null)
                            models_BUS.Delete(model.ModelID);
                        LoadData();

                        MessageBox.Show("Xóa thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi!\n{ex.Message}", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MenuItemEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string modelName = Convert.ToString(selectedRow.Cells["colModelName"].Value);

                var model = models_BUS.GetModelByName(modelName);
                if (model != null)
                {
                    txtQuantity.Value = Convert.ToDecimal(model.Quantity);
                    txtModelName.Text = model.ModelName;
                    lblID.Text = model.ModelID;
                    chkHP.Checked = (model.FujiHP == null || model.FujiHP == false) ? false : true;
                    if (model.FujiHP == true)
                    {
                        txtQuantityHP.Value = Convert.ToInt32(model.QuantityHP);
                    }
                }
                
            }
        }

        private void MenuItemRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
