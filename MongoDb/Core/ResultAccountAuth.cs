namespace ModalStrikeServer.MongoDb.Core
{
    public enum ResultAccountAuth
    {
        SuccessfullyAuth,
        NotAuthorized,
        IncorrectPassword,
        InvalidPassword,
        InvalidLogin,
        Failed
    }
}