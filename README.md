Emerald Dream
=============

项目目录结构
------------

1. Art

   存放所有美术资源。

2. EasyTouchBundle

   EasyTouch插件包：手势按键输入及摇杆等。

3. FlowInLib

   游戏核心框架：GameRoot（游戏启动Scene，用于存放整个游戏过程中不需要被销毁的GO，目前用于存放UIRoot）、配置管理、日志输出、版本维护、打AB包等。

4. NGUI

   GUI库。

5. Prefabs

   存放所有游戏使用到的prefab。

6. Scenes

   存放所有Scene。
   
   目前有两个scene，startup和game，游戏逻辑都写在game里，startup是用于加载一些切换scene时不需要被卸载的GO，目前只有UIRoot。
   
   game中的GO介绍：
   
   * Player为角色GO，和角色相关的脚本如PlayerController和PlayerState等放在其上；
   
   * World为游戏管理类，目前有GameMode和GameState两个类；
   
   * Main Camera为主摄像机；
   
   * Directional Light为场景平行光;
   
   * 基本逻辑框架可以参考UE4。

7. Scripts

   存放所有逻辑脚本。

8. StreamingAssets

   此文件夹为打AB包后自动生成的。


版本信息
--------

使用Unity 5.6.0