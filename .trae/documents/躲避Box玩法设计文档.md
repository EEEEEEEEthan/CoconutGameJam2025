# 躲避Box玩法设计文档

## 1. 玩法概述

一个3D躲避类小游戏，玩家需要连续躲避指定数量的Box才能过关。游戏采用简洁的机制设计：两个发射源交替向玩家发射Box，玩家通过现有的腿部控制系统进行移动躲避，成功躲避足够数量的Box即可获胜。

## 2. 核心机制

### 2.1 玩家系统

* **位置约束**：玩家固定在z=0, y=0平面上，通过HandIKInput系统自动维持位置

* **移动控制**：复用现有输入系统

  * Q/A/Z键：控制左腿（前抬/上抬/后抬）

  * W/S/X键：控制右腿（前抬/上抬/后抬）

* **碰撞检测**：使用Player组件的PlayerPositionTrigger碰撞体

### 2.2 Box发射系统

* **发射机制**：两个发射源交替发射，确保持续的挑战节奏

* **轨迹设计**：Box必须穿过z=0平面上的指定矩形目标区域

* **飞行路径**：直线轨迹，从发射源→目标区域→继续延伸

* **目标计算**：根据玩家当前位置和目标区域动态计算飞行方向

### 2.3 Box生命周期

1. **发射阶段**：从发射源按计算轨迹飞向目标区域
2. **威胁阶段**：在z=0平面附近时可能与玩家发生碰撞
3. **穿越阶段**：飞过z=0平面后继续飞行，不再构成威胁
4. **溶解阶段**：开始播放溶解视觉效果（2秒持续时间）
5. **销毁阶段**：溶解完成后删除GameObject

### 2.4 胜利条件

* **成功条件**：连续躲避指定数量的Box（可配置参数）

* **失败条件**：被任意Box击中，计数器重置为0

* **计数机制**：只有成功躲避的Box才计入计数，失败后需重新开始

## 3. 技术实现要点

### 3.1 核心组件架构

采用简洁的三组件架构设计：

* **BoxDodgeGameManager**：游戏主控制器

  * 管理游戏状态和流程

  * 控制Box发射逻辑

  * 处理碰撞事件和计数

  * 输出游戏结果

* **DodgeBox**：Box实体组件

  * 包含飞行配置参数

  * 实现移动和轨迹逻辑

  * 处理溶解效果

  * 管理生命周期

* **Player**：现有玩家系统

  * 提供PlayerPositionTrigger碰撞体

  * 处理玩家输入和移动

  * 维持z=0平面位置约束

### 3.2 关键技术点

* **轨迹计算算法**

  * 输入：发射源位置、目标区域范围、当前时间

  * 计算：确保Box轨迹必经目标区域的直线路径

  * 输出：Box的飞行方向向量和速度

* **碰撞检测机制**

  * 使用Unity物理系统的3D碰撞检测

  * 监听OnTriggerEnter事件

  * 区分玩家碰撞体和其他物体

* **溶解效果实现**

  * 使用Shader Graph或自定义Shader

  * 基于时间的透明度渐变

  * 可选粒子效果增强视觉表现

### 3.3 组件代码结构

````csharp
// Box实体组件 - 自包含所有配置和逻辑
public class DodgeBox : MonoBehaviour
{
    [Header("飞行配置")]
    public float speed = 10f;              // 飞行速度
    public Vector3 targetPosition;         // 目标区域中心点
    
    [Header("溶解效果")]
    public float dissolveTime = 2f;        // 溶解持续时间
    public Material dissolveMaterial;      // 溶解材质
    
    [Header("碰撞检测")]
    public LayerMask playerLayer;          // 玩家层级掩码
    
    private bool hasPassedTarget = false;  // 是否已穿过目标区域
    private bool isDissolving = false;     // 是否正在溶解
    
    // 核心方法：移动逻辑、碰撞处理、溶解效果
}

// 游戏主控制器 - 管理整体流程
public class BoxDodgeGameManager : MonoBehaviour
{
    [Header("游戏配置")]
    public int requiredDodgeCount = 10;    // 胜利所需躲避数量
    public float launchInterval = 1f;      // Box发射间隔
    
    [Header("发射系统")]
    public Transform[] launcherPositions;  // 发射源位置数组
    public Rect targetArea;                // z=0平面目标区域
    public GameObject boxPrefab;           // Box预制体
    
    [Header("玩家引用")]
    public Player player;                  // 现有玩家组件
    
    private int currentDodgeCount = 0;     // 当前躲避计数
    private int currentLauncherIndex = 0;  // 当前发射器索引
    
    // 核心方法：游戏初始化、Box发射、碰撞处理、结果输出
}```

## 4. 游戏流程设计

### 4.1 初始化阶段
1. **场景准备**：设置发射源位置、目标区域范围
2. **玩家就位**：确保Player组件正常工作，位置约束生效
3. **参数配置**：设定胜利条件、发射间隔等游戏参数

### 4.2 游戏循环
1. **Box发射**：按间隔从交替发射源创建Box
2. **轨迹计算**：为每个Box计算穿过目标区域的飞行路径
3. **玩家操作**：玩家通过腿部控制进行躲避移动
4. **碰撞检测**：实时监测Box与玩家的碰撞状态
5. **状态更新**：根据碰撞结果更新计数或重置游戏

### 4.3 结束条件
- **胜利**：成功躲避达到目标数量，控制台输出成功信息
- **失败**：发生碰撞，重置计数器继续游戏

## 5. 视觉与反馈设计

### 5.1 视觉表现
- **Box外观**：简洁的立方体模型，易于识别
- **溶解效果**：穿过z=0平面后的透明度渐变动画
- **轨迹提示**：可选的飞行路径预览（调试用）

### 5.2 反馈机制
- **即时反馈**：碰撞发生时的视觉或音效提示
- **进度反馈**：当前躲避计数的简单显示
- **结果输出**：游戏完成时控制台打印结果
  ```csharp
  // 扩展点：可在此处添加更丰富的反馈
  Debug.Log($"游戏结束 - 成功躲避: {currentDodgeCount}/{requiredDodgeCount}");
````

### 5.3 扩展预留

控制台输出位置预留接口，便于后续集成：

* UI界面显示

* 音效系统

* 成就系统

* 数据统计

