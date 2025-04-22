using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Common
{
    public class GlobalConstants
    {
        public const string AuthKey = "THIS IS KEY FOR API SECURED AUTHENTICATION";
        public const int OKStatus = 200;
        public const int NoDataStatus = 212;
        public const int ErrorStatus = 500;
        public const int DuplicatePhoneNUmberStatus = 601;
        public const int DuplicateEmailStatus = 602;
        public const int DuplicateCategoryStatus = 603;
        public const int PhoneNumberNotPresnt = 604;
        public const int EmailNotPresent = 605;

        public const int BadRequestStatus = 400;
        public const string MerchantUserCreateMessage = "Merchant User added sucessfully";
        public const string MerchantCreateMessage = "Merchant added sucessfully";
        public const string MerchantUpdateMessage = "Merchant updated sucessfully";
        public const string MerchantUserUpdateMessage = "Member updated successfully";
        public const string MerchantDeleteMessage = "Merchant deleted sucessfully";
        public const string CategoryCreateMessage = "Category added sucessfully";
        public const string CategoryUpdateMessage = "Category updated sucessfully";
        public const string CategoryDeleteMessage = "Category deleted sucessfully";
        public const string DataNotFound = "Data not found";
        public const string InvalidUser = "Invalid credentials";
        public const string DuplicatePhoneNumber = "Phone number allready exists";
        public const string DuplicateEmail = "Email already exists";
        public const string DuplicateCategory = "Category name allready exists";
        public const string NudgeMessageLimitExtend = "requesting send message limit exceeds";
        public const string NudgeMessage = "Nudge Send scucessfully";
        public const string PhoneNumberNotPresntMessage = "Phone number not found";
        public const string EmailNotPresentMessage = "Email not found";
        public const int InactiveCustomer = 202;
        public const int DuplicateTemplateStatus = 603;
        public const string DuplicateTemplate = "Duplicate Template not found";
    }
}
