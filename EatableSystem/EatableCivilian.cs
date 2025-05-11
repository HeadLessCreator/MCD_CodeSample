using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableCivilian : EatableBase
{
    //ó�� ���� �� - �ù�, ���� ���� ���� ����
    protected override void FirstRespond()
    {
        //���Ҹ� ����
        string[] dyingScreams;

        if (isMale)
        {
            dyingScreams = new string[4] {
                        "Scream_Male1", "Scream_Male2",
                        "Scream_Male3", "Scream_Male4"
                        };
        }
        else
        {
            dyingScreams = new string[4] {
                        "Scream_Female1", "Scream_Female2",
                        "Scream_Female3", "Scream_Female4"
                        };
        }

        string dyingScream = dyingScreams[Random.Range(0, dyingScreams.Length)];
        SoundManager.Instance.PlaySound(dyingScream);

        //�׸��� ����
        Transform shadowTr = transform.Find("Shadow");
        if (shadowTr != null)
        {
            shadowTr.gameObject.SetActive(false);
        }

        //�̺�Ʈ - ����Ʈ, �̼�, ���� ���
        OnHumanKilled.Invoke();
    }

    //���� ��, �� ��°, �ִϸ��̼ǿ��� �����
    protected override void SecondRespond()
    {
        int vfxIndex = 0;

        int randomFireworksSoundIndex = Random.Range(1, 3);
        SoundManager.Instance.PlaySound($"Confetti_{randomFireworksSoundIndex}");

        var vfxProperties = new VFXProperties();
        vfxProperties.vfxName = "ConfettiVFX";
        vfxProperties.vfxPosition = transform.position;
        Debug.Log($"Confetti position: {vfxProperties.vfxPosition}");
        vfxProperties.vfxRotation = onEatenVFXOptions[vfxIndex].transform.rotation;
        vfxProperties.vfxScale = onEatenVFXOptions[vfxIndex].transform.localScale;
        vfxProperties.vfxPlayTime = 1f;
        VFXManager.Instance.OnVFXPlayed(vfxProperties);
    }
}
