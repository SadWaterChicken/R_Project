using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Gắn cái Box vũ khí vào đây")]
    public GameObject weaponHitbox;
    
    [Header("Thời gian ẩn vũ khí nếu không đánh (giây)")]
    public float hideDelay = 10f;

    private float lastAttackTime;
    private WeaponHitbox hitboxScript;
    private SpriteRenderer playerSprite;
    private Vector3 originalLocalPos;

    private void Start()
    {
        // Lấy SpriteRenderer của nhân vật (thường nằm ở object con) để biết đang quay mặt bên nào
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (weaponHitbox != null)
        {
            hitboxScript = weaponHitbox.GetComponent<WeaponHitbox>();
            originalLocalPos = weaponHitbox.transform.localPosition;
            
            // Ban đầu ẩn đi
            weaponHitbox.SetActive(false); 
        }
    }

    void Update()
    {
        // 1. HIỆN VŨ KHÍ VÀ CHÉM KHI BẤM CHUỘT
        if (Input.GetMouseButtonDown(0))
        {
            if (weaponHitbox != null)
            {
                weaponHitbox.SetActive(true);
                lastAttackTime = Time.time;
                
                // Gọi hàm này để chém quái lại lần nữa (mặc dù Box vẫn đang bật)
                if (hitboxScript != null)
                {
                    hitboxScript.ResetHit();
                }
            }
        }

        // 2. TỰ ĐỘNG ẨN SAU KHI ĐỂ IM QUÁ LÂU
        if (weaponHitbox != null && weaponHitbox.activeSelf)
        {
            if (Time.time - lastAttackTime > hideDelay)
            {
                weaponHitbox.SetActive(false);
            }
        }

        // 3. XOAY VÀ ĐỔI BÊN THEO NHÂN VẬT
        if (weaponHitbox != null && playerSprite != null)
        {
            // Lật mặt nhân vật (trái/phải)
            float sideMultiplier = playerSprite.flipX ? -1f : 1f;
            
            // Chuyển Box sang bên trái/phải
            weaponHitbox.transform.localPosition = new Vector3(Mathf.Abs(originalLocalPos.x) * sideMultiplier, originalLocalPos.y, originalLocalPos.z);
            
            // Lật chiều xoay của Box (nếu gắn hình cây kiếm vào nó sẽ xoay mũi kiếm theo)
            Vector3 scale = weaponHitbox.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * sideMultiplier;
            weaponHitbox.transform.localScale = scale;
        }
    }
}
