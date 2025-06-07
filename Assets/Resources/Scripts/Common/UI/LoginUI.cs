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
        // �α��� ó���� �񵿱�� ó���Ǳ� ������ �α��� ����� �α��� ��û�� ������ �����ӿ� ó������ ����
        // �α��� ó�� �� �α��� UI�� ��� ���������� ������ �α��� ��ư�� ��� ���� �α��� ��û�� �� �� �ֱ� ������ UI �ݾ���
    }

    public void OnClickSignInWithApple()
    {
        Logger.Log($"{GetType()}::OnClickSignInWithApple");

        FirebaseManager.Instance.SignInWithApple();
        CloseUI();
    }
}
