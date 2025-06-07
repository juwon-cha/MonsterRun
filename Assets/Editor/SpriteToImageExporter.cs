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
            EditorUtility.DisplayDialog("����", "��������Ʈ�� �������ּ���.", "Ȯ��");
            return;
        }

        float pixelsPerUnit = sprite.pixelsPerUnit;
        FilterMode originalFilterMode = sprite.texture.filterMode;

        int width = Mathf.CeilToInt(sprite.rect.width);
        int height = Mathf.CeilToInt(sprite.rect.height);

        // �ӽ� GameObject ����
        GameObject cameraGO = new GameObject("TempCamera");
        GameObject spriteGO = new GameObject("TempSprite");

        var camera = cameraGO.AddComponent<Camera>();
        var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // ī�޶� ����
        camera.orthographic = true;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographicSize = height / pixelsPerUnit / 2f;
        camera.transform.position = new Vector3(0, 0, -10);

        // ��������Ʈ�� �ϴ����� ��ġ���� ��ü �̹����� ī�޶� �ȿ� �������� ����
        spriteGO.transform.position = new Vector3(0, -(height / pixelsPerUnit / 2f), 0);

        // RenderTexture ����
        RenderTexture rt = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
        rt.filterMode = originalFilterMode;
        RenderTexture.active = rt;
        camera.targetTexture = rt;

        camera.Render();

        // Texture2D ���� �� ����
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        tex.filterMode = originalFilterMode; // FilterMode �ݿ�

        // PNG ����
        string path = EditorUtility.SaveFilePanel("Save Sprite", Application.dataPath, sprite.name, "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log($"��������Ʈ ���� �Ϸ�: {path}");
            AssetDatabase.Refresh();

            // ���������� Sprite�� �ٽ� �������� (Unity ���ο��� PPU �����Ϸ��� �ʿ�)
            // PNG�� �ٽ� Sprite�� Import�ؼ� PPU�� ���͸�� ����
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

        // �ӽ� ���ҽ� ����
        RenderTexture.active = null; // Ȱ��ȭ�� ���� �ؽ�ó ����
        camera.targetTexture = null; // ī�޶��� ������ ��� ����
        Object.DestroyImmediate(rt); // RenderTexture ��� �ı�
        Object.DestroyImmediate(cameraGO); // �ӽ� ī�޶� GameObject ��� �ı�
        Object.DestroyImmediate(spriteGO); // �ӽ� ��������Ʈ GameObject ��� �ı�
    }
}
#endif