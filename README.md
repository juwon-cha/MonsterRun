# MonsterRun
## <a id="toc"></a>목차

1. [개요](#1-개요)
2. [클래스 UML](#2-클래스-uml)
3. [게임 예시 이미지](#3-게임-예시-이미지)
4. [기능](#4-기능)
5. [툴](#5-툴)
6. [오류와 문제](#6-오류와-문제)

---

## 1. 개요

* 프로젝트 주제: 모바일 2D 캐주얼 게임
* 프로젝트 목적: 간단한 모바일 캐주얼 게임을 개발하여 다양한 UI 및 게임 시스템 구현, 모바일 최적화를 경험하고 구글 플레이 출시를 목표
* 개발 인원: 1명(차주원)
* 기술적 요구사항
  - 클라이언트: Unity2022
  - 서버: Firebase
  - DB: Firestore
  - CDN: AWS CDN(S3, CloudFront)

[BackToTop](#toc)

---

## 2. 클래스 UML

[BackToTop](#toc)

---

## 3. 게임 예시 이미지

<p align="center">
  <img src="https://github.com/user-attachments/assets/3e76ee18-3800-4410-af73-57c66cf3546c" width="150"/>
  <img src="https://github.com/user-attachments/assets/a293d7fa-0ddf-4a21-8a01-04b63e019a74" width="150"/>
  <img src="https://github.com/user-attachments/assets/7d465dc4-cc17-4440-a75a-58c2a20fc96a" width="150"/>
  <img src="https://github.com/user-attachments/assets/ac2dd375-2bbc-4694-aef3-9d281b3cc83e" width="150"/>
  <img src="https://github.com/user-attachments/assets/e0a2bcb8-81a2-4966-9406-d60c44e053f3" width="150"/>
</p>

[BackToTop](#toc)

---

## 4. 기능

* UI
  - 로그인
    + Firebase Auth 활용한 구글 로그인
  - 로비
  - 챕터 선택
  - 캐릭터 상태 및 인벤토리
    + 아이템 획득 및 장착
    + 아이템 장착에 따른 캐릭터 변경
  - 미션
    + 일정 미션 달성 후 업적 수령
  - 상점
    + 패키지
    + 보석
    + 골드
  - 환경설정
    + 사운드 온/오프
    + 다중 언어
* 인게임
  - 장애물을 점프해서 피하면 점수를 얻는 간단한 게임
  - 게임 클리어 후 보상
  - 게임 오버
* 원격 다운로드
  - AWS CDN 활용
* 다중 언어 지원
  - Unity Localization 활용

[BackToTop](#toc)

---

## 5. 툴

* 멀티플 2D 스프라이트에서 하나의 스프라이트(.png)를 추출하는 툴
  - 하나의 파일에 여러 개의 스프라이트로 이루어진 멀티플 2D 스프라이트에서 특정 스프라이트 하나만 추출하기 위한 툴
  - 2D 스프라이트를 유니티 에디터에서 선택하고 툴을 실행하면 선택한 2D 스프라이트만 원하는 경로에 저장할 수 있는 기능
  - AI를 활용해 제작
    + 추출된 단일 스프라이트가 기존의 스프라이트의 설정(PPU, 필터모드)을 유지할 수 있도록 수정
    + 내 프로젝트에 맞게 전체 이미지가 추출될 수 있도록 아래 처럼 스프라이트 위치 코드 수정
```cs
spriteGO.transform.position = new Vector3(0, -(height / pixelsPerUnit / 2f), 0);
```

  - 전체 코드

```cs
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SpriteToImageExporter : Editor
{
    [MenuItem("Tools/SpriteToImageExporter")]
    public static void ExportImgaeFromSprite()
    {
        if (!(Selection.activeObject is Sprite sprite))
        {
            EditorUtility.DisplayDialog("오류", "스프라이트를 선택해주세요.", "확인");
            return;
        }

        float pixelsPerUnit = sprite.pixelsPerUnit;
        FilterMode originalFilterMode = sprite.texture.filterMode;

        int width = Mathf.CeilToInt(sprite.rect.width);
        int height = Mathf.CeilToInt(sprite.rect.height);

        // 임시 GameObject 생성
        GameObject cameraGO = new GameObject("TempCamera");
        GameObject spriteGO = new GameObject("TempSprite");

        var camera = cameraGO.AddComponent<Camera>();
        var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // 카메라 설정
        camera.orthographic = true;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographicSize = height / pixelsPerUnit / 2f;
        camera.transform.position = new Vector3(0, 0, -10);

        // 스프라이트를 하단으로 위치시켜 전체 이미지가 카메라 안에 들어오도록 조정
        spriteGO.transform.position = new Vector3(0, -(height / pixelsPerUnit / 2f), 0);

        // RenderTexture 생성
        RenderTexture rt = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
        rt.filterMode = originalFilterMode;
        RenderTexture.active = rt;
        camera.targetTexture = rt;

        camera.Render();

        // Texture2D 생성 및 설정
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        tex.filterMode = originalFilterMode; // FilterMode 반영

        // PNG 저장
        string path = EditorUtility.SaveFilePanel("Save Sprite", Application.dataPath, sprite.name, "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log($"스프라이트 저장 완료: {path}");
            AssetDatabase.Refresh();

            // 선택적으로 Sprite도 다시 생성해줌 (Unity 내부에서 PPU 유지하려면 필요)
            // PNG를 다시 Sprite로 Import해서 PPU와 필터모드 유지
            string assetPath = "Assets" + path.Replace(Application.dataPath, "");
            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = originalFilterMode;
                importer.SaveAndReimport();
            }
        }

        // 임시 리소스 정리
        RenderTexture.active = null; // 활성화된 렌더 텍스처 해제
        camera.targetTexture = null; // 카메라의 렌더링 대상 해제
        Object.DestroyImmediate(rt); // RenderTexture 즉시 파괴
        Object.DestroyImmediate(cameraGO); // 임시 카메라 GameObject 즉시 파괴
        Object.DestroyImmediate(spriteGO); // 임시 스프라이트 GameObject 즉시 파괴
    }
}
#endif
```

[BackToTop](#toc)

---

## 6. 오류와 문제

* Google sign-in SDK 추가 및 적용 후 빌드 오류
  - Assets/GoogleSignIn/Editor/m2repository/com/google/signin/google-signin-support/1.0.4(해당하는 버전) 경로의 폴더로 진입하고 .srcaar 파일들을 .aar 확장자로 변경 후 문제 해결 시도
  - 확장자 변경 후 여전히 빌드 에러 발생
  - AI를 활용해 중복된 google-signin-support 라이브러리 문제인 것을 확인
  - 중복 문제를 해결하기 위해 {프로젝트이름}\Assets\Plugins\Android 경로로 가서 mainTemplate.gradle 파일 열기
  - gradle 파일에서 아래 코드 주석 처리하거나 삭제해서 문제 해결. 아래 선언이 존재하고 동시에 .aar 파일도 존재하면 충돌이 생겨 오류 발생 가능성 있음

```
implementation 'com.google.signin:google-signin-support:1.0.4'
```

<br>

* 안드로이드 빌드 후 스마트폰에서 게임 실행 시 버벅이는 문제
  - 유니티 에디터에서는 문제 없지만 빌드 후 스마트폰에서 플레이하면 게임이 버벅이는 문제 발생
  - 리소스들 어드레서블로 관리, 2D 스프라이트들을 아틀라스로 처리

[BackToTop](#toc)
