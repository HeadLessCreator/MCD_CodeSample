using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �� �ִ� ������Ʈ�� �������̽�. Player�� ���� �� �ֵ��� ���� ��� ����.
/// </summary>
public interface IEatable
{
    void OnEaten(int damage);
}