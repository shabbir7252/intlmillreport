//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImillReports.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GL_Ledger
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GL_Ledger()
        {
            this.ICS_Transaction = new HashSet<ICS_Transaction>();
            this.ICS_Transaction1 = new HashSet<ICS_Transaction>();
            this.ICS_Transaction2 = new HashSet<ICS_Transaction>();
            this.ICS_Transaction_Types = new HashSet<ICS_Transaction_Types>();
            this.SM_Location = new HashSet<SM_Location>();
            this.SM_Location1 = new HashSet<SM_Location>();
            this.SM_Location2 = new HashSet<SM_Location>();
            this.SM_Location3 = new HashSet<SM_Location>();
            this.SM_Location4 = new HashSet<SM_Location>();
            this.SM_Location5 = new HashSet<SM_Location>();
            this.SM_Location6 = new HashSet<SM_Location>();
            this.SM_Location7 = new HashSet<SM_Location>();
            this.SM_Location8 = new HashSet<SM_Location>();
            this.SM_Location9 = new HashSet<SM_Location>();
            this.SM_Location10 = new HashSet<SM_Location>();
            this.SM_Location11 = new HashSet<SM_Location>();
            this.SM_Location12 = new HashSet<SM_Location>();
            this.SM_Location13 = new HashSet<SM_Location>();
            this.SM_Location14 = new HashSet<SM_Location>();
            this.SM_Location15 = new HashSet<SM_Location>();
            this.SM_Location16 = new HashSet<SM_Location>();
            this.SM_Location17 = new HashSet<SM_Location>();
            this.SM_Location18 = new HashSet<SM_Location>();
            this.SM_Location19 = new HashSet<SM_Location>();
            this.SM_Location20 = new HashSet<SM_Location>();
            this.SM_Location21 = new HashSet<SM_Location>();
            this.ICS_Item = new HashSet<ICS_Item>();
        }
    
        public int Ldgr_Cd { get; set; }
        public byte Type_Cd { get; set; }
        public int Group_Cd { get; set; }
        public Nullable<int> Parent_Group_Cd_1 { get; set; }
        public Nullable<int> Parent_Group_Cd_2 { get; set; }
        public Nullable<int> Parent_Group_Cd_3 { get; set; }
        public Nullable<int> Parent_Group_Cd_4 { get; set; }
        public Nullable<int> Parent_Group_Cd_5 { get; set; }
        public Nullable<int> Parent_Group_Cd_6 { get; set; }
        public Nullable<int> Parent_Group_Cd_7 { get; set; }
        public short Curr_Cd { get; set; }
        public string Ldgr_No { get; set; }
        public string L_Ldgr_Name { get; set; }
        public string A_Ldgr_Name { get; set; }
        public Nullable<decimal> Credit_Limit_Amount { get; set; }
        public Nullable<short> Credit_Limit_Days { get; set; }
        public string Addr_1 { get; set; }
        public string Addr_2 { get; set; }
        public string Addr_3 { get; set; }
        public string A_Addr_1 { get; set; }
        public string A_Addr_2 { get; set; }
        public string A_Addr_3 { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Contact_Person { get; set; }
        public string Reference_No { get; set; }
        public string Mobile { get; set; }
        public string License_No { get; set; }
        public string Country { get; set; }
        public string Default_Dr_Cr_Type { get; set; }
        public Nullable<int> Default_Cost_Cd { get; set; }
        public Nullable<short> Default_Sman_Cd { get; set; }
        public Nullable<byte> Default_Pay_Mode_Cd { get; set; }
        public Nullable<long> Default_Project_Cd { get; set; }
        public Nullable<long> Default_Sales_Price_Level { get; set; }
        public Nullable<long> Default_Purchase_Price_Level { get; set; }
        public bool Enable_Sales_Price_Restrictions { get; set; }
        public Nullable<long> Maximum_Sales_Price_Level { get; set; }
        public Nullable<long> Minimum_Sales_Price_Level { get; set; }
        public Nullable<decimal> Maximum_Product_Sales_Discount_In_Percent { get; set; }
        public Nullable<decimal> Maximum_Voucher_Sales_Discount_In_Percent { get; set; }
        public bool Enable_Purchase_Price_Restrictions { get; set; }
        public Nullable<long> Maximum_Purchase_Price_Level { get; set; }
        public Nullable<long> Minimum_Purchase_Price_Level { get; set; }
        public Nullable<decimal> Maximum_Product_Purchase_Discount_In_Percent { get; set; }
        public Nullable<decimal> Maximum_Voucher_Purchase_Discount_In_Percent { get; set; }
        public bool Discontinued { get; set; }
        public bool Reconciled { get; set; }
        public Nullable<short> Grade_Cd { get; set; }
        public Nullable<decimal> Last_Reconciled_Bal { get; set; }
        public Nullable<System.DateTime> Last_Reconciled_Date { get; set; }
        public Nullable<decimal> Current_Reconciled_Bal { get; set; }
        public Nullable<System.DateTime> Current_Reconciled_Date { get; set; }
        public decimal Opening_Bal { get; set; }
        public decimal Transaction_Bal { get; set; }
        public decimal Current_Bal { get; set; }
        public Nullable<System.DateTime> Last_Receipt_Date { get; set; }
        public string Comments { get; set; }
        public bool Show { get; set; }
        public bool Protected { get; set; }
        public short User_Cd { get; set; }
        public System.DateTime User_Date_Time { get; set; }
        public byte[] Time_Stamp { get; set; }
        public System.Guid row_id { get; set; }
        public long Reporting_Sequence_ID { get; set; }
        public Nullable<System.DateTime> Contract_date { get; set; }
        public decimal Contract_Value { get; set; }
        public Nullable<decimal> Default_Discount_In_Percent { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction> ICS_Transaction { get; set; }
        public virtual SM_SALESMAN SM_SALESMAN { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction> ICS_Transaction1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction> ICS_Transaction2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction_Types> ICS_Transaction_Types { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location5 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location6 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location7 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location8 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location9 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location10 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location11 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location12 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location13 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location14 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location15 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location16 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location17 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location18 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location19 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location20 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SM_Location> SM_Location21 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Item> ICS_Item { get; set; }
    }
}
