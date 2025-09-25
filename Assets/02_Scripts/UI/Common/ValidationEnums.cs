public enum SignInValidationResult
{
    Success,
    UsernameEmpty,
    PasswordEmpty
}

public enum SignUpValidationResult
{
    Success,
    UsernameEmpty,
    PasswordEmpty,
    NicknameEmpty,
    PasswordsDoNotMatch,
    ProfileNotSelected
}