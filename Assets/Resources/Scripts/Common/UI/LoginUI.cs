using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginUI : BaseUI
{
    public void OnClickSignInWithGoogle()
    {
        Logger.Log($"{GetType()}::OnClickSignInWithGoogle");

        FirebaseManager.Instance.SignInWithGoogle();
        CloseUI();
        // 로그인 처리는 비동기로 처리되기 때문에 로그인 결과는 로그인 요청과 동일한 프레임에 처리되지 않음
        // 로그인 처리 중 로그인 UI가 계속 열려있으면 유저가 로그인 버튼을 계속 눌러 로그인 요청을 할 수 있기 때문에 UI 닫아줌
    }

    public void OnClickSignInWithApple()
    {
        Logger.Log($"{GetType()}::OnClickSignInWithApple");

        FirebaseManager.Instance.SignInWithApple();
        CloseUI();
    }
}
