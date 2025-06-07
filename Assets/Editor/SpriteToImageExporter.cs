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