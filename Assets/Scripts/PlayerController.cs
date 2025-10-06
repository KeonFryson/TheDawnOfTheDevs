using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : MonoBehaviour
{

    //public InputAction LeftAction;
    public InputAction MoveAction;
    void Start()
    {


        //Frame Rate Controls
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 10;
        
        
        //LeftAction.Enable(); 
        MoveAction.Enable();
    }

    void Update()
    {
 
  
        Vector2 move = MoveAction.ReadValue<Vector2>();
        Debug.Log(move);
        Vector2 position = (Vector2)transform.position + move * 3.0f *Time.deltaTime;
        transform.position = position;
     
    }
}










/////////////////Debugging START//////////////////////





/////////////////Debugging END//////////////////////









/////////////////Removed START/////////////////////
//void Update()

//{
//float horizontal = 0.0f;
//float vertical = 0.0f;

////Left key
//if (LeftAction.IsPressed())
//{
//    horizontal = -1.0f;
//}
////Right key
//else if (Keyboard.current.rightArrowKey.isPressed)
//{
//    horizontal = 1.0f;
//}
////Up key
//if (Keyboard.current.upArrowKey.isPressed)
//{
//    vertical = 1.0f;
//}
////Down key
//else if (Keyboard.current.downArrowKey.isPressed)
//{
//    vertical = -1.0f;

//}

//Debug.Log(horizontal);
//Debug.Log(vertical);


//Vector2 position = transform.position;

//position.x = position.x + 0.1f * horizontal;
//position.y = position.y + 0.1f * vertical;
//transform.position = position;
//}
//////////////Removed END