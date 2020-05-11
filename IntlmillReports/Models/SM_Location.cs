//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IntlmillReports.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SM_Location
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SM_Location()
        {
            this.ICS_Transaction = new HashSet<ICS_Transaction>();
            this.ICS_Transaction_Details = new HashSet<ICS_Transaction_Details>();
            this.ICS_Transaction_Details1 = new HashSet<ICS_Transaction_Details>();
        }
    
        public short Locat_Cd { get; set; }
        public int Locat_No { get; set; }
        public string L_Locat_Name { get; set; }
        public string A_Locat_Name { get; set; }
        public string L_Short_Name { get; set; }
        public string A_Short_Name { get; set; }
        public string Addr_1 { get; set; }
        public string Addr_2 { get; set; }
        public string Addr_3 { get; set; }
        public string Contact_Person { get; set; }
        public string Phone { get; set; }
        public bool Auto_Create_Delivery_Notes { get; set; }
        public bool Auto_Create_Receipt_Of_Goods { get; set; }
        public bool Auto_Create_Supplier_Return { get; set; }
        public bool Auto_Create_Stock_Return { get; set; }
        public Nullable<int> Inventory_Cd { get; set; }
        public Nullable<int> Adjustment_Cd { get; set; }
        public Nullable<int> Cost_Of_Sale_Cd { get; set; }
        public Nullable<int> PDis_Cd { get; set; }
        public Nullable<int> Cash_Sale_Cd { get; set; }
        public Nullable<int> Credit_Sale_Cd { get; set; }
        public Nullable<int> Cash_SDis_Cd { get; set; }
        public Nullable<int> Credit_SDis_Cd { get; set; }
        public Nullable<int> PRet_Cd { get; set; }
        public Nullable<int> PRet_Dis_Cd { get; set; }
        public Nullable<int> Cash_SRet_Cd { get; set; }
        public Nullable<int> Credit_SRet_Cd { get; set; }
        public Nullable<int> Cash_SRet_Dis_Cd { get; set; }
        public Nullable<int> Credit_SRet_Dis_Cd { get; set; }
        public Nullable<int> Cash_Cd { get; set; }
        public Nullable<int> Suspense_Cd { get; set; }
        public Nullable<int> Inventory_Cost_Cd { get; set; }
        public Nullable<int> Adjustment_Cost_Cd { get; set; }
        public Nullable<int> Cost_Of_Sale_Cost_Cd { get; set; }
        public Nullable<int> PDis_Cost_Cd { get; set; }
        public Nullable<int> Cash_Sale_Cost_Cd { get; set; }
        public Nullable<int> Credit_Sale_Cost_Cd { get; set; }
        public Nullable<int> Cash_SDis_Cost_Cd { get; set; }
        public Nullable<int> Credit_SDis_Cost_Cd { get; set; }
        public Nullable<int> PRet_Cost_Cd { get; set; }
        public Nullable<int> PRet_Dis_Cost_Cd { get; set; }
        public Nullable<int> Cash_SRet_Cost_Cd { get; set; }
        public Nullable<int> Credit_SRet_Cost_Cd { get; set; }
        public Nullable<int> Cash_SRet_Dis_Cost_Cd { get; set; }
        public Nullable<int> Credit_SRet_Dis_Cost_Cd { get; set; }
        public Nullable<int> Cash_Cost_Cd { get; set; }
        public Nullable<int> Suspense_Cost_Cd { get; set; }
        public bool Protected { get; set; }
        public string Comments { get; set; }
        public short User_Cd { get; set; }
        public System.DateTime User_Date_Time { get; set; }
        public byte[] time_stamp { get; set; }
        public bool Show { get; set; }
        public System.Guid row_id { get; set; }
        public bool Is_Primary_Location { get; set; }
        public string Last_Ldgr_No { get; set; }
        public Nullable<bool> Last_Cash_Type { get; set; }
        public string Last_Cash_No { get; set; }
        public Nullable<int> Service_Sales_Cd { get; set; }
        public Nullable<int> Service_Cost_Of_Sales_Cd { get; set; }
        public Nullable<int> Service_Expense_Cd { get; set; }
        public Nullable<int> Non_Inventory_Sales_Cd { get; set; }
        public Nullable<int> Non_Inventory_Cost_Of_Sales_Cd { get; set; }
        public Nullable<int> non_inventory_expense_cd { get; set; }
        public Nullable<int> Service_Sales_Cost_Cd { get; set; }
        public Nullable<int> Service_Cost_Of_Sales_Cost_Cd { get; set; }
        public Nullable<int> Service_Expense_Cost_Cd { get; set; }
        public Nullable<int> Non_Inventory_Sales_Cost_Cd { get; set; }
        public Nullable<int> Non_Inventory_Cost_Of_Sales_Cost_Cd { get; set; }
        public Nullable<int> Non_Inventory_Expense_Cost_Cd { get; set; }
        public string Data_Upload_Path { get; set; }
        public string Credit_Card_Cd { get; set; }
        public Nullable<short> Default_Sman_No { get; set; }
        public bool Allow_Generation_Of_Reverse_Voucher_Type { get; set; }
        public bool Check_Negative_Stock { get; set; }
        public string Inventory_Anly_Code_1 { get; set; }
        public Nullable<byte> Inventory_Anly_Head_No_1 { get; set; }
        public string Inventory_Anly_Code_2 { get; set; }
        public Nullable<byte> Inventory_Anly_Head_No_2 { get; set; }
        public string adjustment_Anly_Code_1 { get; set; }
        public Nullable<byte> Adjustment_Anly_Head_No_1 { get; set; }
        public string Adjustment_Anly_Code_2 { get; set; }
        public Nullable<byte> Adjustment_Anly_Head_No_2 { get; set; }
        public string Cost_Of_Sale_Anly_Code_1 { get; set; }
        public Nullable<byte> Cost_Of_Sale_Anly_Head_No_1 { get; set; }
        public string Cost_Of_Sale_Anly_Code_2 { get; set; }
        public Nullable<byte> Cost_Of_Sale_Anly_Head_No_2 { get; set; }
        public string PDis_Anly_Code_1 { get; set; }
        public Nullable<byte> PDis_Anly_Head_No_1 { get; set; }
        public string PDis_Anly_Code_2 { get; set; }
        public Nullable<byte> PDis_Anly_Head_No_2 { get; set; }
        public string Cash_Sale_Anly_Code_1 { get; set; }
        public Nullable<byte> Cash_Sale_Anly_Head_No_1 { get; set; }
        public string Cash_Sale_Anly_Code_2 { get; set; }
        public Nullable<byte> Cash_Sale_Anly_Head_No_2 { get; set; }
        public string Credit_Sale_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_Sale_Anly_Head_No_1 { get; set; }
        public string Credit_Sale_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_Sale_Anly_Head_No_2 { get; set; }
        public string Cash_SDis_Anly_Code_1 { get; set; }
        public Nullable<byte> Cash_SDis_Anly_Head_No_1 { get; set; }
        public string Cash_SDis_Anly_Code_2 { get; set; }
        public Nullable<byte> Cash_SDis_Anly_Head_No_2 { get; set; }
        public string Credit_SDis_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_SDis_Anly_Head_No_1 { get; set; }
        public string Credit_SDis_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_SDis_Anly_Head_No_2 { get; set; }
        public string PRet_Anly_Code_1 { get; set; }
        public Nullable<byte> PRet_Anly_Head_No_1 { get; set; }
        public string PRet_Anly_Code_2 { get; set; }
        public Nullable<byte> PRet_Anly_Head_No_2 { get; set; }
        public string PRet_Dis_Anly_Code_1 { get; set; }
        public Nullable<byte> PRet_Dis_Anly_Head_No_1 { get; set; }
        public string PRet_Dis_Anly_Code_2 { get; set; }
        public Nullable<byte> PRet_Dis_Anly_Head_No_2 { get; set; }
        public string Cash_SRet_Anly_Code_1 { get; set; }
        public Nullable<byte> Cash_SRet_Anly_Head_No_1 { get; set; }
        public string Cash_SRet_Anly_Code_2 { get; set; }
        public Nullable<byte> Cash_SRet_Anly_Head_No_2 { get; set; }
        public string Credit_SRet_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_SRet_Anly_Head_No_1 { get; set; }
        public string Credit_SRet_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_SRet_Anly_Head_No_2 { get; set; }
        public string Cash_SRet_Dis_Anly_Code_1 { get; set; }
        public Nullable<byte> Cash_SRet_Dis_Anly_Head_No_1 { get; set; }
        public string Cash_SRet_Dis_Anly_Code_2 { get; set; }
        public Nullable<byte> Cash_SRet_Dis_Anly_Head_No_2 { get; set; }
        public string Credit_SRet_Dis_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_SRet_Dis_Anly_Head_No_1 { get; set; }
        public string Credit_SRet_Dis_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_SRet_Dis_Anly_Head_No_2 { get; set; }
        public string Cash_Anly_Code_1 { get; set; }
        public Nullable<byte> Cash_Anly_Head_No_1 { get; set; }
        public string Cash_Anly_Code_2 { get; set; }
        public Nullable<byte> Cash_Anly_Head_No_2 { get; set; }
        public string Suspense_Anly_Code_1 { get; set; }
        public Nullable<byte> Suspense_Anly_Head_No_1 { get; set; }
        public string Suspense_Anly_Code_2 { get; set; }
        public Nullable<byte> Suspense_Anly_Head_No_2 { get; set; }
        public string Service_Sales_Anly_Code_1 { get; set; }
        public Nullable<byte> Service_Sales_Anly_Head_No_1 { get; set; }
        public string Service_Sales_Anly_Code_2 { get; set; }
        public Nullable<byte> Service_Sales_Anly_Head_No_2 { get; set; }
        public string Service_Expense_Anly_Code_1 { get; set; }
        public Nullable<byte> Service_Expense_Anly_Head_No_1 { get; set; }
        public string Service_Expense_Anly_Code_2 { get; set; }
        public Nullable<byte> Service_Expense_Anly_Head_No_2 { get; set; }
        public string Non_Inventory_Sales_Anly_Code_1 { get; set; }
        public Nullable<byte> Non_Inventory_Sales_Anly_Head_No_1 { get; set; }
        public string Non_Inventory_Sales_Anly_Code_2 { get; set; }
        public Nullable<byte> Non_Inventory_Sales_Anly_Head_No_2 { get; set; }
        public string non_inventory_expense_Anly_Code_1 { get; set; }
        public Nullable<byte> non_inventory_expense_Anly_Head_No_1 { get; set; }
        public string non_inventory_expense_Anly_Code_2 { get; set; }
        public Nullable<byte> non_inventory_expense_Anly_Head_No_2 { get; set; }
        public string Service_Cost_Of_Sales_Anly_Code_1 { get; set; }
        public Nullable<byte> Service_Cost_Of_Sales_Anly_Head_No_1 { get; set; }
        public string Service_Cost_Of_Sales_Anly_Code_2 { get; set; }
        public Nullable<byte> Service_Cost_Of_Sales_Anly_Head_No_2 { get; set; }
        public string Non_Inventory_Cost_Of_Sales_Anly_Code_1 { get; set; }
        public Nullable<byte> Non_Inventory_Cost_Of_Sales_Anly_Head_No_1 { get; set; }
        public string Non_Inventory_Cost_Of_Sales_Anly_Code_2 { get; set; }
        public Nullable<byte> Non_Inventory_Cost_Of_Sales_Anly_Head_No_2 { get; set; }
        public Nullable<long> Inventory_Project_Cd { get; set; }
        public Nullable<long> Adjustment_Project_Cd { get; set; }
        public Nullable<long> Cost_Of_Sale_Project_Cd { get; set; }
        public Nullable<long> PDis_Project_Cd { get; set; }
        public Nullable<long> Cash_Sale_Project_Cd { get; set; }
        public Nullable<long> Credit_Sale_Project_Cd { get; set; }
        public Nullable<long> Cash_SDis_Project_Cd { get; set; }
        public Nullable<long> Credit_SDis_Project_Cd { get; set; }
        public Nullable<long> PRet_Project_Cd { get; set; }
        public Nullable<long> PRet_Dis_Project_Cd { get; set; }
        public Nullable<long> Cash_SRet_Project_Cd { get; set; }
        public Nullable<long> Credit_SRet_Project_Cd { get; set; }
        public Nullable<long> Cash_SRet_Dis_Project_Cd { get; set; }
        public Nullable<long> Credit_SRet_Dis_Project_Cd { get; set; }
        public Nullable<long> Cash_Project_Cd { get; set; }
        public Nullable<long> Suspense_Project_Cd { get; set; }
        public Nullable<long> Service_Sales_Project_Cd { get; set; }
        public Nullable<long> Service_Expense_Project_Cd { get; set; }
        public Nullable<long> Non_Inventory_Sales_Project_Cd { get; set; }
        public Nullable<long> non_inventory_expense_Project_Cd { get; set; }
        public Nullable<long> Service_Cost_Of_Sales_Project_Cd { get; set; }
        public Nullable<long> Non_Inventory_Cost_Of_Sales_Project_Cd { get; set; }
        public Nullable<int> Material_Issue_Return_Cd { get; set; }
        public Nullable<int> Material_Issue_Return_Cost_Cd { get; set; }
        public string Material_Issue_Return_Anly_Code_1 { get; set; }
        public Nullable<byte> Material_Issue_Return_Anly_Head_No_1 { get; set; }
        public string Material_Issue_Return_Anly_Code_2 { get; set; }
        public Nullable<byte> Material_Issue_Return_Anly_Head_No_2 { get; set; }
        public Nullable<int> Material_Issue_Return_Project_Cd { get; set; }
        public Nullable<int> Credit_Service_Sales_Cd { get; set; }
        public Nullable<int> Credit_Service_Sales_Cost_Cd { get; set; }
        public string Credit_Service_Sales_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_Service_Sales_Anly_Head_No_1 { get; set; }
        public string Credit_Service_Sales_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_Service_Sales_Anly_Head_No_2 { get; set; }
        public string Credit_Service_Expense_Anly_Code_1 { get; set; }
        public Nullable<byte> Credit_Service_Expense_Anly_Head_No_1 { get; set; }
        public string Credit_Service_Expense_Anly_Code_2 { get; set; }
        public Nullable<byte> Credit_Service_Expense_Anly_Head_No_2 { get; set; }
        public Nullable<int> Credit_non_inventory_Sales_Cd { get; set; }
        public Nullable<int> Credit_non_inventory_Sales_Cost_Cd { get; set; }
        public Nullable<int> Credit_non_inventory_expense_cd { get; set; }
        public string Credit_non_inventory_Sales_Anly_Code_1 { get; set; }
        public Nullable<int> Credit_non_inventory_Sales_Anly_Head_No_1 { get; set; }
        public string Credit_non_inventory_Sales_Anly_Code_2 { get; set; }
        public Nullable<int> Credit_non_inventory_Sales_Anly_Head_No_2 { get; set; }
        public string Credit_non_inventory_Expense_Anly_Code_1 { get; set; }
        public Nullable<int> Credit_non_inventory_Expense_Anly_Head_No_1 { get; set; }
        public string Credit_non_inventory_Expense_Anly_Code_2 { get; set; }
        public Nullable<int> Credit_non_inventory_Expense_Anly_Head_No_2 { get; set; }
        public Nullable<int> Credit_non_inventory_cost_of_sales_cd { get; set; }
        public Nullable<int> Credit_Service_Expense_Cd { get; set; }
        public Nullable<int> Credit_Service_Cost_Of_Sales_Cost_Cd { get; set; }
        public Nullable<int> Credit_service_cost_of_sales_cd { get; set; }
        public Nullable<int> Credit_service_expense_cost_cd { get; set; }
        public Nullable<int> Credit_non_inventory_cost_of_sales_cost_cd { get; set; }
        public Nullable<int> Credit_non_inventory_expense_cost_cd { get; set; }
        public Nullable<int> Credit_service_sales_project_cd { get; set; }
        public Nullable<int> Credit_service_cost_of_sales_project_cd { get; set; }
        public Nullable<int> Credit_service_expense_project_cd { get; set; }
        public Nullable<int> Credit_non_inventory_sales_project_cd { get; set; }
        public Nullable<int> Credit_non_inventory_cost_of_sales_project_cd { get; set; }
        public Nullable<int> Credit_non_inventory_expense_project_cd { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction> ICS_Transaction { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction_Details> ICS_Transaction_Details { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ICS_Transaction_Details> ICS_Transaction_Details1 { get; set; }
        public virtual GL_Ledger GL_Ledger { get; set; }
        public virtual GL_Ledger GL_Ledger1 { get; set; }
        public virtual GL_Ledger GL_Ledger2 { get; set; }
        public virtual GL_Ledger GL_Ledger3 { get; set; }
        public virtual GL_Ledger GL_Ledger4 { get; set; }
        public virtual GL_Ledger GL_Ledger5 { get; set; }
        public virtual GL_Ledger GL_Ledger6 { get; set; }
        public virtual GL_Ledger GL_Ledger7 { get; set; }
        public virtual GL_Ledger GL_Ledger8 { get; set; }
        public virtual GL_Ledger GL_Ledger9 { get; set; }
        public virtual GL_Ledger GL_Ledger10 { get; set; }
        public virtual GL_Ledger GL_Ledger11 { get; set; }
        public virtual GL_Ledger GL_Ledger12 { get; set; }
        public virtual GL_Ledger GL_Ledger13 { get; set; }
        public virtual GL_Ledger GL_Ledger14 { get; set; }
        public virtual GL_Ledger GL_Ledger15 { get; set; }
        public virtual GL_Ledger GL_Ledger16 { get; set; }
        public virtual GL_Ledger GL_Ledger17 { get; set; }
        public virtual GL_Ledger GL_Ledger18 { get; set; }
        public virtual GL_Ledger GL_Ledger19 { get; set; }
        public virtual GL_Ledger GL_Ledger20 { get; set; }
        public virtual GL_Ledger GL_Ledger21 { get; set; }
    }
}
