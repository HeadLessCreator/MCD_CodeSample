using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableCar : EatableBase
{
    // ����� �ڵ���, ������, ����� ���� ����

    //ó�� ���� ��
    protected override void FirstRespond()
    {
        string eatingSFXName = "Bite_Car";
        SoundManager.Instance.PlaySound(eatingSFXName);

        CarController carCon = this.GetComponent<CarController>();
        carCon.Stop();
        carCon.enabled = false;
        carCon.isGettingDestroyed = true;

        OnVehicleDestroyed.Invoke();
    }

    //���� ��, �� ��°, �ִϸ��̼ǿ��� �����
    protected override void SecondRespond()
    {
        //����
        SoundManager.Instance.PlaySound("Swallow_Car");

        //����Ƽ
        int num = Random.Range(0, onEatenVFXOptions.Length);
        var vfxProperties = new VFXProperties();
        vfxProperties.vfxName = onEatenVFXOptions[num].name;
        vfxProperties.vfxPosition = transform.position;
        if (vfxProperties.vfxName == "LiquidMuzzleOil")
        {
            Quaternion randomRotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
            vfxProperties.vfxRotation = randomRotation;
        }
        else
        {
            vfxProperties.vfxRotation = onEatenVFXOptions[num].transform.rotation;
        }
        vfxProperties.vfxScale = onEatenVFXOptions[num].transform.localScale;
        vfxProperties.vfxPlayTime = 1f;
        VFXManager.Instance.OnVFXPlayed(vfxProperties);
    }
}
