using UnityEngine;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Data
{

    [CreateAssetMenu(fileName = "weapondata", menuName = "Scriptable Objects/weapondata")]
    public class weapondata : ScriptableObject
    {
        // 武器の名前
        [field: SerializeField] public string weaponName { get; private set; }

        // 連射タイプ
        [field: SerializeField] public FireType WeaponFireType { get; private set; }

        // 攻撃力
        [field: SerializeField] public int AttackPower { get; private set; }

        // フルオートやバースト時の連射間隔
        [field: SerializeField] public float FireInterval { get; private set; }

        // 次の球が撃てるまでの待機時間
        [field: SerializeField] public float FireRate { get; private set; }

        // マガジンの最大弾数
        [field: SerializeField] public int MaxAmmo { get; private set; }

        // リロードにかかる時間
        [field: SerializeField] public float ReloadTime { get; private set; }

    }

}