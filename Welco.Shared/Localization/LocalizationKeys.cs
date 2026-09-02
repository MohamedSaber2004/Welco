namespace Welco.Shared.Localization
{
    public static class LocalizationKeys
    {
        public static class ActionResults
        {
            public const string Ok = "ActionResults.Ok";
            public const string Created = "ActionResults.Created";
            public const string Accepted = "ActionResults.Accepted";
            public const string Deleted = "ActionResults.Deleted";
            public const string Updated = "ActionResults.Updated";
        }

        public static class ExceptionMessages
        {
            public const string Validation = "ExceptionMessages.Validation";
            public const string InvalidModelState = "ExceptionMessages.InvalidModelState";
            public const string NotFound = "ExceptionMessages.NotFound";
            public const string BadRequest = "ExceptionMessages.BadRequest";
            public const string Unauthorized = "ExceptionMessages.Unauthorized";
            public const string Forbidden = "ExceptionMessages.Forbidden";
            public const string Conflict = "ExceptionMessages.Conflict";
            public const string InternalServerError = "ExceptionMessages.InternalServerError";
            public const string UnknownException = "ExceptionMessages.UnknownException";
        }

        public static class OpenApi
        {
            public const string Title = "OpenApi.Title";
            public const string Description = "OpenApi.Description";
            public const string ContactName = "OpenApi.ContactName";
            public const string LicenseName = "OpenApi.LicenseName";
            public const string LanguageParameter = "OpenApi.LanguageParameter";
        }

        public static class Auth
        {
            public const string InvalidCredentials = "Auth.InvalidCredentials";
            public const string InvalidRefreshToken = "Auth.InvalidRefreshToken";
            public const string EmailRequired = "Auth.EmailRequired";
            public const string EmailInvalid = "Auth.EmailInvalid";
            public const string PasswordRequired = "Auth.PasswordRequired";
            public const string TokenRequired = "Auth.TokenRequired";
            public const string TokenExpiryInFuture = "Auth.TokenExpiryInFuture";
            public const string UserIdRequired = "Auth.UserIdRequired";
            public const string UserNotFound = "Auth.UserNotFound";
            public const string UserAlreadyExists = "Auth.UserAlreadyExists";
            public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
            public const string UsernameAlreadyExists = "Auth.UsernameAlreadyExists";
            public const string FullNameRequired = "Auth.FullNameRequired";
            public const string BirthDateInFuture = "Auth.BirthDateInFuture";
            public const string LoginSuccess = "Auth.LoginSuccess";
            public const string RegisterSuccess = "Auth.RegisterSuccess";
            public const string TokenRefreshed = "Auth.TokenRefreshed";
            public const string ProfileUpdated = "Auth.ProfileUpdated";
            public const string ProfileFetched = "Auth.ProfileFetched";
            public const string LockedOut = "Auth.LockedOut";
            public const string EmailNotConfirmed = "Auth.EmailNotConfirmed";
            public const string AccountDeactivated = "Auth.AccountDeactivated";
            public const string RefreshTokenExpired = "Auth.RefreshTokenExpired";
            public const string TooManyAttempts = "Auth.TooManyAttempts";
            public const string OtpSent = "Auth.OtpSent";
            public const string OtpVerified = "Auth.OtpVerified";
            public const string PasswordResetSuccess = "Auth.PasswordResetSuccess";
            public const string InvalidOtp = "Auth.InvalidOtp";
            public const string EmailSendFailed = "Auth.EmailSendFailed";
            public const string OtpCodeRequired = "Auth.OtpCodeRequired";
            public const string OtpCodeFormat = "Auth.OtpCodeFormat";
            public const string NewPasswordRequired = "Auth.NewPasswordRequired";
            public const string PasswordTooShort = "Auth.PasswordTooShort";
            public const string ConfirmPasswordRequired = "Auth.ConfirmPasswordRequired";
            public const string PasswordMismatch = "Auth.PasswordMismatch";
            public const string CurrentPasswordRequired = "Auth.CurrentPasswordRequired";
            public const string WrongCurrentPassword = "Auth.WrongCurrentPassword";
            public const string PasswordChanged = "Auth.PasswordChanged";
            public const string NewPasswordSameAsOld = "Auth.NewPasswordSameAsOld";
            public const string LogoutSuccess = "Auth.LogoutSuccess";
            public const string RoleAssigned = "Auth.RoleAssigned";
            public const string RoleNotFound = "Auth.RoleNotFound";
            public const string OtpEmailSubject = "Auth.OtpEmailSubject";
            public const string OtpEmailBody = "Auth.OtpEmailBody";
            public const string UserTypeRequired = "Auth.UserTypeRequired";
            public const string LanguageRequired = "Auth.LanguageRequired";
        }

        public static class UserManagement
        {
            public const string UserCreated = "UserManagement.UserCreated";
            public const string UserUpdated = "UserManagement.UserUpdated";
            public const string UserDeleted = "UserManagement.UserDeleted";
            public const string UserActivated = "UserManagement.UserActivated";
            public const string UserDeactivated = "UserManagement.UserDeactivated";
            public const string UserNotFound = "UserManagement.UserNotFound";
            public const string UserAlreadyExists = "UserManagement.UserAlreadyExists";
            public const string PasswordChanged = "UserManagement.PasswordChanged";
            public const string RoleChanged = "UserManagement.RoleChanged";
            public const string UserFetched = "UserManagement.UserFetched";
            public const string UsersFetched = "UserManagement.UsersFetched";
            public const string CannotDeactivateSelf = "UserManagement.CannotDeactivateSelf";
            public const string CannotDeleteSelf = "UserManagement.CannotDeleteSelf";
            public const string FullNameRequired = "UserManagement.FullNameRequired";
            public const string EmailRequired = "UserManagement.EmailRequired";
            public const string EmailInvalid = "UserManagement.EmailInvalid";
            public const string PasswordRequired = "UserManagement.PasswordRequired";
            public const string UserIdRequired = "UserManagement.UserIdRequired";
            public const string RoleRequired = "UserManagement.RoleRequired";
            public const string UserTypeRequired = "UserManagement.UserTypeRequired";
            public const string PageNumberPositive = "UserManagement.PageNumberPositive";
            public const string PageSizeRange = "UserManagement.PageSizeRange";
        }

        public static class UserAddress
        {
            public const string AddressCreated = "UserAddress.AddressCreated";
            public const string AddressUpdated = "UserAddress.AddressUpdated";
            public const string AddressDeleted = "UserAddress.AddressDeleted";
            public const string AddressNotFound = "UserAddress.AddressNotFound";
            public const string AddressFetched = "UserAddress.AddressFetched";
            public const string AddressesFetched = "UserAddress.AddressesFetched";
            public const string DefaultAddressSet = "UserAddress.DefaultAddressSet";
            public const string StreetRequired = "UserAddress.StreetRequired";
            public const string CityRequired = "UserAddress.CityRequired";
            public const string CountryRequired = "UserAddress.CountryRequired";
            public const string AddressTypeRequired = "UserAddress.AddressTypeRequired";
            public const string UserIdRequired = "UserAddress.UserIdRequired";
            public const string AddressIdRequired = "UserAddress.AddressIdRequired";
            public const string AddressNotBelongToUser = "UserAddress.AddressNotBelongToUser";
            public const string CountryIdRequired = "UserAddress.CountryIdRequired";
            public const string CityIdRequired = "UserAddress.CityIdRequired";
            public const string ZoneIdRequired = "UserAddress.ZoneIdRequired";
        }

        public static class Country
        {
            public const string Created = "Country.Created";
            public const string Updated = "Country.Updated";
            public const string Deleted = "Country.Deleted";
            public const string NotFound = "Country.NotFound";
            public const string Fetched = "Country.Fetched";
            public const string ListFetched = "Country.ListFetched";
            public const string NameEnRequired = "Country.NameEnRequired";
            public const string NameArRequired = "Country.NameArRequired";
            public const string AlreadyExists = "Country.AlreadyExists";
            public const string CountryIdRequired = "Country.CountryIdRequired";
        }

        public static class City
        {
            public const string Created = "City.Created";
            public const string Updated = "City.Updated";
            public const string Deleted = "City.Deleted";
            public const string NotFound = "City.NotFound";
            public const string Fetched = "City.Fetched";
            public const string ListFetched = "City.ListFetched";
            public const string NameEnRequired = "City.NameEnRequired";
            public const string NameArRequired = "City.NameArRequired";
            public const string CountryIdRequired = "City.CountryIdRequired";
            public const string CityIdRequired = "City.CityIdRequired";
            public const string AlreadyExists = "City.AlreadyExists";
        }

        public static class Zone
        {
            public const string Created = "Zone.Created";
            public const string Updated = "Zone.Updated";
            public const string Deleted = "Zone.Deleted";
            public const string NotFound = "Zone.NotFound";
            public const string Fetched = "Zone.Fetched";
            public const string ListFetched = "Zone.ListFetched";
            public const string NameEnRequired = "Zone.NameEnRequired";
            public const string NameArRequired = "Zone.NameArRequired";
            public const string CityIdRequired = "Zone.CityIdRequired";
            public const string ZoneIdRequired = "Zone.ZoneIdRequired";
            public const string AlreadyExists = "Zone.AlreadyExists";
        }

        public static class Certification
        {
            public const string Created = "Certification.Created";
            public const string Updated = "Certification.Updated";
            public const string Deleted = "Certification.Deleted";
            public const string NotFound = "Certification.NotFound";
            public const string Fetched = "Certification.Fetched";
            public const string ListFetched = "Certification.ListFetched";
            public const string CertificateNumberRequired = "Certification.CertificateNumberRequired";
            public const string CertificateNumberAlreadyExists = "Certification.CertificateNumberAlreadyExists";
            public const string TitleRequired = "Certification.TitleRequired";
            public const string IssuedToRequired = "Certification.IssuedToRequired";
            public const string IssuerRequired = "Certification.IssuerRequired";
            public const string IssueDateRequired = "Certification.IssueDateRequired";
            public const string IssueDateInFuture = "Certification.IssueDateInFuture";
            public const string ExpiryDateBeforeIssueDate = "Certification.ExpiryDateBeforeIssueDate";
            public const string CertificationIdRequired = "Certification.CertificationIdRequired";
        }

        public static class Category
        {
            public const string Created = "Category.Created";
            public const string Updated = "Category.Updated";
            public const string Deleted = "Category.Deleted";
            public const string NotFound = "Category.NotFound";
            public const string Fetched = "Category.Fetched";
            public const string ListFetched = "Category.ListFetched";
            public const string NameEnRequired = "Category.NameEnRequired";
            public const string NameArRequired = "Category.NameArRequired";
            public const string ParentCategoryNotFound = "Category.ParentCategoryNotFound";
            public const string CategoryIdRequired = "Category.CategoryIdRequired";
            public const string ProvidersFetched = "Category.ProvidersFetched";
            public const string ProductsFetched = "Category.ProductsFetched";
        }

        public static class Currency
        {
            public const string Created = "Currency.Created";
            public const string Updated = "Currency.Updated";
            public const string Deleted = "Currency.Deleted";
            public const string NotFound = "Currency.NotFound";
            public const string Fetched = "Currency.Fetched";
            public const string ListFetched = "Currency.ListFetched";
            public const string NameEnRequired = "Currency.NameEnRequired";
            public const string NameArRequired = "Currency.NameArRequired";
            public const string CodeRequired = "Currency.CodeRequired";
            public const string SymbolRequired = "Currency.SymbolRequired";
            public const string CodeAlreadyExists = "Currency.CodeAlreadyExists";
            public const string CurrencyIdRequired = "Currency.CurrencyIdRequired";
        }

        public static class Product
        {
            public const string Created = "Product.Created";
            public const string Updated = "Product.Updated";
            public const string Deleted = "Product.Deleted";
            public const string NotFound = "Product.NotFound";
            public const string Fetched = "Product.Fetched";
            public const string ListFetched = "Product.ListFetched";
            public const string NameEnRequired = "Product.NameEnRequired";
            public const string NameArRequired = "Product.NameArRequired";
            public const string SkuRequired = "Product.SkuRequired";
            public const string SkuAlreadyExists = "Product.SkuAlreadyExists";
            public const string SlugRequired = "Product.SlugRequired";
            public const string SlugAlreadyExists = "Product.SlugAlreadyExists";
            public const string PriceRequired = "Product.PriceRequired";
            public const string PricePositive = "Product.PricePositive";
            public const string StockNotNegative = "Product.StockNotNegative";
            public const string CategoryRequired = "Product.CategoryRequired";
            public const string CategoryNotFound = "Product.CategoryNotFound";
            public const string ProductIdRequired = "Product.ProductIdRequired";
        }

        public static class Company
        {
            public const string Created = "Company.Created";
            public const string Updated = "Company.Updated";
            public const string Deleted = "Company.Deleted";
            public const string NotFound = "Company.NotFound";
            public const string Fetched = "Company.Fetched";
            public const string ListFetched = "Company.ListFetched";
            public const string NameRequired = "Company.NameRequired";
            public const string TypeRequired = "Company.TypeRequired";
            public const string CountryRequired = "Company.CountryRequired";
            public const string TierLevelInvalid = "Company.TierLevelInvalid";
            public const string CompanyIdRequired = "Company.CompanyIdRequired";
        }

        public static class DistributorApplication
        {
            public const string Created = "DistributorApplication.Created";
            public const string Updated = "DistributorApplication.Updated";
            public const string NotFound = "DistributorApplication.NotFound";
            public const string Fetched = "DistributorApplication.Fetched";
            public const string ListFetched = "DistributorApplication.ListFetched";
            public const string Approved = "DistributorApplication.Approved";
            public const string Rejected = "DistributorApplication.Rejected";
            public const string AlreadyProcessed = "DistributorApplication.AlreadyProcessed";
            public const string ApplicationIdRequired = "DistributorApplication.ApplicationIdRequired";
            public const string PendingApproval = "DistributorApplication.PendingApproval";
            public const string NotApplied = "DistributorApplication.NotApplied";
            public const string CompanyNotApproved = "DistributorApplication.CompanyNotApproved";
        }

        public static class Cart
        {
            public const string Created = "Cart.Created";
            public const string NotFound = "Cart.NotFound";
            public const string Fetched = "Cart.Fetched";
            public const string ListFetched = "Cart.ListFetched";
            public const string Cleared = "Cart.Cleared";
            public const string ItemAdded = "Cart.ItemAdded";
            public const string ItemUpdated = "Cart.ItemUpdated";
            public const string ItemRemoved = "Cart.ItemRemoved";
            public const string ItemNotFound = "Cart.ItemNotFound";
            public const string CartIdRequired = "Cart.CartIdRequired";
            public const string UserIdRequired = "Cart.UserIdRequired";
            public const string SessionIdRequired = "Cart.SessionIdRequired";
            public const string CartItemIdRequired = "Cart.CartItemIdRequired";
            public const string QuantityPositive = "Cart.QuantityPositive";
            public const string PriceNotNegative = "Cart.PriceNotNegative";
            public const string UserIdOrSessionRequired = "Cart.UserIdOrSessionRequired";
        }

        public static class Order
        {
            public const string Created = "Order.Created";
            public const string NotFound = "Order.NotFound";
            public const string Fetched = "Order.Fetched";
            public const string ListFetched = "Order.ListFetched";
            public const string Updated = "Order.Updated";
            public const string InvalidStatus = "Order.InvalidStatus";
            public const string ItemsRequired = "Order.ItemsRequired";
            public const string QuantityPositive = "Order.QuantityPositive";
            public const string PriceNotNegative = "Order.PriceNotNegative";
            public const string OrderIdRequired = "Order.OrderIdRequired";
            public const string StatusRequired = "Order.StatusRequired";
        }

        public static class Document
        {
            public const string Created = "Document.Created";
            public const string Deleted = "Document.Deleted";
            public const string Fetched = "Document.Fetched";
            public const string ListFetched = "Document.ListFetched";
            public const string NotFound = "Document.NotFound";
            public const string TitleRequired = "Document.TitleRequired";
            public const string DocTypeRequired = "Document.DocTypeRequired";
            public const string FileUrlRequired = "Document.FileUrlRequired";
            public const string FileSizeNotNegative = "Document.FileSizeNotNegative";
            public const string DocumentIdRequired = "Document.DocumentIdRequired";
        }

        public static class HelpCategory
        {
            public const string Created = "HelpCategory.Created";
            public const string Updated = "HelpCategory.Updated";
            public const string Deleted = "HelpCategory.Deleted";
            public const string Fetched = "HelpCategory.Fetched";
            public const string ListFetched = "HelpCategory.ListFetched";
            public const string NotFound = "HelpCategory.NotFound";
            public const string NameRequired = "HelpCategory.NameRequired";
            public const string AlreadyExists = "HelpCategory.AlreadyExists";
            public const string HelpCategoryIdRequired = "HelpCategory.HelpCategoryIdRequired";
        }

        public static class HelpArticle
        {
            public const string Created = "HelpArticle.Created";
            public const string Updated = "HelpArticle.Updated";
            public const string Deleted = "HelpArticle.Deleted";
            public const string Fetched = "HelpArticle.Fetched";
            public const string ListFetched = "HelpArticle.ListFetched";
            public const string NotFound = "HelpArticle.NotFound";
            public const string TitleRequired = "HelpArticle.TitleRequired";
            public const string BodyRequired = "HelpArticle.BodyRequired";
            public const string SlugRequired = "HelpArticle.SlugRequired";
            public const string CategoryRequired = "HelpArticle.CategoryRequired";
            public const string HelpArticleIdRequired = "HelpArticle.HelpArticleIdRequired";
            public const string SlugAlreadyExists = "HelpArticle.SlugAlreadyExists";
        }

        public static class FAQ
        {
            public const string Created = "FAQ.Created";
            public const string Updated = "FAQ.Updated";
            public const string Deleted = "FAQ.Deleted";
            public const string Fetched = "FAQ.Fetched";
            public const string ListFetched = "FAQ.ListFetched";
            public const string NotFound = "FAQ.NotFound";
            public const string QuestionRequired = "FAQ.QuestionRequired";
            public const string AnswerRequired = "FAQ.AnswerRequired";
        }

        public static class SupportTicket
        {
            public const string Created = "SupportTicket.Created";
            public const string Fetched = "SupportTicket.Fetched";
            public const string ListFetched = "SupportTicket.ListFetched";
            public const string NotFound = "SupportTicket.NotFound";
            public const string Updated = "SupportTicket.Updated";
            public const string Closed = "SupportTicket.Closed";
            public const string SubjectRequired = "SupportTicket.SubjectRequired";
            public const string MessageRequired = "SupportTicket.MessageRequired";
            public const string ReplyRequired = "SupportTicket.ReplyRequired";
        }

        public static class LandingPage
        {
            public const string Created = "LandingPage.Created";
            public const string Fetched = "LandingPage.Fetched";
            public const string ListFetched = "LandingPage.ListFetched";
            public const string NotFound = "LandingPage.NotFound";
            public const string SlugAlreadyExists = "LandingPage.SlugAlreadyExists";
            public const string SlugRequired = "LandingPage.SlugRequired";
            public const string TypeRequired = "LandingPage.TypeRequired";
            public const string HeroTitleRequired = "LandingPage.HeroTitleRequired";
        }

        public static class RFQ
        {
            public const string Created = "RFQ.Created";
            public const string Updated = "RFQ.Updated";
            public const string Fetched = "RFQ.Fetched";
            public const string ListFetched = "RFQ.ListFetched";
            public const string NotFound = "RFQ.NotFound";
            public const string InvalidStatus = "RFQ.InvalidStatus";
            public const string ItemsRequired = "RFQ.ItemsRequired";
        }

        public static class Quote
        {
            public const string Created = "Quote.Created";
            public const string Fetched = "Quote.Fetched";
            public const string ListFetched = "Quote.ListFetched";
            public const string NotFound = "Quote.NotFound";
            public const string Approved = "Quote.Approved";
            public const string Declined = "Quote.Declined";
        }

        public static class AttachmentMessages
        {
            public const string FileEmpty = "Attachments.FileEmpty";
            public const string InvalidFormat = "Attachments.InvalidFormat";
            public const string InvalidFileType = "Attachments.InvalidFileType";
            public const string InvalidPlace = "Attachments.InvalidPlace";
            public const string FileTooLarge = "Attachments.FileTooLarge";
            public const string NoMediaProvided = "Attachments.NoMediaProvided";
            public const string UploadFailed = "Attachments.UploadFailed";
            public const string FileUploaded = "Attachments.FileUploaded";
            public const string FileNotFound = "Attachments.FileNotFound";
            public const string FileDeleted = "Attachments.FileDeleted";
        }
    }
}