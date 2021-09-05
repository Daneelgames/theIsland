using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventsSender : MonoBehaviour
{
    public MobCloseAttack mobCloseAttack;

    public void SetMobCloseAttackDangerous()
    {
        mobCloseAttack.DangerousTrue();
    }
    public void SetMobCloseAttackNonDangerous()
    {
        mobCloseAttack.DangerousFalse();
    }
}
