InputController
---
UnityのInputがクソなので俺々スクリプトを書いてみました。

### 使い方
中にあるTestProjectをUnityで起動して、Testシーンで方向キーの横を押してみてください。  
コンソールにメッセージが表示されます。  
そのあと、TestScriptの中身を見てください。  
その処理が数行で完結していることが確認できます。

TestScriptのStart()の中の、SetGamePad();のコメントアウトを消してみてください。  
ゲームパッドで同じことができるようになります。

これをほかのUnityプロジェクトで使いたい場合は、InputController.csをどこかにおいてください。  
加えて、SetFGamePads()を使いたい場合は、このプロジェクトのProjectSettingsにあるInputManager.assetをそのプロジェクトの同じフォルダにコピーしてください。  
ただし、InputManager.assetをコピーした場合は、それまでのInputの設定は消えてしまうので気を付けてください。

詳しくはブログにでも書くとしましょう。

### ライセンス
Apache License Version 2.0
