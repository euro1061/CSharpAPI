namespace QBackend.Dtos
{
    public partial class ChangePasswordDto
    {
        public string OldPassword { get; set; } = null!;

        public string NewPassword { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;
    }
}