using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FujiXeroxShippingSystem.Business;
using UMC.OQC.FujiXerox;
using UMC.OQC.Shipping;

namespace FujiXeroxShippingSystem
{

    public partial class FormMain : Form
    {
        ShippingDAL shippingDAL = null;
        ShippingPO shipping = null;
        public void ResetUI()
        {
            txtModel.ResetText();
            txtBoxID.ResetText();
            txtWorkingOrder.ResetText();
            txtSerial.ResetText();
            txtPoNO.ResetText();
            lblQuantity.Text = string.Format("0/0");
            lblRule.Text = "N/A";
            lblCurrentItem.Text = "N/A";
            dataGridView1.DataSource = null;
            DisplayMessage("N/A", "no results");
            txtWorkingOrder.Enabled = false;
            txtBoxID.Enabled = false;
            txtSerial.Enabled = false;
            txtQrCode.ResetText();
            txtQrCode.Enabled = true;
            txtQrCode.Focus();
        }
        public FormMain()
        {
            InitializeComponent();
            shippingDAL = new ShippingDAL();
            lblCurrentUser.Text = Program.CurrentUser.NAME;
            dataGridView1.AutoGenerateColumns = false;
            lblRunVersion.Text = Ultils.GetRunningVersion();
        }


        void DisplayMessage(string status, string message)
        {
            Color backColor = new Color();
            Color foreColor = new Color();

            switch (status)
            {
                case "OK":
                    backColor = Color.DarkGreen;
                    foreColor = Color.White;
                    break;
                case "NG":
                    backColor = Color.DarkRed;
                    foreColor = Color.White;
                    break;
                case "WARNING":
                    backColor = Color.DarkOrange;
                    foreColor = Color.White;
                    break;
                default:
                    backColor = Color.White;
                    foreColor = Color.FromArgb(192, 64, 0);
                    break;
            }
            this.BeginInvoke(new Action(() => { lblStatus.Text = status; }));
            this.BeginInvoke(new Action(() => { lblStatus.BackColor = backColor; }));
            this.BeginInvoke(new Action(() => { lblStatus.ForeColor = foreColor; }));

            this.BeginInvoke(new Action(() => { lblMessge.Text = message; }));
            this.BeginInvoke(new Action(() => { lblMessge.BackColor = backColor; }));
            this.BeginInvoke(new Action(() => { lblMessge.ForeColor = foreColor; }));
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult dialogResult = MessageBox.Show("Bạn có thực sự muốn đóng hay không!", @"THÔNG BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Application.ExitThread();
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetUI();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearchKey.Visible == false)
            {
                txtSearchKey.Visible = true;
                lblSearchKey.Visible = true;
                btnClear.Visible = true;
                lblNote.Visible = true;
                txtSearchKey.Focus();
            }
            else
            {
                Search(txtSearchKey.Text);
            }
        }

        void Search(string boxid)
        {
            if (boxid != "")
            {
                var data = SingletonHelper.ERPInstance.OQCTestLogFindByBoxID(boxid);
                dataGridView1.DataSource = data;
                if (data.Count == 0)
                {
                    DisplayMessage("NG", $"Không tìm thấy dữ liệu của bản mạch '{boxid}'!");
                }
                txtSearchKey.ResetText();
                txtSearchKey.Focus();
            }
            else
            {
                DisplayMessage("NG", "Vui lòng nhập vào mã thùng cần tìm kiếm!");
                txtSearchKey.Focus();
            }

        }

        private void cboValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearchKey.SelectAll();
            txtSearchKey.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSearchKey.Visible = false;
            lblSearchKey.Visible = false;
            btnClear.Visible = false;
            lblNote.Visible = false;
            dataGridView1.DataSource = null;
            dataGridView1.Refresh();
        }

        private void txtSearchKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search(txtSearchKey.Text);
            }
        }

        private void MenuItemAllBox_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string boxId = Convert.ToString(selectedRow.Cells["Column3"].Value);

                if (MessageBox.Show($"Bạn có chắc muốn xóa tất bản mạch trong thùng [{boxId}] này không?", "Xóa bản ghi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        var data = SingletonHelper.ERPInstance.OQCTestLogFindByBoxID(boxId);
                        if (data.Count > 0)
                        {
                            SingletonHelper.ERPInstance.ShippingRevert(boxId);
                            string modelId = data[0].ModelNO;
                            string poNo = data[0].PO_NO;
                            shipping = shippingDAL.Get(modelId, poNo);
                            //Cập nhật lại số lượng Remain
                            shipping.QuantityRemain = shipping.QuantityRemain + data.Count;
                            shippingDAL.Update(shipping);

                            //foreach (var item in data)
                            //{
                            //    item.Shipping = false;
                            //    item.Date_Shipping = null;
                            //    SingletonHelper.ERPInstance.OQCTestLogSave(item.ProductionID, item);
                            //}
                            MessageBox.Show($"Xóa tất cả bản mạch trong thùng [{boxId}] thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Không tìm thấy dữ liệu xuất hàng của BoxID [{boxId}]!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi!\n{ex.Message}", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MenuItemCopyCell_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string boardNo = Convert.ToString(selectedRow.Cells["Column1"].Value);
                Clipboard.SetText(boardNo);
            }
        }

        private void cboHP_CheckedChanged(object sender, EventArgs e)
        {
            //if (Program.CurrentModels != null)
            //{
            //    if (cboHP.Checked == true)
            //    {
            //        currentQuantity = Convert.ToInt32(Program.CurrentModels.QuantityHP);
            //        if (currentQuantity == 0)
            //        {
            //            currentQuantity = Convert.ToInt32(Program.CurrentModels.Quantity);
            //            DisplayMessage("WARNING", $"Model này chưa được thiết lập để xuất đi Hải phòng. Vui lòng kiểm tra lại!");
            //        }
            //        lblQuantity.Text = $"0/{currentQuantity}";
            //    }
            //    else
            //    {
            //        currentQuantity = Convert.ToInt32(Program.CurrentModels.Quantity);
            //        lblQuantity.Text = $"0/{currentQuantity}";
            //        DisplayMessage("N/A", "no results");
            //    }
            //}
            //txtPoNO.ResetText();
            //txtPoNO.Focus();
        }

        private void txtWorkingOrder_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string label = txtWorkingOrder.Text.Trim();
                if (string.IsNullOrEmpty(label))
                {
                    DisplayMessage("NG", $"Vui lòng bắn vào Label!");
                    txtWorkingOrder.Focus();
                    return;
                }
                if (!label.StartsWith("3N4"))
                {
                    DisplayMessage("NG", $"Label [{label}] không đúng định dạng!");
                    txtWorkingOrder.ResetText();
                    txtWorkingOrder.Focus();
                    return;
                }
                else
                {
                    label = label.Replace("3N4", "");
                    if (label.Contains(" "))
                        label = label.Replace(" ", "");
                    if (label.Length > QrcodeHelper.ProductID.Length)
                    {
                        label = label.Substring(0, QrcodeHelper.ProductID.Length);
                    }
                    if (!label.Contains(QrcodeHelper.ProductID))
                    {
                        DisplayMessage("NG", $"Model bạn chọn [{QrcodeHelper.ProductID}] không giống với Label [{label}] được gắn trên thùng!");
                        txtWorkingOrder.ResetText();
                        txtWorkingOrder.Focus();
                        return;
                    }
                    txtBoxID.Enabled = true;
                    txtBoxID.ResetText();
                    txtBoxID.Focus();
                }
            }
        }

        private void txtPoNO_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (string.IsNullOrEmpty(txtPoNO.Text))
                {
                    DisplayMessage("NG", $"Vui lòng bắn vào PO NO!");
                    txtPoNO.Focus();
                    return;
                }
                if (!txtPoNO.Text.StartsWith("3N3"))
                {
                    DisplayMessage("NG", $"PO NO [{txtPoNO.Text}] không đúng định dạng!");
                    txtPoNO.ResetText();
                    txtPoNO.Focus();
                    return;
                }
                string poNo = txtPoNO.Text.Trim();
                //shipping = shippingDAL.Get(Program.CurrentModels.ModelName, poNo);
                shipping = shippingDAL.Get(QrcodeHelper.ProductID, poNo);
                if (shipping != null)
                {
                    lblQuantityPO.Text = shipping.QuantityPO.ToString();
                    lblRemain.Text = shipping.QuantityRemain.ToString();
                    if (shipping.QuantityRemain <= 0)
                    {
                        DisplayMessage("NG", $"PO NO [{shipping.PO_NO}] đã được bắn xong!");
                    }
                }
                else
                {
                    shipping = new ShippingPO()
                    {
                        ModelID = QrcodeHelper.ProductID,
                        PO_NO = poNo,
                        QuantityPO = QrcodeHelper.TotalQty,
                        QuantityRemain = QrcodeHelper.TotalQty
                    };
                    shippingDAL.Add(shipping);
                    // new FormAddPO(shipping).ShowDialog();
                    shipping = shippingDAL.Get(QrcodeHelper.ProductID, poNo);
                    if (shipping != null)
                    {
                        lblQuantityPO.Text = shipping.QuantityPO.ToString();
                        lblRemain.Text = shipping.QuantityRemain.ToString();
                    }
                }
                if (Program.CurrentModels != null)
                {
                    if (Program.CurrentModels.CheckLabelOnBox == true)
                    {
                        txtBoxID.ResetText();
                        txtBoxID.Focus();
                    }
                    else
                    {
                        txtWorkingOrder.ResetText();
                        txtWorkingOrder.Focus();
                    }
                }

            }
        }

        List<tbl_test_log> test_Logs = new List<tbl_test_log>();
        private void txtBoxID_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (chkManual.Checked)
                {
                    QrcodeHelper.ManualGetBoxID(txtBoxID.Text);
                }
                if (chkXuatLe.Checked == false)
                {
                    string boxID = txtBoxID.Text.Trim();
                    string modelID = txtModel.Text.Trim();
                    foreach (var msg in Validator.CheckData(boxID, modelID))
                    {
                        txtBoxID.ResetText();
                        txtBoxID.Focus();
                        DisplayMessage("NG", msg);
                        return;
                    }

                    //dateServer = SingletonHelper.PVSInstance.GetDateTime();
                    //foreach (var item in Validator.Data)
                    //{
                    //    item.Shipping = true;
                    //    item.Date_Shipping = dateServer;
                    //    item.PO_NO = txtPoNO.Text.Trim();
                    //    item.WorkingOrder = txtWorkingOrder.Text;
                    //    item.Delivery_No = QrcodeHelper.DeliveryNo;
                    //    SingletonHelper.ERPInstance.OQCTestLogSave(item.ProductionID, item);
                    //}

                    var entity = Validator.Data.FirstOrDefault();
                    entity.Shipping = true;
                    entity.Date_Shipping = SingletonHelper.PVSInstance.GetDateTime();
                    entity.PO_NO = txtPoNO.Text.Trim();
                    //entity.WorkingOrder = txtWorkingOrder.Text;
                    entity.MacAddress = txtWorkingOrder.Text.Trim();
                    entity.Delivery_No = QrcodeHelper.DeliveryNo;
                    var update = SingletonHelper.ERPInstance.OQCTestLogUpdate(entity.BoxID, entity);
                    int count = Validator.Data.Count;

                    // Cập nhật lại số lượng Remain
                    shipping = shippingDAL.Get(txtModel.Text.TrimEnd(), txtPoNO.Text.Trim());
                    shipping.QuantityRemain = shipping.QuantityRemain - count;
                    shippingDAL.Update(shipping);

                    shipping = shippingDAL.Get(shipping.ModelID, shipping.PO_NO);
                    lblQuantityPO.Text = shipping.QuantityPO.ToString();
                    lblRemain.Text = shipping.QuantityRemain.ToString();

                    lblQuantity.Text = $"{count}/{QrcodeHelper.Qty}/{QrcodeHelper.TotalQty}";
                    dataGridView1.DataSource = Validator.Data;
                    // Nếu số lượng chưa đủ
                    if (count > 0 && count < QrcodeHelper.Qty)
                    {
                        DisplayMessage("WARNING", $"Số lượng trong Box [{boxID}] bị thiếu {QrcodeHelper.Qty - count} pcb. Vui lòng kiểm tra và bắn tiếp số pcb còn thiếu!");
                        txtSerial.Enabled = true;
                        txtSerial.Focus();
                    }
                    // Nếu số lượng để thì thực hiện cập nhật dữ liệu xuất hàng
                    if (count == QrcodeHelper.Qty)
                    {
                        DisplayMessage("OK", $"Box [{boxID}] OK!\n{count} pcb.");
                        txtBoxID.ResetText();
                        txtWorkingOrder.ResetText();
                        txtWorkingOrder.Focus();
                    }
                    if (shipping.QuantityRemain <= 0)
                    {
                        DisplayMessage("OK", $"PO NO [{shipping.PO_NO}] đã được bắn xong. Vui lòng bắn vào PO mới!");
                        ResetUI();
                    }

                }
                else
                {
                    DisplayMessage("WARNING", $"Bạn chọn xuất lẻ. Vui lòng bắn từng bản mạch!");
                    txtSerial.ResetText();
                    txtSerial.Enabled = true;
                    txtSerial.Focus();
                }
            }
        }
        private void txtSerial_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string boxId = txtBoxID.Text.Trim();
                var count = SingletonHelper.ERPInstance.OQCTestLogFindByBoxID(boxId).Where(r => r.Shipping == true).Count();
                lblQuantity.Text = $"{count}/{QrcodeHelper.Qty}/{QrcodeHelper.TotalQty}";
                if (count >= QrcodeHelper.Qty)
                {
                    txtSerial.Enabled = false;
                    txtSerial.ResetText();
                    DisplayMessage("WARNING", $"Box [{boxId}] đã đầy. Vui lòng bắn vào box khác!");
                    txtBoxID.ResetText();
                    txtBoxID.Focus();
                    return;
                }
                var boardNo = txtSerial.Text.Trim();
                var productID = chkManual.Checked ? txtModel.Text : QrcodeHelper.ProductID;
                var ruleItem = SingletonHelper.PVSInstance.GetBarodeRuleItemsByRuleNo(productID);
                if (boardNo.Length == ruleItem.LENGTH)
                {
                    string content = boardNo.Substring(ruleItem.LOCATION.Value - 1, ruleItem.CONTENT_LENGTH);
                    if (content == ruleItem.CONTENT)
                    {
                        var item = SingletonHelper.ERPInstance.OQCTestLogFind(boardNo);
                        if (item == null)
                        {
                            txtSerial.ResetText();
                            txtSerial.Focus();
                            DisplayMessage("NG", $"Không tìm thấy dữ liệu của bản mạch [{boardNo}]. Vui lòng kiểm tra và bắn thử lại!");
                            return;
                        }
                        if (item.Shipping == true && item.Date_Shipping != null)
                        {
                            txtSerial.ResetText();
                            txtSerial.Focus();
                            DisplayMessage("NG", $"Bản mạch [{boardNo}] đã được xuất hàng ngày: {item.Date_Shipping.Value.ToShortDateString()} trong box [{item.BoxID}]. Vui lòng kiểm tra lại!");
                            return;
                        }
                        if (item.Judge == "NG")
                        {
                            txtSerial.ResetText();
                            txtSerial.Focus();
                            DisplayMessage("NG", $"Bản mạch [{boardNo}] bị 'NG' không được xuất hàng. Vui lòng kiểm tra lại!");
                            return;
                        }
                        else
                        {
                            // Thông tin xuất hàng
                            item.OldBoxID = item.BoxID;
                            item.Shipping = true;
                            item.Date_Shipping = SingletonHelper.PVSInstance.GetDateTime();
                            //item.WorkingOrder = txtWorkingOrder.Text.Trim();
                            item.MacAddress = txtWorkingOrder.Text.Trim();
                            item.PO_NO = txtPoNO.Text.Trim();
                            item.BoxID = boxId;
                            // Cập nhật Box mới
                            //testLog.Update(item, boxId);

                            // test_Logs.Add(item);
                            SingletonHelper.ERPInstance.OQCTestLogSave(item.ProductionID, item);
                            if (chkManual.Checked)
                            {
                                shipping = shipping = shippingDAL.Get(productID, txtPoNO.Text);
                            }
                            // Cập nhật lại số lượng Remain
                            shipping.QuantityRemain = shipping.QuantityRemain - 1;
                            shippingDAL.Update(shipping);
                            shipping = shippingDAL.Get(shipping.ModelID, shipping.PO_NO);
                            lblQuantityPO.Text = shipping.QuantityPO.ToString();
                            lblRemain.Text = shipping.QuantityRemain.ToString();

                            // Đếm lại nếu lượng trong box
                            //var data = testLog.GetAllShipping(boxId, true);
                            var data = SingletonHelper.ERPInstance.OQCTestLogGetByBoxID(boxId);
                            lblQuantity.Text = $"{data.Count}/{QrcodeHelper.Qty}/{QrcodeHelper.TotalQty}";
                            dataGridView1.DataSource = data;
                            // Nếu đầy thì thôi, bắn vào thùng khác
                            if (data.Count >= QrcodeHelper.Qty)
                            {
                                txtSerial.Enabled = false;
                                txtSerial.ResetText();
                                //chkXuatLe.Checked = false;
                                txtWorkingOrder.ResetText();
                                txtBoxID.ResetText();
                                txtWorkingOrder.Focus();
                                DisplayMessage("OK", $"Box [{boxId}] đã đầy. Vui lòng bắn vào box khác!");
                            }
                            else
                            {
                                txtSerial.ResetText();
                                txtSerial.Focus();
                                //chkXuatLe.Checked = false;
                                DisplayMessage("OK", $"Board [{boardNo}] OK!");
                            }
                        }
                    }
                    else
                    {
                        txtSerial.ResetText();
                        txtSerial.Focus();
                        DisplayMessage("NG", "Sai Model. Vui lòng kiểm tra và bắn lại!");
                        return;
                    }
                }
                else
                {
                    txtSerial.ResetText();
                    txtSerial.Focus();
                    DisplayMessage("NG", "Barcode không đúng định dạng. Vui lòng kiểm tra và bắn lại!");
                    return;
                }
            }
        }

        private void txtSerial_TextChanged(object sender, EventArgs e)
        {
            if (Ultils.IsNullOrEmpty(txtSerial))
            {
                DisplayMessage("N/A", "no results");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPoNO.Text))
            {
                DisplayMessage("NG", $"Vui lòng bắn vào PO NO!");
                txtPoNO.Focus();
                return;
            }
            if (!txtPoNO.Text.StartsWith("3N3"))
            {
                DisplayMessage("NG", $"PO NO [{txtPoNO.Text}] không đúng định dạng!");
                txtPoNO.ResetText();
                txtPoNO.Focus();
                return;
            }
            string poNo = txtPoNO.Text.Trim();
            ShippingPO poItem = shippingDAL.Get(Program.CurrentModels.ModelID, poNo);
            if (poItem == null)
            {
                poItem.PO_NO = poNo;
                poItem.ModelID = Program.CurrentModels.ModelName;
            }
            new FormAddPO(poItem).ShowDialog();
            // Display();
        }


        //void LoadModel()
        //{
        //    string model = txtModel.Text;
        //    label1.Text = model;
        //    if (model.Length >= 7)
        //    {
        //        //Program.CurrentRule = BARCODE_RULES_BUS.Get(model);
        //        rule_items = _RULE_ITEMS_BUS.Get(model);
        //        if (rule_items != null)
        //            lblCurrentItem.Text = rule_items.CONTENT;
        //        if (model != null)
        //        {
        //            Program.CurrentModels = models_BUS.GetModelByName(model);
        //            lblCurrentModel.Text = model;
        //            if (Program.CurrentModels != null)
        //            {
        //                bool fujihp = ((Program.CurrentModels.FujiHP == false) || (Program.CurrentModels.FujiHP == null)) ? false : true;
        //                bool checkLabelOnBox = ((Program.CurrentModels.CheckLabelOnBox == false) || (Program.CurrentModels.CheckLabelOnBox == null)) ? true : false;
        //                if (fujihp == true)
        //                {
        //                    currentQuantity = Convert.ToInt32(Program.CurrentModels.QuantityHP);
        //                }
        //                else
        //                {
        //                    currentQuantity = Convert.ToInt32(Program.CurrentModels.Quantity);
        //                }
        //               // cboHP.Checked = fujihp;
        //                txtWorkingOrder.Enabled = checkLabelOnBox;
        //                lblQuantity.Text = $"0/{currentQuantity}";
        //            }
        //            else
        //            {
        //                DisplayMessage("NG", $"Model [{model}] chưa được thiết lập số lượng. Vui lòng thiết lập số lượng, sau đó thử lại!");
        //            }
        //        }
        //        txtPoNO.ResetText();
        //        txtPoNO.Focus();
        //    }
        //    else
        //    {
        //        DisplayMessage("NG", $"Hệ thống không tìm thấy Model [{model}]");
        //        txtModel.SelectAll();
        //        txtModel.Focus();
        //        return;
        //    }
        //}

        //private void txtModel_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (txtModel.SelectedIndex > -1)
        //    {
        //        if (lblStatus.Text == "NG")
        //            DisplayMessage("N/A", "no results");
        //        LoadModel();
        //    }
        //}

        private void chkXuatLe_CheckedChanged(object sender, EventArgs e)
        {
            if (chkXuatLe.Checked == false)
                txtSerial.Enabled = false;

            if (string.IsNullOrEmpty(txtPoNO.Text.Trim()))
            {
                txtPoNO.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtWorkingOrder.Text.Trim()))
            {
                //if (Program.CurrentModels.CheckLabelOnBox == false || Program.CurrentModels.CheckLabelOnBox == null)
                //{
                //    txtWorkingOrder.Focus();
                //    return;
                //}
            }
            if (string.IsNullOrEmpty(txtBoxID.Text))
            {
                txtBoxID.ResetText();
                txtBoxID.Focus();
            }
        }

        private void txtQrCode_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    var data = txtQrCode.Text.Replace("*", "").Trim();
                    if (string.IsNullOrEmpty(data))
                    {
                        MessageBox.Show("Empty");
                        return;
                    }
                    QrcodeHelper.GetQrCode(data);
                    txtQrCode.ResetText();
                    shipping = shippingDAL.Get(QrcodeHelper.ProductID, QrcodeHelper.PoNo);
                    if (shipping != null)
                    {
                        lblQuantityPO.Text = shipping.QuantityPO.ToString();
                        lblRemain.Text = shipping.QuantityRemain.ToString();
                        if (shipping.QuantityRemain <= 0)
                        {
                            DisplayMessage("NG", $"PO NO [{shipping.PO_NO}] đã được bắn xong!");
                            txtQrCode.Focus();
                            return;
                        }
                    }
                    else
                    {
                        shipping = new ShippingPO()
                        {
                            ModelID = QrcodeHelper.ProductID,
                            PO_NO = QrcodeHelper.PoNo,
                            QuantityPO = QrcodeHelper.TotalQty,
                            QuantityRemain = QrcodeHelper.TotalQty
                        };
                        shippingDAL.Add(shipping);
                        shipping = shippingDAL.Get(QrcodeHelper.ProductID, QrcodeHelper.PoNo);
                        if (shipping == null)
                        {
                            DisplayMessage("NG", $"Lưu PO [{shipping.PO_NO}] không thành công!");
                            return;
                        }
                    }

                    txtModel.Text = QrcodeHelper.ProductID;
                    lblQuantity.Text = $"0/{QrcodeHelper.Qty}/{QrcodeHelper.TotalQty}";
                    txtPoNO.Text = QrcodeHelper.PoNo;
                    lblQuantityPO.Text = shipping.QuantityPO.ToString();
                    lblRemain.Text = shipping.QuantityRemain.ToString();
                    this.lblModel.Text = QrcodeHelper.ProductID;
                    var ruleItem = SingletonHelper.PVSInstance.GetBarodeRuleItemsByRuleNo(QrcodeHelper.ProductID);
                    if (ruleItem != null)
                    {
                        this.lblRule.Text = ruleItem.RULE_NO;
                        this.lblCurrentItem.Text = ruleItem.CONTENT;
                    }
                    txtQrCode.Enabled = false;
                    txtWorkingOrder.Enabled = true;
                    txtWorkingOrder.ResetText();
                    txtWorkingOrder.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void chkManual_Click(object sender, EventArgs e)
        {
            ResetUI();
            if (chkManual.Checked)
            {
                txtQrCode.Enabled = false;
                txtModel.Enabled = true;
                txtModel.Focus();
            }
            else
            {
                txtQrCode.Enabled = true;
                txtModel.Enabled = false;
                txtQrCode.Focus();
            }
        }

        private void txtModel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var modelNo = txtModel.Text.Trim();
                if (string.IsNullOrEmpty(modelNo))
                {
                    return;
                }
                txtPoNO.Enabled = true;
                txtPoNO.ResetText();
                txtPoNO.Focus();
            }
        }

        private void txtPoNO_PreviewKeyDown_1(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var poNo = txtPoNO.Text;
                if (string.IsNullOrEmpty(poNo))
                {
                    return;
                }
                var entity = shippingDAL.Get(txtModel.Text, poNo);
                if (entity == null)
                {
                    new FormAddPO(new ShippingPO()
                    {
                        PO_NO = poNo,
                        ModelID = txtModel.Text
                    }).ShowDialog();
                }
                entity = shippingDAL.Get(txtModel.Text, poNo);
                if (entity != null)
                {
                    lblQuantityPO.Text = entity.QuantityPO.ToString();
                    lblRemain.Text = entity.QuantityRemain.ToString();
                    txtBoxID.Enabled = true;
                    txtBoxID.Focus();
                }
            }
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            try
            {
                var data = txtQrCode.Text.Replace("*", "").Trim();
                if (string.IsNullOrEmpty(data))
                {
                    return;
                }
                QrcodeHelper.GetQrCode(data);
                txtQrCode.ResetText();
                shipping = shippingDAL.Get(QrcodeHelper.ProductID, QrcodeHelper.PoNo);
                if (shipping != null)
                {
                    lblQuantityPO.Text = shipping.QuantityPO.ToString();
                    lblRemain.Text = shipping.QuantityRemain.ToString();
                    if (shipping.QuantityRemain <= 0)
                    {
                        DisplayMessage("NG", $"PO NO [{shipping.PO_NO}] đã được bắn xong!");
                        txtQrCode.Focus();
                        return;
                    }
                }
                else
                {
                    shipping = new ShippingPO()
                    {
                        ModelID = QrcodeHelper.ProductID,
                        PO_NO = QrcodeHelper.PoNo,
                        QuantityPO = QrcodeHelper.TotalQty,
                        QuantityRemain = QrcodeHelper.TotalQty
                    };
                    shippingDAL.Add(shipping);
                    shipping = shippingDAL.Get(QrcodeHelper.ProductID, QrcodeHelper.PoNo);
                    if (shipping == null)
                    {
                        DisplayMessage("NG", $"Lưu PO [{shipping.PO_NO}] không thành công!");
                        return;
                    }
                }

                txtModel.Text = QrcodeHelper.ProductID;
                lblQuantity.Text = $"0/{QrcodeHelper.Qty}/{QrcodeHelper.TotalQty}";
                txtPoNO.Text = QrcodeHelper.PoNo;
                lblQuantityPO.Text = shipping.QuantityPO.ToString();
                lblRemain.Text = shipping.QuantityRemain.ToString();
                this.lblModel.Text = QrcodeHelper.ProductID;
                var ruleItem = SingletonHelper.PVSInstance.GetBarodeRuleItemsByRuleNo(QrcodeHelper.ProductID);
                if (ruleItem != null)
                {
                    this.lblRule.Text = ruleItem.RULE_NO;
                    this.lblCurrentItem.Text = ruleItem.CONTENT;
                }
                txtQrCode.Enabled = false;
                txtWorkingOrder.Enabled = true;
                txtWorkingOrder.ResetText();
                txtWorkingOrder.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }
    }
}
