using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardShortcutManager : MonoBehaviour
{
    private string helpMessage;
    private string ctrlHelpMessage;


    [SerializeField] Transform objectToTransform;


    // Start is called before the first frame update
    void Start()
    {
       
       // AddHelp(KeyCode.V, "Hide-show cursor");


        ctrlHelpMessage = "ctrl+key+p or ctrl+key+m to \n";
        ctrlHelpMessage += "increase or decrease values \n";
        //AddCtrlHelp(KeyCode.C, "cutplane position");
    }

    private void ShowMessage(string msg)
    {
       // showTextScript.ShowMessage(msg);

    }

    private void PrintHelp()
    {
        ShowMessage(helpMessage);
    }

    private void PrintCtrlHelp()
    {
        ShowMessage(ctrlHelpMessage);
    }

    // Update is called once per frame
    public void Update()
    {
        // ctrl+key
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                PrintCtrlHelp();
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.P))
            {
                CtrlEvent(1.0f);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.M))
            {
                CtrlEvent(-1.0f);
            }
        }
        else
        {

            // key without ctrl

            if (Input.GetKeyDown(KeyCode.A))
            {

            }
        }
    }

    // value is either 1.0f or -1.0f
    void CtrlEvent(float value)
    {
        float sensitivity = 0.05f;
        if (Input.GetKey(KeyCode.X))
        {
            objectToTransform.localPosition += new Vector3(value * sensitivity, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            objectToTransform.localPosition += new Vector3(0.0f, value * sensitivity, 0.0f);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            objectToTransform.localPosition += new Vector3(0.0f, 0.0f, value * sensitivity);
        }
    }

    void AddHelp(KeyCode code, string text)
    {
        helpMessage = helpMessage + code.ToString() + ": " + text + "\n";
    }
    void AddCtrlHelp(KeyCode code, string text)
    {
        ctrlHelpMessage = ctrlHelpMessage + "ctrl+" + code.ToString() + ": " + text + "\n";
    }
}
