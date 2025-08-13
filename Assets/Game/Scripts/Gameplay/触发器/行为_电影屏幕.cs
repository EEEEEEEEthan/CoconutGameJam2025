namespace Game.Gameplay.触发器
{
	public class 行为_电影屏幕 : GameBehaviour
	{
		void OnEnable() => GameRoot.GameCanvas.Filmic(true);
		void OnDisable() => GameRoot.GameCanvas.Filmic(false);
	}
}
