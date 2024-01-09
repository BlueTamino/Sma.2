using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectionMenu : MonoBehaviour
{
    [SerializeField]
    private bool UseInt;
    [SerializeField]
    private int Limit;
    [SerializeField]
    private string Suffix;
    [SerializeField]
    private string[] SelectionPresets;
    [SerializeField]
    private TMP_InputField Inputfield;
    [HideInInspector]
    public string resultString;
    [HideInInspector]
    public int resultInt;
    private int pointer;
    private int IntPointer;
    private void Start()
    {
        pointer = Random.Range(0, SelectionPresets.Length);
        resultString = SelectionPresets[pointer]; 
        IntPointer = 4;
        resultInt = 4;
        if (UseInt)
        {
            Inputfield.text = resultInt + Suffix;
        }
        else
        {
            Inputfield.text = resultString + Suffix;
        }
    }
    public void MovePointerRight()
    {
        pointer ++;
        IntPointer++;
        if (pointer >= SelectionPresets.Length)
        {
            pointer = 0;
        }
        else if(pointer < 0)
        {
            pointer = SelectionPresets.Length - 1;
        }
        if (UseInt && IntPointer > Limit)
        {
            resultInt = IntPointer;
            Inputfield.text = resultInt.ToString() + Suffix;
        }
        else if(UseInt && IntPointer <= Limit)
        {
            resultInt = Limit + 1;
            Inputfield.text = resultInt.ToString() + Suffix;
        }
        else
        {
            resultString = SelectionPresets[pointer];
            Inputfield.text = resultString + Suffix;
        }
    }
    public void MovePointerLeft()
    {
        pointer --;
        IntPointer--;
        if (pointer >= SelectionPresets.Length)
        {
            pointer = 0;
        }
        else if (pointer < 0)
        {
            pointer = SelectionPresets.Length - 1;
        }
        if (UseInt && IntPointer > Limit)
        {
            resultInt = IntPointer;
            Inputfield.text = resultInt.ToString() + Suffix;
        }
        else if (UseInt && IntPointer <= Limit)
        {
            resultInt = Limit + 1;
            Inputfield.text = resultInt.ToString() + Suffix;
        }
        else
        {
            resultString = SelectionPresets[pointer];
            Inputfield.text = resultString + Suffix;
        }
    }
    public void GetValueFromInput()
    {
        resultString = Inputfield.text;
        int.TryParse(Inputfield.text, out resultInt);
    }
}
