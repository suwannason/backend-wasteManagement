
namespace backend.response {
     public class DeptResponse {
         public bool success { get; set; }
         public deptItem[] data { get; set; }

     }
     public class deptItem {
         public string DEPT_CODE { get; set; }
         public string DEPT_NAME { get; set; }
         public string DEPT_ABB_NAME { get; set; }
         public string DIV_NAME_WC { get; set; }
     }
}