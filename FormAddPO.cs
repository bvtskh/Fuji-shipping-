using System;
using System.Windows.Forms;
using UMC.OQC.Models;
using UMC.OQC.Shipping;

namespace FujiXeroxShippingSystem
{
    public partial class FormAddPO : Form
    {
        ShippingPO shipping = null;
        ShippingDAL shippingDAL = new ShippingDAL();
        Models_BUS models_BUS = new Models_BUS();
        public FormAddPO(ShippingPO shippingPO)
        {
            InitializeComponent();
            shipping = shippingPO;

            txtPO_NO.Text = shipping.PO_NO;
            //txtModel.Text = models_BUS.Get(shipping.ModelID).ModelName;
            txtModel.Text = shippingPO.ModelID;
            txtQuantity.Value = shipping.QuantityPO;
            txtRemain.Value = shipping.QuantityRemain >= 0 ? shipping.QuantityRemain : 0;
            lblModelID.Text = shipping.ModelID;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtQuantity.Value == 0)
            {
                MessageBox.Show("Vui lòng nhập vào số lượng cho PO!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtQuantity.Focus();
                errorProvider1.SetError(txtQuantity, "Vui lòng nhập vào số lượng cho PO! ");
                return;
            }
            shipping = shippingDAL.Get(shipping.ModelID, shipping.PO_NO);
            if (shipping != null)
            {
                shipping.QuantityPO = shipping.QuantityPO + Convert.ToInt32(txtQuantity.Value);
                shipping.QuantityRemain = Convert.ToInt32(txtQuantity.Value);
                shippingDAL.Update(shipping);
                DialogResult dialogResult = MessageBox.Show("Cập nhật số lượng cho PO thành công. Bạn có đóng hay không?",
                    @"THÔNG BÁO",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                shipping = new ShippingPO()
                {
                    ModelID = lblModelID.Text,
                    PO_NO = txtPO_NO.Text.Trim(),
                    QuantityPO = Convert.ToInt32(txtQuantity.Value),
                    QuantityRemain = Convert.ToInt32(txtQuantity.Value)
                }; ;

                shippingDAL.Add(shipping);
                DialogResult dialogResult = MessageBox.Show("Nhập mới PO thành công. Bạn có đóng hay không?", @"THÔNG BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            txtQuantity.Value = shipping.QuantityPO;
            txtRemain.Value = shipping.QuantityRemain >= 0 ? shipping.QuantityRemain : 0;
        }

        private void txtQuantity_ValueChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        private void FormAddPO_Activated(object sender, EventArgs e)
        {
            txtQuantity.Focus();
        }

        private void txtQuantity_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtQuantity.Value == 0)
                {
                    MessageBox.Show("Vui lòng nhập vào số lượng cho PO!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtQuantity.Focus();
                    errorProvider1.SetError(txtQuantity, "Vui lòng nhập vào số lượng cho PO! ");
                    return;
                }
                shipping = shippingDAL.Get(shipping.ModelID, shipping.PO_NO);
                if (shipping != null)
                {
                    shipping.QuantityPO = shipping.QuantityPO + Convert.ToInt32(txtQuantity.Value);
                    shipping.QuantityRemain = Convert.ToInt32(txtQuantity.Value);
                    shippingDAL.Update(shipping);
                    DialogResult dialogResult = MessageBox.Show("Cập nhật số lượng cho PO thành công. Bạn có đóng hay không?",
                    @"THÔNG BÁO",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
                else
                {
                    shipping = new ShippingPO()
                    {
                        ModelID = lblModelID.Text,
                        PO_NO = txtPO_NO.Text.Trim(),
                        QuantityPO = Convert.ToInt32(txtQuantity.Value),
                        QuantityRemain = Convert.ToInt32(txtQuantity.Value)
                    }; ;

                    shippingDAL.Add(shipping);
                    DialogResult dialogResult = MessageBox.Show("Nhập mới PO thành công. Bạn có đóng hay không?",
                    @"THÔNG BÁO",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
                txtQuantity.Value = shipping.QuantityPO;
                txtRemain.Value = shipping.QuantityRemain >= 0 ? shipping.QuantityRemain : 0;
            }
        }

        private void FormAddPO_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (txtQuantity.Value == 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Bạn chưa nhập vào 'Số lượng'. Bạn có thực sự muốn đóng hay không?",
                    @"THÔNG BÁO",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Application.ExitThread();
                    }
                }

            }
        }
    }
}
