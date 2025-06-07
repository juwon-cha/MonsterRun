using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserData
{
    bool IsLoaded { get; set; }

    // 데이터 초기화
    void SetDefaultData();
    // 데이터 로드
    void LoadData();
    // 데이터 저장
    void SaveData();
    void SetData(Dictionary<string, object> firestoreDict);
}
