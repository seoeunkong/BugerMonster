using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PanelCell : MonoBehaviour
{
    public Image IconImg => _iconImg;
    [SerializeField] private Image _iconImg;
    [SerializeField] private Image _iconGrayImg;
    [SerializeField] private Text _Hptext;
    [SerializeField] private Text _Damagetext;
    [SerializeField] private Image _panelCellImg;
     
    public void CreatePanelCell(Character character) //ĳ���� �ִ� ü�°� ���ݷ��� �������� PanelCell �ʱ�ȭ
    {
        this._iconImg.sprite = character.Icon;
        this._iconGrayImg.sprite = character.Icon;
        this._Hptext.text = $"{character.MaxHP}";
        this._Damagetext.text = $"Damage: {character.AttackPower}";

        this._iconImg.fillAmount = 0 ;
    }


    /// <summary>
    /// Panel�� Cell ������ ���������� ����
    /// </summary>
    public void ChangePanelColor() => _panelCellImg.color = Color.red;
 
   
}
