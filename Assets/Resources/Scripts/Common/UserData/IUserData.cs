using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserData
{
    bool IsLoaded { get; set; }

    // ������ �ʱ�ȭ
    void SetDefaultData();
    // ������ �ε�
    void LoadData();
    // ������ ����
    void SaveData();
    void SetData(Dictionary<string, object> firestoreDict);
}
