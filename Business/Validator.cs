using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMC.OQC.FujiXerox;

namespace FujiXeroxShippingSystem.Business
{
    public class Validator
    {
        private static List<ERPService.tbl_test_logEntity> _Data;
        public static List<ERPService.tbl_test_logEntity> Data
        {
            get
            {
                return _Data;
            }
        }
        public static IEnumerable<string> CheckData(string boxID, string modelID)
        {
            if (boxID.Length < 10 || !boxID.StartsWith("F0"))
            {
                yield return $"Vui lòng nhập vào mã Box đúng định dạng!";
            }
            //_Data = new tbl_test_log_bus().GetAll(boxID, false);
            _Data = SingletonHelper.ERPInstance.OQCTestLogGetList(boxID).Where(r => r.Shipping == false || r.Shipping == null).ToList();
            if (_Data == null || _Data.Count == 0)
            {
                yield return $"Không tìm thấy dữ liệu trong Box [{boxID}]!";
            }
            var ruleItem = SingletonHelper.PVSInstance.GetBarodeRuleItemsByRuleNo(modelID);
            if (ruleItem == null)
            {
                yield return $"Chưa tạo rule [{modelID}]!";
            }

            foreach (var item in _Data)
            {
                if (item.ProductionID.Length != ruleItem.LENGTH)
                {
                    yield return $"Barcode {item.ProductionID} NG !\nKhông đúng với rule";
                }
                string content = item.ProductionID.Substring((int)ruleItem.LOCATION - 1, ruleItem.CONTENT_LENGTH);
                if (!content.Equals(ruleItem.CONTENT, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"Barcode {item.ProductionID} NG !\nKhông đúng với rule";
                }
            }
            yield break;
        }
    }
}
