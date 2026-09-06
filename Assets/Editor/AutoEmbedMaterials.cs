using UnityEditor;
using UnityEngine;

public class AutoEmbedMaterials : AssetPostprocessor
{
    // Hàm này tự động chạy ngay trước khi một Model (FBX) được import vào Unity
    void OnPreprocessModel()
    {
        // Lấy thông tin bộ import của Model hiện tại
        ModelImporter modelImporter = assetImporter as ModelImporter;

        if (modelImporter != null)
        {
            // Kiểm tra nếu Location đang là External (Legacy) hoặc chưa tối ưu
            if (modelImporter.materialImportMode == ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                // Thay đổi cài đặt thành Use Embedded Materials
                modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;

                // Ghi nhận log để bạn biết file nào vừa được tự động sửa
                Debug.Log($"[AutoEmbed] Đã tự động chuyển đổi sang Embedded Materials cho: {assetPath}");
            }
        }
    }
}
