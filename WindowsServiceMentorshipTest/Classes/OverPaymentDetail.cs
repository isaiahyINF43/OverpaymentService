using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceMentorshipTest.Classes
{
    public class OverPaymentDetail
    {
        public int OverPaymentID { get; set; }
        public int MemberID { get; set; }
        public decimal OverPaymentAmt { get; set; }
        public string ClaimNumber { get; set; }
        public decimal BalanceAmt { get; set; }
        public string CreateDate { get; set; }
        public string SysSrcSyncDate { get; set; }
        public string LastUpdated { get; set; }
    }
}