using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
    protected void Start()
    {
        //Space, Z, X, CボタンにXBoxで言うところのA, B, X, Yを割り当てているぞ！！
        InputController.SetForKeyboard();

        //この関数一つ呼ぶだけでゲームパッドで操作できるようになるぞ！
        //ただし、このプロジェクトのProjectSettingsにあるInputManager.assetを使いたいプロジェクトの同じフォルダにコピーしてくれ
        //InputController.SetGamePads();

        //上の奴が気に入らなけりゃ、自分で指定することもできるぞ
        //GamePad.Allとかでいっぺんに指定することもできる
        //InputController.SetGameButton(InputController.GamePad.One, InputController.Button.R1, 0);
        //InputController.SetGamePads();
    }
    
    protected void Update()
    {
        //申し訳ないが毎フレームに一回だけUpdate()を呼んでくれ
        InputController.Update();

        //Unityで言うところのInput.anyKeyDownだ
        //ただし「any」の範囲はゲームパッド内のボタンだけだ
        if (InputController.GetAnyButtonDown())
            Debug.Log("AnyButtonDown");

        //Unityにはない機能（あってしかるべき機能）だ
        //Axisが押されたとき、離されたときを取得できる
        if (InputController.GetAxisDown(InputController.Axis.R_Horizontal) != 0)
            Debug.Log("R_Horizontal Downed");

        //この機能は割と気まぐれで作ったぞ！！
        if (InputController.GetAxisUp(InputController.Axis.R_Horizontal) != 0)
            Debug.Log("R_Horizontal Upped, StayTime was " + InputController.GetAxisStayTime(InputController.Axis.R_Horizontal) + " seconds");
    }
}
