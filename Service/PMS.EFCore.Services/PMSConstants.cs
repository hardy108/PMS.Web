using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;



namespace PMS.EFCore.Services
{
    public class PMSConstants
    {
        //Running Number
        public const string AttendanceIdPrefix = "ATT";
        public const string AttendanceOvertimeIdPrefix = "SPL";
        public const string ConsumptionCodePrefix = "BKM";
        public const string DocumentBAFCodePrefix = "BAF";
        public const string DepreciationIdPrefix = "DPR";
        public const string EmployeeIdPrefix = "EMP";
        public const string EquipmentTimeSheetIdPrefix = "BKU";
        public const string FixedAssetIdPrefix = "FA";
        public const string GoodIssuePrefix = "GI";
        public const string GoodMutationPrefix = "GM";
        public const string GoodReceiptPrefix = "GR";
        public const string HarvestingBlockResultPrefix = "HBR";
        public const string HarvestingBlockQualityPrefix = "HBQ";
        public const string HarvestingCodePrefix = "BP";
        public const string HarvestingPlanCodePrefix = "RKH";
        public const string HarvestingPlanCalcCodePrefix = "RHP";
        public const string IncentiveMandorPeriodPrefix = "IMP";
        public const string LoadingBlockResultPrefix = "LBR";
        public const string LoadingCodePrefix = "BKP";
        public const string JournalCodePrefix = "JV";
        public const string MandorFineIdPrefix = "MF";
        public const string MillGradingPrefix = "MG";
        public const string OvertimeCodePrefix = "OV";
        public const string PaymentCodePrefix = "PY";
        public const string PremiNonPanenCodePrefix = "PNP";
        public const string PremiPanenIdPrefix = "BPP";
        public const string PremiMuatIdPrefix = "BPM";
        public const string PremiOperatorIdPrefix = "BPO";
        public const string PurchaseInvoiceNoPrefix = "PINV";
        public const string PurchaseOrderNoPrefix = "PO";
        public const string PurchasePaymentNoPrefix = "PPY";
        public const string PurchaseReceiptNoPrefix = "PGR";
        public const string PurchaseRequestNoPrefix = "PR";
        public const string PurchaseReturnNoPrefix = "PRT";
        public const string RangePrefix = "RNG";
        public const string RkhCodePrefix = "RKHD";
        public const string RkhACodePrefix = "RKHA";
        public const string RunningAccountCodePrefix = "RA";
        public const string SalaryItemIdPrefix = "SIT";
        public const string SalaryTypeMapIdPrefix = "STP";
        public const string SalesDeliveryNoPrefix = "SD";
        public const string SalesInvoiceNoPrefix = "SINV";
        public const string SalesOrderNoPrefix = "SO";
        public const string SalesPaymentNoPrefix = "SPY";
        public const string SalesRequestNoPrefix = "SR";
        public const string SalesReturnNoPrefix = "SRT";
        public const string ScheduleEmployeeIdPrefix = "ES";
        public const string VehicleIdPrefix = "VEH";
        public const string LoanPrefix = "LN";
        public const string SpeksiPrefix = "SPKI";
        public const string SpeksiTPHPrefix = "SPIT";
        public const string TravelPrefix = "TRVL";

        //Configuration
        public const string CfgAttendanceCalcByFingerprint = "ATTENDANCECALCBYFINGER";
        public const string CfgAttendanceCalcByFingerprintTrue = "1";
        public const string CfgAttendanceCheckFinger = "ATTENDANCECHECKFINGER";
        public const string CfgAttendanceCheckFingerTrue = "1";
        public const string CfgAttendanceForbidDiv = "ATTENDANCEFORBIDDIV";
        public const string CfgAttendanceForbidDivTrue = "1";
        public const string CFG_AtendanceInCutOff = "ATTENDANCEINCUTOFF";
        public const string CFG_AtendanceInLimit = "ATTENDANCEINLIMIT";
        public const string CFG_AtendanceOutLimit = "ATTENDANCEOUTLIMIT";
        public const string CFG_AtendanceOutLimitFriday = "ATTENDANCEOUTLIMITFRIDAY";
        public const string CFG_AtendanceCardQuota = "ATTENDANCECARDQUOTA";
        public const string CfgAtendanceCardQuotaUpkeep = "ATTENDANCECARDQUOTAUPKEEP";
        public const string CFG_AtendanceUseCardValidate = "ATTENDANCEUSECARDVALIDATE";
        public const string CFG_AtendanceUseCardValidateTrue = "1";
        public const string CfgAtendanceWork2Division = "ATTENDANCEWORK2DIV";
        public const string CfgAtendanceWork2DivisionTrue = "1";
        public const string CfgEmployeeAllowUnitAsistensi = "EMPALLOWUNITASISTENSI";
        public const string CfgEmployeeAllowUnitAsistensiTrue = "1";
        public const string CfgEmployeeForbidGa = "EMPFORBIDGA";
        public const string CfgEmployeeForbidGaTrue = "1";
        public const string CfgEmployeeOldMin = "EMPOLDMIN";
        public const string CfgEmployeeOldMax = "EMPOLDMAX";
        public const string CfgEquipmentTimeSheetAutoUpload = "EQUIPTIMESHEETAUTOUPLOAD";
        public const string CfgEquipmentTimeSheetAutoUploadTrue = "1";
        public const string CFG_HarvestAutoJournal = "HARVESTAUTOJOURNAL";
        public const string CFG_HarvestAutoJournalTrue = "1";
        public const string CfgHarvestAttendanceCheck = "HARVESTATTENDANCECHECK";
        public const string CfgHarvestAttendanceCheckTrue = "1";
        public const string CfgHarvestAttendanceCheckValid = "HARVESTATTENDANCECHECKVALID";
        public const string CfgHarvestAttendanceCheckValidTrue = "1";
        public const string CFG_HarvestingAutoFillEmployee = "HARVESTINGAUTOFILLEMPLOYEE";
        public const string CFG_HarvestingAutoFillEmployeeTrue = "1";
        public const string CfgHarvestingAutoUpload = "HARVESTAUTOUPLOAD";
        public const string CfgHarvestingAutoUploadTrue = "1";
        public const string CFG_HarvestingBasisByKg = "HARVESTINGBASISBYKG";
        public const string CFG_HarvestingBasisByKgTrue = "1";
        public const string CFG_HarvestingBlockKgCalculate = "HARVESTINGBLOCKKGCALCULATE";
        public const string CFG_HarvestingBlockKgCalculateTrue = "1";
        public const string CfgHarvestingBlockTonaseSource = "HARVESTTONASESOURCE";
        public const string CfgHarvestingBrondolMaxPercent = "HARVESTINGBRONDOLMAXPERCENT";
        public const string CFG_HarvestingEmployeeAllowEmptyHK = "HARVESTINGEMPLOYEEALLOWEMPTYHK";
        public const string CFG_HarvestingEmployeeAllowEmptyHKTrue = "1";
        public const string CFG_HarvestingEmployeeAutoHK = "HARVESTINGEMPLOYEEAUTOHK";
        public const string CFG_HarvestingEmployeeAutoHKTrue = "1";
        public const string CFG_HarvestingEmployeeKbAutoHK = "HARVESTINGEMPLOYEEKBAUTOHK";
        public const string CFG_HarvestingEmployeeKbAutoHKTrue = "1";
        public const string CfgHarvestingMaxAreaDayStart = "HARVESTINGMAXAREADAYSTART";
        public const string CfgHarvestingMaxAreaDayEnd = "HARVESTINGMAXAREADAYEND";
        public const string CfgHarvestingMaxEmployee = "HARVESTINGMAXEMPLOYEE";
        public const string CfgHarvestingPlanStandartDatar = "HARVESTINGPLANSTANDARTDATAR";
        public const string CfgHarvestingPlanStandartBerbukit = "HARVESTINGPLANSTANDARTBERBUKIT";
        public const string CfgHarvestingResultBase1TolerancePercent = "HARVESTINGRESULTBASE1TOLERANCEPERCENT";
        public const string CFG_HarvestingResultMaxBrondol = "HARVESTINGRESULTMAXBRONDOL";
        public const string CFG_HarvestingResultMaxPercent = "HARVESTINGRESULTMAXPERCENT";
        public const string CFG_HarvestingResultValidateMaxPercent = "HARVESTINGRESULTVALIDMAXPERCENT";
        public const string CFG_HarvestingResultValidateMaxPercentTrue = "1";
        public const string CfgLoadingAttendanceCheck = "LOADINGATTENDANCECHECK";
        public const string CfgLoadingAttendanceCheckTrue = "1";
        public const string CfgLoadingAttendanceCheckValid = "LOADINGATTENDANCECHECKVALID";
        public const string CfgLoadingAttendanceCheckValidTrue = "1";
        public const string CfgLoadingAutoUpload = "LOADINGAUTOUPLOAD";
        public const string CfgLoadingAutoUploadTrue = "1";
        public const string CfgLoadingBrondolMaxPercent = "LOADINGBRONDOLMAXPERCENT";
        public const string CFG_LoadingEmployeeAllowEmptyHK = "LOADINGEMPLOYEEALLOWEMPTYHK";
        public const string CFG_LoadingEmployeeAllowEmptyHKTrue = "1";
        public const string CFG_LoadingResultMaxBrondol = "LOADINGRESULTMAXBRONDOL";
        public const string CFG_LoadingResultMaxPercent = "LOADINGRESULTMAXPERCENT";
        public const string CFG_LoadingEmployeeAutoHK = "LOADINGEMPLOYEEAUTOHK";
        public const string CFG_LoadingEmployeeAutoHKTrue = "1";
        public const string CfgLoadingResultBase1TolerancePercent = "LOADINGRESULTBASE1TOLERANCEPERCENT";
        public const string CFG_LoadingBasisByKg = "LOADINGBASISBYKG";
        public const string CFG_LoadingBasisByKgTrue = "1";
        public const string CFG_LoadingAutoFillEmployee = "LOADINGAUTOFILLEMPLOYEE";
        public const string CFG_LoadingAutoFillEmployeeTrue = "1";
        public const string CFG_LoadingBlockKgCalculate = "LOADINGBLOCKKGCALCULATE";
        public const string CFG_LoadingBlockKgCalculateTrue = "1";
        public const string CFG_LoadingEmployeeKbAutoHK = "LOADINGEMPLOYEEKBAUTOHK";
        public const string CFG_LoadingEmployeeKbAutoHKTrue = "1";
        public const string CFG_JournalAutoApprove = "JOURNALAUTOAPPROVE";
        public const string CFG_JournalAutoApproveTrue = "1";
        public const string CFG_PayrollAutoJournal = "PAYROLLAUTOJOURNAL";
        public const string CFG_PayrollAutoJournalTrue = "1";
        public const string CFG_PayrollCalculateTaxYTD = "PAYROLLCALCULATETAXYTD";
        public const string CFG_PayrollCalculateTaxYTDTrue = "1";
        public const string CfgPayrollDayPerMonth = "PAYROLLDAYPERMONTH";
        public const string CFG_PayrollHarvestingResultVersion = "PAYROLLHARVESTINGRESULTVERSION";
        public const string CfgPayrollJamsostekBhl = "PAYROLLJAMSOSTEKBHL";
        public const string CfgPayrollJamsostekBhlTrue = "1";
        public const string CfgPayrollNaturaBhl = "PAYROLLNATURABHL";
        public const string CfgPayrollNaturaBhlTrue = "1";
        public const string CfgPayrollNaturaVersion = "PAYROLLNATURAVERSION";
        public const string CfgPayrollNaturaVersionNumber = "PAYROLLNATURAVERSIONNUMBER";
        public const string CfgPayrollPremiHadirActivated = "PAYROLLPREMIHADIRACTIVE";
        public const string CfgPayrollPremiMandorBrondolAddMandor1 = "PAYROLLPREMIMANDORBRONDOLADDMANDOR1";
        public const string CfgPayrollPremiMandorBrondolAddMandor1True = "1";
        public const string CFG_PayrollPremiMandorBrondolVersion = "PAYROLLPREMIMANDORBRONDOLVERSION";
        public const string CfgPayrollTaxPaid = "PAYROLLTAXPAID";
        public const string CfgPayrollTaxPaidTrue = "1";
        public const string CFG_PurchaseInvoiceAutoApprove = "PURCHASEINVOICEAUTOAPPROVE";
        public const string CFG_PurchaseInvoiceAutoApproveTrue = "1";
        public const string CFG_PurchaseOrderAutoApprove = "PURCHASEORDERAUTOAPPROVE";
        public const string CFG_PurchaseOrderAutoApproveTrue = "1";
        public const string CFG_PurchasePaymentAutoApprove = "PURCHASEPAYMENTAUTOAPPROVE";
        public const string CFG_PurchasePaymentAutoApproveTrue = "1";
        public const string CFG_PurchaseReceiptAutoApprove = "PURCHASERECEIPTAUTOAPPROVE";
        public const string CFG_PurchaseReceiptAutoApproveTrue = "1";
        public const string CFG_PurchaseReturnAutoApprove = "PURCHASERETURNAUTOAPPROVE";
        public const string CFG_PurchaseReturnAutoApproveTrue = "1";
        public const string CfgBhlReportSalaryPayment2Version = "BHLRTPSALARYPAYMENT2VERSION";
        public const string CFG_SalesPaymentAutoApproveTrue = "1";
        public const string CFG_SalesDeliveryAutoApprove = "SALESRECEIPTAUTOAPPROVE";
        public const string CFG_SalesInvoiceAutoApprove = "SALESINVOICEAUTOAPPROVE";
        public const string CFG_SalesInvoiceAutoApproveTrue = "1";
        public const string CFG_SalesOrderAutoApprove = "SALESORDERAUTOAPPROVE";
        public const string CFG_SalesOrderAutoApproveTrue = "1";
        public const string CFG_SalesPaymentAutoApprove = "SALESPAYMENTAUTOAPPROVE";
        public const string CFG_SalesDeliveryAutoApproveTrue = "1";
        public const string CFG_SalesReturnAutoApprove = "SALESRETURNAUTOAPPROVE";
        public const string CFG_SalesReturnAutoApproveTrue = "1";
        public const string CfgSpbAutoUpload = "SPBAUTOUPLOAD";
        public const string CfgSpbAutoUploadTrue = "1";
        public const string CfgSecurityUserPasswordExpireTime = "UPASSEXPIREDTIME";
        public const string CfgSecurityUserPasswordLength = "UPASSLENGTH";
        public const string CfgSecurityUserPasswordMinAlphanumeric = "UPASSALPHA";
        public const string CfgSecurityUserPasswordMinCapital = "UPASSCAPITAL";
        public const string CfgSecurityUserPasswordMinNumeric = "UPASSNUMERIC";
        public const string CfgTransactionMaxInput = "TRANMAXINPUT";
        public const string CFG_UpkeepAttendanceCheck = "UPKEEPATTENDANCECHECK";
        public const string CFG_UpkeepAttendanceCheckTrue = "1";
        public const string CFG_UpkeepAutoJournal = "UPKEEPAUTOJOURNAL";
        public const string CFG_UpkeepAutoJournalTrue = "1";
        public const string CFG_UpkeepAutoFillEmployee = "UPKEEPAUTOFILLEMPLOYEE";
        public const string CFG_UpkeepAutoFillEmployeeTrue = "1";
        public const string CfgUpeepAutoUpload = "UPKEEPAUTOUPLOAD";
        public const string CfgUpeepAllowAsistensi = "UPKEEPALLOWASISTENSI";
        public const string CfgUpeepAllowAsistensiTrue = "1";
        public const string CfgUpeepAutoUploadTrue = "1";
        public const string CFG_UpkeepRunningAccountAutoJournal = "UPKEEPRUNNINGACCOUNTAUTOJOURNAL";
        public const string CFG_UpkeepRunningAccountAutoJournalTrue = "1";
        public const string CFG_UseAMAuthentication = "USEAMAUTH";
        public const string CFG_UseAMAuthenticationTrue = "1";
        public const string CfgTransactionMaxSPBList = "TRANMAXSPBLIST";
        public const string CfgHarvestTonaseSourceAutoDay = "HARVESTTONASESOURCEAUTODAY";
        public const string CfgHarvestTonaseSourceLastDate = "HARVESTTONASESOURCELASTDATE";

        public const string Organization_NurseryDivision = "Z";

        public const int PayrollDaysPerMonth = 25;

        public const int AccountType_CasBank = 1;

        public const string ArestaBlockTopografiBerbukit = "BERBUKIT";
        public const string ArestaBlockTopografiDatar = "DATAR";
        public const string ArestaBlockTopografiGelombang = "GELOMBANG";

        public const string GL_Journal_CashBankList = "CBL";
        public const string GL_Journal_CashIn = "CI";
        public const string GL_Journal_CashOut = "CO";
        public const string GL_Journal_GeneralModulCode = "GL";
        public const string GL_Journal_GeneralModulList = "GLL";
        public const string GL_Journal_GoodIssueModulCode = "GI";
        public const string GL_Journal_GoodIssueReverseModulCode = "GIX";
        public const string GL_Journal_GoodReceiptModulCode = "GR";
        public const string GL_Journal_GoodReceiptReverseModulCode = "GRX";
        public const string GL_Journal_HKModulCode = "HK";
        public const string GL_Journal_PaymentModulCode = "PY";

        public const string General_Card_Type_Expedition = "E";
        public const string General_Card_Type_Vendor = "V";

        public const string GeneralPositionFlagPemanen = "6";

        public const string Security_Menu_Report = "Report";
        public const string SecurityUserPasswordDefault = "pmsinitial";

        public const string ActivityPungunBrondolanManual = "HV0411";
        public const string ActivityPungunBrondolanGardan = "HV0412";

        public const string ActivityMuatBrondolan = "HT0411";

        public const string WfJobHarvestMaxBrondolPercent = "HVT_MAXBRD";
        public const string WfJobHarvestQuantity = "HVT_QTY";
        public const string WF_Job_PurchaseRequestCode = "PURC_REQ";
        public const string WF_Job_SalesRequestCode = "SALE_REQ";
        public const string WF_Flag_Approved = "Di Setujui";
        public const string WF_Flag_Reject = "Di Tolak";

        //AttendanceCardType
        public const string AttendanceCardTypeHarvesting = "HVT";
        public const string AttendanceCardTypeUpkeep = "UPK";

        //Salary Type System Code
        public const string SalaryTypeCodeBasicWages = "BASICWGS";
        public const string SalaryTypeCodeBorongan = "BORONG";
        public const string SalaryTypeCodeDebit = "DEBIT";
        public const string SalaryTypeCodeInsentifMandorPanen = "INCMDRPNN";
        public const string SalaryTypeCodeJamsosJhtComp = "JHTCMP";
        public const string SalaryTypeCodeJamsosJhtCompId = "JAMSJHTCMP";
        public const string SalaryTypeCodeJamsosJhtCompDcId = "JAMSJHTCMPDC";
        public const string SalaryTypeCodeJamsosJhtEmp = "JHTEMP";
        public const string SalaryTypeCodeJamsosJhtEmpId = "JAMSJHTEMP";
        public const string SalaryTypeCodeJamsosJkk = "JKK";
        public const string SalaryTypeCodeJamsosJkkId = "JAMSJKK";
        public const string SalaryTypeCodeJamsosJkkEmp = "JAMSJKKDC";
        public const string SalaryTypeCodeJamsosJkm = "JKM";
        public const string SalaryTypeCodeJamsosJkmId = "JAMSJKM";
        public const string SalaryTypeCodeJamsosJkmEmp = "JAMSJKMDC";
        public const string SalaryTypeCodeJamsosJpComp = "JPCMP";
        public const string SalaryTypeCodeJamsosJpCompId = "JAMSJPCMP";
        public const string SalaryTypeCodeJamsosJpCompDcId = "JAMSJPCMPDC";
        public const string SalaryTypeCodeJamsosJpEmp = "JPEMP";
        public const string SalaryTypeCodeJamsosJpEmpId = "JAMSJPEMP";
        public const string SalaryTypeCodeJamsosKesComp = "KESCMP";
        public const string SalaryTypeCodeJamsosKesCompId = "JAMSKESCMP";
        public const string SalaryTypeCodeJamsosKesCompDcId = "JAMSKESCMPDC";
        public const string SalaryTypeCodeJamsosKesEmp = "KESEMP";
        public const string SalaryTypeCodeJamsosKesEmpId = "JAMSKESEMP";
        public const string SalaryTypeCodeNaturaDeduction = "NATDEDUCT";
        public const string SalaryTypeCodeNaturaIncome = "NATINC";
        public const string SalaryTypeCodeOvertime = "OVERT";
        public const string SalaryTypeCodePremiChecker = "PRMPNNCHK";
        public const string SalaryTypeCodePremiHadirPanen = "PRMPNNHDR";
        public const string SalaryTypeCodePremiHektarePanen = "PRMPNNHA";
        public const string SalaryTypeCodePremiKraniPanen = "PRMPNNKRN";
        public const string SalaryTypeCodePremiMandorPanen = "PRMPNNMDR";
        public const string SalaryTypeCodePremimandor1Panen = "PRMPNNMDR1";
        public const string SalaryTypeCodePremiNonPanen = "PRMNPN";
        public const string SalaryTypeCodePremiPanen = "PRMPNN";
        public const string SalaryTypeCodePremiPanenGerdan = "PRMPNNGRD";
        public const string SalaryTypeCodePremiPokokTinggi = "PRMPKKTGI";
        public const string SalaryTypeCodePremiMuat = "PRMPMT";
        public const string SalaryTypeCodePremiMuatKontanan = "PRMPMTKTN";
        public const string SalaryTypeCodePremiOperatorAngkutTBS = "PROPT";
        public const string SalaryTypeCodePremiOperatorAngkutTBSKontanan = "PROPTKTN";
        public const string SalaryTypeCodePremiPanenKontanan = "PRMPNNKTN";
        public const string SalaryTypeCodePremimandor1PanenKontanan = "PRMPNNMDR1KTN";
        public const string SalaryTypeCodePremiMandorPanenKontanan = "PRMPNNMDRKTN";
        public const string SalaryTypeCodePremiKraniPanenKontanan = "PRMPNNKRNKTN";
        public const string SalaryTypeCodePremiCheckerKontanan = "PRMPNNCHKKTN";
        public const string SalaryTypeCodePremiHektarePanenKontanan = "PRMPNNHAKTN";
        public const string SalaryTypeCodePremiPokokTinggiKontanan = "PRMPKKTGIKTN";
        public const string SalaryTypeCodePremiPanenTerbaik = "PRMPNNTBAIK";
        public const string SalaryTypeCodeTax = "PPH21";

        //Salary Type Payment Code
        public const string SalaryTypePaymentCodeJamsostekIncentive = "JAMSINC";
        public const string SalaryTypePaymentCodeJamsostekDeduction = "JAMSDED";
        public const string SalaryTypePaymentCodePremiNonPanen = "PRMNPN";
        public const string SalaryTypePaymentCodePremiPanen = "PRMPNN";
        public const string SalaryTypePaymentCodePremiPanenKontanan = "PRMPNNKTN";
        public const string SalaryTypePaymentCodeIncentive = "INC";

        //Salary Attribute Employee
        public const string SalaryAttributeEmployeeBpjsBase = "BPJSBASE";
        public const string SalaryAttributeEmployeeBpjsKesehatanBase = "BPJSKESBASE";

        //WB Quality
        public const int WbGradingQualityTbsMatang = 6;
        public const int WbGradingQualityTbsMentah = 4;
        public const int WbGradingQualityTbsJanjangKosong = 8;

        //Salary Type Frequen
        public const string SalaryTypeFreqDaily = "D";
        public const string SalaryTypeFreqMonthly = "M";
        public const string SalaryTypeFreqMin25ProHke = "PRO25";

        public const string ServerListEncryptionKey = "svr";

        public const string WbConString = "WB";

        //public const string PasswordEncryptionKey = "pmspwd";
        public const string PasswordEncryptionKey = "am321pwd";

        public const string TokenSecretKey = "bismillahPMS2019";

        public const string ConnectionStringEncryptionKey = "mps";


        public const string TransactionStatusApproved = "A";
        public const string TransactionStatusCanceled = "C";
        public const string TransactionStatusProcess = "P";
        public const string TransactionStatusDeleted = "D";
        public const string TransactionStatusNone = "";


        public const short PayTypeHarian = 0;
        public const short PayTypeKontanan = 1;
        public const short PayTypeBorongan = 2;

        public const string PremiSystemNone = "";
        public const string PremiSystemHarian = "H";
        public const string PremiSystemBulanan = "B";


        public const short HarvestTypePotongBuah = 0;
        public const short HarvestTypeKutipBrondol = 1;
    }


}
