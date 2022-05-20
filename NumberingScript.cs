using TMPro;
using UnityEngine;

public class NumberingScript : MonoBehaviour
{
    public int numberingOfCoin;
    private static TextMeshPro _textMeshPro;

    public int Numbering
    {
        get => numberingOfCoin;
        set { numberingOfCoin = value;
            _textMeshPro.text = numberingOfCoin.ToString();
        }
    }

    private void Awake()
    {
        _textMeshPro = transform.GetChild(0).GetComponent<TextMeshPro>();
    }
}