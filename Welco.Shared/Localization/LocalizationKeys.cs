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

        public static class Provider
        {
            public const string Created = "Provider.Created";
            public const string Updated = "Provider.Updated";
            public const string Deleted = "Provider.Deleted";
            public const string NotFound = "Provider.NotFound";
            public const string Fetched = "Provider.Fetched";
            public const string ListFetched = "Provider.ListFetched";
            public const string CommercialNameRequired = "Provider.CommercialNameRequired";
            public const string EmailRequired = "Provider.EmailRequired";
            public const string EmailInvalid = "Provider.EmailInvalid";
            public const string PasswordRequired = "Provider.PasswordRequired";
            public const string PasswordTooShort = "Provider.PasswordTooShort";
            public const string CommercialRegistrationNumberAlreadyExists = "Provider.CommercialRegistrationNumberAlreadyExists";
            public const string ProviderIdRequired = "Provider.ProviderIdRequired";
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