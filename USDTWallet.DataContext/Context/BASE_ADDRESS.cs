//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace USDTWallet.DataContext.Context
{
    using System;
    using System.Collections.Generic;
    
    public partial class BASE_ADDRESS
    {
        public string ID { get; set; }
        public string ADDRESS { get; set; }
        public string EXTPUBKEY_WIF { get; set; }
        public string WALLET_ID { get; set; }
        public long NETWORK { get; set; }
        public string KEY_PATH { get; set; }
        public string PARENT_KEY_PATH { get; set; }
        public long PATH_INDEX { get; set; }
        public long ADDRESS_TYPE { get; set; }
        public Nullable<long> CUSTOMER_ID { get; set; }
        public long ADDRESS_CATEGORY { get; set; }
        public string NAME { get; set; }
        public double BALANCE { get; set; }
        public string ACCOUNT { get; set; }
        public string DESCRIPTION { get; set; }
        public System.DateTime CREATE_DATE { get; set; }
    }
}
