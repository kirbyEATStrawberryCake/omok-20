using System;

public static class ValidationMessageMapper
{
    public static string GetMessage(SignInValidationResult result)
    {
        switch (result)
        {
            case SignInValidationResult.Success: return "로그인에 성공했습니다.";
            case SignInValidationResult.UsernameEmpty: return "이메일을 입력해주세요.";
            case SignInValidationResult.PasswordEmpty: return "비밀번호를 입력해주세요.";
            default: return "";
        }
    }

    public static string GetMessage(SignUpValidationResult result)
    {
        switch (result)
        {
            case SignUpValidationResult.Success: return "회원가입에 성공했습니다.";
            case SignUpValidationResult.UsernameEmpty: return "이메일을 입력해주세요.";
            case SignUpValidationResult.PasswordEmpty: return "비밀번호를 입력해주세요.";
            case SignUpValidationResult.NicknameEmpty: return "닉네임을 입력해주세요.";
            case SignUpValidationResult.PasswordsDoNotMatch: return "비밀번호가 일치하지 않습니다.";
            case SignUpValidationResult.ProfileNotSelected: return "프로필 사진을 선택해주세요.";
            default: return "";
        }
    }
}

public static class AuthMessageMapper
{
    public static string GetMessage(AuthResponseType errorType)
    {
        switch (errorType)
        {
            case AuthResponseType.SUCCESS: return "";
            case AuthResponseType.INVALID_USERNAME: return "존재하지 않는 이메일입니다.";
            case AuthResponseType.INVALID_PASSWORD: return "비밀번호가 일치하지 않습니다.";
            case AuthResponseType.DUPLICATED_USERNAME: return "이미 가입된 이메일 입니다.";
            case AuthResponseType.NOT_LOGGED_IN: return "로그인 상태가 아닙니다.";
            case AuthResponseType.NETWORK_ERROR: return "네트워크 연결 실패\n네트워크 연결 상태를 확인해주세요.";
            default: return "알 수 없는 오류가 발생했습니다.";
        }
    }
}