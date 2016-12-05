using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ボタンの名前とかは基本的にXBox用のコントローラー準拠だ！！
/// 毎フレーム一回だけ、こいつを使う前にUpdateしてくれ！！
/// AddController（初めに数回）→Update（毎フレーム一回）→（GetButtonDownとか）みたいな感じで！！
/// </summary>
public static class InputController
{
    private static List<Controller> controllers = new List<Controller>();

    #region GamePadひな形
    private static Dictionary<Button, int> padButtons = new Dictionary<Button, int>() {
        { Button.A, 0 },
        { Button.B, 1 },
        { Button.X, 2 },
        { Button.Y, 3 },
        { Button.L1, 4 },
        { Button.R1, 5 },
        { Button.L2, 6 },
        { Button.R2, 7 },
        { Button.Select, 8 },
        { Button.Start, 9 },
        { Button.LeftStick, 10 },
        { Button.RightStick, 11 },
    };
    #endregion

    private static List<Dictionary<Button, int>> gamePads = null;

    public static double Threshold { get; private set; }

    private static int lastUpdatedFrame = -1;

    /// <summary>
    /// UnityのデフォルトのInputに対応させるぞ！
    /// 詳しくは関数の中身を見てくれ！！
    /// </summary>
    public static void SetDefault()
    {
        controllers = new List<Controller>();

        Controller controller = new Controller();

        controller.Buttons[Button.A].SetButtonName("Jump");
        controller.Buttons[Button.B].SetButtonName("Fire1");
        controller.Buttons[Button.X].SetButtonName("Fire2");
        controller.Buttons[Button.Y].SetButtonName("Fire3");
        controller.Buttons[Button.Start].SetButtonName("Submit");
        controller.Buttons[Button.Select].SetButtonName("Cancel");

        controller.Axes[Axis.Cross_Horizontal].SetAxisName("Horizontal");
        controller.Axes[Axis.Cross_Vertical].SetAxisName("Vertical");

        AddController(controller);
    }

    /// <summary>
    /// キーボードで操作できるようにするぞ！！
    /// 詳しくは関数の中身を見てくれ！！
    /// </summary>
    public static void SetForKeyboard()
    {
        controllers = new List<Controller>();

        Controller controller = new Controller();

        controller.Buttons[Button.A].SetKeyCode(KeyCode.Space);
        controller.Buttons[Button.B].SetKeyCode(KeyCode.Z);
        controller.Buttons[Button.X].SetKeyCode(KeyCode.X);
        controller.Buttons[Button.Y].SetKeyCode(KeyCode.C);

        controller.Buttons[Button.Start].SetKeyCode(KeyCode.Return);
        controller.Buttons[Button.Select].SetKeyCode(KeyCode.Escape);

        controller.Axes[Axis.R_Horizontal].SetKeyCodes(KeyCode.RightArrow, KeyCode.LeftArrow);
        controller.Axes[Axis.R_Vertical].SetKeyCodes(KeyCode.UpArrow, KeyCode.DownArrow);
        controller.Axes[Axis.L_Horizontal].SetKeyCodes(KeyCode.D, KeyCode.A);
        controller.Axes[Axis.L_Vertical].SetKeyCodes(KeyCode.W, KeyCode.S);
        controller.Axes[Axis.Cross_Horizontal].SetKeyCodes(KeyCode.RightArrow, KeyCode.LeftArrow);
        controller.Axes[Axis.Cross_Vertical].SetKeyCodes(KeyCode.UpArrow, KeyCode.DownArrow);
        controller.Axes[Axis.Side].SetKeyCodes(KeyCode.E, KeyCode.Q);

        AddController(controller);
    }
    
    /// <summary>
    /// 任意のKeyCodeを割り当てられるぞ！！
    /// </summary>
    /// <param name="buttons"></param>
    /// <param name="button_keys"></param>
    /// <param name="axes"></param>
    /// <param name="axis_keys">positiveとnegativeが必要なので、それらを配列にしたもののリスト</param>
    public static void SetKeyCodes(List<Button> buttons, List<KeyCode> button_keys, List<Axis> axes, List<KeyCode[]> axis_keys)
    {
        if (controllers == null)
            controllers = new List<Controller>();

        Controller controller;
        if (controllers.Count == 0)
        {
            controller = new Controller();
            AddController(controller);
        }
        else
            controller = controllers[0];
        
        for(var i = 0;i < buttons.Count;i++)
        {
            controller.SetButtonKeyCode(buttons[i], button_keys[i]);
        }

        for(var i = 0;i < axes.Count;i++)
        {
            controller.SetAxisKeyCodes(axes[i], axis_keys[i][0], axis_keys[i][1]);
        }

   }

    #region GamePad初期化用関数
    /// <summary>
    /// InitializeGamePads()でひな形にされるGamePadの設定
    /// </summary>
    /// <param name="button"></param>
    /// <param name="button_num"></param>
    public static void SetDefaultPadButton(Button button, int button_num)
    {
        padButtons[button] = button_num;
    }

    /// <summary>
    /// GameButtonsをひな形に4つのGamePadを作る
    /// </summary>
    private static void InitializeGamePads()
    {
        gamePads = new List<Dictionary<Button, int>>();

        for(var i = 0;i < (int)GamePad.All;i++)
        {
            var buttons = new Dictionary<Button, int>();
            for(var j = 0;j <= 11;j++)
            {
                buttons.Add((Button)j, padButtons[(Button)j]);
            }

            gamePads.Add(buttons);
        }

        SetGamePads();
    }

    /// <summary>
    /// padで指定されたコントローラーのbutton_num番目のボタンを、buttonボタンに割り当てる
    /// </summary>
    /// <param name="pad"></param>
    /// <param name="button"></param>
    /// <param name="button_num">実際に押されるボタンの番号</param>
    public static void SetPadButton(GamePad pad, Button button, int button_num)
    {
        if (gamePads == null) InitializeGamePads();

        if((int)pad < (int)GamePad.All)
        {
            gamePads[(int)pad][button] = button_num;
        }
        else if(pad == GamePad.All)
        {
            for(var i = 0;i < (int)GamePad.All;i++)
            {
                gamePads[i][button] = button_num;
            }
        }

        SetGamePads();
    }

    /// <summary>
    /// GamePadsにもとづいて、4つのコントローラーを作る
    /// </summary>
    public static void SetGamePads()
    {
        if (gamePads == null) InitializeGamePads();

        controllers = new List<Controller>();

        for (var i = 0; i < (int)GamePad.All; i++)
        {
            Controller controller = new Controller();

            for(var j = 0;j < (int)Button.None;j++)
            {
                controller.Buttons[(Button)j].SetKeyCode((KeyCode)(350 + 20 * i + gamePads[i][(Button)j]));
            }

            controller.Axes[Axis.R_Horizontal].SetAxisName("GamePad" + (i + 1) + "_R_Horizontal");
            controller.Axes[Axis.R_Vertical].SetAxisName("GamePad" + (i + 1) + "_R_Vertical");
            controller.Axes[Axis.L_Horizontal].SetAxisName("GamePad" + (i + 1) + "_L_Horizontal");
            controller.Axes[Axis.L_Vertical].SetAxisName("GamePad" + (i + 1) + "_L_Vertical");
            controller.Axes[Axis.Cross_Horizontal].SetAxisName("GamePad" + (i + 1) + "_Cross_Horizontal");
            controller.Axes[Axis.Cross_Vertical].SetAxisName("GamePad" + (i + 1) + "_Cross_Vertical");
            controller.Axes[Axis.Side].SetAxisName("GamePad" + (i + 1) + "_Side");

            AddController(controller);
        }
    }
    #endregion

    /// <summary>
    /// コントローラーを追加するぞ！！
    /// 任意の数追加できるぞ！！
    /// </summary>
    /// <param name="controller"></param>
    public static void AddController(Controller controller)
    {
        controllers.Add(controller);
    }

    /// <summary>
    /// GetAxisDownなどをする場合の閾値を設定するぞ！！
    /// デフォルトは迷ったけど0.8にしたぞ！！
    /// </summary>
    /// <param name="threshold"></param>
    public static void SetThreshold(double threshold, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return;
        if (pad == GamePad.All)
        {
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].SetThreshold(threshold);
            }
        }
        else
            controllers[(int)pad].SetThreshold(threshold);
    }

    public static void SetSensitivity(double sensitivity,  GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return;
        if (pad == GamePad.All)
        {
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].SetAxisSensitivity(sensitivity);
            }
        }
        else
            controllers[(int)pad].SetAxisSensitivity(sensitivity);
    }

    public static void SetGravity(double gravity, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return;
        if (pad == GamePad.All)
        {
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].SetAxisGravity(gravity);
            }
        }
        else
            controllers[(int)pad].SetAxisGravity(gravity);
    }

    public static void SetDead(double dead, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return;
        if (pad == GamePad.All)
        {
            for (var i = 0; i < controllers.Count; i++)
            {
                controllers[i].SetAxisDead(dead);
            }
        }
        else
            controllers[(int)pad].SetAxisDead(dead);
    }

    /// <summary>
    /// こいつを毎フレーム呼んでくれないと動かないぞ！！
    /// </summary>
    public static void Update()
    {
        if (lastUpdatedFrame == Time.frameCount) return;
        lastUpdatedFrame = Time.frameCount;

        if (controllers == null || controllers.Count == 0) SetForKeyboard();

        foreach(var controller in controllers)
        {
            controller.Update();
        }
    }


    #region Button関数

    /// <summary>
    /// ボタンが押された瞬間だけtrueを返すぞ
    /// </summary>
    /// <param name="button"></param>
    /// <param name="controller">何番目のコントローラーか、0オリジンのデフォルト0</param>
    /// <returns></returns>
    public static bool GetButtonDown(Button button, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool down = false;
            foreach (var controller in controllers)
            {
                down = down || controller.GetButtonDown(button);
            }
            return down;
        }
        else
            return controllers[(int)pad].GetButtonDown(button);
    }

    public static bool GetAnyButtonDown(GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool down = false;
            foreach (var controller in controllers)
            {
                down = down || controller.GetAnyButtonDown();
            }
            return down;
        }
        else
            return controllers[(int)pad].GetAnyButtonDown();
    }

    /// <summary>
    /// ボタンが押されている間trueを返すぞ！！
    /// </summary>
    /// <param name="button"></param>
    /// <param name="controller">何番目のコントローラーか、0オリジンのデフォルト0</param>
    /// <returns></returns>
    public static bool GetButtonStay(Button button, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool stay = false;
            foreach (var controller in controllers)
            {
                stay = stay || controller.GetButtonStay(button);
            }
            return stay;
        }
        else
            return controllers[(int)pad].GetButtonStay(button);
    }

    public static bool GetAnyButtonStay(GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool stay = false;
            foreach (var controller in controllers)
            {
                stay = stay || controller.GetAnyButtonStay();
            }
            return stay;
        }
        else
            return controllers[(int)pad].GetAnyButtonStay();
    }

    public static bool GetButtonStayPeriodicly(Button button, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool stay = false;
            foreach (var controller in controllers)
            {
                stay = stay || controller.GetButtonStayPeriodicly(button);
            }
            return stay;
        }
        else
            return controllers[(int)pad].GetButtonStayPeriodicly(button);
    }

    /// <summary>
    /// ボタンが離された瞬間だけtrueを返すぞ！！
    /// </summary>
    /// <param name="button"></param>
    /// <param name="controller">何番目のコントローラーか、0オリジンのデフォルト0</param>
    /// <returns></returns>
    public static bool GetButtonUp(Button button, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool up = false;
            foreach (var controller in controllers)
            {
                up = up || controller.GetButtonUp(button);
            }
            return up;
        }
        else
            return controllers[(int)pad].GetButtonUp(button);
    }

    public static bool GetAnyButtonUp(GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return false;
        if (pad == GamePad.All)
        {
            bool up = false;
            foreach (var controller in controllers)
            {
                up = up || controller.GetAnyButtonUp();
            }
            return up;
        }
        else
            return controllers[(int)pad].GetAnyButtonUp();
    }

    public static float GetButtonStayTime(Button button, GamePad pad = GamePad.One)
    {
        if (pad < GamePad.All)
        {
            return controllers[(int)pad].GetButtonStayTime(button);
        }
        else
        {
            return 0;
        }
    }

    #endregion

    #region Axis関数

    /// <summary>
    /// 軸の方向が変わった時だけ1か-1を返すぞ！！
    /// それ以外は0を返すぞ！！
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="controller">何番目のコントローラーか、0オリジンのデフォルト0</param>
    /// <returns></returns>
    public static int GetAxisDown(Axis axis, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int down = 0;
            foreach (var controller in controllers)
            {
                down = down + controller.GetAxisDown(axis);
            }
            if (Mathf.Abs(down) > 1) down = down / Mathf.Abs(down);
            return down;
        }
        else
            return controllers[(int)pad].GetAxisDown(axis);
    }

    public static int GetAnyAxisDown(bool isHorizontal, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int down = 0;
            foreach (var controller in controllers)
            {
                down = down + controller.GetAnyAxisDown(isHorizontal);
            }
            if (Mathf.Abs(down) > 1) down = down / Mathf.Abs(down);
            return down;
        }
        else
            return controllers[(int)pad].GetAnyAxisDown(isHorizontal);
    }

    /// <summary>
    /// 軸の方向が変わった時だけ1か-1を返すぞ！！
    /// 例えば、右入力を離すと1を、左入力を離すと-1を返すぞ！！
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="controller">軸の方向が変わった時だけ1か-1を返すぞ！！</param>
    /// <returns></returns>
    public static int GetAxisUp(Axis axis, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int up = 0;
            foreach (var controller in controllers)
            {
                up = up + controller.GetAxisUp(axis);
            }
            if (Mathf.Abs(up) > 1) up = up / Mathf.Abs(up);
            return up;
        }
        else
            return controllers[(int)pad].GetAxisUp(axis);
    }

    public static int GetAnyAxisUp(bool isHorizontal, GamePad pad = GamePad.One)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int up = 0;
            foreach (var controller in controllers)
            {
                up = up + controller.GetAnyAxisUp(isHorizontal);
            }
            if (Mathf.Abs(up) > 1) up = up / Mathf.Abs(up);
            return up;
        }
        else
            return controllers[(int)pad].GetAnyAxisUp(isHorizontal);
    }

    /// <summary>
    /// 普通のGetAxisだ！！
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="controller">軸の方向が変わった時だけ1か-1を返すぞ！！</param>
    /// <returns></returns>
    public static float GetAxis(Axis axis, GamePad pad = GamePad.One, bool sum = false)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            float value = 0;
            foreach (var controller in controllers)
            {
                value = value + controller.GetAxis(axis);
            }
            if (!sum && Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);
            return value;
        }
        else
            return controllers[(int)pad].GetAxis(axis);
    }

    public static int GetAxisPeriodicly(Axis axis, GamePad pad = GamePad.One,  bool sum = false)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int value = 0;
            foreach (var controller in controllers)
            {
                value = value + controller.GetAxisPeriodicly(axis);
            }
            if (!sum && Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);
            return value;
        }
        else
            return controllers[(int)pad].GetAxisPeriodicly(axis);
    }

    public static float GetAnyAxis(bool isHorizontal, GamePad pad = GamePad.One, bool sum = false)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            float value = 0;
            foreach (var controller in controllers)
            {
                value = value + controller.GetAnyAxis(isHorizontal);
            }
            if (!sum && Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);
            return value;
        }
        else
            return controllers[(int)pad].GetAnyAxis(isHorizontal);
    }

    /// <summary>
    /// 普通のGetAxisRowだが、返す値はintだ！！
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="controller">軸の方向が変わった時だけ1か-1を返すぞ！！</param>
    /// <param name="sum">複数のコントローラーの入力を合計した値を返すかどうか。trueになってると3とかが帰ってくる可能性がある</param>
    /// <returns></returns>
    public static int GetAxisRow(Axis axis, GamePad pad = GamePad.One, bool sum = false)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int value = 0;
            foreach (var controller in controllers)
            {
                value = value + controller.GetAxisRow(axis);
            }
            if (!sum && Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);
            return value;
        }
        else
            return controllers[(int)pad].GetAxisRow(axis);
    }

    public static int GetAxisRow(bool isHorizontal, GamePad pad = GamePad.One, bool sum = false)
    {
        if (pad == GamePad.None) return 0;
        if (pad == GamePad.All)
        {
            int value = 0;
            foreach (var controller in controllers)
            {
                value = value + controller.GetAnyAxisRow(isHorizontal);
            }
            if (!sum && Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);
            return value;
        }
        else
            return controllers[(int)pad].GetAnyAxisRow(isHorizontal);
    }

    public static float GetAxisStayTime(Axis axis, GamePad pad = GamePad.One)
    {
        if (pad < GamePad.All)
        {
            return controllers[(int)pad].GetAxisStayTime(axis);
        }
        else
            return 0;
    }

    #endregion


    /// <summary>
    /// インスタンスを作ったら、
    /// Button_Aとかに対してSetButtonNameとか、SetKeyCodeを
    /// Horizontal_Rightとかに対してSetAxisNameとか、SetKeyCodesを
    /// それぞれ実行してくれ！！
    /// めんどくさかったら、InputControllerのSetDefault()あるいはSetGamePads()を実行してくれ！！
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// AxisのDownやUp、Rowなどの閾値
        /// </summary>
        public double Threshold { get; private set; }

        public Dictionary<Button, ButtonState> Buttons;
        public Dictionary<Axis, AxisState> Axes; 

        /// <summary>
        /// 初期化するぞ！！
        /// これだけでは動かないぞ！！
        /// </summary>
        public Controller()
        {
            Buttons = new Dictionary<Button, ButtonState>();

            #region Button初期化
            for(var i = 0;i < (int)Button.None;i++)
            {
                Buttons.Add((Button)i, new ButtonState((Button)i));
            }
            #endregion

            Axes = new Dictionary<Axis, AxisState>();

            #region Axis初期化
            for(var i = 0;i < (int)Axis.None;i++)
            {
                Axes.Add((Axis)i, new AxisState((Axis)i));
            }
            #endregion

            SetThreshold(Threshold);
        }

        /// <summary>
        /// 毎フレーム実行してくれ！！
        /// InputControllerにAddControllerしてある場合はInputController.Update()から呼んでくれるぞ！！
        /// </summary>
        public void Update()
        {
            foreach(var button in Buttons.Values)
            {
                if (button != null && button.Enabled)
                    button.Update();
            }
            
            foreach(var axis in Axes.Values)
            {
                if (axis != null && axis.Enabled)
                    axis.Update();
            }
        }

        public void SetThreshold(double threshold)
        {
            foreach(var key in Axes.Keys)
            {
                Axes[key].Threshold = threshold;
            }

            Threshold = threshold;
        }

        public void SetButtonKeyCode(Button button, KeyCode key)
        {
            Buttons[button].SetKeyCode(key);
        }

        public void SetButtonName(Button button, string name)
        {
            Buttons[button].SetButtonName(name);
        }

        public void SetAxisKeyCodes(Axis axis, KeyCode positive, KeyCode negative)
        {
            Axes[axis].SetKeyCodes(positive, negative);
        }

        public void SetAxisName(Axis axis, string name)
        {
            Axes[axis].SetAxisName(name);
        }

        public void SetAxisSensitivity(double sensitivity)
        {
            for(var i = 0;i < (int)Axis.None;i++)
            {
                Axes[(Axis)i].Sensitivity = sensitivity;
            }
        }

        public void SetAxisGravity(double gravity)
        {
            for (var i = 0; i < Axes.Count; i++)
            {
                Axes[(Axis)i].Gravity = gravity;
            }
        }

        public void SetAxisDead(double dead)
        {
            if (dead >= 1) return;
            for (var i = 0; i < (int)Axis.None;i++)
            {
                Axes[(Axis)i].Dead = dead;
            }
        }

        #region Button関数
        public bool GetButtonDown(Button button)
        {
            return Buttons[button].GetButtonDown();
        }

        public bool GetAnyButtonDown()
        {
            bool down = false;

            for(var i = 0;i < (int)Button.None;i++)
            {
                down = down || GetButtonDown((Button)i);
            }

            return down;
        }

        public bool GetButtonStay(Button button)
        {
            return Buttons[button].GetButtonStay();
        }

        public bool GetAnyButtonStay()
        {
            bool stay = false;

            for (var i = 0; i < (int)Button.None; i++)
            {
                stay = stay || GetButtonStay((Button)i);
            }

            return stay;
        }

        public bool GetButtonStayPeriodicly(Button button)
        {
            return Buttons[button].GetButtonStayPeriodicly();
        }

        public bool GetAnyButtonStayPeriodicly()
        {
            bool stay = false;

            for(var i = 0;i < (int)Button.None;i++)
            {
                stay = stay || GetButtonStayPeriodicly((Button)i);
            }

            return stay;
        }

        public bool GetButtonUp(Button button)
        {
            return Buttons[button].GetButtonUp();
        }

        public bool GetAnyButtonUp()
        {
            bool up = false;

            for (var i = 0; i < (int)Button.None; i++)
            {
                up = up || GetButtonUp((Button)i);
            }

            return up;
        }

        public float GetButtonStayTime(Button button)
        {
            if (button == Button.None) return 0;

            return Buttons[button].StayTime;
        }
        
        #endregion

        #region Axis関数
        public int GetAxisDown(Axis axis)
        {
            return Axes[axis].GetAxisDown();
        }

        public int GetAnyAxisDown(bool isHorizontal)
        {
            int value = 0;
            if (isHorizontal)
            {
                value += GetAxisDown(Axis.R_Horizontal);
                value += GetAxisDown(Axis.L_Horizontal);
                value += GetAxisDown(Axis.Cross_Horizontal);
            }
            else
            {
                value += GetAxisDown(Axis.R_Vertical);
                value += GetAxisDown(Axis.L_Vertical);
                value += GetAxisDown(Axis.Cross_Vertical);
            }
            if (Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);

            return value;
        }

        public int GetAxisUp(Axis axis)
        {
            return Axes[axis].GetAxisUp();
        }

        public int GetAnyAxisUp(bool isHorizontal)
        {
            int value = 0;
            if (isHorizontal)
            {
                value += GetAxisUp(Axis.R_Horizontal);
                value += GetAxisUp(Axis.L_Horizontal);
                value += GetAxisUp(Axis.Cross_Horizontal);
            }
            else
            {
                value += GetAxisUp(Axis.R_Vertical);
                value += GetAxisUp(Axis.L_Vertical);
                value += GetAxisUp(Axis.Cross_Vertical);
            }
            if (Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);

            return value;
        }

        public float GetAxis(Axis axis)
        {
            return Axes[axis].GetAxis();
        }

        public float GetAnyAxis(bool isHorizontal)
        {
            float value = 0;
            if (isHorizontal)
            {
                value += GetAxis(Axis.R_Horizontal);
                value += GetAxis(Axis.L_Horizontal);
                value += GetAxis(Axis.Cross_Horizontal);
            }
            else
            {
                value += GetAxis(Axis.R_Vertical);
                value += GetAxis(Axis.L_Vertical);
                value += GetAxis(Axis.Cross_Vertical);
            }
            if (Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);

            return value;
        }

        public int GetAxisPeriodicly(Axis axis)
        {
            return Axes[axis].GetAxisPeriodicly();
        }

        public int GetAxisRow(Axis axis)
        {
            return Axes[axis].GetAxisRow();
        }

        public int GetAnyAxisRow(bool isHorizontal)
        {
            int value = 0;
            if (isHorizontal)
            {
                value += GetAxisRow(Axis.R_Horizontal);
                value += GetAxisRow(Axis.L_Horizontal);
                value += GetAxisRow(Axis.Cross_Horizontal);
            }
            else
            {
                value += GetAxisRow(Axis.R_Vertical);
                value += GetAxisRow(Axis.L_Vertical);
                value += GetAxisRow(Axis.Cross_Vertical);
            }
            if (Mathf.Abs(value) > 1) value = value / Mathf.Abs(value);

            return value;
        }

        public float GetAxisStayTime(Axis axis)
        {
            if (axis == Axis.None) return 0;

            return Axes[axis].StayTime;
        }
        
        #endregion

        /// <summary>
        /// コントローラー上の、ある1つのボタンだ！！
        /// Axisの名前（SetButtonName）か、KeyCode（SetKeyCode）のいずれかを設定してくれ！！
        /// </summary>
        public class ButtonState
        {
            public Button Button { get; private set; }
            private string _name;
            private KeyCode _keycode;
            private bool _previous = false;
            private bool _now = false;
            public bool Enabled { get; private set; }
            public float StayTime { get; private set; }

            public ButtonState(Button button)
            {
                Button = button;
                _name = "";
                _keycode = KeyCode.None;

                Enabled = true;
            }

            public ButtonState(Button button, string name)
            {
                Button = button;
                _name = name;
                _keycode = KeyCode.None;
            }

            public ButtonState(Button button, KeyCode key)
            {
                Button = button;
                _name = "";
                _keycode = key;
            }

            public void SetButtonName(string name)
            {
                _name = name;
                Enabled = true;
            }

            public void SetKeyCode(KeyCode keycode)
            {
                _keycode = keycode;
                Enabled = true;
            }

            public void Update()
            {
                if (Enabled)
                {
                    try
                    {
                        _previous = _now;
                        if (!string.IsNullOrEmpty(_name))
                        {
                            _now = Input.GetButton(_name);
                        }
                        else
                        {
                            _now = Input.GetKey(_keycode);
                        }

                        if (_previous && _now)
                        {
                            StayTime += Time.deltaTime;
                        }
                        else if(!_previous && !_now)
                        {
                            StayTime = 0;
                        }
                    }
                    catch
                    {
                        Enabled = false;
                        if(!string.IsNullOrEmpty(_name))
                        {
                            Debug.Log("The name of Button, \"" + _name + "\" is not available");
                        }
                        else
                        {
                            Debug.Log("The KeyCode of Button, \"" + _keycode + "\" is not available");
                        }
                    }
                }
            }

            public bool GetButtonDown()
            {
                return (!_previous && _now);
            }

            public bool GetButtonStay()
            {
                return _now;
            }

            public bool GetButtonStayPeriodicly()
            {
                var offset = 0.5;
                var interval = 0.1;
                if (!_previous && _now)
                    return true;
                else if (_now)
                {
                    if (StayTime < offset)
                        return false;
                    else if ((int)((StayTime - offset - Time.deltaTime) / interval) != (int)((StayTime - offset) / interval))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }

            public bool GetButtonUp()
            {
                return (_previous && !_now);
            }
        }

        /// <summary>
        /// コントローラー上の、ある1つの軸だ！！
        /// Axisの名前（SetAxisName）か、KeyCode（SetKeyCodes）のいずれかを設定してくれ！！
        /// </summary>
        public class AxisState
        {
            public Axis Axis { get; private set; }
            private KeyCode _positive_keycode;
            private KeyCode _negative_keycode;
            private string _name;
            private float _previous;
            private float _now;
            public double Threshold;
            public double Sensitivity = 2;
            public double Gravity = 10;
            public double Dead = 0.1;
            public bool Enabled { get; private set; }
            public float StayTime { get; private set; }

            public AxisState(Axis axis)
            {
                Axis = axis;
                _name = "";
                _positive_keycode = KeyCode.None;
                _negative_keycode = KeyCode.None;

                Enabled = true;
            }

            public AxisState(Axis axis, string name)
            {
                Axis = axis;
                _name = name;
                _positive_keycode = KeyCode.None;
                _negative_keycode = KeyCode.None;

                Enabled = true;
            }

            public AxisState(Axis axis, KeyCode positive, KeyCode negative)
            {
                Axis = axis;
                _name = "";
                _positive_keycode = positive;
                _negative_keycode = negative;

                Enabled = true;
            }

            public void SetAxisName(string name)
            {
                _name = name;
                Enabled = true;
            }

            public void SetKeyCodes(KeyCode positive, KeyCode negative)
            {
                _positive_keycode = positive;
                _negative_keycode = negative;
                Enabled = true;
            }


            public void Update()
            {
                if (Enabled)
                {
                    try
                    {
                        int previous = GetAxisRow();
                        _previous = _now;
                        if (!string.IsNullOrEmpty(_name))
                        {
                            _now = Input.GetAxis(_name);
                        }
                        else
                        {
                            int key = (Input.GetKey(_positive_keycode) ? 1 : 0) + (Input.GetKey(_negative_keycode) ? -1 : 0);

                            if(key == 0 || previous * key < 0)
                            {
                                if (Gravity > 0)
                                {
                                    _now += -(float)(previous * Gravity * Time.deltaTime);
                                    if (_now * previous < 0) _now = 0;
                                    if (Mathf.Abs(_now) <= Dead) _now = 0;
                                }
                                else
                                    _now = 0;

                                if (Mathf.Abs(_now) <= Dead) _now = 0;
                            }
                            else
                            {
                                if (Sensitivity > 0)
                                {
                                    _now += (float)(key * Sensitivity * Time.deltaTime);
                                    if (Mathf.Abs(_now) > 1) _now = key;
                                }
                                else
                                    _now = key;
                            }
                            
                        }
                        int now = GetAxisRow();

                        if(previous != 0 && previous == now)
                        {
                            StayTime += Time.deltaTime;
                        }
                        else if(previous == 0 && now == 0)
                        {
                            StayTime = 0;
                        }

                    }
                    catch
                    {
                        Enabled = false;
                        if(!string.IsNullOrEmpty(_name))
                        {
                            Debug.Log("The name of Axis, \"" + _name + "\" is not available");
                        }
                        else
                        {
                            Debug.Log("The KeyCode of Axis, \"" + _positive_keycode + ", " + _negative_keycode + "\" is not available");
                        }
                    }
                }
            }

            public int GetAxisDown()
            {
                int p = _previous > Threshold ? 1 : (_previous < -Threshold ? -1 : 0);
                int n = _now > Threshold ? 1 : (_now < -Threshold ? -1 : 0);

                return (p == n) ? 0 : n;
            }

            public int GetAxisUp()
            {
                int p = _previous > Threshold ? 1 : (_previous < -Threshold ? -1 : 0);
                int n = _now > Threshold ? 1 : (_now < -Threshold ? -1 : 0);

                return (n == 0) ? p : 0;
            }

            public float GetAxis()
            {
                return _now;
            }

            public int GetAxisPeriodicly()
            {
                var offset = 0.2;
                var interval = 0.1;
                int p = _previous > Threshold ? 1 : (_previous < -Threshold ? -1 : 0);
                int n = _now > Threshold ? 1 : (_now < -Threshold ? -1 : 0);
                if (p != n)
                    return n;
                else
                {
                    if (StayTime < offset)
                        return 0;
                    else if ((int)((StayTime - offset - Time.deltaTime) / interval) != (int)((StayTime - offset) / interval))
                        return n;
                    else
                        return 0;
                }
            }

            public int GetAxisRow()
            {
                int n = _now > Threshold ? 1 : (_now < -Threshold ? -1 : 0);

                return n;
            }
        }
    }

    #region enum
    /// <summary>
    /// これだけあれば十分だろう！！
    /// </summary>
    public enum Button
    {
        A, B, X, Y, L1, R1, L2, R2, Select, Start, LeftStick, RightStick, None, 
    }  

    /// <summary>
    /// これだけあれば十分だろう！！
    /// </summary>
    public enum Axis
    {
        R_Horizontal, R_Vertical, L_Horizontal, L_Vertical, Cross_Horizontal, Cross_Vertical, Side, None, 
    }

    public enum GamePad
    {
        One, Two, Three, Four, All, None, 
    }
    #endregion
}