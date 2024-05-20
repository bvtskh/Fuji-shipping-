using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FujiXeroxShippingSystem.Business
{
    public class QrcodeHelper
    {
        private static string _ProductID;
        private static string _Po_No;
        private static int _Qty;
        private static int _Total_Qty;
        //private static PVSService.BARCODE_RULE_ITEMSEntity _Rule_Item;
        private static string _Delivery_No;
        public static string ProductID
        {
            get
            {
                return _ProductID;
            }
        }
        public static string PoNo
        {
            get
            {
                return $"3N3{_Po_No} {_Total_Qty} ";
            }
        }
        public static int Qty
        {
            get
            {
                return _Qty;
            }
        }
        public static int TotalQty
        {
            get
            {
                return _Total_Qty;
            }
        }
        //public static PVSService.BARCODE_RULE_ITEMSEntity RuleItem
        //{
        //    get
        //    {
        //        return _Rule_Item;
        //    }
        //}
        public static string DeliveryNo
        {
            get
            {
                return _Delivery_No;
            }
        }
        public static void GetQrCode(string data)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            //RegexOptions options = RegexOptions.None;
            //Regex regex = new Regex("[ ]{2,}", options);
            //data = regex.Replace(data, " ");
            string[] lines = data.Replace("*", "").Trim().Split(stringSeparators, StringSplitOptions.None).Select(r=>r.Trim()).Where(r=>!string.IsNullOrEmpty(r)).ToArray();
            _ProductID = lines[2].Replace(" ", "");
            _Qty = Convert.ToInt32(lines[4]);
            _Total_Qty = Convert.ToInt32(lines[5]);
            _Po_No = lines[1];
            _Delivery_No = lines[1];
           // _Rule_Item = SingletonHelper.PVSInstance.GetBarodeRuleItemsByRuleNo(_ProductID);
        }
        public static void ManualGetModel(string modelNo)
        {
            _ProductID = modelNo;
        }
        public static void ManualGetPO(string PoNo)
        {
            _Po_No = PoNo;
        }
        public static void ManualGetBoxID(string boxID)
        {
            var data = SingletonHelper.ERPInstance.OQCTestLogGetList(boxID).Where(r => r.Shipping == false || r.Shipping == null).ToList();
            _Qty = data.Count;
        }
    }
}
