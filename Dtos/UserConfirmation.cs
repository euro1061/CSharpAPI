namespace QBackend.Dtos
{
    public partial class UserConfirmation
    {
        public byte[] PasswordHash { get; set; } = null!;

        public byte[] PasswordSalt { get; set; } = null!;
    }
}