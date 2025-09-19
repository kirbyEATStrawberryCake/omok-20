namespace _02_Scripts.AI
{
    public class GomokuWeights
    {
        // 1목
        public const int ONE_NO_ENEMY_HAS_FRIEND = 13;
        public const int ONE_NO_ENEMY_NO_FRIEND = 10;
        public const int ONE_ENEMY_HAS_FRIEND = 7;
        public const int ONE_ENEMY_NO_FRIEND = 5;
        public const int ONE_UNIQUE_PATH = 5;

        // 2목
        public const int TWO_NO_ENEMY_HAS_FRIEND = 55;
        public const int TWO_NO_ENEMY_NO_FRIEND = 50;
        public const int TWO_ENEMY_HAS_FRIEND = 35;
        public const int TWO_ENEMY_NO_FRIEND = 30;
        public const int TWO_UNIQUE_PATH = 30;

        // 3목
        public const int THREE_NO_ENEMY_HAS_FRIEND = 600;
        public const int THREE_NO_ENEMY_NO_FRIEND = 500;
        public const int THREE_ENEMY_HAS_FRIEND = 57;
        public const int THREE_ENEMY_NO_FRIEND = 57;
        public const int THREE_UNIQUE_PATH = 57;

        // 4목
        public const int FOUR_OPEN = 5000;
		public const int FOUR_SIDE_CLOSE = 1500;

		// 5목(승리)
		public const int FIVE = 100000;
    }
}